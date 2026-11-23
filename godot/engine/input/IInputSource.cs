namespace Engine.Input;

/// <summary>
/// Where the player's actions come from: their devices, or a recording. The <see cref="InputSampler"/> asks it once
/// per update.
/// </summary>
public interface IInputSource
{
    /// <summary>
    /// The actions down right now, together with every action that went down since the last call even if it has come
    /// up again, so a tap that begins and ends between two updates is still seen once.
    /// </summary>
    IReadOnlySet<GameAction> TakeActionsDown();
}
