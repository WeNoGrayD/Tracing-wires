using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceWiresClassLib
{
    /// <summary>
    /// Класс, предоставляющий услугу сравнения клеток 
    /// по их местоположению в ДРП.
    /// </summary>
    public class CellComparer : IComparer<Cell>
    {
        public int Compare(Cell cell1, Cell cell2)
        {
            CellPoint cp1 = cell1.CPoint,
                      cp2 = cell2.CPoint;

            if (cp1.X == cp2.X)
            {
                if (cp1.Y == cp2.Y)
                    return 0;

                if (cp1.Y > cp2.Y)
                    return 1;

                return -1;
            }

            if (cp1.X > cp2.X)
                return 1;

            return -1;
        }
    }

    /// <summary>
    /// Класс, предоставляющий услугу сравнения клеток 
    /// по их местоположению в ДРП.
    /// </summary>
    public class CellPointComparer : IComparer<CellPoint>
    {
        public int Compare(CellPoint cell1, CellPoint cell2)
        {
            CellPoint cp1 = cell1,
                      cp2 = cell2;

            if (cp1.X == cp2.X)
            {
                if (cp1.Y == cp2.Y)
                    return 0;

                if (cp1.Y > cp2.Y)
                    return 1;

                return -1;
            }

            if (cp1.X > cp2.X)
                return 1;

            return -1;
        }
    }
}
