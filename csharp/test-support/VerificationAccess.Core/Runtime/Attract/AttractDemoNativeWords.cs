using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Runtime.Attract;

/// <summary>
/// Verification's native view of the demo setup identities (#1417): the cartridge routine words naming each member.
/// Core's values are arbitrary; the demo scene importer translates through these tables.
/// </summary>
internal static class AttractDemoNativeWords
{
    /// <summary>The bank-$82 post-room-load callback that does nothing: the scene has no <see cref="AttractDemoRoomSetup"/>.</summary>
    internal const ushort NoRoomSetup = 0x8924;

    /// <summary>
    /// The bank-$82 post-room-load callback DemoRoom_SetBG2TilemapBase, which the Landing Site's scenes name: it points the
    /// background at the sky's tall map, which loading the room's sky already shows, so the scene has no
    /// <see cref="AttractDemoRoomSetup"/> (#2385).
    /// </summary>
    internal const ushort SkyTilemapSetup = 0x8925;

    /// <summary>The bank-$82 post-room-load callback naming each <see cref="AttractDemoRoomSetup"/>.</summary>
    internal static readonly NativeWordIdentities<AttractDemoRoomSetup> RoomSetups = new(new Dictionary<AttractDemoRoomSetup, ushort>
    {
        // DemoRoom_ChargeBeamRoomScroll21: scroll cell $21 becomes red.
        [AttractDemoRoomSetup.ChargeBeamScroll] = 0x891a,
        // DemoRoom_SetKraidFunctionTimer: body variable F becomes sixty.
        [AttractDemoRoomSetup.KraidTimer] = 0x892b,
        // DemoRoom_SetBrinstarBossBits: Brinstar boss byte becomes one.
        [AttractDemoRoomSetup.DefeatedKraid] = 0x8932,
    });

    /// <summary>The bank-$91 Samus setup function naming each of the <see cref="AttractDemoSamusSetups"/>.</summary>
    internal static readonly NativeWordIdentities<AttractDemoSamusSetup> SamusSetups = new(new Dictionary<AttractDemoSamusSetup, ushort>
    {
        [AttractDemoSamusSetups.LandingSite] = 0x8a33,
        [AttractDemoSamusSetups.MorphLeft] = 0x8a3e,
        [AttractDemoSamusSetups.LowHealthLeft] = 0x8a43,
        [AttractDemoSamusSetups.StandingLeft] = 0x8a49,
        [AttractDemoSamusSetups.FallingLeft] = 0x8a4e,
        [AttractDemoSamusSetups.StandingRight] = 0x8a53,
        [AttractDemoSamusSetups.DiagonalShinespark] = 0x8a68,
        [AttractDemoSamusSetups.HorizontalShinespark] = 0x8a81,
    });

    /// <summary>
    /// Where each posed setup's routine loads the pose it starts Samus in, as the immediate of an <c>LDA #</c> whose opcode
    /// byte comes just before it, and, for the low-health setup, where it loads her energy: the pose and energy each
    /// <see cref="AttractDemoSamusSetup.Posed"/> holds are those immediates (#2500).
    /// </summary>
    internal static readonly IReadOnlyList<(AttractDemoSamusSetup.Posed Setup, int PoseImmediate, int? EnergyImmediate)> PosedImmediates =
    [
        (AttractDemoSamusSetups.MorphLeft, 0x918a3f, null),
        (AttractDemoSamusSetups.LowHealthLeft, 0x918a4a, 0x918a44),
        (AttractDemoSamusSetups.StandingLeft, 0x918a4a, null),
        (AttractDemoSamusSetups.FallingLeft, 0x918a4f, null),
        (AttractDemoSamusSetups.StandingRight, 0x918a54, null),
    ];

    /// <summary>Where each shinespark setup's routine loads the pose it launches Samus in, as an <c>LDA #</c> immediate.</summary>
    internal static readonly IReadOnlyList<(AttractDemoSamusSetup.Shinespark Setup, int PoseImmediate)> ShinesparkImmediates =
    [
        (AttractDemoSamusSetups.DiagonalShinespark, 0x918a73),
        (AttractDemoSamusSetups.HorizontalShinespark, 0x918a8c),
    ];
}
