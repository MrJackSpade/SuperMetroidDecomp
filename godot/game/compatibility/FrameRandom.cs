using Engine.Core;

namespace Game.Compatibility;

/// <summary>
/// Draws a random number as every update of play opens and uses none of them, as the cartridge did every frame
/// before Samus, the enemies and the room acted. Much of this game reads the number last drawn without drawing one
/// itself, and so sees a fresh number each update only because of this.
/// </summary>
public sealed class FrameRandom : IUpdateOpener
{
    /// <inheritdoc/>
    public void BeginAgain()
    {
    }

    /// <inheritdoc/>
    public void OpenUpdate(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        session.Random.Next();
    }

    /// <summary>The update's number is drawn as it opens; nothing is left for after the room.</summary>
    public void Step(GameSession session)
    {
    }

    /// <inheritdoc/>
    public GameMode? TakesOver(PlayingMode playing) => null;
}
