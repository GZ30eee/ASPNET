using System;
using System.ComponentModel.DataAnnotations;

namespace AdminPanel3.Models
{
    public class BillsModel
    {
       
        public int? BillID { get; set; }

        [Required(ErrorMessage = "Bill number is required")]
        [StringLength(20, ErrorMessage = "Bill number cannot exceed 20 characters")]
        public string BillNumber { get; set; }

        [Required(ErrorMessage = "Bill date is required")]
        [DataType(DataType.Date)]
        public DateTime? BillDate { get; set; }

        [Required(ErrorMessage = "Order Number is required")]
        public int OrderID { get; set; }

        [Required(ErrorMessage = "Total amount is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Total amount must be a positive value")]
        public double TotalAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Discount must be a positive value")]
        public double? Discount { get; set; }

        [Required(ErrorMessage = "Net amount is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Net amount must be a positive value")]
        public double NetAmount { get; set; }

        [Required(ErrorMessage = "User is required")]
        public int UserID { get; set; }
    }
}
