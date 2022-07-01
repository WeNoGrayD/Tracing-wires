using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace TraceWiresViewModel
{
    // Помощник в области обработки контролов.

    public static class UIHelper
    {
        public static T FindChild<T>(DependencyObject parent, string childName,
                                     bool fullStartStringNameMatch)
                where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName,
                                              fullStartStringNameMatch);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null)
                    {
                        if ((fullStartStringNameMatch &&
                             frameworkElement.Name == childName) ||
                            (!fullStartStringNameMatch &&
                             frameworkElement.Name.StartsWith(childName)))
                        // if the child's name is of the request name
                        {
                            foundChild = (T)child;
                            break;
                        }
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        // Поиск всех элементов с совпадающим в начале именем.

        public static void FindMatchedChildren<T>(
                DependencyObject parent, string childName, 
                ref List<T> matchedChildren)
                where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null)// || string.IsNullOrEmpty(childName))
            {
                matchedChildren = null;
                return;
            }

            List<T> localMatchedChildren = new List<T>();

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    FindMatchedChildren<T>(child, childName, ref matchedChildren);
                }
                else
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null)
                    {
                        if (frameworkElement.Name.StartsWith(childName))
                            // if the child's name is of the request name
                            localMatchedChildren.Add((T)child);

                        if (frameworkElement is Panel)
                        {
                            FindMatchedChildren<T>(child, childName, 
                                                   ref matchedChildren);
                        }
                    }
                }
            }

            matchedChildren.AddRange(localMatchedChildren);

            return;
        }
    }
}
