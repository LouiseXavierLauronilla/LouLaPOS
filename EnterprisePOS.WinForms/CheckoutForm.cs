using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using EnterprisePOS.Domain;
using EnterprisePOS.Services;

namespace EnterprisePOS.WinForms
{
    public sealed partial class CheckoutForm : Form
    {
        private readonly User cashier;
        private readonly TextBox barcode = new TextBox { Width = 220 };
        private readonly NumericUpDown quantity = new NumericUpDown { Minimum = 1, Maximum = 999, Value = 1 };
        private readonly ComboBox paymentMethod = new ComboBox { Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly TextBox amountTendered = new TextBox { Width = 90 };
        private readonly DataGridView grid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false, ReadOnly = true, AllowUserToAddRows = false };
        private readonly Label total = new Label { AutoSize = true, Font = new Font("Segoe UI", 12, FontStyle.Bold) };
        private readonly Button checkout = new Button { Text = "Checkout" };
        private readonly Button preview = new Button { Text = "Preview receipt", Enabled = false };
        private readonly Button print = new Button { Text = "Print receipt", Enabled = false };
        private readonly List<CartLine> cart = new List<CartLine>();
        private ReceiptData lastReceipt;

        public CheckoutForm(User user)
        {
            cashier = user;
            Text = "New Order"; Width = 1050; Height = 650; StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.Font; paymentMethod.Items.AddRange(new object[] { "CASH", "CARD", "TRANSFER" }); paymentMethod.SelectedIndex = 0; paymentMethod.SelectedIndexChanged += (s, e) => amountTendered.Enabled = paymentMethod.Text == "CASH"; amountTendered.Text = "0.00";

            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(8) }; var add = new Button { Text = "Add" }; add.Click += AddLine; checkout.Click += Checkout; preview.Click += (s, e) => new ReceiptPrinter().Preview(lastReceipt); print.Click += (s, e) => new ReceiptPrinter().Print(lastReceipt);
            barcode.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) AddLine(s, e); };
            top.Controls.Add(new Label { Text = "SKU / barcode", AutoSize = true }); top.Controls.Add(barcode); top.Controls.Add(new Label { Text = "Qty", AutoSize = true }); top.Controls.Add(quantity); top.Controls.Add(add); top.Controls.Add(new Label { Text = "Payment", AutoSize = true }); top.Controls.Add(paymentMethod);
            top.Controls.Add(new Label { Text = "Tendered", AutoSize = true }); top.Controls.Add(amountTendered); top.Controls.Add(checkout);

            var footer = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 52, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(8) };
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductName", HeaderText = "Product", ReadOnly = true });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Sku", HeaderText = "SKU", ReadOnly = true });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Quantity", HeaderText = "Qty", ReadOnly = true });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "UnitPrice", HeaderText = "Unit Price", ReadOnly = true });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "LineTotal", HeaderText = "Line Total", ReadOnly = true });
            footer.Controls.Add(print); footer.Controls.Add(preview); footer.Controls.Add(total); Controls.Add(grid); Controls.Add(footer); Controls.Add(top); RefreshCart();
        }

        private void AddLine(object sender, EventArgs e)
        {
            var code = barcode.Text.Trim();
            if (code.Length == 0) return; try
            {
                var product = new InventoryService().Search(code).FirstOrDefault(x =>
                    string.Equals(x.Sku, code, StringComparison.OrdinalIgnoreCase) || string.Equals(x.Barcode, code, StringComparison.OrdinalIgnoreCase));
                if (product == null) { MessageBox.Show("Product not found."); return; }
                cart.Add(new CartLine { ProductId = product.Id, Sku = product.Sku, ProductName = product.Name, Quantity = (int)quantity.Value, UnitPrice = product.Price });
                barcode.Clear(); quantity.Value = 1; RefreshCart(); barcode.Focus();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Product lookup", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void RefreshCart()
        {
            grid.DataSource = null; grid.DataSource = cart.ToList();
            var subtotal = cart.Sum(x => x.LineTotal); 
            var tax = Math.Round(subtotal * .12m, 2);
            total.Text = "Total: " + (subtotal + tax).ToString("C2");
        }

        private void Checkout(object sender, EventArgs e)
        {
            try
            {
                if (cart.Count == 0) throw new InvalidOperationException("Cart is empty."); 
                var subtotal = cart.Sum(x => x.LineTotal); 
                var tax = Math.Round(subtotal * .12m, 2); 
                var totalValue = subtotal + tax; 
                decimal tendered = paymentMethod.Text == "CASH" ? ParseTendered() : totalValue;
                if (paymentMethod.Text == "CASH" && tendered < totalValue) throw new InvalidOperationException("Amount tendered is less than the order total.");
                var orderId = new SalesService().Checkout(cashier.Id, cart.ToList(), null, .12m, 0m); lastReceipt = new ReceiptData { BusinessName = "Enterprise POS", Address = "123 Main Street, City", Phone = "+1 555 0100", OrderNumber = "POS-" + orderId, Date = DateTime.Now, CashierName = cashier.FullName, Subtotal = subtotal, Tax = tax, Discount = 0m, Total = totalValue, PaymentMethod = paymentMethod.Text, AmountTendered = tendered, Change = Math.Max(0m, tendered - totalValue) }; 
                foreach (var line in cart) lastReceipt.Lines.Add(new ReceiptLine { ProductName = line.ProductName, Quantity = line.Quantity, Amount = line.LineTotal });
                preview.Enabled = true; print.Enabled = true;
                MessageBox.Show("Order created: " + orderId + "\nThe receipt is ready to preview or print.", "Checkout", MessageBoxButtons.OK, MessageBoxIcon.Information);
                cart.Clear(); RefreshCart();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Checkout failed", MessageBoxButtons.OK, MessageBoxIcon.Error); 
            }
        }

        private decimal ParseTendered()
        {
            
            if (!decimal.TryParse(amountTendered.Text, out decimal value) || value < 0) throw new InvalidOperationException("Enter a valid cash amount."); return value;
        }
    }
}
