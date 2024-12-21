using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using AdminPanel3.Models;

namespace AdminPanel3.Controllers
{
    public class CountryController : Controller
    {
        private readonly IConfiguration _configuration;

        public CountryController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        #region Get All Countries
        public IActionResult Index()
        {
            string connectionstr = _configuration.GetConnectionString("ConnectionString");
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionstr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("PR_LOC_Country_SelectAll", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                SqlDataReader reader = cmd.ExecuteReader();
                dt.Load(reader);
            }
            return View(dt);
        }
        #endregion

        #region Get Country by ID
        public IActionResult Details(int id)
        {
            Country country = null;
            string connectionstr = _configuration.GetConnectionString("ConnectionString");
            using (SqlConnection conn = new SqlConnection(connectionstr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("PR_LOC_Country_SelectById", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@CountryID", id);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    country = new Country
                    {
                        CountryID = (int)reader["CountryID"],
                        CountryName = reader["CountryName"].ToString(),
                        CountryCode = reader["CountryCode"].ToString(),
                        CreatedDate = (DateTime)reader["CreatedDate"],
                        ModifiedDate = reader["ModifiedDate"] as DateTime?
                    };
                }
            }
            return View(country);
        }
        #endregion

        #region Create Country
        [HttpGet]
        public IActionResult Create()
        {
            return View("CountryForm", new Country()); // Return the form for creating a new country
        }

        [HttpPost]
        public IActionResult Create(Country country)
        {
            if (ModelState.IsValid)
            {
                string connectionstr = _configuration.GetConnectionString("ConnectionString");
                using (SqlConnection conn = new SqlConnection(connectionstr))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("PR_LOC_Country_Insert", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    cmd.Parameters.AddWithValue("@CountryName", country.CountryName);
                    cmd.Parameters.AddWithValue("@CountryCode", country.CountryCode);
                    cmd.ExecuteNonQuery();
                }

                TempData["CountryInsertMsg"] = "Country added successfully!";
                return RedirectToAction("Index");
            }
            return View("CountryForm", country); // Return the form with validation errors if any
        }
        #endregion

        #region Edit Country
        [HttpGet]
        public IActionResult Edit(int id)
        {
            Country country = null;
            string connectionstr = _configuration.GetConnectionString("ConnectionString");
            using (SqlConnection conn = new SqlConnection(connectionstr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("PR_LOC_Country_SelectById", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@CountryID", id);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    country = new Country
                    {
                        CountryID = (int)reader["CountryID"],
                        CountryName = reader["CountryName"].ToString(),
                        CountryCode = reader["CountryCode"].ToString()
                    };
                }
            }
            return View("CountryForm", country); // Return the form with existing data for editing
        }

        [HttpPost]
        public IActionResult Edit(Country country)
        {
            if (ModelState.IsValid)
            {
                string connectionstr = _configuration.GetConnectionString("ConnectionString");
                using (SqlConnection conn = new SqlConnection(connectionstr))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("PR_LOC_Country_Update", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    cmd.Parameters.AddWithValue("@CountryID", country.CountryID);
                    cmd.Parameters.AddWithValue("@CountryName", country.CountryName);
                    cmd.Parameters.AddWithValue("@CountryCode", country.CountryCode);
                    cmd.ExecuteNonQuery();
                }

                TempData["CountryInsertMsg"] = "Country updated successfully!";
                return RedirectToAction("Index");
            }
            return View("CountryForm", country); // Return the form with validation errors if any
        }
        #endregion

        #region Delete Country
        [HttpPost]
        public IActionResult Delete(int id)
        {
            // First, delete all cities associated with this country
            string connectionstr = _configuration.GetConnectionString("ConnectionString");
            using (SqlConnection conn = new SqlConnection(connectionstr))
            {
                conn.Open();

                // Delete cities associated with the country
                SqlCommand cmdDeleteCities = new SqlCommand("PR_LOC_City_DeleteByCountryID", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmdDeleteCities.Parameters.AddWithValue("@CountryID", id);
                cmdDeleteCities.ExecuteNonQuery();

                // Now delete the country
                SqlCommand cmdDeleteCountries = new SqlCommand("PR_LOC_Country_Delete", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmdDeleteCountries.Parameters.AddWithValue("@CountryID", id);
                cmdDeleteCountries.ExecuteNonQuery();
            }

            TempData["CountryInsertMsg"] = "Country deleted successfully!";
            return RedirectToAction("Index");
        }
        #endregion
    }
}