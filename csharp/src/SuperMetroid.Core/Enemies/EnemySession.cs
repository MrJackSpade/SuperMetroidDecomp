using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemy-side words that outlive each room, as the game keeps them across enemy loading (#1329): the
/// elevator journey, the shared fade counter, Ceres's escape progress, the critical-energy drop bias, the language
/// flag, the random enemy counter, the death quota an empty population keeps, the staging surfaces and how far the
/// room's encounters have played their artwork; the session's unauthored Speed Booster cadence, whose delays Ceres Ridley's getaway leaves behind; and the
/// host's installed enemy artwork, which every room's enemies draw from.
/// </summary>
[ArchitectureReview("context-bag", "The enemy state that outlives each room and the installed enemy content every room binds: each member is session-lifetime state or installed data a room reads, not a service looked up.")]
public sealed class EnemySession
{
    /// <summary>Creates the enemy words a fresh power-on holds, bound to the session's unauthored Speed Booster cadence.</summary>
    /// <param name="unauthoredBoostCadence">The session's unauthored Speed Booster cadence, which outlives every runtime.</param>
    public EnemySession(UnauthoredBoostCadence unauthoredBoostCadence) =>
        UnauthoredBoostCadence = Ensure.NotNull(unauthoredBoostCadence);

    /// <summary>The session's unauthored Speed Booster cadence, whose delays Ceres Ridley's getaway wing frames leave behind.</summary>
    internal UnauthoredBoostCadence UnauthoredBoostCadence { get; }

    /// <summary>The elevator journey state carried through an elevator door.</summary>
    internal ElevatorJourneyState Elevator { get; } = new();

    /// <summary>
    /// The shared fade step count, a compatibility service (#1544): enemies' fades and the runtime's door, death and time-up
    /// fades all count their steps on it, so a fade left part-way shortens the next. It outlives every room.
    /// </summary>
    public SharedFadeStepCount FadeStepCount { get; } = new();

    /// <summary>
    /// How far Ceres's escape has progressed, consumed by the Ceres doors and the elevator shaft. A fresh station load
    /// begins before Ridley escapes; Ceres Ridley publishes his escape and the self-destruct, and the elevator door's
    /// destruction publishes the last stage.
    /// </summary>
    public CeresEscapeStatus CeresStatus { get; set; }

    /// <summary>
    /// Biases drops in the 30..49 energy grace band; room transitions
    /// retain it, and false is the game's cleared initial value.
    /// </summary>
    internal bool CriticalEnergyDropBias { get; set; }

    /// <summary>Language flag sampled when the Ceres warning-text phase begins and by Mother Brain's alternate escape-text selection.</summary>
    public bool JapaneseText { get; set; }

    /// <summary>
    /// Enemy-pass random counter read by AI and projectile routines, 0 through 65535. Its one advance wraps
    /// from 65535 back to zero, and every reader takes its low bits or copies it whole, so a wider store holds exactly
    /// the counter the game keeps.
    /// </summary>
    internal int RandomEnemyCounter { get; set; }

    /// <summary>Required room kill count from the nonempty population's terminator for enemy-clear triggers; empty-population initialization retains the previous quota. 0 through 255.</summary>
    internal int DeathQuota { get; set; }

    /// <summary>The session's record of what Mother Brain's rainbow-beam push hides from the camera; the runtime binds it.</summary>
    internal RainbowBeamCameraCheckpoint? RainbowBeamCameraCheckpoint { get; set; }

    /// <summary>
    /// The installation's services reproducing what Kraid's fingernails and Crocomire's volley read past their data
    /// (#1500); the runtime binds them, and a boss that reads past its data without them fails.
    /// </summary>
    internal BossAndIntroOverreadServices? BossOverreads { get; set; }

    /// <summary>
    /// The look each of the room's encounters shows (#1538), which the frames its looks change and the Ceres
    /// Ridley arena's backgrounds follow. Each room load starts it afresh.
    /// </summary>
    internal EncounterLooks Looks { get; } = new();

    /// <summary>
    /// Installed enemy presentation. Drawing or a named artwork transfer requires its corresponding catalog; null
    /// does not enable a fallback. It is installed content, bound with the game's artwork.
    /// </summary>
    internal EnemyTileArtworkCatalog? TileArtwork { get; set; }

    /// <summary>The installed pre-coloured enemy sheets, bound with the game's artwork.</summary>
    internal IEnemySheetSource? Sheets { get; set; }

    /// <summary>
    /// The installed services reproducing what enemies read past their authored data (#1500), bound with the game's
    /// artwork. An enemy that reaches one of those reads requires them; null
    /// does not enable a fallback.
    /// </summary>
    internal EnemyOverreadServices? Overreads { get; set; }
}
