using System;
using System.Runtime.Serialization.Json;

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
}