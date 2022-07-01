using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraceWiresClassLib;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TraceWiresViewModel
{
    /// <summary>
    /// Данные о визуализации ДРП, передаваемые в качестве контекста
    /// приложению.
    /// </summary>
    public class BitmapsData : INotifyPropertyChanged
    {
        /// <summary>
        /// Размер видимой области ДРП по Y.
        /// </summary>
        public const int VisibleDWSSize1 = 7;

        /// <summary>
        /// Размер кэшируемой области ДРП по Y по обе стороны от видимой.
        /// </summary>
        public const int CashedDWSSize1 = 0;

        /// <summary>
        /// Размер видимой области ДРП по X.
        /// </summary>
        public const int VisibleDWSSize2 = 7;

        /// <summary>
        /// Размер кэшируемой области ДРП по X по обе стороны от видимой.
        /// </summary>
        public const int CashedDWSSize2 = 0;

        /// <summary>
        /// Полнй размер uri-матрицы по Y.
        /// </summary>
        public const int FullSize1 = VisibleDWSSize1 + (CashedDWSSize1 << 1);

        /// <summary>
        /// Полнй размер uri-матрицы по X.
        /// </summary>
        public const int FullSize2 = VisibleDWSSize2 + (CashedDWSSize2 << 1);

        /// <summary>
        /// Матрица, в которой хранятся иконки для визуализации ДРП.
        /// </summary>
        public ObservableMatrix<string> bitmapsUris { get; private set; }

        /// <summary>
        /// Точка отсчёта (левый верхний угол) клеток матрицы ДРП,
        /// видимых на экране.
        /// </summary>
        private SignedCellPoint _cPointBias; 

        public SignedCellPoint CPointBias
        {
            get { return _cPointBias; }
            set
            {
                _cPointBias = value;
                OnPropertyChanged(nameof(CPointBias));
            }
        }

        /// <summary>
        /// Ассоциируемый трассировщик.
        /// </summary>
        private Tracer _tracer;

        /// <summary>
        /// Ссылка на предыдущие проставленные приоритеты.
        /// Нужна для изменения цвета стрелок приоритетов на чёрный.
        /// </summary>
        private List<CellPoint> _newestSettedPriorities;

        /// <summary>
        /// Проверка uri содержимого или приориетета клетки на null-овость. 
        /// </summary>
        private Regex _rePriorityNotSetted = 
            new Regex(@"\S*NULL$", RegexOptions.Compiled);

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="tracer"></param>
        public BitmapsData(Tracer tracer)
        {
            _tracer = tracer;

            _newestSettedPriorities = new List<CellPoint>();
            bitmapsUris = new ObservableMatrix<string>(FullSize1, FullSize2);

            CPointBias = new SignedCellPoint(-CashedDWSSize2, -CashedDWSSize1);
            InitDWSScreen();

            _tracer.RefreshWireEndPoints += OnRefreshWireEndPoints;
            _tracer.UpdateSettedPriorities += OnUpdateSettedPriorities;
            _tracer.EndWireTracing += OnEndWireTracing;
        }

        /// <summary>
        /// Установка uri иконок клетки, находящейся в видимой
        /// или кэшированной области иконок ДРП, заданным способом.
        /// </summary>
        /// <param name="changingCells">
        /// Отсортированный (!) список клеток ДРП, у которых есть изменения,
        /// которые следует отобразить на экране.</param>
        /// <param name="getUri"></param>
        private void SetUrisWithinScreen(List<CellPoint> changingCellPoints, 
                                         Func<CellPoint, byte, byte, string> getUri)
        {
            byte buX, buY;

            foreach (CellPoint changingCPoint in changingCellPoints
                .SkipWhile(cPoint => cPoint.X < CPointBias.X ||
                                     cPoint.X >= CPointBias.X +
                                              VisibleDWSSize2 + (CashedDWSSize2 << 1))
                .Where(cPoint => cPoint.Y >= CPointBias.Y &&
                                 cPoint.Y < CPointBias.Y +
                                         VisibleDWSSize1 + (CashedDWSSize1 << 1)))
            {
                buX = (byte)(changingCPoint.X - CPointBias.X);
                buY = (byte)(changingCPoint.Y - CPointBias.Y);

                // Останов в случае выхода за пределы видимой и кэшированной областей.
                if (buX >= FullSize2 || buY >= FullSize1)
                    return;

                bitmapsUris[buY, buX] = getUri(changingCPoint, buX, buY);
            }
        }

        /// <summary>
        /// Обновление клеток на концах проводника.
        /// </summary>
        /// <param name="cellStart"></param>
        /// <param name="cellFinsih"></param>
        public void OnRefreshWireEndPoints(CellPoint startCPoint, CellPoint finishCPoint)
        {
            SetUrisWithinScreen(new List<CellPoint> { startCPoint, finishCPoint },
                (CellPoint cPoint, byte _1, byte _2) => GetBitmapUriOfCell(cPoint));
        }

        /// <summary>
        /// Обновление иконок приоритетов на экране ДРП.
        /// Происходит только для тех клеток, которые находятся
        /// внутри экрана ДРП.
        /// </summary>
        public void OnUpdateSettedPriorities(List<CellPoint> newestSettedPriorities)
        {
            /*
                Для не-последних проставленных приоритетов
                меняем иконки на "старые", чёрного цвета.
             */

            SetUrisWithinScreen(_newestSettedPriorities, 
                (CellPoint _1, byte buX, byte buY) =>
                {
                    // Если у клетки не было приоритета, 
                    // то и на старый менять его не нужно.
                    if (bitmapsUris[buY, buX] != null)
                    {
                        var sbBitmapUri = new StringBuilder(bitmapsUris[buY, buX]);
                        // Удаление расширения.
                        sbBitmapUri.Remove(sbBitmapUri.Length - 4, 4);
                        // Добавление пометки о том, что приоритет не новый + расширение.
                        return sbBitmapUri.Append("_old.png").ToString();
                    }
                    else
                        return null;
                });

            _newestSettedPriorities = newestSettedPriorities;
            _newestSettedPriorities.Sort(Tracer.CPC);

            /*
                Для последних проставленных приоритетов
                меняем иконки на актуальные и новые красного цвета.
             */

            SetUrisWithinScreen(_newestSettedPriorities,
                (CellPoint cPoint, byte buX, byte buY) =>
                {
                    var sbBitmapUri = new StringBuilder(bitmapsUris[buY, buX]);
                    // Удаление расширения.
                    sbBitmapUri.Remove(sbBitmapUri.Length - 4, 4);
                    return GetUriOfCellPriority(cPoint, ref sbBitmapUri, false);
                });

            return;
        }

        /// <summary>
        /// Обновление иконок содержания клеток на экране ДРП.
        /// Происходит только для тех клеток, которые находятся
        /// внутри экрана ДРП.
        /// </summary>
        public void OnEndWireTracing(List<CellPoint> wireCells,
                                     List<CellPoint> settedPriorityCells)
        {
            // Для клеток проведённого проводника меняем иконки содержания.

            wireCells.Sort(Tracer.CPC);
            SetUrisWithinScreen(wireCells,
                (CellPoint cPoint, byte _1, byte _2) => GetBitmapUriOfCell(cPoint));

            /*
                Для клеток с проставленным приоритетом
                убираем этот приоритет.
             */

            settedPriorityCells.Sort(Tracer.CPC);
            SetUrisWithinScreen(settedPriorityCells,
                (CellPoint _1, byte buX, byte buY) =>
                {
                    int priorityStartInd = bitmapsUris[buY, buX].IndexOf(';') + 1;
                    return bitmapsUris[buY, buX].Substring(0, priorityStartInd) + "NULL";
                });
        }

        /*
        public void OnCancelTracing(List<Cell> wireCells, 
                                    List<Cell> settedPriorityCells)
        {

            wireCells.Sort(Tracer.CC);
            SetUrisWithinScreen(wireCells,
                (Cell cell, byte _1, byte _2) => GetBitmapUriOfCell(cell));

            SetUrisWithinScreen(settedPriorityCells,
                (Cell cell, byte buX, byte buY) =>
                {
                    int priorityStartInd = bitmapsUris[buY, buX].IndexOf(';') + 1;
                    return bitmapsUris[buY, buX].Substring(0, priorityStartInd) + "NULL";
                });
        }
        */

        /// <summary>
        /// Очистка полного юри клетки от юри приоритета.
        /// </summary>
        /// <param name="fullUri"></param>
        /// <param name="sbBitmapUri"></param>
        /// <returns></returns>
        public string ClearPriorityUri(string fullUri, StringBuilder sbBitmapUri)
        {

            int startOfPriorityUri;
            startOfPriorityUri = fullUri.IndexOf(';') + 1;
            sbBitmapUri.Remove(startOfPriorityUri,
                fullUri.Length - startOfPriorityUri);

            return sbBitmapUri.ToString();
        }

        /// <summary>
        /// Получение юри иконки с содержимым клетки.
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="sbBitmapUri"></param>
        public void GetUriOfCellContent(CellPoint cPoint, ref StringBuilder sbBitmapUri)
        {
            /*
                У проводов с несколькими вариантами направления
                название = "вид направления" + байтовое значение
                направлений провода.
             */

            byte cDirection = (byte)Tracer.DWSMatrix[cPoint.Y, cPoint.X].CDirection;
            switch (Tracer.DWSMatrix[cPoint.Y, cPoint.X].CContent)
            {
                case CellContent.None:
                    { sbBitmapUri.Append("NULL"); break; }
                case CellContent.Contact:
                    { sbBitmapUri.Append("Contact"); break; }
                case CellContent.Start:
                    { sbBitmapUri.Append("CellStart"); break; }
                case CellContent.Finish:
                    { sbBitmapUri.Append("CellFinish"); break; }
                case CellContent.ContactWire:
                    {
                        sbBitmapUri.Append("ContactWire" + cDirection);
                        break;
                    }
                case CellContent.HorizontalWire:
                    { sbBitmapUri.Append("HorizontalWire"); break; }
                case CellContent.VerticalWire:
                    { sbBitmapUri.Append("VerticalWire"); break; }
                case CellContent.CornerWire:
                    {
                        sbBitmapUri.Append("CornerWire" + cDirection);
                        break;
                    }
                case CellContent.THorizontalWire:
                case CellContent.TVerticalWire:
                    {
                        sbBitmapUri.Append("TWire" + cDirection);
                        break;
                    }
                case CellContent.CrossWire:
                    { sbBitmapUri.Append("CrossWire"); break; }
                case CellContent.WireIntersection:
                    { sbBitmapUri.Append("WireIntersection"); break; }
            }
            if (!_rePriorityNotSetted.IsMatch(sbBitmapUri.ToString()))
                sbBitmapUri.Append(".png;");
            else
                sbBitmapUri.Append(";");

            return;
        }

        /// <summary>
        /// Получение юри иконки со стрелкой приоритета клетки.
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="sbBitmapUri"></param>
        /// <param name="maybeOld">
        /// Флаг, означающий запрос на получение актуальной иконки приоритета.
        /// <para>Если флаг установлен в 0, то иконка предоставляется с красной стрелкой
        /// (т.е. подразумевается, что приоритет был проставлен последним).</para>
        /// Если флаг установлен в 1, то иконка подбирается исходя из списка
        /// последних проставленных приоритетов.
        /// </param>
        /// <returns></returns>
        public string GetUriOfCellPriority(CellPoint cPoint,
                                           ref StringBuilder sbBitmapUri,
                                           bool maybeOld)
        {
            switch (Tracer.DWSMatrix[cPoint.Y, cPoint.X].CPriority)
            {
                case CellPriority.NotSetted:
                    { sbBitmapUri.Append("NULL"); break; }
                case CellPriority.ToUp:
                    { sbBitmapUri.Append("ArrowToUp"); break; }
                case CellPriority.ToLeft:
                    { sbBitmapUri.Append("ArrowToLeft"); break; }
                case CellPriority.ToRight:
                    { sbBitmapUri.Append("ArrowToRight"); break; }
                case CellPriority.ToDown:
                    { sbBitmapUri.Append("ArrowToDown"); break; }
            }
            if (!_rePriorityNotSetted.IsMatch(sbBitmapUri.ToString()))
            {
                if (maybeOld && _newestSettedPriorities
                                    .Find(cp => cp == cPoint) == null)
                    sbBitmapUri.Append("_old");
                sbBitmapUri.Append(".png");
            }

            return sbBitmapUri.ToString();
        }

        /// <summary>
        /// Получение полного uri иконки клетки.
        /// </summary>
        /// <param name="cell">
        /// Клетка, для которой требуется получить uri иконок.
        /// </param>
        /// <returns></returns>
        public string GetBitmapUriOfCell(CellPoint cPoint)
        {
            StringBuilder sbBitmapUri = new StringBuilder();
            GetUriOfCellContent(cPoint, ref sbBitmapUri);
            return GetUriOfCellPriority(cPoint, ref sbBitmapUri, true);
        }

        /// <summary>
        /// Начальная загрузка экрана ДРП.
        /// </summary>
        private void InitDWSScreen()
        {
            byte nX, nY = 0;
            for (int i = CashedDWSSize1; i < FullSize1; i++)
            {
                nX = 0;
                for (int j = CashedDWSSize2; j < FullSize2; j++)
                {
                    bitmapsUris[i, j] = GetBitmapUriOfCell(new CellPoint(nX, nY));
                    nX++;
                }
                nY++;
            }

            return;
        }

        /// <summary>
        /// Изменение объекта типа int по ссылке.
        /// </summary>
        /// <param name="j"></param>
        private delegate void ActionRefInt(ref int j);

        /// <summary>
        /// Передвижение видимой области ДРП.
        /// </summary>
        public void MoveScreen(sbyte dX, sbyte dY)
        {
            int biasXdX = CPointBias.X + dX,
                biasYdY = CPointBias.Y + dY,
                dCorrected;
            int iCopyStart, jCopyStart, iStart, iEnd, jStart, jEnd;
            Func<int, bool> iCondCopy, jCondCopy,
                            iCondLoad, jCondLoad;
            ActionRefInt iInc, jInc;

            if (dY >= 0 && biasYdY + CashedDWSSize1 + VisibleDWSSize1 > Tracer.DWSSize1)
            {
                dCorrected = biasYdY + CashedDWSSize1 + VisibleDWSSize1 - Tracer.DWSSize1;
                dY -= (sbyte)dCorrected;
                biasYdY -= dCorrected;
            }
            else if (dY < 0 && biasYdY + CashedDWSSize1 < 0)
            {
                dCorrected = biasYdY + CashedDWSSize1;
                dY -= (sbyte)dCorrected;
                biasYdY -= dCorrected;
            }
            iStart = biasYdY >= 0 ? 0 : -biasYdY;
            iEnd = biasYdY < FullSize1 ? FullSize1 : (FullSize1 << 1) - biasYdY;
            if (dY >= 0)
            {
                // Индекс строки, с которой начинается копирование.
                iCopyStart = iStart;
                // Условие для номера строки, в которой должно происходить
                // копирование uri кэшированных (или видимых) иконок клеток ДРП.
                iCondCopy = (int ii) => ii < iEnd - dY;
                // Условие для номера строки, в которой должно происходить
                // получение uri отсутствующих в матрице uri клеток ДРП.
                iCondLoad = (int ii) => 
                    ii < iEnd && ii + CPointBias.Y < Tracer.DWSSize1;
                // Изменение индекса строки матрицы uri.
                iInc = (ref int ii) => ii++;
            }
            else
            {
                iCopyStart = iEnd - 1;
                iCondCopy = (int ii) => ii >= iStart - dY;
                iCondLoad = (int ii) => ii >= iStart && ii + CPointBias.Y >= 0;
                iInc = (ref int ii) => ii--;
            }

            if (dX >= 0 && biasXdX + CashedDWSSize2 + VisibleDWSSize2 > Tracer.DWSSize2)
            {
                dCorrected = biasXdX + CashedDWSSize2 + VisibleDWSSize2 - Tracer.DWSSize2;
                dX -= (sbyte)dCorrected;
                biasXdX -= dCorrected;
            }
            else if (dX < 0 && biasXdX + CashedDWSSize2 < 0)
            {
                dCorrected = biasXdX + CashedDWSSize2;
                dX -= (sbyte)dCorrected;
                biasXdX -= dCorrected;
            }
            jStart = biasXdX >= 0 ? 0 : -biasXdX;
            jEnd = biasXdX < FullSize2 ? FullSize2 : (FullSize2 << 1) - biasXdX;
            if (dX >= 0)
            {
                jCopyStart = biasXdX >= 0 ? 0 : -biasXdX;
                jCondCopy = (int jj) => jj < jEnd - dX;
                jCondLoad = (int jj) => 
                    jj < jEnd && jj + CPointBias.X < Tracer.DWSSize2;
                jInc = (ref int jj) => jj++;
            }
            else
            {
                jCopyStart = jEnd - 1;
                jCondCopy = (int jj) => jj >= jStart - dX;
                jCondLoad = (int jj) => jj >= jStart && jj + CPointBias.X >= 0;
                jInc = (ref int jj) => jj--;
            }

            CPointBias = new SignedCellPoint(biasXdX, biasYdY);

            for (int i = iCopyStart; iCondLoad(i); iInc(ref i))
            {
                /*
                    Копирование известной части матрицы uri
                    в сдвинутую на (dX; dY) область этой же матрицы.
                 */

                if (iCondCopy(i))
                {
                    int j;
                    for (j = jCopyStart; jCondCopy(j); jInc(ref j))
                        bitmapsUris[i, j] = bitmapsUris[i + dY, j + dX];
                    
                    /*
                        Загрузка uri иконок новых видимых и кешированных
                        клеток ДРП в строке, в которой присутствует копирование,
                        происходит с конца копирования.
                     */

                    LoadNewUri(i, j);
                }

                /*
                    Загрузка uri иконок новых видимых и кешированных
                    клеток ДРП в строке, в которой вообще нет копирования,
                    происходит с нулевого j.
                 */

                else
                    LoadNewUri(i, jCopyStart);
            }

            return;

            /*
                Загрузка uri иконок новых видимых и кешированных
                клеток ДРП.
             */

            void LoadNewUri(int i, int j)
            {
                byte nX, nY;
                for ( ; jCondLoad(j); jInc(ref j))
                {
                    nX = (byte)(CPointBias.X + j);
                    nY = (byte)(CPointBias.Y + i);
                    if (nX >= 0 && nX < Tracer.DWSSize2 &&
                        nY >= 0 && nY < Tracer.DWSSize1)
                        bitmapsUris[i, j] = GetBitmapUriOfCell(new CellPoint(nX, nY));
                    else
                        break;
                }
                return;
            }
        }

        /// <summary>
        /// Точка, которая может иметь отрицательные координаты.
        /// Введение этой структуры необходимо по причине наличия
        /// кэша матрицы иконок.
        /// </summary>
        public struct SignedCellPoint
        {
            public int X { get; private set; }

            public int Y { get; private set; }

            public SignedCellPoint(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        // Уведомление подписчиков на событие изменения свойства.

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
