namespace DummyProject.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; } = string.Empty;
        public bool ShowRequestId { get; set; } = false;
        public string ErrorMessage { get; set; } = string.Empty;
        public bool HasRequestId => !string.IsNullOrEmpty(RequestId);
    }
}