using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CinematicModel
{
    public static class Parser
    {
        private static readonly XmlSerializer xs;

        static Parser()
        {
            xs = new XmlSerializer(typeof(CinematicModel));
        }

        public static CinematicModel Parse(string filename)
        {
            CinematicModel model = null;
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                model = (CinematicModel)xs.Deserialize(fs);
            }
            return model; 
        }

        public static CinematicModel Parse(Stream stream)
        {
            return (CinematicModel)xs.Deserialize(stream);
        }

        public static void Write(this CinematicModel plan, Stream stream)
        {
            xs.Serialize(stream, plan);
        }

        public static void Write(this CinematicModel plan, string filePath)
        {
            Write(filePath, plan);
        }

        public static string WriteToXml(this CinematicModel plan)
        {
            using (StringWriter stream = new StringWriter())
            {
                xs.Serialize(stream, plan);
                stream.Flush();
                return stream.ToString();
            }
        }

        public static void Write(string filename, CinematicModel model)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Create))
            {
                xs.Serialize(fs, model);
            }
        }
    }
}
