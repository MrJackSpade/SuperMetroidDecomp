using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>What the shown rainbow beam looks like: the colour it adds across its cone, one value (#2498).</summary>
/// <param name="Color">The colour the beam adds inside its cone, as the frame boundary latched it.</param>
/// <param name="Cone">The cone from Mother Brain's mouth the colour is added across.</param>
public readonly record struct MotherBrainRainbowBeamRenderSnapshot(
    RgbColor Color,
    MotherBrainRainbowBeamGeometry Cone);
