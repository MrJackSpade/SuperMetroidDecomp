using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Samus as room objects read and change her (#1395): her position and extent, energy and ammunition, equipment, the
/// controls stations lock, and the few questions room objects ask about her motion. Samus implements it.
/// </summary>
public interface IRoomSamus
{
    /// <summary>Samus's position in the room.</summary>
    FixedPosition<RoomPixels> Position { get; set; }

    /// <summary>Samus's collision half-extents.</summary>
    Extent16<RoomPixels> Radius { get; }

    /// <summary>Current energy.</summary>
    int Health { get; set; }

    /// <summary>Energy capacity.</summary>
    int MaxHealth { get; set; }

    /// <summary>Reserve-tank capacity.</summary>
    int MaxReserveEnergy { get; set; }

    /// <summary>Whether reserve energy refills automatically or by hand.</summary>
    ReserveTankMode ReserveTankMode { get; set; }

    /// <summary>Missiles carried.</summary>
    int Missiles { get; set; }

    /// <summary>Missile capacity.</summary>
    int MaxMissiles { get; set; }

    /// <summary>Super Missiles carried.</summary>
    int SuperMissiles { get; set; }

    /// <summary>Super Missile capacity.</summary>
    int MaxSuperMissiles { get; set; }

    /// <summary>Power Bombs carried.</summary>
    int PowerBombs { get; set; }

    /// <summary>Power Bomb capacity.</summary>
    int MaxPowerBombs { get; set; }

    /// <summary>Beams switched on.</summary>
    SamusBeamFlags EquippedBeams { get; set; }

    /// <summary>Beams collected.</summary>
    SamusBeamFlags CollectedBeams { get; set; }

    /// <summary>Items switched on.</summary>
    SamusEquipmentFlags EquippedItems { get; set; }

    /// <summary>Items collected.</summary>
    SamusEquipmentFlags CollectedItems { get; set; }

    /// <summary>The charge flare's frame counter, which an item pickup clears.</summary>
    int ProjectileFlareCounter { get; set; }

    /// <summary>The remaining invincibility frames.</summary>
    NativeCountdown InvincibilityTimer { get; set; }

    /// <summary>The controls stations lock and release.</summary>
    IRoomSamusControl Control { get; }

    /// <summary>Whether Samus is in a shinespark pose.</summary>
    bool IsShinesparking { get; }

    /// <summary>Whether Samus is running at speed-boosting speed.</summary>
    bool IsActivelySpeedBoosting { get; }

    /// <summary>Turns Samus to face forward, as a save station centres her.</summary>
    void FaceForward();

    /// <summary>Takes the room FX's liquid into Samus's liquid physics, as the broken n00b tube floods the room.</summary>
    void ConfigureLiquidFor(RoomLayer3FxState fx);

    /// <summary>Takes <paramref name="damage"/> through the periodic-damage path, as a chewing plant does.</summary>
    void TakePeriodicDamage(ushort damage);
}
