namespace Core.Visuals;

using Godot;
using System;
using System.Threading.Tasks;

//script that periodically pulses light.


public partial class LightPulse : PointLight2D
{
	[Export] private PointLight2D _light;

	[Export] private float _maxEnergy;
	[Export] private int _pulseInterval, _pulseDuration; //in milliseconds.

	[Export] private float _pulseFrames;

	private bool _continuePulses = true;

    public override void _Ready()
    {
		Pulses();
    }

	private async void Pulses()
	{
		while (_continuePulses == true && _pulseInterval != 0)
		{
			//gets brighter.
			await Task.Delay(_pulseInterval);

			for (int i = 0; i < _pulseFrames; i++)
			{
				_light.Energy += _maxEnergy / _pulseFrames;
				await Task.Delay((int) Math.Round(0.5f * _pulseDuration / _pulseFrames));
			}

			_light.Energy = _maxEnergy;

			//gets dimmer.
			for (int i = 0; i < _pulseFrames; i++)
			{
				_light.Energy -= _maxEnergy / _pulseFrames;
				await Task.Delay((int) Math.Round(0.5f * _pulseDuration / _pulseFrames));
			}

			_light.Energy = 0;
		}
	}
}
