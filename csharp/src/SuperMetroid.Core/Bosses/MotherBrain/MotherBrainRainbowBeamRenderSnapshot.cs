using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Value-only render input for the rainbow beam's windowed fixed-color addition.
/// </summary>
/// <param name="Active">Whether the beam's color-add layer is enabled; an inactive beam draws nothing.</param>
/// <param name="Color">Current fixed backdrop color for the windowed addition.</param>
/// <param name="Geometry">Last non-retain geometry; null until the first build, and retained across deactivation.</param>
public readonly record struct MotherBrainRainbowBeamRenderSnapshot(
    bool Active,
    RgbColor Color,
    MotherBrainRainbowBeamGeometry? Geometry);
