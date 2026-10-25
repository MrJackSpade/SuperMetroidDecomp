using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Runtime.Attract;

/// <summary>
/// Verification's native view of the demo setup identities (#1417): the cartridge routine words naming each member.
/// Core's values are arbitrary; the demo scene importer translates through these tables.
/// </summary>
internal static class AttractDemoNativeWords
{
    /// <summary>The bank-$82 post-room-load callback naming each <see cref="AttractDemoRoomSetup"/>.</summary>
    internal static readonly NativeWordIdentities<AttractDemoRoomSetup> RoomSetups = new(new Dictionary<AttractDemoRoomSetup, ushort>
    {
        // DemoRoom_ChargeBeamRoomScroll21: scroll cell $21 becomes red.
        [AttractDemoRoomSetup.ChargeBeamScroll] = 0x891a,
        // nullsub_291: RTS.
        [AttractDemoRoomSetup.NoOp] = 0x8924,
        // DemoRoom_SetBG2TilemapBase.
        [AttractDemoRoomSetup.LandingSiteSky] = 0x8925,
        // DemoRoom_SetKraidFunctionTimer: body variable F becomes sixty.
        [AttractDemoRoomSetup.KraidTimer] = 0x892b,
        // DemoRoom_SetBrinstarBossBits: Brinstar boss byte becomes one.
        [AttractDemoRoomSetup.DefeatedKraid] = 0x8932,
    });

    /// <summary>The bank-$91 Samus setup function naming each <see cref="AttractDemoSamusSetup"/>.</summary>
    internal static readonly NativeWordIdentities<AttractDemoSamusSetup> SamusSetups = new(new Dictionary<AttractDemoSamusSetup, ushort>
    {
        [AttractDemoSamusSetup.LandingSite] = 0x8a33,
        [AttractDemoSamusSetup.MorphLeft] = 0x8a3e,
        [AttractDemoSamusSetup.LowHealthLeft] = 0x8a43,
        [AttractDemoSamusSetup.StandingLeft] = 0x8a49,
        [AttractDemoSamusSetup.FallingLeft] = 0x8a4e,
        [AttractDemoSamusSetup.StandingRight] = 0x8a53,
        [AttractDemoSamusSetup.DiagonalShinespark] = 0x8a68,
        [AttractDemoSamusSetup.HorizontalShinespark] = 0x8a81,
    });
}
