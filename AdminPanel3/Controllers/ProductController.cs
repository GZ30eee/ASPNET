using AdminPanel3.Models;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.Data.SqlClient;
using System.Data;
using iText.Layout;

public class ProductController : Controller
{
    private IConfiguration configuration;

    public ProductController(IConfiguration _configuration)
    {
        configuration = _configuration;
    }

    public IActionResult ProductList()
    {
        string connectionString = this.configuration.GetConnectionString("ConnectionString");
        SqlConnection connection = new SqlConnection(connectionString);
        connection.Open();
        SqlCommand command = connection.CreateCommand();
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = "PR_Product_SelectAll";
        SqlDataReader reader = command.ExecuteReader();
        DataTable table = new DataTable();
        table.Load(reader);

        

        return View(table);
    }


    public IActionResult ProductForm(int ProductID)
    {
        string connectionString = this.configuration.GetConnectionString("ConnectionString");

        #region User Drop-Down
        SqlConnection connection1 = new SqlConnection(connectionString);
        connection1.Open();
        SqlCommand command1 = connection1.CreateCommand();
        command1.CommandType = System.Data.CommandType.StoredProcedure;
        command1.CommandText = "PR_User_DropDown";
        SqlDataReader reader1 = command1.ExecuteReader();
        DataTable dataTable1 = new DataTable();
        dataTable1.Load(reader1);
        connection1.Close();

        List<UserDropDownModel> users = new List<UserDropDownModel>();
        foreach (DataRow user in dataTable1.Rows)
        {
            UserDropDownModel userDropDownModel = new UserDropDownModel();
            userDropDownModel.UserID = Convert.ToInt32(user["UserID"]);
            userDropDownModel.UserName = user["UserName"].ToString();
            users.Add(userDropDownModel);
        }
        ViewBag.UserList = users;
        #endregion

        #region ProductByID
        SqlConnection connection = new SqlConnection(connectionString);
        connection.Open();
        SqlCommand command = connection.CreateCommand();
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = "PR_Product_SelectByPK";
        command.Parameters.AddWithValue("@ProductID", ProductID);
        SqlDataReader reader = command.ExecuteReader();
        DataTable table = new DataTable();
        table.Load(reader);
        ProductModel productModel = new ProductModel();
        foreach (DataRow product in table.Rows)
        {
            productModel.ProductID = Convert.ToInt32(product["ProductID"]);
            productModel.ProductName = product["ProductName"].ToString();
            productModel.ProductCode = product["ProductCode"].ToString();
            productModel.ProductPrice = Convert.ToDouble(product["ProductPrice"]);
            productModel.Description = product["Description"].ToString();
            productModel.UserID = Convert.ToInt32(product["UserID"]);
        }
        #endregion
        UserDropDown();
        return View("ProductForm", productModel);
    }

