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
    public class StudentsController : Controller
    {
        private readonly IConfiguration _config;

        public StudentsController(IConfiguration config)
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
        // GET: Students
        public ActionResult Index()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT s.Id AS StudentId, s.FirstName, s.LastName, s.SlackHandle, s.CohortId, c.[Name] AS CohortName FROM Student s 
                                        LEFT JOIN Cohort c ON s.CohortId = c.Id";
                    var reader = cmd.ExecuteReader();
                    var students = new List<Student>();
                    while (reader.Read())
                    {
                        students.Add(new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
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
                    return View(students);
                }
            }
        }

        // GET: Student/Details/5
        public ActionResult Details(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT s.Id AS StudentId, s.FirstName, s.LastName, s.SlackHandle, s.CohortId, c.[Name] AS CohortName FROM Student s 
                                        LEFT JOIN Cohort c ON s.CohortId = c.Id
                                        WHERE s.Id = @Id";
                    cmd.Parameters.Add(new SqlParameter("@Id", id));
                    var reader = cmd.ExecuteReader();

                    Student student = null;

                    while (reader.Read())
                    {
                        if (student == null)
                        {
                            student = new Student
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
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
                         student.Exercises = GetAllExercisesByStudentId(id);
                    }

                    reader.Close();

                    if (student == null)
                    {
                        return NotFound();
                    }
                    return View(student);

                }
            }
        }

        // GET: Student/Create
        public ActionResult Create()
        {
            var cohorts = GetCohorts().Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString()
            }).ToList();

            var viewModel = new StudentViewModel()
            {
                Student = new Student(),
                Cohorts = cohorts
            };
            return View(viewModel);
        }

        // POST: Student/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Student student)
        {
            {
                try
                {
                    using (SqlConnection conn = Connection)
                    {
                        conn.Open();
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = @"INSERT INTO Student (FirstName, LastName, SlackHandle, CohortId)
                                            VALUES (@FirstName, @LastName, @SlackHandle, @CohortId)";

                            cmd.Parameters.Add(new SqlParameter("@FirstName", student.FirstName));
                            cmd.Parameters.Add(new SqlParameter("@LastName", student.LastName));
                            cmd.Parameters.Add(new SqlParameter("@SlackHandle", student.SlackHandle));
                            cmd.Parameters.Add(new SqlParameter("@CohortId", student.CohortId));

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

        // GET: Student/Edit/5
        public ActionResult Edit(int id)
        {
            var cohorts = GetCohorts().Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString()
            }).ToList();
            var exercises = GetAllExercises().Select(e => new SelectListItem
            {
                Text = e.Name,
                Value = e.Id.ToString()
            }).ToList();

            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT s.Id AS StudentId, s.FirstName, s.LastName, s.SlackHandle, s.CohortId, c.[Name] AS CohortName FROM Student s 
                                        LEFT JOIN Cohort c ON s.CohortId = c.Id
                                        WHERE s.Id = @Id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    var reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        var student = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            ExerciseIds = GetAllExercisesByStudentId(id).Select(e => e.Id).ToList()

                        };

                        reader.Close();
                        var viewModel = new StudentViewModel()
                        {
                            Student = student,
                            Cohorts = cohorts,
                            Exercises = exercises
                        };
                        return View(viewModel);
                    }
                    reader.Close();
                    return NotFound();
                }
            }

        }

        // POST: Student/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Student student)
        {
            {
                try
                {
                    using (SqlConnection conn = Connection)
                    {
                        conn.Open();
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = @"UPDATE Student
                                            SET FirstName = @FirstName, LastName = @LastName, SlackHandle = @SlackHandle,
                                                CohortId = @CohortId
                                            WHERE Id = @id";

                            cmd.Parameters.Add(new SqlParameter("@id", id));
                            cmd.Parameters.Add(new SqlParameter("@FirstName", student.FirstName));
                            cmd.Parameters.Add(new SqlParameter("@LastName", student.LastName));
                            cmd.Parameters.Add(new SqlParameter("@SlackHandle", student.SlackHandle));
                            cmd.Parameters.Add(new SqlParameter("@CohortId", student.CohortId));

                            cmd.ExecuteNonQuery();
                        }
                    }
                    DeleteAssignedExercises(student.Id);
                    AddStudentExercise(student.Id, student.ExerciseIds);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    return View();
                }
            }
        }

        // GET: Student/Delete/5
        public ActionResult Delete(int id)
        {
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"SELECT s.Id AS StudentId, s.FirstName, s.LastName, s.SlackHandle, s.CohortId, c.[Name] AS CohortName FROM Student s 
                                        LEFT JOIN Cohort c ON s.CohortId = c.Id
                                        WHERE s.Id = @Id";

                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        var reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            var student = new Student
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
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
                            return View(student);
                        }
                        return NotFound();
                    }
                }
            }
        }

        // POST: Student/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, Student student)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Student WHERE Id = @id";

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

        private List<Exercise> GetAllExercises()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, Name, Language 
                                       FROM Exercise";

                    var reader = cmd.ExecuteReader();

                    var exercises = new List<Exercise>();

                    while (reader.Read())
                    {
                        exercises.Add(new Exercise
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Language = reader.GetString(reader.GetOrdinal("Language"))
                        });
                    }

                    reader.Close();

                    return exercises;
                }
            }
        }

        private void AddStudentExercise(int studentId, List<int> exerciseIds)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                foreach (var exerciseId in exerciseIds)
                {
                    using (SqlCommand cmd = conn.CreateCommand())
                    {

                        cmd.CommandText = @"INSERT INTO StudentExercise(StudentId, ExerciseId) 
                                       VALUES(@StudentId, @ExerciseId)";

                        cmd.Parameters.AddWithValue("@StudentId", studentId);
                        cmd.Parameters.AddWithValue("@ExerciseId", exerciseId);

                        cmd.ExecuteNonQuery();

                    }
                }

            }


        }

        //private List<int> GetAllExercisesByStudentId(int studentId)
        //{
        //    using (SqlConnection conn = Connection)
        //    {
        //        conn.Open();
        //        using (SqlCommand cmd = conn.CreateCommand())
        //        {
        //            cmd.CommandText = @"SELECT ExerciseId FROM StudentExercise
        //                              WHERE StudentId = @StudentId";

        //            cmd.Parameters.AddWithValue("@StudentId", studentId);

        //            var reader = cmd.ExecuteReader();

        //            var exerciseIds = new List<int>();

        //            while (reader.Read())
        //            {
        //                exerciseIds.Add(reader.GetInt32(reader.GetOrdinal("ExerciseId")));

        //            };

        //            reader.Close();
        //            return exerciseIds;
        //        }
        //    }
        //}

        private void DeleteAssignedExercises(int studentId)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM StudentExercise WHERE StudentId = @studentId";

                        cmd.Parameters.Add(new SqlParameter("@studentId", studentId));

                        cmd.ExecuteNonQuery();

                    }
                }

            }
            catch
            {
                throw;
            }
        }
        private List<Exercise> GetAllExercisesByStudentId(int studentId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT e.Id AS ExerciseId, e.[Name] AS Exercise, e.[Language]  FROM
                                        Exercise e INNER JOIN StudentExercise se ON e.Id = se.ExerciseId
                                        WHERE StudentId = @StudentId";

                    cmd.Parameters.AddWithValue("@StudentId", studentId);

                    var reader = cmd.ExecuteReader();

                    List<Exercise> exercises = new List<Exercise>();

                    while (reader.Read())
                    {
                        Exercise exercise = new Exercise
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("ExerciseId")),
                            Name = reader.GetString(reader.GetOrdinal("Exercise")),
                            Language = reader.GetString(reader.GetOrdinal("Language")),
                        };
                        exercises.Add(exercise);

                    };
                    reader.Close();
                    return exercises;
                }
            }
        }
    }
}