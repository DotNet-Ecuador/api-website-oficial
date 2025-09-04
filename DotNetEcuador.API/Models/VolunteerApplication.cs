using System;
using System.Collections.Generic;

namespace DotNetEcuador.API.Models
{
    public class VolunteerApplication
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public bool HasVolunteeringExperience { get; set; }
        public List<string> AreasOfInterest { get; set; } = new List<string>();
        public string OtherAreas { get; set; } = string.Empty;
        public string AvailableTime { get; set; } = string.Empty;
        public string SkillsOrKnowledge { get; set; } = string.Empty;
        public string WhyVolunteer { get; set; } = string.Empty;
        public string AdditionalComments { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

    }
}
