using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceWiresClassLib
{
    /// <summary>
    /// Расположение клетки на ДРП.
    /// </summary>
    public struct CellPoint : IEquatable<CellPoint>
    {
        public byte X { get; private set; }

        public byte Y { get; private set; }

        public CellPoint(byte x, byte y)
        {
            X = x;
            Y = y;
        }

        public void Deconstruct(out byte x, out byte y)
        {
            x = X;
            y = Y;
        }

        bool IEquatable<CellPoint>.Equals(CellPoint cp2)
        {
            return this.X == cp2.X && this.Y == cp2.Y;
        }

        public static bool operator ==(CellPoint cp1, CellPoint cp2)
        {
            return ((IEquatable<CellPoint>)cp1).Equals(cp2);
        }

        public static bool operator !=(CellPoint cp1, CellPoint cp2)
        {
            return !((IEquatable<CellPoint>)cp1).Equals(cp2);
        }
    }
}
