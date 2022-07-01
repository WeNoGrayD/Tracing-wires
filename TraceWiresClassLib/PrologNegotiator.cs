using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SbsSW.SwiPlCs;
using System.IO;

namespace TraceWiresClassLib
{
    public class PrologNegotiator
    {
        /*
        static int DWSSize1 = 10;
        static int DWSSize2 = 10;

        static CellComponent[,] DWSMatrix;

        static CellComponent[,] DWSMatrixOnInitializing;

        delegate bool PlTermDelegate(PlTermV args);

        static Stack<List<Cell>> ChangesBySteps;

        static List<Cell> ActualChanges;

        static CellComparer CC = new CellComparer();

        public PrologNegotiator()
        {
            DWSMatrix = new CellComponent[DWSSize1, DWSSize2];

            for (int i = 0; i < DWSSize1; i++)
                for (int j = 0; j < DWSSize2; j++)
                    DWSMatrix[i, j] = new CellComponent
                        (CellPriority.NotSetted, CellContent.None);

            DWSMatrix[1, 1] = new CellComponent(CellPriority.NotSetted, CellContent.ContactWire);
            DWSMatrix[2, 0] = new CellComponent(CellPriority.NotSetted, CellContent.ContactWire);
            DWSMatrix[2, 1] = new CellComponent(CellPriority.NotSetted, CellContent.TWire);
            DWSMatrix[3, 1] = new CellComponent(CellPriority.NotSetted, CellContent.CornerWire);
            DWSMatrix[3, 2] = new CellComponent(CellPriority.NotSetted, CellContent.ContactWire);
            DWSMatrix[0, 3] = new CellComponent(CellPriority.NotSetted, CellContent.ContactWire);
            DWSMatrix[0, 4] = new CellComponent(CellPriority.NotSetted, CellContent.CornerWire);
            DWSMatrix[1, 4] = new CellComponent(CellPriority.NotSetted, CellContent.VerticalWire);
            DWSMatrix[2, 4] = new CellComponent(CellPriority.NotSetted, CellContent.ContactWire);
            DWSMatrix[3, 4] = new CellComponent(CellPriority.NotSetted, CellContent.VerticalWire);
            DWSMatrix[4, 4] = new CellComponent(CellPriority.NotSetted, CellContent.TWire);


            /*
            HashSet<Cell> cells = (from ij in new List<Tuple<byte, byte>>
            {
                  new Tuple<byte, byte>(0,0),
                  new Tuple<byte, byte>(1,2),
                  new Tuple<byte, byte>(2,4),
                  new Tuple<byte, byte>(3,1),
                  new Tuple<byte, byte>(4,3)
            }
                                  select ij
                                  into ijs
                                  let i = ijs.Item1
                                  let j = ijs.Item2
                                  select new Cell(i, j, DWSMatrix[i, j])).ToHashSet<Cell>();

            foreach (var cell in cells)
                ;

            ;
            */


            /*
            List<Cell> sortedList = new List<Cell>
            {
                new Cell(1, 2),
                new Cell(1, 3),
                new Cell(4, 5),
                new Cell(8, 9),
                new Cell(8, 11)
            };

            sortedList.SortedAdd(new Cell(8, 10), CC);
            sortedList.SortedAdd(new Cell(1, 1), CC);
            sortedList.SortedAdd(new Cell(4, 7), CC);
            */

