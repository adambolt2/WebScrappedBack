using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebScrappedBack.Models.Entities;
using WebScrappedBack.Services;

namespace WebScrappedBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService; // Inject the email service
        private readonly IConfiguration _configuration;

        public UsersController(ApplicationDbContext context, IEmailService emailService, IConfiguration configuration)
        {

            _context = context;
            _emailService = emailService; // Initialize the email service
            _configuration = configuration;

        }

        private bool IsValidApiKey(string apiKey)
        {
            var masterKey = _configuration["ApiKeys:MasterKey"];

            return apiKey == masterKey;
        }

        // GET: api/Users/{apiKey}
        [HttpGet("{apiKey}")]
        public async Task<ActionResult<IEnumerable<Users>>> GetUsers(string apiKey)
        {
            if (!IsValidApiKey(apiKey))
            {
                return Unauthorized("Invalid API key.");
            }

            return await _context.Users.ToListAsync();
        }

        // GET: api/Users/email@example.com/password123/{apiKey}
        [HttpGet("{email}/{password}/{apiKey}")]
        public async Task<ActionResult> GetUserByEmailAndPassword(string email, string password, string apiKey)
        {
            if (!IsValidApiKey(apiKey))
            {
                return Unauthorized("Invalid API key.");
            }

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);

            if (user == null || user.Password != password)
            {
                return Unauthorized();
            }

            if (!user.isVerified)
            {
                return Unauthorized("User is not verified.");
            }

            return Ok(new
            {
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.ApiKey,
                user.IndeedSubscribed,
                user.LinkedInSubscribed,
                user.TotalJobsSubscribed,
                user.isVerified
            });
        }

        [HttpPut("{email}/{apiKey}")]
        public async Task<IActionResult> PutUserByEmail(string email, [FromBody] PartialUpdateUser partialUser, string apiKey)
        {
            if (!IsValidApiKey(apiKey))
            {
                return Unauthorized("Invalid API key.");
            }

            if (email != partialUser.Email)
            {
                return BadRequest();
            }

            var existingUser = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (existingUser == null)
            {
                return NotFound();
            }

            // Update only the fields provided in the request
            existingUser.FirstName = partialUser.FirstName ?? existingUser.FirstName;
            existingUser.LastName = partialUser.LastName ?? existingUser.LastName;
            existingUser.ApiKey = partialUser.ApiKey ?? existingUser.ApiKey;
            existingUser.IndeedSubscribed = partialUser.IndeedSubscribed ?? existingUser.IndeedSubscribed;
            existingUser.LinkedInSubscribed = partialUser.LinkedInSubscribed ?? existingUser.LinkedInSubscribed;
            existingUser.TotalJobsSubscribed = partialUser.TotalJobsSubscribed ?? existingUser.TotalJobsSubscribed;
            existingUser.isVerified = partialUser.isVerified ?? existingUser.isVerified;

            _context.Entry(existingUser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(email))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok();
        }

        // POST: api/Users/{apiKey}
        [HttpPost("{apiKey}")]
        public async Task<ActionResult<Users>> PostUser([FromBody] Users user, string apiKey)
        {
            if (!IsValidApiKey(apiKey))
            {
                return Unauthorized("Invalid API key.");
            }

            if (UserExists(user.Email))
            {
                return Conflict(new { message = "Email already exists" });
            }

            user.VerificationCode = GenerateVerificationCode(); // Generate and set the verification code
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Send verification email with the code
            string emailBody = $"Your verification code is: {user.VerificationCode}";
            await _emailService.SendVerificationEmailAsync(user.Email, "Please verify your email", emailBody);

            return CreatedAtAction(nameof(GetUserByEmailAndPassword), new { email = user.Email, password = user.Password, apiKey = apiKey }, user);
        }

        // POST: api/Users/verify/{apiKey}
        [HttpPost("verify/{apiKey}")]
        public async Task<IActionResult> VerifyUser([FromBody] VerificationRequest request, string apiKey)
        {
            if (!IsValidApiKey(apiKey))
            {
                return Unauthorized("Invalid API key.");
            }

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (user.VerificationCode == request.VerificationCode)
            {
                user.isVerified = true;
                user.VerificationCode = string.Empty; // Clear the verification code after successful verification
                await _context.SaveChangesAsync();
                return Ok("User successfully verified.");
            }
            else
            {
                return BadRequest("Invalid verification code.");
            }
        }

        // POST: api/Users/request-verification-code/{apiKey}
        [HttpPost("request-verification-code/{apiKey}")]
        public async Task<IActionResult> RequestVerificationCode([FromBody] RequestVerificationCodeRequest request, string apiKey)
        {
            if (!IsValidApiKey(apiKey))
            {
                return Unauthorized("Invalid API key.");
            }

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Generate and set a new verification code
            user.VerificationCode = GenerateVerificationCode();
            await _context.SaveChangesAsync();

            // Send the new verification code to the user via email
            string emailBody = $"Your new verification code is: {user.VerificationCode}";
            await _emailService.SendVerificationEmailAsync(user.Email, "New Verification Code", emailBody);

            return Ok("New verification code has been sent.");
        }

        // DELETE: api/Users/{email}/{apiKey}
        [HttpDelete("{email}/{apiKey}")]
        public async Task<IActionResult> DeleteUserByEmail(string email, string apiKey)
        {
            if (!IsValidApiKey(apiKey))
            {
                return Unauthorized("Invalid API key.");
            }

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(string email)
        {
            return _context.Users.Any(e => e.Email == email);
        }

        private string GenerateVerificationCode()
        {
            // Generate a unique verification code, you can adjust the format as needed
            return Guid.NewGuid().ToString("N").Substring(0, 8);
        }
    }

    public class VerificationRequest
    {
        public string Email { get; set; }
        public string VerificationCode { get; set; }
    }

    public class RequestVerificationCodeRequest
    {
        public string Email { get; set; }
    }
}
