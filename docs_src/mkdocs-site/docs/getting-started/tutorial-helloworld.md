# Tutorial - Hello World

In this tutorial we'll create a simple WPF application with a single button. Each time the
button is clicked, a counter is incremented and a string is appended to a list. Not tremendously
exciting, but it covers some important basics:

* Setting up a new WPF project
* Value and list cells
* Binding to WPF dependency properties


## Setting up the project

Follow the steps in the [Quickstart](./quickstart.md) guide to get a skeleton project
up and running, then we can start building our UI.

## Create the UI

The UI needs three main widgets: a Button to handle click events, a TextBlock to display the
current count and and a ListView to display the list of strings. We'll put these in a simple
layout with the Button and TextBlock at the top, with the ListView filling the remainder of
the window. Replace the default Grid element in MainWindow.xaml with the following code:

```
<DockPanel LastChildFill="True">
    <StackPanel Height="30" VerticalAlignment="Top" DockPanel.Dock="Top" Orientation="Horizontal">
        <Button x:Name="button" Content="Click me" Width="100" />
        <TextBlock x:Name="label" Margin="10,6,0,0" />
    </StackPanel>
    <ListView x:Name="listView" />
</DockPanel>
```

## Create cells and window bindings

The current state of the application will be maintained by two cells: a value cell holding an
integer for the current count, and a list cell containing string elements.

Change WhileRunning to include the two extra lines below:

```
private void WhileRunning()
{
    var count = new MemoryCell<int>(0);
    var list = new ListCell<string>();

    var window = new MainWindow();
    window.Show();
}
```

To detect when the button is clicked, we will need to read the IsPressed property of the
button widget. We will also need to be able to write to the Text property of the TextBlock
widget to display the count. We will use the PropertyAdapter class to generate cell interfaces
for those properties:

```
private void WhileRunning()
{
    var count = new MemoryCell<int>(0);
    var list = new ListCell<string>();

    var window = new MainWindow();
    window.Show();
    var buttonPressed = PropertyAdapter.Read<bool>(window.button, Button.IsPressedProperty);
    var labelWrite = PropertyAdapter.Write<string>(window.label, TextBlock.TextProperty);
}
```

## Displaying the count

To display the count on the screen, we need to project the count cell onto a string.
We can use the Select extension method of `ICellRead<T>` as follows:

    var labelText = count.Select(x => x.ToString() + " clicks");

Now we can create a binding that will constrain the labelWrite cell to always be equal
to the value of labelText. This will keep the TextBlock's Text property up to date whenever
the value of *count* changes.

    labelText.Bind(labelWrite);

Running the application should show the label displaying "0 clicks".

## Updating the count

Changing the count cell requires triggering an imperative block. We can use the *When*
operator to invoke a block when the buttonPressed cell transitions from false to true.

```
buttonPressed.WhenTrue(() =>
{
    count.Cur++;
});
```

Now clicking the button should increment the count.

## Implementing the list

Finally, clicking the button needs to add an element to the list cell, and a binding needs
to be set up to display the contents of the list cell in the list view. To handle adding
an element to the list, we can just modify the existing button click handler to call
list.Add(). `IListCellRead<T>` has an extension method `Bind` for binding to
an IList, so we can use this to bind to the Items property of the ListView. Including these
two new lines, the bindings and button handler should look like this:

```
var labelText = count.Select(x => x.ToString() + " clicks");
labelText.Bind(labelWrite);

list.Bind(window.listView.Items);

buttonPressed.WhenTrue(() =>
{
	count.Cur++;
	list.Add("Hello World");
});
```

You can check out the full code for this tutorial in the HelloWorldSample project in the
Examples directory of the repository.
