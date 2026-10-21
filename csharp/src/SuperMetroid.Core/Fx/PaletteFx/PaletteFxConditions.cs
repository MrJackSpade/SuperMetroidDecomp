using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>The Samus, equipment, enemy and boss conditions palette-FX pre-instructions test each frame (#633).</summary>
public sealed record PaletteFxConditions
{
    /// <summary>
    /// Samus's whole-pixel room Y this frame, the value the lightning pre-instructions compare with
    /// <see cref="CrateriaLightningPaletteFxColorDefinitions.VerticalSwitchSamusY"/>. It is a read-only copy of
    /// the position's whole word, so it holds 0 through 65535 and is compared as an ordinary non-negative number,
    /// which is the cartridge's unsigned comparison; values outside that range are rejected.
    /// </summary>
    public required int SamusY { get; init => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value); }

    /// <summary>Equipped items.</summary>
    public required SuperMetroid.Core.Progression.SamusEquipmentFlags EquippedItems { get; init; }

    /// <summary>Enemy zero is dead.</summary>
    public required bool EnemyZeroIsDead { get; init; }

    /// <summary>Area mini boss defeated.</summary>
    public required bool AreaMiniBossDefeated { get; init; }

    /// <summary>Samus.</summary>
    public SamusState? Samus { get; init; } = null;

    /// <summary>
    /// The frame counter this frame, 0 through 65535: the heat pre-instruction plays its damage sound on every
    /// eighth frame of it. Values outside the counter's range are rejected.
    /// </summary>
    public int NmiFrameCounter { get; init => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value); }
}
