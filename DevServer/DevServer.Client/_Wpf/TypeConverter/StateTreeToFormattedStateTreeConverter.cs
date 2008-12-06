using System;
using System.Windows.Data;
//+
namespace DevServer.Client.TypeConverter
{
    internal class StateTreeToFormattedStateTreeConverter : IValueConverter
    {
        //- @Convert -//
        public Object Convert(Object value, Type targetType, Object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                try
                {
                    String stateData = (String)value;
                    stateData = Themelia.Xml.XmlFormatter.Format(stateData);
                    return stateData;
                }
                catch
                {
                    return value;
                }
            }
            return String.Empty;
        }

        //- @ConvertBack -//
        public Object ConvertBack(Object value, Type targetType, Object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}