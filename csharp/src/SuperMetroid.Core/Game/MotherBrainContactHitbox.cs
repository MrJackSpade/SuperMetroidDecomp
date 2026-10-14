namespace SuperMetroid.Core.Game;

/// <summary>
/// One asymmetric Mother Brain contact rectangle, expressed as signed distances from its
/// component origin exactly as stored by the cartridge.
/// </summary>
internal readonly record struct MotherBrainContactHitbox(
    short Left,
    short Top,
    short Right,
    short Bottom);
