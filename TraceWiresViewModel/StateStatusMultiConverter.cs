using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Controls;

namespace TraceWiresViewModel
{
    public class StateStatusMultiConverter// : IValueConverter, IMultiValueConverter
    {
        /*
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TMStateStatus tmStatus = (TMStateStatus)value;
            switch (tmStatus)
            {
                case TMStateStatus.Accepting: default: { return "Допускающее"; }
                case TMStateStatus.Rejecting:          { return "Отвергающее"; }
                case TMStateStatus.Usual:              { return "Обычное"; }
                case TMStateStatus.Start:              { return "Стартовое"; }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private TMState _tmState;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            _tmState = values[0] as TMState;
            if (_tmState == null)
                return null;
            return _tmState.Status;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            TMStateStatus tmStatus;
            switch ((string)((ComboBoxItem)value).Content)
            {
                case "Допускающее": default: { tmStatus = TMStateStatus.Accepting; break; }
                case "Отвергающее":          { tmStatus = TMStateStatus.Rejecting; break; }
                case "Обычное":              { tmStatus = TMStateStatus.Usual; break; }
                case "Стартовое":            { tmStatus = TMStateStatus.Start; break; }
            }
            return new object[] { null, tmStatus };
        }
        */
    }
}
