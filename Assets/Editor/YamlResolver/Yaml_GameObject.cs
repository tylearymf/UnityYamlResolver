using System.Collections.Generic;
using UnityEngine;

public class Yaml_GameObject : Yaml_BaseObject
{
    string mName;
    List<string> mComponents = new List<string>();

    public Yaml_Trasnform transform
    {
        get
        {
            return this.GetObject<Yaml_Trasnform>(mComponents[0]);
        }
    }

    public IEnumerable<Yaml_MonoBehaviour> monoScripts
    {
        get
        {
            var tNewComponets = new List<string>(mComponents);
            tNewComponets.RemoveAt(0);
            if (tNewComponets.Count > 0)
            {
                foreach (var item in tNewComponets)
                {
                    yield return this.GetObject<Yaml_MonoBehaviour>(item);
                }
            }
        }
    }

    public Yaml_GameObject(Yaml_BaseArg pArg) : base(pArg)
    {
    }

    public override void Resolver(string pContent)
    {
        if (pContent.StartsWith("  m_Name:"))
        {
            mName = GetNameByContent(pContent);
            return;
        }

        if (pContent.StartsWith("  - component: "))
        {
            var tVal = GetFileIDByContent(pContent, "- component");
            if (tVal != null) mComponents.Add(tVal);
            return;
        }
    }

    public override string ToString()
    {
        var tScriptContents = new List<string>();
        foreach (var script in monoScripts)
        {
            tScriptContents.Add(script.ToString());
        }

        var tCount = transform.parentCount;
        return string.Format("{0} [{1}][{2}]", new string('-', tCount) + mName, transform.siblingIndex, string.Join(",", tScriptContents.ToArray()));
    }

    public override void ToTreeStruct()
    {
        Debug.Log(ToString());
        foreach (var item in transform.childs)
        {
            item.ToTreeStruct();
        }
    }
}