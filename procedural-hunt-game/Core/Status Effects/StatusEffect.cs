namespace Core;

using Godot;
using System;

//base class for status effect resource, derived to build a buff/debuff.
[GlobalClass] public partial class StatusEffect : Resource
{
    [Export] private double _maxDuration; //duration in seconds.
    private double _remainingDuration; //remaining duration in seconds.
    public double RemainingDuration
    {
        get { return _remainingDuration; }
        set {_remainingDuration = value; }
    }

    public bool ReduceDuration(double delta) //reduces duration of effect, returns true when expired.
    {
        _remainingDuration -= delta;

        if (RemainingDuration <= 0)
        {
            GD.Print("Effect expired");
            return true;
        }

        return false;
    }

    public void ResetDuration()
    {
        RemainingDuration = _maxDuration;
    }

    public enum EffectType
    {
        Webbed
    }
}