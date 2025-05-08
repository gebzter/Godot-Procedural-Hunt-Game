namespace Enemy.Latcher;

using Core.Player;
using Godot;
using System;

//spawns webs for Latcher enemy.
public partial class WebSpawner : Node2D
{
	[Export] private PackedScene _web;
	[Export] private float _minWebLength = 30f, _maxWebLength = 64f;

	//angle at which secondary raycasts are fired off from primary raycast to ensure web anchors are not too exposed.
	//measured in radians.
	[Export] private float _secondaryWebRaycastAngle;
	[Export] private float _webCheckTolerance = 1.25f; //multiplier applied to secondary web raycast length to determine validity.

	//places a web object between two walls.
	public void PlaceWeb(RayCast2D rayCast, Player player, Latcher latcher)
	{
		Node2D newWeb = _web.Instantiate<Node2D>();
		Web webScript = (Web) newWeb;

		Random random = new Random();

		Godot.Vector2 webDirection = new Godot.Vector2(random.Next(-1, 1), random.Next(-1, 1));
		rayCast.TargetPosition = GlobalPosition + webDirection * _maxWebLength / 2;

		rayCast.ForceRaycastUpdate();

		GodotObject collision = rayCast.GetCollider();

		if (collision == null || collision == player || !SecondaryWebRaycasts(rayCast.TargetPosition, rayCast, player))
		{
			newWeb.QueueFree();
			return;
		}

		webScript.Anchor1 = rayCast.GetCollisionPoint();

		//checks other direction.
		rayCast.TargetPosition = rayCast.TargetPosition * -1;

		rayCast.ForceRaycastUpdate();

		collision = rayCast.GetCollider();

		if (collision == null || collision == player || !SecondaryWebRaycasts(rayCast.TargetPosition, rayCast, player))
		{
			newWeb.QueueFree();
			return;
		}

		webScript.Anchor2 = rayCast.GetCollisionPoint();

		if (webScript.Anchor1.DistanceTo(webScript.Anchor2) < _minWebLength)
		{
			newWeb.QueueFree();
			return;
		}

		GetTree().CurrentScene.AddChild(newWeb);
	}

	//fires 2 additional raycasts on either side of the web to determine if there is enough nearby terrain to anchor onto.
	public bool SecondaryWebRaycasts(Godot.Vector2 originalTargetPos, RayCast2D rayCast, Player player)
	{
		float length = (originalTargetPos - GlobalPosition).Length() * _webCheckTolerance;
		float originalAngle = (originalTargetPos - GlobalPosition).Angle();

		//checks one side of the web.
		Godot.Vector2 testPos = new Godot.Vector2(length * Mathf.Sin(originalAngle), length * Mathf.Cos(originalAngle + _secondaryWebRaycastAngle));

		rayCast.TargetPosition = testPos;
		rayCast.ForceRaycastUpdate();
		GodotObject collision = rayCast.GetCollider();

		if (collision == null)
		{
			return false;
		}

		//checks other side.
		testPos = new Godot.Vector2(length * Mathf.Sin(originalAngle), length * Mathf.Cos(originalAngle - _secondaryWebRaycastAngle));

		rayCast.TargetPosition = testPos;
		rayCast.ForceRaycastUpdate();
		collision = rayCast.GetCollider();

		if (collision == null)
		{
			return false;
		}

		return true;
	}
}
