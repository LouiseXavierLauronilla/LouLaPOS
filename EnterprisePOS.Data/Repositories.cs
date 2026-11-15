using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using EnterprisePOS.Domain;

namespace EnterprisePOS.Data
{
    public sealed class UserRepository
    {
        public User Login(string username, string password)
        {
            using (var c = Db.Open())
            using (var cmd = new SqlCommand("dbo.usp_Login", c))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Username", username);

                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read()) return null;

                    var hash = r["PasswordHash"] as string;
                    var salt = r["Salt"] as string;
                    if (!PasswordHasher.Verify(password, hash, salt)) return null;

                    return new User
                    {
                        Id = (int)r["UserId"],
                        Username = (string)r["Username"],
                        FullName = (string)r["FullName"],
                        Role = (UserRole)(int)r["RoleId"],
                        IsActive = (bool)r["IsActive"]
                    };
                }
            }
        }

        public int Create(User u, string password)
        {
            var salt = PasswordHasher.CreateSalt();
            var hash = PasswordHasher.Hash(password, salt);

            return Db.Execute(
                @"INSERT dbo.Users(Username,PasswordHash,Salt,FullName,RoleId,IsActive,CreatedAt)
                  VALUES(@u, @p, @s, @f, @r, 1, GETDATE())",
                new SqlParameter("@u", u.Username),
                new SqlParameter("@p", hash),
                new SqlParameter("@s", salt),
                new SqlParameter("@f", u.FullName),
                new SqlParameter("@r", (int)u.Role));
        }
    }

    public sealed class ProductRepository
    {
        public List<Product> GetAll(string search = "")
        {
            var list = new List<Product>();
            var pattern = string.IsNullOrWhiteSpace(search) ? string.Empty : "%" + search.Trim() + "%";

            using (var c = Db.Open())
            using (var cmd = new SqlCommand(@"SELECT ProductId, Sku, Barcode, Name, CategoryId, Price, Cost, StockQty, ReorderLevel, IsActive
                FROM dbo.Products
                WHERE (@Search = '' OR Sku LIKE @Pattern OR Barcode LIKE @Pattern OR Name LIKE @Pattern)
                ORDER BY Name", c))
            {
                cmd.Parameters.AddWithValue("@Search", string.IsNullOrWhiteSpace(search) ? string.Empty : search.Trim());
                cmd.Parameters.AddWithValue("@Pattern", pattern);

                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new Product
                        {
                            Id = (int)r["ProductId"],
                            Sku = (string)r["Sku"],
                            Barcode = r["Barcode"] as string,
                            Name = (string)r["Name"],
                            CategoryId = (int)r["CategoryId"],
                            Cost = (decimal)r["Cost"],
                            Price = (decimal)r["Price"],
                            StockQty = (int)r["StockQty"],
                            ReorderLevel = (int)r["ReorderLevel"],
                            Image = null,
                            IsActive = (bool)r["IsActive"]
                        });
                    }
                }
            }

            return list;
        }

        public int Save(Product p)
        {
            using (var c = Db.Open())
            using (var tx = c.BeginTransaction())
            {
                var imageValue = DBNull.Value;

                if (p.Id > 0)
                {
                    using (var cmd = new SqlCommand(@"UPDATE dbo.Products
                        SET Sku = @Sku,
                            Barcode = @Barcode,
                            Name = @Name,
                            CategoryId = @CategoryId,
                            Price = @Price,
                            Cost = @Cost,
                            StockQty = @StockQty,
                            ReorderLevel = @ReorderLevel,
                            Image = @Image,
                            UpdatedAt = GETDATE()
                        WHERE ProductId = @ProductId; SELECT @ProductId;", c, tx))
                    {
                        cmd.Parameters.AddWithValue("@ProductId", p.Id);
                        cmd.Parameters.AddWithValue("@Sku", p.Sku);
                        cmd.Parameters.AddWithValue("@Barcode", (object)p.Barcode ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Name", p.Name);
                        cmd.Parameters.AddWithValue("@CategoryId", p.CategoryId);
                        cmd.Parameters.AddWithValue("@Price", p.Price);
                        cmd.Parameters.AddWithValue("@Cost", p.Cost);
                        cmd.Parameters.AddWithValue("@StockQty", p.StockQty);
                        cmd.Parameters.AddWithValue("@ReorderLevel", p.ReorderLevel);
                        cmd.Parameters.Add("@Image", SqlDbType.VarBinary, -1).Value = imageValue;
                        var id = Convert.ToInt32(cmd.ExecuteScalar());
                        tx.Commit();
                        return id;
                    }
                }

                using (var cmd = new SqlCommand(@"INSERT INTO dbo.Products (Sku, Barcode, Name, CategoryId, Price, Cost, StockQty, ReorderLevel, Image, IsActive, CreatedAt, UpdatedAt)
                    VALUES (@Sku, @Barcode, @Name, @CategoryId, @Price, @Cost, @StockQty, @ReorderLevel, @Image, 1, GETDATE(), GETDATE());
                    SELECT CAST(SCOPE_IDENTITY() AS int);", c, tx))
                {
                    cmd.Parameters.AddWithValue("@Sku", p.Sku);
                    cmd.Parameters.AddWithValue("@Barcode", (object)p.Barcode ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Name", p.Name);
                    cmd.Parameters.AddWithValue("@CategoryId", p.CategoryId);
                    cmd.Parameters.AddWithValue("@Price", p.Price);
                    cmd.Parameters.AddWithValue("@Cost", p.Cost);
                    cmd.Parameters.AddWithValue("@StockQty", p.StockQty);
                    cmd.Parameters.AddWithValue("@ReorderLevel", p.ReorderLevel);
                    cmd.Parameters.Add("@Image", SqlDbType.VarBinary, -1).Value = imageValue;
                    var id = Convert.ToInt32(cmd.ExecuteScalar());
                    tx.Commit();
                    return id;
                }
            }
        }
    }

    public sealed class OrderRepository
    {
        public int Create(Order o)
        {
            if (o == null) throw new ArgumentNullException(nameof(o));
            if (o.Lines == null || o.Lines.Count == 0) throw new InvalidOperationException("Cart is empty.");

            using (var c = Db.Open())
            using (var tx = c.BeginTransaction())
            {
                var subtotal = 0m;
                foreach (var line in o.Lines)
                    subtotal += line.LineTotal;

                var total = (o.Total > 0m ? o.Total : subtotal + o.Tax - o.Discount);
                var amountPaid = total;
                var paymentMethod = "CASH";
                var orderNumber = "ORD-" + DateTime.Now.ToString("yyMMddHHmmss");

                var orderSql = @"INSERT INTO dbo.Orders (OrderNumber, CustomerId, UserId, OrderDate, Subtotal, TaxAmount, DiscountAmount, Total, PaymentMethod, AmountPaid, ChangeDue, Status)
                    VALUES (@OrderNumber, @CustomerId, @UserId, GETDATE(), @Subtotal, @TaxAmount, @DiscountAmount, @Total, @PaymentMethod, @AmountPaid, @AmountPaid - @Total, 'Completed');
                    SELECT CAST(SCOPE_IDENTITY() AS int);";

                using (var command = new SqlCommand(orderSql, c, tx))
                {
                    command.Parameters.AddWithValue("@OrderNumber", orderNumber);
                    command.Parameters.AddWithValue("@CustomerId", (object)o.CustomerId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@UserId", o.UserId);
                    command.Parameters.AddWithValue("@Subtotal", subtotal);
                    command.Parameters.AddWithValue("@TaxAmount", o.Tax);
                    command.Parameters.AddWithValue("@DiscountAmount", o.Discount);
                    command.Parameters.AddWithValue("@Total", total);
                    command.Parameters.AddWithValue("@PaymentMethod", paymentMethod);
                    command.Parameters.AddWithValue("@AmountPaid", amountPaid);
                    var orderId = Convert.ToInt32(command.ExecuteScalar());

                    foreach (var line in o.Lines)
                    {
                        using (var stockCheck = new SqlCommand(@"SELECT StockQty FROM dbo.Products WHERE ProductId = @ProductId", c, tx))
                        {
                            stockCheck.Parameters.AddWithValue("@ProductId", line.ProductId);
                            var stock = Convert.ToInt32(stockCheck.ExecuteScalar());
                            if (stock < line.Quantity)
                                throw new InvalidOperationException("Insufficient stock for product: " + line.ProductName);
                        }

                        using (var updateStock = new SqlCommand(@"UPDATE dbo.Products SET StockQty = StockQty - @Quantity, UpdatedAt = GETDATE() WHERE ProductId = @ProductId", c, tx))
                        {
                            updateStock.Parameters.AddWithValue("@ProductId", line.ProductId);
                            updateStock.Parameters.AddWithValue("@Quantity", line.Quantity);
                            updateStock.ExecuteNonQuery();
                        }

                        using (var detail = new SqlCommand(@"INSERT INTO dbo.OrderDetails (OrderId, ProductId, Quantity, UnitPrice, LineTotal)
                            VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice, @LineTotal)", c, tx))
                        {
                            detail.Parameters.AddWithValue("@OrderId", orderId);
                            detail.Parameters.AddWithValue("@ProductId", line.ProductId);
                            detail.Parameters.AddWithValue("@Quantity", line.Quantity);
                            detail.Parameters.AddWithValue("@UnitPrice", line.UnitPrice);
                            detail.Parameters.AddWithValue("@LineTotal", line.LineTotal);
                            detail.ExecuteNonQuery();
                        }
                    }

                    tx.Commit();
                    return orderId;
                }
            }
        }
    }

    public sealed class ReportRepository
    {
        public List<SalesSummary> Sales(DateTime from, DateTime to)
        {
            var a = new List<SalesSummary>();

            using (var c = Db.Open())
            using (var cmd = new SqlCommand(@"SELECT CAST(OrderDate AS date) AS SaleDay,
                    COUNT(*) AS OrderCount,
                    SUM(Subtotal) AS GrossSales,
                    SUM(Total) AS NetSales
                FROM dbo.Orders
                WHERE OrderDate >= @From AND OrderDate < @To
                GROUP BY CAST(OrderDate AS date)
                ORDER BY SaleDay", c))
            {
                cmd.Parameters.AddWithValue("@From", from.Date);
                cmd.Parameters.AddWithValue("@To", to.Date);

                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        a.Add(new SalesSummary
                        {
                            Day = (DateTime)r["SaleDay"],
                            OrderCount = (int)r["OrderCount"],
                            GrossSales = r["GrossSales"] == DBNull.Value ? 0m : (decimal)r["GrossSales"],
                            NetSales = r["NetSales"] == DBNull.Value ? 0m : (decimal)r["NetSales"]
                        });
                    }
                }
            }

            return a;
        }
    }
}

