using System;
using System.Windows.Forms;

namespace Jammit.Forms
{
  public partial class SettingsWindow : Form
  {
    public SettingsWindow()
    {
      InitializeComponent();

      latencyUpDown.Value = Properties.Settings.Default.Latency;
    }

    private void cancelButton_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }

    private void saveButton_Click(object sender, EventArgs e)
    {
      Properties.Settings.Default.Latency = latencyUpDown.Value;
      DialogResult = DialogResult.OK;
      Close();
    }
  }
}
