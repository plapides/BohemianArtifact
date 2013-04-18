namespace ArtifactEditor
{
    partial class Form1
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
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.btSave = new System.Windows.Forms.Button();
            this.tbUseDate = new System.Windows.Forms.TextBox();
            this.tbUseDateStart = new System.Windows.Forms.TextBox();
            this.btGoToArtifact = new System.Windows.Forms.Button();
            this.tbArtifactId = new System.Windows.Forms.TextBox();
            this.btNext = new System.Windows.Forms.Button();
            this.btPrevious = new System.Windows.Forms.Button();
            this.tbCatalogNumber = new System.Windows.Forms.TextBox();
            this.tbUseDateEnd = new System.Windows.Forms.TextBox();
            this.rbExact = new System.Windows.Forms.RadioButton();
            this.rbCirca = new System.Windows.Forms.RadioButton();
            this.rbBefore = new System.Windows.Forms.RadioButton();
            this.rbAfter = new System.Windows.Forms.RadioButton();
            this.rbBetween = new System.Windows.Forms.RadioButton();
            this.tbUseQualifier = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbManuDateStart = new System.Windows.Forms.TextBox();
            this.tbManuDateEnd = new System.Windows.Forms.TextBox();
            this.tbManuQualifier = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox
            // 
            this.pictureBox.Location = new System.Drawing.Point(12, 12);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(512, 512);
            this.pictureBox.TabIndex = 0;
            this.pictureBox.TabStop = false;
            this.pictureBox.Click += new System.EventHandler(this.pictureBox_Click);
            // 
            // btSave
            // 
            this.btSave.BackColor = System.Drawing.Color.White;
            this.btSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btSave.Location = new System.Drawing.Point(564, 377);
            this.btSave.Name = "btSave";
            this.btSave.Size = new System.Drawing.Size(248, 147);
            this.btSave.TabIndex = 1;
            this.btSave.Text = "Save and Next";
            this.btSave.UseVisualStyleBackColor = false;
            this.btSave.Click += new System.EventHandler(this.btSave_Click);
            // 
            // tbUseDate
            // 
            this.tbUseDate.Enabled = false;
            this.tbUseDate.Location = new System.Drawing.Point(564, 214);
            this.tbUseDate.Multiline = true;
            this.tbUseDate.Name = "tbUseDate";
            this.tbUseDate.Size = new System.Drawing.Size(373, 41);
            this.tbUseDate.TabIndex = 2;
            // 
            // tbUseDateStart
            // 
            this.tbUseDateStart.Location = new System.Drawing.Point(564, 261);
            this.tbUseDateStart.Name = "tbUseDateStart";
            this.tbUseDateStart.Size = new System.Drawing.Size(119, 20);
            this.tbUseDateStart.TabIndex = 3;
            // 
            // btGoToArtifact
            // 
            this.btGoToArtifact.Location = new System.Drawing.Point(641, 64);
            this.btGoToArtifact.Name = "btGoToArtifact";
            this.btGoToArtifact.Size = new System.Drawing.Size(94, 55);
            this.btGoToArtifact.TabIndex = 14;
            this.btGoToArtifact.Text = "Go to artifact";
            this.btGoToArtifact.UseVisualStyleBackColor = true;
            this.btGoToArtifact.Click += new System.EventHandler(this.btGoToArtifact_Click);
            // 
            // tbArtifactId
            // 
            this.tbArtifactId.Location = new System.Drawing.Point(564, 38);
            this.tbArtifactId.Name = "tbArtifactId";
            this.tbArtifactId.Size = new System.Drawing.Size(248, 20);
            this.tbArtifactId.TabIndex = 5;
            // 
            // btNext
            // 
            this.btNext.Location = new System.Drawing.Point(741, 64);
            this.btNext.Name = "btNext";
            this.btNext.Size = new System.Drawing.Size(71, 55);
            this.btNext.TabIndex = 6;
            this.btNext.Text = "Next";
            this.btNext.UseVisualStyleBackColor = true;
            this.btNext.Click += new System.EventHandler(this.btNext_Click);
            // 
            // btPrevious
            // 
            this.btPrevious.Location = new System.Drawing.Point(564, 64);
            this.btPrevious.Name = "btPrevious";
            this.btPrevious.Size = new System.Drawing.Size(71, 55);
            this.btPrevious.TabIndex = 7;
            this.btPrevious.Text = "Previous";
            this.btPrevious.UseVisualStyleBackColor = true;
            this.btPrevious.Click += new System.EventHandler(this.btPrevious_Click);
            // 
            // tbCatalogNumber
            // 
            this.tbCatalogNumber.Enabled = false;
            this.tbCatalogNumber.Location = new System.Drawing.Point(564, 12);
            this.tbCatalogNumber.Name = "tbCatalogNumber";
            this.tbCatalogNumber.Size = new System.Drawing.Size(248, 20);
            this.tbCatalogNumber.TabIndex = 8;
            // 
            // tbUseDateEnd
            // 
            this.tbUseDateEnd.Location = new System.Drawing.Point(689, 261);
            this.tbUseDateEnd.Name = "tbUseDateEnd";
            this.tbUseDateEnd.Size = new System.Drawing.Size(123, 20);
            this.tbUseDateEnd.TabIndex = 4;
            // 
            // rbExact
            // 
            this.rbExact.AutoSize = true;
            this.rbExact.Location = new System.Drawing.Point(564, 287);
            this.rbExact.Name = "rbExact";
            this.rbExact.Size = new System.Drawing.Size(52, 17);
            this.rbExact.TabIndex = 10;
            this.rbExact.TabStop = true;
            this.rbExact.Text = "Exact";
            this.rbExact.UseVisualStyleBackColor = true;
            // 
            // rbCirca
            // 
            this.rbCirca.AutoSize = true;
            this.rbCirca.Location = new System.Drawing.Point(659, 310);
            this.rbCirca.Name = "rbCirca";
            this.rbCirca.Size = new System.Drawing.Size(49, 17);
            this.rbCirca.TabIndex = 11;
            this.rbCirca.TabStop = true;
            this.rbCirca.Text = "Circa";
            this.rbCirca.UseVisualStyleBackColor = true;
            // 
            // rbBefore
            // 
            this.rbBefore.AutoSize = true;
            this.rbBefore.Location = new System.Drawing.Point(659, 287);
            this.rbBefore.Name = "rbBefore";
            this.rbBefore.Size = new System.Drawing.Size(56, 17);
            this.rbBefore.TabIndex = 12;
            this.rbBefore.TabStop = true;
            this.rbBefore.Text = "Before";
            this.rbBefore.UseVisualStyleBackColor = true;
            // 
            // rbAfter
            // 
            this.rbAfter.AutoSize = true;
            this.rbAfter.Location = new System.Drawing.Point(765, 287);
            this.rbAfter.Name = "rbAfter";
            this.rbAfter.Size = new System.Drawing.Size(47, 17);
            this.rbAfter.TabIndex = 13;
            this.rbAfter.TabStop = true;
            this.rbAfter.Text = "After";
            this.rbAfter.UseVisualStyleBackColor = true;
            // 
            // rbBetween
            // 
            this.rbBetween.AutoSize = true;
            this.rbBetween.Location = new System.Drawing.Point(564, 310);
            this.rbBetween.Name = "rbBetween";
            this.rbBetween.Size = new System.Drawing.Size(67, 17);
            this.rbBetween.TabIndex = 14;
            this.rbBetween.TabStop = true;
            this.rbBetween.Text = "Between";
            this.rbBetween.UseVisualStyleBackColor = true;
            // 
            // tbUseQualifier
            // 
            this.tbUseQualifier.Location = new System.Drawing.Point(818, 261);
            this.tbUseQualifier.Name = "tbUseQualifier";
            this.tbUseQualifier.Size = new System.Drawing.Size(56, 20);
            this.tbUseQualifier.TabIndex = 5;
            this.tbUseQualifier.TextChanged += new System.EventHandler(this.tbQualifier_TextChanged);
            this.tbUseQualifier.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbQualifier_KeyPress);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(561, 198);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 15;
            this.label1.Text = "Use Date";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(561, 159);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(93, 13);
            this.label2.TabIndex = 16;
            this.label2.Text = "Manufacture Date";
            // 
            // tbManuDateStart
            // 
            this.tbManuDateStart.Location = new System.Drawing.Point(564, 175);
            this.tbManuDateStart.Name = "tbManuDateStart";
            this.tbManuDateStart.Size = new System.Drawing.Size(119, 20);
            this.tbManuDateStart.TabIndex = 17;
            // 
            // tbManuDateEnd
            // 
            this.tbManuDateEnd.Location = new System.Drawing.Point(689, 175);
            this.tbManuDateEnd.Name = "tbManuDateEnd";
            this.tbManuDateEnd.Size = new System.Drawing.Size(119, 20);
            this.tbManuDateEnd.TabIndex = 18;
            // 
            // tbManuQualifier
            // 
            this.tbManuQualifier.Location = new System.Drawing.Point(814, 175);
            this.tbManuQualifier.Name = "tbManuQualifier";
            this.tbManuQualifier.Size = new System.Drawing.Size(60, 20);
            this.tbManuQualifier.TabIndex = 19;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(949, 575);
            this.Controls.Add(this.tbManuQualifier);
            this.Controls.Add(this.tbManuDateEnd);
            this.Controls.Add(this.tbManuDateStart);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbUseQualifier);
            this.Controls.Add(this.rbBetween);
            this.Controls.Add(this.rbAfter);
            this.Controls.Add(this.rbBefore);
            this.Controls.Add(this.rbCirca);
            this.Controls.Add(this.rbExact);
            this.Controls.Add(this.tbUseDateEnd);
            this.Controls.Add(this.tbCatalogNumber);
            this.Controls.Add(this.btPrevious);
            this.Controls.Add(this.btNext);
            this.Controls.Add(this.tbArtifactId);
            this.Controls.Add(this.btGoToArtifact);
            this.Controls.Add(this.tbUseDateStart);
            this.Controls.Add(this.tbUseDate);
            this.Controls.Add(this.btSave);
            this.Controls.Add(this.pictureBox);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.Button btSave;
        private System.Windows.Forms.TextBox tbUseDate;
        private System.Windows.Forms.TextBox tbUseDateStart;
        private System.Windows.Forms.Button btGoToArtifact;
        private System.Windows.Forms.TextBox tbArtifactId;
        private System.Windows.Forms.Button btNext;
        private System.Windows.Forms.Button btPrevious;
        private System.Windows.Forms.TextBox tbCatalogNumber;
        private System.Windows.Forms.TextBox tbUseDateEnd;
        private System.Windows.Forms.RadioButton rbExact;
        private System.Windows.Forms.RadioButton rbCirca;
        private System.Windows.Forms.RadioButton rbBefore;
        private System.Windows.Forms.RadioButton rbAfter;
        private System.Windows.Forms.RadioButton rbBetween;
        private System.Windows.Forms.TextBox tbUseQualifier;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbManuDateStart;
        private System.Windows.Forms.TextBox tbManuDateEnd;
        private System.Windows.Forms.TextBox tbManuQualifier;
    }
}

