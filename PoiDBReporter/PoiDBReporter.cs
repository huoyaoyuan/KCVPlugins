using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grabacr07.KanColleViewer.Composition;
using Grabacr07.KanColleWrapper;


namespace Huoyaoyuan.KCVPlugins.PoiDBReporter
{
    [Export(typeof(IToolPlugin))]
    [ExportMetadata("Title", "PoiDBReporter")]
    [ExportMetadata("Description", "http://poi.0u0.moe/")]
    [ExportMetadata("Version", "1.0")]
    [ExportMetadata("Author", "@huoyaoyuan")]
    class PoiDBReporter : IToolPlugin
    {
        public string ToolName { get; } = "PoiDBReporter";

        public object GetSettingsView()
        {
            return null;
        }

        public object GetToolView()
        {
            return new ReporterView();
        }
    }
}
