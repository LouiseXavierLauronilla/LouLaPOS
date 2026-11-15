using System; 
using System.Linq; 
using System.Windows.Forms; 
using EnterprisePOS.Domain; 
using EnterprisePOS.Services; 
namespace EnterprisePOS.WinForms { 
    public sealed partial class ProductForm:Form {
        DataGridView grid=new DataGridView{Dock=DockStyle.Fill,AutoGenerateColumns=true,ReadOnly=true};
        TextBox search=new TextBox{Width=220}; 
        public ProductForm(){Text="Inventory Management";Width=1000;Height=650;
            var top=new FlowLayoutPanel{Dock=DockStyle.Top,Height=45};
            var b=new Button{Text="Search"};
            b.Click+=(s,e)=>LoadRows();
            var add=new Button{Text="Add Product"};
            add.Click+=(s,e) =>
            {
                using (var editor = new ProductEditorForm())
                {
                    if (editor.ShowDialog(this) == DialogResult.OK)
                        LoadRows();
                }
            };
            top.Controls.Add(new Label{Text="Search"});top.Controls.Add(search);top.Controls.Add(b);top.Controls.Add(add);Controls.Add(grid);Controls.Add(top);LoadRows();}
        void LoadRows(){grid.DataSource=new InventoryService().Search(search.Text).Select(x=>new{ x.Id,x.Sku,x.Barcode,x.Name,x.Price,x.StockQty,x.ReorderLevel,x.IsActive}).ToList();
        }
    } 
} 
