using Godot;
using System;

//class to hold data for each room node.

public partial class Room : Node2D
{
	[Export] private bool _hasExitTop, _hasExitBottom, _hasExitLeft, _hasExitRight;

	[Export] private String _name;


	//accessors/mutators.
	public bool HasExitTop()
	{
		return _hasExitTop;

	}	
	public bool HasExitBottom()
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

	public Vector2 GetRoomPosition()
	{
		return Position;
	}

	public void SetRoomPosition(Vector2 position)
	{
		this.Position = position;
	}

	public String GetName()
	{
		return _name;
	}
}
