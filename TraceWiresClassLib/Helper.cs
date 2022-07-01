using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceWiresClassLib
{
    internal static class Helper
    {
        /// <summary>
        /// Метод добавления элемента в отсортированный список.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sortedList"></param>
        /// <param name="newEl"></param>
        /// <param name="comparer"></param>

        public static void SortedAdd<T>(this List<T> sortedList, T newEl, IComparer<T> comparer)
        {
            if (sortedList == null)
                return;

            int indBefore = 0;

            if (sortedList.Count > 0)
            {
                for (; indBefore < sortedList.Count; indBefore++)
                    if (comparer.Compare(sortedList[indBefore], newEl) == 1)
                        break;
            }

            sortedList.Insert(indBefore, newEl);
        }

        /// <summary>
        /// Метод добавления элемента в отсортированный список.
        /// Имеется возможность прервать добавление элемента
        /// по какому-либо условию (либо наоборот, продолжать 
        /// поиск подходящего места).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sortedList"></param>
        /// <param name="newEl"></param>
        /// <param name="comparer"></param>

        public static void SortedAddWithConditionOnCompare<T>
            (this List<T> sortedList, T newEl, IComparer<T> comparer,
             Func<int, bool> userCondition)
        {
            if (sortedList == null)
                return;

            int indBefore = 0;
            bool specCond = false;

            if (sortedList.Count > 0)
            {
                for (; indBefore < sortedList.Count; indBefore++)
                {
                    int innerCond = comparer.Compare(sortedList[indBefore], newEl);
                    specCond = userCondition(innerCond);
                    if (specCond || innerCond == 1)
                        break;
                }
            }

            if (!specCond)
                sortedList.Insert(indBefore, newEl);
        }

        /// <summary>
        /// Метод добавления элемента в отсортированный список.
        /// Имеется возможность прервать добавление элемента
        /// по какому-либо условию (либо наоборот, продолжать 
        /// поиск подходящего места).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sortedList"></param>
        /// <param name="newEl"></param>
        /// <param name="comparer"></param>

        public static void SortedAddWtihConditionOnCompare<T>
            (this HashSet<T> sortedList, T newEl, IComparer<T> comparer,
             Func<int, bool> userCondition)
        {
            if (sortedList == null)
                return;

            int indBefore = 0;
            bool specCond = false;

            if (sortedList.Count > 0)
            {
                for (; indBefore < sortedList.Count; indBefore++)
                {
                    //int innerCond = comparer.Compare(sortedList[indBefore], newEl);
                    //specCond = userCondition(innerCond);
                    //if (specCond || innerCond == 1)
                        break;
                }
            }

            /*
            if (!specCond)
                sortedList.Insert(indBefore, newEl);
                */
        }

        public static void CopyToList<T>(this List<T> source, List<T> dest)
        {
            foreach (T element in source)
                dest.Add(element);
        }

        public static List<T> Copy<T>(this List<T> source)
        {
            List<T> dest = new List<T>();
            source.CopyToList(dest);
            return dest;
        }

        public static Dictionary<TKey, TValue> Copy<TKey, TValue>
            (this Dictionary<TKey, TValue> source)
        {
            Dictionary<TKey, TValue> dest = new Dictionary<TKey, TValue>();
            foreach (TKey key in source.Keys)
                dest.Add(key, source[key]);
            return dest;
        }


        /// <summary>
        /// Операция возвращения индекса элемента в массиве.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        public static int IndexOf<T>(this T[] array, T element)
        {
            int i = -1;
            while (++i < array.Length && !array[i].Equals(element))
                ;
            if (i == array.Length)
                i = -1;

            return i;
        }

        /// <summary>
        /// Операция возвращения индексов всех элементов, 
        /// совпадающих с эталоном.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        public static IEnumerable<int> IndexesOf<T>(this T[] array, T element)
            where T : IEquatable<T>
        {
            int i = -1;
            while (i < array.Length)
            {
                while (++i < array.Length && !((IEquatable<T>)array[i]).Equals(element))
                    ;

                if (i == array.Length)
                    yield break;

                yield return i;
            }
        }

        /// <summary>
        /// Разглаживание списка списков в список.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="superEnum"></param>
        /// <returns></returns>
        public static List<T> FlattenEnumerable<T>(
            this IEnumerable<IEnumerable<T>> superEnum)
        {
            List<T> flattedEnum = new List<T>();
            foreach (IEnumerable<T> subEnum in superEnum)
                flattedEnum.AddRange(subEnum);
            return flattedEnum;
        }
    }
}
