using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Packt.CS7;

namespace NorthwindWeb.Pages
{
    public class CustomersModel : PageModel
    {
        public IEnumerable<string> Customers { get; set; }
     //   public IActionResult Detail(string id) => (db.Customers.FirstOrDefault(a => a.ContactName. == id));
      //  public IEnumerable<string> Countryes { get; set; }
      //  public IEnumerable<string> GroupCustomers { get; set; }

        private Northwind db;
        public CustomersModel(Northwind injectedContext)
        {
            db = injectedContext;
        }

        public void OnGet()
        {
            ViewData["Title"] = "Northwind Web Site - Customers";
           // var Countryes = db.Customers.OrderBy(c => c.Country).Select(country => country.Country).Distinct().ToArray();
            Customers = db.Customers.OrderBy(customer => customer.Country).Select(customer => customer.ContactName).ToArray();

           // GroupCustomers = Countryes.Join()
        }
    }
}
