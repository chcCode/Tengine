using System.IO;
using UnityEditor;
using UnityEngine;

namespace TEngine.Editor
{
    public static class LubanTools
    {
        [MenuItem("TEngine/Luban/转表 &X", priority = -100)]
        private static void ZhuanXiaoYi()
        {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
            string scriptPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Configs/GameConfig/gen_code_bin_to_project_lazyload.sh"));
            string workDir = Path.GetDirectoryName(scriptPath);
            Debug.Log($"执行转表：{scriptPath}");
            ShellHelper.Run($"bash \"{scriptPath}\"", workDir);
#else
            string batPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Configs/GameConfig/gen_code_bin_to_project_lazyload.bat"));
            string workDir = Path.GetDirectoryName(batPath);
            Debug.Log($"执行转表：{batPath}");
            // AI_MODE=1 跳过 bat 末尾 pause；call 确保子脚本结束后返回
            ShellHelper.Run($"set AI_MODE=1&& call \"{batPath}\"", workDir);
#endif
            AssetDatabase.Refresh();
            Debug.Log("Luban 转表命令已执行完毕，请查看上方日志确认是否成功（应出现 bye~）。");
        }
    }
}