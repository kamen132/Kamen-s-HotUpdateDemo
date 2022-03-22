using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class HotPatchManager : Singleton<HotPatchManager>
{
    private string m_CurVersion;
    //资源下载目录
    private string m_DownLoadPath = Application.persistentDataPath + "/DownLoad";
    public string m_CurPackName;
    //已下载的xml
    private string m_LocalXmlPath = Application.persistentDataPath + "/LocalInfo.xml";
    private MonoBehaviour m_Mono;
    private string m_serverXmlPath = Application.persistentDataPath + "/ServerInfo.xml";
    private ServerInfo m_serverInfo;
    private ServerInfo m_LocalInfo;
    private VersionInfo m_GameVersion;
    
    //当前热更Patch
    private Patches m_CurrentPatchs;
    
    //所有需要热更的资源
    private Dictionary<string,Patch> m_HotFixDic=new Dictionary<string, Patch>();
    
    //所有需要下载的东西
    private List<Patch> m_DownLoadList=new List<Patch>();
    //所有需要下载的东西dict
    private Dictionary<string, Patch> m_DownLoadDic = new Dictionary<string, Patch>();

    //文件下载失败回调
    public Action<string> ItemError;
    //文件完成回调
    public Action LoadOver;
    //服务器列表获取错误回调
    public Action ServerInfoError;
    
    //服务器的资源名对应的MD5值  用于下载后md5
    public Dictionary<string, string> m_DownLoadMd5Dic = new Dictionary<string, string>();
    
    //储存已下载的资源
    public List<Patch> m_AlreadyDownList=new List<Patch>();
    //是否已经开始下载
    private bool m_StartDownLoad = false;

    //尝试重复下载次数
    private int m_tryDownCount = 0;
    //重新下载最大次数
    private const int DOWNLOADCOUNT = 4;
    
    
    /// <summary>
    /// 需要下载的资源总个数
    /// </summary>
    public int LoadFileCount { get; set; } = 0;
    /// <summary>
    /// 需要下载资源的大小  kb
    /// </summary>
    public float LoadSunSize { get; set; } = 0;
    //当前正在下载的资源
    public DownLoadAssetBundle m_CurDownLoad = null;
    
    public void Init(MonoBehaviour mono)
    {
        this.m_Mono = mono;
    }

    /// <summary>
    /// 計算AB路徑
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public string ComputeABPath(string name)
    {
        Patch patch = null;
        m_HotFixDic.TryGetValue(name, out patch);
        if (patch!=null)
        {
            return m_DownLoadPath + "/" + name;
        }
        return "";
    }

    //检查热更  是否有热更
    public void CheckVersion(Action<bool> hotCallBack = null)
    {
        m_tryDownCount = 0;
        m_HotFixDic.Clear();
        //读取本地版本 版本
        ReadVersion();
        //读取服务器xml
        m_Mono.StartCoroutine(ReadXml(() =>
        {
            //反序列化为空
            if (m_serverInfo == null)
            {
                //临时处理
                if (hotCallBack != null)
                {
                    hotCallBack(false);
                }
                return;
            }

            foreach (var version in m_serverInfo.GameVersion)
            {
                //找到当前版本version
                if (version.Version == m_CurVersion)
                {
                    m_GameVersion = version;
                    break;
                }
            }
            GetHotAB();
            if (CheckLocalAndServerPatch())
            {
                //计算需要下载的资源
                ComputeDownload();
                if (File.Exists(m_serverXmlPath))
                {
                    if (File.Exists(m_LocalXmlPath))
                    {
                        File.Delete(m_LocalXmlPath);
                    }
                    File.Move(m_serverXmlPath,m_LocalXmlPath);
                }
            }
            else
            {
                //服务器信息 跟本地信息一致的话
                CheckLocalResource();
            }

            LoadFileCount = m_DownLoadList.Count;
            LoadSunSize = m_DownLoadList.Sum(x => x.Size);


            if (hotCallBack != null)
            {
                //临时代码
                hotCallBack(m_DownLoadList.Count > 0);
            }
        }));
    }
    /// <summary>
    /// 检查本地资源是否于服务器下载列表信息是否一致  ，主要用于下载一半退出，在进入游戏，剩下部分下载
    /// </summary>
    private void CheckLocalResource()
    {
        m_DownLoadDic.Clear();
        m_DownLoadList.Clear();
        if (m_GameVersion != null && m_GameVersion.Pathceses != null & m_GameVersion.Pathceses.Length > 0)
        {
            m_CurrentPatchs = m_GameVersion.Pathceses[m_GameVersion.Pathceses.Length - 1];
            if (m_CurrentPatchs!=null&&m_CurrentPatchs.Files.Count>0)
            {
                foreach (var patch in m_CurrentPatchs.Files)
                {
                    if (Application.platform==RuntimePlatform.WindowsPlayer
                        || Application.platform==RuntimePlatform.WindowsEditor&& patch.Platform.Contains("StandaloneWindows64"))
                    {
                        AddDownLoadList(patch);
                    }
                    else if (Application.platform==RuntimePlatform.Android||
                             Application.platform==RuntimePlatform.Android&&patch.Platform.Contains("Android"))
                    {
                        AddDownLoadList(patch);
                    }
                    else if (Application.platform==RuntimePlatform.IPhonePlayer
                             ||Application.platform==RuntimePlatform.IPhonePlayer&&patch.Platform.Contains("IOS"))
                    {
                        AddDownLoadList(patch);
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// 检查本地已下载资源和服务器资源进行对比
    /// </summary>
    /// <returns></returns>
    private bool CheckLocalAndServerPatch()
    {
        //没有本地信息 说明是从来没有下载过热更
        if (!File.Exists(m_LocalXmlPath))
            return true;

        m_LocalInfo = BinarySerializeOpt.XmlDeserialize(m_LocalXmlPath, typeof(ServerInfo)) as ServerInfo;
        VersionInfo localGameVersion = null;
        if (m_LocalInfo!=null)
        {
            foreach (var version in m_LocalInfo.GameVersion)
            {
                if (version.Version==m_CurVersion)
                {
                    //获取当前笨笨信息
                    localGameVersion = version;
                    break;
                }
            }
        }
        if (localGameVersion!=null&&m_GameVersion!=null&&localGameVersion.Pathceses!=null&& m_GameVersion.Pathceses.Length>0&&
            m_GameVersion.Pathceses[m_GameVersion.Pathceses.Length-1].Version!=localGameVersion.Pathceses[localGameVersion.Pathceses.Length-1].Version)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    
    //读取服务器xml
    IEnumerator ReadXml(Action callBack)
    {
        string xmlUrl = "http://127.0.0.1/ServerInfo.xml";
        UnityWebRequest request = UnityWebRequest.Get(xmlUrl);
        request.timeout = 30;
        yield return request.SendWebRequest();

        if (request.isNetworkError)
        {
            Debug.LogError("下载失败：" + request.error);
        }
        else
        {
            FileTool.CreateFile(m_serverXmlPath,request.downloadHandler.data);
            if (File.Exists(m_serverXmlPath))
            {
                m_serverInfo = BinarySerializeOpt.XmlDeserialize(m_serverXmlPath, typeof(ServerInfo)) as ServerInfo;
            }
            else
            {
                Debug.LogError("热更配置写入失败");
            }
        }
        callBack?.Invoke();
    }

    /// <summary>
    /// 读取打包时的版本
    /// </summary>
    void ReadVersion()
    {
        TextAsset versionTxt = Resources.Load<TextAsset>("Version");
        if (versionTxt == null) 
        {
            Debug.LogError("未读到本地版本！！");
            return;
        }
        string[] all = versionTxt.text.Split('\n');
        if (all.Length>0)
        {
            string[] infoList = all[0].Split(';');
            if (infoList.Length>2)
            {
                m_CurVersion = infoList[0].Split('|')[1];
                m_CurPackName = infoList[1].Split('|')[1];
            }
        }
    }

    /// <summary>
    /// 获取所有热更宝信息
    /// </summary>
    private void GetHotAB()
    {
        if (m_GameVersion!=null&&m_GameVersion.Pathceses!=null&m_GameVersion.Pathceses.Length>0)
        {
            //最后一次热更包
            m_CurrentPatchs = m_GameVersion.Pathceses[m_GameVersion.Pathceses.Length - 1];
            if (m_CurrentPatchs!=null&&m_CurrentPatchs.Files!=null)
            {
                foreach (var patch in m_CurrentPatchs.Files)
                {
                    //获取所有需要热更包
                    m_HotFixDic.Add(patch.Name, patch);
                }
            }
        }
    }
    
    /// <summary>
    ///计算下载的资源
    /// </summary>
    private void ComputeDownload()
    {
        m_DownLoadDic.Clear();
        m_DownLoadList.Clear();
        m_DownLoadMd5Dic.Clear();
        if (m_GameVersion != null && m_GameVersion.Pathceses != null & m_GameVersion.Pathceses.Length > 0)
        {
            m_CurrentPatchs = m_GameVersion.Pathceses[m_GameVersion.Pathceses.Length - 1];
            if (m_CurrentPatchs!=null&&m_CurrentPatchs.Files.Count>0)
            {
                foreach (var patch in m_CurrentPatchs.Files)
                {
                    if (Application.platform==RuntimePlatform.WindowsPlayer
                        || Application.platform==RuntimePlatform.WindowsEditor&& patch.Platform.Contains("StandaloneWindows64"))
                    {
                        AddDownLoadList(patch);
                    }
                    else if (Application.platform==RuntimePlatform.Android||
                             Application.platform==RuntimePlatform.Android&&patch.Platform.Contains("Android"))
                    {
                        AddDownLoadList(patch);
                    }
                    else if (Application.platform==RuntimePlatform.IPhonePlayer
                             ||Application.platform==RuntimePlatform.IPhonePlayer&&patch.Platform.Contains("IOS"))
                    {
                        AddDownLoadList(patch);
                    }
                }
            }
        }
    }

    private void AddDownLoadList(Patch patch)
    {
        string filePath = m_DownLoadPath + "/" + patch.Name;
        //存在问件时 进行对比与服务器MD5是否一致  不一致放到下载队列  如果不存在直接放到下载目录
        if (File.Exists(filePath))
        {
            string md5 = MD5Manager.Instance.BuildFileMd5(filePath);
            if (patch.MD5!=md5)
            {
                m_DownLoadList.Add(patch);
                m_DownLoadDic.Add(patch.Name, patch);
                m_DownLoadMd5Dic.Add(patch.Name, patch.MD5);
            }
        }
        else
        {
            m_DownLoadList.Add(patch);
            m_DownLoadDic.Add(patch.Name, patch);
            m_DownLoadMd5Dic.Add(patch.Name, patch.MD5);
        }
    }

    /// <summary>
    /// 开始下载ab包
    /// </summary>
    /// <param name="callBack"></param>
    /// <returns></returns>
    public IEnumerator StartDownLoadAB(Action callBack, List<Patch> allPath=null)
    {
        m_AlreadyDownList.Clear();
        m_StartDownLoad = true;
        if (allPath==null)
        {
            allPath = m_DownLoadList;
        }
        //下载文件夹
        if (!Directory.Exists(m_DownLoadPath))
        {
            Directory.CreateDirectory(m_DownLoadPath);
        }
        List<DownLoadAssetBundle> downLoadAssetBundles = new List<DownLoadAssetBundle>();
        foreach (var patch in allPath)
        {
            downLoadAssetBundles.Add(new DownLoadAssetBundle(patch.Url,m_DownLoadPath));
        }

        foreach (var downLoad in downLoadAssetBundles)
        {
            m_CurDownLoad = downLoad;
            yield return m_Mono.StartCoroutine(downLoad.DownLoad());
            Patch patch = FindPatchByGamePath(downLoad.FileName);
            if (patch!=null)
            {
                m_AlreadyDownList.Add(patch);
            }
            downLoad.Destorty();
        }
        
        
        //已下载所有的资源进行  md5码校验
        //如果校验没有通过 .自动下载没有通过的文件，重复下载计数，达到一定次数后。反馈某某文件下载失败
        VerifyMD5(downLoadAssetBundles, callBack);
    }

    /// <summary>
    /// md5校验
    /// </summary>
    /// <param name="downLoadAssets"></param>
    /// <param name="callBack"></param>
    private void VerifyMD5(List<DownLoadAssetBundle> downLoadAssets, Action callBack)
    {
        List<Patch> downLoadList=new List<Patch>();
        foreach (var downLoad in downLoadAssets)
        {
            string md5 = "";
            if (m_DownLoadMd5Dic.TryGetValue(downLoad.FileName,out md5))
            {
                //计算md5是否与储存的md5是否一致
                if (MD5Manager.Instance.BuildFileMd5(downLoad.SaveFilePath)!=md5)
                {
                    //不一致重新进行下载
                    Debug.LogError(string.Format("此文件{0} MD5校验失败,即将重新下载",downLoad.FileName));
                    Patch patch = FindPatchByGamePath(downLoad.FileName);
                    if (patch!=null)
                    {
                        downLoadList.Add(patch);
                    }
                }
            }
        }
        if (downLoadList.Count<=0)
        {
            m_DownLoadMd5Dic.Clear();
            if (callBack!=null)
            {
                m_StartDownLoad = false;
                callBack();
            }
            if (LoadOver!=null)
            {
                LoadOver();
            }
        }
        else
        {
            if (m_tryDownCount>=DOWNLOADCOUNT)
            {
                //尝试下载四次之后
                string allName = "";
                m_StartDownLoad = false;
                foreach (var patch in downLoadList)
                {
                    allName += patch.Name+";";
                }
                Debug.LogError("资源重复下载四次MD5资源校验都失败 请检查资源："+allName);
                if (ItemError!=null)
                {
                    ItemError(allName);
                }
            }
            else
            {
                //没有的话记性重新下载
                m_tryDownCount++;
                m_DownLoadMd5Dic.Clear();
                foreach (var patch in downLoadList)
                {
                    m_DownLoadMd5Dic.Add(patch.Name, patch.MD5);
                }
                //自动重新下载  校验失败的文件
                m_Mono.StartCoroutine(StartDownLoadAB(callBack, downLoadList));
            }
        }
    }

    /// <summary>
    /// 获取下载进度
    /// </summary>
    /// <returns></returns>
    public float GetProgress()
    {
        // 已下载大小+当前下载大小/总下载大小
        return GetLoadSize() / LoadSunSize;
    }

    /// <summary>
    /// 获取已经下载总大小
    /// </summary>
    /// <returns></returns>
    public float GetLoadSize()
    {
        //已经下载的资源大小
        float alreadySize = m_AlreadyDownList.Sum(x => x.Size);
        //当前下载的资源
        float curAlreadySize = 0;
        if (m_CurDownLoad!=null)
        {
            Patch patch = FindPatchByGamePath(m_CurDownLoad.FileName);
            if (patch!=null&&!m_AlreadyDownList.Contains(patch))
            {
                curAlreadySize = m_CurDownLoad.GetProgess()*patch.Size;
            }
        }
        return alreadySize + curAlreadySize;
    }

    /// <summary>
    /// 根据名字查找对象的热更Patch
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private Patch FindPatchByGamePath(string name)
    {
        Patch patch = null;
        m_DownLoadDic.TryGetValue(name, out patch);
        return patch;
    }
    
}
public class FileTool
{
    /// <summary>
    /// 创建文件
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="bytes"></param>
    public static void CreateFile(string filePath, byte[] bytes)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        FileInfo file = new FileInfo(filePath);
        Stream stream = file.Create();
        stream.Write(bytes, 0, bytes.Length);
        stream.Close();
        stream.Dispose();
    }
}