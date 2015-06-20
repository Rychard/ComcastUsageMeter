using System.Xml.Serialization;

namespace ComcastUsageMeter.Shared.Responses
{
    public abstract class ComcastResponseBase
    {
        [XmlElement(ElementName = "status")]
        public Status Status { get; set; }
    }
}
