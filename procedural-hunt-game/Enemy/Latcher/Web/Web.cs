namespace Enemy.Latcher;

using Godot;
using System;

public partial class Web : Node2D
{
	//positions where web anchors onto walls.
	[Export] private Vector2 _anchor1, _anchor2;
	[Export] private int _resolution = 16;

	public Vector2 Anchor1
	{
		get { return _anchor1; }
		set { _anchor1 = value; }
	}

	public Vector2 Anchor2
	{
		get { return _anchor2; }
		set { _anchor2 = value; }
	}

	public override void _Ready()
	{
		float length = Anchor1.DistanceTo(Anchor2) / _resolution;
		Scale = new Vector2(Scale.X * length, Scale.Y);

		float angle = (Anchor1 - Anchor2).Angle();
		Rotation = angle;

		Position = (_anchor1 + _anchor2) / 2;

		GD.Print("Web initialised: " + Anchor1 + " " + Anchor2);
	}

	public event Action WebCollisionEvent; //event for notifying when the player has collided with the web.

    public void WebCollision() //function used whenever action is fired.
    {
        if (WebCollisionEvent != null) //checks that there are methods subscribed to event.
        {
            WebCollisionEvent(); //fires event.
        }
    }

	//called when colliding with player layer from signal.
	public void _on_area_2d_body_entered(Node2D body)
	{
		WebCollision();
		QueueFree();
	}
}
