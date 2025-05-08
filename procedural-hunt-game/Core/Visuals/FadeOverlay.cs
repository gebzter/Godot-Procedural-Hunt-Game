using Godot;
using System;

//script that fades an overlay in from black to transparency.
public partial class FadeOverlay : Polygon2D
{
    [Export] private Polygon2D _polygon;
    [Export] private float _fadeDuration; //time taken to fully fade.

    private float _alphaDecrement = 1f; //amount that the alpha channel of the Polygon2D decrements by.

    public override void _Ready()
    {
        _alphaDecrement = 1 / _fadeDuration;
    }

    public override void _PhysicsProcess(double delta)
    {
        _polygon.Color = new Color(_polygon.Color.R, _polygon.Color.G, _polygon.Color.B, _polygon.Color.A - (_alphaDecrement * (float) delta));
    }
}
