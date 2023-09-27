using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace Wags__n_Whiskers_Management_System
{
    public partial class Billing : Form
    {
        private Dictionary<Button, PictureBox> buttonPictureBoxMapping = new Dictionary<Button, PictureBox>();
        private Dictionary<Button, Color> buttonOriginalColorMapping = new Dictionary<Button, Color>();

        SqlConnection Connection = new SqlConnection(@"Data Source = (LocalDB)\MSSQLLocalDB; AttachDbFilename=C:\Users\Palesa\Documents\Wags 'n Whiskers DB.mdf;Integrated Security = True; Connect Timeout = 30");
        public Billing()
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
            txtProdCategory.Text = "";
            txtProdPrice.Text = "";          
        }

        private void GetCustomers()
        {
            Connection.Open();
            SqlCommand cmd = new SqlCommand("Select CustID from CustomerTable", Connection);
            SqlDataReader reader = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Columns.Add("CustID", typeof(int));
            dt.Load(reader);
            cboCustID.ValueMember = "CustID";
            cboCustID.DataSource = dt;
            Connection.Close();
        }

        private void GetCustomerName()
        {
            try
            {
                if (cboCustID.SelectedValue != null)
                {
                    Connection.Open();
                    string Query = "Select * from CustomerTable where CustID = " + cboCustID.SelectedValue.ToString();
                    SqlCommand cmd = new SqlCommand(Query, Connection);
                    DataTable dataTable = new DataTable();
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dataTable);
                    foreach (DataRow row in dataTable.Rows)
                    {
                        txtCustName.Text = row["CustName"].ToString();
                        txtCustSurname.Text = row["CustSurname"].ToString();
                    }

                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("An error occurred while fetching customer data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Connection.Close();
            }
        }

        private void Reset()
        {             
                txtProdID.Text = "";
                txtProdName.Text = "";
                txtProdQuantity.Text = "";
                txtProdPrice.Text = "";
                stock = 0;
                key = 0;
        }
        int stock = 0;
        int key = 0;
        private void UpdateStock()
        {
            try
            {
                int newQty = stock - Convert.ToInt32(txtProdQuantity.Text);

                Connection.Open();
                SqlCommand cmd = new SqlCommand("Update ProductTable set ProdQuantity=@PQ where ProdID = @PKey", Connection);
                cmd.Parameters.AddWithValue("@PQ", newQty);
                cmd.Parameters.AddWithValue("@PKey", key);
                cmd.ExecuteNonQuery();
                MessageBox.Show("Product quantity updated", "Quantity Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Connection.Close();
                DisplayProducts();
                Clear();              
            }
            catch (Exception ex) 
            {
                MessageBox.Show(ex.Message, "Exception Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }   
        }

        private void FetchStockFromDatabase(int productId)
        {
            try
            {
                Connection.Open();
                SqlCommand cmd = new SqlCommand("SELECT ProdQuantity FROM ProductTable WHERE ProdID = @PID", Connection);
                cmd.Parameters.AddWithValue("@PID", productId);

                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    stock = Convert.ToInt32(result);
                }
                else
                {
                    stock = 0; // Set a default value if no stock value is found
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
      
        private void Billing_Load(object sender, EventArgs e)
        {
            DisplayProducts();
            GetCustomers();
            GetCustomerName();
            lblEmpName.Text = LogIn.LoggedInEmployee;
        }

        int n = 0;
        int grandTotal = 0;
        private void btnAddToBill_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtProdName.Text == "")
                {
                    MessageBox.Show("Please select a product from the list.", "Product Not Selected", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return; // Exit the method if no product is selected
                }

                else if (string.IsNullOrWhiteSpace(txtProdQuantity.Text))
                {
                    MessageBox.Show("Please input the quantity.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return; // Exit the method if quantity is missing
                }

                else if (!int.TryParse(txtProdQuantity.Text, out int quantity))
                {
                    MessageBox.Show("Invalid quantity. Please enter a valid number.", "Invalid Quantity", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return; // Exit the method if quantity is not a valid number
                }

                else if (quantity <= 0)
                {
                    MessageBox.Show("Quantity must be greater than zero.", "Invalid Quantity", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return; // Exit the method if quantity is not positive
                }
                else if (quantity > stock)
                {
                    MessageBox.Show("Quantity selected exceeds stock in house.", "Invalid Quantity", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else
                {
                    int total = Convert.ToInt32(txtProdQuantity.Text) * Convert.ToInt32(txtProdPrice.Text);

                    // Create a new row and add it to the DataGridView
                    DataGridViewRow newRow = new DataGridViewRow();
                    newRow.CreateCells(dgvInvoice);
                    newRow.Cells[0].Value = txtProdID.Text;
                    newRow.Cells[1].Value = txtProdName.Text;
                    newRow.Cells[2].Value = txtProdPrice.Text;
                    newRow.Cells[3].Value = txtProdQuantity.Text;
                    newRow.Cells[4].Value = total;
                    dgvInvoice.Rows.Add(newRow);

                    // Update the grand total
                    grandTotal += total;
                    lblGrandTotal.Text = grandTotal.ToString(); // Update the label with the new grand total

                    // Update the stock and clear input fields
                    UpdateStock();
                    Reset();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }                     
        
        private void btnReset_Click(object sender, EventArgs e)
        {
            Reset();
        }
        
        private void dgvProducts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow selectedRow = dgvProducts.Rows[e.RowIndex];

                // Access cell values here.
                txtProdName.Text = selectedRow.Cells["ProdName"].Value?.ToString();
                txtProdCategory.Text = selectedRow.Cells["ProdCategory"].Value?.ToString();
                txtProdPrice.Text = selectedRow.Cells["ProdPrice"].Value?.ToString();
                txtProdID.Text = selectedRow.Cells["ProdID"].Value?.ToString();

                stock = Convert.ToInt32(selectedRow.Cells["ProdQuantity"].Value?.ToString());

                // Optionally, set the 'key' variable based on your logic.
                if (string.IsNullOrEmpty(txtProdName.Text))
                {
                    key = 0;
                }
                else
                {
                    key = Convert.ToInt32(selectedRow.Cells[0].Value);
                }

                // Fetch the 'stock' value from the database based on the 'key' or product ID
                FetchStockFromDatabase(key);
            }
        }

        private void InsertBill()
        {
            try
            {
                Connection.Open();
                SqlCommand cmd = new SqlCommand("Insert into BillingTable(BillDate, CustID, CustName, CustSurname, EmpName, EmpSurname, BillAmnt) values (@BD, @CID, @CN, @CS, @EN, @ES,@BA)", Connection);
                cmd.Parameters.AddWithValue("@BD", DateTime.Today.Date);
                cmd.Parameters.AddWithValue("@CID", cboCustID.SelectedValue.ToString());
                cmd.Parameters.AddWithValue("@CS", txtCustSurname.Text);
                cmd.Parameters.AddWithValue("@CN", txtCustName.Text);
                cmd.Parameters.AddWithValue("@EN", lblEmpName.Text);
                cmd.Parameters.AddWithValue("@BA", Convert.ToInt32(lblGrandTotal.Text));
                cmd.ExecuteNonQuery();
                MessageBox.Show("Bill saved to records", "Bill Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Connection.Close();
                DisplayTransactions();
                //Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayTransactions()
        {
            Connection.Open();
            string Query = "Select * from BillingTable";
            SqlDataAdapter adapter = new SqlDataAdapter(Query, Connection);
            SqlCommandBuilder builder = new SqlCommandBuilder(adapter);
            var dataset = new DataSet();
            adapter.Fill(dataset);
            dgvTransactions.DataSource = dataset.Tables[0];
            Connection.Close();
        }
       
        private void btnPrint_Click(object sender, EventArgs e)
        {
            try
            {
                InsertBill();
                printDocument1.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("pprm", 285, 600);
                printPreviewDialog1.Document = printDocument1;
                if (printPreviewDialog1.ShowDialog() == DialogResult.OK)
                {
                    printDocument1.Print();
                }
            } catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while printing: {ex.Message}", "Printing Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            // Define fonts and spacing
            Font headerFont = new Font("Century Gothic", 12, FontStyle.Bold);
            Font bodyFont = new Font("Century Gothic", 10, FontStyle.Regular);
            int lineHeight = bodyFont.Height + 2;

            // Define the starting position for printing
            int startX = 10;
            int startY = 10;

            // Print the header
            e.Graphics.DrawString("Wags 'n Whiskers", headerFont, Brushes.Black, startX, startY);
            startY += 2 * lineHeight; // Move down for the next line

            // Print the column headers
            e.Graphics.DrawString("ID PRODUCT PRICE QUANTITY TOTAL", bodyFont, Brushes.Black, startX, startY);
            startY += lineHeight; // Move down for the next line

            // Loop through the DataGridView rows and print the content
            foreach (DataGridViewRow row in dgvInvoice.Rows)
            {
                // Check if the row is not null and is not a new row (assuming dgvInvoice is a DataGridView)
                if (row != null && !row.IsNewRow)
                {
                    // Attempt to retrieve values from cells and handle null or empty values
                    string id = row.Cells[0].Value?.ToString() ?? "";
                    string product = row.Cells[1].Value?.ToString() ?? "";
                    string price = row.Cells[2].Value?.ToString() ?? "";
                    string quantity = row.Cells[3].Value?.ToString() ?? "";
                    string total = row.Cells[4].Value?.ToString() ?? "";

                    // Format the row content
                    string rowContent = $"{id,-3} {product,-40} {price,-10} {quantity,-10} {total,-10}";

                    // Print the formatted row content
                    e.Graphics.DrawString(rowContent, bodyFont, Brushes.Black, startX, startY);

                    startY += lineHeight; // Move down for the next line
                }

            }
            // Calculate and print the grand total
            int grandTotalY = startY + 2 * lineHeight; // Adjust the position for the grand total
            string grandTotalText = $"Grand Total: R{grandTotal}"; // Convert grandTotal to a string with currency symbol
            e.Graphics.DrawString(grandTotalText, bodyFont, Brushes.Black, startX, grandTotalY);

            dgvInvoice.Rows.Clear();
            Reset();
            grandTotal = 0;
        }

        private void cboCustID_SelectionChanged(object sender, EventArgs e)
        {
            GetCustomerName();
        }

        private void btnProduct_Click(object sender, EventArgs e)
        {
            Products products = new Products();
            products.Show();
            this.Hide();    
        }

        private void btnCustomers_Click(object sender, EventArgs e)
        {
            Customers customers = new Customers();
            customers.Show();
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

        private void dgvProducts_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
             
        }
    }
}
