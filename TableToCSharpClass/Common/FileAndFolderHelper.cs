using System.IO;

namespace AdvExample1
{

    public class FileAndFolderHelper
    {
        public static FileAndFolderResult PickDirectory(string defaultFolder = @"C:\")
        {
            if (string.IsNullOrWhiteSpace(defaultFolder))
                defaultFolder = @"C:\";

            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return new FileAndFolderResult(true, dialog.SelectedPath);
            }

            return new FileAndFolderResult(false, string.Empty);
        }

        public static FileAndFolderResult PickLoadFile(string fileName = @"C:\File.txt")
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.InitialDirectory = string.IsNullOrEmpty(fileName) ?
            "c:\\" : Path.GetDirectoryName(fileName);
            dialog.FileName = fileName;

            if (dialog.ShowDialog() == true)
                return new FileAndFolderResult(false, dialog.FileName);

            return new FileAndFolderResult(false, string.Empty);
        }

        public static FileAndFolderResult PickSaveFile(string fileName = @"C:\File.txt")
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.InitialDirectory = string.IsNullOrEmpty(fileName) ?
                "c:\\" : Path.GetDirectoryName(fileName);
            dialog.FileName = fileName;

            if (dialog.ShowDialog() == true)
                return new FileAndFolderResult(false, dialog.FileName);

            return new FileAndFolderResult(false, string.Empty);
        }
    }
}
