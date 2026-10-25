using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Enemies.Callbacks;

/// <summary>Native Power Bomb callbacks that are a literal long return, distinct from zero/common and private handlers.</summary>
internal static class EnemyPowerBombCallbackDefinitions
{
    /// <summary>$A2:804C, RTL_A2804C: ship pieces and growing shutter.</summary>
    public static readonly SnesAddress ShipAndShutterNoOp = new(0xa2, 0x804c);
    /// <summary>$A3:804C, RTL_A3804C: elevator.</summary>
    public static readonly SnesAddress ElevatorNoOp = new(0xa3, 0x804c);
    /// <summary>$A5:804C, bank-local common RTL: Draygon eye, tail and arms.</summary>
    public static readonly SnesAddress DraygonPartsNoOp = new(0xa5, 0x804c);
    /// <summary>$A5:EDF2, Spore Spawn Power Bomb callback: literal RTL.</summary>
    public static readonly SnesAddress SporeSpawnNoOp = new(0xa5, 0xedf2);
    /// <summary>$A6:804C, bank-local common RTL: Ridley explosion actor.</summary>
    public static readonly SnesAddress RidleyExplosionNoOp = new(0xa6, 0x804c);
    /// <summary>$A7:DD9A, Phantoon Power Bomb callback: literal RTL.</summary>
    public static readonly SnesAddress PhantoonNoOp = new(0xa7, 0xdd9a);
    /// <summary>$A7:804C, bank-local common RTL: Phantoon parts, Etecoon and Dachora.</summary>
    public static readonly SnesAddress PhantoonPartsAndAnimalsNoOp = new(0xa7, 0x804c);
    /// <summary>$A8:804C, bank-local common RTL: Evir projectile and Ki-Hunter wings.</summary>
    public static readonly SnesAddress EvirAndWingsNoOp = new(0xa8, 0x804c);
    /// <summary>$AA:804C, bank-local common RTL: Tourian statue and ghost.</summary>
    public static readonly SnesAddress TourianStatuesNoOp = new(0xaa, 0x804c);
    /// <summary>$B3:804C, bank-local common RTL: escape animals.</summary>
    public static readonly SnesAddress EscapeAnimalsNoOp = new(0xb3, 0x804c);

    /// <summary>
    /// The ten literal-RTL Power Bomb callbacks in the pinned retail header inventory. Zero selects common damage, not a
    /// no-op, and other private callbacks keep their dispatcher checks.
    /// </summary>
    public static readonly IReadOnlySet<SnesAddress> LiteralNoOps = new HashSet<SnesAddress>
    {
        ShipAndShutterNoOp, ElevatorNoOp, DraygonPartsNoOp, SporeSpawnNoOp, RidleyExplosionNoOp, PhantoonNoOp,
        PhantoonPartsAndAnimalsNoOp, EvirAndWingsNoOp, TourianStatuesNoOp, EscapeAnimalsNoOp,
    };
}
