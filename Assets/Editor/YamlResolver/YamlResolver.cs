using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

public class YamlResolver
{
    /// <summary>
    /// key:fullPath
    /// </summary>
    static public Dictionary<string, YamlResolver> sResolverDic = new Dictionary<string, YamlResolver>();
    /// <summary>
    /// 是否初始化过Script的Guid
    /// </summary>
    static bool mInitScriptGuid;
    /// <summary>
    /// key为guid，value为解析类，该Dictionary存储的是Monobehaviour的子类解析器
    /// </summary>
    static Dictionary<string, Type> sScriptTypeDic = new Dictionary<string, Type>();

    Dictionary<YamlType, List<Yaml_BaseObject>> mCurrentDic = new Dictionary<YamlType, List<Yaml_BaseObject>>();
    public Yaml_Prefab prefabRoot { private set; get; }

    static void InitScriptGuid()
    {
        //key为解析类，value为script的meta文件绝对路径
        var tScriptMetaDic = new Dictionary<Type, string>()
        {
            { typeof(Yaml_UISprite), Path.Combine(Application.dataPath ,"NGUI/Scripts/UI/UISprite.cs.meta") },
        };

        foreach (var tMeta in tScriptMetaDic)
        {
            if (!File.Exists(tMeta.Value))
            {
                Debug.LogErrorFormat("{0} Meta文件路径不存在：{1}", tMeta.Key.Name, tMeta.Value);
                continue;
            }

            if (!tMeta.Key.IsSubclassOf(typeof(Yaml_MonoBehaviour)))
            {
                Debug.LogErrorFormat("该解析类不是Yaml_MonoBehaviour的子类：{0}", tMeta.Key.Name);
                continue;
            }

            var tMetaContent = File.ReadAllText(tMeta.Value);
            if (string.IsNullOrEmpty(tMetaContent)) continue;
            var tMatch = Regex.Match(tMetaContent, @"guid: (?<guid>\w+)");
            if (!tMatch.Success) continue;
            var tGuid = tMatch.Groups["guid"].Value;
            sScriptTypeDic.Add(tGuid, tMeta.Key);
        }
    }

