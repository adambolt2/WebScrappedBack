namespace WebScrappedBack.Models.Entities
{
    public class LinkedInModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? JobTitle { get; set; }
        public string? Description { get; set; }
        public string? DatePosted { get; set; }
        public string? SeniorityLevel { get; set; }
        public string? EmploymentType { get; set; }
        public string? Company { get; set; }
        public string? City { get; set; }
        public string? URL { get; set; }
    }
}
