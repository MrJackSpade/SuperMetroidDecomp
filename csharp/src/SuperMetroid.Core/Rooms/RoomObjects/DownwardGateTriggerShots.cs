using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The shots that set a downward gate's trigger off (#2484): only the named families, or every family but them.
/// </summary>
internal sealed partial class DownwardGateTriggerShots
{
    /// <summary>The families whose shots set the trigger off.</summary>
    private readonly HashSet<SamusProjectileFamily> _accepted;

    /// <summary>A rule accepting the shots of <paramref name="accepted"/> alone.</summary>
    private DownwardGateTriggerShots(IEnumerable<SamusProjectileFamily> accepted) => _accepted = [.. accepted];

    /// <summary>Only shots of <paramref name="families"/> set the trigger off.</summary>
    internal static DownwardGateTriggerShots Only(params SamusProjectileFamily[] families) => new(families);

    /// <summary>Every shot but those of <paramref name="families"/> sets the trigger off.</summary>
    internal static DownwardGateTriggerShots AllBut(params SamusProjectileFamily[] families) =>
        new(Enum.GetValues<SamusProjectileFamily>().Except(families).ToHashSet());

    /// <summary>Whether a shot of <paramref name="family"/> sets the trigger off.</summary>
    internal bool Accepts(SamusProjectileFamily family) => _accepted.Contains(family);
}
