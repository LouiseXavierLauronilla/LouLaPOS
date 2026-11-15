using System;
using System.Linq;
using System.Windows.Forms;
using EnterprisePOS.Services;
namespace EnterprisePOS.WinForms
{
    public sealed partial class AnalyticsForm : Form
    {
        DateTimePicker from = new DateTimePicker
        { 
            Value = DateTime.Today.AddDays(-30) }; 
        DateTimePicker to = new DateTimePicker { Value = DateTime.Today }; 
        DataGridView grid = new DataGridView
        { 
            Dock = DockStyle.Fill, AutoGenerateColumns = true }; 
        public AnalyticsForm()
        {
            Text = "Sales Analytics"; 
            Width = 850; Height = 500; 
            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 45 }; 
            var b = new Button
            { 
                Text = "Refresh" 
            }; 
            b.Click += (s, e) => RefreshRows(); 
            top.Controls.Add(from); 
            top.Controls.Add(to); 
            top.Controls.Add(b); 
            Controls.Add(grid); 
            Controls.Add(top); 
            RefreshRows();
        }
        void RefreshRows() 
        { 
            grid.DataSource = new ReportingService().Sales(from.Value.Date, to.Value.Date.AddDays(1))
                .Select(x => new { x.Day, x.OrderCount, x.GrossSales, x.NetSales })
                .ToList(); 
        }
    }
}
