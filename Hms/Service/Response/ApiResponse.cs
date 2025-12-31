namespace Hms.Service.Response
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int StatusCode { get; set; }
        public object? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public object? Response { get; set; }
        public bool Status { get; set; }
    }
}
