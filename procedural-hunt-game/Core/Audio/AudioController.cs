namespace Core.Audio;

using Godot;
using System;

//base class for controlling audio nodes.

public partial class AudioController : Node
{
	[Export] protected AudioStreamPlayer2D _streamPlayer;

	protected void AddSound(AudioStreamMP3 sound)
	{
		_streamPlayer.Stream = sound;
	}

	protected void ClearAudio()
	{
		_streamPlayer.Stream = null;
	}
}
