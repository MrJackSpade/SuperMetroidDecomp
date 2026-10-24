namespace SuperMetroid.Core.Runtime.Attract;

/// <summary>Native bank-$82 post-room-load demo callbacks.</summary>
public enum AttractDemoRoomSetup : ushort
{
    /// <summary>$82:891A, DemoRoom_ChargeBeamRoomScroll21: scroll cell $21 becomes red.</summary>
    ChargeBeamScroll = 0x891a,
    /// <summary>$82:8924, nullsub_291: RTS.</summary>
    NoOp = 0x8924,
    /// <summary>$82:8925, DemoRoom_SetBG2TilemapBase: the Landing Site shows its scrolling sky's tall layer-2 map.</summary>
    LandingSiteSky = 0x8925,
    /// <summary>$82:892B, DemoRoom_SetKraidFunctionTimer: body variable F becomes sixty.</summary>
    KraidTimer = 0x892b,
    /// <summary>$82:8932, DemoRoom_SetBrinstarBossBits: Brinstar boss byte becomes one.</summary>
    DefeatedKraid = 0x8932,
}
