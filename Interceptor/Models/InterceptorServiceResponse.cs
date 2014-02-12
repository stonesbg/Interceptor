namespace Interceptor.Models
{
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public class InterceptorServiceResponse {

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Type { get; set; }

        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value { get; set; }
    }
}