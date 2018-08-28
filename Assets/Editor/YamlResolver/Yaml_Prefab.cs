using UnityEngine;

public class Yaml_Prefab : Yaml_BaseObject
{
    public const int cID = (int)YamlType.Prefab;

    string mRootGameObjectFileID;

    public Yaml_Prefab(string pID, string pFullPath) : base(pID, pFullPath)
    {
    }

    public override void Resolver(string pContent)
    {
        if (pContent.StartsWith("  m_RootGameObject:"))
        {
            var tVal = GetFileIDByContent(pContent, "m_RootGameObject");
            if (tVal != null) mRootGameObjectFileID = tVal;
            return;
        }
    }

    public override void ToTreeStruct()
    {
        var tRoot = this.GetObject<Yaml_GameObject>(mRootGameObjectFileID);
        if (tRoot == null)
        {
            Debug.Log("NULL");
        }
        else
        {
            tRoot.ToTreeStruct();
        }
    }
}