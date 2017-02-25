﻿using System;
using System.Collections.Generic;
using System.IO;
using Jammit.Model;
using NAudio.Wave;

namespace Jammit.Audio
{
  /// <summary>
  /// Handles playing the audio for a track.
  /// </summary>
  public class JammitZipSongPlayer : ISongPlayer
  {
    private readonly List<WaveChannel32> _channels;
    private readonly WaveMixerStream32 _mixer;
    private readonly WaveOutEvent _waveOut;
    private readonly List<string> _chanNames;

    /// <summary>
    /// Creates a song player for the given song.
    /// </summary>
    /// <param name="s">ZipSong to play.</param>
    internal JammitZipSongPlayer(ZipSong s)
    {
      _waveOut = new WaveOutEvent();
      _mixer = new WaveMixerStream32();
      _channels = new List<WaveChannel32>();
      _chanNames = new List<string>();

      using (var x = s.OpenZip())
      {
        foreach (var t in s.Tracks)
        {
          if (t.ClassName == "JMFileTrack")
          {
            var e = x.GetEntry($"{s.Metadata.GuidString}.jcf/{t.Id}_jcfx");
            using (var st = e.Open())
            {
              var ms = new MemoryStream();
              st.CopyTo(ms);
              _channels.Add(new WaveChannel32(new ImaWaveStream(ms)));
              _chanNames.Add(t.Title);
            }
          }
          else if (t.ClassName == "JMClickTrack")
          {
            _channels.Add(new WaveChannel32(new ClickTrackStream(s.Beats)));
            _chanNames.Add(t.Title);
          }
        }
      }
      
      foreach (var d in _channels)
      {
        _mixer.AddInputStream(d);
        d.Volume = 0.75f;
      }
      _waveOut.PlaybackStopped += (sender, args) => {Position = TimeSpan.Zero;};
      _waveOut.DesiredLatency = 60;
      _waveOut.NumberOfBuffers = 2;
      _waveOut.Init(_mixer);
    }

    ~JammitZipSongPlayer()
    {
      if(_waveOut.PlaybackState == PlaybackState.Playing)
        _waveOut.Stop();
      _waveOut.Dispose();
    }

    public void Play()
    {
      _waveOut.Play();
    }

    public void Stop()
    {
      _waveOut.Stop();
    }

    public void Pause()
    {
      _waveOut.Pause();
    }

    public PlaybackState State => _waveOut.PlaybackState;

    public TimeSpan Position
    {
      get { return _mixer.CurrentTime; }
      set { _mixer.CurrentTime = value; }
    }

    // 32-bit samples, 2 channels = 8 bytes per sample frame
    public long PositionSamples => _mixer.Position/8;

    public TimeSpan Length => _mixer.TotalTime;
    public int Channels => _channels.Count;
    public string GetChannelName(int channel) => _chanNames[channel];

    public void SetChannelVolume(int channel, float volume)
    {
      _channels[channel].Volume = volume;
    }

    public float GetChannelVolume(int channel) => _channels[channel].Volume;
  }
}