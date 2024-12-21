using System;
using System.ComponentModel.DataAnnotations;

namespace AdminPanel3.Models
{
    public class OrderDetailModel
    {
      
        public int? OrderDetailID { get; set; }

        [Required(ErrorMessage = "Order Number is required.")]
        public int OrderID { get; set; }

        [Required(ErrorMessage = "Product is required.")]
        public int ProductID { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public double Amount { get; set; }

        [Required(ErrorMessage = "Total amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total amount must be greater than zero.")]
        public double TotalAmount { get; set; }

        [Required(ErrorMessage = "User is required.")]
        public int UserID { get; set; }
    }


    public class OrderDropDownModel
    {
        public int OrderID { get; set; }

        public int OrderNumber { get; set; }

    }

    public class ProductDropDownModel
    {
        public int ProductID { get; set; }

        public string ProductName { get; set; }

    }
}
