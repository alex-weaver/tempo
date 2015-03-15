# Tutorial - Todo List

In this tutorial we'll create a Todo list application using WPF for the UI. The application
will allow items to be added and deleted from the list, the text of an item to be edited and
items can be checked off as done. This application demonstrates a simple master/detail view,
and introduces a couple of concepts:

* The WithEach operator for introducing a scope for list elements.
* Creating a record class which contains one or more memory cells.


## Setting up the project

Follow the steps in the [Quickstart](./quickstart.md) guide to get a skeleton project
up and running, then we can start building our UI.

## Create the UI

The UI will be divided into two panes: on the left hand side will be a list of items in the
todo list, along with buttons to add and delete elements. Filling the rest of the view will
be the *detail* view to edit a todo item. The detail view will contain two controls, a TextBox
to edit the text of the item, and a checkbox to flag items as done.
Replace the default Grid element in MainWindow.xaml with the following code:

```
<DockPanel LastChildFill="True">
    <DockPanel LastChildFill="True">
        <Grid DockPanel.Dock="Bottom" Margin="8,0,8,8">
            <Button x:Name="addButton" Content="Add" Width="80" Height="36" HorizontalAlignment="Left" />
            <Button x:Name="deleteButton" Content="Delete" Width="80" Height="36" HorizontalAlignment="Right" />
        </Grid>
        <ListView x:Name="masterList" Width="200" Margin="0,0,0,16"></ListView>
    </DockPanel>
    <Grid x:Name="detailView" Margin="10">
        <TextBlock Text="Item text:" Margin="0,20,0,0" Width="66" TextAlignment="Right" HorizontalAlignment="Left" VerticalAlignment="Top" />
        <TextBox x:Name="textField" Margin="70,16,10,0" Height="26" VerticalAlignment="Top" />

        <TextBlock Text="Done?" Margin="0,66,0,0" Width="66" TextAlignment="Right" HorizontalAlignment="Left" VerticalAlignment="Top" />
        <CheckBox x:Name="isDoneCheckbox" Margin="69,60,0,0" HorizontalAlignment="Left" VerticalAlignment="Top">
            <CheckBox.LayoutTransform>
                <ScaleTransform ScaleX="2" ScaleY="2" />
            </CheckBox.LayoutTransform>
        </CheckBox>
    </Grid>
</DockPanel>
```

## Create the TodoListItem Class

Add a new class to the project called TodoListItem. This will represent a single item in
the list. Each item needs to store the text of the item, and a flag indicating whether the
item is done. The text will be represented by a string cell, and the done flag will be a bool
cell. Since this class contains cells, it should implement IRefCounted, so that the lifetime
of the cells can be tracked. The implementations of AddRef and Release can simply forward the
calls on to the cells. Adding the cells and the IRefCounted implementation, we end up with this:

```
public class TodoListItem : IRefCounted
{
    public MemoryCell<string> text;
    public MemoryCell<bool> isDone;

    public TodoListItem()
    {
        this.text = new MemoryCell<string>("");
        this.isDone = new MemoryCell<bool>(false);
    }

    public void AddRef()
    {
        text.AddRef();
        isDone.AddRef();
    }

    public void Release()
    {
        text.Release();
        isDone.Release();
    }
}
```


## Construct the main cells

At the top level, the essential state of the application requires a list of TodoListItem
instances, plus an integer to track which item is currently selected. This will determine
which item (if any) is modified by the controls in the detail view. Add the following to
the WhileRunning method:

```
var mainList = new ListCell<TodoListItem>();
var selectedItem = new MemoryCell<int>(-1);
```

A value of -1 in the selectedItem value will be used to indicate that nothing is selected.

## Create window bindings

We need to observe a number of interface properties: the state of the add and delete buttons,
the contents of the text field, the state of the checkbox and the selected index of the listview.
Add the following to WhileRunning to map each of these properties onto a cell:

```
var addButtonPressed = PropertyAdapter.Read<bool>(window.addButton, Button.IsPressedProperty);
var deleteButtonPressed = PropertyAdapter.Read<bool>(window.deleteButton, Button.IsPressedProperty);
var textField = PropertyAdapter.Read<string>(window.textField, TextBox.TextProperty);
var isDoneField = PropertyAdapter.Read<bool>(window.isDoneCheckbox, CheckBox.IsCheckedProperty);
var uiSelection = PropertyAdapter.Read<int>(window.masterList, ListView.SelectedIndexProperty);
```

## Implementing add and delete

Before we can edit or display the list of todo items, we need a way to add new ones. We can
use ICellRead<bool>.WhenTrue to trigger an action whenever the add button transitions from
'not pressed' to
'pressed'. In the add handler, we will create a new blank todo list item, and add it to the
list. To make the user experience smoother, we will also change the selection to the new
item. We need to defer execution of the selection until another logical step, because changing
the list contents may also modify the selection, so we need to make sure our change happens
*after* any implicit change. The code to do this is as follows:

```
addButtonPressed.WhenTrue(() =>
{
    var newIndex = mainList.Add(new TodoListItem());
    Events.Once(() => selectedItem.Cur = newIndex);
});
```

