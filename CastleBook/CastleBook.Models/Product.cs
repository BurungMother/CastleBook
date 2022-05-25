using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastleBook.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string ISBN { get; set; }   
        [Required]
        public string Author { get; set; }
        [Required,Range(1,10000)]
        [Display(Name = "List Price")]
        public double Listprice { get; set; }
        [Required, Range(1,10000)]
        [Display(Name = "Price for 1-50 Items")]
        public double Price { get; set; }
        [Required, Range(1, 10000)]
        [Display(Name = "Price for 51-100 Items")]
        public double Price50 { get; set; }
        [Required, Range(1, 10000)]
        [Display(Name = "Price for 100+ Items")]
        public double Price100 { get; set; }
        [Required]
        [ValidateNever]
        public string ImageUrl{ get; set; }
        
        
        [Required]
        [Display(Name ="Category")]
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        [ValidateNever]
        public Category category { get; set; }

        [Required]
        
        public int CoverTypeId   { get; set; }
       
        [ValidateNever]
        public CoverType coverType { get; set; }
    }
}
