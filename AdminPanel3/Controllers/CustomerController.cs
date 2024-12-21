using AdminPanel3.Models;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout;
using iText.IO.Image;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using System.Net.Mail;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.Data;
using System.Data.SqlClient;

namespace DatabaseSchema.Controllers
{
    public class CustomerController : Controller
    {
        private IConfiguration configuration;

        public CustomerController(IConfiguration _configuration)
        {
            configuration = _configuration;
        }
        public IActionResult CustomerList()
        {

            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PR_Customer_SelectAll";
            SqlDataReader reader = command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(reader);
            return View(table);
        }

        public IActionResult CustomerForm(int CustomerID)
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
                userDropDownModel.UserID = Convert.ToInt32(@user["UserID"]);
                userDropDownModel.UserName = @user["UserName"].ToString();
                users.Add(userDropDownModel);
            }

            ViewBag.UserList = users;

            #endregion

            #region CustomerByID

            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PR_Customer_SelectByPK";
            command.Parameters.AddWithValue("@CustomerID", CustomerID);
            SqlDataReader reader = command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(reader);
            CustomerModel customerModel = new CustomerModel();

            foreach (DataRow customer in table.Rows)
            {
                customerModel.CustomerID = Convert.ToInt32(@customer["CustomerID"]);
                customerModel.CustomerName = @customer["CustomerName"].ToString();
                customerModel.HomeAddress = @customer["HomeAddress"].ToString();
                customerModel.Email = @customer["Email"].ToString();
                customerModel.MobileNo = @customer["MobileNo"].ToString();
                customerModel.GSTNo = @customer["GSTNo"].ToString();
                customerModel.CityName = @customer["CityName"].ToString();
                customerModel.PinCode = @customer["PinCode"].ToString();
                customerModel.NetAmount = Convert.ToDouble(@customer["NetAmount"]);
                customerModel.UserID = Convert.ToInt32(@customer["UserID"]);
            }

            #endregion
            UserDropDown();
            return View("CustomerForm",customerModel);
        }
        [HttpPost]
        public IActionResult CustomerSave(CustomerModel customermodel)
        {
            if (customermodel.UserID <= 0)
            {
                ModelState.AddModelError("UserID", "A valid user is required.");
            }
            if (ModelState.IsValid)
            {
                string connectionString = this.configuration.GetConnectionString("ConnectionString");
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        if (customermodel.CustomerID == 0)
                        {
                            command.CommandText = "PR_Customer_Insert";
                        }
                        else
                        {
                            command.CommandText = "PR_Customer_UpdateByPK";
                            command.Parameters.Add("@CustomerID", SqlDbType.Int).Value = customermodel.CustomerID;
                        }
                        command.Parameters.Add("@CustomerName", SqlDbType.VarChar).Value = customermodel.CustomerName;
                        command.Parameters.Add("@HomeAddress", SqlDbType.VarChar).Value = customermodel.HomeAddress;
                        command.Parameters.Add("@Email", SqlDbType.VarChar).Value = customermodel.Email;
                        command.Parameters.Add("@MobileNo", SqlDbType.VarChar).Value = customermodel.MobileNo;
                        command.Parameters.Add("@GSTNo", SqlDbType.VarChar).Value = customermodel.GSTNo;
                        command.Parameters.Add("@CityName", SqlDbType.VarChar).Value = customermodel.CityName;
                        command.Parameters.Add("@PinCode", SqlDbType.VarChar).Value = customermodel.PinCode;
                        command.Parameters.Add("@NetAmount", SqlDbType.Decimal).Value = customermodel.NetAmount;
                        command.Parameters.Add("@UserID", SqlDbType.Int).Value = customermodel.UserID;
                        command.ExecuteNonQuery();
                    }
                }
                TempData["SuccessMessage"] = "Customer information has been saved successfully.";
                return RedirectToAction("CustomerList");
            }
            UserDropDown();
            return View("CustomerForm", customermodel);
        }

        public IActionResult CustomerDelete(int CustomerID)
        {
            try
            {
                string connectionString = this.configuration.GetConnectionString("ConnectionString");
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "PR_Customer_DeleteByPK";
                        command.Parameters.Add("@CustomerID", SqlDbType.Int).Value = CustomerID;
                        command.ExecuteNonQuery();
                    }
                }
                TempData["SuccessMessage"] = "Customer has been deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the customer: " + ex.Message;
                Console.WriteLine(ex.ToString());
            }
            return RedirectToAction("CustomerList");
        }

        public void UserDropDown()
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection1 = new SqlConnection(connectionString);
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

        public ActionResult ExportToExcel()
        {

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            DataTable dataTable = GetCustomersData();

            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Customers");

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
                string fileName = "Customers.xlsx";
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(stream, contentType, fileName);
            }
        }

        private DataTable GetCustomersData()
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();


                using (SqlCommand sqlCommand = connection.CreateCommand())
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.CommandText = "PR_Customer_SelectAll";


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
            DataTable dataTable = GetCustomersDataPdf();

            using (MemoryStream stream = new MemoryStream())
            {
                // Initialize PDF writer
                PdfWriter writer = new PdfWriter(stream);
                PdfDocument pdfDoc = new PdfDocument(writer);
                Document document = new Document(pdfDoc);

                // Add title
                document.Add(new Paragraph("Customer List").SetTextAlignment(TextAlignment.CENTER).SetFontSize(18));

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

                return File(stream.ToArray(), "application/pdf", "Customers.pdf");
            }
        }

        private DataTable GetCustomersDataPdf()
        {
            // Your existing code to fetch data
            string connectionString = this.configuration.GetConnectionString("ConnectionString");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand sqlCommand = connection.CreateCommand())
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.CommandText = "PR_Customer_SelectAll";

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
}
