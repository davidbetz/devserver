using System;
using System.IO;
using System.Text;
using System.Xml;
//+
namespace General.Xml
{
    public static class XmlFormatter
    {
        //- @Format -//
        public static String Format(String input)
        {
            MemoryStream stream = new MemoryStream();
            //+
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(input);
            //+
            XmlTextWriter writer = new XmlTextWriter(stream, null);
            writer.Formatting = Formatting.Indented;
            writer.IndentChar = ' ';
            writer.Indentation = 2;
            doc.Save(writer);
            //+
            Byte[] tmp = stream.GetBuffer();
            String output = Encoding.UTF8.GetString(tmp);
            Int32 lastAngle = output.LastIndexOf(">");
            output = output.Substring(0, lastAngle + 1);
            //+
            return output;
        }
    }
}