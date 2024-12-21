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
    public class UserController : Controller
    {

        private IConfiguration configuration;

        public UserController(IConfiguration _configuration)
        {
            configuration = _configuration;
        }

        public IActionResult UserList()
        {
            
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PR_User_SelectAll";
            SqlDataReader reader = command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(reader);
            return View(table);
        }

        public IActionResult UserForm(int? UserID)
        {
            if (!UserID.HasValue)
            {
                UserID = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
            }

            string connectionString = this.configuration.GetConnectionString("ConnectionString");

            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PR_User_SelectByPK";
            command.Parameters.AddWithValue("@UserID", UserID);
            SqlDataReader reader = command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(reader);
            UserModel userModel = new UserModel();

            foreach (DataRow user in table.Rows)
            {
                userModel.UserID = Convert.ToInt32(user["UserID"]);
                userModel.UserName = user["UserName"].ToString();
                userModel.Email = user["Email"].ToString();
                userModel.Password = user["Password"].ToString();
                userModel.MobileNo = user["MobileNo"].ToString();
                userModel.Address = user["Address"].ToString();
                userModel.IsActive = Convert.ToBoolean(user["IsActive"]);
            }

            return View("UserForm", userModel);
        }

        [HttpPost]
        public IActionResult UpdateProfile(UserModel userModel)
        {
            if (ModelState.IsValid)
            {
                string connectionString = this.configuration.GetConnectionString("ConnectionString");
                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "PR_User_UpdateByPK";

                command.Parameters.Add("@UserID", SqlDbType.Int).Value = userModel.UserID;
                command.Parameters.Add("@UserName", SqlDbType.VarChar).Value = userModel.UserName;
                command.Parameters.Add("@Email", SqlDbType.VarChar).Value = userModel.Email;
                command.Parameters.Add("@Password", SqlDbType.VarChar).Value = userModel.Password;
                command.Parameters.Add("@MobileNo", SqlDbType.VarChar).Value = userModel.MobileNo;
                command.Parameters.Add("@Address", SqlDbType.VarChar).Value = userModel.Address;
                command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = userModel.IsActive;

                command.ExecuteNonQuery();
                return RedirectToAction("UserForm", new { UserID = userModel.UserID });
            }

            return View("UserForm", userModel);
        }



        [HttpPost]
        public IActionResult UserSave(UserModel usermodel)
        {
            /*if (usermodel.UserID <= 0)
            {
                ModelState.AddModelError("UserID", "A valid User is required.");
            }*/
            if (ModelState.IsValid)
            {
                string connectionString = this.configuration.GetConnectionString("ConnectionString");
                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;

                if (usermodel.UserID == 0 || usermodel.UserID == null) // For adding new user
                {
                    command.CommandText = "PR_User_Insert";
                }
                else // For updating existing user
                {
                    command.CommandText = "PR_User_UpdateByPK";
                    command.Parameters.Add("@UserID", SqlDbType.Int).Value = usermodel.UserID;
                }

                command.Parameters.Add("@UserName", SqlDbType.VarChar).Value = usermodel.UserName;
                command.Parameters.Add("@Email", SqlDbType.VarChar).Value = usermodel.Email;
                command.Parameters.Add("@Password", SqlDbType.VarChar).Value = usermodel.Password;
                command.Parameters.Add("@MobileNo", SqlDbType.VarChar).Value = usermodel.MobileNo;
                command.Parameters.Add("@Address", SqlDbType.VarChar).Value = usermodel.Address;
                command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = usermodel.IsActive;

                command.ExecuteNonQuery();
                return RedirectToAction("UserList");
            }
            return View("UserForm", usermodel);
        }



        public IActionResult UserDelete(int UserID)
        {
            try
            {
                string connectionString = this.configuration.GetConnectionString("ConnectionString");
                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "PR_User_DeleteByPK";
                command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                Console.WriteLine(ex.ToString());
            }
            return RedirectToAction("UserList");
        }

        public IActionResult UserRegister(UserRegisterModel userRegisterModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string connectionString = this.configuration.GetConnectionString("ConnectionString");
                    SqlConnection sqlConnection = new SqlConnection(connectionString);
                    sqlConnection.Open();
                    SqlCommand sqlCommand = sqlConnection.CreateCommand();
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand.CommandText = "PR_User_Register";
                    sqlCommand.Parameters.Add("@UserName", SqlDbType.VarChar).Value = userRegisterModel.UserName;
                    sqlCommand.Parameters.Add("@Password", SqlDbType.VarChar).Value = userRegisterModel.Password;
                    sqlCommand.Parameters.Add("@Email", SqlDbType.VarChar).Value = userRegisterModel.Email;
                    sqlCommand.Parameters.Add("@MobileNo", SqlDbType.VarChar).Value = userRegisterModel.MobileNo;
                    sqlCommand.Parameters.Add("@Address", SqlDbType.VarChar).Value = userRegisterModel.Address;
                    sqlCommand.Parameters.Add("@IsActive", SqlDbType.Bit).Value = userRegisterModel.IsActive = true; // Set default value before inserting
                    sqlCommand.ExecuteNonQuery();
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = e.Message;
                return RedirectToAction("Register");
            }
            return View("Register");
        }


        public IActionResult UserLogin(UserLoginModel userLoginModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string connectionString = this.configuration.GetConnectionString("ConnectionString");
                    using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                    {
                        sqlConnection.Open();
                        using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                        {
                            sqlCommand.CommandType = CommandType.StoredProcedure;
                            sqlCommand.CommandText = "PR_User_Login";
                            sqlCommand.Parameters.Add("@UserName", SqlDbType.NVarChar).Value = userLoginModel.UserName;
                            sqlCommand.Parameters.Add("@Password", SqlDbType.NVarChar).Value = userLoginModel.Password; // Hash this in production

                            using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                            {
                                if (sqlDataReader.Read()) // Check if a user was returned
                                {
                                    // Store user details in session
                                    HttpContext.Session.SetString("UserID", sqlDataReader["UserID"].ToString());
                                    HttpContext.Session.SetString("UserName", sqlDataReader["UserName"].ToString());

                                    // Redirect to the main application page after successful login
                                    return RedirectToAction("Index", "Home"); // Adjust this to your main controller/action
                                }
                                else
                                {
                                    ModelState.AddModelError(string.Empty, "Invalid username or password.");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = e.Message; // Log the error message
            }

            return View("Login"); // Return to login view if not successful
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "User");
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        public ActionResult ExportToExcel()
        {

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            DataTable dataTable = GetUsersData();

            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Users");

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
                string fileName = "Users.xlsx";
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(stream, contentType, fileName);
            }
        }

        private DataTable GetUsersData()
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();


                using (SqlCommand sqlCommand = connection.CreateCommand())
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.CommandText = "PR_User_SelectAll";


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
            DataTable dataTable = GetUsersDataPdf();

            using (MemoryStream stream = new MemoryStream())
            {
                // Initialize PDF writer
                PdfWriter writer = new PdfWriter(stream);
                PdfDocument pdfDoc = new PdfDocument(writer);
                Document document = new Document(pdfDoc);

                // Add title
                document.Add(new Paragraph("User List").SetTextAlignment(TextAlignment.CENTER).SetFontSize(18));

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

                return File(stream.ToArray(), "application/pdf", "Users.pdf");
            }
        }

        private DataTable GetUsersDataPdf()
        {
            // Your existing code to fetch data
            string connectionString = this.configuration.GetConnectionString("ConnectionString");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand sqlCommand = connection.CreateCommand())
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.CommandText = "PR_User_SelectAll";

                    using (SqlDataReader reader = sqlCommand.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);
                        return dataTable;
                    }
                }
            }
        }


        /*List<UserModel> users = new List<UserModel>()
        {
            new UserModel { UserID = 1, UserName = "Alice", Email = "alice@example.com", Password = "password1", MobileNo = "1234567890", Address = "123 Main St", IsActive = true },
            new UserModel { UserID = 2, UserName = "Bob", Email = "bob@example.com", Password = "password2", MobileNo = "1234567891", Address = "124 Main St", IsActive = true },
            // ... other users
        };

        public IActionResult UserList()
        {
            return View(users);
        }

        public IActionResult UserForm(int? id)
        {
            if (id == null)
            {
                ViewData["Action"] = "Create";
                return View(new UserModel());
            }

            var user = users.FirstOrDefault(u => u.UserID == id);
            if (user == null)
            {
                return NotFound();
            }

            ViewData["Action"] = "Edit";
            return View(user);
        }

        [HttpPost]
        public IActionResult UserSave(UserModel usermodel)
        {

            if (ModelState.IsValid)
            {
                if (usermodel.UserID == 0)
                {
                    usermodel.UserID = users.Max(u => u.UserID) + 1;
                    users.Add(usermodel);
                }
                else
                {
                    var existingUser = users.FirstOrDefault(u => u.UserID == usermodel.UserID);
                    if (existingUser != null)
                    {
                        existingUser.UserName = usermodel.UserName;
                        existingUser.Email = usermodel.Email;
                        existingUser.Password = usermodel.Password;
                        existingUser.MobileNo = usermodel.MobileNo;
                        existingUser.Address = usermodel.Address;
                        existingUser.IsActive = usermodel.IsActive;
                    }
                }

                return RedirectToAction("UserList");
            }

            ViewData["Action"] = usermodel.UserID == 0 ? "Create" : "Edit";
            return View("UserForm", usermodel);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var user = users.FirstOrDefault(u => u.UserID == id);
            if (user == null)
            {
                return NotFound();
            }

            users.Remove(user);
            return RedirectToAction("UserList");
        }*/

    }
}

        /*public IActionResult Create()
        {
            ViewData["Action"] = "Create";
            return View("ManageUser");
        }*/

        /*[HttpPost]
        public IActionResult Create(UserModel user)
        {
            if (ModelState.IsValid)
            {
                user.UserID = users.Max(u => u.UserID) + 1;
                users.Add(user);
                return RedirectToAction("UserList");
            }
            ViewData["Action"] = "Create";
            return View("ManageUser", user);
        }*/

        /*public IActionResult Edit(int id)
        {
            var user = users.FirstOrDefault(u => u.UserID == id);
            if (user == null)
            {
                return NotFound();
            }
            ViewData["Action"] = "Edit";
            return View("ManageUser", user);
        }*/


       /* [HttpPost]*/
        /*public IActionResult Edit(UserModel user)
        {
            if (ModelState.IsValid)
            {
                var existingUser = users.FirstOrDefault(u => u.UserID == user.UserID);
                if (existingUser == null)
                {
                    return NotFound();
                }

                *//*existingUser.ProductName = product.ProductName;
                existingUser.ProductPrice = product.ProductPrice;
                existingUser.ProductCode = product.ProductCode;
                existingUser.Description = product.Description;
                existingUser.UserID = product.UserID;*//*


                return RedirectToAction("ProductList");
            }
            *//*ViewData["Action"] = "Edit";
            return View("ManageProduct", product);*//*
        }*/

        /*public IActionResult Delete(int id)
        {
            *//*var product = products.FirstOrDefault(p => p.ProductID == id);
            if (product == null)
            {
                return NotFound();
            }*/
            /*ViewData["Action"] = "Delete";
            return View("ManageProduct", product);*//*
        }*/

        /*[HttpPost]
        public IActionResult Delete(ProductModel product)
        {
            *//*var existingProduct = products.FirstOrDefault(p => p.ProductID == product.ProductID);
            if (existingProduct == null)
            {
                return NotFound();
            }
            products.Remove(existingProduct);
            return RedirectToAction("ProductList");*//*
        }*/
    /*}*/
/*}*/
