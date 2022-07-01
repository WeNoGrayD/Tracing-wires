using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows.Data;
using TraceWiresClassLib;

namespace TraceWiresViewModel
{
    /// <summary>
    /// Конвертер возможности прокрутки экрана ДРП
    /// предоставляет услуги по включению/отключению
    /// кнопок прокрутки/передвижения по экрану ДРП.
    /// </summary>
    public class DWSScrollAbilityConverter : IValueConverter
    {
        private enum ScrollDirection : byte
        {
            UpRight,
            Up,
            UpLeft,
            Right,
            Left,
            DownRight,
            Down,
            DownLeft
        }

        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            bool canScroll = true;
            ScrollDirection scrollDir = (ScrollDirection)
                Enum.Parse(typeof(ScrollDirection), (string)parameter);
            BitmapsData.SignedCellPoint cPointBias = (BitmapsData.SignedCellPoint)value;
            
            switch(scrollDir)
            {
                case ScrollDirection.Up:
                case ScrollDirection.UpRight:
                case ScrollDirection.UpLeft:
                    {
                        canScroll &= (cPointBias.Y + BitmapsData.CashedDWSSize1 > 0);
                        break;
                    }
                case ScrollDirection.Down:
                case ScrollDirection.DownRight:
                case ScrollDirection.DownLeft:
                    {
                        canScroll &= (cPointBias.Y +
                                      BitmapsData.CashedDWSSize1 + BitmapsData.VisibleDWSSize1
                                      < Tracer.DWSSize1);// - 1);
                        break;
                    }

            }
            switch (scrollDir)
            {
                case ScrollDirection.Left:
                case ScrollDirection.UpLeft:
                case ScrollDirection.DownLeft:
                    {
                        canScroll &= (cPointBias.X + BitmapsData.CashedDWSSize2 > 0);
                        break;
                    }
                case ScrollDirection.Right:
                case ScrollDirection.UpRight:
                case ScrollDirection.DownRight:
                    {
                        canScroll &= (cPointBias.X +
                                      BitmapsData.CashedDWSSize2 + BitmapsData.VisibleDWSSize2
                                      < Tracer.DWSSize2);// - 1);
                        break;
                    }
            }

            return canScroll;
        }

        public object ConvertBack(object value, Type targetType, 
                                  object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
