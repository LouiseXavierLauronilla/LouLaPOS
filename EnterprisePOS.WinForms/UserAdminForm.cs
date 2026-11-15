using System;
using System.Windows.Forms;
using EnterprisePOS.Data;
using EnterprisePOS.Domain;
namespace EnterprisePOS.WinForms
{
    public sealed partial class UserAdminForm : Form
    {
        TextBox username = new TextBox(), display = new TextBox(), password = new TextBox(); 
        ComboBox role = new ComboBox { DataSource = Enum.GetValues(typeof(UserRole)) }; 
        Button save = new Button { Text = "Create user" }; 
        public UserAdminForm()
        {
            Text = "User & Role Management"; Width = 450; Height = 280; var t = new TableLayoutPanel
            { Dock = DockStyle.Fill, Padding = new Padding(20), ColumnCount = 2, RowCount = 4 }; 
            Add(t, "Username", username, 0); 
            Add(t, "Display name", display, 1); 
            Add(t, "Password", password, 2); 
            Add(t, "Role", role, 3); 
            save.Click += Save; 
            t.Controls.Add(save, 1, 4); 
            Controls.Add(t);
        }
        void Add(TableLayoutPanel t, string l, Control c, int r) { t.Controls.Add(new Label { Text = l, AutoSize = true }, 0, r); t.Controls.Add(c, 1, r); }
        void Save(object s, EventArgs e)
        {
            try
            {
                new UserRepository().Create(new User
                { Username = username.Text, FullName = display.Text, Role = (UserRole)role.SelectedItem }, password.Text); MessageBox.Show("User created.");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}

