using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp3
{
    public partial class Form1 : Form
    {
        private int _currentPageIndex = 1;
        private int _pageSize = 10;
        private string _sortColumn = "№ запроса";
        private string _sortDirection = "ASC";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            string searchText = toolStripTextBox1.Text.Trim();
            DataTable dataTable = GetFilteredData(searchText, _currentPageIndex, _pageSize, _sortColumn, _sortDirection);
            dgvData.DataSource = dataTable;
            dgvData.Columns["RowNumber"].Visible = false;
        }

        private DataTable GetFilteredData(string searchText, int pageIndex, int pageSize, string sortColumn, string sortDirection)
        {
            // Retrieve data from the database
            string connectionString = "server=37.77.105.162;user id=Tarabotti;password=gGJ130T90!;persistsecurityinfo=True;database=Diplom";
            string query = @"	SELECT `id_ticket` AS `№ запроса`, `name_type` AS `Тип`, `name_status` AS `Статус`, `name_priority` AS `Приоритет`, `name_staff` AS `Автор`, `date_ticket` AS `Дата создания`, `misc_ticket` AS `Детали` FROM `Diplom`.`tickets`
			JOIN `Diplom`.`ticket_type` ON `Diplom`.`tickets`.`type_ticket` = `Diplom`.`ticket_type`.`id_type`
			JOIN `Diplom`.`ticket_status` ON `Diplom`.`tickets`.`status_ticket` = `Diplom`.`ticket_status`.`id_status`
			JOIN `Diplom`.`ticket_priority` ON `Diplom`.`tickets`.`priority_ticket` = `Diplom`.`ticket_priority`.`id_priority`
			JOIN `Diplom`.`staff` ON `Diplom`.`tickets`.`author_ticket` = `Diplom`.`staff`.`id_staff`
		ORDER BY `id_ticket`;";
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);

                // Add a RowNumber column to the DataTable
                if (!dataTable.Columns.Contains("RowNumber"))
                {
                    dataTable.Columns.Add("RowNumber", typeof(int));
                }

                
                // Populate the RowNumber column with the row numbers for each row
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    dataTable.Rows[i]["RowNumber"] = i + 1;
                }

                // Filter the data based on the search text
                if (!string.IsNullOrEmpty(searchText))
                {
                    string filterExpression = GetFilteredExpression();
                    DataRow[] filteredRows = dataTable.Select(filterExpression);
                    dataTable = filteredRows.Any() ? filteredRows.CopyToDataTable() : dataTable.Clone();
                }

                // Sort the data based on the sort column and direction
                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDirection))
                {
                    string sortExpression = string.Format("{0} {1}", sortColumn, sortDirection);
                    dataTable.DefaultView.Sort = sortExpression;
                    dataTable = dataTable.DefaultView.ToTable();
                }

                // Calculate the start index and end index of the current page of data
                int startIndex = (pageIndex - 1) * pageSize;
                int endIndex = Math.Min(startIndex + pageSize, dataTable.Rows.Count) - 1;

                // Create a new DataTable to hold the current page of data
                DataTable pageDataTable = dataTable.Clone();

                // Populate the new DataTable with the current page of data
                for (int i = startIndex; i <= endIndex; i++)
                {
                    DataRow row = dataTable.Rows[i];
                    pageDataTable.ImportRow(row);
                }

                // Calculate the total number of pages
                int totalRows = dataTable.Rows.Count;
                int totalPages = (int)Math.Ceiling((double)totalRows / pageSize);

                // Update the pagination controls
                UpdatePaginationControls(totalPages);

                // Return the current page of data
                return pageDataTable;
            }
        }

        private string GetFilteredExpression()
        {
            string searchValue = toolStripTextBox1.Text.Trim();

            if (string.IsNullOrWhiteSpace(searchValue))
            {
                return null;
            }

            StringBuilder sb = new StringBuilder();
            foreach (DataGridViewColumn column in dgvData.Columns)
            {
                if (column.Visible && column.ValueType == typeof(string))
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(" OR ");
                    }
                    sb.AppendFormat("{0} LIKE '%{1}%'", column.DataPropertyName, searchValue);
                }
            }

            if (sb.Length > 0)
            {
                return sb.ToString();
            }
            else
            {
                return null;
            }
        }

        private void UpdatePaginationControls(int totalPages)
        {
            // Update the current page label
            toolStripLabel1.Text = $"Page {_currentPageIndex} of {totalPages}";

            // Enable/disable the previous button
            btnPrevious.Enabled = (_currentPageIndex > 1);

            // Enable/disable the next button
            btnNext.Enabled = (_currentPageIndex < totalPages);
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            _currentPageIndex = 1;
            LoadData();
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            _currentPageIndex--;
            LoadData();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            _currentPageIndex++;
            LoadData();
        }

        private void dgvData_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            // Determine the sort column and direction based on the column header clicked
            string sortColumn = dgvData.Columns[e.ColumnIndex].DataPropertyName;
            string sortDirection = (_sortColumn == sortColumn && _sortDirection == "ASC") ? "DESC" : "ASC";

            // Update the sort column and direction
            _sortColumn = sortColumn;
            _sortDirection = sortDirection;

            // Reload the data
            LoadData();
        }
    }
}
