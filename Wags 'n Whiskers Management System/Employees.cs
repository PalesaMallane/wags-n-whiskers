using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Wags__n_Whiskers_Management_System
{
    public partial class Employees : Form
    {
        //Dictionary to map buttons to their associated PictureBox and original colors
        private Dictionary<Button, PictureBox> buttonPictureBoxMapping = new Dictionary<Button, PictureBox>();
        private Dictionary<Button, Color> buttonOriginalColorMapping = new Dictionary<Button, Color>();

        SqlConnection Connection = new SqlConnection(@"Data Source = (LocalDB)\MSSQLLocalDB; AttachDbFilename=C:\Users\Palesa\Documents\Wags 'n Whiskers DB.mdf;Integrated Security = True; Connect Timeout = 30");

        public Employees()
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

        private void DisplayEmployees()
        {
            Connection.Open();
            string Query = "Select * from EmployeeTable";
            SqlDataAdapter adapter = new SqlDataAdapter(Query, Connection);
            SqlCommandBuilder builder = new SqlCommandBuilder(adapter);
            var ds = new DataSet();
            adapter.Fill(ds);
            dgvEmployee.DataSource = ds.Tables[0];
            Connection.Close(); ;
        }

        private void Clear()
        {
            txtEmpName.Text = "";
            txtEmpSurname.Text = "";
            cboEmpStatus.Text = "";
            txtPassword.Text = "";
            cboEmpPosition.Text = "";
            txtEmpPhone.Text = "";
        }

        private void btnEmpSave_Click(object sender, EventArgs e)
        {
            if (txtEmpName.Text == "" || txtEmpSurname.Text == "" || txtEmpPhone.Text == "" || cboEmpStatus.Text == "" || cboEmpPosition.Text == "" || txtPassword.Text == "") //Check if required fields are empty
            {
                MessageBox.Show("Fields cannot be empty", "Missing Information", MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation);
            }
            else
            {
                try
                {
                    Connection.Open();

                    // Generate the username based on initials and surname
                    string initials = GetInitials(txtEmpName.Text);
                    string surname = txtEmpSurname.Text;
                    string username = GenerateUsername(initials, surname);

                    SqlCommand cmd = new SqlCommand("Insert into EmployeeTable (EmpName, EmpSurname, EmpPhone, EmpStatus , EmpPosition, EmpUsername, EmpPassword) values(@EN, @ES, @EP,@Estat, @EPos, @EU, @EPass)", Connection);
                    cmd.Parameters.AddWithValue("@EN", txtEmpName.Text);
                    cmd.Parameters.AddWithValue("@ES", txtEmpSurname.Text);
                    cmd.Parameters.AddWithValue("@EP", txtEmpPhone.Text);
                    cmd.Parameters.AddWithValue("@EStat", cboEmpStatus.Text);
                    cmd.Parameters.AddWithValue("@EPos", cboEmpPosition.Text);
                    cmd.Parameters.AddWithValue("@EU", username);
                    cmd.Parameters.AddWithValue("@EPass", txtPassword.Text);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("New employee has been added successfully", "Employee Added", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DisplayEmployees();
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

        private string GetInitials(string name)
        {
            string[] nameParts = name.Split(' ');
            string initials = string.Empty;
            foreach (string part in nameParts)
            {
                initials += part[0];
            }
            return initials;
        }

        // Helper method to generate a unique username
        private string GenerateUsername(string initials, string surname)
        {
            // Combine initials and surname to form a username
            string username = initials + surname;

            return username;
        }

        int key = 0;
        private void btnEmpEdit_Click(object sender, EventArgs e)
        {
            if (txtEmpName.Text == "" || txtEmpSurname.Text == "" || txtEmpPhone.Text == "" || cboEmpStatus.Text == "" || cboEmpPosition.Text == "" || txtPassword.Text == "") //Check if required fields are empty
            {
                MessageBox.Show("Fields cannot be empty", "Missing Information", MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation);
            }
            else
            {
                try
                {
                    Connection.Open();
                    SqlCommand cmd = new SqlCommand("Update EmployeeTable set EmpName=@EN, EmpSurname=@ES, EmpPhone=@EP, EmpStatus=@EStat , EmpPosition=@EPos, EmpPassword=@EPass where EmpID = @EKey", Connection);
                    cmd.Parameters.AddWithValue("@EN", txtEmpName.Text);
                    cmd.Parameters.AddWithValue("@ES", txtEmpSurname.Text);
                    cmd.Parameters.AddWithValue("@EP", txtEmpPhone.Text);
                    cmd.Parameters.AddWithValue("@EStat", cboEmpStatus.Text);
                    cmd.Parameters.AddWithValue("@EPos", cboEmpPosition.Text);
                    cmd.Parameters.AddWithValue("@EPass", txtPassword.Text);
                    cmd.Parameters.AddWithValue("EKey", key);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Employee details have been successfully updated", "Employee Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DisplayEmployees();
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

        private void btnEmpDelete_Click(object sender, EventArgs e)
        {
            if (key == 0)
            {
                MessageBox.Show("Please select an emoloyee", "Make Selection", MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation);
            }
            else
            {
                try
                {
                    Connection.Open();
                    SqlCommand cmd = new SqlCommand("Delete from EmployeeTable where EmpID = @EKey", Connection);
                    cmd.Parameters.AddWithValue("@EKey", key);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Employee Deleted.", "Employee Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DisplayEmployees();
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

        private void btnCustomers_Click(object sender, EventArgs e)
        {
            Customers customer = new Customers();
            customer.Show();
            this.Hide();
        }

        private void btnBilling_Click(object sender, EventArgs e)
        {
            Billing billing = new Billing();
            billing.Show();
            this.Hide();
        }

        private void btnLogOut_Click(object sender, EventArgs e)
        {
            LogIn logIn = new LogIn();
            logIn.Show();
            this.Hide();
        }

        private void Employees_Load(object sender, EventArgs e)
        {
            DisplayEmployees();
            DisplayEmployeeName();
        }

        private void dgvEmployee_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow selectedRow = dgvEmployee.Rows[e.RowIndex];

                // Access cell values here.
                txtEmpName.Text = selectedRow.Cells["EmpName"].Value?.ToString();
                txtEmpSurname.Text = selectedRow.Cells["EmpSurname"].Value?.ToString();
                txtEmpPhone.Text = selectedRow.Cells["EmpPhone"].Value?.ToString();
                cboEmpStatus.Text = selectedRow.Cells["EmpStatus"].Value?.ToString();
                cboEmpPosition.Text = selectedRow.Cells["EmpPosition"].Value?.ToString();
                txtPassword.Text = selectedRow.Cells["EmpPassword"].Value?.ToString();

                // Optionally, set the 'key' variable based on your logic.
                if (string.IsNullOrEmpty(txtEmpName.Text) || string.IsNullOrEmpty(txtEmpSurname.Text))
                {
                    key = 0;
                }
                else
                {
                    key = Convert.ToInt32(selectedRow.Cells[0].Value);
                }
            }
        }

        private void lblEmpName_Click(object sender, EventArgs e)
        {

        }

        private void DisplayEmployeeName()
        {
            Connection.Open();

            SqlCommand cmd = new SqlCommand("SELECT EmpName, EmpSurname FROM EmployeeTable WHERE EmpID = @EID", Connection);
            cmd.Parameters.AddWithValue("@EID", key);
            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                string empName = reader["EmpName"].ToString();
                string empSurname = reader["EmpSurname"].ToString();
                lblEmpName.Text = empName;
                
            }
        }
    }
}
