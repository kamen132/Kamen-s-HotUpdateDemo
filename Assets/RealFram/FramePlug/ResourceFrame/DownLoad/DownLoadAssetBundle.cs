using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DownLoadAssetBundle : DownLoadItem
{
    private UnityWebRequest m_WebRequest;
    
    public DownLoadAssetBundle(string url, string savePath) : base(url, savePath)
    {
    }

    public override IEnumerator DownLoad(Action callBack = null)
    {
        m_WebRequest = new UnityWebRequest(m_Url);
        m_StartDownLoad = true;
        m_WebRequest.timeout = 30;
        yield return m_WebRequest.SendWebRequest();

        if (m_WebRequest.isNetworkError)
        {
            Debug.LogError("DownLoad Error:" + m_WebRequest.error);
        }
        else
        {
            byte[] bytes = m_WebRequest.downloadHandler.data;
            FileTool.CreateFile(m_SavePath, bytes);
            callBack?.Invoke();
        }
    }

    public override float GetProgess()
    {
        if (m_WebRequest!=null)
        {
            return (long) m_WebRequest.downloadProgress;
        }
        return 0;
    }
    public override long GetCurLength()
    {
        if (m_WebRequest!=null)
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
        if (m_WebRequest!=null)
        {
            m_WebRequest.Dispose();
            m_WebRequest = null;
        }
    }
}
