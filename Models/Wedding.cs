using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WeddingPlanner.Models
{
    public class Wedding
        {
                [Key]
                public int WeddingId { get; set; }

               [Required]
                [MinLength(2)]
                public string WedderOne {get; set;}

                [Required]
                [MinLength(2)]
                public string WedderTwo {get; set;}

                [Required]
                public DateTime date {get; set;} 

                [Required]
                public string address {get; set;}
                public DateTime CreatedAt {get; set;} = DateTime.Now;
                public DateTime UpdatedAt {get; set;} = DateTime.Now;

                //to keep track of guests
                public int UserId {get;set;}
                //to know who created this wedding
                public User Creator {get;set;}

                //list of rsvps
                public List<Association> UsersWhoRSVP{ get; set; }


        }
}