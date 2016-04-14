using mass_pnr_lookup.Queues;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices.AccountManagement;
using mass_pnr_lookup.Models;
using System.Net.Mail;

namespace mass_pnr_lookup.tests
{
    [TestClass]
    public class UserTests
    {
        [TestMethod]
        public void GetUserEmail_Current()
        {
            /*
            For Windows 10 only
            -------------------------
            Under
            HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows NT\CurrentVersion

            Add empty string values:
                - RegisteredOwner 
                - RegisteredOrganization
            */
            var user = new User() { Name = Environment.UserName };
            var mail = user.GetUserPrincipal(ContextType.Machine);
        }
    }
}
