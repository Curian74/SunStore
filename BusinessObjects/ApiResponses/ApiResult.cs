namespace BusinessObjects.ApiResponses
{
    public class ApiResult
    {
        public bool IsSuccessful { get; set; } = false;
        public string? Message { get; set; }
    }
}
