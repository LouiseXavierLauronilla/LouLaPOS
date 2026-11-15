using System;
using System.Globalization;
using System.Windows.Forms;
using EnterprisePOS.Domain;
using EnterprisePOS.Services;
namespace EnterprisePOS.WinForms
{
    public sealed partial class ProductEditorForm : Form
    {
        TextBox sku = new TextBox(), barcode = new TextBox(), name = new TextBox(), cost = new TextBox(), price = new TextBox(), stock = new TextBox(), reorder = new TextBox(); Button save = new Button { Text = "Save" }; 
        public ProductEditorForm()
        { Text = "Product"; Width = 460; Height = 360; var t = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(15), ColumnCount = 2, RowCount = 8 }; Add(t, "SKU", sku, 0); Add(t, "Barcode", barcode, 1); Add(t, "Name", name, 2); Add(t, "Cost", cost, 3); Add(t, "Selling price", price, 4); Add(t, "Stock", stock, 5); Add(t, "Reorder level", reorder, 6); save.Click += Save; t.Controls.Add(save, 1, 7); Controls.Add(t); cost.Text = "0"; price.Text = "0"; stock.Text = "0"; reorder.Text = "0"; }
        void Add(TableLayoutPanel t, string label, Control c, int r)
        { t.Controls.Add(new Label { Text = label, AutoSize = true }, 0, r); t.Controls.Add(c, 1, r); }
        void Save(object s, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sku.Text) || string.IsNullOrWhiteSpace(name.Text))
                    throw new InvalidOperationException("SKU and Name are required.");

                decimal costValue;
                decimal priceValue;
                int stockQty;
                int reorderLevel;

                if (!decimal.TryParse(cost.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out costValue))
                    if (!decimal.TryParse(cost.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out costValue))
                        throw new InvalidOperationException("Cost must be a valid number.");

                if (!decimal.TryParse(price.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out priceValue))
                    if (!decimal.TryParse(price.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out priceValue))
                        throw new InvalidOperationException("Selling price must be a valid number.");

                if (!int.TryParse(stock.Text, out stockQty) || stockQty < 0)
                    throw new InvalidOperationException("Stock must be a non-negative number.");

                if (!int.TryParse(reorder.Text, out reorderLevel) || reorderLevel < 0)
                    throw new InvalidOperationException("Reorder level must be a non-negative number.");

                new InventoryService().Save(new Product
                {
                    Sku = sku.Text.Trim(),
                    Barcode = barcode.Text.Trim(),
                    Name = name.Text.Trim(),
                    Cost = costValue,
                    Price = priceValue,
                    StockQty = stockQty,
                    ReorderLevel = reorderLevel,
                    CategoryId = 1,
                    IsActive = true
                });

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }
    }
}
