using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mass_pnr_lookup.Models
{
    public class BatchLine
    {
        [Key]
        public int BatchElementId { get; set; }

        public Batch Batch { get; set; }

        public int Row { get; set; }
        public string Address { get; set; }
        public string Name { get; set; }
        public string PNR { get; set; }
        public string Error { get; set; }


        public BatchLine(string name, string address)
        {
            Address = address;
            Name = name;
        }

    }
}