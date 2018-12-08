﻿using System;
using System.IO;
using System.Windows.Forms;
using Jammit.Model;
using Jammit.Properties;

namespace Jammit
{

  public partial class LibraryWindow : Form
  {
    
    public LibraryWindow()
    {
      InitializeComponent();
      UpdateListView();
    }

    private void UpdateListView()
    {
      if (Settings.Default.TrackPath == "")
      {
        ShowOptions();
      }

      var tracks = Library.GetSongs();
      tracks.Sort((t1, t2) => t1.Artist.CompareTo(t2.Artist) * 10 + t1.Name.CompareTo(t2.Name));
      listView1.Items.Clear();
      foreach (var t in tracks)
      {
        this.listView1.Items.Add(new ListViewItem
        {
          Tag = t,
          Text = t.Name,
          SubItems = { t.Artist, t.Instrument, t.Album }
        });
      }
    }

    private void ShowOptions()
    {
      if (new OptionsWindow().ShowDialog(this) == DialogResult.OK)
      {
        Library.ResetCache();
        UpdateListView();
      }
    }

    private void ShowDownloadWindow()
    {
      new Forms.DownloadForm().ShowDialog(this);
      Library.ResetCache();
      UpdateListView();
    }

    private void ShowSettingsWindow()
    {
      new Forms.SettingsWindow().ShowDialog(this);
    }

    private void rescanContentFilesToolStripMenuItem_Click(object sender, EventArgs e)
    {
      Library.UpdateCache();
      UpdateListView();
    }

    private void LibraryWindow_Load(object sender, EventArgs e)
    {

    }

    private void listView1_DoubleClick(object sender, EventArgs e)
    {
      if (listView1.SelectedItems.Count == 1)
      {
        try
        {
          var sw = new SongWindow(listView1.SelectedItems[0].Tag as SongMeta);
          sw.Show();
        }
        catch (Exception ex)
        {
          MessageBox.Show(this, "Error loading song: " + ex.Message);
        }
      }
    }

    private void quitToolStripMenuItem_Click(object sender, EventArgs e)
    {
      Application.Exit();
    }

    private void clearContentCacheToolStripMenuItem_Click(object sender, EventArgs e)
    {
      Library.ResetCache();
      UpdateListView();
    }

    private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
    {
      new AboutBox().ShowDialog(this);
    }

    private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      ShowOptions();
    }

    private void downloadSongsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      ShowDownloadWindow();
    }

    private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      ShowSettingsWindow();
    }
  }
}
