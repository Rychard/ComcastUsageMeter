using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ComcastUsageMeter.Shared.Responses
{
    [XmlRoot(ElementName = "home_device_details")]
    public class HomeDeviceDetails
    {
        [XmlElement(ElementName = "device")]
        public Device Device { get; set; }
    }
}
