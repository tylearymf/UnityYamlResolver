using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class Test : Editor
{
    const string cPrefabName = "Test2018.2.3f1.prefab";

    [MenuItem("Test2/TestMethod")]
    static public void TestMethod()
    {
        YamlResolver.ResolveYaml(Path.Combine(Application.dataPath, "Test/" + cPrefabName), true);
    }

    [MenuItem("Test2/TestMethod1")]
    static public void TestMethod1()
    {
        var tGoName = cPrefabName.Replace(".prefab", string.Empty);
        var tPrefab = GameObject.Find(tGoName);
        if (tPrefab == null)
        {
            throw new System.NullReferenceException("找不到" + tGoName);
        }
        var tTrans = YamlHelper.GetSpriteByIndex(tPrefab, Path.Combine(Application.dataPath, "Test/" + cPrefabName), "62adb30369acbf943a67cee0f69267a3", "Emoticon - Rambo", 5);
        if (tTrans != null)
        {
            Selection.activeTransform = tTrans.transform;
        }
        else
        {
            Debug.LogError("逻辑错误");
        }
    }
}
