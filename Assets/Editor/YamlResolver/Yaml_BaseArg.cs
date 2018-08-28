using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Yaml_BaseArg
{
    public string fileID { set; get; }
    public string fullPath { set; get; }
    public YamlType yamlType { set; get; }

    public Yaml_BaseArg(string pFileID, string pFullPath, YamlType pYamlType)
    {
        fileID = pFileID;
        fullPath = pFullPath;
        yamlType = pYamlType;
    }

}
