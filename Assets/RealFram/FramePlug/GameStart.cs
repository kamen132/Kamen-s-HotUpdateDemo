using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameStart : MonoSingleton<GameStart>
{
    private GameObject m_obj;
    protected override void Awake()
    {
        base.Awake();
        GameObject.DontDestroyOnLoad(gameObject);
        ResourceManager.Instance.Init(this);
        ObjectManager.Instance.Init(transform.Find("RecyclePoolTrs"), transform.Find("SceneTrs"));
        HotPatchManager.Instance.Init(this);
        UIManager.Instance.Init(transform.Find("UIRoot") as RectTransform, transform.Find("UIRoot/WndRoot") as RectTransform, transform.Find("UIRoot/UICamera").GetComponent<Camera>(), transform.Find("UIRoot/EventSystem").GetComponent<EventSystem>());
        RegisterUI();
    }
    // Use this for initialization
    void Start ()
    {

        UIManager.Instance.PopUpWnd(ConStr.HOTFIXPANEL, resource: true);
    }

    public IEnumerator StartGame(Image image,Text text)
    {
        image.fillAmount = 0;
        yield return null;
        //加载DLL
        AssetBundleManager.Instance.LoadAssetBundleConfig();
        text.text = "加载本地数据中.....";
        HotFixBinding.instance.Init();
        image.fillAmount = 0.2f;
        text.text = "加载数据表.....";
        LoadConfiger();
        image.fillAmount = 0.8f;
        text.text = "Load UI.....";
       
        image.fillAmount = 0.9f;
        text.text = "Load Scene.....";
        GameMapManager.Instance.Init(this);
        //绑定注册
        image.fillAmount = 1;
    }
    //注册UI窗口
    void RegisterUI()
    {
        UIManager.Instance.Register<Window>(ConStr.MENUPANEL);
        UIManager.Instance.Register<Window>(ConStr.LOADINGPANEL);
        UIManager.Instance.Register<HotFixUI>(ConStr.HOTFIXPANEL);
    }

    //加载配置表
    void LoadConfiger()
    {
        //ConfigerManager.Instance.LoadData<MonsterData>(CFG.TABLE_MONSTER);
        //ConfigerManager.Instance.LoadData<BuffData>(CFG.TABLE_BUFF);
    }
	
	// Update is called once per frame
	void Update ()
    {
        UIManager.Instance.OnUpdate();
	}

    private void OnApplicationQuit()
    {
#if UNITY_EDITOR
        ResourceManager.Instance.ClearCache();
        Resources.UnloadUnusedAssets();
        Debug.Log("清空编辑器缓存");
#endif
    }

    public static void OpenCommonConfrim(string des,UnityAction confirmActcion,UnityAction CancleAciotn)
    {
        GameObject obj=GameObject.Instantiate(Resources.Load("CommonConfigPanel")) as GameObject;
        obj.transform.SetParent(UIManager.Instance.m_WndRoot, false);
        CommonHotFix commonHotFix = obj.GetComponent<CommonHotFix>();
        commonHotFix.Show(des, confirmActcion, CancleAciotn);
    }
}
