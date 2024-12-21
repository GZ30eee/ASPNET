using System;
using System.ComponentModel.DataAnnotations;

namespace AdminPanel3.Models
{
    public class CustomerModel
    {
        
        public int? CustomerID { get; set; }

        [Required(ErrorMessage = "Customer name is required.")]
        [StringLength(100, ErrorMessage = "Customer name cannot exceed 100 characters.")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Home address is required.")]
        [StringLength(200, ErrorMessage = "Home address cannot exceed 200 characters.")]
        public string HomeAddress { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mobile number is required.")]
        [Phone(ErrorMessage = "Invalid mobile number.")]
        [StringLength(15, ErrorMessage = "Mobile number cannot exceed 15 characters.")]
        public string MobileNo { get; set; }

        [StringLength(15, ErrorMessage = "GST number cannot exceed 15 characters.")]
        public string GSTNo { get; set; }

        [Required(ErrorMessage = "City name is required.")]
        [StringLength(50, ErrorMessage = "City name cannot exceed 50 characters.")]
        public string CityName { get; set; }

        [Required(ErrorMessage = "Pin code is required.")]
        [StringLength(10, ErrorMessage = "Pin code cannot exceed 10 characters.")]
        public string PinCode { get; set; }

        [Required(ErrorMessage = "Net amount is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Net amount must be a non-negative value.")]
        public double NetAmount { get; set; }

        [Required(ErrorMessage = "User is required.")]
        public int UserID { get; set; }
    }

    /*public class CustomerDropDownModel
    {
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }
    }*/
}
