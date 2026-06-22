using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace TEngine
{
    /// <summary>
    /// 微信小游戏打包辅助工具。
    /// 依赖微信官方转换 SDK：<see href="https://github.com/wechat-miniprogram/minigame-tuanjie-transform-sdk"/>
    /// </summary>
    public static class WechatMiniGameBuildHelper
    {
        public const string DefineSymbol = "WEIXINMINIGAME";
        public const string SdkPackageName = "com.qq.weixin.minigame";
        public const string SdkFolderV2 = "Assets/WX-WASM-SDK-V2";
        public const string SdkFolderLegacy = "Assets/WX-WASM-SDK";
        public const string SdkGitUrl = "https://github.com/wechat-miniprogram/minigame-tuanjie-transform-sdk.git";

        private const string WxConvertCoreTypeName = "WeChatWASM.WXConvertCore";

        /// <summary>
        /// 是否已安装微信小游戏转换 SDK。
        /// </summary>
        public static bool IsSdkInstalled()
        {
            return FindWxConvertCoreType() != null
                   || AssetDatabase.IsValidFolder(SdkFolderV2)
                   || AssetDatabase.IsValidFolder(SdkFolderLegacy);
        }

        /// <summary>
        /// WebGL 平台是否已启用 WEIXINMINIGAME 宏。
        /// </summary>
        public static bool IsDefineSymbolEnabled()
        {
            return GetDefineSymbols(BuildTargetGroup.WebGL).Contains(DefineSymbol);
        }

        /// <summary>
        /// 设置 WebGL 平台的 WEIXINMINIGAME 宏。
        /// </summary>
        public static void SetDefineSymbol(bool enable)
        {
            var defines = GetDefineSymbols(BuildTargetGroup.WebGL);
            bool changed = enable
                ? defines.Add(DefineSymbol)
                : defines.Remove(DefineSymbol);

            if (changed)
            {
                SetDefineSymbols(BuildTargetGroup.WebGL, defines);
            }
        }

        /// <summary>
        /// 应用微信小游戏推荐的 WebGL PlayerSettings。
        /// </summary>
        public static void ApplyRecommendedWebGLSettings()
        {
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.threadsSupport = false;
            PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
            PlayerSettings.WebGL.memoryGrowthMode = WebGLMemoryGrowthMode.Geometric;
            PlayerSettings.WebGL.initialMemorySize = 256;
            PlayerSettings.WebGL.maximumMemorySize = 2048;

            if (IsSdkInstalled())
            {
                TrySetWebGLTemplate("PROJECT:WXTemplate");
            }
        }

        /// <summary>
        /// 打包前环境检查，返回 null 表示通过。
        /// </summary>
        public static string ValidateBuildEnvironment(bool requireSdkForExport)
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
            {
                return "当前未切换到 WebGL 平台，请先在 Build Settings 中 Switch Platform 到 WebGL。";
            }

            if (!IsDefineSymbolEnabled())
            {
                return $"WebGL 平台未定义 {DefineSymbol} 宏，资源模块将无法使用微信文件系统。";
            }

            if (requireSdkForExport && !IsSdkInstalled())
            {
                return $"未检测到微信小游戏转换 SDK。请通过 Package Manager 安装：\n{SdkGitUrl}";
            }

            return null;
        }

        /// <summary>
        /// 调用微信 SDK 导出小游戏。
        /// </summary>
        /// <param name="buildWebGL">是否在导出前由 SDK 构建 WebGL（TEngine 已构建 WebGL 时传 false）。</param>
        public static bool TryExportMiniGame(bool buildWebGL, out string errorMessage)
        {
            errorMessage = string.Empty;
            Type convertCoreType = FindWxConvertCoreType();
            if (convertCoreType == null)
            {
                errorMessage = $"未找到 {WxConvertCoreTypeName}，请先安装微信小游戏转换 SDK。";
                return false;
            }

            MethodInfo doExportMethod = convertCoreType.GetMethod(
                "DoExport",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(bool) },
                null);

            if (doExportMethod == null)
            {
                errorMessage = "微信 SDK 版本不兼容：未找到 WXConvertCore.DoExport(bool) 方法。";
                return false;
            }

            try
            {
                object result = doExportMethod.Invoke(null, new object[] { buildWebGL });
                string resultName = result?.ToString() ?? string.Empty;
                if (resultName.Contains("SUCCEED"))
                {
                    Debug.Log("[WechatMiniGame] 微信小游戏导出成功。");
                    return true;
                }

                errorMessage = $"微信小游戏导出失败：{resultName}";
                Debug.LogError($"[WechatMiniGame] {errorMessage}");
                return false;
            }
            catch (TargetInvocationException ex)
            {
                errorMessage = ex.InnerException?.Message ?? ex.Message;
                Debug.LogException(ex.InnerException ?? ex);
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Debug.LogException(ex);
                return false;
            }
        }

        [MenuItem("TEngine/Build/微信小游戏/启用 WEIXINMINIGAME 宏", false, 40)]
        public static void EnableDefineSymbolMenu()
        {
            SetDefineSymbol(true);
            Debug.Log($"[WechatMiniGame] 已为 WebGL 平台启用 {DefineSymbol} 宏。");
        }

        [MenuItem("TEngine/Build/微信小游戏/禁用 WEIXINMINIGAME 宏", false, 41)]
        public static void DisableDefineSymbolMenu()
        {
            SetDefineSymbol(false);
            Debug.Log($"[WechatMiniGame] 已为 WebGL 平台禁用 {DefineSymbol} 宏。");
        }

        [MenuItem("TEngine/Build/微信小游戏/应用推荐 WebGL 设置", false, 42)]
        public static void ApplyRecommendedSettingsMenu()
        {
            ApplyRecommendedWebGLSettings();
            Debug.Log("[WechatMiniGame] 已应用微信小游戏推荐 WebGL 设置。");
        }

        [MenuItem("TEngine/Build/微信小游戏/打开 SDK 安装说明", false, 43)]
        public static void OpenSdkInstallGuide()
        {
            Application.OpenURL("https://developers.weixin.qq.com/minigame/dev/guide/game-engine/unity-webgl-transform.html");
        }

        private static void TrySetWebGLTemplate(string templateName)
        {
            try
            {
                PlayerSettings.WebGL.template = templateName;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[WechatMiniGame] 设置 WebGL Template 失败: {ex.Message}");
            }
        }

        private static Type FindWxConvertCoreType()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(WxConvertCoreTypeName, false);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        private static System.Collections.Generic.HashSet<string> GetDefineSymbols(BuildTargetGroup group)
        {
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            return new System.Collections.Generic.HashSet<string>(
                defines.Split(';', StringSplitOptions.RemoveEmptyEntries));
        }

        private static void SetDefineSymbols(BuildTargetGroup group, System.Collections.Generic.HashSet<string> defines)
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", defines));
        }
    }
}
