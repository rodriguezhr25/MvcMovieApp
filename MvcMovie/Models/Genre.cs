using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace MvcMovie.Models
{
    public class Genre
    {
        public int ID { get; set; }

        [Required, StringLength(30, MinimumLength = 3)]
        public string GenreName { get; set; }
 
    }
}
