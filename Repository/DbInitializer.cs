using SampleApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace SampleApi.Repository
{
    public static class DbInitializer
    {
        public static void Initialize(IRepository repository)
        {
            repository.EnsureDatabaseCreated();

            if (repository.Any<Tenant>())
            {
                return;
            }
        }
    }
}