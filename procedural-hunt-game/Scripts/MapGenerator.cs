using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

//node that procedurally generates layout of map.

public partial class MapGenerator : Node2D
{
	//references
	[Export] private PackedScene[] _rooms; //packed scenes that can be used to instantiate rooms.
	private PackedScene[] _downExitRooms, _upExitRooms, _rightExitRooms, _leftExitRooms;

	[Export] private PackedScene[] _rootRooms; //potential scenes the root room could use.

	[Export] private PackedScene _testWidget; //GUI widget to mark A* nodes.



	[Export] private int _depthLimit; //max depth of each room generation branch.

	[Export] private int _roomSize = 256; //size of each square in the map grid.

	[Export] private int tileSize = 16; //size of each individual tile.

	//collection of instantiated rooms.
	private Dictionary<Godot.Vector2, Node> _instantiatedRooms = new Dictionary<Godot.Vector2, Node>();
	public Dictionary<Godot.Vector2, Node> InstantiatedRooms
	{
		get { return _instantiatedRooms; }
		set { _instantiatedRooms = value; }
	}

	//dictionary of instantiated rooms that have not yet been fully loaded in/initialised.
	//will pause execution until dictionary is empty.
	private Dictionary<Godot.Vector2, Room> _pendingRoomInstances = new Dictionary<Godot.Vector2, Room>();

	private AStar2D _aStar = new AStar2D();
	public AStar2D AStar
	{
		get { return _aStar; }
		set { _aStar = value; }
	}

	//dictionary storing all position/ID pairs of A* points.
	private Dictionary<Godot.Vector2, int> _points = new Dictionary<Godot.Vector2, int>();
	public Dictionary<Godot.Vector2, int> Points
	{
		get { return _points; }
		set { _points = value; }
	}

	public async Task GenerateMap()
	{
		SortRooms();

		//randomly selects first room.
		Node2D rootRoom;

		Random random = new Random();

		int randomIndex = random.Next(0, _rootRooms.Length);
		rootRoom = _rootRooms[randomIndex].Instantiate<Node2D>();
		_pendingRoomInstances.Add(rootRoom.Position, (Room) rootRoom);

		rootRoom.Name = "RootRoom";

		//must be cast first to use event.
		Room room = (Room) rootRoom;
		room.RoomInitialisedEvent += OnRoomInitialised; //subscribes to event.

		//adds root room as a child to MapGenerator node.
		_instantiatedRooms.Add(new Godot.Vector2(0, 0), rootRoom);
		AddChild(rootRoom);
		
		//begins recursive calls.
		GenerateRoom(rootRoom as Room, _depthLimit);

		GD.Print("Room Generation Finished");

		//waits until all rooms are finished loading.
		while (_pendingRoomInstances.Count > 0)
		{
			await Task.Delay(1);
		}

		InitialisePoints();
	}

