using System;
using System.Xml.Serialization;

namespace ComcastUsageMeter.Shared.Responses
{
    [XmlRoot(ElementName = "status")]
    public class Status
    {
        [XmlElement(ElementName = "error_code")]
        public String ErrorCode { get; set; }

        [XmlElement(ElementName = "error_text")]
        public String ErrorText { get; set; }

        [XmlElement(ElementName = "update_url_mac")]
        public String UpdateUrlMacintosh { get; set; }

        [XmlElement(ElementName = "update_url_pc")]
        public String UpdateUrlWindows { get; set; }
    }
}
