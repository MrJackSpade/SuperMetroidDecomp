namespace SuperMetroid.Core.Frontend;

/// <summary>Cartridge definition tables consumed by the title-demo loaders.</summary>
public static class AttractDemoRomData
{
    /// <summary>$82:8548 waits ninety NMI calls on the completed demo's final image.</summary>
    public const int FinalImageHoldFrames = 90;
    /// <summary>$82:8000 demo enemy-graphics loop starts at six and includes zero.</summary>
    public const int EnemyTransferFrames = 7;
    /// <summary>$80:8261 enables three sets before the completed-game SRAM marker.</summary>
    public const int DefaultSetCount = 3;
    /// <summary>Literal state writes in the demo setup routines.</summary>
    public static class SetupValues
    {
        /// <summary>$91:8A43 sets current energy to twenty without changing maximum energy.</summary>
        public const ushort LowHealth = 20;
        /// <summary>$82:891A writes the scroll cell at byte offset $21.</summary>
        public const int ChargeBeamScrollIndex = 0x21;
        /// <summary>$82:892B writes sixty to Kraid's body function timer.</summary>
        public const ushort KraidFunctionTimer = 60;
    }
    /// <summary>Number of physical entries in each retail demo-set pointer table.</summary>
    public const int SetCount = 4;
}

/// <summary>Native bank-$91 Samus setup dispatcher identities.</summary>
public enum AttractDemoSamusSetup : ushort
{
    /// <summary>$91:8A33, DemoSetFunc_0: front-facing Landing Site actor.</summary>
    LandingSite = 0x8a33,
    /// <summary>$91:8A3E, DemoSetFunc_3: left-facing grounded morph ball.</summary>
    MorphLeft = 0x8a3e,
    /// <summary>$91:8A43, DemoSetFunc_7: twenty health, standing left.</summary>
    LowHealthLeft = 0x8a43,
    /// <summary>$91:8A49, DemoSetFunc_2: standing left.</summary>
    StandingLeft = 0x8a49,
    /// <summary>$91:8A4E, DemoSetFunc_4: falling left.</summary>
    FallingLeft = 0x8a4e,
    /// <summary>$91:8A53, DemoSetFunc_1: standing right.</summary>
    StandingRight = 0x8a53,
    /// <summary>$91:8A68, DemoSetFunc_5: immediate diagonal-right shinespark.</summary>
    DiagonalShinespark = 0x8a68,
    /// <summary>$91:8A81, DemoSetFunc_6: immediate horizontal-left shinespark.</summary>
    HorizontalShinespark = 0x8a81,
}

/// <summary>Native bank-$82 post-room-load demo callbacks.</summary>
public enum AttractDemoRoomSetup : ushort
{
    /// <summary>$82:891A, DemoRoom_ChargeBeamRoomScroll21: scroll cell $21 becomes red.</summary>
    ChargeBeamScroll = 0x891a,
    /// <summary>$82:8924, nullsub_291: RTS.</summary>
    NoOp = 0x8924,
    /// <summary>$82:8925, DemoRoom_SetBG2TilemapBase: Landing Site BG2SC=$4A.</summary>
    LandingSiteSky = 0x8925,
    /// <summary>$82:892B, DemoRoom_SetKraidFunctionTimer: body variable F becomes sixty.</summary>
    KraidTimer = 0x892b,
    /// <summary>$82:8932, DemoRoom_SetBrinstarBossBits: Brinstar boss byte becomes one.</summary>
    DefeatedKraid = 0x8932,
}
