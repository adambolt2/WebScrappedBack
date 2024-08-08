namespace WebScrappedBack.Models.Entities
{
    public class PartialUpdateUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ApiKey { get; set; }
        public bool? IndeedSubscribed { get; set; }
        public bool? LinkedInSubscribed { get; set; }
        public bool? TotalJobsSubscribed { get; set; }
        public bool? isVerified { get; set; }
        public string Email { get; set; }
    }

}
