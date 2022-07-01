using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace TraceWiresClassLib
{
    /// <summary>
    /// Класс, осуществляющий работу по трассировке проводников.
    /// </summary>
    public class Tracer
    {
        /// <summary>
        /// Размерность матрицы ДРП по Y.
        /// </summary>
        private const int _dwsSize1 = 7;

        /// <summary>
        /// Размерность матрицы ДРП по Y.
        /// </summary>
        public static int DWSSize1 { get; private set; } = _dwsSize1;

        /// <summary>
        /// Размерность матрицы ДРП по X.
        /// </summary>
        private const int _dwsSize2 = 7;

        /// <summary>
        /// Размерность матрицы ДРП по X.
        /// </summary>
        public static int DWSSize2 { get; private set; } = _dwsSize2;

        /// <summary>
        /// Матрица ДРП размером _dwsSize1 x _dwsSize2.
        /// </summary>
        public static CellComponent[,] DWSMatrix;

        /// <summary>
        /// Матрицы ДРП, созданная при загрузке файла.
        /// </summary>
        private static CellComponent[,] DWSMatrixOnInitializing;

        /// <summary>
        /// Стек словарей клеток, к которым применяется
        /// изменение содержимого во время каждого этапа трассировки.
        /// </summary>
        public Stack<Dictionary<CellPoint, CellComponent>> CellsWithSettedContentByStep;

        /// <summary>
        /// Стек списков клеток, к которым применяется
        /// изменение приоритета во время этапа трассировки
        /// одного проводника (от точки А до точки Б).
        /// <para>Нужен для визуализации этапа трассировки.</para>
        /// </summary>
        public Stack<List<Cell>> CellsWithSettedPriorityMidStep;

        /// <summary>
        /// Действующие изменения?
        /// </summary>
        static List<Cell> ActualChanges;

        /// <summary>
        /// Очередь словарей клеток, которые берутся в расчёт
        /// при построении проводника вторым приоритетом, 
        /// т.е. клетки, содержащие проводник, но по которым
        /// может быть проведён ещё один кусок провода.
        /// ключ: точка расположения клетки.
        /// значение: область видимости точки в виде массива _точек_.
        /// На момент извлечения словаря клеток из вершины очереди
        /// информация о скопе может стать неактуальной, поэтому 
        /// лучше хранить ссылку на массив точек. 
        /// Также в очереди хранится длина проводника на момент
        /// достижения второочередных клеток.
        /// </summary>
        static Queue<(int, Dictionary<CellPoint, Cell[]>)> SecondPriorityCells;

        /// <summary>
        /// Компаратор расположений клеток.
        /// </summary>
        public static CellComparer CC = new CellComparer();

        /// <summary>
        /// Компаратор расположений клеток.
        /// </summary>
        public static CellPointComparer CPC = new CellPointComparer();

        /// <summary>
        /// Входные данные для этапа трассировки 
        /// (проведения проводника от точки А до точки Б).
        /// </summary>
        static TracingStepInfo currentTracingStepInfo;

        /// <summary>
        /// Обновление конечных точек проводника.
        /// Только визуализация.
        /// </summary>
        public event Action<CellPoint, CellPoint> RefreshWireEndPoints;

        /// <summary>
        /// Событие, оповещающее об окончании одиночного
        /// шага проставления приоритетов.
        /// </summary>
        public event Action<List<CellPoint>> UpdateSettedPriorities;

        public event Action FinishCellCannotBeReached;

        /// <summary>
        /// Событие, оповещающее об окончании трассировки
        /// одиночного проводника.
        /// </summary>
        public event Action<List<CellPoint>, List<CellPoint>> EndWireTracing;

        public enum TracingState
        {
            BeforeStart,
            SettingPriorities,
            TracingWire,
            EndOfTracing,
            CancellingTracedWire,
            Breaked
        }

        /// <summary>
        /// Конструктор.
        /// </summary>
        public Tracer()
        {
            DWSMatrix = new CellComponent[_dwsSize1, _dwsSize2];
            DWSMatrixOnInitializing = new CellComponent[_dwsSize1, _dwsSize2];

            for (int i = 0; i < _dwsSize1; i++)
                for (int j = 0; j < _dwsSize2; j++)
                    DWSMatrix[i, j] = new CellComponent
                        (CellPriority.NotSetted, CellContent.None);

            DWSMatrix[1, 1] = new CellComponent(CellPriority.NotSetted, CellContent.ContactWire, CellContentDirection.ToDown);
            DWSMatrix[2, 0] = new CellComponent(CellPriority.NotSetted, CellContent.ContactWire, CellContentDirection.ToRight);
            DWSMatrix[2, 1] = new CellComponent(CellPriority.NotSetted, CellContent.TVerticalWire, CellContentDirection.UpToLeftToDown);
            DWSMatrix[3, 1] = new CellComponent(CellPriority.NotSetted, CellContent.CornerWire, CellContentDirection.UpToRight);
            DWSMatrix[3, 2] = new CellComponent(CellPriority.NotSetted, CellContent.ContactWire, CellContentDirection.ToLeft);
            DWSMatrix[0, 3] = new CellComponent(CellPriority.NotSetted, CellContent.ContactWire, CellContentDirection.ToRight);
            DWSMatrix[0, 4] = new CellComponent(CellPriority.NotSetted, CellContent.CornerWire, CellContentDirection.DownToLeft);
            DWSMatrix[1, 4] = new CellComponent(CellPriority.NotSetted, CellContent.VerticalWire);
            DWSMatrix[2, 4] = new CellComponent(CellPriority.NotSetted, CellContent.ContactWire, CellContentDirection.UpToDown);
            DWSMatrix[3, 4] = new CellComponent(CellPriority.NotSetted, CellContent.VerticalWire);
            DWSMatrix[4, 4] = new CellComponent(CellPriority.NotSetted, CellContent.TVerticalWire, CellContentDirection.UpToRightToDown);
            DWSMatrix[4, 1] = new CellComponent(CellPriority.NotSetted, CellContent.VerticalWire);

            /*
            DWSMatrix[2, 0] = new CellComponent(CellPriority.FromDown, CellContent.ContactWire);
            DWSMatrix[1, 1] = new CellComponent(CellPriority.NotSetted, CellContent.None);
            */
            DWSMatrix[1, 0] = new CellComponent(CellPriority.NotSetted, CellContent.Contact);
            DWSMatrix[3, 0] = new CellComponent(CellPriority.NotSetted, CellContent.Contact);

            //DWSMatrix[0, 9] = new CellComponent(CellPriority.NotSetted, CellContent.Contact);
            DWSMatrix[5, 0] = new CellComponent(CellPriority.NotSetted, CellContent.HorizontalWire);
            DWSMatrix[5, 1] = new CellComponent(CellPriority.NotSetted, CellContent.CornerWire, CellContentDirection.UpToLeft);

            for (int i = 0; i < _dwsSize1; i++)
                for (int j = 0; j < _dwsSize2; j++)
                    DWSMatrixOnInitializing[i, j] = DWSMatrix[i, j].Clone();

            Wire w1 = new Wire(1);
            //w1.AddCellsRange(new List<CellPoint>
            //{ new CellPoint(3, 0), new CellPoint(4, 0), new CellPoint(4, 1),
            //  new CellPoint(4, 2), new CellPoint(4, 3), new CellPoint(4, 4) });//, new CellPoint(2, 3)});
            currentTracingStepInfo = new TracingStepInfo(
                //new CellPoint(4, 2),
                new CellPoint(0, 1),
                new CellPoint(0, 3),
                w1);

            CellsWithSettedContentByStep = new Stack<Dictionary<CellPoint, CellComponent>>();
            CellsWithSettedPriorityMidStep = new Stack<List<Cell>>();
            SecondPriorityCells = new Queue<(int, Dictionary<CellPoint, Cell[]>)>();

            //TraceWire();
            Console.Read();
        }

        /// <summary>
        /// Получение области видимости клетки на ДРП.
        /// </summary>
        /// <param name="cell">
        /// Клетка, для которой необходимо получить область видимости.
        /// </param>
        /// <returns></returns>
        private Cell[] GetCellScope(Cell сell)
        {
            return GetPointScope(сell.CPoint);
        }

        /// <summary>
        /// Получение области видимости точки ДРП.
        /// </summary>
        /// <param name="cPoint">
        /// Точка, для которой необходимо получить область видимости.
        /// </param>
        /// <returns></returns>
        private Cell[] GetPointScope(CellPoint cPoint)
        {
            byte сellX = cPoint.X,
                 cellY = cPoint.Y;

            Cell[] wscScope = new Cell[4];

            /*
                Порядок расположения клеток в области: верх - лево - право - низ.
             */

            if (cellY > 0)
            {
                wscScope[0] = new Cell(
                    new CellPoint(сellX, (byte)(cellY - 1)),
                    DWSMatrix[cellY - 1, сellX]); ;
            }
            if (сellX > 0)
            {
                wscScope[1] = new Cell(
                    new CellPoint((byte)(сellX - 1), cellY),
                    DWSMatrix[cellY, сellX - 1]);
            }
            if (сellX < _dwsSize2 - 1)
            {
                wscScope[2] = new Cell(
                    new CellPoint((byte)(сellX + 1), cellY),
                    DWSMatrix[cellY, сellX + 1]);
            }
            if (cellY < _dwsSize1 - 1)
            {
                wscScope[3] = new Cell(
                    new CellPoint(сellX, (byte)(cellY + 1)),
                    DWSMatrix[cellY + 1, сellX]);
            }

            return wscScope;
        }

        /// <summary>
        /// Правило прохода из клетки в центре до клетки 
        /// в области этой клетки с указанным индексом.
        /// <para>В прямом режиме клетка cellWithinScope
        /// содержит приоритет, ведущий из центра области.</para>
        /// В обратном режиме клетка cellWithinScope
        /// содержит приоритет, ведущий в центр области.
        /// </summary>
        /// <param name="centerCellScope"></param>
        /// <param name="cellWithinScope"></param>
        /// <param name="mode">Режим работы правила: 0 - прямой, 1 - обратный. </param>
        /// <returns></returns>
        private bool GoThroughRule(Cell[] centerCellScope, Cell cellWithinScope, int mode)
        {
            int ind = -1;
            while (cellWithinScope.CPoint != centerCellScope[++ind]?.CPoint)
                ;

            /*
                Возвращает признак равенства местоположения клетки
                в области и её приоритета.
                0 - вверх, 1 - влево, 2 - вправо, 3 - вниз.
             */

            switch(mode)
            {
                case 0: default:
                    return (sbyte)centerCellScope[ind].CComponent.CPriority == 3 - ind;
                case 1:
                    return (sbyte)centerCellScope[ind].CComponent.CPriority == ind;
            }
        }

        /// <summary>
        /// Проведение проводника.
        /// </summary>
        public IEnumerator<TracingState> TraceWire()
        {
            CellPoint startCPoint = currentTracingStepInfo.StartCellPoint,
                // Стартовая клетка этапа.
                      finishCPoint = currentTracingStepInfo.FinishCellPoint,
                // Конечная клетка этапа.
                      waveStoppedPoint = finishCPoint;
            Wire actualWire = currentTracingStepInfo.ActualWire;
            Dictionary<CellPoint, Cell[]> waveStartPoints =
                new Dictionary<CellPoint, Cell[]>(),
                // Список источников волны и их скопов.
                                          nearestFreeCells =
                new Dictionary<CellPoint, Cell[]>(),
                // Ближайшие свободные клетки.
                                          nearestHalfFreeCells =
                new Dictionary<CellPoint, Cell[]>(),
                // Ближайшие полусвободные клетки.
                                          nfpOrigin;
                // Ссылка на список клетки (полу- или не полусвободных).
            Dictionary<CellPoint, List<Cell>> 
                nearestFreeCellsByWSCS =  
                new Dictionary<CellPoint, List<Cell>>();
                // Словарь свободных клеток по источникам волны.
            Cell[] nfcScope, wscScope;
            List<Cell> withPriorities;
            int[] conflictPrioritiesIndsInOrder;
            byte[] bPriorityMask = new byte[4]
                { 0b0001, 0b0010, 0b0100, 0b1000 };
                // Сверка установленного приоритета и направления провода. 
            int wireLen = 0, intersectionsCount = 0;
            bool actualWireContainsStartCell = 
                    actualWire.ContainsCellPoint(startCPoint),
                 actualWireContainsFinishCell =
                    actualWire.ContainsCellPoint(finishCPoint);
            Func<bool> endOfWaveRadiationPred = null;

            /*
                Флаги окончания цикла испускания волны приоритетов:
                1) достигнута ли клетка прокладываемого проводника
                   (который состоит из >=1 провода от точки А до точки Б),
                   которая способна вместить ещё один вход, если 
                   конечная клетка входит в прокладываемый надпроводник;
                2) достигнута ли конечная клетка, в противном случае.
                Если в прокладываемый надпроводник входит начальная клетка,
                то в список источников волны помещаются все клетки,
                на которых располагается проводник.
                Если конечная клетка входит в проводник, то клеткой, от которой
                начинается проведение непосредственно провода от А до Б становится
                та клетка Ц, на которой остановилось излучение волны.
             */

            if (actualWireContainsFinishCell)
            {
                endOfWaveRadiationPred = () =>
                {
                    bool actualWireHasBeenReached = false;
                    foreach (CellPoint nhfp in nearestHalfFreeCells.Keys)
                    {
                        actualWireHasBeenReached |=
                            actualWire.ContainsCellPoint(nhfp);
                        if (actualWireHasBeenReached)
                        {
                            waveStoppedPoint = nhfp;
                            break;
                        }
                    }
                    return actualWireHasBeenReached;
                };
            }
            else 
            {
                endOfWaveRadiationPred = () =>
                {
                    return nearestFreeCells.ContainsKey(finishCPoint);
                };
                if (actualWireContainsStartCell)
                {
                    foreach(CellPoint occupiedPoint in actualWire.OccupiedCells.Keys)
                        nearestFreeCellsByWSCS.Add(occupiedPoint, null);
                }
            }
            if (!actualWireContainsStartCell)
                nearestFreeCellsByWSCS.Add(startCPoint, null);

            nearestFreeCells =
                (
                    from nfcByWSCs in nearestFreeCellsByWSCS
                    let nfp = nfcByWSCs.Key
                    let cContent = (sbyte)DWSMatrix[nfp.Y, nfp.X].CContent
                    where cContent < 2 || cContent == 3 ||
                          (actualWireContainsStartCell && 
                           currentTracingStepInfo.ActualWire.ContainsCellPoint(nfp))
                    select new
                    {
                        NFP = nfp,
                        NFCScope = GetPointScope(nfp)
                    }
                )
                .ToDictionary(nfcKVP => nfcKVP.NFP,
                              nfcKVP => nfcKVP.NFCScope);

            DWSMatrix[startCPoint.Y, startCPoint.X].SetStart();
            DWSMatrix[finishCPoint.Y, finishCPoint.X].SetFinish();

            RefreshWireEndPoints(startCPoint, finishCPoint);

            TracingState stateDuringWaveRadiation;
            do
            {
                // Останов конечного автомата: шаг простановки приоритетов выполнен.

                stateDuringWaveRadiation = WaveRadiation();
                yield return stateDuringWaveRadiation;
                if (stateDuringWaveRadiation == TracingState.Breaked)
                    yield break;
            }
            while (!endOfWaveRadiationPred());
            
            Cell currentCell = new Cell(waveStoppedPoint, 
                    DWSMatrix[waveStoppedPoint.Y, waveStoppedPoint.X]), 
                 nextCell = null;
            CellPoint tracingEndCellPoint = startCPoint;
                // Конечная для трассировки точка.
            Cell[] ccpScope = GetPointScope(currentCell.CPoint), 
                   ncpScope = null;
            Func<bool> endOfTracingPred = null;
            Dictionary<CellPoint, CellComponent> additionToWire = 
                new Dictionary<CellPoint, CellComponent>();

            /*
                Флаги окончания трассировки:
                1) достигнута ли клетка провода, если от начальной точки
                   уже был проведён какой-либо проводник;
                2) достигнута ли начальная клетка в противном случае.
             */

            if (actualWireContainsStartCell)
            {
                endOfTracingPred = () =>
                    {
                        bool actualWireHasBeenReached =
                            actualWire.ContainsCellPoint(currentCell.CPoint);

                        if (actualWireHasBeenReached)
                            tracingEndCellPoint = ccpScope
                                .Where(c => c != null)
                                .Select(c => c.CPoint)
                                .Intersect(actualWire.OccupiedCells.Keys).ToArray()[0];

                        return actualWireHasBeenReached;
                    };
            }
            else
            {
                endOfTracingPred = () => currentCell.CPoint == tracingEndCellPoint;
                /*ccpScope
                                .Where(c => c != null)
                                .Select(c => c?.CPoint)
                                .Contains(startCellPoint);*/
            }
            
            do
            {
                additionToWire.Add(currentCell.CPoint, currentCell.CComponent);
                foreach (Cell cellWithinScope in ccpScope)
                {
                    if (cellWithinScope == null)
                        continue;
                    ncpScope = GetCellScope(cellWithinScope);
                    if (GoThroughRule(ncpScope, currentCell, 1))
                    {
                        nextCell = cellWithinScope;
                        break;
                    }
					//else
					//	cellWithinScope.CComponent.CPriority = 
					//		CellPriority.NotSetted;
                }
                AddSnapPoints();

                // Останов конечного автомата.

                yield return TracingState.TracingWire;
            }
            while (!endOfTracingPred());
            additionToWire.Add(currentCell.CPoint, currentCell.CComponent);
            ApproveTracingWire(additionToWire);
            //nextCell = new Cell(tracingEndCellPoint, 
            //    DWSMatrix[tracingEndCellPoint.Y, tracingEndCellPoint.X]);
            //ncpScope = GetCellScope(nextCell);
            //AddSnapPoints();
            //additionToWire.Add(currentCell);
            //ApproveTracingWire(additionToWire);
            //*/

            yield return TracingState.EndOfTracing;
            yield return TracingState.CancellingTracedWire;
            yield break;

            TracingState WaveRadiation()
            {
                /*
                    В список источников волны входят:
                    -- либо ближайшие пустые клетки с предыдущего этапа, 
                       если таковые имеются;
                    -- либо ближайшие клетки, по которым можно провести
                       _заданный_ проводник с предыдущих этапов
                       (в их число входят клетки с вертикальным/горизонтальным
                        проводом, а также т-провод, если относится к тому же
                        проводнику).
                 */

                waveStartPoints.Clear();

                if (nearestFreeCells.Count == 0)
                {
                    if (SecondPriorityCells.Count > 0)
                    {
                        /*
                            Из очереди второприоритетных полусвободных клеток
                            выбираются те, которым не был проставлен приоритет
                            в предыдущие выборки полусвободных клеток, т.к.
                            может случится такое, что полусвободная клетка
                            встретится в области видимости источников волны
                            из разных списков.
                         */

                        intersectionsCount++;
                        (wireLen, nearestHalfFreeCells) = SecondPriorityCells.Dequeue();
                        foreach (CellPoint nhfc in nearestHalfFreeCells.Keys)
                            waveStartPoints.Add(nhfc, GetPointScope(nhfc));
                    }
                    else
                    {
                        // !!!! Вывод невозможности проложить путь.
                        FinishCellCannotBeReached?.Invoke();
                        return TracingState.Breaked;
                    }
                }
                else
                {
                    wireLen++; // Длина проводника увеличивается на 1.

                    /*
                        Из предыдущего списка свободных клеток выбираются 
                        все клетки.
                     */

                    foreach (KeyValuePair<CellPoint, Cell[]> nfcKVP in nearestFreeCells)
                        waveStartPoints.Add(nfcKVP.Key, nfcKVP.Value);
                }

                /*
                    Собираем словарь:
                        -- ключ: координаты свободной клетки;
                        -- значение: список клеток с приоритетами
                           после предыдущего этапа, в области видимости
                           которых присутствует свободная клетка-ключ.
                           Список необходим для выявления конфликтов
                           приоритетов.
                 */

                nearestFreeCellsByWSCS.Clear();
                nearestFreeCellsByWSCS =
                    (
                        from wsp in waveStartPoints.Keys
                        let scope = GetPointScope(wsp)
                        from nearestCell in scope
                        where nearestCell != null
                        let ncPoint = nearestCell.CPoint
                        let ncComp = DWSMatrix[ncPoint.Y, ncPoint.X]
                        where ncPoint == finishCPoint ||
                              CheckFreeCellConditions(ncPoint, ncComp)
                        group new Cell(wsp, DWSMatrix[wsp.Y, wsp.X])
                        by ncPoint
                        into nfcByWSCs
                        select nfcByWSCs
                    )
                    .ToDictionary(nfcByWSCs => nfcByWSCs.Key,
                                  nfcByWSCs => nfcByWSCs.ToList());

                /*
                    Получение списка свободных на данном этапе клеток
                    в области видимости текущих источников волны.
                 */

                nearestFreeCells.Clear();
                nearestFreeCells =
                    (
                        from nfcByWSCs in nearestFreeCellsByWSCS
                        let nfp = nfcByWSCs.Key
                        where (sbyte)DWSMatrix[nfp.Y, nfp.X].CContent < 2
                        select new
                        {
                            NFP = nfp,
                            NFCScope = GetPointScope(nfp)
                        }
                    )
                    .ToDictionary(nfcKVP => nfcKVP.NFP,
                                  nfcKVP => nfcKVP.NFCScope);

                /*
                    Получение списка полусвободных (с проводником) на данном
                    этапе клеток в области видимости текущих источников волны.
                 */

                nearestHalfFreeCells.Clear();
                nearestHalfFreeCells =
                    (
                        from nfcByWSCs in nearestFreeCellsByWSCS
                        let nfp = nfcByWSCs.Key
                        where (sbyte)DWSMatrix[nfp.Y, nfp.X].CContent >= 2
                        select new
                        {
                            NFP = nfp,
                            NFCScope = GetPointScope(nfp)
                        }
                    )
                    .ToDictionary(nfcKVP => nfcKVP.NFP,
                                  nfcKVP => nfcKVP.NFCScope);

                foreach (CellPoint nfp in nearestFreeCellsByWSCS.Keys)
                {
                    Cell nearestFreeCell = new Cell(nfp, DWSMatrix[nfp.Y, nfp.X]);
                    nfpOrigin = nearestFreeCells.ContainsKey(nfp) ?
                        nearestFreeCells :
                        nearestHalfFreeCells;
                    nfcScope = nfpOrigin[nfp];
                    withPriorities = nearestFreeCellsByWSCS[nfp];

                    switch (withPriorities.Count)
                    {
                        /*
                            Если в области обсуждаемой клетки присутствует только
                            1 клетка с приоритетом, то обсуждаемой клетке 
                            назначнается приоритет-стрелка, который ведёт в клетку
                            с приоритетом.
                         */

                        case 1:
                            {
                                SetFreeCellPriority(withPriorities[0]);

                                break;
                            }
                        case 2:
                        case 3:
                            {
                                /*
                                conflictPrioritiesIndsInOrder = new int[2]
                                    {
                                        currentTracingStepInfo
                                            .PrioritiesOrder
                                            .IndexOf(withPriorities[0]
                                                .CComponent.CPriority),
                                        currentTracingStepInfo
                                            .PrioritiesOrder
                                            .IndexOf(withPriorities[1]
                                                .CComponent.CPriority)
                                    };
                                    */

                                conflictPrioritiesIndsInOrder = new int[3];
                                for (int i = 0; i < withPriorities.Count; i++)
                                {
                                    conflictPrioritiesIndsInOrder[i] =
                                        currentTracingStepInfo
                                              .PrioritiesOrder
                                              .IndexOf(withPriorities[i]
                                                  .CComponent.CPriority);
                                }
                                
                                if (withPriorities.Count == 2)
                                    conflictPrioritiesIndsInOrder[2] = int.MaxValue;
                                    
                                int maxPriority = conflictPrioritiesIndsInOrder.Min();
                                // Смотрим клетки с одинаковым приоритетом (максимум 2).
                                int[] conflictPrioritiesClones = conflictPrioritiesIndsInOrder
                                        .IndexesOf(maxPriority)
                                        .ToArray();

                                Cell maxPriorityWSC;
                                // Если клонов нет, то просто выбираем 
                                // источник волны с максимальным приоритетом.
                                if (conflictPrioritiesClones.Length == 1)
                                /*
                                    Получение клетки с максимальным приоритетом
                                    (чем ближе к нулю - тем больше приоритет)
                                    из списка конфликтующих источников волны.
                                 */
                                {
                                    maxPriorityWSC = withPriorities[
                                      conflictPrioritiesIndsInOrder.IndexOf(
                                          conflictPrioritiesIndsInOrder.Min())];
                                }
                                // Иначе среди клонов находим клетку, ближайшую к финишу.
                                else
                                {
                                    Cell[] withPrioritiesClones = new Cell[2]
                                        { withPriorities[conflictPrioritiesClones[0]],
                                      withPriorities[conflictPrioritiesClones[1]]};

                                    // Находим ближайшую к финишу клетку.
                                    byte[] dXsToFC =
                                        {
                                            (byte)Math.Abs(finishCPoint.X - withPrioritiesClones[0].CPoint.X),
                                            (byte)Math.Abs(finishCPoint.X - withPrioritiesClones[1].CPoint.X)
                                        },
                                          dYsToFC =
                                        {
                                            (byte)Math.Abs(finishCPoint.X - withPrioritiesClones[0].CPoint.X),
                                            (byte)Math.Abs(finishCPoint.X - withPrioritiesClones[1].CPoint.X)
                                        };
                                    byte nearestToFinishInd;
                                    if (dXsToFC[0] != dXsToFC[1])
                                    {
                                        if (dXsToFC[0] < dXsToFC[1])
                                            nearestToFinishInd = 0;
                                        else
                                            nearestToFinishInd = 1;
                                    }
                                    else
                                    {
                                        if (dYsToFC[0] < dYsToFC[1])
                                            nearestToFinishInd = 0;
                                        else
                                            nearestToFinishInd = 1;
                                    }
                                    maxPriorityWSC = withPrioritiesClones[nearestToFinishInd];
                                }

                                SetFreeCellPriority(maxPriorityWSC);

                                break;
                            }
                            /*
                        case 3:
                            {
                                conflictPrioritiesIndsInOrder = new int[3];
                                for (int i = 0; i < withPriorities.Count; i++)
                                {
                                    conflictPrioritiesIndsInOrder[i] =
                                        currentTracingStepInfo
                                              .PrioritiesOrder
                                              .IndexOf(withPriorities[i]
                                                  .CComponent.CPriority);
                                }

                                // Смотрим клетки с одинаковым приоритетом (их всего 2).
                                int[] conflictPrioritiesClones = conflictPrioritiesIndsInOrder
                                        .IndexesOf(conflictPrioritiesIndsInOrder.Min())
                                        .ToArray();

                                // Проставляем для неё приоритет.
                                SetFreeCellPriority(nearestToFinish);

                                break;
                            }*/
                    }

                    continue;

                    /*
                        Установка приоритета в клетке.
                     */
                    void SetFreeCellPriority(Cell wsc)
                    {
                        /*
                            Если из свободной клетки в источник волны
                            можно провести стрелку, как у источника волны,
                            то она дублируется в свободную клетку.
                         */
                        if (GoThroughRule(nfcScope, wsc, 0))
                            nearestFreeCell.CComponent.CPriority =
                                wsc.CComponent.CPriority;

                        /*
                            В противном случае происходит перебор значения
                            приоритета в текущей пустой клетке таким образом,
                            чтобы он вёл в клетку-источник волны, в области
                            видимости которого находится эта пустая клетка.
                         */

                        else
                        {
                            nearestFreeCell.CComponent.CPriority = 0;
                            wscScope = waveStartPoints[wsc.CPoint];

                            while (!GoThroughRule(wscScope, nearestFreeCell, 1))
                                nearestFreeCell.CComponent.CPriority++;
                        }

                        // Проверка на допустимость проведения подходящего 
                        // по приоритету провода.
                        CellContent cContent = nearestFreeCell.CComponent.CContent;
                        byte bDirection = (byte)nearestFreeCell.CComponent.CDirection,
                             bPriority = (byte)nearestFreeCell.CComponent.CPriority;
                        /*
                            Если провод нельзая провести, то приоритет удаляется,
                            клетка из своего списка удаляется.
                         */
                        if ((bPriorityMask[bPriority] & bDirection) != 0)
                        {
                            nearestFreeCell.CComponent.CPriority = CellPriority.NotSetted;
                            nfpOrigin.Remove(nfp);
                        }
                    }
                }

                MakeChangesMidStep(nearestFreeCellsByWSCS.Keys
                    .Select(nfp => new Cell(nfp, DWSMatrix[nfp.Y, nfp.X])).ToList());

                // Сохраняем список второочередных клеток вместе с текущей длиной провода.

                if (nearestHalfFreeCells.Count > 0)
                    SecondPriorityCells.Enqueue((wireLen, nearestHalfFreeCells.Copy()));

                return TracingState.SettingPriorities;
            }


            void AddSnapPoints()
            {
                currentCell.CComponent.AddSnapPoint(nextCell, ccpScope,
                    currentCell.CPoint != waveStoppedPoint &&
                    (sbyte)currentCell.CComponent.CContent > 2);
                //ncpScope = GetCellScope(nextCell);
                nextCell.CComponent.AddSnapPoint(currentCell, ncpScope,
                    nextCell.CPoint != tracingEndCellPoint &&
                    (sbyte)nextCell.CComponent.CContent > 2);
                // Визуализировать!
                currentCell.CComponent.CPriority = CellPriority.NotSetted;
                currentCell = nextCell;
                ccpScope = ncpScope;
            }

            /// <summary>
            /// Проверка клетки на предмет того, стоит ли её рассматривать
            /// в качестве элемента пути от клетки А до клетки Б.
            /// </summary>
            /// <param name="wsc">Клетка, данные которые следует проверить.</param>
            /// <returns>Возврат: флаг, указывающий на свободность клетки.</returns>
            bool CheckFreeCellConditions(CellPoint wsp, CellComponent wscComp)
            {
                sbyte sbCContent = (sbyte)wscComp.CContent;
                return wscComp.CPriority == CellPriority.NotSetted &&
                    (
                     (
                      (
                       (sbCContent & 0b00000001) == 0 && sbCContent < 5
                      ) && 
                        !currentTracingStepInfo.ActualWire.ContainsCellPoint(wsp)
                     ) ||
                    (
                     (
                      (sbCContent & 0b00000001) == 0 || sbCContent == 3 || sbCContent == 5
                     ) &&
                        !actualWireContainsStartCell && actualWireContainsFinishCell &&
                        currentTracingStepInfo.ActualWire.ContainsCellPoint(wsp)
                    )
                   );
            }
        }

        /// <summary>
        /// Создаёт актуальную матрицу ДРП на основе
        /// начальной матрицы и таблицы поэтапных изменений.
        /// </summary>
        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!! закомментировал блок со стеком словарей клеток
        // Не нужно.
        private void CreateChanges()
        {
            /*
                Составляем словарь списков изменений.
                -- ключ: глубина в стеке (актуальность списка изменений);
                -- значение:
                    -- ключ: индекс текущего элемента в списке изменений
                    (необходим для того, чтобы не возиться с копированием
                    списков из стека);
                    -- значение: сам список изменений.
             */

            var changesByDepth = new Dictionary<int, KeyValueTuple<int, List<Cell>>>();
            for (int i = 0; i < CellsWithSettedContentByStep.Count;)
            {
                //changesByDepth.Add(++i, new KeyValueTuple<int, List<Cell>>
                //                        (0, CellsWithSettedContentByStep.ElementAt(i)));
            }

            int currentIndex; // Индекс текущего элемента в списке изменений.
            Cell currentCell; // Клетка, помещаемая в конец актуального списка изменений.
            int currentDepth; // глубина в стеке списка, из которого берётся currentCell.

            int iIndex;
            Cell iCell;

            /*
                Цикл длится, пока не будут обработаны все списки изменений.
             */

            while (changesByDepth.Count > 0)
            {
                // Инициализация стартовых значений.

                currentDepth = changesByDepth.Keys.First();
                currentIndex = changesByDepth[currentDepth].Key;
                currentCell = changesByDepth[currentDepth].Value[currentIndex];

                if (changesByDepth.Count > 1)
                {
                    foreach (int deeper in changesByDepth.Keys.Skip(1))
                    {
                        iIndex = changesByDepth[deeper].Key;
                        iCell = changesByDepth[deeper].Value[iIndex];

                        /*
                            Сравниваем расположение клеток.
                            Если оно одинаковое, то в списке с большей глубиной
                            текущий индекс увеличиваем на 1 (поскольку неактуальная 
                            информация нам не нужна).
                            Если найдена клетка с "меньшим" местоположением,
                            то приравниваем к ней "минимальную" клетку.
                         */

                        switch (CC.Compare(currentCell, iCell))
                        {
                            case -1: break;
                            case 0:
                                {
                                    int greaterDepth =
                                        deeper > currentDepth ? deeper : currentDepth;
                                    changesByDepth[greaterDepth].Key++;
                                    break;
                                }
                            case 1:
                                {
                                    currentCell = iCell;
                                    currentDepth = deeper;
                                    break;
                                }
                        }
                    }
                }

                /*
                    К текущему индексу в списке изменений, из которого взяли
                    клетку, добавляем единицу (чтобы перейти на следующий элемент списка),
                    Если весь список изменений, совершённых на этапе currentDepth,
                    был пройден, то удаляем его из словаря.
                 */

                changesByDepth[currentDepth].Key++;
                if (changesByDepth[currentDepth].Key ==
                        changesByDepth[currentDepth].Value.Count)
                    changesByDepth.Remove(currentDepth);
            }

        }

        /// <summary>
        /// Создаёт актуальную матрицу ДРП с проставленными приоритетами
        /// для ближайших к текущим источникам волны свободных клеток.
        /// </summary>

        private void MakeChangesMidStep(List<Cell> mutatingCells)
        {
            CellsWithSettedPriorityMidStep.Push(mutatingCells);

            foreach (Cell mutatingCell in mutatingCells)
                DWSMatrix[mutatingCell.CPoint.Y, mutatingCell.CPoint.X] =
                    mutatingCell.CComponent.Clone();

            /*
                Оповещение внешних подписчиков о том, что 
                приоритеты клеток в ДРП были обновлены.
             */

            UpdateSettedPriorities?.Invoke(mutatingCells.Select(mc => mc.CPoint).ToList());
        }

        // Версия для бт
        /// <summary>
        /// Создаёт актуальную матрицу ДРП с проставленными приоритетами
        /// для ближайших к текущим источникам волны свободных клеток.
        /// </summary>

        private void MakeChangesMidStepBT(BinaryTree<Cell> mutatingCells)
        {
            //CellsWithSettedPriorityMidStep.Push(mutatingCells);

            foreach (Cell mutatingCell in mutatingCells)
                DWSMatrix[mutatingCell.CPoint.Y, mutatingCell.CPoint.X] =
                    mutatingCell.CComponent.Clone();
        }

        /// <summary>
        /// Подтверждает изменение содержимого клеток,
        /// по которым будет проведён проводник на текущем этапе,
        /// и добавляет их в список изменений.
        /// </summary>
        private void ApproveTracingWire(Dictionary<CellPoint, CellComponent> mutatingCells)
        {
            CellsWithSettedContentByStep.Push(mutatingCells);

            foreach (CellPoint mutatingCPoint in mutatingCells.Keys)
                DWSMatrix[mutatingCPoint.Y, mutatingCPoint.X] =
                    mutatingCells[mutatingCPoint].Clone();

            List<Cell> settedPriorityCells = CellsWithSettedPriorityMidStep
                                                .FlattenEnumerable();

            foreach (Cell settedPriorityCell in settedPriorityCells)
                DWSMatrix[settedPriorityCell.CPoint.Y, 
                          settedPriorityCell.CPoint.X].CPriority =
                    CellPriority.NotSetted;

            EndWireTracing?.Invoke(mutatingCells.Keys.ToList(), 
                                   settedPriorityCells.Select(spc => spc.CPoint).ToList());
        }

        /// <summary>
        /// Отменяет изменения в матрице ДРП, совершённые на текущем этапе.
        /// Завершающий этап после того, как будет произведена обработка
        /// всех возможных вариантов трассировки с проводником на текущем этапе.
        /// </summary>
        public (List<CellPoint>, List<CellPoint>) UndoChanges()
        {
            /*
                Отмена изменений может происходить двумя путями.
                Вариант первый: проводник был проведён первым, до этого
                ДРП представляло собой чистый лист. В этом случае
                изменения просто откатываются.
                Вариант второй: до этого проводника были проведены другие,
                следовательно изменения производятся с оглядкой на состояние
                ДРП до проведения текущего проводника.
                Изменения откатываются на один этап для изменений содержимого
                и полностью - для списка клеток с приоритетами.
                Сначала производится откат клеток проводника, 
                затем - клеток с проставленным приоритетом.
             */

            Dictionary<CellPoint, CellComponent> cellsWithContent =
                CellsWithSettedContentByStep.Pop();
            List<Cell> cellsWithPriority,
                       cellsWithPriorityFlatten = new List<Cell>();

            if (CellsWithSettedContentByStep.Count == 0)
            {
                foreach (CellPoint tenetCPoint in cellsWithContent.Keys)
                {
                    DWSMatrix[tenetCPoint.Y, tenetCPoint.X] =
                        DWSMatrixOnInitializing[tenetCPoint.Y,
                                                tenetCPoint.X].Clone();
                }
            }
            else
            {
                foreach (CellPoint tenetCPoint in cellsWithContent.Keys)
                {
                    foreach (Dictionary<CellPoint, CellComponent> prevChanges 
                                 in CellsWithSettedContentByStep)
                    {
                        if (prevChanges.ContainsKey(tenetCPoint))
                        {
                            DWSMatrix[tenetCPoint.Y, tenetCPoint.X] =
                                prevChanges[tenetCPoint].Clone();
                            break;
                        }
                    }
                }
            }
            
            while (CellsWithSettedPriorityMidStep.Count > 0)
            {
                cellsWithPriority = CellsWithSettedPriorityMidStep.Pop();
                foreach (Cell tenetCell in cellsWithPriority)
                {
                    DWSMatrix[tenetCell.CPoint.Y, tenetCell.CPoint.X]
                        .CPriority = CellPriority.NotSetted;
                }
                cellsWithPriorityFlatten.AddRange(cellsWithPriority);
            }

            return (cellsWithContent.Keys.ToList(), 
                    cellsWithPriorityFlatten.Select(cwp => cwp.CPoint).ToList());
        }

        // Версия для двоичного дерева.
        /// <summary>
        /// Отменяет изменения в матрице ДРП, совершённые на текущем этапе.
        /// Завершающий этап после того, как будет произведена обработка
        /// всех возможных вариантов трассировки с проводником на текущем этапе.
        /// </summary>
        private void UndoChangesBT()
        {
            /*
                Отмена изменений может происходить двумя путями.
                Вариант первый: проводник был проведён первым, до этого
                ДРП представляло собой чистый лист. В этом случае
                изменения просто откатываются.
                Вариант второй: до этого проводника были проведены другие,
                следовательно изменения производятся с оглядкой на состояние
                ДРП до проведения текущего проводника.
                Изменения откатываются на один этап для изменений содержимого
                и полностью - для списка клеток с приоритетами.
                Сначала производится откат клеток проводника, 
                затем - клеток с проставленным приоритетом.
             */

            BinaryTree<Cell> cellsWithContent = null,//CellsWithSettedContentByStep.Pop(),
                             cellsWithPriority = null;
            Cell prevActualCell;

            if (CellsWithSettedContentByStep.Count == 1)
            {
                foreach (Cell tenetCell in cellsWithContent)
                {
                    DWSMatrix[tenetCell.CPoint.Y, tenetCell.CPoint.X] =
                        DWSMatrixOnInitializing[tenetCell.CPoint.Y,
                                                tenetCell.CPoint.X];
                }
            }
            else
            {
                BinaryTree<Cell> prevChanges = null;//CellsWithSettedContentByStep.Peek();

                foreach (Cell tenetCell in cellsWithContent)
                {
                    if ((prevActualCell = prevChanges.FindEqual(tenetCell)) != null)
                        DWSMatrix[tenetCell.CPoint.Y, tenetCell.CPoint.X] =
                            prevActualCell.CComponent.Clone();
                    else
                        DWSMatrix[tenetCell.CPoint.Y, tenetCell.CPoint.X] =
                            DWSMatrixOnInitializing[tenetCell.CPoint.Y,
                                                    tenetCell.CPoint.X];
                }
            }

            while ((cellsWithPriority == null))//CellsWithSettedPriorityMidStep.Pop()) != null)
                foreach (Cell tenetCell in cellsWithPriority)
                {
                    DWSMatrix[tenetCell.CPoint.Y, tenetCell.CPoint.X]
                        .CPriority = CellPriority.NotSetted;
                }
        }

        /*
        private void AnalyzeCellContentImage(Cell cell)
        {
            string uri;

            Cell[] cellScopePoints = GetCellScope(cell);
            CellContent?[] cellScopeContent =
                (
                    from Cell nearest in GetCellScope(cell)
                    select nearest?.CComponent.CContent
                ).ToArray();
            sbyte c0 = currentTracingStepInfo.ActualWire.OccupiedCells
                           .ContainsKey(cellScopePoints[0]) ? 
                           (sbyte)cellScopeContent[0] : (sbyte)CellContent.Ignore,
                  c1 = currentTracingStepInfo.ActualWire.OccupiedCells
                           .ContainsKey(cellScopePoints[1]) ?
                           (sbyte)cellScopeContent[1] : (sbyte)CellContent.Ignore,
                  c2 = currentTracingStepInfo.ActualWire.OccupiedCells
                           .ContainsKey(cellScopePoints[2]) ?
                           (sbyte)cellScopeContent[2] : (sbyte)CellContent.Ignore,
                  c3 = currentTracingStepInfo.ActualWire.OccupiedCells
                           .ContainsKey(cellScopePoints[3]) ?
                           (sbyte)cellScopeContent[3] : (sbyte)CellContent.Ignore;

            bool[] snapDirections = new bool[4];
            switch (cell.CComponent.CContent)
            {
                case CellContent.ContactWire:
                    {
                        switch (cellScopeContent)
                        {
                            case CellContent?[] c when
                                c0 > 2 && c0 != 7:
                                { uri = "toUp"; break; }
                            case CellContent?[] c when
                                c1 == 5 || (c1 % 2 == 0 && c1 > 0):
                                { uri = "toLeft"; break; }
                            case CellContent?[] c when
                                c2 == 5 || (c2 % 2 == 2 && c0 > 0):
                                { uri = "toRight"; break; }
                            case CellContent?[] c when
                                c3 == 5 || (c3 % 2 == 3 && c0 > 0):
                                { uri = "toDown"; break; }
                        }

                        break;
                    }
            }

            switch (cell.CComponent.CContent)
            {
                case CellContent.ContactWire:
                    {
                        switch(cellScopeContent)
                        {
                            case CellContent?[] c when 
                                c0 > 2 && c0 != 7 :
                                { uri = "toUp"; break; }
                            case CellContent?[] c when
                                c1 == 5 || (c1 % 2 == 0 && c1 > 0):
                                { uri = "toLeft"; break; }
                            case CellContent?[] c when
                                c2 == 5 || (c2 % 2 == 2 && c0 > 0):
                                { uri = "toRight"; break; }
                            case CellContent?[] c when
                                c3 == 5 || (c3 % 2 == 3 && c0 > 0):
                                { uri = "toDown"; break; }
                        }

                        break;
                    }
            }
        }
        */
    }
}
