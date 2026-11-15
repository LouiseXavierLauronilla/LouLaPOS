using System;
using System.Collections.Generic;
using System.Linq;
using EnterprisePOS.Data;
using EnterprisePOS.Domain;

namespace EnterprisePOS.Services
{
    public sealed class AuthService
    {
        readonly UserRepository repo = new UserRepository(); 
        public User Login(string u, string p) { return repo.Login(u, p); }
    }
    public sealed class InventoryService { readonly ProductRepository repo = new ProductRepository(); 
        public List<Product> Search(string q) { return repo.GetAll(q); } 
        public int Save(Product p) { if (string.IsNullOrWhiteSpace(p.Sku) || string.IsNullOrWhiteSpace(p.Name) || p.Price < 0) throw new ArgumentException("SKU, name, and a non-negative selling price are required."); 
            return repo.Save(p); } }
    public sealed class SalesService
    {
        readonly OrderRepository repo = new OrderRepository(); 
        public int Checkout(int userId, List<CartLine> lines, int? customerId, decimal taxRate, decimal discount)
        {
            if (lines == null || lines.Count == 0) throw new InvalidOperationException("Cart is empty."); var subtotal = lines.Sum(x => x.LineTotal); return repo.Create(new
    Order
            { UserId = userId, CustomerId = customerId, Lines = lines, Subtotal = subtotal, Tax = Math.Round(subtotal * taxRate, 2), Discount = discount, Total = subtotal + Math.Round(subtotal * taxRate, 2) - discount });
        }
    }
    public sealed class ReportingService { readonly ReportRepository repo = new ReportRepository(); public List<SalesSummary> Sales(DateTime from, DateTime to) { return repo.Sales(from, to); } }
}
