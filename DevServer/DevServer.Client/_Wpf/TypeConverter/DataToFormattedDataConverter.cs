using System;
using System.IO;
using System.Text;
using System.Windows.Data;
using General.Xml;
using Jayrock.Json;
//+
namespace DevServer.Client.TypeConverter
{
    public class DataToFormattedDataConverter : IValueConverter
    {
        //- @Convert -//
        public Object Convert(Object value, Type targetType, Object parameter, System.Globalization.CultureInfo culture)
        {
            String data = (String)value;
            if (!String.IsNullOrEmpty(data))
            {
                data = data.Trim();
                if (data.StartsWith("<"))
                {
                    try
                    {
                        data = XmlFormatter.Format(data);
                        return data;
                    }
                    catch
                    {
                        return value;
                    }
                }
                else if (data.StartsWith("{"))
                {
                    try
                    {
                        StringReader reader = new StringReader(data);
                        StringBuilder b = new StringBuilder();
                        StringWriter writer = new StringWriter(b);
                        //+
                        using (JsonTextReader jsonReader = new JsonTextReader(reader))
                        using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
                        {
                            jsonWriter.PrettyPrint = true;
                            jsonWriter.WriteFromReader(jsonReader);
                        }
                        return writer.GetStringBuilder().ToString();
                    }
                    catch
                    {
                        return value;
                    }
                }
            }
            return value;
        }

        //- @ConvertBack -//
        public Object ConvertBack(Object value, Type targetType, Object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}