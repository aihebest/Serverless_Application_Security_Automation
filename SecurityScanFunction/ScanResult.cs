using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SecurityScanFunction
{
    public class ScanResult
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string ResourceId { get; set; }
        public string ResourceName { get; set; }
        public DateTime ScanTime { get; set; }
        public List<Finding> Findings { get; set; }
    }

    public class Finding
    {
        public string Severity { get; set; }
        public string Description { get; set; }
        public string Recommendation { get; set; }
    }
}