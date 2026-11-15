using System.Collections.Frozen;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>
/// Compiled policy for Samus's water splashes, running foot contacts, Crateria-specific atmospheric effects and
/// the Brinstar and Tourian rooms whose landings raise dust.
/// </summary>
internal static class SamusAtmosphericEffectDefinitions
{
    /// <summary>
    /// The Crateria rooms with an atmospheric effect, and that effect; every other Crateria room has none.
    /// </summary>
    internal static readonly FrozenDictionary<RoomDefinition, CrateriaAtmosphericEffect> CrateriaRoomEffects =
        new Dictionary<RoomDefinition, CrateriaAtmosphericEffect>
        {
            [RoomDefinitions.LandingSite] = CrateriaAtmosphericEffect.LandingSite,
            [RoomDefinitions.WestOcean] = CrateriaAtmosphericEffect.WreckedShipEntrance,
            [RoomDefinitions.CrateriaKihunter] = CrateriaAtmosphericEffect.WetFootsteps,
            [RoomDefinitions.EastOcean] = CrateriaAtmosphericEffect.WetFootsteps,
            [RoomDefinitions.ForgottenHighwayKagos] = CrateriaAtmosphericEffect.WetFootsteps,
            [RoomDefinitions.CrabMaze] = CrateriaAtmosphericEffect.WetFootsteps,
            [RoomDefinitions.ForgottenHighwayElbow] = CrateriaAtmosphericEffect.WetFootsteps,
            [RoomDefinitions.Moat] = CrateriaAtmosphericEffect.WetFootsteps,
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

    /// <summary>
    /// The room Y of the waterline in the Wrecked Ship entrance (the West Ocean): above it Samus's landings and footsteps
    /// are dry, at and below it they splash.
    /// </summary>
    internal const int WreckedShipEntranceWaterlineY = 944;
}
