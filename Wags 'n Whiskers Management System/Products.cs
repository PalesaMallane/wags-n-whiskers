using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Wags__n_Whiskers_Management_System
{
    public partial class Products : Form
    {
        //Dictionary to map buttons to their associated PictureBox and original colors
        private Dictionary<Button, PictureBox> buttonPictureBoxMapping = new Dictionary<Button, PictureBox>();
        private Dictionary<Button, Color> buttonOriginalColorMapping = new Dictionary<Button, Color>();

        SqlConnection Connection = new SqlConnection(@"Data Source = (LocalDB)\MSSQLLocalDB; AttachDbFilename=C:\Users\Palesa\Documents\Wags 'n Whiskers DB.mdf;Integrated Security = True; Connect Timeout = 30");
        public Products()
        {
            InitializeComponent();
            
            buttonPictureBoxMapping.Add(btnProduct, pbxProduct);
            buttonPictureBoxMapping.Add(btnCustomers, pbxCustomers);
            buttonPictureBoxMapping.Add(btnEmployees, pbxEmployees);
            buttonPictureBoxMapping.Add(btnBilling, pbxBilling);
            buttonPictureBoxMapping.Add(btnLogOut, pbxLogOut);

            // Store the original colors in the dictionary
            
            buttonOriginalColorMapping.Add(btnProduct, pbxProduct.BackColor);
            buttonOriginalColorMapping.Add(btnCustomers, pbxCustomers.BackColor);
            buttonOriginalColorMapping.Add(btnEmployees, pbxEmployees.BackColor);
            buttonOriginalColorMapping.Add(btnBilling, pbxBilling.BackColor);
            buttonOriginalColorMapping.Add(btnLogOut, pbxLogOut.BackColor);


            // Initialize event handlers for buttons
            foreach (var button in buttonPictureBoxMapping.Keys)
            {
                button.MouseEnter += Button_MouseEnter;
                button.MouseLeave += Button_MouseLeave;
            }
        }

        private void Button_MouseEnter(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            if (buttonPictureBoxMapping.TryGetValue(button, out PictureBox pictureBox) && pictureBox != null)
            {
                // Change the BackColor of the associated PictureBox for hover
                pictureBox.BackColor = Color.DarkGreen; // Change to your hover color
            }
        }

        private void Button_MouseLeave(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            if (buttonPictureBoxMapping.TryGetValue(button, out PictureBox pictureBox) && pictureBox != null)
            {
                // Retrieve the original back color from the dictionary
                if (buttonOriginalColorMapping.TryGetValue(button, out Color originalColor))
                {
                    // Restore the original BackColor of the associated PictureBox
                    pictureBox.BackColor = originalColor;
                }
            }
        }

        private void DisplayProducts()
        {
            Connection.Open();
            string Query = "Select * from ProductTable";
            SqlDataAdapter adapter = new SqlDataAdapter(Query, Connection);
            SqlCommandBuilder builder = new SqlCommandBuilder(adapter);
            var ds = new DataSet();
            adapter.Fill(ds);
            dgvProducts.DataSource = ds.Tables[0];
            Connection.Close(); ;
        }

        private void Clear()
        {
            txtProdName.Text = "";
            cboProdCategory.Text = "";
            nudProdPrice.Text = "";
            nudProdQty.Text = "";
        }

        private void btnProdSave_Click(object sender, EventArgs e)
        {
            if (txtProdName.Text == "" || cboProdCategory.Text == "" || nudProdPrice.Text == "" || nudProdQty.Text == "") //Check if required fields are empty
            {
                MessageBox.Show("Fields cannot be empty", "Missing Information", MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation);
            }
            else
            {
                try
                {
                    Connection.Open();
                    SqlCommand cmd = new SqlCommand("Insert into ProductTable (ProdName, ProdCategory, ProdPrice, ProdQuantity ) values(@PN, @PC, @PP, @PQ)", Connection);
                    cmd.Parameters.AddWithValue("@PN", txtProdName.Text);
                    cmd.Parameters.AddWithValue("@PC", cboProdCategory.Text);
                    cmd.Parameters.AddWithValue("@PP", nudProdPrice.Value);
                    cmd.Parameters.AddWithValue("@PQ", nudProdQty.Value);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("New product has been added successfully", "Product Added", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DisplayProducts();
                    Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Exception Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    Connection.Close();
                }
            }
        }
        int key = 0;
        private void dgvProducts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow selectedRow = dgvProducts.Rows[e.RowIndex];

                // Access cell values here.
                txtProdName.Text = selectedRow.Cells["ProdName"].Value?.ToString();
                cboProdCategory.Text = selectedRow.Cells["ProdCategory"].Value?.ToString();
                nudProdQty.Text = selectedRow.Cells["ProdQuantity"].Value?.ToString();
                nudProdPrice.Text = selectedRow.Cells["ProdPrice"].Value?.ToString();

                // Optionally, set the 'key' variable based on your logic.
                if (string.IsNullOrEmpty(txtProdName.Text))
                {
                    key = 0;
                }
                else
                {
                    key = Convert.ToInt32(selectedRow.Cells[0].Value);
                }
            }
        }

        private void btnProdEdit_Click(object sender, EventArgs e)
        {
            if (txtProdName.Text == "" || cboProdCategory.Text == "" || nudProdPrice.Text == "" || nudProdQty.Text == "" ) //Check if required fields are empty
            {
                MessageBox.Show("Fields cannot be empty", "Missing Information", MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation);
            }
            else
            {
                try
                {
                    Connection.Open();
                    SqlCommand cmd = new SqlCommand("Update ProductTable set ProdName=@PN, ProdCategory=@PC, ProdPrice=@PP, ProdQuantity=@PQ where ProdID = @PKey", Connection);
                    cmd.Parameters.AddWithValue("@PN", txtProdName.Text);
                    cmd.Parameters.AddWithValue("@PC", cboProdCategory.Text);
                    cmd.Parameters.AddWithValue("@PP", nudProdPrice.Text);
                    cmd.Parameters.AddWithValue("@PQ", nudProdQty.Text);
                    cmd.Parameters.AddWithValue("PKey", key);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Product details have been successfully updated!", "Employee Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DisplayProducts();
                    Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Exception Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    Connection.Close();
                }
            }
        }

        private void btnCustomers_Click(object sender, EventArgs e)
        {
            Customers customers = new Customers();
            customers.Show();
            this.Close();
        }

        private void btnBilling_Click(object sender, EventArgs e)
        {
            Billing billing = new Billing();
            billing.Show();
            this.Close();
        }

        private void btnEmployees_Click(object sender, EventArgs e)
        {
            Employees employees = new Employees();
            employees.Show();
            this.Close();
        }

        private void btnLogOut_Click(object sender, EventArgs e)
        {
            LogIn logIn = new LogIn(); 
            logIn.Show();
            this.Close();               
        }

        private void Products_Load(object sender, EventArgs e)
        {
            DisplayProducts();
            lblEmpName.Text = LogIn.LoggedInEmployee;
        }

        private void btnProdDelete_Click(object sender, EventArgs e)
        {
            if (key == 0)
            {
                MessageBox.Show("Please select a product", "Make Selection", MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation);
            }
            else
            {
                try
                {
                    Connection.Open();
                    SqlCommand cmd = new SqlCommand("Delete from ProductTable where ProdID = @PID", Connection);
                    cmd.Parameters.AddWithValue("@PID", key);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Product Deleted.", "Product Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DisplayProducts();
                    Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Exception Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    Connection.Close();
                }
            }
        }
    }
}
