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

    internal class AdTokenResponse
    {
        public string access_token { get; set; }
    }

    public class FunctionAppMetadataProperties
    {
        public object name { get; set; }
        public string publishingUserName { get; set; }
        public string publishingPassword { get; set; }
        public object publishingPasswordHash { get; set; }
        public object publishingPasswordHashSalt { get; set; }
        public object metadata { get; set; }
        public bool isDeleted { get; set; }
        public string scmUri { get; set; }
    }

    public class FunctionAppMetadata
    {
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string location { get; set; }
        public FunctionAppMetadataProperties properties { get; set; }
    }
}
