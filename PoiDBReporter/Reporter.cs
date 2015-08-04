using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper;
using Grabacr07.KanColleWrapper.Models.Raw;

namespace Huoyaoyuan.KCVPlugins.PoiDBReporter
{
    class Reporter
    {
        public bool EnableReporter { get; set; }
        public Reporter()
        {
            var proxy = KanColleClient.Current.Proxy;
            proxy.api_req_kousyou_createitem.TryParse<kcsapi_createitem>().Subscribe(x => this.CreateItem(x.Data, x.Request));
        }
        void CreateItem(kcsapi_createitem api,NameValueCollection request)
        {

        }
    }
}
