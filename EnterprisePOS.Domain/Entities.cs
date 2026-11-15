using System; 
using System.Collections.Generic; 
 
namespace EnterprisePOS.Domain 
{ 
    public enum UserRole { Admin = 1, Manager = 2, Cashier = 3 } 
 
    public sealed class User 
    { 
        public int Id; 
        public string Username; 
        public string PasswordHash; 
        public string FullName;         
        public UserRole Role; 
        public bool IsActive; 
    } 
 
    public sealed class Category { public int Id; public string Name; public bool IsActive; } 
 
    public sealed class Product 
    { 
        public int Id; 
        public string Sku; 
        public string Barcode; 
        public string Name; 
        public int CategoryId;         
        public decimal Cost; 
        public decimal Price; 
        public int StockQty; 
        public int ReorderLevel;         
        public string Image; 
        public bool IsActive;     } 
 
    public sealed class Customer { public int Id; public string Name; public string Phone; public string Email; public string Address; } 
 
    public sealed class CartLine 
    { 
        public int ProductId { get; set; }
        public string Sku { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get { return Quantity * UnitPrice; } }
    } 
 
    public sealed class Order 
    { 
        public int Id; 
        public string OrderNumber; 
        public int? CustomerId; 
        public int UserId;         
        public decimal Subtotal; 
        public decimal Tax; 
        public decimal Discount; 
        public decimal Total;         
        public DateTime OrderedAt; 
        public List<CartLine> Lines = new List<CartLine>(); 
    } 
 
    public sealed class SalesSummary 
    { 
        public DateTime Day { get; set; }
        public int OrderCount { get; set; }
        public decimal GrossSales { get; set; }
        public decimal NetSales { get; set; }
    } 
 
    public sealed class ReceiptLine 
    { 
        public string ProductName { get; set; }         
        public int Quantity { get; set; } 
        public decimal Amount { get; set; }     
    } 
 
    public sealed class ReceiptData 
    { 
        public string BusinessName { get; set; }         
        public string Address { get; set; }         
        public string Phone { get; set; }         
        public string OrderNumber { get; set; }         
        public DateTime Date { get; set; }         
        public string CashierName { get; set; }         
        public List<ReceiptLine> Lines { get; } = new List<ReceiptLine>();         
        public decimal Subtotal { get; set; }         
        public decimal Tax { get; set; }         
        public decimal Discount { get; set; }        
        public decimal Total { get; set; }         
        public string PaymentMethod { get; set; } 
        public decimal AmountTendered { get; set; }         
        public decimal Change { get; set; } 
    } 
} 
