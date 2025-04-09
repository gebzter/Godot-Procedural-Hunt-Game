using Godot;
using System;

public partial class Player : CharacterBody2D
{
	//references.
	[Export] private PackedScene _tracks; //tracks player periodically leaves behind for enemy to follow.
	[Export] private Node2D _gameNode; //main node for the game world.


	[Export] private float _speed = 200.0f;
	[Export] private float _deceleration = 100f;



	double _tracksTimer = 0;
	[Export] double _tracksInterval = 5; //time interval between leaving tracks.

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

			velocity.X = direction.X * _speed;
			velocity.Y = direction.Y * _speed;
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
}
