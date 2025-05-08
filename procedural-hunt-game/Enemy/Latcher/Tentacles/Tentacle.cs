namespace Enemy.Latcher;

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

//class for latcher tentacle.

public partial class Tentacle : Node2D
{
	//references.
	[Export] private Node2D _tipSegment;
	[Export] private StaticBody2D _anchor;
	[Export] private Latcher _latcher;
	[Export] private Node2D _segmentWrapper; //empty node used to group segments together.

	[Export] private float _lungeForce;
	[Export] private Vector2 _forceDirection = new Vector2(0, 0);
	[Export] private int _maxLungeInterval, _minLungeInterval; //time between lunges in milliseconds.
	
	private RigidBody2D _tipRb;



    public override void _Ready()
    {
		_tipRb = (RigidBody2D) _tipSegment.GetChild(0, true);

		//initialises segment positions to match anchor.
		foreach (Node2D segment in _segmentWrapper.GetChildren())
		{
			PinJoint2D joint = segment.GetChild<PinJoint2D>(1);
			joint.GlobalPosition += _anchor.GlobalPosition; //adjusts joint position.

			RigidBody2D rb = segment.GetChild<RigidBody2D>(0);
			rb.GlobalPosition += _anchor.GlobalPosition; //adjusts rigidbody position.

			((TentacleSegment) segment).InitialiseJoint(); //initialises the nodes for each joint for each segment.
		}
    }

	private double _lungeIntervalTimer = 0;

	private Random _random = new Random();

	//tracks whether next movement should be a lunge or a retraction.
	private bool _lungeNext = true;

    public override void _PhysicsProcess(double delta)
    {
        if (_lungeIntervalTimer > 0)
		{
			_lungeIntervalTimer -= delta;
			return;
		}

		if (_lungeNext)
		{
			_tipRb.ApplyForce(DetermineForceDirection() * _lungeForce);
		}
		else
		{
			_tipRb.ApplyForce(DetermineForceDirection() * -_lungeForce);
		}

		_lungeNext = !_lungeNext;
		
		_lungeIntervalTimer = _random.Next(_minLungeInterval, _maxLungeInterval) / 1000;
    }

	private Vector2 DetermineForceDirection()
	{
		if (_latcher.CanSeePlayer)
		{
			_tipRb.LinearVelocity = new Vector2(0, 0);
			return (_latcher.Player.Position - Position).Normalized(); //towards player.
		}
		else
		{
			return _forceDirection.Normalized();
		}
	}
}
