namespace Enemy.Latcher.Audio;

using Core.Audio;
using Core.Player;
using Godot;
using System;
using System.Threading.Tasks;

//dynamically controls audio for latcher.


public partial class LatcherAudioController : AudioController
{
	//references.
	[Export] private AudioStreamMP3[] _distantSounds;
	[Export] private AudioStreamMP3[] _passiveSounds;
	[Export] private AudioStreamMP3[] _aggressiveSounds;

	//reference to player used to gauge distance.
	private Player _player;
	[Export] private Latcher _latcher;

	//minimum and maximum durations between sounds being played in seconds
	[Export] private int _minSoundInterval = 1, _maxSoundInterval = 3;

	//minimum distance enemy must be from player to play distant sounds.
	[Export] private int _distantThreshold = 750;

	//maximum distances that distant and passive/aggressive sounds can be heard respectively.
	[Export] private int _distantMaxRange = 3000, _closeMaxRange = 500;

	private double _noiseTimer = 0;
	private Random _random;

    public override void _Ready()
    {
		_player = _latcher.Player;
		_random = new Random();
        //NoiseCycle();
    }



    public override void _PhysicsProcess(double delta)
    {
		//will not continue until current sound has finished.
		if (_streamPlayer.Playing == true)
		{
			return;
		}

		_noiseTimer -= delta;

		//timer has expired.
        if (_noiseTimer <= 0)
		{
			_noiseTimer = _random.Next(_minSoundInterval, _maxSoundInterval);


			AudioStreamMP3 sound = _distantSounds[0]; //need to initialise value.
			
			if (_latcher.GlobalPosition.DistanceTo(_player.GlobalPosition) >= _distantThreshold)
			{
				sound = _distantSounds[_random.Next(0, _distantSounds.Length)];
				_streamPlayer.MaxDistance = _distantMaxRange;
			}
			else
			{
				switch (_latcher.Behaviour)
				{
					case Enemy.EnemyBehaviour.Wander:
						sound = _passiveSounds[_random.Next(0, _passiveSounds.Length)];
						break;
					case Enemy.EnemyBehaviour.Hunt:
						sound = _aggressiveSounds[_random.Next(0, _aggressiveSounds.Length)];
						break;
					case Enemy.EnemyBehaviour.Chase:
						sound = _aggressiveSounds[_random.Next(0, _aggressiveSounds.Length)];
						break;
					default:
						break;
				}

				_streamPlayer.MaxDistance = _closeMaxRange;
			}

			AddSound(sound);
			_streamPlayer.Play(0);
		}
    }
}
