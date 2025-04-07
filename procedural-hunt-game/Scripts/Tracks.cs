using Godot;
using System;
using System.Threading.Tasks;

public partial class Tracks : Node2D
{
	[Export] private int _lifeTime = 3000; //duration tracks stay before deletion in milliseconds.

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		BeginExpiration();
	}

	//deletes node after lifetime expires.
	private async void BeginExpiration()
	{
		await Task.Delay(_lifeTime);

		if (IsInstanceValid(this))
		{
			QueueFree();
		}
	}
}
