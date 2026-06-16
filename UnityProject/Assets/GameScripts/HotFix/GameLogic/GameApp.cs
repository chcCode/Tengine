using System.Collections.Generic;
using System.Reflection;
using GameConfig;
using GameLogic;
#if ENABLE_OBFUZ
using Obfuz;
#endif
using TEngine;
#pragma warning disable CS0436


/// <summary>
/// 游戏App。
/// </summary>
#if ENABLE_OBFUZ
[ObfuzIgnore(ObfuzScope.TypeName | ObfuzScope.MethodName)]
#endif
public partial class GameApp
{
    private static List<Assembly> _hotfixAssembly;

    /// <summary>
    /// 热更域App主入口。
    /// </summary>
    /// <param name="objects"></param>
    public static void Entrance(object[] objects)
    {
        GameEventHelper.Init();
        _hotfixAssembly = (List<Assembly>)objects[0];
        Log.Warning("======= 看到此条日志代表你成功运行了热更新代码 =======");
        Log.Warning("======= Entrance GameApp =======");
        Utility.Unity.AddDestroyListener(Release);
        Log.Warning("======= StartGameLogic =======");
        StartGameLogic();
    }
    
    private static void StartGameLogic()
    {
        LogLubanItemExample();
        GameModule.UI.ShowUIAsync<BattleMainUI>();
        GameModule.UI.ShowUIAsync<LoginUI>();
    }

    private static void LogLubanItemExample()
    {
        var potion = ItemConfigMgr.Instance.GetItem(1001);
        if (potion == null)
        {
            Log.Warning("Luban 示例：道具 1001 未找到，请先运行 TEngine/Luban/转表");
            return;
        }

        Log.Info("Luban 示例：{0} type={1} quality={2} price={3}",
            potion.Name, potion.Type, potion.Quality, potion.SellPrice);

        var weapons = ItemConfigMgr.Instance.GetItemsByType(EItemType.Weapon);
        Log.Info("Luban 示例：武器数量={0}", weapons.Count);
    }
    
    private static void Release()
    {
        SingletonSystem.Release();
        Log.Warning("======= Release GameApp =======");
    }
}