using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ILRuntime.Runtime.Enviorment;
using System.IO;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Mono.Cecil.Pdb;
using ILRuntime.Runtime.Intepreter;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;
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
        
        // MemoryStream ms = new MemoryStream(dllText.bytes);
        // MemoryStream p = new MemoryStream(pdBText.bytes);
        // m_AppDomain.LoadAssembly(ms,p,new PdbReaderProvider());
        InitializeILRuntime();
        OnHotFixLoaded();
    }

    void InitializeILRuntime()
    {
        
        //默认委托注册仅仅支持系统自带的Action以及Function
        m_AppDomain.DelegateManager.RegisterMethodDelegate<bool>();
        m_AppDomain.DelegateManager.RegisterFunctionDelegate<int, string>();
        m_AppDomain.DelegateManager.RegisterMethodDelegate<int>();
        m_AppDomain.DelegateManager.RegisterMethodDelegate<string>();
        m_AppDomain.DelegateManager
            .RegisterMethodDelegate<System.String, UnityEngine.Object, System.Object, System.Object, System.Object>();

        m_AppDomain.DelegateManager
            .RegisterMethodDelegate<System.String, UnityEngine.Object, System.Object, System.Object>();

        //自定义委托或Unity委托注册
        m_AppDomain.DelegateManager.RegisterDelegateConvertor<TestDelegateMethod>((action) =>
        {
            return new TestDelegateMethod((a) => { ((System.Action<int>) action)(a); });
        });

        m_AppDomain.DelegateManager.RegisterDelegateConvertor<TestDelegateFunction>((action) =>
        {
            return new TestDelegateFunction((a) => { return ((System.Func<int, string>) action)(a); });
        });

        m_AppDomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<bool>>((action) =>
        {
            return new UnityEngine.Events.UnityAction<bool>((a) => { ((System.Action<bool>) action)(a); });
        });

        m_AppDomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction>((action) =>
        {
            return new UnityEngine.Events.UnityAction(() => { ((System.Action) action)(); });
        });

        m_AppDomain.DelegateManager.RegisterDelegateConvertor<OnAsyncObjFinish>((action) =>
        {
            return new OnAsyncObjFinish((path, obj, param1, param2, param3) =>
            {
                ((System.Action<System.String, UnityEngine.Object, System.Object, System.Object, System.Object>)
                    action)(path, obj, param1, param2, param3);
            });
        });
        
        //跨域继承的注册
        m_AppDomain.RegisterCrossBindingAdaptor(new InheritanceAdapter());
        
        //绑定注册
        //HotFixBinding.instance.Init();
    }

    void OnHotFixLoaded()
    {
        //第一个简单方法的调用
        //m_AppDomain.Invoke("HotFix.TestClass", "staticFunTest", null, null);
        
        //先单独获取类，之后一直使用这个类来调用
        // IType type = m_AppDomain.LoadedTypes["HotFix.TestClass"];
        
        //第一种含参调用
        // //根据方法名和参数个数获取方法
        // IMethod method = type.GetMethod("staticFunTest",0);
        // m_AppDomain.Invoke(method, null, null);
        
        // //第二种含参调用
        // //根据获取函数来调用有参的函数
        // IType stringType = m_AppDomain.GetType(typeof(string));
        // List<IType> paraList = new List<IType>();
        // paraList.Add(stringType);
        // IMethod method = type.GetMethod("staticFunTest2",paraList,null);
        // m_AppDomain.Invoke(method, null, "类型2执行成功");
        //
        //
        // //实例化类
        // //第一种实例化  （带参）
        // object obj = m_AppDomain.Instantiate("HotFix.TestClass",new object[]{"测试成功！！！"});
        // //第二种实例化 （不带参）
        // //object obj = ((ILType) type).Instantiate();
        // string str =(string) m_AppDomain.Invoke("HotFix.TestClass", "get_Str", obj, null);
        // Debug.LogError("实例化：" + str);
        //
        //
        // //调用泛型方法
        // //第一种方法
        // IType[] genericArguments=new IType[]{stringType};
        // m_AppDomain.InvokeGenericMethod("HotFix.TestClass", "GenericMethod", genericArguments, null, "Kamen");
        // //第二种方法
        // paraList.Clear();
        // paraList.Add(stringType);
        // method = type.GetMethod("GenericMethod",paraList,genericArguments,null);
        // m_AppDomain.Invoke(method, null, "Kamen2222"); 

        //-----------------------------------------------------------------------------------------------------------------

//        委托调用
//        热更内部委托调用
        /*m_AppDomain.Invoke("HotFix_Project.TestDelegate", "Initialize", null, null);
        m_AppDomain.Invoke("HotFix_Project.TestDelegate", "RunTest", null, null);*/

        //m_AppDomain.Invoke("HotFix.TestDele", "Initialize2", null, null);
        //m_AppDomain.Invoke("HotFix.TestDele", "RunTest2", null, null);
        
        //委托调用
        // m_AppDomain.Invoke("HotFix.TestDelegate", "Initialize", null);
        // m_AppDomain.Invoke("HotFix.TestDelegate", "RunTest", null);

        //if (DelegateMethod != null)
        //{
        //    DelegateMethod(666);
        //}
        //if (DelegateFunc != null)
        //{
        //    string str = DelegateFunc(789);
        //    Debug.Log(str);
        //}
        //if (DelegateAction != null)
        //{
        //    DelegateAction("Ocean666");
        //}

        //-----------------------------------------------------------------------------------------------------------------

        
        //跨域继承
        // TestClassBase2 obj = m_AppDomain.Instantiate<TestClassBase2>("HotFix.TestInheritance");
        // obj.TestAbstract(1);
        // obj.TestVirtual("Kamen");
        m_AppDomain = HotFixFacade.Instance.appdomain;
        //CLR绑定测试
        long curTime = System.DateTime.Now.Ticks;
        m_AppDomain.Invoke("HotFix.TestCLRBinding", "RunTest",null);
        Debug.LogError("使用时间："+( System.DateTime.Now.Ticks-curTime));
        //1352499
        //1415537
    }
    
}

