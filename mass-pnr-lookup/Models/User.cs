using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.DirectoryServices.AccountManagement;

namespace mass_pnr_lookup.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Index(IsUnique = true)]
        [MinLength(1)]
        [MaxLength(255)]
        public string Name { get; set; }

        public virtual ICollection<Batch> Batches { get; set; }

        public UserPrincipal GetUserPrincipal(ContextType contextType)
        {
            var context = new PrincipalContext(contextType);
            var user = UserPrincipal.FindByIdentity(context, this.Name);
            return user;
        }
    }
}