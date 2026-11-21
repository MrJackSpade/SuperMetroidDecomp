using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>One stage of Mother Brain's death body fade as installed: her body's colours and her rear legs' together (#2565).</summary>
/// <param name="Body">Her body and brain's colours.</param>
/// <param name="Legs">Her rear legs' colours.</param>
internal sealed record MotherBrainDeathFadeStage(RgbColor[] Body, RgbColor[] Legs);
