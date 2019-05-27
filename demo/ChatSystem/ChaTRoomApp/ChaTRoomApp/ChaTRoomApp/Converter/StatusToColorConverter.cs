using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace ChaTRoomApp.Converter
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch(value.ToString())
            {
                case "":
                    {
                        return Color.Default;
                    }
                case "Offline":
                    {
                        return Color.Default;
                    }
                case "Leave":
                    {
                        return Color.Red;
                    }
                case "Online":
                    {
                        return Color.Green;
                    }

            }

            return Color.Default;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
