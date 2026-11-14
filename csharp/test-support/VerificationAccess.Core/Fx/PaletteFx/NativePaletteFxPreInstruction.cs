namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>
/// The cartridge's palette-FX pre-instruction routines, verification's native view (#1554): each names the
/// <see cref="ColourEffectCondition"/> it stands for through <see cref="FxNativeWords.ConditionOf"/>.
/// </summary>
internal enum NativePaletteFxPreInstruction
{
    /// <summary>The routine a new object starts with, which does nothing: no condition.</summary>
    Null = 1,
    /// <summary>The routine the clear instruction installs, which does nothing: no condition.</summary>
    Cleared,
    /// <summary>The routine the intro's setup installs, which does nothing: no condition.</summary>
    Intro,
    /// <summary>Deletes the object when enemy zero dies: <see cref="ColourEffectCondition.EndsWhenTorizoDies"/>.</summary>
    DeleteWhenEnemyZeroDies,
    /// <summary>The Samus-in-heat routine: <see cref="ColourEffectCondition.BurnsSamusInHeat"/>.</summary>
    Heat,
    /// <summary>Restarts the lightning while Samus is above Y $380: the surface lightning's calm.</summary>
    SwitchAboveY380,
    /// <summary>Deletes the object when the area's mini-boss dies: <see cref="ColourEffectCondition.EndsWhenAreaMiniBossDies"/>.</summary>
    DeleteWhenAreaMiniBossDies,
}
