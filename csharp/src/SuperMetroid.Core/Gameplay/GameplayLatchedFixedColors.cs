using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// The fixed colours the accepted frame blends with. Effects choose them during the main loop, so a colour reaches
/// the screen one frame after it is chosen, even though the same effect's window is read live during display.
/// </summary>
/// <param name="Shared">The room's fixed colour, which Fireflea darkness and X-ray blending use.</param>
/// <param name="PowerBomb">The power-bomb explosion's colour.</param>
/// <param name="SuitPickup">The suit pickup light beam's colour.</param>
/// <param name="RainbowBeam">Mother Brain's rainbow beam's colour, or null when no Mother Brain exists.</param>
public readonly record struct GameplayLatchedFixedColors(
    RgbColor Shared,
    RgbColor PowerBomb,
    RgbColor SuitPickup,
    RgbColor? RainbowBeam);
