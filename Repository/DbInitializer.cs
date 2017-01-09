using System.Linq;
using SampleApi.Models;

namespace SampleApi.Repository
{
    public static class DbInitializer
    {
        public static void Initialize(IRepository repository)
        {
            
            repository.EnsureDatabaseCreated();
            // Look for any items.
            if (repository.Any<Item>())
            {
                return;   // DB has been seeded
            }

            var items = new Item[]
            {
                new Item{Name="Shirt",Cost=20,Color="Red"},
                new Item{Name="Coat",Cost=100,Color="Red"},
                new Item{Name="Trouser",Cost=15,Color="Red"}
            };
            foreach (Item i in items)
            {
                repository.Create(i);
            }
            repository.Save();
        }
    }
}