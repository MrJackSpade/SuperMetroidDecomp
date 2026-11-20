using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Core.Samus.Drawing.SamusAtmosphericEffects;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>
/// The ground Samus's running footsteps and landings go by (#2416): a room's, by its area and its own policy, or a scene's
/// own ground where no room has loaded.
/// </summary>
/// <param name="Wetness">When the ground is wet, so that her footsteps and landings splash.</param>
/// <param name="DryLandingsRaiseDust">Whether her landings raise dust while the ground is dry; otherwise they raise nothing.</param>
public sealed record SamusGround(SamusGroundWetness Wetness, bool DryLandingsRaiseDust)
{
    /// <summary>
    /// The ground of <paramref name="room"/>: Crateria's by its rooms' policy, the Green Pirates Shaft's dusty; Brinstar's
    /// and Tourian's dusty in their dusty rooms; Norfair's and the Wrecked Ship's dusty; Maridia's wet; Ceres's bare.
    /// </summary>
    /// <exception cref="InvalidDataException">The room lies outside the seven areas.</exception>
    internal static SamusGround Of(RoomDefinition room) => Ensure.NotNull(room).Area switch
    {
        AreaId.Crateria when room == RoomDefinitions.GreenPiratesShaft => new(SamusGroundWetness.Dry, DryLandingsRaiseDust: true),
        AreaId.Crateria => new(ForCrateriaRoom(room) switch
        {
            null => SamusGroundWetness.Dry,
            CrateriaAtmosphericEffect.LandingSite => SamusGroundWetness.WetWhileRaining,
            CrateriaAtmosphericEffect.WreckedShipEntrance => SamusGroundWetness.WetBelowWaterline,
            CrateriaAtmosphericEffect.WetFootsteps => SamusGroundWetness.Wet,
            _ => throw new InvalidOperationException("Undefined Crateria atmospheric effect."),
        }, DryLandingsRaiseDust: false),
        AreaId.Brinstar or AreaId.Tourian => new(SamusGroundWetness.Dry, LandingRaisesDustInBrinstarOrTourian(room)),
        AreaId.Norfair or AreaId.WreckedShip => new(SamusGroundWetness.Dry, DryLandingsRaiseDust: true),
        AreaId.Maridia => new(SamusGroundWetness.Wet, DryLandingsRaiseDust: false),
        AreaId.Ceres => new(SamusGroundWetness.Dry, DryLandingsRaiseDust: false),
        _ => throw new InvalidDataException($"Room {room.Name}'s area {room.Area} is outside the seven areas."),
    };
}
