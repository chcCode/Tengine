using Luban;
using GameConfig;
using TEngine;
using UnityEngine;

/// <summary>
/// 配置加载器。
/// </summary>
public class ConfigSystem
{
    private static ConfigSystem _instance;

    public static ConfigSystem Instance => _instance ??= new ConfigSystem();

    private bool _init = false;

    private Tables _tables;

    public Tables Tables
    {
        get
        {
            if (!_init)
            {
                Load();
            }

            return _tables;
        }
    }
    
    private IResourceModule _resourceModule;

    /// <summary>
    /// 加载配置。
    /// </summary>
    public void Load()
    {
        _tables = new Tables(LoadByteBuf);
        _init = true;
    }

    /// <summary>
    /// 加载二进制配置。
    /// </summary>
    /// <param name="file">FileName</param>
    /// <returns>ByteBuf</returns>
    private ByteBuf LoadByteBuf(string file)
    {
        if (_resourceModule == null)
        {
            _resourceModule = ModuleSystem.GetModule<IResourceModule>();
        }
        TextAsset textAsset = _resourceModule.LoadAsset<TextAsset>(file);
        if (textAsset == null)
        {
            Log.Error(
                "Config bytes [{0}] not found. 请先执行 TEngine/Luban/转表，确认 Assets/AssetRaw/Configs/bytes/{0}.bytes 存在，然后停止并重新进入 Play 模式刷新 YooAsset 模拟清单。",
                file);
            return null;
        }
        byte[] bytes = textAsset.bytes;
        return new ByteBuf(bytes);
    }
}