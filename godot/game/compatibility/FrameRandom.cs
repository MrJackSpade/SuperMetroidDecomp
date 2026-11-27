using Engine.Core;
using Engine.World;

namespace Game.Compatibility;

/// <summary>
/// Draws a random number as every update of play opens and uses none of them, as the cartridge did every frame
/// before Samus, the enemies and the room acted. Much of this game reads the number last drawn without drawing one
/// itself, and so sees a fresh number each update only because of this. Lava and acid also stir the number each time
/// they move, in view or out of it, before the update's number is drawn: the two halves of the number change places.
/// The first move of a room's liquid after the room is set up only starts the cartridge's liquid, and stirs nothing.
/// </summary>
public sealed class FrameRandom : IUpdateOpener, ILiquidWatcher
{
    /// <summary>The liquids whose move stirs the number, by their types' names.</summary>
    private static readonly string[] Stirring = ["lava", "acid"];

    private readonly CartridgeRandom _sequence;

    /// <summary>The liquid that moved last; a room, and its liquid with it, is made afresh each time it is set up.</summary>
    private Liquid? _moved;

    /// <summary>The drawing and stirring of <paramref name="sequence"/>, the session's own sequence of random numbers.</summary>
    public FrameRandom(CartridgeRandom sequence)
    {
        ArgumentNullException.ThrowIfNull(sequence);
        _sequence = sequence;
    }

    /// <inheritdoc/>
    public void LiquidMoved(GameSession session, Liquid liquid)
    {
        ArgumentNullException.ThrowIfNull(liquid);
        bool first = !ReferenceEquals(liquid, _moved);
        _moved = liquid;
        if (!first && Array.IndexOf(Stirring, liquid.Type.Name) >= 0)
            _sequence.SwapHalves();
    }

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
    public bool WantsToTakeOver => false;

    /// <inheritdoc/>
    public GameMode? TakesOver(PlayingMode playing) => null;
}
