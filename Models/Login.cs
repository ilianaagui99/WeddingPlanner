using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WeddingPlanner.Models
{
    public class Login 
        {
            [Required]
            [EmailAddress]
            public string Email {get; set;}

            [Required]
            [DataType(DataType.Password)]
            public string Password {get; set;}

        }
}