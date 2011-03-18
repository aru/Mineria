using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Stereo
{
    public partial class Form2 : Form
    {
        public StereoCopy stereoCopy1 { get; protected set; }
        
        public Form2()
        {
            InitializeComponent();
        }
        public Form2( Texture2D text )
        {
            InitializeComponent( text );
        }
        private void Form2_SizeChanged(object sender, System.EventArgs e)
        {
            this.FormBorderStyle = FormBorderStyle.None;
        }
    }
}
