using System.Collections.Frozen;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>
/// Compiled policy for Samus's water splashes, running foot contacts, and
/// Crateria-specific atmospheric effects.
/// </summary>
internal static class SamusAtmosphericEffectDefinitions
{
    /// <summary>
    /// The Crateria rooms whose atmospheric policy is nonzero, and that policy; every other Crateria room has none.
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
