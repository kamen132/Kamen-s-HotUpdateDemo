#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using ILRuntime.Mono.Cecil.Pdb;
using ILRuntimeDemo;
[System.Reflection.Obfuscation(Exclude = true)]
public class ILRuntimeCLRBinding
{
    //热更dll加载路径
    private const string DLLPATH = "Assets/GameData/HotFix/HotFix.dll.txt";
    private const string PDBPATH = "Assets/GameData/HotFix/HotFix.pdb.txt";
   [MenuItem("ILRuntime/通过自动分析热更DLL生成CLR绑定")]
    static void GenerateCLRBindingByAnalysis()
    {
        //用新的分析热更dll调用引用来生成绑定代码
        ILRuntime.Runtime.Enviorment.AppDomain domain = new ILRuntime.Runtime.Enviorment.AppDomain();
        using (System.IO.FileStream fs = new System.IO.FileStream(DLLPATH, System.IO.FileMode.Open, System.IO.FileAccess.Read))
        {
            //最初读取方式
            //domain.LoadAssembly(fs);
            //PDB文件，可调试，日志报错
            //TextAsset pdBText = ResourceManager.Instance.LoadResource<TextAsset>(PDBPATH);
            
            
            // //用下面的会报错：ObjectDisposedException: Cannot access a closed Stream.
            // /**
            // using (MemoryStream fs = new MemoryStream(dll))
            // {
            //     using (MemoryStream p = new MemoryStream(pdb))
            //     {
            //         appdomain.LoadAssembly(fs, p, new Mono.Cecil.Pdb.PdbReaderProvider());
            //     }
            // }
            // **/
            
              
            //读取热更资源DLL
            TextAsset dllText = ResourceManager.Instance.LoadResource<TextAsset>(DLLPATH);
            MemoryStream ms = new MemoryStream(dllText.bytes);
            //MemoryStream p = new MemoryStream(pdBText.bytes);
            domain.LoadAssembly(ms);
            //Crossbind Adapter is needed to generate the correct binding code
            InitILRuntime(domain);
            ILRuntime.Runtime.CLRBinding.BindingCodeGenerator.GenerateBindingCode(domain, "Assets/Scripts/Generated");
        }

        AssetDatabase.Refresh();
    }

    static void InitILRuntime(ILRuntime.Runtime.Enviorment.AppDomain domain)
    {
        //这里需要注册所有热更DLL中用到的跨域继承Adapter，否则无法正确抓取引用
         HotFixBinding.RegisterDelegate(domain);
         HotFixBinding.CrossBindingAdapter(domain);
    }
}
#endif
