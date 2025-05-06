namespace Core;

using Godot;
using System;

//effect for being webbed, slows down target.
[GlobalClass] public partial class Webbed : StatusEffect
{
    [Export] private float _slowFactor;
    public float SlowFactor 
    {
        get { return _slowFactor; } 
    }
}
