namespace opc.ua.pubsub.dotnet.visualizer
{
    partial class ImportStringDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ImportStringDialogTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.hexStringTextBox = new System.Windows.Forms.TextBox();
            this.ImportStringDialogTableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // ImportStringDialogTableLayoutPanel
            // 
            this.ImportStringDialogTableLayoutPanel.ColumnCount = 2;
            this.ImportStringDialogTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.ImportStringDialogTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.ImportStringDialogTableLayoutPanel.Controls.Add(this.okButton, 0, 1);
            this.ImportStringDialogTableLayoutPanel.Controls.Add(this.cancelButton, 1, 1);
            this.ImportStringDialogTableLayoutPanel.Controls.Add(this.hexStringTextBox, 0, 0);
            this.ImportStringDialogTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ImportStringDialogTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.ImportStringDialogTableLayoutPanel.Name = "ImportStringDialogTableLayoutPanel";
            this.ImportStringDialogTableLayoutPanel.RowCount = 2;
            this.ImportStringDialogTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ImportStringDialogTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.ImportStringDialogTableLayoutPanel.Size = new System.Drawing.Size(800, 450);
            this.ImportStringDialogTableLayoutPanel.TabIndex = 0;
            // 
            // okButton
            // 
            this.okButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(162, 418);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 0;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(562, 418);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // hexStringTextBox
            // 
            this.ImportStringDialogTableLayoutPanel.SetColumnSpan(this.hexStringTextBox, 2);
            this.hexStringTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hexStringTextBox.Location = new System.Drawing.Point(3, 3);
            this.hexStringTextBox.Multiline = true;
            this.hexStringTextBox.Name = "hexStringTextBox";
            this.hexStringTextBox.Size = new System.Drawing.Size(794, 404);
            this.hexStringTextBox.TabIndex = 2;
            // 
            // ImportStringDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.ControlBox = false;
            this.Controls.Add(this.ImportStringDialogTableLayoutPanel);
            this.Name = "ImportStringDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Import OPC UA PubSub message from Hex-String";
            this.Load += new System.EventHandler(this.ImportStringDialog_Load);
            this.ImportStringDialogTableLayoutPanel.ResumeLayout(false);
            this.ImportStringDialogTableLayoutPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel ImportStringDialogTableLayoutPanel;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.TextBox hexStringTextBox;
    }
}