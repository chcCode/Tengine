using System.Collections.Generic;
using System.Linq;
using GameConfig;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 道具配置管理器。封装 TbItem 查询，避免业务代码散落 ConfigSystem 调用。
    /// </summary>
    public class ItemConfigMgr
    {
        private static ItemConfigMgr _instance;
        public static ItemConfigMgr Instance => _instance ??= new ItemConfigMgr();

        public Item GetItem(int id)
        {
            return ConfigSystem.Instance.Tables.TbItem.GetOrDefault(id);
        }

        public IReadOnlyList<Item> GetAllItems()
        {
            return ConfigSystem.Instance.Tables.TbItem.DataList;
        }

        public List<Item> GetItemsByType(EItemType type)
        {
            return ConfigSystem.Instance.Tables.TbItem.DataList
                .Where(i => i.Type == type)
                .ToList();
        }

        public List<Item> GetItemsByQuality(EQuality quality)
        {
            return ConfigSystem.Instance.Tables.TbItem.DataList
                .Where(i => i.Quality == quality)
                .ToList();
        }
    }
}
