using System;
using System.Windows.Data;
//+
namespace DevServer.Client.TypeConverter
{
    internal class InstanceStateToImageConverter : IValueConverter
    {
        //- @Convert -//
        public Object Convert(Object value, Type targetType, Object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(System.Windows.Media.ImageSource))
            {
                throw new InvalidOperationException("Target type must be of type ImageSource");
            }
            DevServer.Instance.InstanceState state = (DevServer.Instance.InstanceState)value;
            switch (state)
            {
                case DevServer.Instance.InstanceState.Started:
                    return "Image/bullet_green.png";
                case DevServer.Instance.InstanceState.Stopped:
                    return "Image/bullet_red.png";
                default:
                    throw new ArgumentOutOfRangeException("Unknown InstateState value");
            }
        }

        //- @ConvertBack -//
        public Object ConvertBack(Object value, Type targetType, Object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}