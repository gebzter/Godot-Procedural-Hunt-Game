using Godot;
using System;

public partial class Room : Node2D
{
	//rooms fit on a grid where each square is 16x16 tiles.
	//tracks how many squares the room fits on.
	[Export] private Vector2 _roomSize;

	//tracks which exits the room has.
	[Export] private bool _hasExitTop, _hasExitBottom, _hasExitLeft, _hasExitRight;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

	}

	public Vector2 GetRoomSize()
	{
		return _roomSize;
	}

	public bool HasExitTop()
	{
		return _hasExitTop;

	}		public bool HasExitBottom()
	{
		return _hasExitBottom;
	}
		public bool HasExitLeft()
	{
		return _hasExitLeft;
	}

		public bool HasExitRight()
	{
		return _hasExitRight;
	}
}
