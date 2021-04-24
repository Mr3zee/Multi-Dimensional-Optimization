using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MultyDimentionalOptimization.graphics;

namespace MultyDimentionalOptimization
{
    public partial class ContourForm : Form
    {
        public ContourForm()
        {
            InitializeComponent();
            Width = 500;
            Height = 500;
            Paint += Draw.Paint;
        }
    }
}