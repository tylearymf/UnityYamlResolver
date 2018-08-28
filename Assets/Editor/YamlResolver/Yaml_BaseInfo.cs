public class Yaml_BaseInfo
{
    public string fileID { set; get; }
    public string guid { set; get; }

    public Yaml_BaseInfo(string pFileID, string pGuid)
    {
        fileID = pFileID;
        guid = pGuid;
    }
}

public class Yaml_ScriptInfo : Yaml_BaseInfo
{
    public Yaml_ScriptInfo(string pFileID, string pGuid) : base(pFileID, pGuid)
    {
    }
}

public class Ymal_UISpriteInfo : Yaml_BaseInfo
{
    public Ymal_UISpriteInfo(string pFileID, string pGuid) : base(pFileID, pGuid)
    {
    }
}