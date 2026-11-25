using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrabajoFinalProgramacion3.Data;
using TrabajoFinalProgramacion3.DTOs;
using TrabajoFinalProgramacion3.Models;

namespace TrabajoFinalProgramacion3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ClientsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Create(CreateClientDTO clientDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var client = new Client
            {
                Name = clientDTO.Name,
                Email = clientDTO.Email
            };

            _context.Clients.Add(client);
            _context.SaveChanges();

            return Ok(client);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var clients = _context.Clients.ToList();
            return Ok(clients);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var client = _context.Clients.FirstOrDefault(c => c.Id == id);
            if (client == null)
            {
                return NotFound($"No se encontró el cliente con ID {id}");
            }
            return Ok(client);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, UpdateClientDTO clientDTO)
        {
            var client = _context.Clients.FirstOrDefault(c => c.Id == id);
            if (client == null)
            {
                return NotFound($"No se encontró el cliente con ID {id}");
            }

            if (!string.IsNullOrEmpty(clientDTO.Name))
            {
                client.Name = clientDTO.Name;
            }

            if (!string.IsNullOrEmpty(clientDTO.Email))
            {
                client.Email = clientDTO.Email;
            }

            _context.SaveChanges();
            return Ok(client);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var client = _context.Clients.FirstOrDefault(c => c.Id == id);
            if (client == null)
            {
                return NotFound($"No se encontró el cliente con ID {id}");
            }

            var hasTransactions = _context.Transactions.Any(t => t.ClientId == id);
            if (hasTransactions)
            {
                return BadRequest("No se puede eliminar el cliente porque tiene transacciones asociadas.");
            }

            _context.Clients.Remove(client);
            _context.SaveChanges();

            return NoContent();
        }
    }
}