using Newtonsoft.Json.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace JsonEditorNew
{
    public partial class MainWindow : Window
    {
        private String jsonMode = string.Empty;
        private List<KeyValuePairEditable> keyValuePairs = new List<KeyValuePairEditable>();

        public MainWindow()
        {
            InitializeComponent();
            //register shortcut

            var saveGesture = new KeyGesture(Key.S, ModifierKeys.Control);
            var saveCommand = new RoutedCommand();
            saveCommand.InputGestures.Add(saveGesture);
            CommandBindings.Add(new CommandBinding(saveCommand, menuBarControl.SaveJson_Click));

            var openGesture = new KeyGesture(Key.O, ModifierKeys.Control);
            var openCommand = new RoutedCommand();
            openCommand.InputGestures.Add(openGesture);
            CommandBindings.Add(new CommandBinding(openCommand, menuBarControl.OpenJson_Click));

            //subscribe to the LoadJsonRequested event from the MenuBar control
            menuBarControl.LoadJsonObject += OnJsonObjectLoaded;
            menuBarControl.LoadJsonArray += OnJsonArrayLoaded;
        }

        //JArray
        private void OnJsonArrayLoaded(JToken? token)
        {
            dataGrid.ItemsSource = token; // or token.Children(), or convert to list
            jsonMode = "array"; // Set mode to array
        }

        //JObject
        private void OnJsonObjectLoaded(List<KeyValuePairEditable> keyValuePairs)
        {
            jsonMode = "object"; // Set mode to object
            dataGrid.ItemsSource = keyValuePairs; // Bind the DataGrid to the list of KeyValuePairEditable
            this.keyValuePairs = keyValuePairs; // Store for saving later
        }

        private void dataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (!dataGrid.Columns.Any(c => c.Header?.ToString() == "Delete"))
            {
                DataGridTemplateColumn actionColumn = new DataGridTemplateColumn
                {
                    Header = jsonMode == "array" ? "Delete Record" : "Delete Property",
                    CellTemplate = (DataTemplate)this.Resources["DeleteButtonTemplate"]
                };
                dataGrid.Columns.Add(actionColumn);
            }
        }

        private void Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (MessageBox.Show($"Are you sure you want to delete this {(btn?.DataContext is JToken ? "object" : "property")} ?",
                "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                if (btn?.DataContext is JToken token)
                {
                    //Debug.WriteLine($"Deleting token: {token.ToString(Newtonsoft.Json.Formatting.None)}");
                    JToken parent = token.Parent;
                    //JSONDebug.Text = $"Parent: {parent?.ToString(Newtonsoft.Json.Formatting.None)}\nToken: {token.ToString(Newtonsoft.Json.Formatting.None)}";
                    if (parent is JArray array)
                    {
                        array.Remove(token); // remove directly if in array
                    }
                    else if (parent is JProperty prop)
                    {
                        prop.Remove(); // remove property if part of object
                    }

                    dataGrid.Items.Refresh(); // Refresh the DataGrid to reflect changes
                }
                else
                {
                    var item = btn?.DataContext as KeyValuePairEditable;
                    keyValuePairs.Remove(item);
                    dataGrid.Items.Refresh();
                }
                MessageBox.Show("Deleted successfully!");
            }
            else
                return;
        }


    }
}

public class KeyValuePairEditable
{
    public string Key { get; set; }
    public string Value { get; set; }
}
