﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Claunia.PropertyList;
using Jammit.Audio;

namespace Jammit.Model
{
  /// <summary>
  /// Represents a song backed with a standard .zip content file.
  /// </summary>
  class ZipSong : ISong
  {
    public SongMeta Metadata { get; }

    public IReadOnlyList<Track> Tracks { get; }
    public IReadOnlyList<Beat> Beats { get; }
    public IReadOnlyList<Section> Sections { get; }

    public ZipSong(SongMeta metadata)
    {
      Metadata = metadata;
      Tracks = InitTracks();
      Beats = InitBeats();
      Sections = InitSections();
    }

    public sbyte[] GetWaveform()
    {
      using (var a = OpenZip())
      using (var s = a.GetEntry($"{Metadata.GuidString}.jcf/music.waveform").Open())
      using (var ms = new MemoryStream())
      {
        s.CopyTo(ms);
        return new UnionArray { Bytes = ms.ToArray() }.Sbytes;
      }
    }

    public Image GetCover()
    {
      using (var x = OpenZip())
      using (var stream = x.GetEntry($"{Metadata.GuidString}.jcf/cover.jpg").Open())
      using (var ms = new MemoryStream())
      {
        stream.CopyTo(ms);
        return Image.FromStream(ms);
      }
    }

    public List<Image> GetNotation(Track t)
    {
      var ret = new List<Image>();
      if (!t.HasNotation) return null;
      using (var arc = OpenZip())
        for (var i = 0; i < t.NotationPages; i++)
          using (var img = arc.GetEntry($"{Metadata.GuidString}.jcf/{t.Id}_jcfn_{i:D2}").Open())
            ret.Add(Image.FromStream(img));

      return ret;
    }

    public List<Image> GetTablature(Track t)
    {
      var ret = new List<Image>();
      if (!t.HasTablature) return null;
      using (var arc = OpenZip())
        for (var i = 0; i < t.NotationPages; i++)
          using (var img = arc.GetEntry($"{Metadata.GuidString}.jcf/{t.Id}_jcft_{i:D2}").Open())
            ret.Add(Image.FromStream(img));

      return ret;
    }

    public ISongPlayer GetSongPlayer()
    {
      return new JammitZipSongPlayer(this);
    }

    private List<Track> InitTracks()
    {
      var tracks = new List<Track>();
      using (var x = OpenZip())
      {
        using (var s = x.GetEntry(Metadata.GuidString + ".jcf/tracks.plist").Open())
        {
          var tracksArray = (NSArray)PropertyListParser.Parse(s);
          foreach (var track in tracksArray.GetArray())
          {
            var dict = track as NSDictionary;
            if (dict == null) continue;
            var t = new Track
            {
              ClassName = dict.String("class"),
              Title = dict.String("title") ?? "",
              Id = dict.String("identifier") ?? ""
            };
            if (t.ClassName == "JMFileTrack")
            {
              t.ScoreSystemHeight = dict.Int("scoreSystemHeight") ?? 0;
              t.ScoreSystemInterval = dict.Int("scoreSystemInterval") ?? 0;
              if (x.GetEntry($"{Metadata.GuidString}.jcf/{t.Id}_jcfn_00") != null)
              {
                t.HasNotation = true;
                t.NotationPages = 1;
                while (x.GetEntry($"{Metadata.GuidString}.jcf/{t.Id}_jcfn_{t.NotationPages:D2}") != null) t.NotationPages++;
              }
              if (x.GetEntry($"{Metadata.GuidString}.jcf/{t.Id}_jcft_00") != null)
              {
                t.HasTablature = true;
                t.TablaturePages = 1;
                while (x.GetEntry($"{Metadata.GuidString}.jcf/{t.Id}_jcft_{t.TablaturePages:D2}") != null) t.TablaturePages++;
              }
            }
            tracks.Add(t);
          }
        }
      }
      return tracks;
    }

    private List<Beat> InitBeats()
    {
      NSArray beatArray, ghostArray;
      using (var arc = OpenZip())
      {
        using (var stream = arc.GetEntry($"{Metadata.GuidString}.jcf/beats.plist").Open())
          beatArray = (NSArray)PropertyListParser.Parse(stream);
        using (var stream = arc.GetEntry($"{Metadata.GuidString}.jcf/ghost.plist").Open())
          ghostArray = (NSArray) PropertyListParser.Parse(stream);
      }
      var beats = new List<Beat>();
      for (var i = 0; i < beatArray.Count; i++)
      {
        var dict = beatArray.GetArray()[i] as NSDictionary;
        beats.Add(new Beat
        {
          Time = dict.Double("position") ?? 0,
          IsDownBeat = dict.Bool("isDownbeat") ?? false,
          IsGhostBeat = (ghostArray.GetArray()[i] as NSDictionary).Bool("isGhostBeat") ?? false
        });
      }
      return beats;
    }

    private List<Section> InitSections()
    {
      NSArray sectionArray;
      using (var arc = OpenZip())
      using (var stream = arc.GetEntry($"{Metadata.GuidString}.jcf/sections.plist").Open())
        sectionArray = (NSArray)PropertyListParser.Parse(stream);
      return sectionArray.GetArray().OfType<NSDictionary>().Select(dict => new Section
      {
        BeatIdx = dict.Int("beat") ?? 0,
        Beat = Beats[dict.Int("beat") ?? 0],
        Number = dict.Int("number") ?? 0,
        Type = dict.Int("type") ?? 0
      }).ToList();
    }

    public ZipArchive OpenZip() => ZipFile.OpenRead(Metadata.ZipFileName);
  }
}