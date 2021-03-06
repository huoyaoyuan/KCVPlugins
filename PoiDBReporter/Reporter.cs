﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper;
using Grabacr07.KanColleWrapper.Models.Raw;
using Huoyaoyuan.KCVPlugins.PoiDBReporter.Models;
using System.Runtime.Serialization.Json;
using System.Net;
using System.Reactive.Linq;

namespace Huoyaoyuan.KCVPlugins.PoiDBReporter
{
    class Reporter
    {
        public bool EnableReporter { get; set; } = true;
        private readonly string SERVER_HOSTNAME = "poi.0u0.moe";
        private readonly string UAString =
#if DEBUG
            "KCV Plugin Test";
#else
            "KCV Plugin v1.0";
#endif
        private bool WaitForKDock = false;
        private CreateShip createship;
        private kcsapi_mst_mapinfo[] mapinfo;
        private DropShip dropship;
        private bool WaitForBattleResult = false;

        #region Serializer
        DataContractJsonSerializer CreateItemSerializer = new DataContractJsonSerializer(typeof(CreateItem));
        DataContractJsonSerializer CreateShipSerializer = new DataContractJsonSerializer(typeof(CreateShip));
        DataContractJsonSerializer DropShipSerializer = new DataContractJsonSerializer(typeof(DropShip));
        #endregion

        public Reporter()
        {
            var proxy = KanColleClient.Current.Proxy;
            proxy.api_req_kousyou_createitem.TryParse<kcsapi_createitem>().Subscribe(x => this.CreateItemEvent(x.Data, x.Request));
            proxy.api_req_kousyou_createship.TryParse<kcsapi_createship>().Subscribe(x => this.CreateShipEvent(x.Request));
            proxy.api_get_member_kdock.TryParse<kcsapi_kdock[]>().Subscribe(x => this.KDockEvent(x.Data));
            proxy.ApiSessionSource.Where(s => s.PathAndQuery.StartsWith("/kcsapi/api_get_member/mapinfo")).TryParse<kcsapi_mst_mapinfo[]>()
                .Subscribe(x => this.mapinfo = x.Data);
            proxy.api_req_sortie_battleresult.TryParse<kcsapi_battleresult>().Subscribe(x => this.BattleResultEvent(x.Data));
            proxy.api_req_combined_battle_battleresult.TryParse<kcsapi_battleresult>().Subscribe(x => this.BattleResultEvent(x.Data));
            proxy.api_req_map_start.TryParse<map_start_next>().Subscribe(x => this.StartNextEvent(x.Data));
            proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_req_map/next")
                .TryParse<map_start_next>().Subscribe(x => this.StartNextEvent(x.Data));
            proxy.api_req_sortie_battle.TryParse<api_battle>().Subscribe(x => this.BattleEvent(x.Data));
            proxy.api_req_combined_battle_battle.TryParse<api_battle>().Subscribe(x => this.BattleEvent(x.Data));
            proxy.api_req_combined_battle_airbattle.TryParse<api_battle>().Subscribe(x => this.BattleEvent(x.Data));
        }
        private async void ReportAsync(object v, DataContractJsonSerializer Serializer,string APIName)
        {
            HttpWebRequest wrq = WebRequest.Create($"http://{SERVER_HOSTNAME}/api/report/v2/{APIName}") as HttpWebRequest;
            wrq.UserAgent = UAString;
            wrq.Method = "POST";
            using (System.IO.MemoryStream mms = new System.IO.MemoryStream())
            {
                byte[] data = Encoding.UTF8.GetBytes("data=");
                mms.Write(data, 0, data.Length);
                Serializer.WriteObject(mms, v);
                wrq.ContentLength = mms.Length;
                mms.Seek(0, System.IO.SeekOrigin.Begin);
                using (System.IO.Stream reqs = wrq.GetRequestStream())
                    mms.CopyTo(reqs);
            }
            wrq.ContentType = "text/plain-text";
            try
            {
                using (var wrs = await wrq.GetResponseAsync()) { }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        void CreateItemEvent(kcsapi_createitem data,NameValueCollection request)
        {
            if (EnableReporter)
            {
                CreateItem res = new CreateItem();
                res.items = new int[4];
                res.items[0] = int.Parse(request["api_item1"]);
                res.items[1] = int.Parse(request["api_item2"]);
                res.items[2] = int.Parse(request["api_item3"]);
                res.items[3] = int.Parse(request["api_item4"]);
                res.secretary = KanColleClient.Current.Homeport.Organization.Fleets[1].Ships[0].Info.Id;
                res.successful = (data.api_create_flag != 0);
                res.teitokuLv = KanColleClient.Current.Homeport.Admiral.Level;
                res.itemId = res.successful ? data.api_slot_item.api_slotitem_id : int.Parse(data.api_fdata.Split(',')[1]);
                ReportAsync(res, CreateItemSerializer, "create_item");
            }
        }
        void CreateShipEvent(NameValueCollection request)
        {
            createship = new CreateShip();
            createship.items = new int[4];
            createship.items[0] = int.Parse(request["api_item1"]);
            createship.items[1] = int.Parse(request["api_item2"]);
            createship.items[2] = int.Parse(request["api_item3"]);
            createship.items[3] = int.Parse(request["api_item4"]);
            createship.kdockId = int.Parse(request["api_kdock_id"]) - 1;
            createship.secretary = KanColleClient.Current.Homeport.Organization.Fleets[1].Ships[0].Info.Id;
            createship.teitokuLv = KanColleClient.Current.Homeport.Admiral.Level;
            createship.largeFlag = (int.Parse(request["api_large_flag"]) != 0);
            createship.highspeed = int.Parse(request["api_highspeed"]);
            WaitForKDock = true;
        }
        void KDockEvent(kcsapi_kdock[] data)
        {
            if (!WaitForKDock) return;
            if (EnableReporter)
            {
                createship.shipId = data[createship.kdockId].api_created_ship_id;
                ReportAsync(createship, CreateShipSerializer, "create_ship");
            }
            WaitForKDock = false;
        }
        void StartNextEvent(map_start_next data)
        {
            dropship = new DropShip();
            dropship.mapId = data.api_maparea_id * 10 + data.api_mapinfo_no;
            dropship.cellId = data.api_no;
            dropship.isBoss = (data.api_event_id == 5);
            WaitForBattleResult = true;
        }
        void BattleEvent(api_battle data)
        {
            dropship.enemyFormation = data.api_formation[1];
        }
        void BattleResultEvent(kcsapi_battleresult data)
        {
            if (!WaitForBattleResult) return;
            dropship.shipId = (data.api_get_ship == null) ? -1 : data.api_get_ship.api_ship_id;
            dropship.enemy = data.api_enemy_info.api_deck_name;
            dropship.quest = data.api_quest_name;
            dropship.mapLv = mapinfo.Where(x => x.api_id == dropship.mapId).First().api_level;
            dropship.rank = data.api_win_rank;
            dropship.teitokuLv = KanColleClient.Current.Homeport.Admiral.Level;
            dropship.enemyShips = new int[6];
            Array.Copy(data.api_ship_id, 1, dropship.enemyShips, 0, 6);
            ReportAsync(dropship, DropShipSerializer, "drop_ship");
            WaitForBattleResult = false;
        }
    }
}