    [HttpPost]
    public IActionResult ProductSave(ProductModel productmodel)
    {
        if (productmodel.UserID <= 0)
        {
            ModelState.AddModelError("UserID", "A valid user is required.");
        }

        if (ModelState.IsValid)
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                if (productmodel.ProductID == 0 || productmodel.ProductID == null)
                {
                    command.CommandText = "PR_Product_Insert";
                }
                else
                {
                    command.CommandText = "PR_Product_UpdateByPK";
                    command.Parameters.Add("@ProductID", SqlDbType.Int).Value = productmodel.ProductID;
                }
                command.Parameters.Add("@ProductName", SqlDbType.VarChar).Value = productmodel.ProductName;
                command.Parameters.Add("@ProductCode", SqlDbType.VarChar).Value = productmodel.ProductCode;
                command.Parameters.Add("@ProductPrice", SqlDbType.Decimal).Value = productmodel.ProductPrice;
                command.Parameters.Add("@Description", SqlDbType.VarChar).Value = productmodel.Description;
                command.Parameters.Add("@UserID", SqlDbType.Int).Value = productmodel.UserID;
                command.ExecuteNonQuery();

                // Set success message
                TempData["SuccessMessage"] = productmodel.ProductID == 0 ? "Product successfully created!" : "Product successfully updated!";
                return RedirectToAction("ProductList");
            }
        }

        UserDropDown();
        return View("ProductForm", productmodel);
    }

    public IActionResult ProductDelete(int ProductID)
    {
        try
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "PR_Product_DeleteByPK";
                command.Parameters.Add("@ProductID", SqlDbType.Int).Value = ProductID;
                command.ExecuteNonQuery();
            }

            // Set success message
            TempData["SuccessMessage"] = "Product successfully deleted!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            Console.WriteLine(ex.ToString());
        }
        return RedirectToAction("ProductList");
    }

    public void UserDropDown()
    {
        string connectionString = this.configuration.GetConnectionString("ConnectionString");
        using (SqlConnection connection1 = new SqlConnection(connectionString))
        {
            connection1.Open();
            SqlCommand command1 = connection1.CreateCommand();
            command1.CommandType = CommandType.StoredProcedure;
            command1.CommandText = "PR_User_DropDown";
            SqlDataReader reader1 = command1.ExecuteReader();
            DataTable dataTable1 = new DataTable();
            dataTable1.Load(reader1);
            List<UserDropDownModel> userList = new List<UserDropDownModel>();
            foreach (DataRow data in dataTable1.Rows)
            {
                UserDropDownModel userDropDownModel = new UserDropDownModel();
                userDropDownModel.UserID = Convert.ToInt32(data["UserID"]);
                userDropDownModel.UserName = data["UserName"].ToString();
                userList.Add(userDropDownModel);
            }
            ViewBag.UserList = userList;
        }
    }

    public ActionResult ExportToExcel()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        DataTable dataTable = GetProductsData();

        using (ExcelPackage package = new ExcelPackage())
        {
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Products");

            for (int col = 1; col <= dataTable.Columns.Count; col++)
            {
                worksheet.Cells[1, col].Value = dataTable.Columns[col - 1].ColumnName;
            }

            for (int row = 1; row <= dataTable.Rows.Count; row++)
            {
                for (int col = 1; col <= dataTable.Columns.Count; col++)
                {
                    worksheet.Cells[row + 1, col].Value = dataTable.Rows[row - 1][col - 1];
                }
            }

            var stream = new MemoryStream(package.GetAsByteArray());
            string fileName = "Products.xlsx";
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(stream, contentType, fileName);
        }
    }

    private DataTable GetProductsData()
    {
        string connectionString = this.configuration.GetConnectionString("ConnectionString");

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            using (SqlCommand sqlCommand = connection.CreateCommand())
            {
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.CommandText = "PR_Product_SelectAll";

                using (SqlDataReader reader = sqlCommand.ExecuteReader())
                {
                    DataTable dataTable = new DataTable();
                    dataTable.Load(reader);
                    return dataTable;
                }
            }
        }
    }

    public IActionResult ExportToPdf()
    {
        DataTable dataTable = GetProductsDataPdf();

        using (MemoryStream stream = new MemoryStream())
        {
            // Initialize PDF writer
            PdfWriter writer = new PdfWriter(stream);
            PdfDocument pdfDoc = new PdfDocument(writer);
            Document document = new Document(pdfDoc);

            // Add title
            document.Add(new Paragraph("Product List").SetTextAlignment(TextAlignment.CENTER).SetFontSize(18));

            // Create table with the same number of columns as the DataTable
            Table table = new Table(dataTable.Columns.Count, true);

            // Add headers
            foreach (DataColumn column in dataTable.Columns)
            {
                table.AddHeaderCell(new Cell().Add(new Paragraph(column.ColumnName)));
            }

            // Add data rows
            foreach (DataRow row in dataTable.Rows)
            {
                foreach (var cell in row.ItemArray)
                {
                    table.AddCell(new Cell().Add(new Paragraph(cell.ToString())));
                }
            }

            // Add table to document
            document.Add(table);

            // Close the document
            document.Close();

            return File(stream.ToArray(), "application/pdf", "Products.pdf");
        }
    }

    private DataTable GetProductsDataPdf()
    {
        string connectionString = this.configuration.GetConnectionString("ConnectionString");

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            using (SqlCommand sqlCommand = connection.CreateCommand())
            {
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.CommandText = "PR_Product_SelectAll";

                using (SqlDataReader reader = sqlCommand.ExecuteReader())
                {
                    DataTable dataTable = new DataTable();
                    dataTable.Load(reader);
                    return dataTable;
                }
            }
        }
    }
}
