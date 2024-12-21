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
    public class OrderDetailController : Controller
    {

        private IConfiguration configuration;

        public OrderDetailController(IConfiguration _configuration)
        {
            configuration = _configuration;
        }
        public IActionResult OrderDetailList()
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PR_OrderDetail_SelectAll";
            SqlDataReader reader = command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(reader);
            return View(table);
        }

        public IActionResult OrderDetailForm(int OrderDetailID)
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

                // Check for DBNull before converting
                orderDropDownModel.OrderID = DBNull.Value.Equals(order["OrderID"])
                    ? 0 // Or another default value if necessary
                    : Convert.ToInt32(order["OrderID"]);

                orderDropDownModel.OrderNumber = DBNull.Value.Equals(order["OrderNumber"])
                    ? 0 // Or another default value if necessary
                    : Convert.ToInt32(order["OrderNumber"]);

                orders.Add(orderDropDownModel);
            }


            ViewBag.OrderList = orders;

            #endregion



            #region Product Drop-Down

            SqlConnection connection2 = new SqlConnection(connectionString);
            connection2.Open();
            SqlCommand command2 = connection2.CreateCommand();
            command2.CommandType = System.Data.CommandType.StoredProcedure;
            command2.CommandText = "PR_Product_DropDown";
            SqlDataReader reader2 = command2.ExecuteReader();
            DataTable dataTable2 = new DataTable();
            dataTable2.Load(reader2);
            connection2.Close();

            List<ProductDropDownModel> products = new List<ProductDropDownModel>();

            foreach (DataRow product in dataTable2.Rows)
            {
                ProductDropDownModel productDropDownModel = new ProductDropDownModel();
                productDropDownModel.ProductID = Convert.ToInt32(@product["ProductID"]);
                productDropDownModel.ProductName = @product["ProductName"].ToString();
                products.Add(productDropDownModel);
            }

            ViewBag.ProductList = products;

            #endregion


            #region User Drop-Down

            SqlConnection connection3 = new SqlConnection(connectionString);
            connection3.Open();
            SqlCommand command3 = connection3.CreateCommand();
            command3.CommandType = System.Data.CommandType.StoredProcedure;
            command3.CommandText = "PR_User_DropDown";
            SqlDataReader reader3 = command3.ExecuteReader();
            DataTable dataTable3 = new DataTable();
            dataTable3.Load(reader3);
            connection3.Close();

            List<UserDropDownModel> users = new List<UserDropDownModel>();

            foreach (DataRow user in dataTable3.Rows)
            {
                UserDropDownModel userDropDownModel = new UserDropDownModel();
                userDropDownModel.UserID = Convert.ToInt32(@user["UserID"]);
                userDropDownModel.UserName = @user["UserName"].ToString();
                users.Add(userDropDownModel);
            }

            ViewBag.UserList = users;

            #endregion

            #region OrderDetailByID

            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PR_OrderDetail_SelectByPK";
            command.Parameters.AddWithValue("@OrderDetailID", OrderDetailID);
            SqlDataReader reader = command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(reader);
            OrderDetailModel orderdetailModel = new OrderDetailModel();

            foreach (DataRow orderdetail in table.Rows)
            {
                orderdetailModel.OrderDetailID = Convert.ToInt32(@orderdetail["OrderDetailID"]);
                orderdetailModel.OrderID = Convert.ToInt32(@orderdetail["OrderID"]);
                orderdetailModel.ProductID = Convert.ToInt32(@orderdetail["ProductID"]);
                orderdetailModel.Quantity = Convert.ToInt32(@orderdetail["Quantity"]);
                orderdetailModel.Amount = Convert.ToDouble(@orderdetail["Amount"]);
                orderdetailModel.TotalAmount = Convert.ToDouble(@orderdetail["TotalAmount"]);
                orderdetailModel.UserID = Convert.ToInt32(@orderdetail["UserID"]);
            }
            #endregion
            OrderDropDown();
            ProductDropDown();
            UserDropDown();
            return View("OrderDetailForm",orderdetailModel);
        }

        [HttpPost]

        public IActionResult OrderDetailSave(OrderDetailModel orderdetailmodel)
        {
            if (orderdetailmodel.ProductID <= 0 || orderdetailmodel.UserID <= 0)
            {
                ModelState.AddModelError("UserID", "A valid user is required.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string connectionString = this.configuration.GetConnectionString("ConnectionString");
                    SqlConnection connection = new SqlConnection(connectionString);
                    connection.Open();
                    SqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.StoredProcedure;
                    if (orderdetailmodel.OrderDetailID == 0 || orderdetailmodel.OrderDetailID == null)
                    {
                        command.CommandText = "PR_OrderDetail_Insert";
                        TempData["SuccessMessage"] = "Order detail successfully added!";
                    }
                    else
                    {
                        command.CommandText = "PR_OrderDetail_UpdateByPK";
                        command.Parameters.Add("@OrderDetailID", SqlDbType.Int).Value = orderdetailmodel.OrderDetailID;
                        TempData["SuccessMessage"] = "Order detail successfully updated!";
                    }

                    command.Parameters.Add("@OrderID", SqlDbType.Int).Value = orderdetailmodel.OrderID;
                    command.Parameters.Add("@ProductID", SqlDbType.Int).Value = orderdetailmodel.ProductID;
                    command.Parameters.Add("@Quantity", SqlDbType.Int).Value = orderdetailmodel.Quantity;
                    command.Parameters.Add("@Amount", SqlDbType.Float).Value = orderdetailmodel.Amount;
                    command.Parameters.Add("@TotalAmount", SqlDbType.Float).Value = orderdetailmodel.TotalAmount;
                    command.Parameters.Add("@UserID", SqlDbType.Int).Value = orderdetailmodel.UserID;
                    command.ExecuteNonQuery();

                    return RedirectToAction("OrderDetailList");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An error occurred while saving the order detail: " + ex.Message;
                }
            }

            OrderDropDown();
            ProductDropDown();
            UserDropDown();

            return View("OrderDetailForm", orderdetailmodel);
        }

        public IActionResult OrderDetailDelete(int OrderDetailID)
        {
            try
            {
                string connectionString = this.configuration.GetConnectionString("ConnectionString");
                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "PR_OrderDetail_DeleteByPK";
                command.Parameters.Add("@OrderDetailID", SqlDbType.Int).Value = OrderDetailID;
                command.ExecuteNonQuery();

                TempData["SuccessMessage"] = "Order detail successfully deleted!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the order detail: " + ex.Message;
            }
            return RedirectToAction("OrderDetailList");
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

                // Handle OrderID with DBNull check
                orderDropDownModel.OrderID = Convert.IsDBNull(data["OrderID"])
                    ? 0 // Use a default value or handle null as needed
                    : Convert.ToInt32(data["OrderID"]);

                // Handle OrderNumber with DBNull check
                orderDropDownModel.OrderNumber = Convert.IsDBNull(data["OrderNumber"])
                    ? 0 // Use a default value or handle null as needed
                    : Convert.ToInt32(data["OrderNumber"]);

                orderList.Add(orderDropDownModel);
            }

            ViewBag.OrderList = orderList;
        }

        public void ProductDropDown()
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection1 = new SqlConnection(connectionString);
            connection1.Open();
            SqlCommand command1 = connection1.CreateCommand();
            command1.CommandType = CommandType.StoredProcedure;
            command1.CommandText = "PR_Product_DropDown";
            SqlDataReader reader1 = command1.ExecuteReader();
            DataTable dataTable1 = new DataTable();
            dataTable1.Load(reader1);
            List<ProductDropDownModel> productList = new List<ProductDropDownModel>();
            foreach (DataRow data in dataTable1.Rows)
            {
                ProductDropDownModel productDropDownModel = new ProductDropDownModel();
                productDropDownModel.ProductID = Convert.ToInt32(data["ProductID"]);
                productDropDownModel.ProductName = data["ProductName"].ToString();
                productList.Add(productDropDownModel);
            }
            ViewBag.ProductList = productList;
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

            DataTable dataTable = GetOrderDetailsData();

            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("OrderDetails");

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
                string fileName = "OrderDetails.xlsx";
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(stream, contentType, fileName);
            }
        }

        private DataTable GetOrderDetailsData()
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();


                using (SqlCommand sqlCommand = connection.CreateCommand())
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.CommandText = "PR_OrderDetail_SelectAll";


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
            DataTable dataTable = GetOrderDetailsDataPdf();

            using (MemoryStream stream = new MemoryStream())
            {
                // Initialize PDF writer
                PdfWriter writer = new PdfWriter(stream);
                PdfDocument pdfDoc = new PdfDocument(writer);
                Document document = new Document(pdfDoc);

                // Add title
                document.Add(new Paragraph("OrderDetail List").SetTextAlignment(TextAlignment.CENTER).SetFontSize(18));

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

                return File(stream.ToArray(), "application/pdf", "OrderDetails.pdf");
            }
        }

        private DataTable GetOrderDetailsDataPdf()
        {
            // Your existing code to fetch data
            string connectionString = this.configuration.GetConnectionString("ConnectionString");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand sqlCommand = connection.CreateCommand())
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.CommandText = "PR_OrderDetail_SelectAll";

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
