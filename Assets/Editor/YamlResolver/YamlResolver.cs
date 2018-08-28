using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

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
    static public Dictionary<string, Type> sScriptTypeDic = new Dictionary<string, Type>();

    Dictionary<YamlType, List<Yaml_BaseObject>> mCurrentDic = new Dictionary<YamlType, List<Yaml_BaseObject>>();
    public Yaml_Prefab prefabRoot { private set; get; }

    static public void InitScriptGuid()
    {
        //key为解析类，value为script的meta文件绝对路径
        var tScriptMetaDic = new Dictionary<Type, string>()
            {
                { typeof(Yaml_UISprite),"" },
            };

        foreach (var tMeta in tScriptMetaDic)
        {
            if (!File.Exists(tMeta.Value))
            {
                Console.WriteLine("{0} Meta文件路径不存在：{1}", tMeta.Key.Name, tMeta.Value);
                continue;
            }

            if (!tMeta.Key.IsSubclassOf(typeof(Yaml_MonoBehaviour)))
            {
                Console.WriteLine("该解析类不是Yaml_MonoBehaviour的子类：{0}", tMeta.Key.Name);
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

    static public void ResolveYaml(string pFullPath)
    {
        if (!File.Exists(pFullPath))
        {
            Console.WriteLine("不存在该路径：" + pFullPath);
            return;
        }

        if (!mInitScriptGuid)
        {
            mInitScriptGuid = true;
            InitScriptGuid();
        }

        if (sResolverDic.ContainsKey(pFullPath))
        {
            Console.WriteLine("已存在解析数据：" + pFullPath);
            return;
        }

        try
        {
            var tResolver = new YamlResolver();
            sResolverDic.Add(pFullPath, tResolver);
            tResolver.Resolve(pFullPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine(string.Format("{0} yaml文件解析失败", pFullPath));
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

                    var tScript = tLastObject as Yaml_MonoBehaviour;
                    if (tScript != null && tScript.scriptInfo != null && !string.IsNullOrEmpty(tScript.scriptInfo.guid))
                    {
                        Yaml_MonoBehaviour tTempScript = null;
                        if (sScriptTypeDic.ContainsKey(tScript.scriptInfo.guid))
                        {
                            var tMonoScript = sScriptTypeDic[tScript.scriptInfo.guid];
                            tTempScript = Activator.CreateInstance(tMonoScript, tScript.fileID, pFullPath) as Yaml_MonoBehaviour;
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
                var tYamlType = 0;
                if (!int.TryParse(tVal, out tYamlType)) continue;
                switch (tYamlType)
                {
                    case Yaml_Prefab.cID:
                        prefabRoot = new Yaml_Prefab(tFileID, pFullPath);
                        tLastObject = prefabRoot;
                        break;
                    case Yaml_GameObject.cID:
                        tLastObject = new Yaml_GameObject(tFileID, pFullPath);
                        break;
                    case Yaml_Trasnform.cID:
                        tLastObject = new Yaml_Trasnform(tFileID, pFullPath);
                        break;
                    case Yaml_MonoBehaviour.cID:
                        tLastObject = new Yaml_MonoBehaviour(tFileID, pFullPath);
                        break;
                }
                Add((YamlType)tYamlType, tLastObject);
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

    public List<T> GetObjects<T>(YamlType pYamlType) where T : Yaml_BaseObject
    {
        List<Yaml_BaseObject> tObjects = null;
        if (!mCurrentDic.TryGetValue(pYamlType, out tObjects)) return null;
        return tObjects.ConvertAll(x => (T)x);
    }

    public T GetObject<T>(YamlType pYamlType, Predicate<T> pPredicate) where T : Yaml_BaseObject
    {
        T tObject = default(T);
        if (pPredicate == null) return tObject;
        var tObjects = GetObjects<T>(pYamlType);
        if (tObjects == null) return tObject;
        foreach (var item in tObjects)
        {
            if (pPredicate(item)) return (T)item;
        }
        return tObject;
    }

    public T GetObject<T>(string pFileID) where T : Yaml_BaseObject
    {
        var tIDField = typeof(T).GetField("cID", BindingFlags.Static | BindingFlags.Public);
        var tID = (int)tIDField.GetValue(null);
        return GetObject<T>((YamlType)tID, x =>
        {
            return x.fileID == pFileID;
        });
    }
}