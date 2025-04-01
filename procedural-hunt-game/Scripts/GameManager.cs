using Godot;
using System;
using System.Threading.Tasks;

//class to organise high-level flow of the program.

public partial class GameManager : Node2D
{
	[Export] private MapGenerator _mapGenerator;
	[Export] private Enemy _enemy;

	public override void _Ready()
	{
		StartGame();
	}

	//async method to ensure that each game component is run in the correct order.
	private async void StartGame()
	{
		//room generation can be out of sync so await needed to maintain program flow.
		await _mapGenerator.GenerateMap();

		_enemy.HuntPlayer();
	}
}
