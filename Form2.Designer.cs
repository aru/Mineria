using Microsoft.Xna.Framework.Graphics;

namespace Stereo
{
    public partial class Form2
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
            this.stereoCopy1 = new Stereo.StereoCopy();
            this.SuspendLayout();
            // 
            // stereoCopy1
            // 
            this.stereoCopy1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.stereoCopy1.Location = new System.Drawing.Point(0, 0);
            this.stereoCopy1.Name = "stereoCopy1";
            this.stereoCopy1.Size = new System.Drawing.Size(634, 474);
            this.stereoCopy1.TabIndex = 0;
            this.stereoCopy1.Text = "stereoCopy1";
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(634, 474);
            this.Controls.Add(this.stereoCopy1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Name = "Form2";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Form2";
            this.TopMost = true;
            this.SizeChanged += new System.EventHandler(Form2_SizeChanged);
            this.ResumeLayout(false);

        }

        /// <summary>
        /// Totally my own code for Designer support - modify
        /// the contents of this method with the code editor
        /// </summary>
        private void InitializeComponent( Texture2D text )
        {
            this.stereoCopy1 = new Stereo.StereoCopy( text );
            this.SuspendLayout();
            // 
            // stereoCopy1
            // 
            this.stereoCopy1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.stereoCopy1.Location = new System.Drawing.Point(0, 0);
            this.stereoCopy1.Name = "stereoCopy1";
            this.stereoCopy1.Size = new System.Drawing.Size(634, 474);
            this.stereoCopy1.TabIndex = 0;
            this.stereoCopy1.Text = "stereoCopy1";
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(634, 474);
            this.Controls.Add(this.stereoCopy1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Name = "Form2";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Form2";
            this.TopMost = true;
            this.ResumeLayout(false);

        }
        /// </summary>

        #endregion

        //public StereoCopy stereoCopy1;
    }
}