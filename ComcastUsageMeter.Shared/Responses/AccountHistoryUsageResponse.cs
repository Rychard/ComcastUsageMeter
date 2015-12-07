using System.Xml.Serialization;

namespace ComcastUsageMeter.Shared.Responses
{
    [XmlRoot(ElementName = "response")]
    public sealed class AccountHistoryUsageResponse : ComcastResponseBase
    {
        [XmlElement(ElementName = "account")]
        public Account[] Account { get; set; }
    }
}