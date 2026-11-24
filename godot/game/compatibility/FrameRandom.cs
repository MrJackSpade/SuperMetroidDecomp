using Engine.Core;

namespace Game.Compatibility;

/// <summary>
/// Draws a random number every update of play and uses none of them, as the cartridge did every frame. Much of this
/// game reads the number last drawn without drawing one itself, and so sees a fresh number each update only because
/// of this.
/// </summary>
public sealed class FrameRandom : ISessionService
{
    /// <inheritdoc/>
    public void BeginAgain()
    {
    }

    /// <inheritdoc/>
    public void Step(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        session.Random.Next();
    }

    /// <inheritdoc/>
    public GameMode? TakesOver(PlayingMode playing) => null;
}
