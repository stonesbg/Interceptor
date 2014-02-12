namespace Interceptor.Models
{
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public class InterceptorService {

        public InterceptorServiceResponse Response { get; set; }

        public string Url {get; set; }

        public string HttpMethod { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name { get; set; }
    }
}
