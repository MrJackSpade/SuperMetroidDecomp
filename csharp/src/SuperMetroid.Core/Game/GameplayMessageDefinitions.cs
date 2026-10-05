namespace SuperMetroid.Core.Game;

/// <summary>Native message-ID dispatch to setup, drawing and independently owned presentation resources.</summary>
internal static class GameplayMessageDefinitions
{
    /// <summary>$85:869B..8748 contains 29 six-byte records, including both content-boundary terminators.</summary>
    public const int NativeDefinitionCount = 29;
    /// <summary>$85:8737, record 1B: boundary used to size the preceding Gravity Suit content.</summary>
    private const int ContentBoundaryIndex = 27;
    /// <summary>$85:8743, record 1D: boundary used to size the preceding gunship save content.</summary>
    private const int GunshipContentBoundaryIndex = 29;

    private enum Content : ushort
    {
        /// <summary>$85:877F, native MessageTilemaps_energyTank; presentation payload identity only.</summary>
        EnergyTank = 0x877f,
        /// <summary>$85:87BF, native MessageTilemaps_missile; presentation payload identity only.</summary>
        MissileTank = 0x87bf,
        /// <summary>$85:88BF, native MessageTilemaps_superMissile; presentation payload identity only.</summary>
        SuperMissileTank = 0x88bf,
        /// <summary>$85:89BF, native MessageTilemaps_powerBomb; presentation payload identity only.</summary>
        PowerBombTank = 0x89bf,
        /// <summary>$85:8ABF, native MessageTilemaps_grapplingBeam; presentation payload identity only.</summary>
        GrappleBeam = 0x8abf,
        /// <summary>$85:8BBF, native MessageTilemaps_xrayScope; presentation payload identity only.</summary>
        XrayScope = 0x8bbf,
        /// <summary>$85:8CBF, native MessageTilemaps_variaSuit; presentation payload identity only.</summary>
        VariaSuit = 0x8cbf,
        /// <summary>$85:8CFF, native MessageTilemaps_springBall; presentation payload identity only.</summary>
        SpringBall = 0x8cff,
        /// <summary>$85:8D3F, native MessageTilemaps_morphingBall; presentation payload identity only.</summary>
        MorphBall = 0x8d3f,
        /// <summary>$85:8D7F, native MessageTilemaps_screwAttack; presentation payload identity only.</summary>
        ScrewAttack = 0x8d7f,
        /// <summary>$85:8DBF, native MessageTilemaps_hiJumpBoots; presentation payload identity only.</summary>
        HiJumpBoots = 0x8dbf,
        /// <summary>$85:8DFF, native MessageTilemaps_spaceJump; presentation payload identity only.</summary>
        SpaceJump = 0x8dff,
        /// <summary>$85:8E3F, native MessageTilemaps_speedBooster; presentation payload identity only.</summary>
        SpeedBooster = 0x8e3f,
        /// <summary>$85:8F3F, native MessageTilemaps_chargeBeam; presentation payload identity only.</summary>
        ChargeBeam = 0x8f3f,
        /// <summary>$85:8F7F, native MessageTilemaps_iceBeam; presentation payload identity only.</summary>
        IceBeam = 0x8f7f,
        /// <summary>$85:8FBF, native MessageTilemaps_waveBeam; presentation payload identity only.</summary>
        WaveBeam = 0x8fbf,
        /// <summary>$85:8FFF, native MessageTilemaps_spazer; presentation payload identity only.</summary>
        SpazerBeam = 0x8fff,
        /// <summary>$85:903F, native MessageTilemaps_plasmaBeam; presentation payload identity only.</summary>
        PlasmaBeam = 0x903f,
        /// <summary>$85:907F, native MessageTilemaps_bomb; presentation payload identity only.</summary>
        Bombs = 0x907f,
        /// <summary>$85:917F, native MessageTilemaps_map; presentation payload identity only.</summary>
        MapDataAccessCompleted = 0x917f,
        /// <summary>$85:923F, native MessageTilemaps_energyRecharge; presentation payload identity only.</summary>
        EnergyRechargeCompleted = 0x923f,
        /// <summary>$85:92FF, native MessageTilemaps_missileReload; presentation payload identity only.</summary>
        MissileRechargeCompleted = 0x92ff,
        /// <summary>$85:93BF, native MessageTilemaps_save; presentation payload identity only.</summary>
        SaveConfirmation = 0x93bf,
        /// <summary>$85:94BF, native MessageTilemaps_saveCompleted; presentation payload identity only.</summary>
        SaveCompleted = 0x94bf,
        /// <summary>$85:94FF, native MessageTilemaps_reserveTank; presentation payload identity only.</summary>
        ReserveTank = 0x94ff,
        /// <summary>$85:953F, native MessageTilemaps_gravitySuit; presentation payload identity only.</summary>
        GravitySuit = 0x953f,
        /// <summary>$85:957F, native MessageTilemaps_Terminator; presentation payload identity only.</summary>
        ContentBoundary = 0x957f,
    }

