using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceWiresClassLib
{
    /// <summary>
    /// Класс Cell. Представляет клетку на ДРП.
    /// </summary>

    public class Cell
    {
        public CellPoint CPoint { get; private set; }

        public CellComponent CComponent { get; set; }

        public Cell(CellPoint cPoint, CellComponent cComponent)
        {
            CPoint = cPoint;
            CComponent = cComponent;
        }
    }

    /// <summary>
    /// Пара клетка-область её видимости.
    /// </summary>
    internal struct CellScopePair
    {
        public Cell CenterCell { get; private set; }

        public Cell[] CellScope { get; private set; }

        public CellScopePair(Cell centerCell, Cell[] cellScope)
        {
            CenterCell = centerCell;
            CellScope = cellScope;
        }
    }
}
