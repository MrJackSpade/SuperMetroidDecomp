using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Corpses.Artwork;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// How a dead Sidehopper lies in its room (#2558): placed already dead, drained from the start, or Baby Metroid's victim,
/// whose drain has gone as far as its stage. Only the victim has a drain.
/// </summary>
public abstract partial record SidehopperCorpseLayout
{
    /// <summary>Only the two layouts below exist.</summary>
    private protected SidehopperCorpseLayout()
    {
    }

    /// <summary>A corpse placed already dead: drained from the start, it never drains.</summary>
    public static SidehopperCorpseLayout PlacedDead { get; } = new PlacedDeadSidehopper();

    /// <summary>
    /// Whether the corpse is drained, placed already dead or turned into a corpse by Baby Metroid's feeding, so a shot, touch
    /// or Power Bomb starts it rotting.
    /// </summary>
    internal abstract bool IsDrained { get; }

    /// <summary>
    /// Whether draining has begun: Baby Metroid has fed on the corpse, or it was placed already dead. The hop's end and the
    /// post-landing pause turn to the drain once it has.
    /// </summary>
    internal abstract bool HasBegunDraining { get; }

    /// <summary>
    /// The colours the corpse shows (#1512): a corpse placed already dead lies in the drained corpses' colours; the victim
    /// shows the colours the Baby Metroid's room entry gives it until the drain's first step, then each step of the drain in
    /// turn.
    /// </summary>
    internal abstract PictureShading Shading { get; }

    /// <summary>The layout after the drain's next step.</summary>
    /// <exception cref="InvalidOperationException">The corpse was placed already dead, which never drains.</exception>
    internal abstract SidehopperCorpseLayout DrainedOneStep();

    /// <summary>A corpse placed already dead.</summary>
    private sealed partial record PlacedDeadSidehopper : SidehopperCorpseLayout
    {
        /// <inheritdoc/>
        internal override bool IsDrained => true;

        /// <inheritdoc/>
        internal override bool HasBegunDraining => true;

        /// <inheritdoc/>
        internal override PictureShading Shading => TourianCorpseShades.InDrainedColours;

        /// <inheritdoc/>
        internal override SidehopperCorpseLayout DrainedOneStep() =>
            throw new InvalidOperationException("A Sidehopper corpse placed already dead never drains.");
    }
}