    /// <summary>Returns the native one-based definition, preserving both content-boundary records.</summary>
    public static GameplayMessageDefinition AtNativeIndex(int oneBasedIndex)
    {
        if (oneBasedIndex is < 1 or > NativeDefinitionCount)
            throw new ArgumentOutOfRangeException(nameof(oneBasedIndex));
        if (oneBasedIndex == ContentBoundaryIndex)
            return Small((ushort)Content.ContentBoundary);
        if (oneBasedIndex == GunshipContentBoundaryIndex)
            return Small((ushort)Content.SaveCompleted);
        return (GameplayMessageId)oneBasedIndex switch
        {
            GameplayMessageId.EnergyTank => Small((ushort)Content.EnergyTank),
            GameplayMessageId.MissileTank => Shoot((ushort)Content.MissileTank),
            GameplayMessageId.SuperMissileTank => Shoot((ushort)Content.SuperMissileTank),
            GameplayMessageId.PowerBombTank => Shoot((ushort)Content.PowerBombTank),
            GameplayMessageId.GrappleBeam => Shoot((ushort)Content.GrappleBeam),
            GameplayMessageId.XrayScope => Run((ushort)Content.XrayScope),
            GameplayMessageId.VariaSuit => Small((ushort)Content.VariaSuit),
            GameplayMessageId.SpringBall => Small((ushort)Content.SpringBall),
            GameplayMessageId.MorphBall => Small((ushort)Content.MorphBall),
            GameplayMessageId.ScrewAttack => Small((ushort)Content.ScrewAttack),
            GameplayMessageId.HiJumpBoots => Small((ushort)Content.HiJumpBoots),
            GameplayMessageId.SpaceJump => Small((ushort)Content.SpaceJump),
            GameplayMessageId.SpeedBooster => Run((ushort)Content.SpeedBooster),
            GameplayMessageId.ChargeBeam => Small((ushort)Content.ChargeBeam),
            GameplayMessageId.IceBeam => Small((ushort)Content.IceBeam),
            GameplayMessageId.WaveBeam => Small((ushort)Content.WaveBeam),
            GameplayMessageId.SpazerBeam => Small((ushort)Content.SpazerBeam),
            GameplayMessageId.PlasmaBeam => Small((ushort)Content.PlasmaBeam),
            GameplayMessageId.Bombs => Shoot((ushort)Content.Bombs),
            GameplayMessageId.MapDataAccessCompleted => Small((ushort)Content.MapDataAccessCompleted),
            GameplayMessageId.EnergyRechargeCompleted => Small((ushort)Content.EnergyRechargeCompleted),
            GameplayMessageId.MissileRechargeCompleted => Small((ushort)Content.MissileRechargeCompleted),
            GameplayMessageId.SaveConfirmation => LargeSetupSmallDraw((ushort)Content.SaveConfirmation),
            GameplayMessageId.SaveCompleted => Small((ushort)Content.SaveCompleted),
            GameplayMessageId.ReserveTank => Small((ushort)Content.ReserveTank),
            GameplayMessageId.GravitySuit => Small((ushort)Content.GravitySuit),
            GameplayMessageId.GunshipSaveConfirmation => LargeSetupSmallDraw((ushort)Content.SaveConfirmation),
            _ => throw new InvalidOperationException("Validated native message has no dispatch case."),
        };
    }
    private static GameplayMessageDefinition Small(ushort contentPointer) => new(
        GameplayMessageRomData.Routines.SetupSmall,
        GameplayMessageRomData.Routines.DrawSmallTilemap,
        contentPointer);

    private static GameplayMessageDefinition Shoot(ushort contentPointer) => new(
        GameplayMessageRomData.Routines.PatchShootButton,
        GameplayMessageRomData.Routines.DrawLargeTilemap,
        contentPointer);

    private static GameplayMessageDefinition Run(ushort contentPointer) => new(
        GameplayMessageRomData.Routines.PatchRunButton,
        GameplayMessageRomData.Routines.DrawLargeTilemap,
        contentPointer);

    private static GameplayMessageDefinition LargeSetupSmallDraw(ushort contentPointer) => new(
        GameplayMessageRomData.Routines.SetupLarge,
        GameplayMessageRomData.Routines.DrawSmallTilemap,
        contentPointer);
}

/// <summary>One native setup callback, draw callback, and presentation payload pointer.</summary>
internal readonly record struct GameplayMessageDefinition(
    ushort ModifyFunction,
    ushort DrawFunction,
    ushort ContentPointer);
