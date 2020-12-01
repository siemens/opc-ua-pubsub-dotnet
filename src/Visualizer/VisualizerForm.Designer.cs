namespace opc.ua.pubsub.dotnet.visualizer
{
    partial class VisualizerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VisualizerForm));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.MainTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.MainTabControl = new System.Windows.Forms.TabControl();
            this.OverviewPage = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.OverviewDataGridView = new System.Windows.Forms.DataGridView();
            this.DetailsPage = new System.Windows.Forms.TabPage();
            this.DetailsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.DevicesTreeView = new System.Windows.Forms.TreeView();
            this.DetailsDataGridView = new System.Windows.Forms.DataGridView();
            this.MessageTabPage = new System.Windows.Forms.TabPage();
            this.MessagePanel = new System.Windows.Forms.Panel();
            this.MessagePropertyGrid = new System.Windows.Forms.PropertyGrid();
            this.MainTableLayoutPanel.SuspendLayout();
            this.MainTabControl.SuspendLayout();
            this.OverviewPage.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.OverviewDataGridView)).BeginInit();
            this.DetailsPage.SuspendLayout();
            this.DetailsTableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DetailsDataGridView)).BeginInit();
            this.MessageTabPage.SuspendLayout();
            this.MessagePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Location = new System.Drawing.Point(0, 590);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 19, 0);
            this.statusStrip1.Size = new System.Drawing.Size(1145, 22);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1145, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // MainTableLayoutPanel
            // 
            this.MainTableLayoutPanel.ColumnCount = 2;
            this.MainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.MainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.MainTableLayoutPanel.Controls.Add(this.MainTabControl, 0, 0);
            this.MainTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainTableLayoutPanel.Location = new System.Drawing.Point(0, 25);
            this.MainTableLayoutPanel.Margin = new System.Windows.Forms.Padding(4);
            this.MainTableLayoutPanel.Name = "MainTableLayoutPanel";
            this.MainTableLayoutPanel.RowCount = 1;
            this.MainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.MainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.MainTableLayoutPanel.Size = new System.Drawing.Size(1145, 565);
            this.MainTableLayoutPanel.TabIndex = 2;
            // 
            // MainTabControl
            // 
            this.MainTableLayoutPanel.SetColumnSpan(this.MainTabControl, 2);
            this.MainTabControl.Controls.Add(this.OverviewPage);
            this.MainTabControl.Controls.Add(this.DetailsPage);
            this.MainTabControl.Controls.Add(this.MessageTabPage);
            this.MainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainTabControl.Location = new System.Drawing.Point(4, 4);
            this.MainTabControl.Margin = new System.Windows.Forms.Padding(4);
            this.MainTabControl.Name = "MainTabControl";
            this.MainTabControl.SelectedIndex = 0;
            this.MainTabControl.Size = new System.Drawing.Size(1137, 557);
            this.MainTabControl.TabIndex = 2;
            // 
            // OverviewPage
            // 
            this.OverviewPage.AutoScroll = true;
            this.OverviewPage.Controls.Add(this.panel1);
            this.OverviewPage.Location = new System.Drawing.Point(4, 25);
            this.OverviewPage.Margin = new System.Windows.Forms.Padding(4);
            this.OverviewPage.Name = "OverviewPage";
            this.OverviewPage.Padding = new System.Windows.Forms.Padding(4);
            this.OverviewPage.Size = new System.Drawing.Size(1129, 528);
            this.OverviewPage.TabIndex = 0;
            this.OverviewPage.Text = "Overview";
            this.OverviewPage.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.OverviewDataGridView);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(4, 4);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1121, 520);
            this.panel1.TabIndex = 1;
            // 
            // OverviewDataGridView
            // 
            this.OverviewDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.OverviewDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.OverviewDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OverviewDataGridView.Location = new System.Drawing.Point(0, 0);
            this.OverviewDataGridView.Margin = new System.Windows.Forms.Padding(4);
            this.OverviewDataGridView.Name = "OverviewDataGridView";
            this.OverviewDataGridView.ReadOnly = true;
            this.OverviewDataGridView.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.OverviewDataGridView.Size = new System.Drawing.Size(1121, 520);
            this.OverviewDataGridView.TabIndex = 0;
            
            // 
            // DetailsPage
            // 
            this.DetailsPage.Controls.Add(this.DetailsTableLayoutPanel);
            this.DetailsPage.Location = new System.Drawing.Point(4, 25);
            this.DetailsPage.Margin = new System.Windows.Forms.Padding(4);
            this.DetailsPage.Name = "DetailsPage";
            this.DetailsPage.Padding = new System.Windows.Forms.Padding(4);
            this.DetailsPage.Size = new System.Drawing.Size(1129, 528);
            this.DetailsPage.TabIndex = 1;
            this.DetailsPage.Text = "Details";
            this.DetailsPage.UseVisualStyleBackColor = true;
            // 
            // DetailsTableLayoutPanel
            // 
            this.DetailsTableLayoutPanel.ColumnCount = 2;
            this.DetailsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 400F));
            this.DetailsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.DetailsTableLayoutPanel.Controls.Add(this.DevicesTreeView, 0, 0);
            this.DetailsTableLayoutPanel.Controls.Add(this.DetailsDataGridView, 1, 0);
            this.DetailsTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DetailsTableLayoutPanel.Location = new System.Drawing.Point(4, 4);
            this.DetailsTableLayoutPanel.Margin = new System.Windows.Forms.Padding(4);
            this.DetailsTableLayoutPanel.Name = "DetailsTableLayoutPanel";
            this.DetailsTableLayoutPanel.RowCount = 1;
            this.DetailsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.DetailsTableLayoutPanel.Size = new System.Drawing.Size(1121, 520);
            this.DetailsTableLayoutPanel.TabIndex = 0;
            // 
            // DevicesTreeView
            // 
            this.DevicesTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DevicesTreeView.Location = new System.Drawing.Point(4, 4);
            this.DevicesTreeView.Margin = new System.Windows.Forms.Padding(4);
            this.DevicesTreeView.Name = "DevicesTreeView";
            this.DevicesTreeView.Size = new System.Drawing.Size(392, 512);
            this.DevicesTreeView.TabIndex = 1;
            this.DevicesTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.DevicesTreeView_AfterSelect);
            // 
            // DetailsDataGridView
            // 
            this.DetailsDataGridView.AllowUserToAddRows = false;
            this.DetailsDataGridView.AllowUserToDeleteRows = false;
            this.DetailsDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DetailsDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.DetailsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DetailsDataGridView.Location = new System.Drawing.Point(404, 4);
            this.DetailsDataGridView.Margin = new System.Windows.Forms.Padding(4);
            this.DetailsDataGridView.Name = "DetailsDataGridView";
            this.DetailsDataGridView.ReadOnly = true;
            this.DetailsDataGridView.Size = new System.Drawing.Size(713, 512);
            this.DetailsDataGridView.TabIndex = 2;
            this.DetailsDataGridView.RowHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.DetailsDataGridView_RowHeaderMouseClick_1);
            // 
            // MessageTabPage
            // 
            this.MessageTabPage.Controls.Add(this.MessagePanel);
            this.MessageTabPage.Location = new System.Drawing.Point(4, 25);
            this.MessageTabPage.Margin = new System.Windows.Forms.Padding(4);
            this.MessageTabPage.Name = "MessageTabPage";
            this.MessageTabPage.Padding = new System.Windows.Forms.Padding(4);
            this.MessageTabPage.Size = new System.Drawing.Size(1129, 528);
            this.MessageTabPage.TabIndex = 2;
            this.MessageTabPage.Text = "Last Message";
            this.MessageTabPage.UseVisualStyleBackColor = true;
            // 
            // MessagePanel
            // 
            this.MessagePanel.Controls.Add(this.MessagePropertyGrid);
            this.MessagePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MessagePanel.Location = new System.Drawing.Point(4, 4);
            this.MessagePanel.Margin = new System.Windows.Forms.Padding(4);
            this.MessagePanel.Name = "MessagePanel";
            this.MessagePanel.Size = new System.Drawing.Size(1121, 520);
            this.MessagePanel.TabIndex = 0;
            // 
            // MessagePropertyGrid
            // 
            this.MessagePropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MessagePropertyGrid.Location = new System.Drawing.Point(0, 0);
            this.MessagePropertyGrid.Margin = new System.Windows.Forms.Padding(4);
            this.MessagePropertyGrid.Name = "MessagePropertyGrid";
            this.MessagePropertyGrid.Size = new System.Drawing.Size(1121, 520);
            this.MessagePropertyGrid.TabIndex = 0;
            // 
            // VisualizerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1145, 612);
            this.Controls.Add(this.MainTableLayoutPanel);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "VisualizerForm";
            this.Text = "Siemens Energy - OPC UA PubSub MQTT Subscriber";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.VisualizerForm_FormClosing);
            this.MainTableLayoutPanel.ResumeLayout(false);
            this.MainTabControl.ResumeLayout(false);
            this.OverviewPage.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.OverviewDataGridView)).EndInit();
            this.DetailsPage.ResumeLayout(false);
            this.DetailsTableLayoutPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.DetailsDataGridView)).EndInit();
            this.MessageTabPage.ResumeLayout(false);
            this.MessagePanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.TableLayoutPanel MainTableLayoutPanel;
        private System.Windows.Forms.DataGridView OverviewDataGridView;
        private System.Windows.Forms.TreeView DevicesTreeView;
        private System.Windows.Forms.TabControl MainTabControl;
        private System.Windows.Forms.TabPage OverviewPage;
        private System.Windows.Forms.TabPage DetailsPage;
        private System.Windows.Forms.TableLayoutPanel DetailsTableLayoutPanel;
        public System.Windows.Forms.DataGridView DetailsDataGridView;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TabPage MessageTabPage;
        private System.Windows.Forms.Panel MessagePanel;
        private System.Windows.Forms.PropertyGrid MessagePropertyGrid;
    }
}