The implementation of delete follows a similar pattern. We use WhenTrue to trigger an
imperative block when the button is pressed, and in the body of the block we remove
the currently selected item from the list. The only thing we need to check is whether
the selection is empty (represented by -1) or not valid (refers to an index past the
end of the list).

```
deleteButtonPressed.WhenTrue(() =>
{
    if (selectedItem.Cur >= 0 && selectedItem.Cur < mainList.Count)
    {
        mainList.RemoveAt(selectedItem.Cur);
    }
});
```

Adding this code will implement the complete functionality for the two buttons, however
the changes they make to the application state will be invisible since changes to the
list cell are not reflected in the list view. We cover this in the next section.

## Dispaying the list items

This is perhaps the most important part: displaying the todo list contents in the list view.
We will use the data stored in the list cell to generate a list of strings to display in
the view. This generated list will automatically be kept up to date with any changes to
the data model. The output strings will always include the item's text, and if the item
is marked as done, the text will be followed by a check mark in square brackets.

Since we need to observe cells held within each element of the source list, we will need
a continuous scope to observe them in. `IListCellRead<T>.WithEach` constructs a scope for each element
of a list, which lasts until that element is removed, replaced or the list is destroyed.
The body of WithEach must return an ICellRead for each element of the list, which allows
us to handle the case where a modification to a member of TodoListItem, such as the isDone
cell, can cause the output list to change even though the source list itself has not changed.

If we only wanted to display the item's text, and ignore the value of isDone, we could write
the following:

    var listText = mailList.WithEach(item => item.text);

This would construct an `IListCellRead<string>` which would update both when items are added
and removed, and when any item's text cell changes. To incorporate the value of isDone into
the result, we can use CellBuilder.Merge to construct a new cell based on the text and isDone
cells. This new cell would be updated whenever either of the source cells changed. The final
version of the above line looks like this:

```
var listText = mainList.WithEach(item =>
    CellBuilder.Merge(item.text, item.isDone, (text, isDone) => text + (isDone ? " [✔]" : "")));
```

Each time any element's text or isDone cell changes, the lambda in the second line is
re-evaluated. If isDone is true, a check mark in square brackets is appended to the text,
otherwise the empty string is appended to the text. Displaying this new list of strings
in the ListView can easily be done using SelectorBinding:

    SelectorBinding.Bind(listText, window.masterList);

If you run the application now, you should see that the add and delete buttons will add and
remove empty list items. To check it's working properly, you can manually add some test data
to mainList at the beginning of WhileRunning, and verify that it shows up in the list.

The last thing missing from the ListView is a binding for the selection. Currently, when
the user changes the selection in the list, the *selectedItem* cell is not updated, and
vice versa. We can use `ICellRead<T>.Latest` to watch for the selectedItem cell changing, and update
the list view, and we can use the Bind method to read from the uiSelection variable set up earlier
and keep the selecteItem cell updated:

```
// Bind the ui selection to the model's representation of the current selection. Apply changes in both directions.
selectedItem.Latest(value => window.masterList.SelectedIndex = value);
uiSelection.Bind(selectedItem);
```

Now, running the application and repeatedly clicking 'add' should select the last element
of the list, rather than previously where the selection didn't change.

## Implementing the detail view

Next, we implement the functionality of the ui controls on the right hand side of the screen.
This will broadly comprise three parts: Firstly, the controls should be enabled if and only
if the selection is valid. Next, the controls should be updated when the selection changes.
Finally, when the content of the TextBox or the status of the CheckBox are modified, the
changes should be mirrored in the model.

First, we can simplify these operations by creating a cell which contains the currently
selected list item, or null if the selection is empty. We can do this with the ElementAt
extension method of IListCellRead:

    var curItem = mainList.ElementAt(selectedItem);

We can disable all the detail controls by changing the value of window.detailView.IsEnabled.
The following line project the contents of curItem to a boolean cell, which holds true iff
the contents of curItem is non-null, and updates the window.detailView.IsEnabled property
whenever this boolean cell changes:

    curItem.Select(x => x != null).Latest(value => window.detailView.IsEnabled = value);

Next, we follow a similar pattern for updating the contents of the fields when the
selection changes. We use Select to construct cells which contain either the desired
value if the selection is non-null, or a default value. Then we use `ICellRead<T>.Latest` to update
the field contents when these derived cells change:

```
// When the selection changes, update contents of the text field and the CheckBox
curItem.Select(x => x == null ? null : x.text.Cur).Latest(value => window.textField.Text = value);
curItem.Select(x => x == null ? false : x.isDone.Cur).Latest(value => window.isDoneCheckbox.IsChecked = value);
```

Finally, we can use `ICellRead<T>.Lastest` to observe when the value of the fields changes. If the
selection is non-null, we update the selection with the new values:

```
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
```

## Wrapping up

This completes the application. Editing the detail fields should now update the list immediately.
Changes to the selection (either by clicking on list items, or by clicking add or delete) should
update the contents of the fields. The full source code for this tutorial is available in the
TodoListSample in the Examples directory of the repository.


