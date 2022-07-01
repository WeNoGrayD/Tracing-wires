using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace TraceWiresViewModel
{
    public class StringToImageSourceConverter : IValueConverter
    {
        /// <summary>
        /// Массив флагов загрузки изображения для элемента управления
        /// с итым индексом.
        /// </summary>
        private bool[] isLoaded = new bool[2] { true, true };

        private BitmapImage[] bitmaps = new BitmapImage[2];

        public object Convert(object value, Type targetType, 
                              object parameter, CultureInfo culture)
        {
            /*
                Если isLoaded == true, то в bitmaps хранятся
                неактуальные картинки, их надо обновить. 
                Повторять эту операцию лишний раз не надо.
             */

            if (isLoaded[0] && isLoaded[1])
            {
                string[] urisSplitted = ((string)value).Split(';');

                if (urisSplitted[0].Equals("NULL"))
                    bitmaps[0] = null;
                else
                    bitmaps[0] = new BitmapImage(
                        new Uri("icons\\" + urisSplitted[0], UriKind.Relative));

                if (urisSplitted[1].Equals("NULL"))
                    bitmaps[1] = null;
                else
                    bitmaps[1] = new BitmapImage(
                        new Uri("icons\\" + urisSplitted[1], UriKind.Relative));
            }

            int ind = (int)parameter;

            // Устанавливаем флаг загрузки одной из картинок в ложь.
            isLoaded[ind] = false;

            /*
                Если обе картинки были установлены, то
                устанавливаем оба флага в истину, таким образом
                подтверждая, что это актуальнейшая информация на данный момент
                и неактуальная - при повторной загрузке одной из картинок
                в будущем.
             */

            if (!isLoaded[0] && !isLoaded[1])
            {
                isLoaded[0] = true;
                isLoaded[1] = true;
            }

            return bitmaps[ind];
        }

        public object ConvertBack(object value, Type targetType, 
                                  object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
