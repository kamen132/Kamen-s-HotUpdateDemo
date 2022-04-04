using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ILRuntimeDemo;
using UnityEngine;

public class HotFixBinding
{
    private bool _isBind = false;
    private static HotFixBinding _instance;
    public static HotFixBinding instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new HotFixBinding();
            }

            return _instance;
        }
    }
    //热更dll加载路径
    private const string DLLPATH = "Assets/GameData/HotFix/HotFix.dll.txt";
    public void Init()
    {
        Debug.LogError("初始化熱更代碼開始！！！");
        if (!_isBind)
        {
            _isBind = true;
            TextAsset dllText = ResourceManager.Instance.LoadResource<TextAsset>(DLLPATH);
            MemoryStream ms = new MemoryStream(dllText.bytes);
            HotFixFacade.Instance.appdomain.LoadAssembly(ms, null, new ILRuntime.Mono.Cecil.Pdb.PdbReaderProvider());
            Debug.Log("HotFixBinding RegisterDelegate");
            RegisterDelegate(HotFixFacade.Instance.appdomain);
            Debug.Log("HotFixBinding CrossBindingAdapter");
            CrossBindingAdapter(HotFixFacade.Instance.appdomain);
            Debug.Log("HotFixBinding LitJson.JsonMapper.RegisterILRuntimeCLRRedirection");
            //LitJson.JsonMapper.RegisterILRuntimeCLRRedirection(HotFixFacade.Instance.appdomain);
            Debug.Log("HotFixBinding RegisterValueTypeBinder");
            HotFixFacade.Instance.appdomain.RegisterValueTypeBinder(typeof(Vector3), new Vector3Binder());
            HotFixFacade.Instance.appdomain.RegisterValueTypeBinder(typeof(Quaternion), new QuaternionBinder());
            HotFixFacade.Instance.appdomain.RegisterValueTypeBinder(typeof(Vector2), new Vector2Binder());
            try
            {
                //HotFixFacade.Instance.appdomain.CLRBindings = false;
#if UNITY_EDITOR
                System.Type clrType = System.Type.GetType("ILRuntime.Runtime.Generated.CLRBindings");
                if (clrType != null)
                    clrType.GetMethod("Initialize").Invoke(null, new object[] {HotFixFacade.Instance.appdomain}); // CLR绑定
#else
                ILRuntime.Runtime.Generated.CLRBindings.Initialize(HotFixFacade.Instance.appdomain);
#endif
                //HotFixFacade.Instance.appdomain.CLRBindings = true;
            }
            catch (Exception e)
            {
                Debug.Log("HotFixBinding CLRBindings Failed:" + e.Message + " " + e.StackTrace);
                Debug.LogError("HotFixBinding CLRBindings Failed:" + e.Message + " " + e.StackTrace);
            }
            Debug.Log("HotFixBinding Finish");
        }
    }

    public static void RegisterDelegate(ILRuntime.Runtime.Enviorment.AppDomain appdomain)
    {
        appdomain.DelegateManager.RegisterMethodDelegate<System.String, UnityEngine.Object, System.Object, System.Object, System.Object>();
        appdomain.DelegateManager.RegisterMethodDelegate<System.Object>();
        appdomain.DelegateManager.RegisterMethodDelegate<UnityEngine.GameObject>();
        appdomain.DelegateManager.RegisterMethodDelegate<UnityEngine.GameObject, UnityEngine.Vector2>();
        appdomain.DelegateManager.RegisterFunctionDelegate<System.String, System.Boolean>();
        appdomain.DelegateManager.RegisterMethodDelegate<System.Boolean>();
        appdomain.DelegateManager.RegisterFunctionDelegate<System.Single>();
        appdomain.DelegateManager.RegisterMethodDelegate<System.Single>();
        appdomain.DelegateManager.RegisterMethodDelegate<System.Boolean, System.String>();
        appdomain.DelegateManager.RegisterMethodDelegate<UnityEngine.Object>();
        appdomain.DelegateManager.RegisterMethodDelegate<System.String, System.Object>();
        appdomain.DelegateManager.RegisterMethodDelegate<UnityEngine.GameObject, System.Boolean>();
        appdomain.DelegateManager.RegisterFunctionDelegate<System.Int32>();
        appdomain.DelegateManager.RegisterMethodDelegate<System.Int32>();
        appdomain.DelegateManager.RegisterMethodDelegate<UnityEngine.GameObject, System.Single>();
        appdomain.DelegateManager.RegisterMethodDelegate<UnityEngine.Vector2>();
        appdomain.DelegateManager.RegisterFunctionDelegate<UnityEngine.Vector3>();
        appdomain.DelegateManager.RegisterMethodDelegate<UnityEngine.Vector3>();
        appdomain.DelegateManager.RegisterMethodDelegate<System.Boolean, UnityEngine.Vector3>();
        appdomain.DelegateManager.RegisterMethodDelegate<System.Int32, System.Object, System.Int32, System.Object>();
        appdomain.DelegateManager.RegisterFunctionDelegate<System.Int64>();
        appdomain.DelegateManager.RegisterFunctionDelegate<System.Object, System.Object, System.Int32>();
        appdomain.DelegateManager.RegisterMethodDelegate<System.Int64>();
        appdomain.DelegateManager.RegisterMethodDelegate<System.Int32, System.Object>();
        appdomain.DelegateManager.RegisterFunctionDelegate<System.Single, System.Single, System.Boolean>();
        appdomain.DelegateManager.RegisterMethodDelegate<System.Single, System.Int32, System.Int32>();
        appdomain.DelegateManager.RegisterMethodDelegate<System.String>();
        appdomain.DelegateManager.RegisterMethodDelegate<UnityEngine.Playables.PlayableDirector>();
        appdomain.DelegateManager.RegisterMethodDelegate<UnityEngine.Color>();
        appdomain.DelegateManager.RegisterFunctionDelegate<UnityEngine.Color>();
        appdomain.DelegateManager.RegisterMethodDelegate<System.String, System.Boolean, System.String>();
        appdomain.DelegateManager.RegisterFunctionDelegate<System.Int32, System.Int32, System.Boolean>();
        appdomain.DelegateManager.RegisterMethodDelegate<System.Object, System.Collections.Generic.List<UnityEngine.GameObject>>();
        appdomain.DelegateManager.RegisterMethodDelegate<System.Int32, System.Int32>();
        appdomain.DelegateManager.RegisterMethodDelegate<UnityEngine.GameObject, System.Int32, System.Int32>();
        appdomain.DelegateManager.RegisterFunctionDelegate<UnityEngine.Vector4>();
        appdomain.DelegateManager.RegisterMethodDelegate<UnityEngine.Vector4>();
        appdomain.DelegateManager.RegisterMethodDelegate<UnityEngine.Quaternion>();
        appdomain.DelegateManager.RegisterDelegateConvertor<System.Threading.ThreadStart>((act) =>
        {
            return new System.Threading.ThreadStart(() =>
            {
                ((Action) act)();
            });
        });

        appdomain.DelegateManager.RegisterDelegateConvertor<System.Threading.WaitCallback>((act) =>
        {
            return new System.Threading.WaitCallback((state) =>
            {
                ((Action<System.Object>) act)(state);
            });
        });
        appdomain.DelegateManager.RegisterDelegateConvertor<System.Comparison<System.Object[]>>((act) =>
        {
            return new System.Comparison<System.Object[]>((x, y) =>
            {
                return ((Func<System.Object[], System.Object[], System.Int32>) act)(x, y);
            });
        });


        appdomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction>((act) =>
        {
            return new UnityEngine.Events.UnityAction(() =>
            {
                ((Action) act)();
            });
        });
        
        appdomain.DelegateManager.RegisterDelegateConvertor<OnAsyncObjFinish>((action) =>
        {
            return new OnAsyncObjFinish((path, obj, param1, param2, param3) =>
            {
                ((System.Action<System.String, UnityEngine.Object, System.Object, System.Object, System.Object>)
                    action)(path, obj, param1, param2, param3);
            });
        });

        appdomain.DelegateManager.RegisterMethodDelegate<System.String, System.Byte[], System.Int64>();
        appdomain.DelegateManager.RegisterMethodDelegate<System.String, System.Byte[]>();


    }

    public static void CrossBindingAdapter(ILRuntime.Runtime.Enviorment.AppDomain appdomain)
    {
        //这里做一些ILRuntime的注册，这里应该写继承适配器的注册，为了演示方便，这个例子写在OnHotFixLoaded了
        //appdomain.RegisterCrossBindingAdaptor(new MonoBehaviourAdapter());
        appdomain.RegisterCrossBindingAdaptor(new CoroutineAdapter());
        appdomain.RegisterCrossBindingAdaptor(new TestClassBaseAdapter());
        appdomain.RegisterValueTypeBinder(typeof(Vector3), new Vector3Binder());
        appdomain.RegisterValueTypeBinder(typeof(Vector2), new Vector2Binder());
        appdomain.RegisterValueTypeBinder(typeof(Quaternion), new QuaternionBinder());
        
        appdomain.RegisterCrossBindingAdaptor(new WindowAdapter());
    }


}
