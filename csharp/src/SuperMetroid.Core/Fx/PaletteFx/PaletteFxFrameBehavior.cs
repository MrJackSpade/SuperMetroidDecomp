namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>The per-frame condition a palette effect checks before its program runs.
/// Its values are arbitrary; verification's view names each member's original identity (#1415).
/// </summary>
public enum PaletteFxFrameBehavior
{
    /// <summary>No per-frame condition.</summary>
    Null = 1,
    /// <summary>The condition left once a cue clears it.</summary>
    Cleared,
    /// <summary>The intro's per-frame condition.</summary>
    Intro,
    /// <summary>Deletes the object when the enemy in slot zero dies.</summary>
    DeleteWhenEnemyZeroDies,
    /// <summary>The Samus-in-heat condition: heat damage and the heat palette phase.</summary>
    Heat,
    /// <summary>Switches the lightning record when Samus is above the surface line.</summary>
    SwitchAboveY380,
    /// <summary>The second lightning program's switch when Samus is above the surface line.</summary>
    SwitchAboveY380Second,
    /// <summary>Deletes the object when the area's mini-boss dies.</summary>
    DeleteWhenAreaMiniBossDies,
    /// <summary>Deletes the object when a slot below its own holds an object.</summary>
    InspectAdjacentSlot,
}
