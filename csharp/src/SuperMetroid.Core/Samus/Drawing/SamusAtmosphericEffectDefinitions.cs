using System.Collections.Frozen;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>
/// Compiled cartridge policy for Samus's water splashes, running foot contacts, and
/// Crateria-specific atmospheric effects.
/// </summary>
internal static class SamusAtmosphericEffectDefinitions
{
    /// <summary>
    /// The atmospheric effect type whose native sprite-list pointer is zero (<c>$90:8C03</c>), so <c>$90:8B03</c>'s
    /// <c>LDA $0000,Y</c> reads direct-page scratch as its attributes.
    /// </summary>
    internal const SamusAtmosphericEffectType ScratchListType = SamusAtmosphericEffectType.AlternateFootstepSplash;

    /// <summary>The number of Crateria rooms the $90:EDC9/$91:F0F3 atmospheric policy covers.</summary>
    internal const int CrateriaPolicyRooms = 16;

    /// <summary>
    /// The Crateria rooms whose $90:EDC9/$91:F0F3 atmospheric policy is nonzero, and that policy; every other room the
    /// policy covers has none.
    /// </summary>
    internal static readonly FrozenDictionary<RoomIdentity, CrateriaAtmosphericEffectFlags> CrateriaRoomEffects =
        new Dictionary<RoomIdentity, CrateriaAtmosphericEffectFlags>
        {
            [RoomIdentities.CrateriaLandingSite] = CrateriaAtmosphericEffectFlags.LandingSite,
            [RoomIdentities.CrateriaWestOcean] = CrateriaAtmosphericEffectFlags.WreckedShipEntrance,
            [RoomIdentities.CrateriaKihunterRoom] = CrateriaAtmosphericEffectFlags.WetFootsteps,
            [RoomIdentities.CrateriaEastOcean] = CrateriaAtmosphericEffectFlags.WetFootsteps,
            [RoomIdentities.CrateriaForgottenHighwayKagoRoom] = CrateriaAtmosphericEffectFlags.WetFootsteps,
            [RoomIdentities.CrateriaCrabMaze] = CrateriaAtmosphericEffectFlags.WetFootsteps,
            [RoomIdentities.CrateriaForgottenHighwayElbow] = CrateriaAtmosphericEffectFlags.WetFootsteps,
            [RoomIdentities.CrateriaMoat] = CrateriaAtmosphericEffectFlags.WetFootsteps,
        }.ToFrozenDictionary();
}
