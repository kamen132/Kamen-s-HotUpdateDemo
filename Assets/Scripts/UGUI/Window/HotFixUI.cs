using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotFixUI : Window
{
    private HotFixPanel m_Panel;
    public override void Awake(params object[] paralist)
    {
        base.Awake(paralist);
        m_Panel = GameObject.GetComponent<HotFixPanel>();
        m_Panel.m_Image.fillAmount = 0;
        m_Panel.m_progress.text = string.Format("下载中。。。{0/F}M/s", 0);
        HotPatchManager.Instance.ServerInfoError += ServerInfoError;
        HotPatchManager.Instance.ItemError += ItemError;
    }

    void HotFix()
    {
        if (Application.internetReachability==NetworkReachability.NotReachable)
        {
            //提示网络错误，检测网络链接是否正常
            //GameStart.open 
        }
        else
        {
            
        }
    }

    void CheckVersion()
    {
        HotPatchManager.Instance.CheckVersion((hot) =>
        {
            if (hot)
            {
                //提示玩家确认是偶下载
            }
            else
            {
                //进入游戏
            }
        });
    }

    private void ServerInfoError()
    {
        
    }

    private void ItemError(string all)
    {
        
    }

    public override void OnClose()
    {
        base.OnClose();
        HotPatchManager.Instance.ServerInfoError -= ServerInfoError;
        HotPatchManager.Instance.ItemError -= ItemError;
    }
}
