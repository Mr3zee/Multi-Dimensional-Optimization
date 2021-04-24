using System.Windows.Forms;
using MultiDimensionalOptimization.draw;

namespace MultiDimensionalOptimization
{
    public partial class ContourForm : Form
    {
        public ContourForm()
        {
            InitializeComponent();
            Width = Window.Width;
            Height = Window.Height;
            var window = new Window(Refresh, Controls);
            Paint += window.Paint;

            // Define the border style of the form to a dialog box.
            FormBorderStyle = FormBorderStyle.FixedDialog;

            // Set the MaximizeBox to false to remove the maximize box.
            MaximizeBox = false;

            // Set the MinimizeBox to false to remove the minimize box.
            MinimizeBox = false;

            // Set the start position of the form to the center of the screen.
            StartPosition = FormStartPosition.CenterScreen;

        }
    }
}