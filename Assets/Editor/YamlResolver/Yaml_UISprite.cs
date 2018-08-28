using System.Text.RegularExpressions;

public class Yaml_UISprite : Yaml_MonoBehaviour
{
    string mSpriteName;

    public Ymal_UISpriteInfo spriteInfo { private set; get; }
    public string atlasGuid
    {
        get
        {
            return spriteInfo == null ? string.Empty : spriteInfo.guid;
        }
    }

    public Yaml_UISprite(string pID, string pFullPath) : base(pID, pFullPath)
    {
    }

    public override void Resolver(string pContent)
    {
        base.Resolver(pContent);

        if (pContent.StartsWith("  mAtlas:"))
        {
            var tMatch = Regex.Match(pContent, @"mAtlas: {fileID: (?<fileID>\w+), guid: (?<guid>\w+), type: \w+}");
            if (tMatch.Success)
            {
                spriteInfo = new Ymal_UISpriteInfo(tMatch.Groups["fileID"].Value, tMatch.Groups["guid"].Value);
            }
            return;
        }

        if (pContent.StartsWith("  mSpriteName:"))
        {
            var tVal = GetNameByContent(pContent, "mSpriteName");
            if (tVal != null) mSpriteName = tVal;
        }
    }

    public override string ToString()
    {
        return atlasGuid;
    }
}