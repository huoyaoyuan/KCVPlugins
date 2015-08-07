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
        public string origin;
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
}