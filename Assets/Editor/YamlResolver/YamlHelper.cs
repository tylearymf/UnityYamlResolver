
static public class YamlHelper
{
    static public T GetObject<T>(this Yaml_BaseObject pObject, string pFileID) where T : Yaml_BaseObject
    {
        if (pObject == null) return default(T);
        var tResolver = GetResolver(pObject.fullPath);
        return tResolver == null ? default(T) : tResolver.GetObject<T>(pFileID);
    }

    /// <summary>
    /// »ñÈ¡½âÎöÆ÷
    /// </summary>
    /// <param name="pFileID"></param>
    /// <returns></returns>
    static public YamlResolver GetResolver(string pFullPath)
    {
        if (YamlResolver.sResolverDic == null) return null;
        YamlResolver tResolver = null;
        if (!YamlResolver.sResolverDic.TryGetValue(pFullPath, out tResolver)) return null;
        return tResolver;
    }
}