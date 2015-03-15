using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Tempo;
using Tempo.Wpf;

namespace MasterDetailSample
{
    public partial class App : Application
    {
        public App()
        {
            TempoApp.Init(this, true, WhileRunning);
        }

        private void WhileRunning()
        {
            // The two root data structures for the todo list application
            var mainList = new ListCell<TodoListItem>();
            var selectedItem = new MemoryCell<int>(-1);

            // Construct and show the window, and grab references to properties that will need to be observed
            var window = new MainWindow();
            window.Show();
            var addButtonPressed = PropertyAdapter.Read<bool>(window.addButton, Button.IsPressedProperty);
            var deleteButtonPressed = PropertyAdapter.Read<bool>(window.deleteButton, Button.IsPressedProperty);
            var textField = PropertyAdapter.Read<string>(window.textField, TextBox.TextProperty);
            var isDoneField = PropertyAdapter.Read<bool>(window.isDoneCheckbox, CheckBox.IsCheckedProperty);
            var uiSelection = PropertyAdapter.Read<int>(window.masterList, ListView.SelectedIndexProperty);


            // Handle clicks on add and delete
            addButtonPressed.WhenTrue(() =>
            {
                var newIndex = mainList.Add(new TodoListItem());
                Events.Once(() => selectedItem.Cur = newIndex);
            });

            deleteButtonPressed.WhenTrue(() =>
            {
                if (selectedItem.Cur >= 0 && selectedItem.Cur < mainList.Count)
                {
                    mainList.RemoveAt(selectedItem.Cur);
                }
            });


            // Bind content to the list view
            var listText = mainList.WithEach(item =>
                CellBuilder.Merge(item.text, item.isDone, (text, isDone) => text + (isDone ? " [✔]" : "")));

            SelectorBinding.Bind(listText, window.masterList);

            // Bind the ui selection to the model's representation of the current selection. Apply changes in both directions.
            selectedItem.Latest(value => window.masterList.SelectedIndex = value);
            uiSelection.Bind(selectedItem);


            var curItem = mainList.ElementAt(selectedItem);

            // Disable the detail view if the selection is empty
            curItem.Select(x => x != null).Latest(value => window.detailView.IsEnabled = value);

            // When the selection changes, update contents of the text field and the checkbox
            curItem.Select(x => x == null ? null : x.text.Cur).Latest(value => window.textField.Text = value);
            curItem.Select(x => x == null ? false : x.isDone.Cur).Latest(value => window.isDoneCheckbox.IsChecked = value);

            // When the text field changes, update the model
            textField.Latest(value =>
            {
                if (curItem.Cur != null)
                {
                    curItem.Cur.text.Cur = value;
                }
            });

            // When the checkbox changes, update the model
            isDoneField.Latest(value =>
            {
                if (curItem.Cur != null)
                {
                    curItem.Cur.isDone.Cur = value;
                }
            });
        }
    }
}
