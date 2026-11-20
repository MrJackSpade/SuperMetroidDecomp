using SuperMetroid.Core.Enemies.Corpses.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// How far a dead Sidehopper has been drained (#2369, #2554). The Baby Metroid owns the drain of its Tourian victim: it
/// makes it at its own setup and latches it on when it feeds, and the victim, set up after it, holds the same drain from
/// its own setup and steps it as it drains. A corpse placed already dead holds a drain of its own that lies drained from
/// the start, in the drained corpses' colours.
/// </summary>
public sealed class SidehopperDrain
{
    /// <summary>Whether the corpse was placed already dead, which shows the drained corpses' colours.</summary>
    private readonly bool placedDead;

    private SidehopperDrain(SidehopperDrainStage stage, bool placedDead)
    {
        Stage = stage;
        this.placedDead = placedDead;
    }

    /// <summary>The drain a Baby Metroid makes for its victim: untouched until the Baby latches on.</summary>
    internal static SidehopperDrain ForVictim() => new(SidehopperDrainStage.Untouched, placedDead: false);

    /// <summary>The drain of a corpse placed already dead: drained from the start.</summary>
    internal static SidehopperDrain PlacedDead() => new(SidehopperDrainStage.Drained, placedDead: true);

    /// <summary>How far the drain has gone.</summary>
    public SidehopperDrainStage Stage { get; private set; }

    /// <summary>Whether the corpse is drained, so a shot, touch or Power Bomb starts it rotting.</summary>
    internal bool IsDrained => Stage.IsDrained;

    /// <summary>Whether the drain has begun: the Baby Metroid has latched on, or the corpse was placed already dead.</summary>
    internal bool HasBegun => Stage.HasBegun;

    /// <summary>Latches the Baby Metroid on to feed: the drain's first stage.</summary>
    internal void LatchOn() => Stage = SidehopperDrainStage.LatchedOn;

    /// <summary>Steps the drain on to its next stage.</summary>
    /// <exception cref="InvalidOperationException">The corpse is already drained.</exception>
    internal void Step() => Stage = Stage.Next;

    /// <summary>
    /// The colours the corpse shows, from how far its drain has gone (#1512): a corpse placed already dead lies in the
    /// drained corpses' colours; the victim shows the colours the Baby Metroid's room entry gives it until the drain's first
    /// shaded stage, then each stage of the drain in turn.
    /// </summary>
    internal PictureShading Shading => placedDead ? TourianCorpseShades.InDrainedColours
        : !Stage.IsShaded ? TourianCorpseShades.InBabyMetroidTargetColours
        : PictureShading.Shaded(TourianCorpseShades.SidehopperDrain(Stage));
}
