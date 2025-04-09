using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

//class for enemy tentacle.

public partial class Tentacle : Node2D
{
	//references.
	[Export] private Node2D _tipSegment;
	[Export] private StaticBody2D _anchor;
	[Export] private Enemy _enemy;

	[Export] private float _lungeForce, _retractForce;
	[Export] private Vector2 _forceDirection = new Vector2(0, 0);
	[Export] private int _maxLungeInterval, _minLungeInterval; //time between lunges in milliseconds.
	[Export] private int _retractInterval; //time between retractions in milliseconds.
	
	private bool _continueLunging = true;
	private RigidBody2D _tipRb;



    public override void _Ready()
    {
		_tipRb = (RigidBody2D) _tipSegment.GetChild(0, true);
        Lunges();
    }



	private async void Lunges()
	{
		Random random = new Random();

		while (_continueLunging)
		{
			await Task.Delay(random.Next(_minLungeInterval, _maxLungeInterval));

			_tipRb.ApplyForce(DetermineForceDirection() * _lungeForce);

			await Task.Delay(_retractInterval);

			_tipRb.ApplyForce(DetermineForceDirection() * -_lungeForce);
		}
	}

	private Vector2 DetermineForceDirection()
	{
		if (_enemy.CanSeePlayer)
		{
			_tipRb.LinearVelocity = new Vector2(0, 0);
			return (_enemy.Player.Position - Position).Normalized(); //towards player.
		}
		else
		{
			return _forceDirection.Normalized();
		}
	}
}
