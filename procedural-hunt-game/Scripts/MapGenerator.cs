using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;

//node that procedurally generates layout of map.

public partial class MapGenerator : Node2D
{
	//packed scenes that can be used to instantiate rooms.
	[Export] private PackedScene[] _rooms;
	private PackedScene[] _downExitRooms, _upExitRooms, _rightExitRooms, _leftExitRooms;

	//potential scenes the root room could use.
	[Export] private PackedScene[] _rootRooms;

	//max depth of each room generation branch.
	[Export] private int _depthLimit;

	//size of each square in the map grid.
	[Export] private int squareSize = 256;

	//collection of instantiated rooms.
	private Dictionary<Godot.Vector2, Node> instantiatedRooms = new Dictionary<Godot.Vector2, Node>();

    public override void _Ready()
    {
		//sorts rooms into respective arrays.
		_upExitRooms = new PackedScene[_rooms.Length];
		_downExitRooms = new PackedScene[_rooms.Length];
		_rightExitRooms = new PackedScene[_rooms.Length];
		_leftExitRooms = new PackedScene[_rooms.Length];

		for (int i = 0; i < _rooms.Length; i++)
		{
			Room room = _rooms[i].Instantiate() as Room;

			if (room.HasExitTop())
			{
				_upExitRooms[i] = _rooms[i];
			}
			if (room.HasExitBottom())
			{
				_downExitRooms[i] = _rooms[i];
			}
			if (room.HasExitRight())
			{
				_rightExitRooms[i] = _rooms[i];
			}
			if (room.HasExitLeft())
			{
				_leftExitRooms[i] = _rooms[i];
			}

			room.QueueFree();
		}

		GenerateMap();
    }


	public void GenerateMap()
	{
		//randomly selects first room.
		Node2D rootRoom;

		Random random = new Random();

		int randomIndex = random.Next(0, _rootRooms.Length);
		rootRoom = _rootRooms[randomIndex].Instantiate<Node2D>();

		rootRoom.Name = "RootRoom";

		//adds root room as a child to MapGenerator node.
		AddChild(rootRoom);

		instantiatedRooms.Add(new Godot.Vector2(0, 0), rootRoom);

		//begins recursive calls.
		GenerateRoom(rootRoom as Room, _depthLimit);

		GD.Print("Room Generation Finished");
	}

