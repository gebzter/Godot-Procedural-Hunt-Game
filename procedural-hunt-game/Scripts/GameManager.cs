using Godot;
using System;
using System.Threading.Tasks;

//class to organise high-level flow of the program.

public partial class GameManager : Node2D
{
	//references.
	[Export] private MapGenerator _mapGenerator;
	[Export] private PackedScene _enemyPackedScene;
	[Export] private Player _player;

	private Enemy _enemy;

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
		Node2D newEnemy = _enemyPackedScene.Instantiate<Node2D>();
		_enemy = (Enemy) newEnemy;

		//initialises references.
		_enemy.MapGenerator = _mapGenerator;
		_enemy.Player = _player;
		_enemy.AStar = _mapGenerator.AStar;

		newEnemy.Position = _enemy.GetRandomPosition(new Random()); //randomly positions enemy at a minimum distance from the starting location.

		while (newEnemy.Position.Length() < _enemy.SpawnRadius)
		{
			newEnemy.Position = _enemy.GetRandomPosition(new Random()); //randomly positions enemy at a minimum distance from the starting location.
			GD.Print("Repositioning Enemy");
		}

		GetParent().AddChild(newEnemy);

		_enemy.StartEnemyBehaviour();
	} 
}
