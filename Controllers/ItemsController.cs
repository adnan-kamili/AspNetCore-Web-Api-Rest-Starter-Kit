using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SampleApi.Repository;
using SampleApi.Models;

namespace SampleApi.Controllers
{
    public class ItemsController : BaseController<Item>
    {
        public ItemsController(IRepository repository) : base(repository) { }
    }
}
