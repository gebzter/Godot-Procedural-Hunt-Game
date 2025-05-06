namespace Core.Player;

using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody2D
{
	//references.
	[Export] private PackedScene _tracks; //tracks player periodically leaves behind for enemy to follow.
	[Export] private Node2D _gameNode; //main node for the game world.


	[Export] private float _speed = 200.0f;
	public float Speed
	{
		get { return _speed; }
		set { _speed = value; }
	}

	[Export] private float _deceleration = 100f;



	double _tracksTimer = 0;
	[Export] double _tracksInterval = 5; //time interval between leaving tracks.

	[Export] private StatusEffectHolder _effectHolder;
	public StatusEffectHolder EffectHolder
	{
		get { return _effectHolder; }
		set { _effectHolder = value; }
	}

	public override void _PhysicsProcess(double delta)
	{
		_tracksTimer -= delta;
		Vector2 velocity = Velocity;

		//periodically leaves behind tracks for enemy to follow.
		if (_tracksTimer <= 0)
		{
			_tracksTimer = _tracksInterval;

			Node2D newTracks = _tracks.Instantiate<Node2D>();
			_gameNode.AddChild(newTracks);
			
			((Tracks) newTracks).GlobalPosition = GlobalPosition;
		}

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		if (direction != Vector2.Zero)
		{
			direction = direction.Normalized(); //sets magnitude of vector to 1;

			velocity.X = direction.X * Speed * EffectHolder.SpeedMultiplier;
			velocity.Y = direction.Y * Speed * EffectHolder.SpeedMultiplier;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, _deceleration);
			velocity.Y = Mathf.MoveToward(Velocity.Y, 0, _deceleration);
		}

		Velocity = velocity;
		MoveAndSlide();


		Vector2 mousePos = GetGlobalMousePosition();
		Rotation = (mousePos - GlobalPosition).Angle();
	}

	public void OnWebCollision()
	{
		GD.Print("P: Web collision detected");

		EffectHolder.AddEffect(StatusEffect.EffectType.Webbed);
	}
}
