using System.ComponentModel.DataAnnotations;
namespace AdminPanel3.Models;

public class ProductModel
{
    public int? ProductID { get; set; }

    [Required(ErrorMessage = "Product Name is required.")]
    [StringLength(100, ErrorMessage = "Product Name cannot be longer than 100 characters.")]
    public string ProductName { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Product Price must be a positive number.")]
    public double ProductPrice { get; set; }

    [StringLength(50, ErrorMessage = "Product Code cannot be longer than 50 characters.")]
    public string ProductCode { get; set; }

    [StringLength(500, ErrorMessage = "Description cannot be longer than 500 characters.")]
    public string Description { get; set; }

    [Required(ErrorMessage = "User is required.")]
    public int UserID { get; set; }
}

public class UserDropDownModel
{
    public int UserID { get; set; }

    public string UserName { get; set; }

}


