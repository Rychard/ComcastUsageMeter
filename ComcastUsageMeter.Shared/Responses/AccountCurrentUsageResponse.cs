using System.Xml.Serialization;

namespace ComcastUsageMeter.Shared.Responses
{
    [XmlRoot(ElementName = "response")]
    public class AccountCurrentUsageResponse : ComcastResponseBase
    {
        [XmlElement(ElementName = "account")]
        public Account Account { get; set; }
    }
}
