namespace BusinessObjects.Queries
{
    public class UserQueryObject
    {
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int? Role {  get; set; } 
    }
}
