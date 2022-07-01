using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceWiresClassLib
{
    /// <summary>
    /// Класс, представляющий проводник.
    /// </summary>
    internal class Wire
    {
        private uint _id;

        public Dictionary<CellPoint, CellComponent> OccupiedCells { get; private set; }

        public Wire(uint id)
        {
            _id = id;
            OccupiedCells = new Dictionary<CellPoint, CellComponent>();
        }

        public bool ContainsCellPoint(CellPoint point)
        {
            return OccupiedCells.ContainsKey(point);
        }

        public void AddCellsRange(List<CellPoint> cellPoints)
        {
            foreach(CellPoint cPoint in cellPoints)
            {
                OccupiedCells.Add(cPoint, Tracer.DWSMatrix[cPoint.Y, cPoint.X]);
            }
        }
    }
}
