﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExerciseMVC.Models.ViewModel
{
    public class InstructorViewModel
    {
        public Instructor Instructor { get; set; }
        public List<SelectListItem> Cohorts { get; set; }
    }
}
