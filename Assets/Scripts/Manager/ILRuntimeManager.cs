using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ILRuntime.Runtime.Enviorment;
using System.IO;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Mono.Cecil.Pdb;
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
        
        MemoryStream ms = new MemoryStream(dllText.bytes);
        MemoryStream p = new MemoryStream(pdBText.bytes);
        m_AppDomain.LoadAssembly(ms,p,new PdbReaderProvider());
        InitializeILRuntime();
        OnHotFixLoaded();
    }

    void InitializeILRuntime()
    {
        
    }

    void OnHotFixLoaded()
    {
        //第一个简单方法的调用
        //m_AppDomain.Invoke("HotFix.TestClass", "staticFunTest", null, null);
        
        //先单独获取类，之后一直使用这个类来调用
        IType type = m_AppDomain.LoadedTypes["HotFix.TestClass"];
        
        //第一种含参调用
        // //根据方法名和参数个数获取方法
        // IMethod method = type.GetMethod("staticFunTest",0);
        // m_AppDomain.Invoke(method, null, null);
        
        //第二种含参调用
        //根据获取函数来调用有参的函数
        IType stringType = m_AppDomain.GetType(typeof(string));
        List<IType> paraList = new List<IType>();
        paraList.Add(stringType);
        IMethod method = type.GetMethod("staticFunTest2",paraList,null);
        m_AppDomain.Invoke(method, null, "类型2执行成功");
        
        
        //实例化类
        //第一种实例化  （带参）
        object obj = m_AppDomain.Instantiate("HotFix.TestClass",new object[]{"测试成功！！！"});
        //第二种实例化 （不带参）
        //object obj = ((ILType) type).Instantiate();
        string str =(string) m_AppDomain.Invoke("HotFix.TestClass", "get_Str", obj, null);
        Debug.LogError("实例化：" + str);
        
        
        //调用泛型方法
        //第一种方法
        IType[] genericArguments=new IType[]{stringType};
        m_AppDomain.InvokeGenericMethod("HotFix.TestClass", "GenericMethod", genericArguments, null, "Kamen");
        //第二种方法
        paraList.Clear();
        paraList.Add(stringType);
        method = type.GetMethod("GenericMethod",paraList,genericArguments,null);
        m_AppDomain.Invoke(method, null, "Kamen2222"); 
    }
    
}
