using System;
using System.Xml.Serialization;

namespace ComcastUsageMeter.Shared.Responses
{
    [XmlRoot(ElementName = "response")]
    public class AuthenticationResponse : ComcastResponseBase
    {
        [XmlElement(ElementName = "access_token")]
        public String AccessToken { get; set; }
    }
}
