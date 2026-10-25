using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Fx;

/// <summary>Which earthquakes shake what, how rising lava and acid rumble, and the rooms whose quakes are silent (#1415).</summary>
public static class RoomEarthquakeDefinitions
{
    /// <summary>First earthquake type that also shakes enemy positions.</summary>
    public const ushort FirstEnemyShakingType = 18;

    /// <summary>First earthquake type whose displacement is not shown.</summary>
    public const ushort FirstNonRenderedType = 36;

    /// <summary>Timer bit selecting the alternating shake direction.</summary>
    public const ushort AlternatingDirectionTimerMask = 2;

    /// <summary>Updates for which an enemy retains one applied shake displacement.</summary>
    public const ushort EnemyShakeDuration = 2;

    /// <summary>Room-shake type written every active lava/acid rise frame.</summary>
    public const ushort RisingLiquidType = 21;

    /// <summary>
    /// Bits set in the global earthquake timer every active lava/acid rise frame.
    /// The rise sets them, so existing low bits are preserved.
    /// </summary>
    public const ushort RisingLiquidTimerBits = 32;

    /// <summary>
    /// Base delays between rising lava's or acid's repeated rumble sounds, in emission order. The live RNG low two
    /// bits are added at each emission.
    /// </summary>
    /// <remarks>
    /// Retained as the particular irregular rhythm of repeated identical rumble
    /// sounds. The index advances chronologically by one emission, not by angle,
    /// distance, intensity or a semantic sound choice. The complete cycle has
    /// neither a ramp nor half-cycle reflection/repetition; equal delay values
    /// have different successors. Live RNG supplies separate variation. Reciting
    /// these eight choices in cases or fitting them would merely disguise the
    /// same rhythmic content, while replacing the rhythm changes sound cadence.
    /// This is the arbitrary-sequence/nonsense exception, not a cost or size exemption.
    /// </remarks>
    public static ReadOnlySpan<ushort> RisingLiquidSoundBaseTimers =>
        [1, 3, 2, 1, 1, 2, 2, 1];

    /// <summary>
    /// The six rooms whose special-FX initialization silences quake sounds, in the order the game compares them;
    /// every other room keeps them.
    /// </summary>
    public static IReadOnlyList<RoomHeaderDefinition> SoundSuppressedRooms { get; } =
    [
        RoomHeaderDefinitions.BombTorizo,
        RoomHeaderDefinitions.Climb,
        RoomHeaderDefinitions.Ridley,
        RoomHeaderDefinitions.Pillar,
        RoomHeaderDefinitions.MotherBrain,
        RoomHeaderDefinitions.TourianEscape4,
    ];
}
