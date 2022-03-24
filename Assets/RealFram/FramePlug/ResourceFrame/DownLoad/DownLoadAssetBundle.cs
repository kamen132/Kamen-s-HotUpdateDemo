using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DownLoadAssetBundle : DownLoadItem
{
    //定义web请求
    private UnityWebRequest m_WebRequest;

    //构造函数
    public DownLoadAssetBundle(string url, string path) : base(url, path)
    {
    }

    //重写download
    public override IEnumerator DownLoad(Action callback = null)
    {
        //获取URL
        m_WebRequest = UnityWebRequest.Get(m_Url);
        //开始下载
        m_StartDownLoad = true;
        m_WebRequest.timeout = 30;
        //开始下载
        yield return m_WebRequest.SendWebRequest();
        m_StartDownLoad = false;
        //报错处理
        if (m_WebRequest.isNetworkError)
        {
            Debug.LogError("Download Error" + m_WebRequest.error);
        }
        else
        {
            byte[] bytes = m_WebRequest.downloadHandler.data;
            FileTool.CreateFile(m_SaveFilePath, bytes);
            callback?.Invoke();
        }
    }

    public override float GetProcess()
    {
        if (m_WebRequest != null)
        {
            return (long) m_WebRequest.downloadProgress;
        }

        return 0;
    }

    public override long GetCurLength()
    {
        if (m_WebRequest != null)
        {
            return (long) m_WebRequest.downloadedBytes;
        }

        return 0;
    }

    public override long GetLength()
    {
        return 0;
    }

    public override void Destorty()
    {
        if (m_WebRequest != null)
        {
            m_WebRequest.Dispose();
            m_WebRequest = null;
        }
    }
}
