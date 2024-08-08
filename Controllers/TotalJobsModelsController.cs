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
    public class TotalJobsModelsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public TotalJobsModelsController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;

        }



        [HttpGet("{apiKey}")]
        public async Task<ActionResult<IEnumerable<TotalJobsModel>>> GetAllLinkedInModels(string apiKey)
        {
            try
            {
                // Retrieve all LinkedInModels
                var masterKey = _configuration["ApiKeys:MasterKey"];


                if (string.IsNullOrEmpty(apiKey) || apiKey != masterKey)
                {
                    return Unauthorized("Invalid or missing API key.");
                }


                var allModels = await _context.TotalJobsModels.ToListAsync();

                if (allModels == null || !allModels.Any())
                {
                    return NotFound("No TotalJobs models found.");
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

        // GET: api/TotalJobsModels/JobTitle/{jobTitle}/{limit}
        [HttpGet("JobTitle/{jobTitle}/{limit}/{startRow?}/{apiKey}")]
        public async Task<ActionResult<IEnumerable<TotalJobsModel>>> GetTotalJobsModelsByJobTitle(
        string jobTitle,
        int limit,
        int startRow ,
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

            // Check if the user exists and if they are subscribed to TotalJobs
            if (user == null)
            {
                return Unauthorized("Invalid API key.");
            }

            if (!user.TotalJobsSubscribed)
            {
                return Unauthorized("User is not subscribed to TotalJobs.");
            }

            // Calculate actual start row and end row
            int actualStartRow = (startRow - 1) * 100 + 1;
            int endRow = actualStartRow + 100 - 1;

            // Retrieve all data
            var allData = await _context.TotalJobsModels.ToListAsync();

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


        // POST: api/TotalJobsModels
        [HttpPost("{apiKey}")]
        public async Task<ActionResult<TotalJobsModel>> PostTotalJobsModel(TotalJobsModel totalJobsModel, string apiKey)
        {
            var masterKey = _configuration["ApiKeys:MasterKey"];

            // Validate the provided API key
            if (string.IsNullOrEmpty(apiKey) || apiKey != masterKey)
            {
                return Unauthorized("Invalid or missing API key.");
            }

            // Check for duplicates
            var existingModel = await _context.TotalJobsModels
                .Where(m => m.City == totalJobsModel.City
                            && m.Company == totalJobsModel.Company
                            && m.Description == totalJobsModel.Description)
                .FirstOrDefaultAsync();

            if (existingModel != null)
            {
                // If a duplicate is found, return a Conflict response
                return Conflict("A job listing with the same City, Company, and Description already exists.");
            }

            _context.TotalJobsModels.Add(totalJobsModel);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTotalJobsModelsByJobTitle", new { jobTitle = totalJobsModel.JobTitle, limit = 1 }, totalJobsModel);
        }
    }
}
