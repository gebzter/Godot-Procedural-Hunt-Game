namespace Enemy.Latcher;

using Godot;
using System;
using System.Collections.Generic;
using System.Numerics;

//child class for latcher enemy that uses line of sight and hunting to locate player.
public partial class Latcher : Enemy
{
	//references.
	[Export] private RayCast2D _rayCast;
	[Export] private float _rayCastLength = 500;


 	//interpolation weight of Lerp() function.
	[Export] private float _wanderInterpolationWeight;
	[Export] private float _huntInterpolationWeight;
	[Export] private float _chaseInterpolationWeight;


	[Export] private float _passiveDetectionRadius; //range at which the enemy will detect the player regardless of other conditions, e.g. line of sight.

	[Export] private float _spawnRadius; //distance from (0, 0) that enemy is forbidden from spawning at.
	public float SpawnRadius
	{
		get {return _spawnRadius;}
		set {_spawnRadius = value;}
	}


	private bool _canSeePlayer; //has line of sight to player.
	public bool CanSeePlayer
	{
		get { return _canSeePlayer; }
		set { _canSeePlayer = value; }
	}

	private double _wanderTimer = 0; //timer used for wandering.
	[Export] private int _minWanderDuration, _maxWanderDuration;
	private Godot.Vector2 _wanderTargetPos = new Godot.Vector2(0, 0); //random position used with wandering algorithm.

	//periodically pathfinds to random tile.
	private void Wander()
	{
		if (_wanderTimer <= 0 || GlobalPosition.DistanceTo(_wanderTargetPos) <= _pathfindFalloff)
		{
			Random random = new Random();
			_wanderTimer = random.Next(_minWanderDuration, _maxWanderDuration);
			_wanderTargetPos = MapGenerator.GetRandomPosition(random);
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
			Behaviour = EnemyBehaviour.Chase;
		}
		else if (GlobalPosition.DistanceTo(_lastPlayerPos) > _pathfindFalloff && Behaviour != EnemyBehaviour.Wander)
		{
			Behaviour = EnemyBehaviour.Hunt;
		}
		else
		{
			Behaviour = EnemyBehaviour.Wander;
		}

		switch (Behaviour)
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

    //updates information known about player.
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
}
