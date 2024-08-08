using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebScrappedBack.Models.Entities;

namespace WebScrappedBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IndeedModelsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;


        public IndeedModelsController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        [HttpGet("{apiKey}")]
        public async Task<ActionResult<IEnumerable<IndeedModel>>> GetAllLinkedInModels(string apiKey)
        {
            try
            {

                var masterKey = _configuration["ApiKeys:MasterKey"];


                if (string.IsNullOrEmpty(apiKey) || apiKey != masterKey)
                {
                    return Unauthorized("Invalid or missing API key.");
                }
                // Retrieve all LinkedInModels
                var allModels = await _context.IndeedModels.ToListAsync();

                if (allModels == null || !allModels.Any())
                {
                    return NotFound("No Indeed models found.");
                }

                return Ok(allModels);
            }
            catch (Exception ex)
            {
                // Log the exception (if you have a logger)
                // _logger.LogError(ex, "Error occurred while fetching all LinkedIn models.");

                return StatusCode(500, "Internal server error occurred.");
            }
        }

        [HttpGet("JobTitle/{jobTitle}/{limit}/{startRow?}/{apiKey}")]
        public async Task<ActionResult<IEnumerable<IndeedModel>>> GetIndeedModelsByJobTitle(
       string jobTitle,
       int limit,
       int startRow,
       string apiKey)
        {
            // Validate startRow to be between 1 and 9
            if (startRow < 1 || startRow > 9)
            {
                return BadRequest("startRow must be between 1 and 9.");
            }

            // Ensure limit is within the maximum allowed
            if (limit > 100)
            {
                limit = 100;
            }

            // Check if API key is provided
            if (string.IsNullOrEmpty(apiKey))
            {
                return BadRequest("API key is required.");
            }

            // Retrieve the user associated with the API key
            var user = await _context.Users.SingleOrDefaultAsync(u => u.ApiKey == apiKey);

            // Check if the user exists and if they are subscribed to Indeed
            if (user == null)
            {
                return Unauthorized("Invalid API key.");
            }

            if (!user.IndeedSubscribed)
            {
                return Unauthorized("User is not subscribed to Indeed.");
            }

            // Calculate actual start row and end row
            int actualStartRow = (startRow - 1) * 100 + 1;
            int endRow = actualStartRow + 100 - 1;

            // Retrieve all data
            var allData = await _context.IndeedModels.ToListAsync();

            // Trim it down to only the start and end row
            var trimmedData = allData.Skip(actualStartRow - 1).Take(endRow - actualStartRow + 1);

            // Filter the results by JobTitle using Contains for partial match
            var filteredData = trimmedData.Where(m => m.JobTitle.Contains(jobTitle, StringComparison.OrdinalIgnoreCase));

            // Limit the number of results to the specified limit
            var limitedResults = filteredData.Take(limit).ToList();

            if (limitedResults == null || !limitedResults.Any())
            {
                return NotFound("Sorry, no results found.");
            }

            return Ok(limitedResults);
        }


        // POST: api/IndeedModels
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("{apiKey}")]
        public async Task<ActionResult<IndeedModel>> PostIndeedModel(IndeedModel indeedModel, string apiKey)
        {

            var masterKey = _configuration["ApiKeys:MasterKey"];

            // Validate the provided API key
            if (string.IsNullOrEmpty(apiKey) || apiKey != masterKey)
            {
                return Unauthorized("Invalid or missing API key.");
            }

            // Check if the job entry already exists in the database
            bool exists = await _context.IndeedModels
                .AnyAsync(m => m.City == indeedModel.City &&
                               m.Company == indeedModel.Company &&
                               m.Description == indeedModel.Description);

            if (exists)
            {
                return Conflict("A job listing with the same City, Company, and Description already exists.");
            }

            // Add the new job entry
            _context.IndeedModels.Add(indeedModel);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetIndeedModelsByJobTitle",
                                    new { jobTitle = indeedModel.JobTitle, limit = 1 },
                                    indeedModel);
        }

    }
}
