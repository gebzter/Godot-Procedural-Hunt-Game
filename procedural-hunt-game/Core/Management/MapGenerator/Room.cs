namespace Core.Management.MapGeneration;

using Godot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

//class to hold data for each room node.

public partial class Room : Node2D
{
	//references
	[Export] private TileMapLayer _tileMapLayer;

	[Export] private bool _hasExitTop, _hasExitBottom, _hasExitLeft, _hasExitRight;
	
	[Export] private String _name; //name of room type.
	[Export] private int _tileSize; //width of each individual tile.



	private Dictionary<Vector2I, TileData> _floorCells = new Dictionary<Vector2I, TileData>(); //non-solid floor cells that can be walked on.
	public Dictionary<Vector2I, TileData> FloorCells
	{
		get { return _floorCells; }
		set { _floorCells = value; }
	}

    public event Action<Vector2> RoomInitialisedEvent; //event for notifying when the room is completely initialised. Useful for coordinating timing.

    public void RoomInitialised() //function used whenever action is fired.
    {
        if (RoomInitialisedEvent != null) //checks that there are methods subscribed to event.
        {
            RoomInitialisedEvent(Position); //fires event.
        }
    }



	public override void _Ready()
	{
		foreach (Vector2I cellCoord in _tileMapLayer.GetUsedCells())
		{
			TileData tile = _tileMapLayer.GetCellTileData(cellCoord);

			TerrainType cusData =  (TerrainType) (int) tile.GetCustomData("Terrain");

			if (cusData == TerrainType.Floor)
			{
				FloorCells.Add(new Vector2I(cellCoord.X * _tileSize, cellCoord.Y * _tileSize), tile);
			}
		}

		RoomInitialised();
	}


	//accessor
	public bool HasExit(ExitDirection direction)
	{
		bool hasExit = false;

		switch (direction)
		{
			case ExitDirection.Up:
				hasExit = _hasExitTop;
				break;
			case ExitDirection.Down:
				hasExit = _hasExitBottom;
				break;
			case ExitDirection.Left:
				hasExit = _hasExitLeft;
				break;
			case ExitDirection.Right:
				hasExit = _hasExitRight;
				break;
		}

		return hasExit;
	}

	//enums
	public enum TerrainType
	{
		Wall = 0,
		Floor = 1
	}

	public enum ExitDirection //makes HasExit() function more readable.
	{
		Up = 0,
		Down = 1,
		Left = 2,
		Right = 3
	}
}
