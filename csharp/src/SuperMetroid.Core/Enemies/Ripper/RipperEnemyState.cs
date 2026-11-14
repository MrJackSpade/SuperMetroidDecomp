using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>
/// The state of one Ripper, GRipper or Ripper II: its flying animation, its patrol velocity, its linear-speed record,
/// and GRipper's two patrol bounds. All are the enemy's own: setup starts the animation for every variant and writes
/// the bounds for the GRipper, the only variant that reads them, so nothing a previous occupant of the slot left
/// reaches a new one.
/// </summary>
public sealed class RipperEnemyState
{
    /// <summary>The Ripper-family typed flying loop (#641), started by setup and restarted at each reversal.</summary>
    internal AnimationPlayback<RipperSegment> Playback { get; set; }

    /// <summary>Signed 1/65536-pixel horizontal velocity per update, one of the selected linear-speed row's speeds.</summary>
    public SignedSixteenSixteen XVelocity { get; internal set; }

    /// <summary>The selected row of the shared linear speed table, set by initialization and read back when the velocity is loaded.</summary>
    public EnemyLinearSpeedRow Speeds { get; internal set; }

    /// <summary>
    /// GRipper's exclusive left patrol reversal threshold, a room X coordinate in pixels from its spawn settings. The
    /// patrol compares it by the wrapped difference read as signed.
    /// </summary>
    public int MinimumXPosition { get; internal set; }

    /// <summary>
    /// GRipper's inclusive right patrol reversal threshold, a room X coordinate in pixels from its spawn settings. The
    /// patrol compares it by the wrapped difference read as signed.
    /// </summary>
    public int MaximumXPosition { get; internal set; }
}
