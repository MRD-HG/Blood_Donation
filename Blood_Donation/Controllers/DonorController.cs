using Microsoft.AspNetCore.Mvc;
using Blood_Donation.Models;
using Blood_Donation.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Blood_Donation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DonorController : ControllerBase
    {
        private readonly DonorContext _context;
        private readonly ILogger<DonorController> _logger;

        public DonorController(DonorContext context, ILogger<DonorController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET all donors
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Donor>>> GetAll()
        {
            try
            {
                var donors = await _context.Donors.ToListAsync();
                return Ok(donors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving donors");
                return StatusCode(500, new { message = "Error retrieving donors", error = ex.Message });
            }
        }

        // GET donor by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Donor>> GetById(int id)
        {
            try
            {
                var donor = await _context.Donors.FindAsync(id);
                if (donor == null)
                    return NotFound(new { message = $"Donor with ID {id} not found" });
                return Ok(donor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving donor with ID {DonorId}", id);
                return StatusCode(500, new { message = "Error retrieving donor", error = ex.Message });
            }
        }

        // POST new donor - FIXED VERSION
        [HttpPost]
        public async Task<ActionResult<Donor>> Create([FromBody] Donor donor)
        {
            try
            {
                // Log the incoming request
                _logger.LogInformation("Attempting to create new donor: {DonorName}", donor?.FullName ?? "Unknown");

                // Check if donor object is null
                if (donor == null)
                {
                    _logger.LogWarning("Received null donor object");
                    return BadRequest(new { message = "Donor data is required" });
                }

                // Log the received data for debugging
                _logger.LogInformation("Received donor data: Name={Name}, Phone={Phone}, BirthDate={BirthDate}, Gender={Gender}",
                    donor.FullName, donor.Phone, donor.BirthDate, donor.Gender);

                // Validate required fields manually (in case ModelState doesn't catch everything)
                var validationErrors = new List<string>();

                if (string.IsNullOrWhiteSpace(donor.FullName))
                    validationErrors.Add("Full name is required");

                if (string.IsNullOrWhiteSpace(donor.Phone))
                    validationErrors.Add("Phone number is required");

                if (donor.BirthDate == default(DateTime))
                    validationErrors.Add("Birth date is required");

                if (string.IsNullOrWhiteSpace(donor.Gender))
                    validationErrors.Add("Gender is required");

                if (validationErrors.Any())
                {
                    _logger.LogWarning("Validation errors: {Errors}", string.Join(", ", validationErrors));
                    return BadRequest(new { message = "Validation failed", errors = validationErrors });
                }

                // Check ModelState
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    _logger.LogWarning("ModelState validation failed: {Errors}", string.Join(", ", errors));
                    return BadRequest(new { message = "Model validation failed", errors = errors });
                }

                // Set default values if needed
                if (string.IsNullOrWhiteSpace(donor.GeneratedId))
                {
                    donor.GeneratedId = $"DN{DateTime.Now:yyyyMMddHHmmss}";
                    _logger.LogInformation("Generated ID for donor: {GeneratedId}", donor.GeneratedId);
                }

                // Ensure Age is calculated correctly
              

                // Ensure NumberDonation has a default value
                if (donor.NumberDonation < 0)
                    donor.NumberDonation = 0;

                // Try to add to database
                _logger.LogInformation("Adding donor to database context");
                _context.Donors.Add(donor);

                _logger.LogInformation("Saving changes to database");
                await _context.SaveChangesAsync();

                _logger.LogInformation("Donor created successfully with ID: {DonorId}", donor.IdDonor);

                return CreatedAtAction(nameof(GetById), new { id = donor.IdDonor }, donor);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error while creating donor");

                // Check for specific database errors
                if (dbEx.InnerException?.Message.Contains("duplicate key") == true)
                {
                    return Conflict(new { message = "A donor with this information already exists" });
                }

                return StatusCode(500, new
                {
                    message = "Database error occurred while creating donor",
                    error = dbEx.InnerException?.Message ?? dbEx.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating donor");
                return StatusCode(500, new
                {
                    message = "An unexpected error occurred while creating the donor",
                    error = ex.Message,
                    stackTrace = ex.StackTrace // Remove this in production
                });
            }
        }

        // PUT update donor
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Donor donor)
        {
            if (id != donor.IdDonor)
            {
                return BadRequest(new { message = "Donor ID mismatch" });
            }

            try
            {
                var existingDonor = await _context.Donors.FindAsync(id);
                if (existingDonor == null)
                {
                    return NotFound(new { message = $"Donor with ID {id} not found" });
                }

                // Update properties
                existingDonor.FullName = donor.FullName;
                existingDonor.Phone = donor.Phone;
                existingDonor.BirthDate = donor.BirthDate;
                existingDonor.Gender = donor.Gender;
                existingDonor.Address = donor.Address;
                existingDonor.NumberDonation = donor.NumberDonation;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Donor with ID {DonorId} updated successfully", id);
                return Ok(existingDonor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating donor with ID {DonorId}", id);
                return StatusCode(500, new { message = "Error updating donor", error = ex.Message });
            }
        }

        // DELETE donor
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var donor = await _context.Donors.FindAsync(id);
                if (donor == null)
                {
                    return NotFound(new { message = $"Donor with ID {id} not found" });
                }

                _context.Donors.Remove(donor);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Donor with ID {DonorId} deleted successfully", id);
                return Ok(new { message = "Donor deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting donor with ID {DonorId}", id);
                return StatusCode(500, new { message = "Error deleting donor", error = ex.Message });
            }
        }

        // Health check endpoint
        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            try
            {
                // Test database connection
                var canConnect = _context.Database.CanConnect();
                return Ok(new
                {
                    status = "healthy",
                    database = canConnect ? "connected" : "disconnected",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "unhealthy",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
}

