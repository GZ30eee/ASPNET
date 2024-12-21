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
    public class OrderController : Controller
    {

        private IConfiguration configuration;

        public OrderController(IConfiguration _configuration)
        {
            configuration = _configuration;
        }
        public IActionResult OrderList()
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PR_Order_SelectAll";
            SqlDataReader reader = command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(reader);
            return View(table);
        }


        public IActionResult OrderForm(int OrderID)
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");

            #region Customer Drop-Down

            SqlConnection connection1 = new SqlConnection(connectionString);
            connection1.Open();
            SqlCommand command1 = connection1.CreateCommand();
            command1.CommandType = System.Data.CommandType.StoredProcedure;
            command1.CommandText = "PR_Customer_DropDown";
            SqlDataReader reader1 = command1.ExecuteReader();
            DataTable dataTable1 = new DataTable();
            dataTable1.Load(reader1);
            connection1.Close();

            List<CustomerDropDownModel> customers = new List<CustomerDropDownModel>();

            foreach (DataRow customer in dataTable1.Rows)
            {
                CustomerDropDownModel customerDropDownModel = new CustomerDropDownModel();
                customerDropDownModel.CustomerID = Convert.ToInt32(@customer["CustomerID"]);
                customerDropDownModel.CustomerName = @customer["CustomerName"].ToString();
                customers.Add(customerDropDownModel);
            }

            ViewBag.CustomerList = customers;

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

            #region OrderByID

            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PR_Order_SelectByPK";
            command.Parameters.AddWithValue("@OrderID", OrderID);
            SqlDataReader reader = command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(reader);
            OrderModel orderModel = new OrderModel();

            foreach (DataRow order in table.Rows)
            {
                orderModel.OrderID = Convert.ToInt32(@order["OrderID"]);
                orderModel.OrderNumber = Convert.ToInt32(@order["OrderNumber"]);
                orderModel.OrderDate = Convert.ToDateTime(@order["OrderDate"]);
                orderModel.CustomerID = Convert.ToInt32(@order["CustomerID"]);
                orderModel.PaymentMode = @order["PaymentMode"].ToString();
                orderModel.TotalAmount = Convert.ToDouble(@order["TotalAmount"]);
                orderModel.ShippingAddress = @order["ShippingAddress"].ToString();
                orderModel.UserID = Convert.ToInt32(@order["UserID"]);
            }
            #endregion
            CustomerDropDown();
            UserDropDown();


            return View("OrderForm",orderModel);
        }

        [HttpPost]

        public IActionResult OrderSave(OrderModel ordermodel)
        {
            if (ordermodel.CustomerID <= 0 || ordermodel.UserID <= 0)
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
                        if (ordermodel.OrderID == 0 || ordermodel.OrderID == null)
                        {
                            command.CommandText = "PR_Order_Insert";
                            TempData["SuccessMessage"] = "Order successfully created!";
                        }
                        else
                        {
                            command.CommandText = "PR_Order_UpdateByPK";
                            command.Parameters.Add("@OrderID", SqlDbType.Int).Value = ordermodel.OrderID;
                            TempData["SuccessMessage"] = "Order successfully updated!";
                        }

                        command.Parameters.Add("@OrderNumber", SqlDbType.Int).Value = ordermodel.OrderNumber;
                        command.Parameters.Add("@OrderDate", SqlDbType.DateTime).Value = ordermodel.OrderDate;
                        command.Parameters.Add("@CustomerID", SqlDbType.Int).Value = ordermodel.CustomerID;
                        command.Parameters.Add("@PaymentMode", SqlDbType.VarChar).Value = ordermodel.PaymentMode;
                        command.Parameters.Add("@TotalAmount", SqlDbType.Float).Value = ordermodel.TotalAmount;
                        command.Parameters.Add("@ShippingAddress", SqlDbType.VarChar).Value = ordermodel.ShippingAddress;
                        command.Parameters.Add("@UserID", SqlDbType.Int).Value = ordermodel.UserID;

                        command.ExecuteNonQuery();
                    }
                }

                // Redirect to the OrderList view with success message
                return RedirectToAction("OrderList");
            }

            // Reload dropdowns and show validation errors
            CustomerDropDown();
            UserDropDown();

            return View("OrderForm", ordermodel);
        }

        public IActionResult OrderDelete(int OrderID)
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
                        command.CommandText = "PR_Order_DeleteByPK";
                        command.Parameters.Add("@OrderID", SqlDbType.Int).Value = OrderID;
                        command.ExecuteNonQuery();
                    }
                }

                // Set success message
                TempData["SuccessMessage"] = "Order successfully deleted!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                Console.WriteLine(ex.ToString());
            }

            return RedirectToAction("OrderList"); 
        }

        public void CustomerDropDown()
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection1 = new SqlConnection(connectionString);
            connection1.Open();
            SqlCommand command1 = connection1.CreateCommand();
            command1.CommandType = CommandType.StoredProcedure;
            command1.CommandText = "PR_Customer_DropDown";
            SqlDataReader reader1 = command1.ExecuteReader();
            DataTable dataTable1 = new DataTable();
            dataTable1.Load(reader1);
            List<CustomerDropDownModel> customerList = new List<CustomerDropDownModel>();
            foreach (DataRow customer in dataTable1.Rows)
            {
                CustomerDropDownModel customerDropDownModel = new CustomerDropDownModel();
                customerDropDownModel.CustomerID = Convert.ToInt32(@customer["CustomerID"]);
                customerDropDownModel.CustomerName = @customer["CustomerName"].ToString();
                customerList.Add(customerDropDownModel);
            }
            ViewBag.CustomerList = customerList;
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
            foreach (DataRow user in dataTable1.Rows)
            {
                UserDropDownModel userDropDownModel = new UserDropDownModel();
                userDropDownModel.UserID = Convert.ToInt32(@user["UserID"]);
                userDropDownModel.UserName = @user["UserName"].ToString();
                userList.Add(userDropDownModel);
            }
            ViewBag.UserList = userList;
        }

        public ActionResult ExportToExcel()
        {

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            DataTable dataTable = GetOrdersData();

            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Orders");

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
                string fileName = "Orders.xlsx";
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(stream, contentType, fileName);
            }
        }

        private DataTable GetOrdersData()
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();


                using (SqlCommand sqlCommand = connection.CreateCommand())
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.CommandText = "PR_Order_SelectAll";


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
            DataTable dataTable = GetOrdersDataPdf();

            using (MemoryStream stream = new MemoryStream())
            {
                // Initialize PDF writer
                PdfWriter writer = new PdfWriter(stream);
                PdfDocument pdfDoc = new PdfDocument(writer);
                Document document = new Document(pdfDoc);

                // Add title
                document.Add(new Paragraph("Order List").SetTextAlignment(TextAlignment.CENTER).SetFontSize(18));

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

                return File(stream.ToArray(), "application/pdf", "Orders.pdf");
            }
        }

        private DataTable GetOrdersDataPdf()
        {
            // Your existing code to fetch data
            string connectionString = this.configuration.GetConnectionString("ConnectionString");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand sqlCommand = connection.CreateCommand())
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.CommandText = "PR_Order_SelectAll";

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
