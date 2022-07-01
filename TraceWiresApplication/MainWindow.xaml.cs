using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TraceWiresClassLib;

namespace TraceWiresApplication
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TWController _controller;

        public MainWindow()
        {
            InitializeComponent();

            _controller = new TWController(this);
            //Uri uri1 = new Uri("icons\\ContactCornerWire2.png", UriKind.Relative), 
            //    uri2 = new Uri("icons\\ArrowToUp.png", UriKind.Relative);

            //imgContent.Source = new BitmapImage(uri1);
            //imgPriority.Source = new BitmapImage(uri2);


            //var t1 = imgCellContent1.DataContext;
            //var t2 = imgCellContent1.GetBindingExpression(Image.SourceProperty).ParentBinding;
            //var t3 = t2.Path;
        }

        IEnumerator<TraceWiresClassLib.Tracer.TracingState> _tracingEnumerator;

        public void btnTraceByStepClick(object sender, RoutedEventArgs e)
        {
            if (_tracingEnumerator == null)
                _tracingEnumerator = _controller._tracer.TraceWire();
            _tracingEnumerator.MoveNext();
            TraceWiresClassLib.Tracer.TracingState trst = _tracingEnumerator.Current;
            //if (trst == Tracer.TracingState.EndOfTracing || trst == Tracer.TracingState.Breaked)
            //    _tracingEnumerator = null;
        }

        public void btnTraceAutoClick(object sender, RoutedEventArgs e)
        {
            if (_tracingEnumerator == null)
                _tracingEnumerator = _controller._tracer.TraceWire();
            while (_tracingEnumerator.Current != Tracer.TracingState.EndOfTracing)
                _tracingEnumerator.MoveNext();
        }

        public void btnCancelTracingClick(object sender, RoutedEventArgs e)
        {
            if (_tracingEnumerator != null)
            {
                _tracingEnumerator.MoveNext();
                List<CellPoint> cellsWithContent, cellsWithPriorities;
                if (_tracingEnumerator.Current == Tracer.TracingState.CancellingTracedWire)
                {
                    (cellsWithContent, cellsWithPriorities) = _controller._tracer.UndoChanges();
                    _controller.UndoChanges(cellsWithContent, cellsWithPriorities);
                }
                _tracingEnumerator = null;
            }
        }

        public void btnUpLeftClick(object sender, RoutedEventArgs e)
        {
            _controller.MoveDWSScreen(-1, -1);
        }

        public void btnUpClick(object sender, RoutedEventArgs e)
        {
            _controller.MoveDWSScreen(0, -1);
        }

        public void btnUpRightClick(object sender, RoutedEventArgs e)
        {
            _controller.MoveDWSScreen(1, -1);
        }
        
        public void btnLeftClick(object sender, RoutedEventArgs e)
        {
            _controller.MoveDWSScreen(-3, 0);
        }

        public void btnRightClick(object sender, RoutedEventArgs e)
        {
            _controller.MoveDWSScreen(2, 0);
        }

        public void btnDownLeftClick(object sender, RoutedEventArgs e)
        {
            _controller.MoveDWSScreen(-1, 1);
        }

        public void btnDownClick(object sender, RoutedEventArgs e)
        {
            _controller.MoveDWSScreen(0, 1);
        }

        public void btnDownRightClick(object sender, RoutedEventArgs e)
        {
            _controller.MoveDWSScreen(1, 1);
        }
    }
}
