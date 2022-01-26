#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SodaOrderService.Models;


namespace SodaOrderService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SodaOrdersController : ControllerBase
    {
        private readonly SodaContext _context;

        public SodaOrdersController(SodaContext context)
        {
            _context = context;
        }

        // GET: api/SodaOrders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SodaOrder>>> GetTodoItems()
        {
            var storage = new OrderStorage();
            return storage.GetOrders();
        }

        // GET: api/SodaOrders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SodaOrder>> GetSodaOrder(long id)
        {
            var sodaOrder = await _context.SodaOrders.FindAsync(id);

            if (sodaOrder == null)
            {
                return NotFound();
            }

            return sodaOrder;
        }

        // PUT: api/SodaOrders/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSodaOrder(long id, SodaOrder sodaOrder)
        {
            if (id != sodaOrder.Id)
            {
                return BadRequest();
            }

            _context.Entry(sodaOrder).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SodaOrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/SodaOrders
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<SodaOrder>> PostSodaOrder(SodaOrder sodaOrder)
        {
            HandleLocalSodaStorage(ref sodaOrder);
            _context.SodaOrders.Add(sodaOrder);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSodaOrder), new { id = sodaOrder.Id }, sodaOrder);
        }

        private static SodaOrder HandleLocalSodaStorage(ref SodaOrder sodaOrder)
        {
            var storage = new OrderStorage();
            storage.AddOrderToStorage(ref sodaOrder);
            var sodaInventory = new SodaInventory();
            sodaInventory.ReserveSoda(sodaOrder.Soda);
            return sodaOrder;
        }

        // DELETE: api/SodaOrders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSodaOrder(long id)
        {
            var sodaOrder = await _context.SodaOrders.FindAsync(id);
            if (sodaOrder == null)
            {
                return NotFound();
            }

            _context.SodaOrders.Remove(sodaOrder);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SodaOrderExists(long id)
        {
            return _context.SodaOrders.Any(e => e.Id == id);
        }
    }
}
