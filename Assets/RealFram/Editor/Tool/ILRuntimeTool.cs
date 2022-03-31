using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ILRuntimeTool : MonoBehaviour
{
    private const string UsedHotDll = "HotFix";
    private const string NotUsedHotDll = "OVERALL";

    [MenuItem("Kamen/Tools/[IL模式切换]使用主工程")]
    public static void ToMainProject()
    {
        var dataPath = Application.dataPath;
        var projectPath = dataPath.Substring(0, dataPath.Length - 7).Replace('\\', '/');
        var path = $"{projectPath}/HotFixProject/ToMainProject.bat";
        var startInfo = new ProcessStartInfo {CreateNoWindow = true, FileName = path, WindowStyle = ProcessWindowStyle.Maximized};
        Process.Start(startInfo);
        var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
        var newDefineSymbols = defineSymbols.Replace($"{UsedHotDll};", "").Replace(UsedHotDll, "");
        newDefineSymbols = newDefineSymbols.Replace($"{NotUsedHotDll};", "").Replace(NotUsedHotDll, "");
        newDefineSymbols = $"{newDefineSymbols};{NotUsedHotDll};";
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, newDefineSymbols);

        AssetDatabase.Refresh();
    }

    [MenuItem("Kamen/Tools/[IL模式切换]使用热更工程")]
    public static void ToHotProject()
    {
        var dataPath = Application.dataPath;
        var projectPath = dataPath.Substring(0, dataPath.Length - 7).Replace('\\', '/');
        var path = $"{projectPath}/HotFixProject/ToHotProject.bat";
        var startInfo = new ProcessStartInfo {CreateNoWindow = true, FileName = path, WindowStyle = ProcessWindowStyle.Maximized};
        Process.Start(startInfo);
        var dirPath = $"{projectPath}/HotFixProject/ScriptsHotFix";
        if (Directory.Exists(dirPath))
        {
            var dirInfo = new DirectoryInfo(dirPath);
            var fileInfos = dirInfo.GetFiles("*", SearchOption.AllDirectories);
            foreach (var fileInfo in fileInfos)
            {
                if (fileInfo.FullName.EndsWith(".meta"))
                {
                    fileInfo.Delete();
                }
            }
        }

        var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
        var newDefineSymbols = defineSymbols.Replace($"{UsedHotDll};", "").Replace(UsedHotDll, "");
        newDefineSymbols = newDefineSymbols.Replace($"{NotUsedHotDll};", "").Replace(NotUsedHotDll, "");
        newDefineSymbols = $"{newDefineSymbols};{UsedHotDll};";
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, newDefineSymbols);

        AssetDatabase.Refresh();
    }
}
