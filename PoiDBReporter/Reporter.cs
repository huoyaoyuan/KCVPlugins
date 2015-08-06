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

        #region Serializer
        DataContractJsonSerializer CreateItemSerializer = new DataContractJsonSerializer(typeof(CreateItem));
        #endregion

        public Reporter()
        {
            var proxy = KanColleClient.Current.Proxy;
            proxy.api_req_kousyou_createitem.TryParse<kcsapi_createitem>().Subscribe(x => this.CreateItemEvent(x.Data, x.Request));
        }
        void CreateItemEvent(kcsapi_createitem api,NameValueCollection request)
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
                res.successful = (api.api_create_flag != 0);
                res.teitokuLv = KanColleClient.Current.Homeport.Admiral.Level;
                res.itemId = res.successful ? api.api_slot_item.api_slotitem_id : int.Parse(api.api_fdata.Split(',')[1]);
                res.origin = "KCV Plugin Test";
                HttpWebRequest wrq = WebRequest.Create($"http://{SERVER_HOSTNAME}/api/report/v2/create_item") as HttpWebRequest;
                System.IO.MemoryStream mms = new System.IO.MemoryStream();
                wrq.UserAgent = "KCV Plugin Test";
                wrq.Method = "POST";
                byte[] data = Encoding.UTF8.GetBytes("data=");
                mms.Write(data,0,data.Length);
                CreateItemSerializer.WriteObject(mms, res);
                wrq.ContentLength = mms.Length;
                mms.Seek(0, System.IO.SeekOrigin.Begin);
                mms.CopyTo(wrq.GetRequestStream());
                wrq.GetRequestStream().Close();
                System.Diagnostics.Debug.Write(Encoding.UTF8.GetString(mms.ToArray()));
                wrq.ContentType = "text/plain-text";
                using (var wrs = wrq.GetResponse())
                {
                    HttpWebResponse httprs = wrs as HttpWebResponse;
                    System.Diagnostics.Debugger.Break();
                }
                GC.Collect(2);
            }
        }
    }
}
