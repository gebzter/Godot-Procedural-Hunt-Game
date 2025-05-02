using Godot;
using System;
using System.Collections.Generic;
using System.Numerics;

//base class for enemy that hunts player.
public partial class Enemy : Node2D
{
	//reference attributes assigned manually upon instantiation.
	private MapGenerator _mapGenerator;
	public MapGenerator MapGenerator
	{
		get { return _mapGenerator; }
		set { _mapGenerator = value; }
	}

	private Player _player;
	public Player Player
	{
		get { return _player; }
		set { _player = value; }
	}

	//references.
	[Export] protected Node2D _visuals; //basic node holding visual elements.

	protected Dictionary<Godot.Vector2, int> _points; //dictionary storing all position/ID pairs of A* points.

	[Export] protected float _pathfindFalloff = 12f; //range at which the enemy stops pathfinding to a specific point and changes its behaviour.

	protected bool _beginEnemy = false;

	protected Godot.Vector2 currentPos;
	protected Godot.Vector2 playerPos;
	protected Godot.Vector2 _lastPlayerPos = new Godot.Vector2(); //last position player was detected at.

	protected EnemyBehaviour _behaviour = EnemyBehaviour.Wander;

	private float _moveAngle = 0; //angle from current position to next tile.

	public void StartEnemyBehaviour()
	{
		GD.Print("Enemy behaviour start");
		_points = MapGenerator.Points;

		_beginEnemy = true;
	}

	protected void Pathfind(Godot.Vector2 targetPos, float interpolationWeight)
	{
		//updated position used to determine if next position to move to needs updating.
		Godot.Vector2 updatedPos = MapGenerator.AStar.GetPointPosition(MapGenerator.AStar.GetClosestPoint(Position));

		//only updates currentPos if it has changed point.
		if (currentPos != updatedPos)
		{
			currentPos = updatedPos;
		}
		
		playerPos = MapGenerator.AStar.GetPointPosition(MapGenerator.AStar.GetClosestPoint(targetPos));

		Godot.Vector2[] path = MapGenerator.AStar.GetPointPath(_points[currentPos], _points[playerPos], true);

		Godot.Vector2 nextPos = new Godot.Vector2();

		//doesn't move if there is nowhere to move to.
		if (path.Length > 1)
		{
			nextPos = path[1];

			float posX = Mathf.Lerp(Position.X, nextPos.X, interpolationWeight);
			float posY = Mathf.Lerp(Position.Y, nextPos.Y, interpolationWeight);

			Position = new Godot.Vector2(posX, posY);

			_moveAngle = (nextPos - Position).Angle();
			_visuals.Rotation = _moveAngle;
		}
	}

	//enums.
	public enum EnemyBehaviour
	{
		Wander,
		Hunt,
		Chase
	}
}
