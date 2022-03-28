using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ILRuntime.Runtime.Enviorment;
using System.IO;
public class ILRuntimeManager : Singleton<ILRuntimeManager>
{
    //热更dll加载路径
    private const string DLLPATH = "Assets/GameData/HotFix/HotFix.dll.txt";
    private const string PDBPATH = "Assets/GameData/HotFix/HotFix.pdb.txt";
    //程序集
    private AppDomain m_AppDomain;
    
    public void Init()
    {
        LoadHotFixAssembly();
    }

    /// <summary>
    /// 加载热更程序集
    /// </summary>
    void LoadHotFixAssembly()
    {
        //整个工程只有一个ILR的AppDomain
        m_AppDomain = new AppDomain();
        //读取热更资源DLL
        TextAsset dllText = ResourceManager.Instance.LoadResource<TextAsset>(DLLPATH);
        //PDB文件，可调试，日志报错
        TextAsset pdBText = ResourceManager.Instance.LoadResource<TextAsset>(PDBPATH);

        using (MemoryStream ms=new MemoryStream(dllText.bytes))
        {
            using (MemoryStream p = new MemoryStream(pdBText.bytes))
            {
                //m_AppDomain.LoadAssembly(ms,p,Mono);
            }
        }
    }
}
