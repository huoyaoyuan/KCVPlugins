using System;

namespace Huoyaoyuan.KCVPlugins.PoiDBReporter.Models
{
    [Serializable]
    struct CreateItem
    {
        public int[] items;
        public int secretary;
        public int itemId;
        public int teitokuLv;
        public bool successful;
    }
    [Serializable]
    struct CreateShip
    {
        public int[] items;
        public int kdockId;
        public int secretary;
        public int shipId;
        public int highspeed;
        public int teitokuLv;
        public bool largeFlag;
    }
    [Serializable]
    struct DropShip
    {
        public int shipId;
        public int mapId;
        public string quest;
        public int cellId;
        public string enemy;
        public string rank;
        public bool isBoss;
        public int teitokuLv;
        public int mapLv;
        public int[] enemyShips;
        public int enemyFormation;
    }
}