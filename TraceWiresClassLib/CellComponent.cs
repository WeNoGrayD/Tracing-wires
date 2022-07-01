using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceWiresClassLib
{
    /// <summary>
    /// Содержимое клетки.
    /// </summary>
    public class CellComponent
    {
        public CellPriority CPriority { get; set; }

        private CellContent _cContent;
        public CellContent CContent
        {
            get { return _cContent; }
            private set
            {
                _cContent = value;
                switch(value)
                {
                    case CellContent.None:
                    case CellContent.Contact:
                    default:
                        { _cDirection = CellContentDirection.Nowhere; break; }
                    case CellContent.HorizontalWire:
                        { _cDirection = CellContentDirection.LeftToRight; break; }
                    case CellContent.VerticalWire:
                        { _cDirection = CellContentDirection.UpToDown; break; }
                    case CellContent.CrossWire:
                    case CellContent.WireIntersection:
                        { _cDirection = CellContentDirection.Anywhere; break; }
                }
            }
        }

        private CellContentDirection _cDirection;
        public CellContentDirection CDirection
        {
            get { return _cDirection; }
            private set
            {
                _cDirection = value;
                /*
                byte snapPoints = (byte)((byte)value & 0b00001111);

                switch (CContent) // Смотрим старое содержимое.
                {

                    case CellContent.None:
                        {
                            switch (snapPoints)
                            {
                                case 0b00001100:
                                case 0b00001010:
                                case 0b00000101:
                                case 0b00000011:
                                    { CContent = CellContent.CornerWire; break; }
                                case 0b00001001:
                                    { CContent = CellContent.VerticalWire; break; }
                                case 0b00000110:
                                    { CContent = CellContent.HorizontalWire; break; }
                                case 0b00001000:
                                case 0b00000100:
                                case 0b00000010:
                                case 0b00000001:
                                    { break; }
                            }
                            break;
                        }

                    case CellContent.Contact:
                        {
                            CContent = CellContent.ContactWire;
                            break;
                        }

                    case CellContent.HorizontalWire:
                    case CellContent.VerticalWire:
                    case CellContent.CornerWire:
                        {
                            switch (snapPoints)
                            {
                                case 0b00001110:
                                case 0b00000111:
                                    { CContent = CellContent.THorizontalWire; break; }
                                case 0b00001101:
                                case 0b00001011:
                                    { CContent = CellContent.TVerticalWire; break; }
                            }
                            break;
                        }

                    case CellContent.THorizontalWire:
                    case CellContent.TVerticalWire:
                        {
                            CContent = CellContent.CrossWire;
                            break;
                        }
                }
                */
            }
        }

        public CellComponent(CellPriority cPriority, CellContent cContent)
        {
            CPriority = cPriority;
            CContent = cContent;
        }

        public CellComponent(CellPriority cPriority, 
                             CellContent cContent,
                             CellContentDirection cDirection)
        {
            CPriority = cPriority;
            CContent = cContent;
            _cDirection = cDirection;
        }

        /// <summary>
        /// Установка содержимого и направления клетки при помощи
        /// добавления точки соединения с проводом.
        /// </summary>
        /// <param name="newContent"></param>
        /// <param name="prevCell"></param>
        public void AddSnapPoint(Cell prevCell, Cell[] thisScope, 
            bool hasIntersection = false)
        {
            //Добавим одну точку соединения.

            byte addSnapPoint = 0b00001000,
                 bCDirection = (byte)CDirection,
                 snapPointsQuantity = (byte)(bCDirection >> 4),
                 snapPoints = 0;

            /*
                Если количество входов вместе меньше четырёх,
                то имеет смысл менять направления входов
                и содержимое клетки.
             */

            if (snapPointsQuantity < 4)
            {
                foreach (Cell cellWithinScope in thisScope)
                {
                    if (prevCell.CPoint == cellWithinScope?.CPoint)
                        break;
                    addSnapPoint = (byte)(addSnapPoint >> 1);
                }
                CDirection = (CellContentDirection)
                    ((snapPoints = (byte)((bCDirection & 0b00001111) | addSnapPoint)) |
                        (byte)(++snapPointsQuantity << 4));


                switch (CContent) // Смотрим старое содержимое.
                {
                    /*
                        Если клетка была пустой, то здесь самая большая неопределённость.
                        Из алгоритма простановки проводника следует,
                        что в пустую клетку придётся возвращаться два раза.
                        Один раз - проставить направление, из которого мы пришли в эту
                        клетку в первый раз, второй - откуда во второй.
                        В первый раз содержимое никогда не меняется.
                        (хотя можно менять на HalfWire и добавить соответствующее значение
                         в перечисление, но смысла особого нет)
                     */

                    case CellContent.None:
                        {
                            switch (snapPoints)
                            {
                                case 0b00001100:
                                case 0b00001010:
                                case 0b00000101:
                                case 0b00000011:
                                    { _cContent = CellContent.CornerWire; break; }
                                case 0b00001001:
                                    { _cContent = CellContent.VerticalWire; break; }
                                case 0b00000110:
                                    { _cContent = CellContent.HorizontalWire; break; }
                                case 0b00001000:
                                case 0b00000100:
                                case 0b00000010:
                                case 0b00000001:
                                    { break; }
                            }
                            break;
                        }

                    /*
                        Для контакта достаточно поменять содержимое 
                        на "контакт с проводом".     
                     */

                    case CellContent.Contact:
                        {
                            _cContent = CellContent.ContactWire;
                            break;
                        }

                    /*
                        Если к угловому, горизонтальному или вертикальному проводнику
                        добавляется один полупровод, то это - т-проводник.
                        Если есть пересечение, то вместо т-проводника ставится, 
                        собственно, пересечение проводников.
                     */

                    case CellContent.HorizontalWire:
                    case CellContent.VerticalWire:
                    case CellContent.CornerWire:
                        {
                            if (hasIntersection)
                            {
                                _cContent = CellContent.WireIntersection;
                                CDirection = CellContentDirection.Anywhere;
                            }
                            else
                            {
                                switch (snapPoints)
                                {
                                    case 0b00001110:
                                    case 0b00000111:
                                        { _cContent = CellContent.THorizontalWire; break; }
                                    case 0b00001101:
                                    case 0b00001011:
                                        { _cContent = CellContent.TVerticalWire; break; }
                                }
                            }
                            break;
                        }

                    /*
                        Если к т-проводу добавляется ещё один полупровод,
                        то получается крестовый проводник.
                     */

                    case CellContent.THorizontalWire:
                    case CellContent.TVerticalWire:
                        {
                            _cContent = CellContent.CrossWire;
                            break;
                        }
                }
            }
        }

        public void SetStart()
        {
            CContent = CellContent.Start;
            return;
        }

        public void SetFinish()
        {
            CContent = CellContent.Finish;
            return;
        }

        /// <summary>
        /// Клонирование объекта.
        /// </summary>
        /// <returns></returns>
        public CellComponent Clone()
        {
            return new CellComponent(CPriority, CContent, CDirection);
        }
    }

    /// <summary>
    /// Перечисление возможных приоритетов клеток.
    /// </summary>
    public enum CellPriority : sbyte
    {
        Closed = -2,
        NotSetted = -1,
        ToDown = 0,
        ToRight = 1,
        ToLeft = 2,
        ToUp = 3
    }

    /// <summary>
    /// Перечисление возможных вариантов содержимого клетки.
    /// Требуемые соотношения значений:
    /// 1) x >= 0 && x % 2 == 0 => x свободен для прокладки проводника.
    /// 2) для свободных x -> x & 00000010 != 0 => в x можно провести
    /// горизонтальный провод, иначе - вертикальный.
    /// </summary>
    public enum CellContent : sbyte
    {
        Ignore = -1,
        None = 0,
        Contact = 1,
        HorizontalWire = 2,
        ContactWire = 3,
        VerticalWire = 4,
        CornerWire = 5,
        THorizontalWire = 6, // горизонтальная перекладина и полустолб (sbyte)cellContent & 2
        CrossWire = 7,
        TVerticalWire = 8, // столб и горизонтальная полуперекладина. !((sbyte)cellContent & 2)
        WireIntersection = 9,
        Start = 127,
        Finish = -127
    }

    /// <summary>
    /// Перечисление возможных вариантов направления содержимого клетки.
    /// Свойства флагов:
    /// 2) 4-бит флаг направлений входов = x[3..0].
    /// 2) 4-бит флаг количества входов = x[7..4] = x >> 4.
    /// </summary>
    public enum CellContentDirection : byte
    {
        Nowhere = 0b00000000,
        ToDown = 0b00010001,
        ToRight = 0b00010010,
        ToLeft = 0b00010100,
        ToUp = 0b00011000,
        DownToRight = 0b00100011,
        DownToLeft = 0b00100101,
        LeftToRight = 0b00100110,
        UpToDown = 0b00101001,
        UpToRight = 0b00101010,
        UpToLeft = 0b00101100,
        LeftToDownToRight = 0b00110111,
        UpToRightToDown = 0b00111011,
        UpToLeftToDown = 0b00111101,
        LeftToUpToRight = 0b00111110,
        Anywhere = 0b01001111
    }
}
