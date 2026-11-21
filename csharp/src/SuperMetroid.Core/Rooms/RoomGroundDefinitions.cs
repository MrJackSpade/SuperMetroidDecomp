using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms;

/// <summary>The grounds rooms are made of (<see cref="RoomGround"/>), and each area's own (#2416).</summary>
internal static partial class RoomGroundDefinitions
{
    /// <summary>Dry ground whose landings raise nothing.</summary>
    internal static readonly RoomGround Bare = new(RoomGroundWetness.Dry, DryLandingsRaiseDust: false);

    /// <summary>Dry ground whose landings raise dust.</summary>
    internal static readonly RoomGround Dusty = new(RoomGroundWetness.Dry, DryLandingsRaiseDust: true);

    /// <summary>Ground that is always wet.</summary>
    internal static readonly RoomGround Wet = new(RoomGroundWetness.Wet, DryLandingsRaiseDust: false);

    /// <summary>Ground that is wet while it rains, as on the Landing Site.</summary>
    internal static readonly RoomGround WetWhileRaining = new(RoomGroundWetness.WetWhileRaining, DryLandingsRaiseDust: false);

    /// <summary>Ground that is wet at and below the Wrecked Ship entrance's waterline.</summary>
    internal static readonly RoomGround WetBelowWaterline = new(RoomGroundWetness.WetBelowWaterline, DryLandingsRaiseDust: false);

    /// <summary>
    /// Each area's ground, which its rooms have unless they declare their own: Crateria's, Brinstar's, Tourian's and
    /// Ceres's bare, Norfair's and the Wrecked Ship's dusty, and Maridia's wet.
    /// </summary>
    internal static readonly IReadOnlyDictionary<AreaId, RoomGround> AreaGrounds = new Dictionary<AreaId, RoomGround>
    {
        [AreaId.Crateria] = Bare,
        [AreaId.Brinstar] = Bare,
        [AreaId.Norfair] = Dusty,
        [AreaId.WreckedShip] = Dusty,
        [AreaId.Maridia] = Wet,
        [AreaId.Tourian] = Bare,
        [AreaId.Ceres] = Bare,
    };
}
