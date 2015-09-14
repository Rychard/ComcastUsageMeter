using System;

namespace ComcastUsageMeter.Shared
{
    public class DeserializedResponse<T>
    {
        public String ResponseBody { get; set; }
        public T ResponseObject { get; set; }
    }
}
