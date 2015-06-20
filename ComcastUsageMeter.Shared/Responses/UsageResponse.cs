using System.Xml.Serialization;

namespace ComcastUsageMeter.Shared.Responses
{
    [XmlRoot(ElementName = "response")]
    public class UsageResponse : ComcastResponseBase
    {
        [XmlElement(ElementName = "device")]
        public Device Device { get; set; }
    }
}
