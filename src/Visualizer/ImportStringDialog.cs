// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Windows.Forms;

namespace opc.ua.pubsub.dotnet.visualizer
{
    public partial class ImportStringDialog : Form
    {
        public ImportStringDialog()
        {
            InitializeComponent();
        }

        public string UserString
        {
            get
            {
                return hexStringTextBox.Text;
            }
        }

        private void ImportStringDialog_Load( object sender, EventArgs e )
        {
            hexStringTextBox.Select();
        }
    }
}