	//recursively generates rooms until depth limit reached or no room possible to generate.
	private void GenerateRoom(Room previousRoom, int remainingDepth)
	{
		//base case.
		if (remainingDepth <= 0)
		{
			return;
		}

		remainingDepth -= 1;
		
		Node2D newRoom = null;

		//refactored method to handle specifics of room.
		void GenerateRoomDetails(PackedScene[] scenes, Godot.Vector2 offset)
		{
			Random random = new Random();

			//first find what exits are required/forbidden for a given room position.
			Godot.Vector2 newPosition = previousRoom.Position + new Godot.Vector2(squareSize * offset.X, squareSize * offset.Y);
			PackedScene[][] requiredRooms = new PackedScene[4][];
			PackedScene[][] forbiddenRooms = new PackedScene[4][];

			//room present at top.
			Godot.Vector2 testPosition = newPosition + new Godot.Vector2(0, -squareSize);
			if (instantiatedRooms.ContainsKey(testPosition))
			{
				if (((Room) instantiatedRooms[testPosition]).HasExitBottom())
				{
					requiredRooms[0] = _upExitRooms;
				}
				else
				{
					forbiddenRooms[0] = _upExitRooms;
				}
			}
			else if (remainingDepth == 1) //end of recursion so needs to be dead end.
			{
				forbiddenRooms[0] = _upExitRooms;
			}

			//room present at bottom.
			testPosition = newPosition + new Godot.Vector2(0, squareSize);
			if (instantiatedRooms.ContainsKey(testPosition))
			{
				if (((Room) instantiatedRooms[testPosition]).HasExitTop())
				{
					requiredRooms[1] = _downExitRooms;
				}
				else
				{
					forbiddenRooms[1] = _downExitRooms;
				}
			}
			else if (remainingDepth == 1) //end of recursion so needs to be dead end.
			{
				forbiddenRooms[1] = _downExitRooms;
			}

			//room present at right
			testPosition = newPosition + new Godot.Vector2(squareSize, 0);
			if (instantiatedRooms.ContainsKey(testPosition))
			{
				if (((Room) instantiatedRooms[testPosition]).HasExitLeft())
				{
					requiredRooms[2] = _rightExitRooms;
				}
				else
				{
					forbiddenRooms[2] = _rightExitRooms;
				}
			}
			else if (remainingDepth == 1) //end of recursion so needs to be dead end.
			{
				forbiddenRooms[2] = _rightExitRooms;
			}

			//room present at left.
			testPosition = newPosition + new Godot.Vector2(-squareSize, 0);
			if (instantiatedRooms.ContainsKey(testPosition))
			{
				if (((Room) instantiatedRooms[testPosition]).HasExitRight())
				{
					requiredRooms[3] = _leftExitRooms;
				}
				else
				{
					forbiddenRooms[3] = _leftExitRooms;
				}
			}
			else if (remainingDepth == 1) //end of recursion so needs to be dead end.
			{
				forbiddenRooms[3] = _leftExitRooms;
			}

			//randomly pick new room in accordance with above rules.
			scenes = scenes.OrderBy(e => random.NextDouble()).ToArray(); //randomises array.

			bool continueSearch = true;
			PackedScene packedRoom = null; //chosen room to be instantiated.

			//iterates over sets of rooms, picking out the first room to meet requirements and avoid restrictions.
			for	(int i = 0; i < scenes.Length; i++)
			{
				if (scenes[i] == null)
				{
					continue;
				}

				packedRoom = scenes[i];

				continueSearch = false;

				for	(int j = 0; j < 4; j++)
				{
					//no adjacent rooms (exits or otherwise) in that direction.
					if (requiredRooms[j] == null && forbiddenRooms[j] == null)
					{
						//continues to next iteration, this direction doesn't matter.
						continue;
					}

					if (requiredRooms[j] != null)
					{
						//invalid therefore continue searching.
						if (!requiredRooms[j].Contains(packedRoom))
						{
							continueSearch = true;
							break;
						}
					}

					if (forbiddenRooms[j] != null)
					{
						//invalid therefore continue searching.
						if (forbiddenRooms[j].Contains(packedRoom))
						{
							continueSearch = true;
							break;
						}
					}

				}

				//a suitable room was found, therefore stop search.
				if (continueSearch == false)
				{
					packedRoom = scenes[i];
					break;
				}
			}

			newRoom = packedRoom.Instantiate<Node2D>();

			//room already exists in that position.
			if (instantiatedRooms.ContainsKey(newPosition))
			{
				newRoom.QueueFree();

				return;
			}
			else
			{
				newRoom.Position = newPosition;
				instantiatedRooms.Add(newRoom.Position, newRoom);
				AddChild(newRoom); //adds new room as child to MapGenerator node.
				GenerateRoom(newRoom as Room, remainingDepth);

				newRoom.Name = previousRoom.Name + " => " + newPosition;
			}
		}

		if (previousRoom.HasExitTop())
		{
			GenerateRoomDetails(_downExitRooms, new Godot.Vector2(0, -1));
		}
		
		if (previousRoom.HasExitBottom())
		{
			GenerateRoomDetails(_upExitRooms, new Godot.Vector2(0, 1));
		}
		
		if (previousRoom.HasExitRight())
		{
			GenerateRoomDetails(_leftExitRooms, new Godot.Vector2(1, 0));
		}
		
		if (previousRoom.HasExitLeft())
		{
			GenerateRoomDetails(_rightExitRooms, new Godot.Vector2(-1, 0));
		}
	}
}
