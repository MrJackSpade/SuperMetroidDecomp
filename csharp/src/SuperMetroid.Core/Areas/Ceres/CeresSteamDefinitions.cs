
namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Compiled fixed initialization values for the Ceres steam actor.</summary>
public static class CeresSteamDefinitions
{
    /// <summary>The random selection the initializer draws for a vent's activation delay, before adding one: 32 delays.</summary>
    internal static readonly RandomSampleField ActivationDelay = new(0, 5);


    /// <summary>
    /// Each vent's animation and action, by its <see cref="CeresSteamVariant"/>: the up, left, down and right vents puff
    /// their own way and take no action; the two vents in the rotating elevator room puff left and right, as the left and
    /// right vents do, and are drawn turned with the room.
    /// </summary>
    internal static readonly IReadOnlyDictionary<CeresSteamVariant, CeresSteamInitialization> Initializations =
        new Dictionary<CeresSteamVariant, CeresSteamInitialization>
        {
            [CeresSteamVariant.Up] = new(CeresSteamSegment.UpDormant, null),
            [CeresSteamVariant.Left] = new(CeresSteamSegment.LeftDormant, null),
            [CeresSteamVariant.Down] = new(CeresSteamSegment.DownDormant, null),
            [CeresSteamVariant.Right] = new(CeresSteamSegment.RightDormant, null),
            [CeresSteamVariant.RotatingElevatorLeft] = new(CeresSteamSegment.LeftDormant,
                CeresSteamAction.ApplyRotatingElevatorOffset),
            [CeresSteamVariant.RotatingElevatorRight] = new(CeresSteamSegment.RightDormant,
                CeresSteamAction.ApplyRotatingElevatorOffset),
        };
}
