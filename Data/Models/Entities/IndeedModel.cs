﻿namespace WebScrappedBack.Models.Entities
{
    public class IndeedModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? JobTitle { get; set; }
        public string? Description { get; set; }
        public string? ContractType { get; set; }
        public string? Company { get; set; }
        public string? City { get; set; }
        public string? SalaryRange { get; set; }
        public string? URL { get; set; }
    }
}
