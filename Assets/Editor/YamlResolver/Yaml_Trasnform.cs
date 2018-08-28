using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class Yaml_Trasnform : Yaml_BaseObject
{
    string mGameObjectFileID;
    List<string> mChildFileIDs = new List<string>();
    string mParentFileID;
    int mParentCount = -1;

    public Yaml_GameObject gameObject
    {
        get
        {
            return this.GetObject<Yaml_GameObject>(mGameObjectFileID);
        }
    }

    public Yaml_Trasnform parent
    {
        get
        {
            return this.GetObject<Yaml_Trasnform>(mParentFileID);
        }
    }

    public IEnumerable<Yaml_Trasnform> childs
    {
        get
        {
            foreach (var item in mChildFileIDs)
            {
                yield return this.GetObject<Yaml_Trasnform>(item);
            }
        }
    }

    public int siblingIndex
    {
        get
        {
            if (parent == null) return 0;
            return parent.childs.ToList().FindIndex(x => x == this);
        }
    }

    public int childCount
    {
        get
        {
            return mChildFileIDs.Count;
        }
    }

    public int parentCount
    {
        get
        {
            if (mParentCount == -1)
            {
                Yaml_Trasnform tParent = parent;
                var tCount = 0;
                while (tParent != null)
                {
                    tCount++;
                    tParent = tParent.parent;
                }
                mParentCount = tCount;
            }
            return mParentCount;
        }
    }

    public Yaml_Trasnform(Yaml_BaseArg pArg) : base(pArg)
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

        if (pContent.StartsWith("  - {fileID:"))
        {
            var tMatch = Regex.Match(pContent, @"  - {fileID: (?<fileID>\w+)}");
            if (tMatch.Success)
            {
                mChildFileIDs.Add(tMatch.Groups["fileID"].Value);
            }
            return;
        }

        if (pContent.StartsWith("  m_Father:"))
        {
            var tVal = GetFileIDByContent(pContent, "m_Father");
            if (tVal != null) mParentFileID = tVal;
            return;
        }
    }

    public override void ToTreeStruct()
    {
        gameObject.ToTreeStruct();
    }
}
