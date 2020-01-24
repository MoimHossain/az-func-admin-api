using System;
using System.Collections.Generic;
using System.Text;

namespace AzFunc.Admin.RestApi
{
    public class HostStatus
    {
        public string id { get; set; }
        public string state { get; set; }
        public string version { get; set; }
        public string versionDetails { get; set; }
        public int processUptime { get; set; }
    }
}
