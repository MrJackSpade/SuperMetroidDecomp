using Engine.Core;
using Engine.World;

namespace Game.Compatibility;

/// <summary>
/// Draws a random number as every update of play opens and uses none of them, as the cartridge did every frame
/// before Samus, the enemies and the room acted. Much of this game reads the number last drawn without drawing one
/// itself, and so sees a fresh number each update only because of this. Lava and acid also stir the number each time
/// they move, in view or out of it, before the update's number is drawn: the two halves of the number change places.
/// The first move of a room's liquid after the room is set up only begins its motion and is told to no one, so it
/// stirs nothing, as the cartridge's first pass does not. Lava also bubbles aloud by the number: on every 112th of
/// its moves with its surface within the room, counted from the move that began its motion, one of its three
/// bubbling sounds is heard, chosen by the number as it stands before that move stirs it.
/// </summary>
public sealed class FrameRandom : IUpdateOpener, ILiquidWatcher
{
    /// <summary>The liquids whose move stirs the number, by their types' names.</summary>
    private static readonly string[] Stirring = ["lava", "acid"];

    private readonly CartridgeRandom _sequence;

    /// <summary>The drawing and stirring of <paramref name="sequence"/>, the session's own sequence of random numbers.</summary>
    public FrameRandom(CartridgeRandom sequence)
    {
        ArgumentNullException.ThrowIfNull(sequence);
        _sequence = sequence;
    }

    /// <inheritdoc/>
    public void LiquidMoved(GameSession session, Liquid liquid)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(liquid);

        // Lava's count runs only while its surface lies within the room, and starts over with each room's lava.
        if (liquid.Type.Name == BubblingLiquid && liquid.Surface.Whole >= 0)
        {
            if (!ReferenceEquals(_bubbling, liquid))
                (_bubbling, _bubblesIn) = (liquid, BubblesEvery);
            if (--_bubblesIn == 0)
            {
                // The number's eight lowest values take the three sounds in turn.
                _bubblesIn = BubblesEvery;
                session.Sounds.Request(BubblingSounds[(int)(_sequence.Current % 8) % BubblingSounds.Length]);
            }
        }

        if (Array.IndexOf(Stirring, liquid.Type.Name) >= 0)
            _sequence.SwapHalves();
    }

    /// <summary>The liquid that bubbles aloud, by its type's name, on every how manieth of its moves, and the sounds it bubbles with, in the order the number takes them.</summary>
    private const string BubblingLiquid = "lava";
    private const int BubblesEvery = 112;
    private static readonly string[] BubblingSounds = ["enemy_lava_bubbling_burst", "enemy_lava_bubbling_falling", "enemy_lava_bubbling_rising"];

    /// <summary>The lava whose moves are being counted, and how many more of them until it next bubbles.</summary>
    private Liquid? _bubbling;
    private int _bubblesIn;

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
