using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] private float _speed = 200.0f;
	[Export] private float _deceleration = 100f;

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

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
	}
}