	public void SortRooms()
	{
		//sorts rooms into respective arrays.
		_upExitRooms = new PackedScene[_rooms.Length];
		_downExitRooms = new PackedScene[_rooms.Length];
		_rightExitRooms = new PackedScene[_rooms.Length];
		_leftExitRooms = new PackedScene[_rooms.Length];

		for (int i = 0; i < _rooms.Length; i++)
		{
			Room room = _rooms[i].Instantiate() as Room;

			if (room.HasExit(Room.ExitDirection.Up))
			{
				_upExitRooms[i] = _rooms[i];
			}
			if (room.HasExit(Room.ExitDirection.Down))
			{
				_downExitRooms[i] = _rooms[i];
			}
			if (room.HasExit(Room.ExitDirection.Right))
			{
				_rightExitRooms[i] = _rooms[i];
			}
			if (room.HasExit(Room.ExitDirection.Left))
			{
				_leftExitRooms[i] = _rooms[i];
			}

			room.QueueFree();
		}
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
			Godot.Vector2 newPosition = previousRoom.Position + new Godot.Vector2(_roomSize * offset.X, _roomSize * offset.Y);
			PackedScene[][] requiredRooms = new PackedScene[4][];
			PackedScene[][] forbiddenRooms = new PackedScene[4][];

			//room present at top.
			Godot.Vector2 testPosition = newPosition + new Godot.Vector2(0, -_roomSize);
			if (_instantiatedRooms.ContainsKey(testPosition))
			{
				if (((Room) _instantiatedRooms[testPosition]).HasExit(Room.ExitDirection.Down))
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
			testPosition = newPosition + new Godot.Vector2(0, _roomSize);
			if (_instantiatedRooms.ContainsKey(testPosition))
			{
				if (((Room) _instantiatedRooms[testPosition]).HasExit(Room.ExitDirection.Up))
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
			testPosition = newPosition + new Godot.Vector2(_roomSize, 0);
			if (_instantiatedRooms.ContainsKey(testPosition))
			{
				if (((Room) _instantiatedRooms[testPosition]).HasExit(Room.ExitDirection.Left))
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
			testPosition = newPosition + new Godot.Vector2(-_roomSize, 0);
			if (_instantiatedRooms.ContainsKey(testPosition))
			{
				if (((Room) _instantiatedRooms[testPosition]).HasExit(Room.ExitDirection.Right))
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
			if (_instantiatedRooms.ContainsKey(newPosition))
			{
				newRoom.QueueFree();

				return;
			}
			else
			{
				newRoom.Position = newPosition;
				_instantiatedRooms.Add(newRoom.Position, newRoom);
				_pendingRoomInstances.Add(newRoom.Position, (Room) newRoom);
				AddChild(newRoom); //adds new room as child to MapGenerator node.

				Room room = (Room) newRoom; //casts to room script.
				room.RoomInitialisedEvent += OnRoomInitialised; //subscribes to event.

				GenerateRoom(newRoom as Room, remainingDepth);

				newRoom.Name = previousRoom.Name + " => " + newPosition;
			}
		}

		if (previousRoom.HasExit(Room.ExitDirection.Up))
		{
			GenerateRoomDetails(_downExitRooms, new Godot.Vector2(0, -1));
		}
		
		if (previousRoom.HasExit(Room.ExitDirection.Down))
		{
			GenerateRoomDetails(_upExitRooms, new Godot.Vector2(0, 1));
		}
		
		if (previousRoom.HasExit(Room.ExitDirection.Right))
		{
			GenerateRoomDetails(_leftExitRooms, new Godot.Vector2(1, 0));
		}
		
		if (previousRoom.HasExit(Room.ExitDirection.Left))
		{
			GenerateRoomDetails(_rightExitRooms, new Godot.Vector2(-1, 0));
		}
	}


	//adds A* points to A* object and connects them together.
	public void InitialisePoints()
	{
		//adds points to A* object.
		foreach (var item in InstantiatedRooms)
		{
			Godot.Vector2 roomCoord = item.Key; //global room coordinate.
			Godot.Vector2 finalCoord; //sum of global room coordinate and local tile coordinate.
			Room room = (Room) item.Value;

			foreach (var tile in room.FloorCells)
			{
				Godot.Vector2 tileCoord = tile.Key; //local tile coordinate.
				finalCoord = roomCoord + tileCoord;
				
				//generates id from global position in the form XY.
				//"1" must be added inbetween to prevent errors with palindromes. E.g. "8" + "88" == "88" + "8".
				int id = Int32.Parse(Mathf.Abs(finalCoord.X).ToString() + "1" + Mathf.Abs(finalCoord.Y).ToString());

				//IDs with negative x and y values get extra 1 digits in certain places.
				if (finalCoord.X < 0)
				{
					id = id + 1000000;
				}

				if (finalCoord.Y < 0)
				{
					id = id + 100000;
				}

				if (AStar.HasPoint(id))
				{
					Godot.Vector2 oldCoords = AStar.GetPointPosition(id);

					GD.Print("-----------------");
					GD.Print("Already contains point: ID: " + id + " X:" + finalCoord.X + " Y:" + finalCoord.Y);
					GD.Print("Current value: " + oldCoords);
					GD.Print("-----------------");
				}
				else
				{
					//GD.Print("No overwriting error");
				}

				AStar.AddPoint(id, finalCoord);
				Points.Add(finalCoord, id);
			}
		}

		GD.Print("Points generated");

		//connects points together.
		foreach (int id in AStar.GetPointIds())
		{
			Godot.Vector2 pointCoord = AStar.GetPointPosition(id);
			
			//point being checked that is adjacent to the current point.
			Godot.Vector2 checkPointCoord;
			int checkPointId;

			//iterates over every adjacent coordinate.
			for (int x = -1; x <= 1; x++)
			{
				for (int y = -1; y <= 1; y++)
				{
					if (Mathf.Abs(x) == Mathf.Abs(y))
					{
						continue;
					}

					checkPointCoord = pointCoord + new Godot.Vector2(x * tileSize, y * tileSize);

					if (Points.ContainsKey(checkPointCoord) && checkPointCoord != pointCoord)
					{
						checkPointId = Points[checkPointCoord];
					}
					else
					{
						continue;
					}

					if (AStar.HasPoint(checkPointId))
					{
						AStar.ConnectPoints(id, checkPointId, true);
					}
				}
			}
		}

		GD.Print("A* points initialised");

		//testing.
		foreach (int id in Points.Values)
		{
			//Node2D node = _testWidget.Instantiate<Node2D>();
			//GetParent().AddChild(node);
			//node.Position = _aStar.GetPointPosition(id);
		}
	}

	//gets random position on A* graph.
	public Godot.Vector2 GetRandomPosition(Random random)
	{
		long[] points = AStar.GetPointIds();

		int randomIndex = random.Next(0, points.Length);

		Godot.Vector2 randomPos = AStar.GetPointPosition(points[randomIndex]);

		return randomPos;
	}

	//event methods

	//called when RoomInitialisedEvent is fired.
	public void OnRoomInitialised(Godot.Vector2 roomPos)
	{
		_pendingRoomInstances.Remove(roomPos);
	}
}
