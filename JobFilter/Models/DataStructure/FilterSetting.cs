﻿using System.ComponentModel.DataAnnotations;

namespace JobFilter.Models
{
    public class FilterSetting
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "此欄位不能為空")]
        [StringLength(800, ErrorMessage = "最多800個字")]
        public string CrawlUrl { get; set; }

        [Required(ErrorMessage = "此欄位不能為空")]
        [RegularExpression(@"^[0-9''-'\d]{5,6}$", ErrorMessage = "必須為5~6個數字")]
        public int MinimumWage { get; set; }

        [Required(ErrorMessage = "此欄位不能為空")]
        [RegularExpression(@"^[0-9''-'\d]{5,6}$", ErrorMessage = "必須為5~6個數字")]
        public int MaximumWage { get; set; }

        [MaxLength(50, ErrorMessage = "最多50個字")]
        public string ExcludeWord { get; set; }

        [MaxLength(3000, ErrorMessage = "最多3000個字")]
        public string IgnoreCompany { get; set; }

        [MaxLength(5, ErrorMessage = "最多5個字")]
        public string Remarks { get; set; }

        public string UserEmail { get; set; }
    }
}
