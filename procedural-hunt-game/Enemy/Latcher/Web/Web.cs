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
}
