namespace Core.Management.GameManager;

using Godot;
using System;
using System.Threading.Tasks;

using Core.Management.MapGeneration;
using Core.Player;
using Enemy.Latcher;

//class to organise high-level flow of the program.

public partial class GameManager : Node2D
{
	//references.
	[Export] private MapGenerator _mapGenerator;
	[Export] private PackedScene _latcherPackedScene;
	[Export] private Player _player;

	private Latcher _latcher;

	public override void _Ready()
	{
		StartGame();
	}

	//async method to ensure that each game component is run in the correct order.
	private async void StartGame()
	{
		//room generation can be out of sync so await needed to maintain program flow.
		await _mapGenerator.GenerateMap();

		await InstantiateEnemy();
	}

	//generates enemy and puts it in a random position.
	private async Task InstantiateEnemy()
	{
		Node2D newLatcher = _latcherPackedScene.Instantiate<Node2D>();
		_latcher = (Latcher) newLatcher;

		//initialises references.
		_latcher.MapGenerator = _mapGenerator;
		_latcher.Player = _player;

		newLatcher.Position = _mapGenerator.GetRandomPosition(new Random()); //randomly positions enemy at a minimum distance from the starting location.

		while (newLatcher.Position.Length() < _latcher.SpawnRadius)
		{
			newLatcher.Position = _mapGenerator.GetRandomPosition(new Random()); //randomly positions enemy at a minimum distance from the starting location.
			GD.Print("Repositioning Enemy");
		}

		GetTree().CurrentScene.AddChild(newLatcher);

		_latcher.StartEnemyBehaviour();
	} 
}