    /// <summary>
    /// 解析该yaml文本，参数：绝对路径
    /// </summary>
    /// <param name="pFullPath">绝对路径</param>
    static public void ResolveYaml(string pFullPath, bool pForceResolve = false)
    {
        if (!File.Exists(pFullPath))
        {
            Debug.LogErrorFormat("不存在该路径：" + pFullPath);
            return;
        }

        if (!mInitScriptGuid)
        {
            mInitScriptGuid = true;
            InitScriptGuid();
        }

        if (!pForceResolve && sResolverDic.ContainsKey(pFullPath))
        {
            Debug.LogErrorFormat("已存在解析数据：" + pFullPath);
            return;
        }

        try
        {
            var tResolver = new YamlResolver();
            if (!sResolverDic.ContainsKey(pFullPath))
            {
                sResolverDic.Add(pFullPath, null);
            }
            sResolverDic[pFullPath] = tResolver;
            tResolver.Resolve(pFullPath);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public void Resolve(string pFullPath)
    {
        mCurrentDic = new Dictionary<YamlType, List<Yaml_BaseObject>>();
        Yaml_BaseObject tLastObject = null;
        using (StreamReader tReader = new StreamReader(pFullPath))
        {
            while (!tReader.EndOfStream)
            {
                var tContent = tReader.ReadLine();
                if (string.IsNullOrEmpty(tContent)) continue;
                var tMatch = Regex.Match(tContent, @"--- !u!(?<yamlType>\d+) &(?<fileID>\d+)");
                if (!tMatch.Success)
                {
                    if (tLastObject == null) continue;
                    tLastObject.Resolver(tContent);

                    if (tLastObject.GetType() == typeof(Yaml_MonoBehaviour))
                    {
                        var tScript = tLastObject as Yaml_MonoBehaviour;
                        if (string.IsNullOrEmpty(tScript.scriptGuid)) continue;
                        Yaml_MonoBehaviour tTempScript = null;
                        if (sScriptTypeDic.ContainsKey(tScript.scriptGuid))
                        {
                            var tMonoScript = sScriptTypeDic[tScript.scriptGuid];
                            tTempScript = Activator.CreateInstance(tMonoScript, tScript) as Yaml_MonoBehaviour;
                        }

                        if (tTempScript != null)
                        {
                            Remove(YamlType.Monobehaviour, tLastObject);
                            tLastObject = tTempScript;
                            Add(YamlType.Monobehaviour, tLastObject);
                        }
                    }
                    continue;
                }
                var tVal = tMatch.Groups["yamlType"].Value;
                var tFileID = tMatch.Groups["fileID"].Value;
                var tYamlTypeVal = 0;
                if (!int.TryParse(tVal, out tYamlTypeVal)) continue;
                var tYamlType = (YamlType)tYamlTypeVal;
                if (tYamlType == YamlType.Unknown) continue;
                switch (tYamlType)
                {
                    case YamlType.GameObject:
                        tLastObject = new Yaml_GameObject(new Yaml_BaseArg(tFileID, pFullPath, tYamlType));
                        break;
                    case YamlType.Transform:
                        tLastObject = new Yaml_Trasnform(new Yaml_BaseArg(tFileID, pFullPath, tYamlType));
                        break;
                    case YamlType.Monobehaviour:
                        tLastObject = new Yaml_MonoBehaviour(new Yaml_BaseArg(tFileID, pFullPath, tYamlType));
                        break;
                    case YamlType.Prefab:
                        prefabRoot = new Yaml_Prefab(new Yaml_BaseArg(tFileID, pFullPath, tYamlType));
                        tLastObject = prefabRoot;
                        break;
                }
                Add(tYamlType, tLastObject);
            }
        }
        if (prefabRoot == null)
        {
            throw new NullReferenceException("找不到根节点");
        }
        prefabRoot.ToTreeStruct();
    }

    public void Add(YamlType pYamlType, Yaml_BaseObject pObject)
    {
        if (!mCurrentDic.ContainsKey(pYamlType)) mCurrentDic.Add(pYamlType, new List<Yaml_BaseObject>());
        mCurrentDic[pYamlType].Add(pObject);
    }

    public void Remove(YamlType pYamlType, Yaml_BaseObject pObject)
    {
        if (!mCurrentDic.ContainsKey(pYamlType) || mCurrentDic[pYamlType] == null) return;
        mCurrentDic[pYamlType].Remove(pObject);
    }

    public List<T> GetObjects<T>() where T : Yaml_BaseObject
    {
        var pYamlType = YamlHelper.GetYamlType<T>();
        List<Yaml_BaseObject> tObjects = null;
        if (!mCurrentDic.TryGetValue(pYamlType, out tObjects)) return null;
        return tObjects.ConvertAll(x => (T)x);
    }

    public T GetObject<T>(Predicate<T> pPredicate) where T : Yaml_BaseObject
    {
        T tObject = default(T);
        if (pPredicate == null) return tObject;
        var tObjects = GetObjects<T>();
        if (tObjects == null) return tObject;
        foreach (var item in tObjects)
        {
            if (pPredicate(item)) return (T)item;
        }
        return tObject;
    }

    public T GetObject<T>(string pFileID) where T : Yaml_BaseObject
    {
        return GetObject<T>(x =>
        {
            return x.fileID == pFileID;
        });
    }

    /// <summary>
    /// 根据guid获取解析类类型
    /// </summary>
    /// <param name="pGuid"></param>
    /// <returns></returns>
    static public Type GetTypeByGuid(string pGuid)
    {
        if (sScriptTypeDic == null) return null;
        Type tType = null;
        sScriptTypeDic.TryGetValue(pGuid, out tType);
        return tType;
    }
}