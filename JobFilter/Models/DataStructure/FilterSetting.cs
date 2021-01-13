using System.ComponentModel.DataAnnotations;

namespace JobFilter.Models
{
    public class FilterSetting
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "此欄位不能為空")]
        [StringLength(800, ErrorMessage = "最多只能輸入800個字元!")]
        public string CrawlUrl { get; set; }

        [Required(ErrorMessage = "此欄位不能為空")]
        [RegularExpression(@"^[0-9''-'\d]{3,6}$", ErrorMessage = "輸入內容必須為3~6個數字")]
        public int MinimumWage { get; set; }

        [Required(ErrorMessage = "此欄位不能為空")]
        [RegularExpression(@"^[0-9''-'\d]{3,7}$", ErrorMessage = "輸入內容必須為3~7個數字")]
        public int MaximumWage { get; set; }

        [MaxLength(50, ErrorMessage = "最多只能輸入50個字元!")]
        public string ExcludeWord { get; set; }

        [MaxLength(1000, ErrorMessage = "最多只能輸入1000個字元!")]
        public string IgnoreCompany { get; set; }

        [MaxLength(5, ErrorMessage = "最多只能輸入5個字元!")]
        public string Remarks { get; set; }

        public string UserId { get; set; }

        public string UserEmail { get; set; }
    }
}
