﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExerciseMVC.Models.ViewModel
{
    public class StudentViewModel
    {
        public Student Student { get; set; }
     
        public List<SelectListItem> Cohorts { get; set; }
        public List<SelectListItem> Exercises { get; set; }
    }
}