            /*
            string filename = "D:\\GraduatingProj\\Prolog_repos\\exercise_lists.pl";

            try
            {
                PlEngine.Initialize(new string[] { "libswipl.dll" });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            PlTermDelegate getWSPScope = GetCellScope;
            PlEngine.RegisterForeign("get_wsp_scope", 2, getWSPScope);

            string tempFilename = Path.GetTempFileName();
            StreamWriter sw = File.CreateText(tempFilename);
            sw.WriteLine("write_cell_scope(Cell):-" +
                         "get_wsp_scope(Cell, L), write_list(L).");
            sw.Close();

            using (PlQuery query = new PlQuery("consult({filename}),consult({tempFilename})"))
            {
                ;
            }

            /*
            PlTerm singleDomain1 = new PlTerm("domain1");
            PlTerm singleDomain2 = new PlTerm("domain2");
            PlTermV term = new PlTermV(2) { };
            PlTermV t = new PlTermV();

            using (PlFrame frame1 = new PlFrame())
            {
                PlTerm varList = PlTerm.PlVar();
                PlTermV args = new PlTermV(2);
                args[0] = new PlTerm("cell(4, 5)");
                args[1] = varList;
                PlQuery.PlCall("get_wps_scope", args);
                Console.WriteLine(varList.ToString());
            }
            */

            Console.Read();
        }

        /*
			Получение области видимости клетки, из которой стартует волна.
		 */

        private bool GetCellScope(PlTermV args)
        {
            PlTerm waveStartCellT = args[0],
                   wscScope = args[1];

            //string[] coords = ((string)waveStartPoint)
            //    .Remove(0, "cell".Length).Trim('(', ')').Split(',');

            PlTerm waveStartPointT = waveStartCellT[1];
            int сellX  = (int)waveStartPointT[1],
                cellY = (int)waveStartPointT[2];

            //int сellX  = int.Parse(coords[0]),
            //    cellY = int.Parse(coords[1]);

            StringBuilder pointScopeBuilder = new StringBuilder("[");

            PlTerm cellScopeListHead = PlTerm.PlVar(),
                   cellScopeListTail = PlTerm.PlTail(cellScopeListHead);

            if (сellX  > 0 &&
                CheckFreeCellConditions(DWSMatrix[сellX  - 1, cellY]))
            {
                cellScopeListTail
                    .Append(new PlTerm($"cell(point({сellX  - 1},{cellY})," +
                        "priority(notsetted), content(none))"));
            }
            if (сellX  < DWSSize1 - 1 &&
                CheckFreeCellConditions(DWSMatrix[сellX  + 1, cellY]))
            {
                cellScopeListTail
                    .Append(new PlTerm($"cell({сellX  + 1},{cellY})"));
            }
            if (cellY > 0 &&
                CheckFreeCellConditions(DWSMatrix[сellX , cellY - 1]))
            {
                cellScopeListTail
                    .Append(new PlTerm($"cell({сellX },{cellY - 1})"));
            }
            if (cellY < DWSSize2 - 1 &&
                CheckFreeCellConditions(DWSMatrix[сellX , cellY + 1]))
            {
                cellScopeListTail
                    .Append(new PlTerm($"cell({сellX },{cellY + 1})"));
            }

            //string pointScope = pointScopeBuilder.ToString().TrimEnd(',') + "]";

            cellScopeListTail.Close();

            //wspScope.Unify(pointScopeListHead);
            //Console.WriteLine(wspScope.ToString());

            //return wspScope.Unify(pointScopeListHead);
            return cellScopeListHead.Unify(wscScope);
        }

        /// <summary>
        /// Получение области видимости клетки на ДРП.
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>

        private List<Cell> GetCellScope(Cell waveStartCell)
        {
            byte сellX  = waveStartCell.X,
                 cellY = waveStartCell.Y;

            List<Cell> wscScope = new List<Cell>(4);

            if (сellX  > 0 &&
                CheckFreeCellConditions(DWSMatrix[cellY, сellX  - 1]))
            {
                wscScope.Add(
                    new Cell((byte)(сellX  - 1), cellY, DWSMatrix[cellY, сellX  - 1])
                            );
            }
            if (сellX  < DWSSize1 - 1 &&
                CheckFreeCellConditions(DWSMatrix[cellY, сellX  + 1]))
            {
                wscScope.Add(
                    new Cell((byte)(сellX  + 1), cellY, DWSMatrix[cellY, сellX  + 1])
                            );
            }
            if (cellY > 0 &&
                CheckFreeCellConditions(DWSMatrix[cellY - 1, сellX ]))
            {
                wscScope.Add(
                    new Cell(сellX , (byte)(cellY - 1), DWSMatrix[cellY - 1, сellX ])
                            );
            }
            if (cellY < DWSSize2 - 1 &&
                CheckFreeCellConditions(DWSMatrix[cellY + 1, сellX ]))
            {
                wscScope.Add(
                    new Cell(сellX , (byte)(cellY + 1), DWSMatrix[cellY + 1, сellX ])
                            );
            }

            return wscScope;
        }

