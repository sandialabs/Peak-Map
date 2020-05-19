/********************************************************************************************************************************************************************************************************************************************************                          
NOTICE:

For five (5) years from 1/21/2020 the United States Government is granted for itself and others acting on its behalf a paid-up, nonexclusive, irrevocable worldwide license in this data to reproduce, prepare derivative works, and perform 
publicly and display publicly, by or on behalf of the Government. There is provision for the possible extension of the term of this license. Subsequent to that period or any extension granted, the United States Government is granted for itself
and others acting on its behalf a paid-up, nonexclusive, irrevocable worldwide license in this data to reproduce, prepare derivative works, distribute copies to the public, perform publicly and display publicly, and to permit others to do so. The
specific term of the license can be identified by inquiry made to National Technology and Engineering Solutions of Sandia, LLC or DOE.
 
NEITHER THE UNITED STATES GOVERNMENT, NOR THE UNITED STATES DEPARTMENT OF ENERGY, NOR NATIONAL TECHNOLOGY AND ENGINEERING SOLUTIONS OF SANDIA, LLC, NOR ANY OF THEIR EMPLOYEES, MAKES ANY WARRANTY, 
EXPRESS OR IMPLIED, OR ASSUMES ANY LEGAL RESPONSIBILITY FOR THE ACCURACY, COMPLETENESS, OR USEFULNESS OF ANY INFORMATION, APPARATUS, PRODUCT, OR PROCESS DISCLOSED, OR REPRESENTS THAT ITS USE WOULD
NOT INFRINGE PRIVATELY OWNED RIGHTS.
 
Any licensee of this software has the obligation and responsibility to abide by the applicable export control laws, regulations, and general prohibitions relating to the export of technical data. Failure to obtain an export control license or other 
authority from the Government may result in criminal liability under U.S. laws.
 
                                             (End of Notice)
*********************************************************************************************************************************************************************************************************************************************************/
namespace PeakMap
{
    partial class Main
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.peaksGrid = new System.Windows.Forms.DataGridView();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.libLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.CntTimeLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.AqTimeLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.matchedNuclidesGrid = new System.Windows.Forms.DataGridView();
            this.matchedLinesGrid = new System.Windows.Forms.DataGridView();
            this.label2 = new System.Windows.Forms.Label();
            this.lineMatchScore = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearInputToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem8 = new System.Windows.Forms.ToolStripSeparator();
            this.openToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SaveLibraryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.linesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showDaughtersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripSeparator();
            this.writeSelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.writeMatchedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.writeYieldToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.writeAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.writeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lineMatchSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.spectralToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.librarySettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.modeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.matchingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.librayOnlyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.generateDataLibraryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.modeStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.rightClickMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.showDaughtersRightClickMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripSeparator();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
            this.clearMatchesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearSelectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearCurrentRowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.rightClickWriteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.peaksGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.matchedNuclidesGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.matchedLinesGrid)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.rightClickMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // peaksGrid
            // 
            this.peaksGrid.AllowUserToDeleteRows = false;
            this.peaksGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.peaksGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.peaksGrid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.142858F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.peaksGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.peaksGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.Format = "F3";
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.peaksGrid.DefaultCellStyle = dataGridViewCellStyle2;
            this.peaksGrid.Location = new System.Drawing.Point(12, 43);
            this.peaksGrid.Name = "peaksGrid";
            this.peaksGrid.RowHeadersVisible = false;
            this.peaksGrid.RowHeadersWidth = 72;
            this.peaksGrid.RowTemplate.Height = 28;
            this.peaksGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.peaksGrid.Size = new System.Drawing.Size(657, 916);
            this.peaksGrid.TabIndex = 1;
            this.peaksGrid.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.PeaksGrid_CellEndEdit);
            this.peaksGrid.CellEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.PeaksGrid_CellEnter);
            this.peaksGrid.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.GridView_DataError);
            this.peaksGrid.SelectionChanged += new System.EventHandler(this.PeaksGrid_SelectionChanged);
            this.peaksGrid.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Main_MouseClick);
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
            this.statusLabel.Size = new System.Drawing.Size(41, 25);
            this.statusLabel.Text = "Idle";
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // libLabel
            // 
            this.libLabel.Name = "libLabel";
            this.libLabel.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
            this.libLabel.Size = new System.Drawing.Size(69, 25);
            this.libLabel.Text = "Library:";
            this.libLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // CntTimeLabel
            // 
            this.CntTimeLabel.LinkVisited = true;
            this.CntTimeLabel.Name = "CntTimeLabel";
            this.CntTimeLabel.Size = new System.Drawing.Size(95, 25);
            this.CntTimeLabel.Text = "Start Time:";
            this.CntTimeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // AqTimeLabel
            // 
            this.AqTimeLabel.Name = "AqTimeLabel";
            this.AqTimeLabel.Size = new System.Drawing.Size(140, 25);
            this.AqTimeLabel.Text = "Aqusition Time: ";
            this.AqTimeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // matchedNuclidesGrid
            // 
            this.matchedNuclidesGrid.AllowUserToAddRows = false;
            this.matchedNuclidesGrid.AllowUserToDeleteRows = false;
            this.matchedNuclidesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.matchedNuclidesGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.matchedNuclidesGrid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.142858F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.matchedNuclidesGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.matchedNuclidesGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle4.Format = "F3";
            dataGridViewCellStyle4.NullValue = null;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.matchedNuclidesGrid.DefaultCellStyle = dataGridViewCellStyle4;
            this.matchedNuclidesGrid.Location = new System.Drawing.Point(695, 43);
            this.matchedNuclidesGrid.Name = "matchedNuclidesGrid";
            this.matchedNuclidesGrid.ReadOnly = true;
            this.matchedNuclidesGrid.RowHeadersVisible = false;
            this.matchedNuclidesGrid.RowHeadersWidth = 72;
            this.matchedNuclidesGrid.RowTemplate.Height = 28;
            this.matchedNuclidesGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.matchedNuclidesGrid.Size = new System.Drawing.Size(1071, 463);
            this.matchedNuclidesGrid.TabIndex = 5;
            this.matchedNuclidesGrid.VirtualMode = true;
            this.matchedNuclidesGrid.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.MatchedNuclidesGrid_CellValidating);
            this.matchedNuclidesGrid.SelectionChanged += new System.EventHandler(this.MatchedNuclides_SelectionChanged);
            this.matchedNuclidesGrid.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Main_MouseClick);
            // 
            // matchedLinesGrid
            // 
            this.matchedLinesGrid.AllowUserToAddRows = false;
            this.matchedLinesGrid.AllowUserToDeleteRows = false;
            this.matchedLinesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.matchedLinesGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.matchedLinesGrid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.142858F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.matchedLinesGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle5;
            this.matchedLinesGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle6.Format = "F3";
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.matchedLinesGrid.DefaultCellStyle = dataGridViewCellStyle6;
            this.matchedLinesGrid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.matchedLinesGrid.Location = new System.Drawing.Point(695, 536);
            this.matchedLinesGrid.Name = "matchedLinesGrid";
            this.matchedLinesGrid.ReadOnly = true;
            this.matchedLinesGrid.RowHeadersVisible = false;
            this.matchedLinesGrid.RowHeadersWidth = 72;
            this.matchedLinesGrid.RowTemplate.Height = 28;
            this.matchedLinesGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.matchedLinesGrid.Size = new System.Drawing.Size(1071, 423);
            this.matchedLinesGrid.TabIndex = 6;
            this.matchedLinesGrid.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Main_MouseClick);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(691, 513);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(137, 20);
            this.label2.TabIndex = 7;
            this.label2.Text = "Line Match Score:";
            this.label2.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Main_MouseClick);
            // 
            // lineMatchScore
            // 
            this.lineMatchScore.AutoSize = true;
            this.lineMatchScore.Location = new System.Drawing.Point(834, 513);
            this.lineMatchScore.Name = "lineMatchScore";
            this.lineMatchScore.Size = new System.Drawing.Size(0, 20);
            this.lineMatchScore.TabIndex = 8;
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.linesToolStripMenuItem,
            this.settingsToolStripMenuItem,
            this.dataToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1778, 33);
            this.menuStrip1.TabIndex = 9;
            this.menuStrip1.Text = "menuStrip1";
            this.menuStrip1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Main_MouseClick);
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.clearInputToolStripMenuItem,
            this.toolStripMenuItem8,
            this.openToolStripMenuItem1,
            this.newToolStripMenuItem,
            this.SaveLibraryToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.toolStripMenuItem3,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(54, 29);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.I)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(281, 34);
            this.openToolStripMenuItem.Text = "Open &Input...";
            this.openToolStripMenuItem.ToolTipText = "Open input file";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.OpenToolStripMenuItem_Click);
            // 
            // clearInputToolStripMenuItem
            // 
            this.clearInputToolStripMenuItem.Name = "clearInputToolStripMenuItem";
            this.clearInputToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.clearInputToolStripMenuItem.Size = new System.Drawing.Size(281, 34);
            this.clearInputToolStripMenuItem.Text = "&Close Input";
            this.clearInputToolStripMenuItem.Click += new System.EventHandler(this.ClearInputToolStripMenuItem_Click);
            // 
            // toolStripMenuItem8
            // 
            this.toolStripMenuItem8.Name = "toolStripMenuItem8";
            this.toolStripMenuItem8.Size = new System.Drawing.Size(278, 6);
            // 
            // openToolStripMenuItem1
            // 
            this.openToolStripMenuItem1.Name = "openToolStripMenuItem1";
            this.openToolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem1.Size = new System.Drawing.Size(281, 34);
            this.openToolStripMenuItem1.Text = "&Open Library";
            this.openToolStripMenuItem1.Click += new System.EventHandler(this.OpenToolStripMenuItem1_Click);
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newToolStripMenuItem.Size = new System.Drawing.Size(281, 34);
            this.newToolStripMenuItem.Text = "&New Library";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.NewToolStripMenuItem_Click);
            // 
            // SaveLibraryToolStripMenuItem
            // 
            this.SaveLibraryToolStripMenuItem.Enabled = false;
            this.SaveLibraryToolStripMenuItem.Name = "SaveLibraryToolStripMenuItem";
            this.SaveLibraryToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.SaveLibraryToolStripMenuItem.Size = new System.Drawing.Size(281, 34);
            this.SaveLibraryToolStripMenuItem.Text = "&Save Library";
            this.SaveLibraryToolStripMenuItem.Click += new System.EventHandler(this.SaveLibraryToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Enabled = false;
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(281, 34);
            this.saveAsToolStripMenuItem.Text = "Save As...";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.SaveAsToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(278, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(281, 34);
            this.exitToolStripMenuItem.Text = "&Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItem_Click);
            // 
            // linesToolStripMenuItem
            // 
            this.linesToolStripMenuItem.Checked = true;
            this.linesToolStripMenuItem.CheckOnClick = true;
            this.linesToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.linesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showDaughtersToolStripMenuItem,
            this.toolStripMenuItem6,
            this.writeSelectedToolStripMenuItem,
            this.writeMatchedToolStripMenuItem,
            this.writeYieldToolStripMenuItem,
            this.writeAllToolStripMenuItem,
            this.toolStripMenuItem1,
            this.writeToolStripMenuItem});
            this.linesToolStripMenuItem.Name = "linesToolStripMenuItem";
            this.linesToolStripMenuItem.Size = new System.Drawing.Size(67, 29);
            this.linesToolStripMenuItem.Text = "&Lines";
            // 
            // showDaughtersToolStripMenuItem
            // 
            this.showDaughtersToolStripMenuItem.Checked = true;
            this.showDaughtersToolStripMenuItem.CheckOnClick = true;
            this.showDaughtersToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showDaughtersToolStripMenuItem.Name = "showDaughtersToolStripMenuItem";
            this.showDaughtersToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F8;
            this.showDaughtersToolStripMenuItem.Size = new System.Drawing.Size(312, 34);
            this.showDaughtersToolStripMenuItem.Tag = "daughters";
            this.showDaughtersToolStripMenuItem.Text = "Show &Daughter Lines";
            this.showDaughtersToolStripMenuItem.Click += new System.EventHandler(this.ShowDaughtersToolStripMenuItem_Click);
            // 
            // toolStripMenuItem6
            // 
            this.toolStripMenuItem6.Name = "toolStripMenuItem6";
            this.toolStripMenuItem6.Size = new System.Drawing.Size(309, 6);
            // 
            // writeSelectedToolStripMenuItem
            // 
            this.writeSelectedToolStripMenuItem.Checked = true;
            this.writeSelectedToolStripMenuItem.CheckOnClick = true;
            this.writeSelectedToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.writeSelectedToolStripMenuItem.Enabled = false;
            this.writeSelectedToolStripMenuItem.Name = "writeSelectedToolStripMenuItem";
            this.writeSelectedToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H)));
            this.writeSelectedToolStripMenuItem.Size = new System.Drawing.Size(312, 34);
            this.writeSelectedToolStripMenuItem.Tag = "selected";
            this.writeSelectedToolStripMenuItem.Text = "Write &Selected";
            this.writeSelectedToolStripMenuItem.Click += new System.EventHandler(this.WriteSelectedToolStripMenuItem_Click);
            // 
            // writeMatchedToolStripMenuItem
            // 
            this.writeMatchedToolStripMenuItem.CheckOnClick = true;
            this.writeMatchedToolStripMenuItem.Enabled = false;
            this.writeMatchedToolStripMenuItem.Name = "writeMatchedToolStripMenuItem";
            this.writeMatchedToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.M)));
            this.writeMatchedToolStripMenuItem.Size = new System.Drawing.Size(312, 34);
            this.writeMatchedToolStripMenuItem.Tag = "matched";
            this.writeMatchedToolStripMenuItem.Text = "Write &Matched";
            this.writeMatchedToolStripMenuItem.Click += new System.EventHandler(this.WriteMatchedToolStripMenuItem_Click);
            // 
            // writeYieldToolStripMenuItem
            // 
            this.writeYieldToolStripMenuItem.CheckOnClick = true;
            this.writeYieldToolStripMenuItem.Enabled = false;
            this.writeYieldToolStripMenuItem.Name = "writeYieldToolStripMenuItem";
            this.writeYieldToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
            this.writeYieldToolStripMenuItem.Size = new System.Drawing.Size(312, 34);
            this.writeYieldToolStripMenuItem.Tag = "yield";
            this.writeYieldToolStripMenuItem.Text = "Write &Yield...";
            this.writeYieldToolStripMenuItem.Click += new System.EventHandler(this.WriteYieldToolStripMenuItem_Click);
            // 
            // writeAllToolStripMenuItem
            // 
            this.writeAllToolStripMenuItem.CheckOnClick = true;
            this.writeAllToolStripMenuItem.Enabled = false;
            this.writeAllToolStripMenuItem.Name = "writeAllToolStripMenuItem";
            this.writeAllToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.writeAllToolStripMenuItem.Size = new System.Drawing.Size(312, 34);
            this.writeAllToolStripMenuItem.Tag = "all";
            this.writeAllToolStripMenuItem.Text = "Write &All";
            this.writeAllToolStripMenuItem.Click += new System.EventHandler(this.WriteAllToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(309, 6);
            // 
            // writeToolStripMenuItem
            // 
            this.writeToolStripMenuItem.Enabled = false;
            this.writeToolStripMenuItem.Name = "writeToolStripMenuItem";
            this.writeToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Insert)));
            this.writeToolStripMenuItem.Size = new System.Drawing.Size(312, 34);
            this.writeToolStripMenuItem.Tag = "write";
            this.writeToolStripMenuItem.Text = "&Write";
            this.writeToolStripMenuItem.Click += new System.EventHandler(this.WriteToolStripMenuItem_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lineMatchSettingsToolStripMenuItem,
            this.spectralToolStripMenuItem,
            this.librarySettingsToolStripMenuItem,
            this.modeToolStripMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(92, 29);
            this.settingsToolStripMenuItem.Text = "&Settings";
            // 
            // lineMatchSettingsToolStripMenuItem
            // 
            this.lineMatchSettingsToolStripMenuItem.Name = "lineMatchSettingsToolStripMenuItem";
            this.lineMatchSettingsToolStripMenuItem.Size = new System.Drawing.Size(200, 34);
            this.lineMatchSettingsToolStripMenuItem.Text = "&Matching...";
            this.lineMatchSettingsToolStripMenuItem.Click += new System.EventHandler(this.LineMatchSettingsToolStripMenuItem_Click);
            // 
            // spectralToolStripMenuItem
            // 
            this.spectralToolStripMenuItem.Enabled = false;
            this.spectralToolStripMenuItem.Name = "spectralToolStripMenuItem";
            this.spectralToolStripMenuItem.Size = new System.Drawing.Size(200, 34);
            this.spectralToolStripMenuItem.Text = "&Spectral...";
            this.spectralToolStripMenuItem.Click += new System.EventHandler(this.SpectralToolStripMenuItem_Click);
            // 
            // librarySettingsToolStripMenuItem
            // 
            this.librarySettingsToolStripMenuItem.Enabled = false;
            this.librarySettingsToolStripMenuItem.Name = "librarySettingsToolStripMenuItem";
            this.librarySettingsToolStripMenuItem.Size = new System.Drawing.Size(200, 34);
            this.librarySettingsToolStripMenuItem.Text = "&Library...";
            this.librarySettingsToolStripMenuItem.Click += new System.EventHandler(this.LibraryToolStripMenuItem_Click);
            // 
            // modeToolStripMenuItem
            // 
            this.modeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.matchingToolStripMenuItem,
            this.librayOnlyToolStripMenuItem});
            this.modeToolStripMenuItem.Name = "modeToolStripMenuItem";
            this.modeToolStripMenuItem.Size = new System.Drawing.Size(200, 34);
            this.modeToolStripMenuItem.Text = "&Mode";
            // 
            // matchingToolStripMenuItem
            // 
            this.matchingToolStripMenuItem.Checked = true;
            this.matchingToolStripMenuItem.CheckOnClick = true;
            this.matchingToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.matchingToolStripMenuItem.Name = "matchingToolStripMenuItem";
            this.matchingToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.matchingToolStripMenuItem.Size = new System.Drawing.Size(234, 34);
            this.matchingToolStripMenuItem.Text = "&Matching";
            this.matchingToolStripMenuItem.Click += new System.EventHandler(this.MatchingToolStripMenuItem_Click);
            // 
            // librayOnlyToolStripMenuItem
            // 
            this.librayOnlyToolStripMenuItem.CheckOnClick = true;
            this.librayOnlyToolStripMenuItem.Name = "librayOnlyToolStripMenuItem";
            this.librayOnlyToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F6;
            this.librayOnlyToolStripMenuItem.Size = new System.Drawing.Size(234, 34);
            this.librayOnlyToolStripMenuItem.Text = "&Libray Only";
            this.librayOnlyToolStripMenuItem.Click += new System.EventHandler(this.LibrayOnlyToolStripMenuItem_Click);
            // 
            // dataToolStripMenuItem
            // 
            this.dataToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.generateDataLibraryToolStripMenuItem});
            this.dataToolStripMenuItem.Name = "dataToolStripMenuItem";
            this.dataToolStripMenuItem.Size = new System.Drawing.Size(107, 29);
            this.dataToolStripMenuItem.Text = "&Advanced";
            // 
            // generateDataLibraryToolStripMenuItem
            // 
            this.generateDataLibraryToolStripMenuItem.Name = "generateDataLibraryToolStripMenuItem";
            this.generateDataLibraryToolStripMenuItem.Size = new System.Drawing.Size(322, 34);
            this.generateDataLibraryToolStripMenuItem.Text = "&Rebuild Library Database...";
            this.generateDataLibraryToolStripMenuItem.Click += new System.EventHandler(this.GenerateDataLibraryToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(65, 29);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(195, 34);
            this.aboutToolStripMenuItem.Text = "&About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItem_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.modeStatusLabel,
            this.statusLabel,
            this.libLabel,
            this.AqTimeLabel,
            this.CntTimeLabel});
            this.statusStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.statusStrip.Location = new System.Drawing.Point(0, 966);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1778, 32);
            this.statusStrip.TabIndex = 2;
            this.statusStrip.Text = "statusStrip";
            this.statusStrip.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Main_MouseClick);
            // 
            // modeStatusLabel
            // 
            this.modeStatusLabel.Name = "modeStatusLabel";
            this.modeStatusLabel.Size = new System.Drawing.Size(142, 25);
            this.modeStatusLabel.Text = "Mode: Matching";
            // 
            // rightClickMenu
            // 
            this.rightClickMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.rightClickMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showDaughtersRightClickMenuItem,
            this.toolStripMenuItem7,
            this.pasteToolStripMenuItem,
            this.toolStripMenuItem5,
            this.clearMatchesToolStripMenuItem,
            this.clearSelectionToolStripMenuItem,
            this.clearCurrentRowToolStripMenuItem,
            this.toolStripMenuItem4,
            this.rightClickWriteToolStripMenuItem});
            this.rightClickMenu.Name = "rightClickMenu";
            this.rightClickMenu.Size = new System.Drawing.Size(252, 214);
            // 
            // showDaughtersRightClickMenuItem
            // 
            this.showDaughtersRightClickMenuItem.Checked = true;
            this.showDaughtersRightClickMenuItem.CheckOnClick = true;
            this.showDaughtersRightClickMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showDaughtersRightClickMenuItem.Name = "showDaughtersRightClickMenuItem";
            this.showDaughtersRightClickMenuItem.Size = new System.Drawing.Size(251, 32);
            this.showDaughtersRightClickMenuItem.Text = "Show Daughter Lines";
            this.showDaughtersRightClickMenuItem.Click += new System.EventHandler(this.ShowDaughtersToolStripMenuItem_Click);
            // 
            // toolStripMenuItem7
            // 
            this.toolStripMenuItem7.Name = "toolStripMenuItem7";
            this.toolStripMenuItem7.Size = new System.Drawing.Size(248, 6);
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.Enabled = false;
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(251, 32);
            this.pasteToolStripMenuItem.Text = "Paste";
            this.pasteToolStripMenuItem.Click += new System.EventHandler(this.PasteToolStripMenuItem_Click);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(248, 6);
            // 
            // clearMatchesToolStripMenuItem
            // 
            this.clearMatchesToolStripMenuItem.Enabled = false;
            this.clearMatchesToolStripMenuItem.Name = "clearMatchesToolStripMenuItem";
            this.clearMatchesToolStripMenuItem.Size = new System.Drawing.Size(251, 32);
            this.clearMatchesToolStripMenuItem.Text = "Clear Matches";
            this.clearMatchesToolStripMenuItem.Click += new System.EventHandler(this.ClearMatchesToolStripMenuItem_Click);
            // 
            // clearSelectionToolStripMenuItem
            // 
            this.clearSelectionToolStripMenuItem.Enabled = false;
            this.clearSelectionToolStripMenuItem.Name = "clearSelectionToolStripMenuItem";
            this.clearSelectionToolStripMenuItem.Size = new System.Drawing.Size(251, 32);
            this.clearSelectionToolStripMenuItem.Text = "Clear Selection";
            this.clearSelectionToolStripMenuItem.Click += new System.EventHandler(this.ClearSelectionToolStripMenuItem_Click);
            // 
            // clearCurrentRowToolStripMenuItem
            // 
            this.clearCurrentRowToolStripMenuItem.Name = "clearCurrentRowToolStripMenuItem";
            this.clearCurrentRowToolStripMenuItem.Size = new System.Drawing.Size(251, 32);
            this.clearCurrentRowToolStripMenuItem.Text = "Clear Current Row";
            this.clearCurrentRowToolStripMenuItem.Click += new System.EventHandler(this.ClearCurrentRowToolStripMenuItem_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(248, 6);
            // 
            // rightClickWriteToolStripMenuItem
            // 
            this.rightClickWriteToolStripMenuItem.Enabled = false;
            this.rightClickWriteToolStripMenuItem.Name = "rightClickWriteToolStripMenuItem";
            this.rightClickWriteToolStripMenuItem.Size = new System.Drawing.Size(251, 32);
            this.rightClickWriteToolStripMenuItem.Text = "Write";
            this.rightClickWriteToolStripMenuItem.Click += new System.EventHandler(this.WriteToolStripMenuItem_Click);
            // 
            // Main
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1778, 998);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.lineMatchScore);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.matchedLinesGrid);
            this.Controls.Add(this.matchedNuclidesGrid);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.peaksGrid);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Main";
            this.Text = "Peak Map";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Form1_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.Form1_DragEnter);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Main_KeyDown);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Main_MouseClick);
            ((System.ComponentModel.ISupportInitialize)(this.peaksGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.matchedNuclidesGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.matchedLinesGrid)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.rightClickMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView peaksGrid;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.ToolStripStatusLabel CntTimeLabel;
        private System.Windows.Forms.DataGridView matchedNuclidesGrid;
        private System.Windows.Forms.DataGridView matchedLinesGrid;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lineMatchScore;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem linesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem writeSelectedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem writeMatchedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem writeAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel AqTimeLabel;
        private System.Windows.Forms.ToolStripStatusLabel libLabel;
        private System.Windows.Forms.ToolStripMenuItem lineMatchSettingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SaveLibraryToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem dataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem generateDataLibraryToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripMenuItem spectralToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem writeYieldToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem writeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem modeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem matchingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem librayOnlyToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel modeStatusLabel;
        private System.Windows.Forms.ContextMenuStrip rightClickMenu;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem clearMatchesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearSelectionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearCurrentRowToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem rightClickWriteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem librarySettingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showDaughtersToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem6;
        private System.Windows.Forms.ToolStripMenuItem showDaughtersRightClickMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem7;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem8;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearInputToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
    }
}

