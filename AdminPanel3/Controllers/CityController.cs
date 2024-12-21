using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using AdminPanel3.Models;

namespace AdminPanel3.Controllers
{
    public class CityController : Controller
    {
        private readonly IConfiguration _configuration;

        #region Configuration
        public CityController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        #endregion

        #region City List
        public IActionResult CityList()
        {
            string connectionstr = _configuration.GetConnectionString("ConnectionString");
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionstr))
            {
                conn.Open();
                SqlCommand objCmd = conn.CreateCommand();
                objCmd.CommandType = CommandType.StoredProcedure;
                objCmd.CommandText = "PR_LOC_City_SelectAll";

                using (SqlDataReader objSDR = objCmd.ExecuteReader())
                {
                    dt.Load(objSDR);
                }
            }
            return View("CityList", dt);
        }
        #endregion

        #region Delete
        [HttpPost]
        public IActionResult Delete(int cityID)
        {
            string connectionstr = _configuration.GetConnectionString("ConnectionString");
            using (SqlConnection conn = new SqlConnection(connectionstr))
            {
                conn.Open();
                using (SqlCommand sqlCommand = conn.CreateCommand())
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.CommandText = "PR_LOC_City_Delete";
                    sqlCommand.Parameters.AddWithValue("@CityID", cityID);
                    sqlCommand.ExecuteNonQuery();
                }
            }
            TempData["CityInsertMsg"] = "City deleted successfully!";
            return RedirectToAction("CityList");
        }
        #endregion

        #region Add/Edit City
        [HttpGet]
        public IActionResult CityAddEdit(string? cityID)
        {
            CityModel cityModel = new CityModel();

            if (!string.IsNullOrEmpty(cityID))
            {
                // Decrypt the CityID
                int decryptedCityID;
                try
                {
                    decryptedCityID = Convert.ToInt32(UrlEncryptor.Decrypt(cityID));
                }
                catch (Exception)
                {
                    return NotFound(); // Handle decryption failure
                }

                // Fetch existing city details for editing
                string connectionstr = _configuration.GetConnectionString("ConnectionString");
                using (SqlConnection conn = new SqlConnection(connectionstr))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("PR_LOC_City_SelectById", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    cmd.Parameters.AddWithValue("@CityID", decryptedCityID);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        cityModel.CityID = (int)reader["CityID"];
                        cityModel.CityName = reader["CityName"].ToString();
                        cityModel.StateID = (int)reader["StateID"];
                        cityModel.CountryID = (int)reader["CountryID"];
                        cityModel.CityCode = reader["CityCode"].ToString();
                    }
                    else
                    {
                        return NotFound(); // Handle case where no data is found for the ID
                    }
                }
            }

            ViewBag.CountryList = GetCountries();
            ViewBag.StateList = GetStates();

            return View(cityModel);
        }

        [HttpPost]
        public IActionResult CityAddEdit(CityModel city)
        {
            if (ModelState.IsValid)
            {
                string connectionstr = _configuration.GetConnectionString("ConnectionString");
                using (SqlConnection conn = new SqlConnection(connectionstr))
                {
                    conn.Open();
                    SqlCommand cmd;

                    if (city.CityID.HasValue)
                    {
                        cmd = new SqlCommand("PR_LOC_City_Update", conn)
                        {
                            CommandType = CommandType.StoredProcedure
                        };
                        cmd.Parameters.AddWithValue("@CityID", city.CityID.Value);
                    }
                    else
                    {
                        cmd = new SqlCommand("PR_LOC_City_Insert", conn)
                        {
                            CommandType = CommandType.StoredProcedure
                        };
                    }

                    cmd.Parameters.AddWithValue("@CityName", city.CityName);
                    cmd.Parameters.AddWithValue("@StateID", city.StateID);
                    cmd.Parameters.AddWithValue("@CountryID", city.CountryID);
                    cmd.Parameters.AddWithValue("@CityCode", city.CityCode);

                    cmd.ExecuteNonQuery();
                }

                TempData["CityInsertMsg"] = "City saved successfully!";
                return RedirectToAction("CityList");
            }

            // If we get here, something went wrong, re-populate dropdowns
            ViewBag.CountryList = GetCountries();
            ViewBag.StateList = GetStates();

            return View(city); // Return to view with validation errors if any
        }
        #endregion

        #region Helper Methods

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
                            Value = reader["CountryID"].ToString(),
                            Text = reader["CountryName"].ToString()
                        });
                    }
                }
            }

            return countries;
        }

        private List<SelectListItem> GetStates()
        {
            List<SelectListItem> states = new List<SelectListItem>();

            string connectionstr = _configuration.GetConnectionString("ConnectionString");

            using (SqlConnection conn = new SqlConnection(connectionstr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("PR_LOC_State_SelectAll", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        states.Add(new SelectListItem
                        {
                            Value = reader["StateID"].ToString(),
                            Text = reader["StateName"].ToString()
                        });
                    }
                }
            }

            return states;
        }

        #endregion
    }
}