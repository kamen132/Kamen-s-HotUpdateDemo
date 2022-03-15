using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

public class BunldeHotfix : EditorWindow
{
   [MenuItem("Kamen/Tools/打包热更包")]
   static void Init()
   {
      BunldeHotfix window = (BunldeHotfix)EditorWindow.GetWindow(typeof(BunldeHotfix), false, "热更包界面", true);
      window.Show();
   }

   private string md5Path = "";
   private OpenFileName m_OpenFileName = null;
   private void OnGUI()
   {
      GUILayout.BeginHorizontal();
      md5Path = EditorGUILayout.TextField("ABMD5路径：", md5Path, GUILayout.Width(300), GUILayout.Height(20));
      if (GUILayout.Button("选择版本ABMD5文件",GUILayout.Width(150),GUILayout.Height(30)))
      {
         m_OpenFileName=new OpenFileName();
         m_OpenFileName.structSize = Marshal.SizeOf(m_OpenFileName);
         m_OpenFileName.filter = "ABMD5文件(*.bytes)\0*.bytes";
         m_OpenFileName.file=new string(new char[256]);
         m_OpenFileName.maxFile = m_OpenFileName.file.Length;
         m_OpenFileName.fileTitle=new string(new char[64]);
         m_OpenFileName.maxCustFilter = m_OpenFileName.fileTitle.Length;
         m_OpenFileName.initialDir = (Application.dataPath + "/../Version").Replace("/","\\");//默认路径
         m_OpenFileName.title = "选择md5窗口";
         m_OpenFileName.flags = 0x0008000 | 0x00001000 | 0x00000800 | 0x00000008;
         if (LocalDialog.GetSaveFileName(m_OpenFileName))
         {
            Debug.Log(m_OpenFileName.file);
            md5Path = m_OpenFileName.file;
         }
      }
      GUILayout.EndHorizontal();
   }
}
