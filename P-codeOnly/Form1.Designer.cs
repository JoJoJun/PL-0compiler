namespace compilerCplusFour
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
            this.SourceText = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.resultBox = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.instruBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // SourceText
            // 
            this.SourceText.Location = new System.Drawing.Point(23, 17);
            this.SourceText.Multiline = true;
            this.SourceText.Name = "SourceText";
            this.SourceText.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.SourceText.Size = new System.Drawing.Size(305, 306);
            this.SourceText.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(130, 330);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "read";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // resultBox
            // 
            this.resultBox.Location = new System.Drawing.Point(372, 17);
            this.resultBox.Multiline = true;
            this.resultBox.Name = "resultBox";
            this.resultBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.resultBox.Size = new System.Drawing.Size(302, 306);
            this.resultBox.TabIndex = 2;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(509, 330);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "compile";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // instruBox
            // 
            this.instruBox.Location = new System.Drawing.Point(711, 17);
            this.instruBox.Multiline = true;
            this.instruBox.Name = "instruBox";
            this.instruBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.instruBox.Size = new System.Drawing.Size(275, 306);
            this.instruBox.TabIndex = 4;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(998, 443);
            this.Controls.Add(this.instruBox);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.resultBox);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.SourceText);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox SourceText;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox resultBox;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox instruBox;
    }
}

