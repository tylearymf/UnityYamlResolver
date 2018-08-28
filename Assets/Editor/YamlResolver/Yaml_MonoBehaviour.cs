using System.Text.RegularExpressions;

public class Yaml_MonoBehaviour : Yaml_BaseObject
{
    public string gameObjectFileID { protected set; get; }
    public Yaml_ScriptInfo scriptInfo { protected set; get; }
    public string scriptGuid
    {
        get
        {
            return scriptInfo == null ? string.Empty : scriptInfo.guid;
        }
    }
    public Yaml_GameObject gameObject
    {
        get
        {
            return this.GetObject<Yaml_GameObject>(gameObjectFileID);
        }
    }

    public Yaml_Trasnform transform
    {
        get
        {
            return gameObject == null ? null : gameObject.transform;
        }
    }

    public Yaml_MonoBehaviour(Yaml_BaseArg pArg) : base(pArg)
    {
    }

    public override void Resolver(string pContent)
    {
        if (pContent.StartsWith("  m_GameObject:"))
        {
            var tVal = GetFileIDByContent(pContent, "m_GameObject");
            if (tVal != null) gameObjectFileID = tVal;
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