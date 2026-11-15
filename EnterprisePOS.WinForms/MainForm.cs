using EnterprisePOS.Domain;
using System;
using System.Drawing;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
namespace EnterprisePOS.WinForms
{
    public sealed partial class MainForm : Form
    {
        readonly User user; public MainForm(User u)
        {
            user = u; Text = "Enterprise POS | "+u.FullName;WindowState=FormWindowState.Maximized;
            var menu=new MenuStrip();
            var inv=new ToolStripMenuItem("Inventory"); 
            inv.DropDownItems.Add("Products", null, (s, e) => new ProductForm().ShowDialog(this)); 
            var sales = new ToolStripMenuItem("Sales"); 
            sales.DropDownItems.Add("New Order", null, (s, e) => new CheckoutForm(user).ShowDialog(this)); 
            sales.DropDownItems.Add("Sales Analytics", null, (s, e) => new AnalyticsForm().ShowDialog(this)); 
            menu.Items.Add(inv); 
            menu.Items.Add(sales); 
            if (u.Role == UserRole.Admin)
            {
                var admin = new ToolStripMenuItem("Administration"); admin.DropDownItems.Add("Users", null, (s, e) => new UserAdminForm().ShowDialog(this)); menu.Items.Add(admin);
            }
            Controls.Add(menu); var title = new Label
            {
                Text = "Enterprise POS & Inventory Management",Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleCenter,Font=new Font("SegoeUI",28,FontStyle.Bold),ForeColor=Color.DarkSlateBlue};Controls.Add(title);MainMenuStrip=menu;
        }
    } 
} 
