using Godot;
using System;

//class for individual segment of an enemy tentacle.

public partial class TentacleSegment : Node2D
{
	[Export] private String _jointObjectPath; //path relative to joint that the it should attach to.
	[Export] private PinJoint2D _joint;

    public void InitialiseJoint()
    {
        _joint.NodeA = _jointObjectPath;
    }

}
