using System;
using System.Configuration;
using System.IO;
using System.Xml.Serialization;
using NLog;

namespace Interceptor.ConfigurationHandler
{
    public class ConfigurationHandler
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public Models.Interceptor LoadConfig()
        {
            try
            {
                string filePath = ConfigurationManager.AppSettings["ConfigFileLocation"];

                var reader = new StreamReader(filePath);
                string xml = reader.ReadToEnd();

                var config = DeserializeFromXml<Models.Interceptor>(xml);
                return config;
            }
            catch (FileNotFoundException e)
            {
                Logger.Error("No configuration file found.", e);
                throw new Exception("No configuration file found.", e);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString(), ex);
                throw;
            }
        }

/*
        public static void SerializeToXml<T>(T obj, string fileName)
        {
            var ser = new XmlSerializer(typeof(T));
            var fileStream = new FileStream(fileName, FileMode.Create);
            ser.Serialize(fileStream, obj);
            fileStream.Close();
        }
*/

        private static T DeserializeFromXml<T>(string xml)
        {
            T result;
            var ser = new XmlSerializer(typeof (T));
            using (TextReader tr = new StringReader(xml))
            {
                result = (T) ser.Deserialize(tr);
            }
            return result;
        }
    }
}
