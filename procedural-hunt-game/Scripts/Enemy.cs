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
	[Export] private Node2D _visuals; //basic node holding visual elements.

	[Export] private RayCast2D _rayCast;
	[Export] private float _rayCastLength = 500;


	private AStar2D _aStar;
	public AStar2D AStar
	{
		get { return _aStar; }
		set {_aStar = value; }
	}

	private Dictionary<Godot.Vector2, int> _points;


 	//interpolation weight of Lerp() function.
	[Export] private float _wanderInterpolationWeight;
	[Export] private float _huntInterpolationWeight;
	[Export] private float _chaseInterpolationWeight;

	[Export] private float _passiveDetectionRadius; //range at which the enemy will detect the player regardless of other conditions, e.g. line of sight.

	[Export] private float _pathfindFalloff = 8f; //range at which the enemy stops pathfinding to a specific point and changes its behaviour.

	[Export] private float _spawnRadius; //distance from (0, 0) that enemy is forbidden from spawning at.
	public float SpawnRadius
	{
		get {return _spawnRadius;}
		set {_spawnRadius = value;}
	}

	private bool _beginEnemy = false;
	Godot.Vector2 currentPos;
	Godot.Vector2 playerPos;

	private EnemyBehaviour _behaviour = EnemyBehaviour.Wander;

	private Godot.Vector2 _lastPlayerPos = new Godot.Vector2(); //last postiion player was detected at.
	private bool _canSeePlayer; //has line of sight to player.
	public bool CanSeePlayer
	{
		get { return _canSeePlayer; }
		set { _canSeePlayer = value; }
	}

	private double _wanderTimer = 0; //timer used for wandering.
	[Export] private int _minWanderDuration, _maxWanderDuration;
	private Godot.Vector2 _wanderTargetPos = new Godot.Vector2(0, 0); //random position used with wandering algorithm.

	private float _moveAngle = 0; //angle from current position to next tile.

	public void StartEnemyBehaviour()
	{
		GD.Print("Enemy behaviour start");
		_points = MapGenerator.Points;

		_beginEnemy = true;
	}

	public Godot.Vector2 GetRandomPosition(Random random)
	{
		long[] points = AStar.GetPointIds();

		int randomIndex = random.Next(0, points.Length);

		Godot.Vector2 randomPos = AStar.GetPointPosition(points[randomIndex]);

		return randomPos;
	}

	//periodically pathfinds to random tile.
	private void Wander()
	{
		if (_wanderTimer <= 0 || GlobalPosition.DistanceTo(_wanderTargetPos) <= _pathfindFalloff)
		{
			Random random = new Random();
			_wanderTimer = random.Next(_minWanderDuration, _maxWanderDuration);
			_wanderTargetPos = GetRandomPosition(random);
		}

		Pathfind(_wanderTargetPos, _wanderInterpolationWeight);
	}

	//pathfinds to last position player was detected at.
	private void HuntPlayer()
	{
		Pathfind(_lastPlayerPos, _huntInterpolationWeight);
	}

	//pathfinds directly to player at higher speed.
	private void ChasePlayer()
	{
		Pathfind(Player.GlobalPosition, _chaseInterpolationWeight);
	}

	private void Pathfind(Godot.Vector2 targetPos, float interpolationWeight)
	{
		//updated position used to determine if next position to move to needs updating.
		Godot.Vector2 updatedPos = AStar.GetPointPosition(AStar.GetClosestPoint(Position));

		//only updates currentPos if it has changed point.
		if (currentPos != updatedPos)
		{
			currentPos = updatedPos;
		}
		
		playerPos = AStar.GetPointPosition(AStar.GetClosestPoint(targetPos));

		Godot.Vector2[] path = AStar.GetPointPath(_points[currentPos], _points[playerPos], true);

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

	//called every physics cycle.
    public override void _PhysicsProcess(double delta)
    {
		if (!_beginEnemy)
		{
			return;
		}

		_wanderTimer -= delta;

		PlayerDetection();

		if (CanSeePlayer == true)
		{
			_behaviour = EnemyBehaviour.Chase;
		}
		else if (GlobalPosition.DistanceTo(_lastPlayerPos) > _pathfindFalloff && _behaviour != EnemyBehaviour.Wander)
		{
			_behaviour = EnemyBehaviour.Hunt;
		}
		else
		{
			_behaviour = EnemyBehaviour.Wander;
		}

		switch (_behaviour)
		{
			case (EnemyBehaviour.Wander):
				Wander();
				break;
			case (EnemyBehaviour.Hunt):
				HuntPlayer();
				break;
			case (EnemyBehaviour.Chase):
				ChasePlayer();
				break;
		}

		
    }

	private void PlayerDetection()
	{
		if (GlobalPosition.DistanceTo(Player.GlobalPosition) <= _passiveDetectionRadius)
		{
			_lastPlayerPos = Player.GlobalPosition;
		}

		_rayCast.TargetPosition = (Player.GlobalPosition - _rayCast.GlobalPosition).Normalized() * _rayCastLength;
		_rayCast.ForceRaycastUpdate();

		GodotObject collision = _rayCast.GetCollider();

		if (collision == null)
		{
			CanSeePlayer = false;
			return;
		}
		else if (collision == Player)
		{
			CanSeePlayer = true;
			_lastPlayerPos = Player.GlobalPosition;
			return;
		}

		CanSeePlayer = false;
	}

	//called when Area2D enters this Node's Area2D.
	public void OnAreaEntered(Area2D area)
	{
		if (Player == null) //negates race condition when enemy is instantiated.
		{
			return;
		}

		//enemy uses tracks laid down by player to follow.
		_lastPlayerPos = Player.GlobalPosition;

		area.GetParent().QueueFree();
	}

	//enums.
	public enum EnemyBehaviour
	{
		Wander,
		Hunt,
		Chase
	}
}
