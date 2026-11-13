using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>The Samus, equipment, enemy and boss conditions the colour effects watch each update (#633, #1554).</summary>
public sealed record PaletteFxConditions
{
    /// <summary>
    /// Samus's whole-pixel room height this update, which the lightning compares with
    /// <see cref="CrateriaLightningDefinitions.SamusHighOnSurfaceHeight"/>; a smaller value is higher in
    /// the room. It holds 0 through 65535, and values outside that range are rejected.
    /// </summary>
    public required int SamusY { get; init => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value); }

    /// <summary>Equipped items.</summary>
    public required SuperMetroid.Core.Progression.SamusEquipmentFlags EquippedItems { get; init; }

    /// <summary>
    /// Whether the Torizo whose belly glows, the room's first enemy, has no health left; also true in a room with no
    /// enemies. Only <see cref="ColourEffectCondition.EndsWhenTorizoDies"/> reads it.
    /// </summary>
    public required bool TorizoIsDead { get; init; }

    /// <summary>Area mini boss defeated.</summary>
    public required bool AreaMiniBossDefeated { get; init; }

    /// <summary>Samus.</summary>
    public IRoomSamus? Samus { get; init; } = null;

    /// <summary>
    /// The frame counter this update, 0 through 65535: the heat shimmer plays its damage sound on every eighth update of
    /// it. Values outside the counter's range are rejected.
    /// </summary>
    public int FrameCounter { get; init => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value); }
}
