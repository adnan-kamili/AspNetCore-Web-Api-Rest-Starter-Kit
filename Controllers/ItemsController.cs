using SampleApi.Models;
using SampleApi.Repository;

namespace SampleApi.Controllers
{
    public class ItemsController : BaseController<Item>
    {
        public ItemsController(IRepository repository) : base(repository) { }
    }
}
