using System.Text.RegularExpressions;

public class Yaml_MonoBehaviour : Yaml_BaseObject
{
    public const int cID = (int)YamlType.Monobehaviour;

    string mGameObjectFileID;

    public Yaml_ScriptInfo scriptInfo { private set; get; }

    public Yaml_MonoBehaviour(string pID, string pFullPath) : base(pID, pFullPath)
    {
    }

    public override void Resolver(string pContent)
    {
        if (pContent.StartsWith("  m_GameObject:"))
        {
            var tVal = GetFileIDByContent(pContent, "m_GameObject");
            if (tVal != null) mGameObjectFileID = tVal;
            return;
        }

        if (pContent.StartsWith("  m_Script:"))
        {
            var tMatch = Regex.Match(pContent, @"m_Script: {fileID: (?<fileID>\w+), guid: (?<guid>\w+), type: \w+}");
            if (tMatch.Success)
            {
                scriptInfo = new Yaml_ScriptInfo(tMatch.Groups["fileID"].Value, tMatch.Groups["guid"].Value);
            }
            return;
        }
    }

    public override string ToString()
    {
        return scriptInfo.guid;
    }
}