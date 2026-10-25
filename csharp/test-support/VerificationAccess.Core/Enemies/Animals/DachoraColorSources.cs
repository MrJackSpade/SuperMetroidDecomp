using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>Verification's view of where the cartridge keeps Dachora's colour images and the selectors naming them.</summary>
internal static class DachoraColorSources
{
    /// <summary>Four native shine-image pointers at $A7:F92D.</summary>
    internal const int ShinePointerTable = 0xa7f92d;

    /// <summary>Four native speed-image pointers at $A7:F787.</summary>
    internal const int SpeedPointerTable = 0xa7f787;

    /// <summary>The cartridge location of the colour image for a phase and animation frame.</summary>
    internal static int Source(DachoraPalettePhase phase, int frame) => phase switch
    {
        DachoraPalettePhase.Default when frame == 0 => EnemySpeciesSourceDefinitions.Dachora.DefaultColors,
        DachoraPalettePhase.Speed when (uint)frame < DachoraColorDefinitions.AnimatedFrameCount =>
            EnemySpeciesSourceDefinitions.Dachora.SpeedColors + frame * EnemySpeciesSourceDefinitions.Dachora.ImageBytes,
        DachoraPalettePhase.Shine when (uint)frame < DachoraColorDefinitions.AnimatedFrameCount =>
            EnemySpeciesSourceDefinitions.Dachora.ShineColors + frame * EnemySpeciesSourceDefinitions.Dachora.ImageBytes,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), $"Dachora phase {phase} has no frame {frame}."),
    };
}
