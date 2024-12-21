using System;
using System.ComponentModel.DataAnnotations;

namespace AdminPanel3.Models
{
    public class OrderModel
    {
        public int? OrderID { get; set; }

        [Required(ErrorMessage ="OrderNumber field is required")]
        public int OrderNumber { get; set; }

        [Required(ErrorMessage = "Order date is required.")]
        public DateTime OrderDate { get; set; }

        [Required(ErrorMessage = "Customer is required.")]
        public int CustomerID { get; set; }

        [Required(ErrorMessage = "Payment mode is required.")]
        [StringLength(50, ErrorMessage = "Payment mode cannot exceed 50 characters.")]
        public string PaymentMode { get; set; }

        [Required(ErrorMessage = "Total amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total amount must be greater than zero.")]
        public double? TotalAmount { get; set; }

        [Required(ErrorMessage = "Shipping address is required.")]
        [StringLength(200, ErrorMessage = "Shipping address cannot exceed 200 characters.")]
        public string ShippingAddress { get; set; }

        [Required(ErrorMessage = "User is required.")]
        public int UserID { get; set; }
    }
    public class CustomerDropDownModel
    {
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }
    }

    /*public class UserDropDownModel
    {
        public int UserID { get; set; }

        public string UserName { get; set; }

    }*/
}
