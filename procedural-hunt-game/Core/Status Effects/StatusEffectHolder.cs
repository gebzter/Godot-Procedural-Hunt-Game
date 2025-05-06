namespace Core;

using Godot;
using System;
using System.Collections.Generic;

//class to hold and manage status effects of parent node.

public partial class StatusEffectHolder : Node
{
    //references to custom resources of each type of status effect.
    [Export] protected Webbed _webbedEffect;
    protected List<StatusEffect> _statusEffects = new List<StatusEffect>();

    private float _speedMultiplier = 1;
    public float SpeedMultiplier
    {
        get { return _speedMultiplier; }
        set { _speedMultiplier = value; }
    }

    public override void _PhysicsProcess(double delta)
    {
        ReduceEffectDurations(delta);
        EffectBehaviours();
    }

    private void ReduceEffectDurations(double delta)
	{
		if (_statusEffects.Count == 0)
		{
			return;
		}

        for (int i = 0; i < _statusEffects.Count; i++)
        {
            StatusEffect effect = _statusEffects[i];
			bool hasExpired = effect.ReduceDuration(delta);

			if (hasExpired)
			{
				RemoveEffect(effect);
			}
        }
	}

    protected virtual void EffectBehaviours()
    {
        SpeedMultiplier = 1;

        if (_statusEffects.Contains(_webbedEffect))
        {
            SpeedMultiplier = 1 - _webbedEffect.SlowFactor;
        }
    }

    public void AddEffect(StatusEffect.EffectType effect)
    {
        StatusEffect newEffect;

        switch (effect)
        {
            case StatusEffect.EffectType.Webbed:
                newEffect = _webbedEffect;
                break;
            default:
                return;
        }

        if (_statusEffects.Contains(newEffect))
        {
            _statusEffects.Remove(newEffect);
        }

        newEffect.ResetDuration();

        _statusEffects.Add(newEffect);
        GD.Print(newEffect.RemainingDuration);
    }

    public void RemoveEffect(StatusEffect effect)
    {
        _statusEffects.Remove(effect);
    }

}
