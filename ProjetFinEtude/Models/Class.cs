﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetFinEtude.Models
{
    public class Class
    {
        public int Id { get; set; }
        [Required, MaxLength(50)]
        public string Name { get; set; }
        [Required]
        public int Year { get; set; }
        [Required]
        public byte Semester { get; set; }

        public List<Subject> Subjects { get; set; }
        public List<Student> Student { get; set; }
    }
}
