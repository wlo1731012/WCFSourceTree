using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WCFClient;

namespace WCFClient
{
    public partial class ServiceFileListForm : Form
    {
        private DataSet _dataSet = new DataSet();
        private string _chooseData = "";
        public string selectedFile = "";

        public ServiceFileListForm()
        {
            InitializeComponent();

            _dataSet = ClientForm.serviceFileList;
            DataTable dataTable = _dataSet.Tables[0].Copy();

            dgvFileList.Columns[0].Name = "Name";
            dgvFileList.Columns[1].Name = "Size";
            dgvFileList.AutoResizeColumnHeadersHeight();
            dgvFileList.AutoResizeRows(DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders);
            dgvFileList.ReadOnly = true;
            int fileSize = 0;
            for (var i = 0; i < dataTable.Rows.Count; i++)
            {
                fileSize = int.Parse(dataTable.Rows[i]["FileSize"].ToString());
                dgvFileList.Rows.Add(dataTable.Rows[i]["FileName"], dataTable.Rows[i]["FileSize"]);
            }
            dgvFileList.AllowUserToAddRows = false;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (_chooseData != null)
            {
                selectedFile = _chooseData;
                this.Close();
            }
            else
                this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dgvFileList_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            //_chooseData = dgvFileList.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
        }

        private void dgvFileList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            _chooseData = dgvFileList.Rows[e.RowIndex].Cells[0].Value.ToString();
        }

        private void dgvFileList_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            //_chooseData = dgvFileList.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
        }
    }
}