public class CLRBindingTestClass
{
    public static float DoSomeTest(int a,float b)
    {
        return a + b;
    }
}

/// <summary>
/// 跨域继承适配器
/// </summary>
public class InheritanceAdapter : CrossBindingAdaptor
{
    public override System.Type BaseCLRType
    {
        get
        {
            //想继承的类
            return typeof(TestClassBase2);
        }
    }

    public override System.Type AdaptorType
    {
        get
        {
            //实际的适配器类
            return typeof(Adapter);
        }
    }

    public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
    {
        return new Adapter(appdomain, instance);
    }

    class Adapter : TestClassBase2, CrossBindingAdaptorType
    {
        private ILRuntime.Runtime.Enviorment.AppDomain m_Appdomain;
        private ILTypeInstance m_Instance;
        private IMethod m_TestAbstract;
        private IMethod m_TestVirtual;
        private IMethod m_GetValue;
        private IMethod m_ToString;
        object[] param1 = new object[1];
        private bool m_TestVirtualInvoking = false;
        private bool m_GetValueInvoking = false;

        public Adapter()
        {
        }

        public Adapter(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
        {
            m_Appdomain = appdomain;
            m_Instance = instance;
        }

        public ILTypeInstance ILInstance
        {
            get { return m_Instance; }
        }

        //在适配器中重写所有需要在热更脚本重写的方法，并且将控制权转移到脚本里去
        public override void TestAbstract(int a)
        {
            if (m_TestAbstract == null)
            {
                m_TestAbstract = m_Instance.Type.GetMethod("TestAbstract", 1);
            }

            if (m_TestAbstract != null)
            {
                param1[0] = a;
                m_Appdomain.Invoke(m_TestAbstract, m_Instance, param1);
            }
        }

        public override void TestVirtual(string str)
        {
            if (m_TestVirtual == null)
            {
                m_TestVirtual = m_Instance.Type.GetMethod("TestVirtual", 1);
            }

            //必须要设定一个标识位来表示当前是否在调用中, 否则如果脚本类里调用了base.TestVirtual()就会造成无限循环
            if (m_TestVirtual != null && !m_TestVirtualInvoking)
            {
                m_TestVirtualInvoking = true;
                param1[0] = str;
                m_Appdomain.Invoke(m_TestVirtual, m_Instance, param1);
                m_TestVirtualInvoking = false;
            }
            else
            {
                base.TestVirtual(str);
            }
        }

        public override int Value
        {
            get
            {
                if (m_GetValue == null)
                {
                    m_GetValue = m_Instance.Type.GetMethod("get_Value", 1);
                }

                if (m_GetValue != null && !m_GetValueInvoking)
                {
                    m_GetValueInvoking = true;
                    int res = (int) m_Appdomain.Invoke(m_GetValue, m_Instance, null);
                    m_GetValueInvoking = false;
                    return res;
                }
                else
                {
                    return base.Value;
                }
            }
        }

        public override string ToString()
        {
            if (m_ToString == null)
            {
                m_ToString = m_Appdomain.ObjectType.GetMethod("ToString", 0);
            }

            IMethod m = m_Instance.Type.GetVirtualMethod(m_ToString);
            if (m == null || m is ILMethod)
            {
                return m_Instance.ToString();
            }
            else
            {
                return m_Instance.Type.FullName;
            }
        }
    }
}

public abstract class TestClassBase2
{
    public virtual int Value
    {
        get
        {
            return 0;
        }
    }
    public virtual void TestVirtual(string str)
    {
        Debug.LogError("TestClass TestVirtal  str==" + str);
    }
    public abstract void TestAbstract(int a);
}
