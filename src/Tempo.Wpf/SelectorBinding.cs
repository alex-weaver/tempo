using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls.Primitives;
using Tempo.Util;

namespace Tempo.Wpf
{
    /// <summary>
    /// Provides a method for binding the contents of a WPF selector control to a list cell.
    /// </summary>
    public static class SelectorBinding
    {
        /// <summary>
        /// Binds the contents of a WPF selector control to a list cell, until the calling scope ends.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list cell.</typeparam>
        /// <param name="source">The list containing the items to show in the selector.</param>
        /// <param name="target">The selector control.</param>
        public static void Bind<T>(IListCellRead<T> source, Selector target)
        {
            target.Items.Clear();
            source.Changes(changes =>
                {
                    var oldSelectedIndex = target.SelectedIndex;
                    int newSelectedIndex = oldSelectedIndex;

                    foreach (var change in changes)
                    {
                        switch (change.Action)
                        {
                            case ListChangeAction.Add:
                                ListRangeActions.InsertRange(target.Items, change.NewStartingIndex, change.NewItems);
                                //newSelectedIndex = change.NewStartingIndex + change.NewItems.Count - 1;
                                if(change.NewStartingIndex <= oldSelectedIndex)
                                {
                                    newSelectedIndex = oldSelectedIndex + change.NewItems.Count;
                                }
                                if(newSelectedIndex < 0 && target.Items.Count > 0)
                                {
                                    newSelectedIndex = 0;
                                }
                                break;
                            case ListChangeAction.Remove:
                                ListRangeActions.RemoveRange<T>(target.Items, change.OldStartingIndex, change.OldItemCount);
                                if(target.Items.Count == 0)
                                {
                                    newSelectedIndex = -1;
                                }
                                else if(oldSelectedIndex >= change.OldStartingIndex + change.OldItemCount)
                                {
                                    newSelectedIndex = oldSelectedIndex - change.OldItemCount;
                                }
                                else if(oldSelectedIndex >= change.OldStartingIndex)
                                {
                                    newSelectedIndex = change.OldStartingIndex - 1;
                                    if(newSelectedIndex < 0 && target.Items.Count > 0)
                                    {
                                        newSelectedIndex = 0;
                                    }
                                }
                                break;
                            case ListChangeAction.Replace:
                                ListRangeActions.ReplaceRange(target.Items, change.NewStartingIndex, change.NewItems);
                                break;
                        }
                    }

                    target.SelectedIndex = newSelectedIndex;
                });
        }
    }
}
