using Microsoft.EntityFrameworkCore;

namespace MyStore.Response
{
    public class PagedResponse<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int TotalItems { get; set; }
        public int Page {  get; set; }
        public int PageSize { get; set; }
        public int TotalPages =>(int)Math.Ceiling((double)TotalItems / Page);
    }
}
