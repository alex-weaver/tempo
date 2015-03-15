using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tempo
{
    /// <summary>
    /// Describes the action that caused a list cell to change.
    /// </summary>
    public enum ListChangeAction
    {
        Add = 0,
        Remove = 1,
        Replace = 2,
    }

    /// <summary>
    /// Describes a single change of a list cell.
    /// If Action is Add, then NewItems contains the items added and NewStartingIndex is a valid index.
    /// If Action is Removed, then OldStartingIndex and OldItemCount define the range that was removed
    /// If Action is Replace, then NewItems contains the replacement elements. OldStartingIndex and
    /// NewStartingIndex are equal, and are valid indices. OldItemCount is equal to NewItems.Count()
    /// </summary>
    /// <remarks>
    /// ListCellChanged implements IRefCounted in case the NewItems collection contains reference counted
    /// items. When the ListCellChanged object is destroyed, the items in the NewItems collection are released.
    /// </remarks>
    /// <typeparam name="T">The type of the list elements.</typeparam>
    public class ListCellChanged<T> : RefCountedSafe
    {
        public ListChangeAction Action { get; private set; }
        public int NewStartingIndex { get; private set; }
        public IList<T> NewItems { get; private set; }
        public int OldStartingIndex { get; private set; }
        public int OldItemCount { get; private set; }


        /// <summary>
        /// Constructs a new instance of ListCellChanged.
        /// </summary>
        public ListCellChanged(ListChangeAction action, int newStartingIndex, IList<T> newItems, int oldStartingIndex, int oldItemCount)
        {
            this.Action = action;
            this.NewStartingIndex = newStartingIndex;
            this.NewItems = newItems;
            this.OldStartingIndex = oldStartingIndex;
            this.OldItemCount = oldItemCount;

            RefCountHelpers.AddRefRange(newItems);
        }

        protected override void Destroy()
        {
            RefCountHelpers.ReleaseRange(NewItems);
        }
    }
}
