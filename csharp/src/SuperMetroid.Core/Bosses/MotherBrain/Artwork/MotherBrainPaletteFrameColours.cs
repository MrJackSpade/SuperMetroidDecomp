namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>One selected Mother Brain palette phase: its body and brain colours, its rear-leg colours, and the trailing colour a grey-transition phase carries.</summary>
/// <param name="Body">The body and brain colours.</param>
/// <param name="BackLegs">The rear-leg colours.</param>
/// <param name="TrailingColor">The colour after the rear legs, or null for a phase that carries none.</param>
internal sealed record MotherBrainPaletteFrameColours(RgbColor[] Body, RgbColor[] BackLegs, RgbColor? TrailingColor);
