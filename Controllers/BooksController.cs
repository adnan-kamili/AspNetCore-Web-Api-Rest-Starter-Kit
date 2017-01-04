using SampleApi.Models;
using SampleApi.Repository;

namespace SampleApi.Controllers
{
    public class BooksController : BaseController<Book>
    {
        public BooksController(IRepository repository) : base(repository) { }
    }
}
