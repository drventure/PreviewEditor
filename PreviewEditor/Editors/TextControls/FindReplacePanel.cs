using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace PreviewEditor.Editors.TextControls
{
    public class FindReplacePanel : UserControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tbxFind = new System.Windows.Forms.TextBox();
            this.stripFind = new System.Windows.Forms.StatusStrip();
            this.btnCaseSensitive = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnWholeWord = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnRegex = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblResults = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnPrevious = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnNext = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnFindInSelection = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnClose = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnToggleFindReplace = new System.Windows.Forms.Button();
            this.stripReplace = new System.Windows.Forms.StatusStrip();
            this.btnReplaceNext = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnReplaceAll = new System.Windows.Forms.ToolStripStatusLabel();
            this.tbxReplace = new System.Windows.Forms.TextBox();
            this.splitSizer = new System.Windows.Forms.Splitter();
            this.stripFind.SuspendLayout();
            this.stripReplace.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbxFind
            // 
            this.tbxFind.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbxFind.Location = new System.Drawing.Point(53, 23);
            this.tbxFind.Margin = new System.Windows.Forms.Padding(6);
            this.tbxFind.Name = "tbxFind";
            this.tbxFind.Size = new System.Drawing.Size(431, 24);
            this.tbxFind.TabIndex = 0;
            this.tbxFind.TextChanged += new System.EventHandler(this.tbxFind_TextChanged);
            this.tbxFind.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbxFind_KeyDown);
            // 
            // stripFind
            // 
            this.stripFind.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.stripFind.AutoSize = false;
            this.stripFind.BackColor = System.Drawing.Color.Transparent;
            this.stripFind.Dock = System.Windows.Forms.DockStyle.None;
            this.stripFind.GripMargin = new System.Windows.Forms.Padding(0);
            this.stripFind.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.stripFind.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnCaseSensitive,
            this.btnWholeWord,
            this.btnRegex,
            this.lblResults,
            this.btnPrevious,
            this.btnNext,
            this.btnFindInSelection,
            this.btnClose});
            this.stripFind.Location = new System.Drawing.Point(495, 13);
            this.stripFind.Name = "stripFind";
            this.stripFind.Padding = new System.Windows.Forms.Padding(2, 0, 28, 0);
            this.stripFind.Size = new System.Drawing.Size(445, 40);
            this.stripFind.SizingGrip = false;
            this.stripFind.TabIndex = 1;
            this.stripFind.Text = "statusStrip1";
            // 
            // btnCaseSensitive
            // 
            this.btnCaseSensitive.Name = "btnCaseSensitive";
            this.btnCaseSensitive.Size = new System.Drawing.Size(41, 30);
            this.btnCaseSensitive.Text = "Aa";
            this.btnCaseSensitive.Click += new System.EventHandler(this.btnCaseSensitive_Click);
            // 
            // btnWholeWord
            // 
            this.btnWholeWord.Name = "btnWholeWord";
            this.btnWholeWord.Size = new System.Drawing.Size(40, 30);
            this.btnWholeWord.Text = "ab";
            this.btnWholeWord.Click += new System.EventHandler(this.btnWholeWord_Click);
            // 
            // btnRegex
            // 
            this.btnRegex.Name = "btnRegex";
            this.btnRegex.Size = new System.Drawing.Size(29, 30);
            this.btnRegex.Text = ".*";
            this.btnRegex.Click += new System.EventHandler(this.btnRegex_Click);
            // 
            // lblResults
            // 
            this.lblResults.Name = "lblResults";
            this.lblResults.Size = new System.Drawing.Size(93, 30);
            this.lblResults.Spring = true;
            this.lblResults.Text = "No results";
            // 
            // btnPrevious
            // 
            this.btnPrevious.Name = "btnPrevious";
            this.btnPrevious.Size = new System.Drawing.Size(43, 30);
            this.btnPrevious.Text = "UP";
            this.btnPrevious.Click += new System.EventHandler(this.btnPrevious_Click);
            // 
            // btnNext
            // 
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(49, 30);
            this.btnNext.Text = "DN";
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // btnFindInSelection
            // 
            this.btnFindInSelection.Name = "btnFindInSelection";
            this.btnFindInSelection.Size = new System.Drawing.Size(30, 30);
            this.btnFindInSelection.Text = "=";
            this.btnFindInSelection.Click += new System.EventHandler(this.btnFindInSelection_Click);
            // 
            // btnClose
            // 
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(28, 30);
            this.btnClose.Text = "X";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnToggleFindReplace
            // 
            this.btnToggleFindReplace.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.btnToggleFindReplace.FlatAppearance.BorderSize = 0;
            this.btnToggleFindReplace.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnToggleFindReplace.Location = new System.Drawing.Point(16, 6);
            this.btnToggleFindReplace.Margin = new System.Windows.Forms.Padding(6);
            this.btnToggleFindReplace.Name = "btnToggleFindReplace";
            this.btnToggleFindReplace.Size = new System.Drawing.Size(27, 106);
            this.btnToggleFindReplace.TabIndex = 2;
            this.btnToggleFindReplace.Text = ">";
            this.btnToggleFindReplace.UseVisualStyleBackColor = true;
            // 
            // stripReplace
            // 
            this.stripReplace.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.stripReplace.AutoSize = false;
            this.stripReplace.BackColor = System.Drawing.Color.Transparent;
            this.stripReplace.Dock = System.Windows.Forms.DockStyle.None;
            this.stripReplace.GripMargin = new System.Windows.Forms.Padding(0);
            this.stripReplace.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.stripReplace.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnReplaceNext,
            this.btnReplaceAll});
            this.stripReplace.Location = new System.Drawing.Point(495, 56);
            this.stripReplace.Name = "stripReplace";
            this.stripReplace.Padding = new System.Windows.Forms.Padding(2, 0, 28, 0);
            this.stripReplace.Size = new System.Drawing.Size(445, 38);
            this.stripReplace.SizingGrip = false;
            this.stripReplace.TabIndex = 4;
            this.stripReplace.Text = "statusStrip2";
            // 
            // btnReplaceNext
            // 
            this.btnReplaceNext.Name = "btnReplaceNext";
            this.btnReplaceNext.Size = new System.Drawing.Size(27, 28);
            this.btnReplaceNext.Text = "1";
            // 
            // btnReplaceAll
            // 
            this.btnReplaceAll.Name = "btnReplaceAll";
            this.btnReplaceAll.Size = new System.Drawing.Size(28, 28);
            this.btnReplaceAll.Text = "n";
            // 
            // tbxReplace
            // 
            this.tbxReplace.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbxReplace.Location = new System.Drawing.Point(54, 65);
            this.tbxReplace.Margin = new System.Windows.Forms.Padding(6);
            this.tbxReplace.Name = "tbxReplace";
            this.tbxReplace.Size = new System.Drawing.Size(431, 24);
            this.tbxReplace.TabIndex = 3;
            // 
            // splitSizer
            // 
            this.splitSizer.Location = new System.Drawing.Point(0, 0);
            this.splitSizer.Margin = new System.Windows.Forms.Padding(6);
            this.splitSizer.Name = "splitSizer";
            this.splitSizer.Size = new System.Drawing.Size(6, 117);
            this.splitSizer.TabIndex = 100;
            this.splitSizer.TabStop = false;
            // 
            // FindReplacePanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitSizer);
            this.Controls.Add(this.stripReplace);
            this.Controls.Add(this.tbxReplace);
            this.Controls.Add(this.btnToggleFindReplace);
            this.Controls.Add(this.stripFind);
            this.Controls.Add(this.tbxFind);
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "FindReplacePanel";
            this.Size = new System.Drawing.Size(956, 117);
            this.stripFind.ResumeLayout(false);
            this.stripFind.PerformLayout();
            this.stripReplace.ResumeLayout(false);
            this.stripReplace.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbxFind;
        private System.Windows.Forms.StatusStrip stripFind;
        private System.Windows.Forms.ToolStripStatusLabel btnCaseSensitive;
        private System.Windows.Forms.ToolStripStatusLabel btnWholeWord;
        private System.Windows.Forms.ToolStripStatusLabel btnRegex;
        private System.Windows.Forms.ToolStripStatusLabel lblResults;
        private System.Windows.Forms.ToolStripStatusLabel btnPrevious;
        private System.Windows.Forms.ToolStripStatusLabel btnNext;
        private System.Windows.Forms.ToolStripStatusLabel btnFindInSelection;
        private System.Windows.Forms.ToolStripStatusLabel btnClose;
        private System.Windows.Forms.Button btnToggleFindReplace;
        private System.Windows.Forms.StatusStrip stripReplace;
        private System.Windows.Forms.ToolStripStatusLabel btnReplaceNext;
        private System.Windows.Forms.ToolStripStatusLabel btnReplaceAll;
        private System.Windows.Forms.TextBox tbxReplace;
        private System.Windows.Forms.Splitter splitSizer;


        /// <summary>
        /// Event to indicate Description
        /// </summary>
        public event EventHandler FindNext;
        /// <summary>
        /// Called to signal to subscribers that Description
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnFindNext(EventArgs e)
        {
            this.FindNext?.Invoke(this, e);
        }


        /// <summary>
        /// Event to indicate Description
        /// </summary>
        public event EventHandler FindPrevious;
        /// <summary>
        /// Called to signal to subscribers that Description
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnFindPrevious(EventArgs e)
        {
            this.FindPrevious?.Invoke(this, e);
        }


        /// <summary>
        /// Event to indicate Description
        /// </summary>
        public event EventHandler ReplaceNext;
        /// <summary>
        /// Called to signal to subscribers that Description
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnReplaceNext(EventArgs e)
        {
            this.ReplaceNext?.Invoke(this, e);
        }


        /// <summary>
        /// Event to indicate Description
        /// </summary>
        public event EventHandler ReplaceAll;
        /// <summary>
        /// Called to signal to subscribers that Description
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnReplaceAll(EventArgs e)
        {
            this.ReplaceAll?.Invoke(this, e);
        }


        public FindReplacePanel()
        {
            InitializeComponent();

            //hook all child controls to monitor for certain keystrokes
            //there's no keypreview on UserControls, so this is it
            foreach (Control c in this.Controls)
            {
                c.KeyDown += (sender, e) =>
                {
                    if (e.KeyCode == Keys.Escape)
                    {
                        e.Handled = true;
                        this.Visible = false;
                    }
                };
            }

            this.FindInSelection = PreviewEditor.Settings.TextEditorOptions.FindInSelection;
            this.RegEx = PreviewEditor.Settings.TextEditorOptions.FindWithRegex;
            this.WholeWord = PreviewEditor.Settings.TextEditorOptions.FindWholeWordsOnly;
            this.CaseSensitive = PreviewEditor.Settings.TextEditorOptions.FindCaseSensitive;
        }


        public string FindText { get; set; }
        public string ReplaceText { get; set; }


        public bool CaseSensitive
        {
            get
            {
                return this.btnCaseSensitive.Font.Bold;
            }
            set
            {
                var style = value ? FontStyle.Bold : FontStyle.Regular;
                btnCaseSensitive.Font = new Font(btnCaseSensitive.Font, style);
                PreviewEditor.Settings.TextEditorOptions.FindCaseSensitive = value;
            }
        }


        public bool RegEx
        {
            get
            {
                return this.btnRegex.Font.Bold;
            }
            set
            {
                var style = value ? FontStyle.Bold : FontStyle.Regular;
                btnRegex.Font = new Font(btnRegex.Font, style);
                PreviewEditor.Settings.TextEditorOptions.FindWithRegex = value;
            }
        }


        public bool WholeWord
        {
            get
            {
                return this.btnWholeWord.Font.Bold;
            }
            set
            {
                var style = value ? FontStyle.Bold : FontStyle.Regular;
                btnWholeWord.Font = new Font(btnWholeWord.Font, style);
                PreviewEditor.Settings.TextEditorOptions.FindWholeWordsOnly = value;
            }
        }


        public bool FindInSelection
        {
            get
            {
                return this.btnFindInSelection.Font.Bold;
            }
            set
            {
                var style = value ? FontStyle.Bold : FontStyle.Regular;
                btnFindInSelection.Font = new Font(btnFindInSelection.Font, style);
                PreviewEditor.Settings.TextEditorOptions.FindInSelection = value;
            }
        }


        private void tbxFind_TextChanged(object sender, EventArgs e)
        {
            this.FindText = tbxFind.Text;
        }


        private void btnNext_Click(object sender, EventArgs e)
        {
            OnFindNext(new EventArgs());
        }


        private void btnPrevious_Click(object sender, EventArgs e)
        {
            OnFindPrevious(new EventArgs());
        }


        public bool IsFindActive
        {
            get
            {
                return !string.IsNullOrEmpty(this.FindText);
            }
        }


        private void tbxFind_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && this.IsFindActive)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                OnFindNext(new EventArgs());
            }
        }


        private void btnCaseSensitive_Click(object sender, EventArgs e)
        {
            this.CaseSensitive = !this.CaseSensitive;
        }


        private void btnRegex_Click(object sender, EventArgs e)
        {
            this.RegEx = !this.RegEx;
        }


        private void btnWholeWord_Click(object sender, EventArgs e)
        {
            this.WholeWord = !this.WholeWord;
        }


        private void btnFindInSelection_Click(object sender, EventArgs e)
        {
            this.FindInSelection = !this.FindInSelection;
        }


        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Visible = false;
        }
    }
}
