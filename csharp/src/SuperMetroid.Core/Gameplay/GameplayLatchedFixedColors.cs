namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// COLDATA values an accepted NMI uploaded (<c>$80:91EE</c>). The HDMA pre-instructions write
/// these shadows during the main loop, so the color reaches the screen one frame after it is
/// written even though the same effect's HDMA window table is read live during display.
/// </summary>
/// <param name="Shared">The WRAM COLDATA shadows used by Fireflea darkness and X-ray blending.</param>
/// <param name="PowerBomb">The power-bomb explosion's COLDATA color.</param>
/// <param name="SuitPickup">The suit pickup light beam's COLDATA color.</param>
/// <param name="RainbowBeam">Mother Brain's rainbow-beam fixed color, or null when no Mother Brain exists.</param>
public readonly record struct GameplayLatchedFixedColors(
    FixedColorRegisterSnapshot Shared,
    FixedColorRegisterSnapshot PowerBomb,
    FixedColorRegisterSnapshot SuitPickup,
    Bgr555? RainbowBeam);
