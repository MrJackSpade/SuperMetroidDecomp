using System.Collections.Frozen;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>
/// Compiled policy for Samus's water splashes, running foot contacts, Crateria-specific atmospheric effects and
/// the Brinstar and Tourian rooms whose landings raise dust.
/// </summary>
internal static class SamusAtmosphericEffectDefinitions
{
    /// <summary>
    /// The Crateria rooms whose atmospheric policy is nonzero, and that policy; every other Crateria room has none.
    /// </summary>
    internal static readonly FrozenDictionary<RoomDefinition, CrateriaAtmosphericEffectFlags> CrateriaRoomEffects =
        new Dictionary<RoomDefinition, CrateriaAtmosphericEffectFlags>
        {
            [RoomDefinitions.LandingSite] = CrateriaAtmosphericEffectFlags.LandingSite,
            [RoomDefinitions.WestOcean] = CrateriaAtmosphericEffectFlags.WreckedShipEntrance,
            [RoomDefinitions.CrateriaKihunter] = CrateriaAtmosphericEffectFlags.WetFootsteps,
            [RoomDefinitions.EastOcean] = CrateriaAtmosphericEffectFlags.WetFootsteps,
            [RoomDefinitions.ForgottenHighwayKagos] = CrateriaAtmosphericEffectFlags.WetFootsteps,
            [RoomDefinitions.CrabMaze] = CrateriaAtmosphericEffectFlags.WetFootsteps,
            [RoomDefinitions.ForgottenHighwayElbow] = CrateriaAtmosphericEffectFlags.WetFootsteps,
            [RoomDefinitions.Moat] = CrateriaAtmosphericEffectFlags.WetFootsteps,
        }.ToFrozenDictionary();

    /// <summary>
    /// The Brinstar and Tourian rooms where Samus's landings raise dust; a landing in any other room of those two areas
    /// shows nothing. Tourian's dusty rooms are Blue Hopper, Dust Torizo, Big Boy, Seaweed and the Eye Door room.
    /// Brinstar applies Tourian's choice of rooms to its own, so Brinstar's dust shows in the Green Brinstar map
    /// station, the Green Brinstar Firefleas room, the Green Brinstar missile refill, Dachora's room and Spore Spawn's
    /// room.
    /// </summary>
    internal static readonly FrozenSet<RoomDefinition> BrinstarAndTourianLandingDustRooms =
        new[]
        {
            RoomDefinitions.GreenBrinstarMap,
            RoomDefinitions.GreenBrinstarFirefleas,
            RoomDefinitions.GreenBrinstarMissileRefill,
            RoomDefinitions.Dachora,
            RoomDefinitions.SporeSpawn,
            RoomDefinitions.BlueHopper,
            RoomDefinitions.DustTorizo,
            RoomDefinitions.BigBoy,
            RoomDefinitions.Seaweed,
            RoomDefinitions.TourianEyeDoor,
        }.ToFrozenSet();
}
