using System.ComponentModel.DataAnnotations;

namespace Blood_Donation.Models
{
    public class Donor
    {
        [Key]
        public int IdDonor { get; set; }
        [Required]
        public string? FullName { get; set; }
        [Required]
        public string? Phone { get; set; }
   
        public DateTime BirthDate { get; set; }
        public int Age => DateTime.Now.Year - BirthDate.Year -
                          (DateTime.Now.DayOfYear < BirthDate.DayOfYear ? 1 : 0);
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? GeneratedId { get; set; }
        public int NumberDonation { get; set; }
    }
}
