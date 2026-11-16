namespace SuperMetroid.Core.Saves;

/// <summary>
/// What a save file keeps of the game's history as a whole, beside its slots (#2140): that the game was completed, or that
/// a power-on found no valid save. The two never hold together: completing the game replaces a power-on that found no
/// valid save, and such a power-on replaces a completed game. Saving, copying and clearing a slot leave the history as it
/// is.
/// </summary>
public enum SaveFileHistory
{
    /// <summary>Neither a completed game nor a power-on that found no valid save.</summary>
    None,

    /// <summary>The game has been completed: the title's attract demos include the completion set.</summary>
    GameCompleted,

    /// <summary>A power-on found no valid save, and the game has not been completed since.</summary>
    PowerOnFoundNoValidSave,
}
