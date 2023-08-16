using System;
using System.Globalization;
using System.Windows.Data;

namespace WoTModAssistant
{
    public class ActivateDeactivateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isEnabled = (bool)value;
            return isEnabled ? "Deactivate" : "Activate";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
