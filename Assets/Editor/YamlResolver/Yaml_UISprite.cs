using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class Yaml_UISprite : Yaml_MonoBehaviour
{
    public Ymal_UISpriteInfo spriteInfo { private set; get; }
    public string atlasGuid
    {
        get
        {
            return spriteInfo == null ? string.Empty : spriteInfo.guid;
        }
    }
    public string spriteName
    {
        get
        {
            return spriteInfo == null ? string.Empty : spriteInfo.spriteName;
        }
    }

    public Yaml_UISprite(Yaml_BaseArg pArg) : base(pArg)
    {
    }
    public Yaml_UISprite(Yaml_MonoBehaviour pMono) : base(pMono.arg)
    {
        scriptInfo = pMono.scriptInfo;
        gameObjectFileID = pMono.gameObjectFileID;
    }

    public override void Resolver(string pContent)
    {
        if (spriteInfo == null) spriteInfo = new Ymal_UISpriteInfo();

        if (pContent.StartsWith("  mAtlas:"))
        {
            var tMatch = Regex.Match(pContent, @"mAtlas: {fileID: (?<fileID>\w+), guid: (?<guid>\w+), type: \w+}");
            if (tMatch.Success)
            {
                spriteInfo.fileID = tMatch.Groups["fileID"].Value;
                spriteInfo.guid = tMatch.Groups["guid"].Value;
            }
            return;
        }

        if (pContent.StartsWith("  mSpriteName:"))
        {
            var tVal = GetNameByContent(pContent, "mSpriteName");
            if (tVal != null) spriteInfo.spriteName = tVal;
        }
    }

    internal UISprite FindThis(GameObject pInstance)
    {
        var tChildIndexs = new List<int>();
        tChildIndexs.Add(transform.siblingIndex);
        var tParent = transform.parent;
        while (tParent != null)
        {
            tChildIndexs.Add(tParent.siblingIndex);
            tParent = tParent.parent;
        }
        tChildIndexs.Reverse();

        var tTrans = pInstance.transform;
        foreach (var tIndex in tChildIndexs)
        {
            if (tIndex >= 0 && tIndex < tTrans.childCount)
            {
                tTrans = tTrans.GetChild(tIndex);
            }
            else
            {
                throw new NullReferenceException(string.Format("{0}µÄchildCount£º{1},tIndex:{2}", tTrans.name, tTrans.childCount, tIndex));
            }
        }
        return tTrans == null ? null : tTrans.GetComponent<UISprite>();
    }

    public override string ToString()
    {
        return string.Format("atlasGuid:{0},spriteName:{1}", atlasGuid, spriteName);
    }
}