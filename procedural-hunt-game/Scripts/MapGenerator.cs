using Godot;
using System;
using System.Collections.Generic;
using System.Numerics;

public partial class MapGenerator : Node2D
{
	//room scenes sorted in relation of how they can be generates relative to the previous room's location.
	//e.g. an up room can be generated above another scene (it has a lower exit to connect to).
	[Export] private PackedScene[] _upRoomScenes, _downRoomScenes, _leftRoomScenes, _rightRoomScenes;

	[Export] private int _depthLimit;

	//collection of rooms after being instantiated.
	private Dictionary<Godot.Vector2, Node> instantiatedRooms = new Dictionary<Godot.Vector2, Node>();

	//contains arrays of PackedScenes.
	private PackedScene[][] _fullScenes;

    public override void _Ready()
    {
		_fullScenes = [_upRoomScenes, _downRoomScenes, _leftRoomScenes, _rightRoomScenes];
		GenerateMap();
    }


	public void GenerateMap()
	{
		//randomly selects first room.
		Node rootRoom;

		Random random = new Random();
		int randomIndex = random.Next(0, 5);

		PackedScene[] sceneArray = _fullScenes[randomIndex];

		randomIndex = random.Next(0, sceneArray.Length);
		rootRoom = sceneArray[randomIndex].Instantiate();

		instantiatedRooms.Add(new Godot.Vector2(0, 0), rootRoom);

		GenerateRoom(rootRoom as Room, _depthLimit);
	}

	//recursively generates rooms until depth limit reached or no room possible to generate.
	private void GenerateRoom(Room previousRoom, int remainingDepth)
	{
		if (remainingDepth <= 0)
		{
			return;
		}

		Godot.Vector2 oldRoomSize = previousRoom.GetRoomSize();
		GD.Print(oldRoomSize);

		remainingDepth -= 1;

	}
}
