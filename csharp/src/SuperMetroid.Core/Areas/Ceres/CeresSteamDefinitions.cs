
namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Compiled fixed initialization values for the Ceres steam actor.</summary>
public static class CeresSteamDefinitions
{
    /// <summary>The random selection the initializer draws for a vent's activation delay, before adding one: 32 delays.</summary>
    internal static readonly RandomSampleField ActivationDelay = new(0, 5);


    /// <summary>
    /// Each vent's animation and action, by its <see cref="CeresSteamVariant"/>: the up, left, down and right vents puff
    /// their own way and hold their place; the two vents in the rotating elevator room puff left and right, as the left and
    /// right vents do, and are drawn turned with the room.
    /// </summary>
    internal static readonly IReadOnlyDictionary<CeresSteamVariant, CeresSteamInitialization> Initializations =
        new Dictionary<CeresSteamVariant, CeresSteamInitialization>
        {
            [CeresSteamVariant.Up] = new(CeresSteamSegment.UpDormant, CeresSteamAction.HoldsItsPlace),
            [CeresSteamVariant.Left] = new(CeresSteamSegment.LeftDormant, CeresSteamAction.HoldsItsPlace),
            [CeresSteamVariant.Down] = new(CeresSteamSegment.DownDormant, CeresSteamAction.HoldsItsPlace),
            [CeresSteamVariant.Right] = new(CeresSteamSegment.RightDormant, CeresSteamAction.HoldsItsPlace),
            [CeresSteamVariant.RotatingElevatorLeft] = new(CeresSteamSegment.LeftDormant,
                CeresSteamAction.ApplyRotatingElevatorOffset),
            [CeresSteamVariant.RotatingElevatorRight] = new(CeresSteamSegment.RightDormant,
                CeresSteamAction.ApplyRotatingElevatorOffset),
        };
}
