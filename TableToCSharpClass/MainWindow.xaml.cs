using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace AdvExample1
{
    public partial class MainWindow : Window
    {
        protected CancellationTokenSource _DbTablesToFilesCancelToken = null;

        public MainWindow()
        {
            InitializeComponent();
            LogMessage("Enter a connection string and hit the 'Do Work' button.  You will be prompted for a directory were the POCO files will be deposited.");
        }

        private async void DoWorkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_DbTablesToFilesCancelToken != null)
                {
                    _DbTablesToFilesCancelToken.Cancel();
                    return;
                }

                string myConnectionString = TbConnectionString.Text;
                if (string.IsNullOrWhiteSpace(myConnectionString))
                {
                    throw new ArgumentException("Please put a connection string in the SQL Connection text box.");
                }

                string namespaceName = string.IsNullOrWhiteSpace(TbNamespaceName.Text) ? "SampleNamespace" : TbNamespaceName.Text;

                var saveDirectory = FileAndFolderHelper.PickDirectory();
                if (saveDirectory.Success == false)
                    return;

                DoWorkButton.Content = "Stop Work";
                _DbTablesToFilesCancelToken = new CancellationTokenSource();

                await Task.Run(() =>
                    {
                        DbTablesToFiles(saveDirectory.Value, myConnectionString, namespaceName);
                    }, _DbTablesToFilesCancelToken.Token)
                    .ContinueWith((t) =>
                    {
                        if (t.IsFaulted)
                        {
                            LogError(t.Exception);
                        }
                        else
                        {
                            if (_DbTablesToFilesCancelToken.IsCancellationRequested)
                                LogMessage("Cancelled Requested.");
                            else LogMessage("Finished");
                        }

                        DoWorkButton.Content = "Do Work";
                        _DbTablesToFilesCancelToken = null;

                    }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }


        private void DbTablesToFiles(string directoryName, string myConnectionString, string namespaceName)
        {
            LogMessage("Press the button again to stop creating files.");

            using (var destinationConnection = new SqlConnection(myConnectionString))
            {
                destinationConnection.Open();
                var newHelper = new DatabaseTableHelper(destinationConnection);
                foreach (DatabaseTable table in newHelper.LoadTableNames())
                {
                    if (_DbTablesToFilesCancelToken.IsCancellationRequested)
                    {
                        break;
                    }

                    // Load table fields
                    var tableNameWithSchema = table.ToString();
                    var tableFields = newHelper.LoadFields(tableNameWithSchema);

                    // Create class in memory
                    string someClass = newHelper.ConvertFieldsToClass(tableFields, table.TableName, namespaceName);

                    // Save class to file system
                    string fileName = Path.Combine(directoryName, $"{table.TableName}.cs");
                    using (FileStream fs = File.Create(fileName))
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.WriteLine(someClass);
                    }
                }
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearLog();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveLog();
        }

        #region Logging
        private delegate void NoArgsDelegate();
        private void ClearLog()
        {
            if (Dispatcher.CheckAccess())
            {
                RtbLog.Document.Blocks.Clear();
            }
            else this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new NoArgsDelegate(ClearLog));
        }

        /// <summary>Threadsafe logging method.</summary>
        private void LogMessage(string message)
        {
            if (Dispatcher.CheckAccess())
            {
                var p = new Paragraph(new Run(message));
                p.Foreground = Brushes.Black;
                RtbLog.Document.Blocks.Add(p);
            }
            else this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action<string>(LogMessage), message);
        }

        private void LogError(Exception ex)
        {
            if (Dispatcher.CheckAccess())
            {
                // We are back on the UI thread here so calling LogMessage will not cause a BeginInvoke for all these LogMessage calls:
                LogMessage(ex.Message);
                LogMessage(ex.StackTrace);
                if (ex.InnerException != null)
                {
                    LogMessage(ex.InnerException.Message);
                    LogMessage(ex.InnerException.StackTrace);
                }
            }
            else this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action<Exception>(LogError), ex);
        }

        private void SaveLog()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            if (dialog.ShowDialog() != true)
                return;

            using (var fs = new FileStream(dialog.FileName, FileMode.Create))
            {
                var myTextRange = new TextRange(RtbLog.Document.ContentStart, RtbLog.Document.ContentEnd);
                myTextRange.Save(fs, DataFormats.Text);
            }
        }
        #endregion

    }
}
