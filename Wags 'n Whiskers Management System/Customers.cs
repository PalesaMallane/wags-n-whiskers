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
    public partial class Customers : Form
    {
        //Dictionary to map buttons to their associated PictureBox and original colors
        private Dictionary<Button, PictureBox> buttonPictureBoxMapping = new Dictionary<Button, PictureBox>();
        private Dictionary<Button, Color> buttonOriginalColorMapping = new Dictionary<Button, Color>();

        SqlConnection Connection = new SqlConnection(@"Data Source = (LocalDB)\MSSQLLocalDB; AttachDbFilename=C:\Users\Palesa\Documents\Wags 'n Whiskers DB.mdf;Integrated Security = True; Connect Timeout = 30");
        public Customers()
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

        private void DisplayCustomers()
        {
            Connection.Open();
            string Query = "Select * from CustomerTable";
            SqlDataAdapter adapter = new SqlDataAdapter(Query, Connection);
            SqlCommandBuilder builder = new SqlCommandBuilder(adapter);
            var ds = new DataSet();
            adapter.Fill(ds);
            dgvCustomers.DataSource = ds.Tables[0];
            Connection.Close(); ;
        }

        private void Clear()
        {
            txtCustName.Text = "";                      
            txtCustEmail.Text = "";           
            txtCustPhone.Text = "";
        }

        private void btnCustSave_Click(object sender, EventArgs e)
        {
            if (txtCustName.Text == "" || txtCustSurname.Text == "" || txtCustPhone.Text == "" || txtCustEmail.Text == "" || txtAmountDue.Text == "") //Check if required fields are empty
            {
                MessageBox.Show("Fields cannot be empty", "Missing Information", MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation);
            }
            else
            {
                try
                {
                    Connection.Open();

                    
                    SqlCommand cmd = new SqlCommand("Insert into CustomerTable (CustName, CustSurname, CustPhone, CustEmailAdd , CustAmountDue) values(@CN, @CS, @EP,@CE, @CAD)", Connection);
                    cmd.Parameters.AddWithValue("@CN", txtCustName.Text);
                    cmd.Parameters.AddWithValue("@CS", txtCustSurname.Text);
                    cmd.Parameters.AddWithValue("@EP", txtCustPhone.Text);
                    cmd.Parameters.AddWithValue("@CE", txtCustEmail.Text);
                    cmd.Parameters.AddWithValue("@CAD", txtAmountDue.Text);
                    
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("New customer has been added successfully", "Customer Added", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Connection.Close();
                    DisplayCustomers();
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

        private void Customers_Load(object sender, EventArgs e)
        {
            DisplayCustomers();
            lblEmpName.Text = LogIn.LoggedInEmployee;
        }

        int key = 0; // Variable to store the key for editing and deleting products
        private void btnCustEdit_Click(object sender, EventArgs e)
        {
            if (txtCustName.Text == "" || txtCustSurname.Text == "" || txtCustPhone.Text == "" || txtCustEmail.Text == "" || txtAmountDue.Text == "") //Check if required fields are empty
            {
                MessageBox.Show("Fields cannot be empty", "Missing Information", MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation);
            }
            else
            {
                try
                {
                    Connection.Open();
                    SqlCommand cmd = new SqlCommand("Update CustomerTable set CustName=@CN, CustSurname=@CS, CustPhone=@CP, CustAmountDue=@CAD where CustID = @CKey", Connection);
                    cmd.Parameters.AddWithValue("@CN", txtCustName.Text);
                    cmd.Parameters.AddWithValue("@CS", txtCustSurname.Text);
                    cmd.Parameters.AddWithValue("@EP", txtCustPhone.Text);
                    cmd.Parameters.AddWithValue("@CE", txtCustEmail.Text);
                    cmd.Parameters.AddWithValue("@CAD", txtAmountDue.Text);
                    cmd.Parameters.AddWithValue("CKey", key);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Customer details have been successfully updated!", "Customer Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Connection.Close();
                    DisplayCustomers();
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

        private void btnCustDelete_Click(object sender, EventArgs e)
        {
            if (key == 0)
            {
                MessageBox.Show("Please select a customer", "Make Selection", MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation);
            }
            else
            {
                try
                {
                    Connection.Open();
                    SqlCommand cmd = new SqlCommand("Delete from CustomerTable where CustID = @CKey", Connection);
                    cmd.Parameters.AddWithValue("@CKey", key);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Customer Deleted.", "Customer Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);               
                    DisplayCustomers();
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

        private void btnProduct_Click(object sender, EventArgs e)
        {
            Products products = new Products();
            products.Show();
            this.Hide();
        }

        private void btnBilling_Click(object sender, EventArgs e)
        {
            Billing billing = new Billing();
            billing.Show();
            this.Hide();
        }

        private void btnEmployees_Click(object sender, EventArgs e)
        {
            Employees employees = new Employees();
            employees.Show();
            this.Hide();
        }

        private void btnLogOut_Click(object sender, EventArgs e)
        {
            LogIn logIn = new LogIn();  
            logIn.Show();
            this.Hide();
        }

        private void dgvCustomers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow selectedRow = dgvCustomers.Rows[e.RowIndex];

                // Access cell values here.
                txtCustName.Text = selectedRow.Cells["CustName"].Value?.ToString();
                txtCustSurname.Text = selectedRow.Cells["CustSurname"].Value?.ToString();
                txtCustPhone.Text = selectedRow.Cells["CustPhone"].Value?.ToString();
                txtAmountDue.Text = selectedRow.Cells["CustAmountDue"].Value?.ToString();
                txtCustEmail.Text = selectedRow.Cells["CustEmailAdd"].Value?.ToString();
                               
                if (string.IsNullOrEmpty(txtCustName.Text) || string.IsNullOrEmpty(txtCustSurname.Text))
                {
                    key = 0;
                }
                else
                {
                    key = Convert.ToInt32(selectedRow.Cells[0].Value);
                }
            }
        }
    }
}
