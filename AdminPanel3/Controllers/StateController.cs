using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using AdminPanel3.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AdminPanel3.Controllers
{
    public class StateController : Controller
    {
        private readonly IConfiguration _configuration;
        public StateController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        #region Get All States
        public IActionResult Index()
        {
            string connectionstr = _configuration.GetConnectionString("ConnectionString");
            DataTable dtStates = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionstr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("PR_LOC_State_SelectAll", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                SqlDataReader reader = cmd.ExecuteReader();
                dtStates.Load(reader);
            }

            // Get countries and store them in ViewBag
            ViewBag.CountryList = GetCountries(); // This should return SelectListItems as previously discussed

            // Return the DataTable with countries available for dropdowns
            return View(dtStates);
        }
        #endregion

        #region Create State
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.CountryList = GetCountries(); // Populate countries for dropdown
            return View("StateForm", new State()); // Return a new instance for creating a state
        }

        [HttpPost]
        public IActionResult Create(State state)
        {
            if (ModelState.IsValid)
            {
                string connectionstr = _configuration.GetConnectionString("ConnectionString");
                using (SqlConnection conn = new SqlConnection(connectionstr))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("PR_LOC_State_Insert", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    cmd.Parameters.AddWithValue("@CountryID", state.CountryID);
                    cmd.Parameters.AddWithValue("@StateName", state.StateName);
                    cmd.Parameters.AddWithValue("@StateCode", state.StateCode);
                    cmd.ExecuteNonQuery();
                }

                TempData["StateInsertMsg"] = "State added successfully!";
                return RedirectToAction("Index");
            }

            ViewBag.CountryList = GetCountries(); // Re-populate countries if validation fails
            return View("StateForm", state); // Return the form with validation errors if any
        }
        #endregion

        #region Edit State
        [HttpGet]
        public IActionResult Edit(int id)
        {
            State state = null;
            string connectionstr = _configuration.GetConnectionString("ConnectionString");
            using (SqlConnection conn = new SqlConnection(connectionstr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("PR_LOC_State_SelectById", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@StateID", id);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    state = new State
                    {
                        StateID = (int)reader["StateID"],
                        CountryID = (int)reader["CountryID"],
                        StateName = reader["StateName"].ToString(),
                        StateCode = reader["StateCode"].ToString()
                    };
                }
            }

            ViewBag.CountryList = GetCountries(); // Populate countries for dropdown
            return View("StateForm", state); // Return the form with existing data for editing
        }

        [HttpPost]
        public IActionResult Edit(State state)
        {
            if (ModelState.IsValid)
            {
                string connectionstr = _configuration.GetConnectionString("ConnectionString");
                using (SqlConnection conn = new SqlConnection(connectionstr))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("PR_LOC_State_Update", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    cmd.Parameters.AddWithValue("@StateID", state.StateID);
                    cmd.Parameters.AddWithValue("@CountryID", state.CountryID);
                    cmd.Parameters.AddWithValue("@StateName", state.StateName);
                    cmd.Parameters.AddWithValue("@StateCode", state.StateCode);
                    cmd.ExecuteNonQuery();
                }

                TempData["StateInsertMsg"] = "State updated successfully!";
                return RedirectToAction("Index");
            }

            ViewBag.CountryList = GetCountries(); // Re-populate countries if validation fails
            return View("StateForm", state); // Return the form with validation errors if any
        }
        #endregion

        #region Delete State
        [HttpPost]
        public IActionResult Delete(int id)
        {
            string connectionstr = _configuration.GetConnectionString("ConnectionString");
            using (SqlConnection conn = new SqlConnection(connectionstr))
            {
                conn.Open();

                // Now delete the state itself
                SqlCommand cmdDeleteStates = new SqlCommand("PR_LOC_State_Delete", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmdDeleteStates.Parameters.AddWithValue("@StateID", id);
                cmdDeleteStates.ExecuteNonQuery();
            }

            TempData["StateInsertMsg"] = "State deleted successfully!";
            return RedirectToAction("Index");
        }
        #endregion

        private List<SelectListItem> GetCountries()
        {
            List<SelectListItem> countries = new List<SelectListItem>();
            string connectionstr = _configuration.GetConnectionString("ConnectionString");

            using (SqlConnection conn = new SqlConnection(connectionstr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("PR_LOC_Country_SelectAll", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        countries.Add(new SelectListItem
                        {
                            Value = reader["CountryID"].ToString(), // Assuming CountryID is a string
                            Text = reader["CountryName"].ToString()
                        });
                    }
                }
            }

            return countries;
        }
        private string GetCountryNameById(int countryId)
        {
            string countryName = "Unknown"; // Default value if not found
            string connectionstr = _configuration.GetConnectionString("ConnectionString");

            using (SqlConnection conn = new SqlConnection(connectionstr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("PR_LOC_Country_SelectById", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@CountryID", countryId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        countryName = reader["CountryName"].ToString();
                    }
                }
            }

            return countryName;
        }
    }
}