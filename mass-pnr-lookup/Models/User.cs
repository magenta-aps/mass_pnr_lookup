using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mass_pnr_lookup.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Index(IsUnique = true)]
        [MinLength(1)]
        public string Name { get; set; }

        ICollection<Batch> Batches { get; set; }
    }
}