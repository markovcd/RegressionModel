namespace ExcelAddin
{
    partial class MainForm
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
            this.dataButton = new System.Windows.Forms.Button();
            this.dataText = new System.Windows.Forms.TextBox();
            this.dataLabel = new System.Windows.Forms.Label();
            this.okButton = new System.Windows.Forms.Button();
            this.termsLabel = new System.Windows.Forms.Label();
            this.termsText = new System.Windows.Forms.TextBox();
            this.termsButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // dataButton
            // 
            this.dataButton.Location = new System.Drawing.Point(215, 49);
            this.dataButton.Name = "dataButton";
            this.dataButton.Size = new System.Drawing.Size(22, 20);
            this.dataButton.TabIndex = 0;
            this.dataButton.Text = "...";
            this.dataButton.UseVisualStyleBackColor = true;
            this.dataButton.Click += new System.EventHandler(this.dataButton_Click);
            // 
            // dataText
            // 
            this.dataText.Location = new System.Drawing.Point(12, 49);
            this.dataText.Name = "dataText";
            this.dataText.Size = new System.Drawing.Size(200, 20);
            this.dataText.TabIndex = 1;
            // 
            // dataLabel
            // 
            this.dataLabel.Location = new System.Drawing.Point(12, 9);
            this.dataLabel.Name = "dataLabel";
            this.dataLabel.Size = new System.Drawing.Size(228, 37);
            this.dataLabel.TabIndex = 2;
            this.dataLabel.Text = "Choose data (one single column range per variable, first range is Y):";
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(152, 149);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(88, 28);
            this.okButton.TabIndex = 3;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // termsLabel
            // 
            this.termsLabel.Location = new System.Drawing.Point(12, 84);
            this.termsLabel.Name = "termsLabel";
            this.termsLabel.Size = new System.Drawing.Size(180, 22);
            this.termsLabel.TabIndex = 6;
            this.termsLabel.Text = "Choose range with function terms:";
            // 
            // termsText
            // 
            this.termsText.Location = new System.Drawing.Point(12, 109);
            this.termsText.Name = "termsText";
            this.termsText.Size = new System.Drawing.Size(200, 20);
            this.termsText.TabIndex = 5;
            // 
            // termsButton
            // 
            this.termsButton.Location = new System.Drawing.Point(215, 109);
            this.termsButton.Name = "termsButton";
            this.termsButton.Size = new System.Drawing.Size(22, 20);
            this.termsButton.TabIndex = 4;
            this.termsButton.Text = "...";
            this.termsButton.UseVisualStyleBackColor = true;
            this.termsButton.Click += new System.EventHandler(this.termsButton_Click);
            // 
            // MainForm
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(249, 189);
            this.Controls.Add(this.termsLabel);
            this.Controls.Add(this.termsText);
            this.Controls.Add(this.termsButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.dataLabel);
            this.Controls.Add(this.dataText);
            this.Controls.Add(this.dataButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "MainForm";
            this.Text = "RegressionModel";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button dataButton;
        private System.Windows.Forms.TextBox dataText;
        private System.Windows.Forms.Label dataLabel;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label termsLabel;
        private System.Windows.Forms.TextBox termsText;
        private System.Windows.Forms.Button termsButton;
    }
}