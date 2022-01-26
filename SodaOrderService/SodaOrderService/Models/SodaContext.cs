using Microsoft.EntityFrameworkCore;

namespace SodaOrderService.Models
{
    public class SodaContext : DbContext
    {
        public SodaContext(DbContextOptions<SodaContext> options)
            : base(options)
        {
        }

        public DbSet<SodaOrder> SodaOrders { get; set; } = null!;

    }
}
