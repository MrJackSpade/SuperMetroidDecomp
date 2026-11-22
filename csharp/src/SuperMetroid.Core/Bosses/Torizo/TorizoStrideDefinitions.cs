namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>How far each of a Torizo's legs carries its body as it steps (#1891, #2411), whichever way it faces.</summary>
internal static partial class TorizoStrideDefinitions
{
    /// <summary>A step of the leg away from the side the Torizo faces: its right leg facing left, its left facing right.</summary>
    internal static TorizoStridePaces FarLeg { get; } = new(AfterLifting: 0, AfterSwinging: 5, AfterReaching: 19, AfterPlanting: 16, AfterPlanted: 7);

    /// <summary>A step of the leg on the side the Torizo faces: its left leg facing left, its right facing right.</summary>
    internal static TorizoStridePaces NearLeg { get; } = new(AfterLifting: 0, AfterSwinging: 7, AfterReaching: 17, AfterPlanting: 18, AfterPlanted: 5);
}
