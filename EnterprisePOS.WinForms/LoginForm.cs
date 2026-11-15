using System;
using System.Drawing;
using System.Windows.Forms;
using EnterprisePOS.Domain;
using EnterprisePOS.Services;
namespace EnterprisePOS.WinForms
{
    public sealed partial class LoginForm : Form
    {
        TextBox user = new TextBox(), pass = new TextBox(); 
        Button login = new Button(); 
        Label status = new Label(); 
        public LoginForm()
    { Text = "Enterprise POS - Login";
            Width=420;Height=260;StartPosition=FormStartPosition.CenterScreen;
            var panel=new TableLayoutPanel{Dock=DockStyle.Fill,Padding=new Padding(30),RowCount=4,ColumnCount=2};
            panel.Controls.Add(new Label{ Text = "Username",AutoSize = true},0,0); 
            panel.Controls.Add(user, 1, 0); 
            panel.Controls.Add(new Label{ Text = "Password", AutoSize = true }, 0, 1); 
            pass.PasswordChar = '*'; 
            panel.Controls.Add(pass, 1, 1); 
            login.Text = "Sign in";
            login.Dock=DockStyle.Fill;
            login.Click+=DoLogin;panel.Controls.Add(login,1,2);
            status.ForeColor=Color.Firebrick;
            panel.Controls.Add(status,0,3);
            panel.SetColumnSpan(status,2);
            Controls.Add(panel);
        }
        void DoLogin(object s,EventArgs e)
        {
            var u=new AuthService().Login(user.Text.Trim(), pass.Text); 
            if (u == null) { status.Text = "Invalid credentials or inactive user."; 
                return; 
            }
            Hide(); using (var f = new MainForm(u)) f.ShowDialog(); Show();
        }
    }
}
