using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Micro_social_app.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext>
        options)
        : base(options)
        {
        }
        

    }
}
