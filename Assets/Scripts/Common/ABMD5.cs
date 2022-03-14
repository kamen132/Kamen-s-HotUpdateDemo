using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
[System.Serializable]
public class ABMD5 
{
   [XmlElement("ABMD5List")]
   public List<ABMD5Base> ABMD5List { get; set; }
}

[System.Serializable]
public class ABMD5Base
{
   [XmlElement("Name")]
   public string Name { get; set; }
   [XmlElement("Md5")]
   public string Md5 { get; set; }
   [XmlElement("Size")]
   public float Size { get; set; }
}
