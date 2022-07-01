using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TraceWiresClassLib;
using TraceWiresViewModel;

namespace TraceWiresApplication
{
    /// <summary>
    /// Класс, осуществляющий сообщение между 
    /// приложением и данными.
    /// </summary>
    public class TWController
    {
        private BitmapsData _dwsScreen;

        public Tracer _tracer;

        public TWController(MainWindow mainWindow)
        {
            _tracer = new Tracer();

            _dwsScreen = new BitmapsData(_tracer);

            InitializeDWSScreen(mainWindow);

            mainWindow.grDWSScreenContainer.DataContext = _dwsScreen;
        }

        /// <summary>
        /// Автоматическое создание экрана ДРП.
        /// </summary>
        private void InitializeDWSScreen(MainWindow mainWindow)
        {
            for (int i = 0; i < BitmapsData.VisibleDWSSize1; i++)
                mainWindow.grDWSScreen.RowDefinitions.Add(new RowDefinition());
            for (int j = 0; j < BitmapsData.VisibleDWSSize2; j++)
                mainWindow.grDWSScreen.ColumnDefinitions.Add(new ColumnDefinition());

            Grid grDWSScreen = mainWindow.grDWSScreen,
                 grCellIcons;
            StringToImageSourceConverter cellImgSrcConverter;
            for (int i = 0; i < BitmapsData.VisibleDWSSize1; i++)
                for (int j = 0; j < BitmapsData.VisibleDWSSize2; j++)
                {
                    grCellIcons = (Grid)mainWindow.Resources["grCellIcons"];
                    grDWSScreen.Children.Add(grCellIcons);
                    Grid.SetRow(grCellIcons, i);
                    Grid.SetColumn(grCellIcons, j);
                    grCellIcons.DataContext = _dwsScreen;

                    cellImgSrcConverter = (StringToImageSourceConverter)
                        grCellIcons.Resources["cellImgSrcConverter"];

                    Image imgCellContent = UIHelper.FindChild<Image>(
                        grCellIcons, "imgCellContent", true),
                          imgCellPriority = UIHelper.FindChild<Image>(
                        grCellIcons, "imgCellPriority", true);
                    SetBindingToCellImage(imgCellContent, 0);
                    SetBindingToCellImage(imgCellPriority, 1);

                    void SetBindingToCellImage(Image img, int converterParam)
                    {
                        Binding cellBinding = new Binding(
                            nameof(_dwsScreen.bitmapsUris) +
                            $"[{i + BitmapsData.CashedDWSSize1}," +
                            $"{j + BitmapsData.CashedDWSSize2}]");
                        cellBinding.Mode = BindingMode.OneWay;
                        cellBinding.Converter = cellImgSrcConverter;
                        cellBinding.ConverterParameter = converterParam;

                        img.SetBinding(Image.SourceProperty, cellBinding);
                    }
                }
        }

        public void MoveDWSScreen(sbyte dX, sbyte dY)
        {
            _dwsScreen.MoveScreen(dX, dY);
        }
        
        public void UndoChanges(List<CellPoint> content, List<CellPoint> priority)
        {
            _dwsScreen.OnEndWireTracing(content, priority);
        }
    }
}
