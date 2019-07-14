using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using NorthwindMvc.Models;

using Packt.CS7;

namespace NorthwindMvc.Controllers
{
    public class HomeController : Controller
    {
        private Northwind db;
        public HomeController(Northwind injectedContext)
        {
            db = injectedContext;
        }
        public IActionResult Index()
        {
            var model = new HomeIndexViewModel
            {
                VisitorCount = (new Random()).Next(1, 1001),
                Categories = db.Categories.ToList(),
                Products = db.Products.ToList()
            };
            return View(model); // pass model to view 
        }
        public  async Task<IActionResult> ProductDetail(int? id)
        {
            if (!id.HasValue)
            {
                return NotFound("You must pass a product ID in the route, for example, /Home/ProductDetail/21");
            }
            var model = await db.Products.SingleOrDefaultAsync(p => p.ProductID == id);
            if (model == null)
            {
                return NotFound($"A product with the ID of {id} was not found.");
            }
            return View(model);
        }
        public async Task<IActionResult> CategoryProducts(int? id)
        {
            if (!id.HasValue)
            {
                return NotFound("You must pass a category ID in the route, for example, /Home/Categories/1");
            }
            var model = await db.Products.Include(p => p.Category).Include(
            p => p.Supplier).Where(p => p.CategoryID == id).ToArrayAsync();
            if (model.Count() == 0)
            {
                return NotFound($"No products with category {id}.");
            }
            ViewData["Category"] = model.Select(c => c.Category.CategoryName).First().ToString();
            return View(model); // передача модели представлению
        }

        public async Task<IActionResult> ProductsThatCostMoreThan(decimal? price)
        {
            if (!price.HasValue)
            {
                return NotFound("You must pass a product price in the query string, for example, / Home / ProductsThatCostMoreThan ? price = 50");
            }
            var model = await db.Products.Include(p => p.Category).Include(
            p => p.Supplier).Where(p => p.UnitPrice > price).ToArrayAsync();
            if (model.Count() == 0)
            {
                return NotFound($"No products cost more than {price:C}.");
            }
            ViewData["MaxPrice"] =  price.Value.ToString("C");
            return View(model); // передача модели представлению
        }
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
