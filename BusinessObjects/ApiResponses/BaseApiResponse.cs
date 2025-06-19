namespace BusinessObjects.ApiResponses
{
    public class BaseApiResponse
    {
        public bool IsSuccessful { get; set; } = false;
        public string? Message { get; set; }
    }
}
