using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceWiresClassLib
{
    /// <summary>
    /// Сбалансированное двоичное дерево.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class BinaryTree<T> : IEnumerable, IEnumerable<T> 
    {
        public T Root { get; private set; }

        private IComparer<T> _elementsComparer;

        public BinaryTree<T>[] ChildrenTrees { get; private set; }

        public BinaryTree(T _root, IComparer<T> elementsComparer)
        {
            Root = _root;
            _elementsComparer = elementsComparer;
            ChildrenTrees = null;
        }

        /// <summary>
        /// Добавление элемента.
        /// </summary>
        /// <param name="newChild"></param>
        /// <param name="newChildTree"></param>
        public void Add(T newChild, 
                        BinaryTree<T> newChildTree = null, 
                        bool needBalance = true)
        {
            if (ChildrenTrees == null)
                ChildrenTrees = new BinaryTree<T>[2];

            int cmpRes = _elementsComparer.Compare(newChild, Root);

            BinaryTree<T> lessBalancedChildBT;
            if (newChildTree == null)
                newChildTree = new BinaryTree<T>(newChild, this._elementsComparer);

            int[] childrenDepths = new int[2]
                {
                    ChildrenTrees[0]?.GetDepth() ?? 0,
                    ChildrenTrees[1]?.GetDepth() ?? 0
                };
            int lessBalancedInd, moreBalancedInd;
            if (childrenDepths[0] >= childrenDepths[1])
            {
                lessBalancedInd = 1;
                moreBalancedInd = 0;
            }
            else
            {
                lessBalancedInd = 0;
                moreBalancedInd = 1;
            }
            lessBalancedChildBT = ChildrenTrees[lessBalancedInd];

            int sideInd = -1;

            switch (cmpRes)
            {
                case -1:
                    {
                        sideInd = 0;
                        break;
                    }

                /*
                    Если корень дерева и новый элемент равны,
                    то с несбалансированной стороны веткой 
                    становится новый элемент, а в его дети
                    записывается старый несбалансированный узел.
                 */

                case 0:
                    {
                        ChildrenTrees[lessBalancedInd] = newChildTree;
                        newChildTree.AddChildBinaryTree(lessBalancedChildBT);
                        break;
                    }
                case 1:
                    {
                        sideInd = 1;
                        break;
                    }
            }

            /*
                Если результат сравнения - "элементы неравны"
                и если новый элемент должен добавиться
                в менее сбалансированную ветку, то дерево балансируется.
             */

            if (cmpRes != 0 && needBalance)
            {
                if (ChildrenTrees[sideInd] == null)
                {
                    ChildrenTrees[sideInd] = newChildTree;
                    childrenDepths[sideInd]++;
                }
                else
                {
                    ChildrenTrees[sideInd].Add(newChild, newChildTree);
                    childrenDepths[sideInd] = ChildrenTrees[sideInd].GetDepth();
                }

                if (childrenDepths[0] >= childrenDepths[1])
                {
                    lessBalancedInd = 1;
                    moreBalancedInd = 0;
                }
                else
                {
                    lessBalancedInd = 0;
                    moreBalancedInd = 1;
                }

                if (childrenDepths[0] - childrenDepths[1] != 0)
                {
                    lessBalancedChildBT =
                        ChildrenTrees[lessBalancedInd];
                    BinaryTree<T> moreBalancedChildBT =
                        ChildrenTrees[moreBalancedInd];

                    if (moreBalancedChildBT.ChildrenTrees == null)
                        moreBalancedChildBT.ChildrenTrees = new BinaryTree<T>[2];

                    if ((moreBalancedChildBT.ChildrenTrees[moreBalancedInd]?.GetDepth() ?? 0) >=
                        (moreBalancedChildBT.ChildrenTrees[lessBalancedInd]?.GetDepth() ?? 0))
                        DoLittleChildrenRotation(lessBalancedInd, lessBalancedChildBT,
                                                 moreBalancedInd, moreBalancedChildBT);
                    else
                        DoBigChildrenRotation(lessBalancedInd, lessBalancedChildBT,
                                              moreBalancedInd, moreBalancedChildBT);
                }
            }
        }

        /// <summary>
        /// Малый поворот дерева.
        /// </summary>
        /// <param name="lessBalancedInd"></param>
        /// <param name="lessBalancedChildBT"></param>
        /// <param name="moreBalancedInd"></param>
        /// <param name="moreBalancedChildBT"></param>
        private void DoLittleChildrenRotation(int lessBalancedInd,
                                              BinaryTree<T> lessBalancedChildBT,
                                              int moreBalancedInd,
                                              BinaryTree<T> moreBalancedChildBT)
        {
            BinaryTree<T>[] balancingBTChildren = new BinaryTree<T>[2];

            balancingBTChildren[lessBalancedInd] = lessBalancedChildBT;
            balancingBTChildren[moreBalancedInd] = moreBalancedChildBT.ChildrenTrees[lessBalancedInd];

            this.ChildrenTrees[lessBalancedInd] =
                    new BinaryTree<T>(this.Root, this._elementsComparer);
            this.ChildrenTrees[lessBalancedInd].ChildrenTrees = balancingBTChildren;

            this.Root = moreBalancedChildBT.Root;
            this.ChildrenTrees[moreBalancedInd] =
                moreBalancedChildBT.ChildrenTrees[moreBalancedInd];
        }

        /// <summary>
        /// Большой поворот дерева.
        /// </summary>
        /// <param name="lessBalancedInd"></param>
        /// <param name="lessBalancedChildBT"></param>
        /// <param name="moreBalancedInd"></param>
        /// <param name="moreBalancedChildBT"></param>
        private void DoBigChildrenRotation(int lessBalancedInd,
                                           BinaryTree<T> lessBalancedChildBT,
                                           int moreBalancedInd,
                                           BinaryTree<T> moreBalancedChildBT)
        {
            /*
            foreach (T t in this)
            {
                Cell cell = t as Cell;
                Console.WriteLine($"Cell: {cell.X}, {cell.Y}");
            }
            */

            BinaryTree<T> losingWeightBT =
                moreBalancedChildBT.ChildrenTrees[lessBalancedInd],
                          oldLessBalancedChildBT =
                this.ChildrenTrees[lessBalancedInd];

            this.ChildrenTrees[lessBalancedInd] =
                new BinaryTree<T>(this.Root, this._elementsComparer)
                {
                    ChildrenTrees = new BinaryTree<T>[2]
                };
            this.ChildrenTrees[lessBalancedInd].ChildrenTrees[lessBalancedInd] =
                oldLessBalancedChildBT;
            this.ChildrenTrees[lessBalancedInd].ChildrenTrees[moreBalancedInd] =
                losingWeightBT.ChildrenTrees[lessBalancedInd];

            moreBalancedChildBT.ChildrenTrees[lessBalancedInd] =
                losingWeightBT.ChildrenTrees[moreBalancedInd];

            this.Root = losingWeightBT.Root;
        }

        /// <summary>
        /// Присоединение целого дерева к текущему узла в качестве отпрыска.
        /// </summary>
        /// <param name="newChildBT"></param>
        public void AddChildBinaryTree(BinaryTree<T> newChildBT)
        {
            if (ChildrenTrees == null)
                ChildrenTrees = new BinaryTree<T>[2];

            if (ChildrenTrees[0] == null)
            {
                ChildrenTrees[0] = newChildBT;
                return;
            }
            if (ChildrenTrees[1] == null)
            {
                ChildrenTrees[1] = newChildBT;
                return;
            }

            //// ! Потом посмотреть, как добавлять новый узел.

            int child1Depth = ChildrenTrees[0].GetDepth(),
                child2Depth = ChildrenTrees[1].GetDepth();

            if (child1Depth >= child2Depth)
                ChildrenTrees[1].AddChildBinaryTree(newChildBT);
            else
                ChildrenTrees[0].AddChildBinaryTree(newChildBT);

            return;
        }

        /// <summary>
        /// Получение максимальной глубины дерева.
        /// </summary>
        /// <returns></returns>
        public int GetDepth()
        {
            if (ChildrenTrees == null)
                return 1;
            return 1 + ChildrenTrees.Max(ct => ct?.GetDepth() ?? 0);
        }

        /// <summary>
        /// Создание дерева из отсортированного списка.
        /// </summary>
        /// <param name="itemsEnumerable"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        static public BinaryTree<T> CreateFromSortedList(
            IEnumerable<T> itemsEnumerable, IComparer<T> comparer)
        {
            List<T> items = itemsEnumerable as List<T> ?? itemsEnumerable.ToList();

            if (items.Count == 0)
                return null;

            int middleItemInd = items.Count >> 1;
            BinaryTree<T> btRoot = new BinaryTree<T>(items[middleItemInd], comparer),
                          btLeftChild = null,
                          btRightChild = null;

            btLeftChild = CreateFromSortedList(
                      items.Take(middleItemInd), comparer);
            btRightChild = CreateFromSortedList(
                         items.Skip(middleItemInd + 1)
                              .Take(items.Count - middleItemInd),
                         comparer);

            if (btLeftChild != null)
            {
                btRoot.ChildrenTrees = new BinaryTree<T>[2];
                btRoot.ChildrenTrees[0] = btLeftChild;
                btRoot.ChildrenTrees[1] = btRightChild;
            }

            return btRoot;
        }

        /// <summary>
        /// Добавление отсортированного списка.
        /// </summary>
        public void AddSortedList(IEnumerable<T> itemsEnumerable)
        {
            List<T> items = itemsEnumerable as List<T> ?? itemsEnumerable.ToList(),
                    itemsTenet = new List<T>();
            itemsTenet.AddRange(items);
            itemsTenet.Reverse();
            int itemsMid = items.Count >> 1, 
                itemsTenetMid = items.Count % 2 == 0 ? itemsMid + 1 : itemsMid;
            bool noNeedBalance = true;

            for (int id = 0, it = 0; 
                 id < itemsTenetMid || (noNeedBalance = id == itemsMid);
                 id++, it++)
            {
                this.Add(items[id], needBalance: noNeedBalance);
                if (noNeedBalance)
                    this.Add(itemsTenet[it], needBalance: true);
            }
        }

        /// <summary>
        /// Проверка наличия элемента в двоичном дереве.
        /// Элемент должен иметь инициализированными те характеристики,
        /// по которым производится поиск в двоичном дереве.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public bool Contains(T element)
        {
            int cmpRes = -1;
            if ((cmpRes = _elementsComparer.Compare(Root, element)) == 0)
                return Root.Equals(element);

            if (ChildrenTrees == null)
                return false;

            if (cmpRes == -1 && ChildrenTrees[0] != null)
                return ChildrenTrees[0].Contains(element);
            else if (cmpRes == 1 && ChildrenTrees[1] != null)
                return ChildrenTrees[1].Contains(element);

            return false;
        }

        /// <summary>
        /// Проверка наличия элемента с заданными характеристиками,
        /// по которым производится поиск в двоичном дереве.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public bool ContainsEqual(T element)
        {
            int cmpRes = -1;
            if ((cmpRes = _elementsComparer.Compare(Root, element)) == 0)
                return true;

            if (ChildrenTrees == null)
                return false;

            if (cmpRes == -1 && ChildrenTrees[0] != null)
                return ChildrenTrees[0].ContainsEqual(element);
            else if (cmpRes == 1 && ChildrenTrees[1] != null)
                return ChildrenTrees[1].ContainsEqual(element);

            return false;
        }

        /// <summary>
        /// Поиск элемента с заданными характеристиками,
        /// по которым производится поиск в двоичном дереве.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public T FindEqual(T value)
        {
            int cmpRes = -1;
            if ((cmpRes = _elementsComparer.Compare(Root, value)) == 0)
                return Root;

            if (ChildrenTrees == null)
                return default(T);

            if (cmpRes == -1 && ChildrenTrees[0] != null)
                return ChildrenTrees[0].FindEqual(value);
            else if (cmpRes == 1 && ChildrenTrees[1] != null)
                return ChildrenTrees[1].FindEqual(value);

            return default(T);
        }

        /// <summary>
        /// Перечислитель дерева.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            bool hasChildren = ChildrenTrees != null;

            if (hasChildren && ChildrenTrees[0] != null)
            {
                IEnumerator<T> lesserChildrenEnumerator =
                    (IEnumerator<T>)ChildrenTrees[0].GetEnumerator();

                while (lesserChildrenEnumerator.MoveNext())
                    yield return lesserChildrenEnumerator.Current;
            }

            yield return Root;

            if (hasChildren && ChildrenTrees[1] != null)
            {
                IEnumerator<T> greaterChildrenEnumerator =
                    (IEnumerator<T>)ChildrenTrees[1].GetEnumerator();

                while (greaterChildrenEnumerator.MoveNext())
                    yield return greaterChildrenEnumerator.Current;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
