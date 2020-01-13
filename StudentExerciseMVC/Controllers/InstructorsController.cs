using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StudentExerciseMVC.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Rendering;
using StudentExerciseMVC.Models.ViewModel;

namespace StudentExerciseMVC.Controllers
{
    public class InstructorsController : Controller
    {
        private readonly IConfiguration _config;

        public InstructorsController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }
        // GET: Instructors
        public ActionResult Index()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT i.Id AS InstructorId, i.FirstName, i.LastName, i.SlackHandle, i.CohortId, c.[Name] AS CohortName FROM Instructor i 
                                        LEFT JOIN Cohort c ON i.CohortId = c.Id";
                    var reader = cmd.ExecuteReader();
                    var instructors = new List<Instructor>();
                    while (reader.Read())
                    {
                        instructors.Add(new Instructor
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("InstructorId")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Cohort = new Cohort
                            {
                                Name = reader.GetString(reader.GetOrdinal("CohortName")),
                            }

                        });
                    }
                    reader.Close();
                    return View(instructors);
                }
            }
        }

        // GET: Instructor/Details/5
        public ActionResult Details(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT i.Id AS InstructorId, i.FirstName, i.LastName, i.SlackHandle, i.CohortId, c.[Name] AS CohortName FROM Instructor i 
                                        LEFT JOIN Cohort c ON i.CohortId = c.Id
                                        WHERE i.Id = @Id";
                    cmd.Parameters.Add(new SqlParameter("@Id", id));
                    var reader = cmd.ExecuteReader();

                    Instructor instructor = null;

                    while (reader.Read())
                    {
                        if (instructor == null)
                        {
                            instructor = new Instructor
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("InstructorId")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Cohort = new Cohort
                                {
                                    Name = reader.GetString(reader.GetOrdinal("CohortName"))
                                }
                            };
                        }

                    }

                    reader.Close();

                    if (instructor == null)
                    {
                        return NotFound();
                    }
                    return View(instructor);

                }
            }
        }

        // GET: Instructor/Create
        public ActionResult Create()
        {
            var cohorts = GetCohorts().Select(d => new SelectListItem
            {
                Text = d.Name,
                Value = d.Id.ToString()
            }).ToList();

            var viewModel = new InstructorViewModel()
            {
                Instructor = new Instructor(),
                Cohorts = cohorts
            };
            return View(viewModel);
        }

        // POST: Instructor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Instructor instructor)
        {
            {
                try
                {
                    using (SqlConnection conn = Connection)
                    {
                        conn.Open();
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = @"INSERT INTO Instructor (FirstName, LastName, SlackHandle, CohortId)
                                            VALUES (@FirstName, @LastName, @SlackHandle, @CohortId)";

                            cmd.Parameters.Add(new SqlParameter("@FirstName", instructor.FirstName));
                            cmd.Parameters.Add(new SqlParameter("@LastName", instructor.LastName));
                            cmd.Parameters.Add(new SqlParameter("@SlackHandle", instructor.SlackHandle));
                            cmd.Parameters.Add(new SqlParameter("@CohortId", instructor.CohortId));

                            cmd.ExecuteNonQuery();
                        }
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    return View();
                }
            }
        }

        // GET: Instructor/Edit/5
        public ActionResult Edit(int id)
        {
            var cohorts = GetCohorts().Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString()
            }).ToList();
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, FirstName, LastName, SlackHandle, CohortId
                                        FROM Instructor 
                                        WHERE Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    var reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        var instructor = new Instructor
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId"))
                        };

                        reader.Close();
                        var viewModel = new InstructorViewModel()
                        {
                            Instructor = instructor,
                            Cohorts = cohorts
                        };
                        return View(viewModel);
                    }
                    reader.Close();
                    return NotFound();
                }
            }

        }

        // POST: Instructor/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Instructor instructor)
        {
            {
                try
                {
                    using (SqlConnection conn = Connection)
                    {
                        conn.Open();
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = @"UPDATE Instructor
                                            SET FirstName = @FirstName, LastName = @LastName, SlackHandle = @SlackHandle,
                                                CohortId = @CohortId
                                            WHERE Id = @id";

                            cmd.Parameters.Add(new SqlParameter("@id", id));
                            cmd.Parameters.Add(new SqlParameter("@FirstName", instructor.FirstName));
                            cmd.Parameters.Add(new SqlParameter("@LastName", instructor.LastName));
                            cmd.Parameters.Add(new SqlParameter("@SlackHandle", instructor.SlackHandle));
                            cmd.Parameters.Add(new SqlParameter("@CohortId", instructor.CohortId));
                            
                            cmd.ExecuteNonQuery();
                        }
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    return View();
                }
            }
        }

        // GET: Instructor/Delete/5
        public ActionResult Delete(int id)
        {
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"SELECT i.Id AS InstructorId, i.FirstName, i.LastName, i.SlackHandle, i.CohortId, c.[Name] AS CohortName FROM Instructor i 
                                        LEFT JOIN Cohort c ON i.CohortId = c.Id
                                        WHERE i.Id = @Id";

                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        var reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            var instructor = new Instructor
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("InstructorId")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Cohort = new Cohort
                                {
                                    Name = reader.GetString(reader.GetOrdinal("CohortName"))
                                }

                            };

                            reader.Close();
                            return View(instructor);
                        }
                        return NotFound();
                    }
                }
            }
        }

        // POST: Instructor/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, Instructor instructor)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Instructor WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        cmd.ExecuteNonQuery();

                        return RedirectToAction(nameof(Index));
                    }
                }

            }
            catch
            {
                return View();
            }
        }

        // GET: Cohort List
        private List<Cohort> GetCohorts()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, Name 
                                       FROM Cohort";

                    var reader = cmd.ExecuteReader();

                    var cohorts = new List<Cohort>();

                    while (reader.Read())
                    {
                        cohorts.Add(new Cohort
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name"))
                        });
                    }

                    reader.Close();

                    return cohorts;
                }
            }
        }
    }
}