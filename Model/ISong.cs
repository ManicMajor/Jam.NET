﻿using System.Collections.Generic;
using System.Drawing;
using Jammit.Audio;

namespace Jammit.Model
{
  public interface ISong
  {
    SongMeta Metadata { get; }

    IReadOnlyList<Track> Tracks { get; }
    IReadOnlyList<Beat> Beats { get; }
    IReadOnlyList<Section> Sections { get; }

    sbyte[] GetWaveform();
    Image GetCover();
    List<Image> GetNotation(Track t);
    List<Image> GetTablature(Track t);
    ISongPlayer GetSongPlayer();
  }
}