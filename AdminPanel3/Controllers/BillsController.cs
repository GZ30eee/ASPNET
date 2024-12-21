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
    public class BillsController : Controller
    {

        private IConfiguration configuration;

        public BillsController(IConfiguration _configuration)
        {
            configuration = _configuration;
        }
        public IActionResult BillsList()
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PR_Bills_SelectAll";
            SqlDataReader reader = command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(reader);
            return View(table);

        }

        public IActionResult BillsForm(int BillID)
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");

            #region Order Drop-Down

            SqlConnection connection1 = new SqlConnection(connectionString);
            connection1.Open();
            SqlCommand command1 = connection1.CreateCommand();
            command1.CommandType = System.Data.CommandType.StoredProcedure;
            command1.CommandText = "PR_Order_DropDown";
            SqlDataReader reader1 = command1.ExecuteReader();
            DataTable dataTable1 = new DataTable();
            dataTable1.Load(reader1);
            connection1.Close();

            List<OrderDropDownModel> orders = new List<OrderDropDownModel>();

            foreach (DataRow order in dataTable1.Rows)
            {
                OrderDropDownModel orderDropDownModel = new OrderDropDownModel();

                // Handle OrderID with DBNull check
                orderDropDownModel.OrderID = Convert.IsDBNull(order["OrderID"])
                    ? 0 // Use a default value or handle null as needed
                    : Convert.ToInt32(order["OrderID"]);

                // Handle OrderNumber with DBNull check
                orderDropDownModel.OrderNumber = Convert.IsDBNull(order["OrderNumber"])
                    ? 0 // Use a default value or handle null as needed
                    : Convert.ToInt32(order["OrderNumber"]);

                orders.Add(orderDropDownModel);
            }


            ViewBag.OrderList = orders;

            #endregion


            #region User Drop-Down

            SqlConnection connection2 = new SqlConnection(connectionString);
            connection2.Open();
            SqlCommand command2 = connection2.CreateCommand();
            command2.CommandType = System.Data.CommandType.StoredProcedure;
            command2.CommandText = "PR_User_DropDown";
            SqlDataReader reader2 = command2.ExecuteReader();
            DataTable dataTable2 = new DataTable();
            dataTable2.Load(reader2);
            connection2.Close();

            List<UserDropDownModel> users = new List<UserDropDownModel>();

            foreach (DataRow user in dataTable2.Rows)
            {
                UserDropDownModel userDropDownModel = new UserDropDownModel();
                userDropDownModel.UserID = Convert.ToInt32(@user["UserID"]);
                userDropDownModel.UserName = @user["UserName"].ToString();
                users.Add(userDropDownModel);
            }

            ViewBag.UserList = users;

            #endregion

            

            #region BillByID

            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PR_Bills_SelectByPK";
            command.Parameters.AddWithValue("@BillID", BillID);
            SqlDataReader reader = command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(reader);
            BillsModel billModel = new BillsModel();

            foreach (DataRow bill in table.Rows)
            {
                billModel.BillID = Convert.ToInt32(@bill["BillID"]);
                billModel.BillNumber =Convert.ToString(@bill["BillNumber"]);
                billModel.BillDate = Convert.ToDateTime(@bill["BillDate"]);
                billModel.OrderID = Convert.ToInt32(@bill["OrderID"]);
                billModel.TotalAmount = Convert.ToDouble(@bill["TotalAmount"]);
                billModel.Discount = Convert.ToDouble(@bill["Discount"]);
                billModel.NetAmount = Convert.ToDouble(@bill["NetAmount"]);
                billModel.UserID = Convert.ToInt32(@bill["UserID"]);
            }
            #endregion

            OrderDropDown();
            UserDropDown();
            return View("BillsForm",billModel);
        }

        [HttpPost]
        public IActionResult BillSave(BillsModel billmodel)
        {
            if (billmodel.UserID <= 0)
            {
                ModelState.AddModelError("UserID", "A valid user is required.");
            }

            if (ModelState.IsValid)
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

                            if (billmodel.BillID == null)
                            {
                                command.CommandText = "PR_Bills_Insert";
                            }
                            else
                            {
                                command.CommandText = "PR_Bills_UpdateByPK";
                                command.Parameters.Add("@BillID", SqlDbType.Int).Value = billmodel.BillID;
                            }

                            command.Parameters.Add("@BillNumber", SqlDbType.VarChar).Value = string.IsNullOrEmpty(billmodel.BillNumber) ? (object)DBNull.Value : billmodel.BillNumber;
                            command.Parameters.Add("@BillDate", SqlDbType.DateTime).Value = billmodel.BillDate == default(DateTime) ? (object)DBNull.Value : billmodel.BillDate;
                            command.Parameters.Add("@OrderID", SqlDbType.Int).Value = billmodel.OrderID;
                            command.Parameters.Add("@TotalAmount", SqlDbType.Decimal).Value = billmodel.TotalAmount;
                            command.Parameters.Add("@Discount", SqlDbType.Decimal).Value = billmodel.Discount;
                            command.Parameters.Add("@NetAmount", SqlDbType.Decimal).Value = billmodel.NetAmount;
                            command.Parameters.Add("@UserID", SqlDbType.Int).Value = billmodel.UserID;

                            command.ExecuteNonQuery();
                        }
                    }

                    TempData["SuccessMessage"] = "Bill saved successfully.";
                    return RedirectToAction("BillsList");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An error occurred while saving the bill: " + ex.Message;
                }
            }
            // Re-populate dropdowns if validation fails
            OrderDropDown();
            UserDropDown();
            return View("BillsForm", billmodel);
        }

        public IActionResult BillDelete(int BillID)
        {
            try
            {
                string connectionString = this.configuration.GetConnectionString("ConnectionString");
                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "PR_Bills_DeleteByPK";
                command.Parameters.Add("@BillID", SqlDbType.Int).Value = BillID;
                command.ExecuteNonQuery();

                TempData["SuccessMessage"] = "Bill deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the bill: " + ex.Message;
            }

            return RedirectToAction("BillsList");
        }

        public void OrderDropDown()
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection1 = new SqlConnection(connectionString);
            connection1.Open();
            SqlCommand command1 = connection1.CreateCommand();
            command1.CommandType = CommandType.StoredProcedure;
            command1.CommandText = "PR_Order_DropDown";
            SqlDataReader reader1 = command1.ExecuteReader();
            DataTable dataTable1 = new DataTable();
            dataTable1.Load(reader1);
            List<OrderDropDownModel> orderList = new List<OrderDropDownModel>();
            foreach (DataRow data in dataTable1.Rows)
            {
                OrderDropDownModel orderDropDownModel = new OrderDropDownModel();

                // Check if OrderID is DBNull before converting
                if (data["OrderID"] != DBNull.Value)
                {
                    orderDropDownModel.OrderID = Convert.ToInt32(data["OrderID"]);
                }
                else
                {
                    // Handle DBNull as needed, e.g., set a default value or leave it unset
                    orderDropDownModel.OrderID = 0; // Default value or handle accordingly
                }

                // Check if OrderNumber is DBNull before converting
                if (data["OrderNumber"] != DBNull.Value)
                {
                    orderDropDownModel.OrderNumber = Convert.ToInt32(data["OrderNumber"]);
                }
                else
                {
                    // Handle DBNull as needed, e.g., set a default value or leave it unset
                    orderDropDownModel.OrderNumber = 0; // Default value or handle accordingly
                }

                orderList.Add(orderDropDownModel);
            }

            ViewBag.OrderList = orderList;
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

            DataTable dataTable = GetBillsData();

            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Bills");

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
                string fileName = "Bills.xlsx";
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(stream, contentType, fileName);
            }
        }

        private DataTable GetBillsData()
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();


                using (SqlCommand sqlCommand = connection.CreateCommand())
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.CommandText = "PR_Bills_SelectAll";


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
            DataTable dataTable = GetBillsDataPdf();

            using (MemoryStream stream = new MemoryStream())
            {
                // Initialize PDF writer
                PdfWriter writer = new PdfWriter(stream);
                PdfDocument pdfDoc = new PdfDocument(writer);
                Document document = new Document(pdfDoc);

                // Add title
                document.Add(new Paragraph("Bills List").SetTextAlignment(TextAlignment.CENTER).SetFontSize(18));

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

                return File(stream.ToArray(), "application/pdf", "Bills.pdf");
            }
        }

        private DataTable GetBillsDataPdf()
        {
            // Your existing code to fetch data
            string connectionString = this.configuration.GetConnectionString("ConnectionString");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand sqlCommand = connection.CreateCommand())
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.CommandText = "PR_Bills_SelectAll";

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
