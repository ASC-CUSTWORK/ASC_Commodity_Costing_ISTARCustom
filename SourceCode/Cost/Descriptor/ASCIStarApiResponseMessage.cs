namespace ASCISTARCustom.Cost.Descriptor
{
    public class ASCIStarApiResponseMessage
    {
        public const string Error = "Error";
        public const string Success = "Success";
        public const string Warning = "Warning";
        public string Status { get; set; }
        public string Message { get; set; }
        public decimal Price { get; set; }
    }
}
