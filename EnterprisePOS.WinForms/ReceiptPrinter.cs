using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using EnterprisePOS.Domain;
using EnterprisePOS.Services;

namespace EnterprisePOS.WinForms
{
    public sealed class ReceiptPrinter : IReceiptPrinter
    {
        private readonly PrintDocument document = new PrintDocument(); private readonly Font normal = new Font("Consolas", 8.5f); private readonly Font bold = new Font("Consolas", 8.5f, FontStyle.Bold); private ReceiptData current;

        public ReceiptPrinter()
        {
            document.DocumentName = "Enterprise POS Receipt"; document.DefaultPageSettings.PaperSize = new PaperSize("Receipt", 315, 900); document.DefaultPageSettings.Margins = new Margins(12, 12, 12, 12); document.PrintPage += PrintPage;
        }

        public void Print(ReceiptData receipt)
        {
            current = receipt ?? throw new ArgumentNullException("receipt");
            using (var dialog = new PrintDialog { Document = document, AllowSomePages = false })
            {
                if (dialog.ShowDialog() == DialogResult.OK) document.Print();
            }
        }

        public void Preview(ReceiptData receipt)
        {
            current = receipt ?? throw new ArgumentNullException("receipt");
            using (var preview = new PrintPreviewDialog { Document = document, Width = 900, Height = 700 }) preview.ShowDialog();
        }

        private void PrintPage(object sender, PrintPageEventArgs e)
        {
            var x = e.MarginBounds.Left; var y = e.MarginBounds.Top; var width = e.MarginBounds.Width;
            Center(e.Graphics, current.BusinessName, bold, ref y, x, width);
            Center(e.Graphics, current.Address, normal, ref y, x, width);
            Center(e.Graphics, current.Phone, normal, ref y, x, width);
            Text(e.Graphics, new string('-', 40), normal, ref y, x);
            Text(e.Graphics, "Order: " + current.OrderNumber, normal, ref y, x);
            Text(e.Graphics, "Date : " + current.Date.ToString("yyyy-MM-dd HH:mm"), normal, ref y, x);
            Text(e.Graphics, "Cashier: " + current.CashierName, normal, ref y, x);
            Text(e.Graphics, new string('-', 40), normal, ref y, x);
            Text(e.Graphics, "Product                  Qty     Amount", bold, ref y, x);

            foreach (var line in current.Lines)
            {
                var name = line.ProductName ?? string.Empty; if (name.Length > 23) name = name.Substring(0, 23);
                Text(e.Graphics, string.Format("{0,-23} {1,3} {2,10:C2}", name, line.Quantity, line.Amount), normal, ref y, x);
            }

            Text(e.Graphics, new string('-', 40), normal, ref y, x);
            Right(e.Graphics, "Subtotal: " + current.Subtotal.ToString("C2"), normal, ref y, x, width);
            Right(e.Graphics, "Tax: " + current.Tax.ToString("C2"), normal, ref y, x, width);
            Right(e.Graphics, "Discount: " + current.Discount.ToString("C2"), normal, ref y, x, width);
            Right(e.Graphics, "TOTAL: " + current.Total.ToString("C2"), bold, ref y, x, width);
            Text(e.Graphics, new string('-', 40), normal, ref y, x);
            Text(e.Graphics, "Payment: " + current.PaymentMethod, normal, ref y, x);
            Right(e.Graphics, "Tendered: " + current.AmountTendered.ToString("C2"), normal, ref y, x, width);
            Right(e.Graphics, "Change: " + current.Change.ToString("C2"), normal, ref y, x, width); y += 12; Center(e.Graphics, "Thank you for your purchase!", bold, ref y, x, width); e.HasMorePages = false;
        }
        private static void Text(Graphics g, string value, Font font, ref int y, int x) { g.DrawString(value, font, Brushes.Black, x, y); y += font.Height + 2; }

        private static void Center(Graphics g, string value, Font font, ref int y, int x, int width)
        { var size = g.MeasureString(value, font); g.DrawString(value, font, Brushes.Black, x + (width - size.Width) / 2, y); y += font.Height + 2; }

        private static void Right(Graphics g, string value, Font font, ref int y, int x, int width)
        { var size = g.MeasureString(value, font); g.DrawString(value, font, Brushes.Black, x + width - size.Width, y); y += font.Height + 2; }
    }
}
