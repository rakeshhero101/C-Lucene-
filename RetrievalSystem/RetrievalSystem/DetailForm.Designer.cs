namespace RetrievalSystem
{
    partial class DetailForm
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
            this.txtAbtract = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // txtAbtract
            // 
            this.txtAbtract.Location = new System.Drawing.Point(13, 13);
            this.txtAbtract.Multiline = true;
            this.txtAbtract.Name = "txtAbtract";
            this.txtAbtract.ReadOnly = true;
            this.txtAbtract.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtAbtract.Size = new System.Drawing.Size(775, 142);
            this.txtAbtract.TabIndex = 0;
            // 
            // DetailForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 167);
            this.Controls.Add(this.txtAbtract);
            this.Name = "DetailForm";
            this.Text = "DetailForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtAbtract;
    }
}