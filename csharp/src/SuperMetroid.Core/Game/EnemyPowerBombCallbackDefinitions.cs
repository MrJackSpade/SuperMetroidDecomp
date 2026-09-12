namespace SuperMetroid.Core.Game;

/// <summary>Native Power Bomb callback classifications, distinct from zero/common and private handlers.</summary>
public static class EnemyPowerBombCallbackDefinitions
{
    /// <summary>$A2:804C, RTL_A2804C: ship pieces and growing shutter.</summary>
    public const int ShipAndShutterNoOp = 0xa2804c;
    /// <summary>$A3:804C, RTL_A3804C: elevator.</summary>
    public const int ElevatorNoOp = 0xa3804c;
    /// <summary>$A5:804C, bank-local common RTL: Draygon eye, tail and arms.</summary>
    public const int DraygonPartsNoOp = 0xa5804c;
    /// <summary>$A5:EDF2, Spore Spawn Power Bomb callback: literal RTL.</summary>
    public const int SporeSpawnNoOp = 0xa5edf2;
    /// <summary>$A6:804C, bank-local common RTL: Ridley explosion actor.</summary>
    public const int RidleyExplosionNoOp = 0xa6804c;
    /// <summary>$A7:DD9A, Phantoon Power Bomb callback: literal RTL.</summary>
    public const int PhantoonNoOp = 0xa7dd9a;
    /// <summary>$A7:804C, bank-local common RTL: Phantoon parts, Etecoon and Dachora.</summary>
    public const int PhantoonPartsAndAnimalsNoOp = 0xa7804c;
    /// <summary>$A8:804C, bank-local common RTL: Evir projectile and Ki-Hunter wings.</summary>
    public const int EvirAndWingsNoOp = 0xa8804c;
    /// <summary>$AA:804C, bank-local common RTL: Tourian statue and ghost.</summary>
    public const int TourianStatuesNoOp = 0xaa804c;
    /// <summary>$B3:804C, bank-local common RTL: escape animals.</summary>
    public const int EscapeAnimalsNoOp = 0xb3804c;

    /// <summary>Zero selects common damage, not a no-op. Other private callbacks retain their dispatcher checks.</summary>
    public static bool IsLiteralNoOp(byte bank, ushort pointer) => ((bank << 16) | pointer) is
        ShipAndShutterNoOp or ElevatorNoOp or DraygonPartsNoOp or SporeSpawnNoOp or
        RidleyExplosionNoOp or PhantoonNoOp or PhantoonPartsAndAnimalsNoOp or
        EvirAndWingsNoOp or TourianStatuesNoOp or EscapeAnimalsNoOp;
}
