
using System;
using UnityEngine;

static public class YamlHelper
{
    static public T GetObject<T>(this Yaml_BaseObject pObject, string pFileID) where T : Yaml_BaseObject
    {
        if (pObject == null) return default(T);
        var tResolver = GetResolver(pObject.fullPath);
        return tResolver == null ? default(T) : tResolver.GetObject<T>(pFileID);
    }

    /// <summary>
    /// 获取解析器
    /// </summary>
    /// <param name="pFileID"></param>
    /// <returns></returns>
    static public YamlResolver GetResolver(string pFullPath, bool pIfNullResolve = false)
    {
        if (YamlResolver.sResolverDic == null) return null;
        YamlResolver tResolver = null;
        YamlResolver.sResolverDic.TryGetValue(pFullPath, out tResolver);
        if (tResolver == null && pIfNullResolve)
        {
            YamlResolver.ResolveYaml(pFullPath, true);
            return GetResolver(pFullPath, false);
        }
        return tResolver;
    }

    static public UISprite GetSpriteByIndex(GameObject pInstance, string pFullPath, string pAtlasGuid, string pSpriteName, int pIndex)
    {
        var tResolver = GetResolver(pFullPath, true);
        var tAllSprites = tResolver.GetObjects<Yaml_UISprite>();
        var tSprites = tAllSprites.FindAll(x => x.atlasGuid == pAtlasGuid && x.spriteName == pSpriteName);
        if (pIndex >= 0 && pIndex < tSprites.Count)
        {
            return tSprites[pIndex].FindThis(pInstance);
        }
        return null;
    }

    static public YamlType GetYamlType<T>()
    {
        var tType = typeof(T);
        if (tType.IsAssignableFrom(typeof(Yaml_Prefab)) || tType.IsSubclassOf(typeof(Yaml_Prefab)))
        {
            return YamlType.Prefab;
        }
        else if (tType.IsAssignableFrom(typeof(Yaml_GameObject)) || tType.IsSubclassOf(typeof(Yaml_GameObject)))
        {
            return YamlType.GameObject;
        }
        else if (tType.IsAssignableFrom(typeof(Yaml_Trasnform)) || tType.IsSubclassOf(typeof(Yaml_Trasnform)))
        {
            return YamlType.Transform;
        }
        else if (tType.IsAssignableFrom(typeof(Yaml_MonoBehaviour)) || tType.IsSubclassOf(typeof(Yaml_MonoBehaviour)))
        {
            return YamlType.Monobehaviour;
        }
        throw new NotImplementedException("未实现:" + tType.Name);
    }
}