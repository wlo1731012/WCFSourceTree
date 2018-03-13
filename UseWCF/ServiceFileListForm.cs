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

        public string selectedFile = "";

        public ServiceFileListForm()
        {
            InitializeComponent();

            _dataSet = ClientForm.serviceFileList;
            DataTable dataTable = _dataSet.Tables[0].Copy();
            for (var j = 0; j < dataTable.Rows.Count; j++)
            {
                lsbFileList.Items.Add(dataTable.Rows[j]["FileName"]);
            }
                
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (lsbFileList.SelectedItem != null)
            {
                selectedFile = lsbFileList.SelectedItem.ToString();
                this.Close();
            }
            else
                this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
