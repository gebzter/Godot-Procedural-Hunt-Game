using Godot;
using System;
using System.Collections.Generic;
using System.Numerics;

//base class for enemy that hunts player.

public partial class Enemy : Node2D
{
	//references.
	[Export] private MapGenerator mapGenerator;
	[Export] private Player _player;


	private AStar2D _aStar;
	private Dictionary<Godot.Vector2, int> _points;


	//interpolation weight of Lerp() function.
	[Export] private float _interpolationWeight;

	private bool _beginHunt = false;
	Godot.Vector2 currentPos;
	Godot.Vector2 playerPos;

	public void HuntPlayer()
	{
		GD.Print("Hunting player");
		_aStar = mapGenerator.AStar;
		_points = mapGenerator.Points;
		_beginHunt = true;
	}

	//called every physics cycle.
    public override void _PhysicsProcess(double delta)
    {
		if (!_beginHunt)
		{
			return;
		}

		//updated position used to determine if next position to move to needs updating.
		Godot.Vector2 updatedPos = _aStar.GetPointPosition(_aStar.GetClosestPoint(Position));

		//only updates currentPos if it has changed point.
		if (currentPos != updatedPos)
		{
			currentPos = updatedPos;
		}
		
		playerPos = _aStar.GetPointPosition(_aStar.GetClosestPoint(_player.Position));

		Godot.Vector2[] path = _aStar.GetPointPath(_points[currentPos], _points[playerPos], true);

		//doesn't move if there is nowhere to move to.
		if (path.Length > 1)
		{
			Godot.Vector2 nextPos = path[1];

			float posX = Mathf.Lerp(Position.X, nextPos.X, _interpolationWeight);
			float posY = Mathf.Lerp(Position.Y, nextPos.Y, _interpolationWeight);

			Position = new Godot.Vector2(posX, posY);
		}
    }
}
