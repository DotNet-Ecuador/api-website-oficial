using System;
using System.Collections.Generic;

namespace api.Models
{
    public class VolunteerApplication
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public bool HasVolunteeringExperience { get; set; }
        public Dictionary<string, bool> AreasOfInterest { get; set; } = new Dictionary<string, bool>();
        public string OtherAreas { get; set; } = string.Empty;
        public string AvailableTime { get; set; } = string.Empty;
        public string SkillsOrKnowledge { get; set; } = string.Empty;
        public string WhyVolunteer { get; set; } = string.Empty;
        public string AdditionalComments { get; set; } = string.Empty;

        public bool ValidateOtherAreas()
        {
            if (AreasOfInterest.ContainsKey("Other") && string.IsNullOrWhiteSpace(OtherAreas))
            {
                return false;
            }
            return true;
        }
    }
}
