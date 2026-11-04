using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// An artwork sequence an encounter plays in the middle of another (#1513): the interlude starts once the main sequence
/// has reached <paramref name="HeldStep"/>, plays while the main sequence holds that step, and has finished before the main
/// sequence moves on. Only these joint steps can show, so a frame following both has pictures of these alone.
/// </summary>
/// <param name="Main">The encounter's main artwork sequence.</param>
/// <param name="HeldStep">The step of the main sequence the interlude plays at.</param>
/// <param name="Interlude">The sequence played while the main sequence holds that step.</param>
internal sealed record EncounterArtworkInterlude(EncounterArtworkSequence Main, int HeldStep, EncounterArtworkSequence Interlude)
{
    /// <summary>The step of the main sequence the interlude plays at.</summary>
    public int HeldStep { get; } = Ensure.AtLeastZero(HeldStep);
}