        /// <summary>
        /// Проверка клетки на предмет того, стоит ли её рассматривать
        /// в качестве элемента пути от клетки А до клетки Б.
        /// </summary>
        /// <param name="wsc">Клетка, данные которые следует проверить.</param>
        /// <returns>Возврат: флаг, указывающий на свободность клетки.</returns>

        private static bool CheckFreeCellConditions(CellComponent wsc)
        {
            return wsc.cPriority == CellPriority.NotSetted &&
                   (int)wsc.cContent % 2 == 0;
        }

        private void ArrangePrioritiesWhileTracing()
        {

        }

        private void GetCellInfo()
        {

        }

        /// <summary>
        /// Создаёт актуальную матрицу ДРП на основе
        /// начальной матрицы и таблицы поэтапных изменений.
        /// </summary>

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
            for (int i = 0; i < ChangesBySteps.Count;)
            {
                changesByDepth.Add(++i, new KeyValueTuple<int, List<Cell>>
                                        (0, ChangesBySteps.ElementAt(i)));
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
        /// Создаёт актуальную матрицу ДРП на основе данных о последних изменениях.
        /// Подготовительный этап перед началом построения нового проводника.
        /// </summary>

        private void MakeChanges()
        {
            foreach(Cell mutatingCell in ChangesBySteps.Peek())
                DWSMatrix[mutatingCell.X, mutatingCell.Y] = mutatingCell.cComponent;
        }

        /// <summary>
        /// Отменяет изменения в матрице ДРП, совершённые на текущем этапе.
        /// Завершающий этап после того, как будет произведена обработка
        /// всех возможных вариантов трассировки с проводником на текущем этапе.
        /// </summary>

        private void UndoChanges()
        {
            /*
                Отмена изменений может происходить двумя путями.
                Вариант первый: проводник был проведён первым, до этого
                ДРП представляло собой чистый лист. В этом случае
                изменения просто откатываются.
                Вариант второй: до этого проводника были проведены другие,
                следовательно изменения производятся с оглядкой на состояние
                ДРП до проведения текущего проводника.
                !!!
                Первый вариант может отталкиваться от стартового значения ДРП,
                которое было загружено из файла, либо структура списков
                изменений будет иная - (С0 -> C1), с указанием начального 
                и результирующего содержимого клетки. Лучше иметь стартовую ДРП.
             */

            List<Cell> lastChanges = ChangesBySteps.Pop();
            if (ChangesBySteps.Count == 1)
            {
                foreach (Cell tenetCell in lastChanges)
                {
                    DWSMatrix[tenetCell.X, tenetCell.Y] = 
                        DWSMatrixOnInitializing[tenetCell.X, tenetCell.Y];
                }
            }
            else
            {
                List<Cell> prevChanges = ChangesBySteps.Peek();
                Cell prevActualCell;

                foreach (Cell tenetCell in lastChanges)
                {
                    if ((prevActualCell = prevChanges.Find(actualCell =>
                             actualCell.X == tenetCell.X &&
                             actualCell.Y == tenetCell.Y)) != null)
                        DWSMatrix[tenetCell.X, tenetCell.Y] = prevActualCell.cComponent;
                    else
                        DWSMatrix[tenetCell.X, tenetCell.Y] = tenetCell.cComponent;
                }
            }
        }
    
    }
}
