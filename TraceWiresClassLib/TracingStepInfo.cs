using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceWiresClassLib
{
    /// <summary>
    /// Структура, предоставляющая входную информацию
    /// для этапа трассировки.
    /// </summary>
    internal struct TracingStepInfo
    {
        public CellPoint StartCellPoint { get; private set; }

        public CellPoint FinishCellPoint { get; private set; }

        public CellPriority[] PrioritiesOrder { get; private set; }

        public Wire ActualWire { get; set; }

        public TracingStepInfo(CellPoint startCellPoint, CellPoint finishCellPoint,
                               Wire actualWire)
        {
            StartCellPoint = startCellPoint;
            FinishCellPoint = finishCellPoint;

            // Построение порядка приоритетов.

            PrioritiesOrder = new CellPriority[4];

            int dX = StartCellPoint.X - FinishCellPoint.X,
                dY = StartCellPoint.Y - FinishCellPoint.Y,
                ddXdY = Math.Abs(dX) - Math.Abs(dY);

            int prXInd, prYInd;

            if (ddXdY >= 0)
            {
                prXInd = 0;
                prYInd = 1;
            }
            else
            {
                prYInd = 0;
                prXInd = 1;
            }

            bool dXCond = dX >= 0, dYCond = dY >= 0;

            if (dXCond)
            {
                PrioritiesOrder[prXInd] = CellPriority.ToRight;
                PrioritiesOrder[prXInd + 2] = CellPriority.ToLeft;
            }
            else
            {
                PrioritiesOrder[prXInd] = CellPriority.ToLeft;
                PrioritiesOrder[prXInd + 2] = CellPriority.ToRight;
            }
            if (dYCond)
            {
                PrioritiesOrder[prYInd] = CellPriority.ToDown;
                PrioritiesOrder[prYInd + 2] = CellPriority.ToUp;
            }
            else
            {
                PrioritiesOrder[prYInd] = CellPriority.ToUp;
                PrioritiesOrder[prYInd + 2] = CellPriority.ToDown;
            }

            // Ссылка на провод равна null.

            ActualWire = actualWire;
        }
    }
}
