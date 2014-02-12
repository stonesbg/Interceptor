using System.Xml.Serialization;
namespace Interceptor.Models
{
    public class Interceptor 
    {
        [XmlArrayItemAttribute("Service", IsNullable=false)]
        public InterceptorService[] Services { get; set; }
    }
}