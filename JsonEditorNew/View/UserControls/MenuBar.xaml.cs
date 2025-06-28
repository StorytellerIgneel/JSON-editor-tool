using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;


namespace JsonEditorNew.View.UserControls
{
    /// <summary>
    /// Interaction logic for MenuBar.xaml
    /// </summary>
    
   
    public partial class MenuBar : UserControl
    {
        private JObject jsonData;
        private JToken selectedToken;
        private List<KeyValuePairEditable> keyValuePairs = new List<KeyValuePairEditable>();
        private String lastSavedFilePath = string.Empty; 
        public event Action<JToken?> LoadJsonArray;
        public event Action<List<KeyValuePairEditable>?> LoadJsonObject;

        public MenuBar()
        {
            InitializeComponent();
        }

        public void OpenJson_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Filter = "JSON files (*.json)|*.json" };
            if (dialog.ShowDialog() == true)
            {
                string jsonText = File.ReadAllText(dialog.FileName);
                selectedToken = JToken.Parse(jsonText);

                lastSavedFilePath = dialog.FileName; // Store the last saved file path
                // Handle JArray, no need serialize
                if (selectedToken is JArray array)
                    LoadJsonArray?.Invoke(selectedToken);
                //handle JObject, serialize to KVPE
                else if (selectedToken is JObject obj)
                {
                    // Handle JSON object
                    var items = obj.Properties().Select(p => new KeyValuePairEditable
                    {
                        Key = p.Name,
                        Value = p.Value.ToString(Newtonsoft.Json.Formatting.None)
                    }).ToList();
                    LoadJsonObject?.Invoke(items);
                    keyValuePairs = items; // Store for saving later
                }
                else if (selectedToken is JValue val)
                {
                    var items = new List<KeyValuePairEditable>
                    {
                        new KeyValuePairEditable
                        {
                            Key = "Value",
                            Value = val.ToString(Newtonsoft.Json.Formatting.None)
                        }
                    };
                    LoadJsonObject?.Invoke(items); // Treat single value as an array for consistency
                }
                else
                    LoadJsonArray?.Invoke(null); // Handle unexpected token types
            }
            else
                return; // User canceled the dialog
        }

        public void SaveAsJson_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog { Filter = "JSON files (*.json)|*.json" };
            if (dialog.ShowDialog() == true)
            {
                string json = convertToJson();
                File.WriteAllText(dialog.FileName, json);
                MessageBox.Show("JSON saved as new file name!");
            }
        }

        public void SaveJson_Click(object sender, RoutedEventArgs e)
        {
            string json = convertToJson();
            File.WriteAllText(lastSavedFilePath, json);
            MessageBox.Show("JSON saved!");
        }

        private void ExitApp_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private String convertToJson()
        {
            string json;

            if (selectedToken is JArray array)
                json = array.ToString(Newtonsoft.Json.Formatting.None);
            else if (selectedToken is JObject)
            {
                var writeObject = new JObject();
                foreach (var pair in keyValuePairs)
                    writeObject[pair.Key] = JToken.Parse(pair.Value);
                json = writeObject.ToString(Newtonsoft.Json.Formatting.None);
            }
            else if (selectedToken is JValue val)
                json = val.ToString(Newtonsoft.Json.Formatting.None);
            else
                json = selectedToken.ToString(Newtonsoft.Json.Formatting.None);
            return json;
        }
    }
}