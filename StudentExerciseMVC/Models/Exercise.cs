using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExerciseMVC.Models
{
    public class Exercise
    {
        public int Id { get; set; }
        [Display(Name = "Exercise")]
        public string Name { get; set; }
        public string Language { get; set; }
        public List<Student> Students { get; set; }
    }
}
