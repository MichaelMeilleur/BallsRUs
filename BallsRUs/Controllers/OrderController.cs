using BallsRUs.Context;
using BallsRUs.Entities;
using BallsRUs.Models.Order;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BallsRUs.Controllers
{
    public class OrderController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public OrderController(UserManager<User> userManager, ApplicationDbContext context)
        {   
            _userManager = userManager;
            _context = context;
        }
      
        public IActionResult OrderDetails(Guid orderId)
        {
            ViewBag.OrderId = orderId;
            Order order = _context.Orders.FirstOrDefault(x => x.Id == orderId);
            OrderDetailsVM vm = null;

            if (order is not null)
            {
                vm = new()
                {
                    Id = order.Id,
                    ConfirmationDate = order.ConfirmationDate,
                    CreationDate = order.CreationDate,
                    EmailAddress = order.EmailAddress,
                    FirstName = order.FirstName,
                    LastName = order.LastName,
                    ModificationDate = order.ModificationDate,
                    Number = order.Number,
                    PaymentDate = order.PaymentDate,
                    PhoneNumber = order.PhoneNumber,
                    ProductQuantity = order.ProductQuantity,
                    ProductsCost = order.ProductsCost,
                    ShippingCost = order.ShippingCost,
                    Status = order.Status,
                    SubTotal = order.SubTotal,
                    Taxes = order.Taxes,
                    Total = order.Total,
                };

            }
            return View(vm);
        }
        public IActionResult DeleteOrder(Guid OrderId)
        {
            Order order = _context.Orders.FirstOrDefault(x => x.Id == OrderId);
            if (order != null)
            {
                _context.Orders.Remove(order);
                _context.SaveChanges();
            }
            return RedirectToAction("OrdersHistory","Account");
        }
        //Méthode qui renvoie le Id de l'utilisateur authentifié actuellement
       
    }
}
