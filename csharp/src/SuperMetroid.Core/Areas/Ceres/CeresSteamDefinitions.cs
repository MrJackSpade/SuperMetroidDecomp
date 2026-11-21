
namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The Ceres steam actor's fixed starting values.</summary>
public static partial class CeresSteamDefinitions
{
    /// <summary>The random selection the setup draws for a vent's activation delay, before adding one: 32 delays.</summary>
    internal static readonly RandomSampleField ActivationDelay = RandomSampleField.OneOf(32);


    /// <summary>
    /// Each vent's animation and action, by its <see cref="CeresSteamVariant"/>: the up, left, down and right vents puff
    /// their own way and hold their place; the two vents in the rotating elevator room puff left and right, as the left and
    /// right vents do, and are drawn turned with the room.
    /// </summary>
    internal static readonly IReadOnlyDictionary<CeresSteamVariant, CeresSteamInitialization> Initializations =
        new Dictionary<CeresSteamVariant, CeresSteamInitialization>
        {
            [CeresSteamVariant.Up] = new(CeresSteamWay.Up, CeresSteamAction.HoldsItsPlace),
            [CeresSteamVariant.Left] = new(CeresSteamWay.Left, CeresSteamAction.HoldsItsPlace),
            [CeresSteamVariant.Down] = new(CeresSteamWay.Down, CeresSteamAction.HoldsItsPlace),
            [CeresSteamVariant.Right] = new(CeresSteamWay.Right, CeresSteamAction.HoldsItsPlace),
            [CeresSteamVariant.RotatingElevatorLeft] = new(CeresSteamWay.Left,
                CeresSteamAction.ApplyRotatingElevatorOffset),
            [CeresSteamVariant.RotatingElevatorRight] = new(CeresSteamWay.Right,
                CeresSteamAction.ApplyRotatingElevatorOffset),
        };
}
