using System;
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

namespace Huoyaoyuan.KCVPlugins.PoiDBReporter
{
    class Reporter
    {
        public bool EnableReporter { get; set; } = true;
        private readonly string SERVER_HOSTNAME = "poi.0u0.moe";
        private readonly string UAString = "KCV Plugin Test";
        private bool WaitForKDock = false;
        private CreateShip createship;

        #region Serializer
        DataContractJsonSerializer CreateItemSerializer = new DataContractJsonSerializer(typeof(CreateItem));
        DataContractJsonSerializer CreateShipSerializer = new DataContractJsonSerializer(typeof(CreateShip));
        #endregion

        public Reporter()
        {
            var proxy = KanColleClient.Current.Proxy;
            proxy.api_req_kousyou_createitem.TryParse<kcsapi_createitem>().Subscribe(x => this.CreateItemEvent(x.Data, x.Request));
            proxy.api_req_kousyou_createship.TryParse<kcsapi_createship>().Subscribe(x => this.CreateShipEvent(x.Request));
            proxy.api_get_member_kdock.TryParse<kcsapi_kdock[]>().Subscribe(x => this.KDockEvent(x.Data));
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
                res.origin = UAString;
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
    }
}
