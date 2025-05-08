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

	[Export] private double _intervalTimer = 0;
	[Export] private double _pulseTimer = 0;
	[Export] private double _fadeTimer = 0;

    public override void _PhysicsProcess(double delta)
    {
		//waiting between pulses.
        if (_intervalTimer > 0)
		{
			_intervalTimer -= delta;
			return;
		}

		//pulse is getting brighter.
		if (_pulseTimer > 0)
		{
			_pulseTimer -= delta;
			_light.Energy += _maxEnergy * (float) delta;
			return;
		}

		//getting dimmer.
		if (_fadeTimer > 0)
		{
			_fadeTimer -= delta;
			_light.Energy -= _maxEnergy * (float) delta;
			return;
		}

		_light.Energy = 0;

		_intervalTimer = _pulseInterval;
		_pulseTimer = _pulseDuration / 2;
		_fadeTimer = _pulseDuration / 2;
    }
}
