using SuperMetroid.Core;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Diagnostics.Saves;

/// <summary>
/// The numbers the cartridge's save and work memory keep for a slot's HUD selection and reserve-tank mode: the HUD
/// selection by its place in the selection ring, the reserve mode as none, AUTO or MANUAL counted from zero. The
/// cartridge save codec (<see cref="SaveRamCodec"/>), the upgrade of schema-six JSON saves
/// (<see cref="LegacyGameSaveJson"/>) and the native-state checks read and write these numbers through it; the game
/// itself holds only <see cref="HudItem"/> and <see cref="ReserveTankMode"/>.
/// </summary>
[EncodingOwner("save slot selection numbers")]
public static class SaveSlotNumbering
{
    /// <summary>The HUD selection a saved number keeps; any other number is corrupt.</summary>
    /// <exception cref="InvalidDataException"><paramref name="number"/> names no HUD selection.</exception>
    public static HudItem SavedHudItem(int number) => number switch
    {
        0 => HudItem.Beam,
        1 => HudItem.Missile,
        2 => HudItem.SuperMissile,
        3 => HudItem.PowerBomb,
        4 => HudItem.Grapple,
        5 => HudItem.XRay,
        _ => throw new InvalidDataException($"HUD selection number {number} names none of the six selections."),
    };

    /// <summary>The number a save keeps for <paramref name="item"/>: its place in the selection ring.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="item"/> is not a defined selection.</exception>
    public static int SavedNumber(this HudItem item) => item switch
    {
        HudItem.Beam => 0,
        HudItem.Missile => 1,
        HudItem.SuperMissile => 2,
        HudItem.PowerBomb => 3,
        HudItem.Grapple => 4,
        HudItem.XRay => 5,
        _ => throw new ArgumentOutOfRangeException(nameof(item), item, "Undefined HUD item."),
    };

    /// <summary>The reserve-tank mode a saved number keeps; any other number is corrupt.</summary>
    /// <exception cref="InvalidDataException"><paramref name="number"/> is not none, AUTO or MANUAL.</exception>
    public static ReserveTankMode SavedReserveMode(int number) => number switch
    {
        0 => ReserveTankMode.None,
        1 => ReserveTankMode.Auto,
        2 => ReserveTankMode.Manual,
        _ => throw new InvalidDataException($"Reserve-tank mode number {number} is not none, AUTO or MANUAL."),
    };

    /// <summary>The number a save keeps for <paramref name="mode"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="mode"/> is not a defined mode.</exception>
    public static int SavedNumber(this ReserveTankMode mode) => mode switch
    {
        ReserveTankMode.None => 0,
        ReserveTankMode.Auto => 1,
        ReserveTankMode.Manual => 2,
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Undefined reserve-tank mode."),
    };
}
