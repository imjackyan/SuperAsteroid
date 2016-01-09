using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SuperAsteroids
{
    public partial class Asteroids : Form
    {
        public Asteroids()
        {
            InitializeComponent();
        }

        private void Asteroids_Shown(object sender, EventArgs e)
        { }

        private void Asteroids_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up) { Program.Players[0].Controls[0] = true; }
            if (e.KeyCode == Keys.Down) { Program.Players[0].Controls[2] = true; }
            if (e.KeyCode == Keys.Left) { Program.Players[0].Controls[1] = true; }
            if (e.KeyCode == Keys.Right) { Program.Players[0].Controls[3] = true; }

            if (e.KeyCode == Keys.Space) { Program.Players[0].FireBullet(); }
        }

        private void Asteroids_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up) { Program.Players[0].Controls[0] = false; }
            if (e.KeyCode == Keys.Down) { Program.Players[0].Controls[2] = false; }
            if (e.KeyCode == Keys.Left) { Program.Players[0].Controls[1] = false; }
            if (e.KeyCode == Keys.Right) { Program.Players[0].Controls[3] = false; }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Visible = false;
            button1.Enabled = false;
            pictureBox1.Visible = false;
            pictureBox1.Enabled = false;
            Program.Initialize(this);
        }
    }
}
