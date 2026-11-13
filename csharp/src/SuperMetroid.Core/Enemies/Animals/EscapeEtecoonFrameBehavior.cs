namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The behaviour an escape Etecoon's main AI calls every frame.
/// Its values are arbitrary.
/// </summary>
public enum EscapeEtecoonFrameBehavior
{
    /// <summary>No frame behaviour: the cleared step that does nothing.</summary>
    Cleared = 1,

    /// <summary>Add 3.5 pixels to X without terrain collision.</summary>
    EscapeRight,

    /// <summary>Wait for the persistent “critters escaped” event.</summary>
    WaitForEscapeEvent,

    /// <summary>Walk, reverse at walls, and remain floor-aligned.</summary>
    WalkAndFall,
}
