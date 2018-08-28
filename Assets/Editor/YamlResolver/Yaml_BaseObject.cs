using System.Text.RegularExpressions;

public abstract class Yaml_BaseObject
{
    public string fileID { get { return arg.fileID; } }
    public string fullPath { get { return arg.fullPath; } }
    public YamlType yamlType { get { return arg.yamlType; } }

    public Yaml_BaseArg arg { private set; get; }

    public Yaml_BaseObject(Yaml_BaseArg pArg)
    {
        arg = pArg;
    }

    public string GetNameByContent(string pContent)
    {
        return GetNameByContent(pContent, "m_Name");
    }

    public string GetNameByContent(string pContent, string pFilter)
    {
        var tMatch = Regex.Match(pContent, pFilter + @": (?<name>.*)");
        if (tMatch.Success) return tMatch.Groups["name"].Value;
        return null;
    }

    public string GetFileIDByContent(string pContent, string pFilter)
    {
        var tMatch = Regex.Match(pContent, pFilter + @": {fileID: (?<filter>\w+)}");
        if (tMatch.Success) return tMatch.Groups["filter"].Value;
        return null;
    }

    public abstract void Resolver(string pContent);
    public virtual void ToTreeStruct() { }
}