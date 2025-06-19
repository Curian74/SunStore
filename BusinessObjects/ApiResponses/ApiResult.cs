
namespace BusinessObjects.ApiResponses
{
    public class ApiResult<T> : BaseApiResponse
    {
        public T? Data { get; set; }
    }
}
