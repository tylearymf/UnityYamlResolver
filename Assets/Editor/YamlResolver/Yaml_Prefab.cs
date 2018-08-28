using UnityEngine;

public class Yaml_Prefab : Yaml_BaseObject
{
    string mRootGameObjectFileID;

    public Yaml_Prefab(Yaml_BaseArg pArg) : base(pArg)
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