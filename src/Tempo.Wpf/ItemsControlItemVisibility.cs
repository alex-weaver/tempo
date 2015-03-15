using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Tempo.Wpf
{
    public static class ItemsControlItemVisibility
    {
        public static void Watch(ItemsControl view, Action<int, int> visibleSetChanged)
        {
            var callingScope = CurrentThread.CurrentContinuousScope();

            ScrollChangedEventHandler handler = (s, e) =>
            {
                var scrollViewer = (FrameworkElement)s;

                int firstVisible = -1;
                int lastVisible = -1;

                for (int i = 0; i < view.Items.Count; ++i)
                {
                    var container = (FrameworkElement)view.ItemContainerGenerator.ContainerFromIndex(i);

                    if (IsVisible(container, scrollViewer))
                    {
                        if (firstVisible < 0)
                            firstVisible = i;

                        lastVisible = i;
                    }
                }

                if (firstVisible < 0)
                    visibleSetChanged(0, 0);
                else
                    visibleSetChanged(firstVisible, (lastVisible - firstVisible) + 1);
            };

            view.AddHandler(ScrollViewer.ScrollChangedEvent, handler);
            callingScope.lifetime.WhenDead(() => view.RemoveHandler(ScrollViewer.ScrollChangedEvent, handler));
        }


        private static bool IsVisible(FrameworkElement child, FrameworkElement scrollViewer)
        {
            var childTransform = child.TransformToAncestor(scrollViewer);
            var childRectangle = childTransform.TransformBounds(new Rect(new Point(0, 0), child.RenderSize));
            var ownerRectangle = new Rect(new Point(0, 0), scrollViewer.RenderSize);
            return ownerRectangle.IntersectsWith(childRectangle);
        }
    }
}
