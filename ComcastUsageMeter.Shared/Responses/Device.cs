using System;
using System.Xml.Serialization;

namespace ComcastUsageMeter.Shared.Responses
{
    [XmlRoot(ElementName = "device")]
    public class Device
    {
        [XmlElement(ElementName = "device_usage")]
        public String DeviceUsage { get; set; }

        [XmlElement(ElementName = "device_mac")]
        public String MacAddress { get; set; }

        [XmlElement(ElementName = "policy_context")]
        public String PolicyContext { get; set; }

        [XmlElement(ElementName = "policy_name")]
        public String PolicyName { get; set; }

        [XmlElement(ElementName = "policy_type")]
        public String PolicyType { get; set; }
    }
}
