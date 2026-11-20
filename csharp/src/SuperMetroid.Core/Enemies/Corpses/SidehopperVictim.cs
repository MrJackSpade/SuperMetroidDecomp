using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Corpses.Artwork;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>Baby Metroid's Sidehopper victim (#2369, #2558), whose drain has gone as far as <paramref name="Stage"/>.</summary>
/// <param name="Stage">How far Baby Metroid's drain has gone.</param>
public sealed record SidehopperVictim(SidehopperDrainStage Stage) : SidehopperCorpseLayout
{
    /// <inheritdoc/>
    internal override bool IsDrained => Stage.IsDrained;

    /// <inheritdoc/>
    internal override bool HasBegunDraining => Stage.HasBegun;

    /// <inheritdoc/>
    internal override PictureShading Shading => Stage.IsShaded
        ? PictureShading.Shaded(TourianCorpseShades.SidehopperDrain(Stage))
        : TourianCorpseShades.InBabyMetroidTargetColours;

    /// <inheritdoc/>
    internal override SidehopperCorpseLayout DrainedOneStep() => new SidehopperVictim(Stage.Next);
}
