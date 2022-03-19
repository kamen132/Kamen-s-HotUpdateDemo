using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;

[System.Serializable]
public class ServerInfo
{
    [XmlElement("GameVersion")]
    public VersionInfo[] GameVersion;
    
}

//当前游戏版本 所有补丁
[System.Serializable]
public class VersionInfo
{
    [XmlAttribute]
    public string Version;

    //热更包
    [XmlAttribute]
    public Patches[] Pathceses;

}

/// 热更补丁
[System.Serializable]
public class Patches
{
    //当前热更的版本号
    [XmlAttribute]
    public int Version;

    //热更描述
    [XmlAttribute]
    public string Des;

    //所有的热更文件
    public List<Patch> Files;
}

// 单个补丁包
[System.Serializable]
public class Patch
{
    [XmlAttribute]
    public string Name;
    [XmlAttribute]
    public string Url;
    [XmlAttribute]
    public string Platform;
    [XmlAttribute]
    public string MD5;
    [XmlAttribute]
    public float Size;
}
 