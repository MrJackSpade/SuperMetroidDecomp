using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>
/// The Samus, equipment and mini-boss conditions the colour effects watch each update (#633, #1554); a Torizo's belly glow
/// watches its own Torizo instead (#1700).
/// </summary>
public sealed record ColourEffectInputs
{
    /// <summary>
    /// Samus's whole-pixel room height this update, which the lightning compares with
    /// <see cref="CrateriaLightningDefinitions.SamusHighOnSurfaceHeight"/>; a smaller value is higher in
    /// the room. It is a copy of her position's height, which her position holds to the room's coordinates (#1627).
    /// </summary>
    public required int SamusY { get; init; }

    /// <summary>Equipped items.</summary>
    public required SuperMetroid.Core.Progression.SamusEquipmentFlags EquippedItems { get; init; }

    /// <summary>Area mini boss defeated.</summary>
    public required bool AreaMiniBossDefeated { get; init; }

    /// <summary>Samus.</summary>
    public IRoomSamus? Samus { get; init; } = null;

    /// <summary>
    /// The long frame counter this update (<see cref="FrameClock.Counter"/>), 0 through one short of
    /// <see cref="FrameClock.CounterPeriod"/>: the heat shimmer plays its damage sound on every eighth update of it. Values
    /// the counter never holds are rejected (#1627).
    /// </summary>
    public int FrameCounter { get; init => field = Ensure.BetweenInclusive(0, FrameClock.CounterPeriod - 1, value); }
}
