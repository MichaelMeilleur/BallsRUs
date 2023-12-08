﻿using BallsRUs.Context;
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
        public IActionResult CancelOrder(Guid OrderId)
        {
            Order? orderToCancel = _context.Orders.Find(OrderId);

            if (orderToCancel is null)
                throw new ArgumentOutOfRangeException(nameof(OrderId));

            if (orderToCancel.Status != OrderStatus.Canceled)
            {
                List<OrderItem> orderToCancelItems = _context.OrderItems.Where(oi => oi.OrderId == orderToCancel.Id).ToList();

                foreach (OrderItem item in orderToCancelItems)
                {
                    Product? product = _context.Products.Find(item.ProductId);

                    if (product is not null)
                        product.Quantity += item.Quantity;
                }

                orderToCancel.Status = OrderStatus.Canceled;
                _context.SaveChanges();
            }

            return RedirectToAction("OrdersHistory", "Account");
        }
    }
}
