using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Wags__n_Whiskers_Management_System
{
    public partial class LogIn : Form
    {
        public LogIn()
        {
            InitializeComponent();
        }
        public static string LoggedInEmployee { get; set; }

        SqlConnection Connection = new SqlConnection(@"Data Source = (LocalDB)\MSSQLLocalDB; AttachDbFilename=C:\Users\Palesa\Documents\Wags 'n Whiskers DB.mdf;Integrated Security = True; Connect Timeout = 30");
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string enteredUsername = txtUsername.Text;
            string enteredPassword = txtPassword.Text;

            try
            {
                Connection.Open();

                SqlCommand cmd = new SqlCommand("SELECT EmpPassword, EmpName, EmpSurname FROM EmployeeTable WHERE EmpUsername = @Username", Connection);
                cmd.Parameters.AddWithValue("@Username", enteredUsername);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    string storedPassword = reader["EmpPassword"].ToString();
                    string employeeName = reader["EmpName"].ToString();
                    string employeeSurname = reader["EmpSurname"].ToString();

                    // Verify the entered password against the stored password (plain text)
                    if (enteredPassword == storedPassword)
                    {
                        // Password is correct; allow login
                        MessageBox.Show("Login successful", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Assign the employee's name and surname to the LoggedInEmployee property
                        LoggedInEmployee = $"{employeeName} {employeeSurname}";

                        this.Close();
                        Products products = new Products();
                        products.Show();
                    }
                    else
                    {
                        MessageBox.Show("Invalid password", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Invalid username", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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
