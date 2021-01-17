using System.ComponentModel.DataAnnotations;

namespace JobFilter.Models
{
    public class FilterSetting
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "此欄位不能為空")]
        [StringLength(800, ErrorMessage = "最多800個字")]
        public string CrawlUrl { get; set; }

        [Required(ErrorMessage = "此欄位不能為空")]
        [RegularExpression(@"^[0-9''-'\d]{3,6}$", ErrorMessage = "必須為3~6個數字")]
        public int MinimumWage { get; set; }

        [Required(ErrorMessage = "此欄位不能為空")]
        [RegularExpression(@"^[0-9''-'\d]{3,7}$", ErrorMessage = "必須為3~7個數字")]
        public int MaximumWage { get; set; }

        [MaxLength(50, ErrorMessage = "最多50個字")]
        public string ExcludeWord { get; set; }

        [MaxLength(1500, ErrorMessage = "最多1500個字")]
        public string IgnoreCompany { get; set; }

        [MaxLength(5, ErrorMessage = "最多5個字")]
        public string Remarks { get; set; }

        public string UserId { get; set; }

        public string UserEmail { get; set; }
    }
}
