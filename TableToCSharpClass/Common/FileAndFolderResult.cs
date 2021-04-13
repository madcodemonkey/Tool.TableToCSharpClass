namespace AdvExample1
{
    public class FileAndFolderResult
    {
        public FileAndFolderResult(bool success, string value)
        {
            Success = success;
            Value = value;
        }

        public bool Success { get; set; }
        public string Value { get; set; }
    }
}
