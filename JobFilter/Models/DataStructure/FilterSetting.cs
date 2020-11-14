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

        [StringLength(100, ErrorMessage = "最多只能輸入100個字元!")]
        public string ExcludeWord { get; set; }

        [StringLength(300, ErrorMessage = "最多只能輸入300個字元!")]
        public string IgnoreCompany { get; set; }

        [StringLength(10, ErrorMessage = "最多只能輸入10個字元!")]
        public string Remarks { get; set; }

        public string UserId { get; set; }

        public string UserEmail { get; set; }
    }
}
