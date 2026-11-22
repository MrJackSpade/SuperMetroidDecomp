namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>What a Crystal Flash asks of Samus and what it gives her back.</summary>
internal static partial class CrystalFlashDefinitions
{
    /// <summary>Samus's energy must be below this for the flash to start.</summary>
    public const int AdmittingEnergyBelow = 51;

    /// <summary>
    /// The Missiles, Super Missiles and Power Bombs the flash spends of each kind, one at each drain: each count must
    /// hold at least this many for the flash to start.
    /// </summary>
    public const int AmmunitionPerKind = 10;

    /// <summary>The energy each spent round of ammunition restores.</summary>
    public const int EnergyPerAmmunition = 50;

    /// <summary>The updates the flash raises Samus for before it starts spending ammunition.</summary>
    public const int RaiseUpdates = 10;

    /// <summary>The whole pixels the flash raises Samus by on each raising update; her position's fraction of a pixel is kept.</summary>
    public const int RaiseStep = 2;

    /// <summary>
    /// The updates the first picture of the glow, when the raise ends, and of the finish, when the last ammunition is
    /// spent, shows for: the flash sets each picture directly rather than waiting for its animation to reach it.
    /// </summary>
    public const int PhasePictureHold = 3;


    /// <summary>How far Samus's whole vertical position steps down each update it differs from the raised height: one pixel.</summary>
    internal static Offset16<RoomPixels> SettleStep { get; } = new(0, 1);
}
