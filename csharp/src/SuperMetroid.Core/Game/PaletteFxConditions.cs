using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>The Samus, equipment, enemy and boss conditions palette-FX pre-instructions test each frame (#633).</summary>
public sealed record PaletteFxConditions
{
    /// <summary>Samus y.</summary>
    public required ushort SamusY { get; init; }

    /// <summary>Equipped items.</summary>
    public required ushort EquippedItems { get; init; }

    /// <summary>Enemy zero is dead.</summary>
    public required bool EnemyZeroIsDead { get; init; }

    /// <summary>Area mini boss defeated.</summary>
    public required bool AreaMiniBossDefeated { get; init; }

    /// <summary>Samus.</summary>
    public SamusState? Samus { get; init; } = null;

    /// <summary>Nmi frame counter.</summary>
    public ushort NmiFrameCounter { get; init; } = 0;
}
