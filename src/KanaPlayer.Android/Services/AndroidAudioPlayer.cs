using System;
using System.ComponentModel;
using Android.Media;
using Android.Provider;
using KanaPlayer.Core.Interfaces;
using Stream = System.IO.Stream;

namespace KanaPlayer.Android.Services;

public class AndroidAudioPlayer: IAudioPlayer
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public PlayStatus Status { get; }
    public double Progress { get; set; }
    public TimeSpan Duration { get; }
    public double Volume { get; set; }
    public void Load(Stream audioStream)
    {
        var mediaPlayer = new MediaPlayer();
        mediaPlayer.Reset();
        mediaPlayer.SetAudioStreamType(audioStream);
    }
    public void Play()
    {
        throw new NotImplementedException();
    }
    public void Pause()
    {
        throw new NotImplementedException();
    }
}