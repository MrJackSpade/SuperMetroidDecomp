using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Elevator;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemy-side state that outlives each room, as the game keeps it across enemy loading (#1329): the
/// elevator journey, Ceres's escape progress, the critical-energy drop bias, the language
/// flag, the random enemy counter, the death quota an empty population keeps and how far the
/// room's encounters have played their artwork; and what it receives when it is built: the compatibility services its
/// enemies consult (the installation's enemy and boss over-read services and the runtime's rainbow-beam camera checkpoint)
/// and the installed enemy sheets and enemy frame displays every room's enemies draw from (#1616).
/// </summary>
public sealed class EnemySession
{
    /// <summary>
    /// Creates the enemy state a fresh power-on holds, bound to the compatibility services its enemies consult.
    /// </summary>
    /// <param name="overreads">The installation's services reproducing what enemies read past their authored data (#1500).</param>
    /// <param name="bossOverreads">
    /// The installation's services reproducing what Kraid's fingernails and Crocomire's volley read past their data (#1500).
    /// </param>
    /// <param name="rainbowBeamCameraCheckpoint">
    /// The runtime's record of what Mother Brain's rainbow-beam push hides from the camera, which the runtime's scrolling
    /// consumes.
    /// </param>
    /// <param name="sheets">The installed pre-coloured enemy sheets every room's enemies draw from.</param>
    /// <param name="frameDisplays">Every installed enemy frame's display, which drawing an enemy reads.</param>
    public EnemySession(
        EnemyOverreadServices overreads,
        BossAndIntroOverreadServices bossOverreads,
        RainbowBeamCameraCheckpoint rainbowBeamCameraCheckpoint,
        IEnemySheetSource sheets,
        EnemyFrameDisplays frameDisplays)
    {
        Overreads = Ensure.NotNull(overreads);
        BossOverreads = Ensure.NotNull(bossOverreads);
        RainbowBeamCameraCheckpoint = Ensure.NotNull(rainbowBeamCameraCheckpoint);
        Sheets = Ensure.NotNull(sheets);
        FrameDisplays = Ensure.NotNull(frameDisplays);
    }

    /// <summary>The elevator journey state carried through an elevator door.</summary>
    internal ElevatorJourneyState Elevator { get; } = new();

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
    /// Enemy-pass random counter read by AI and projectiles, 0 through 65535. Its one advance wraps
    /// from 65535 back to zero, and every reader takes it modulo a power of two or copies it whole, so a wider store holds exactly
    /// the counter the game keeps.
    /// </summary>
    internal int RandomEnemyCounter { get; set; }

    /// <summary>
    /// The kills that clear the room for its grey doors and clearance watches: the room population's own quota
    /// (<see cref="RoomEnemyPopulationDefinition.DeathQuota"/>) when the room places enemies; a room that places none
    /// keeps the quota of the last room that did.
    /// </summary>
    internal int DeathQuota { get; set; }

    /// <summary>
    /// The runtime's record of what Mother Brain's rainbow-beam push hides from the camera, which the push writes during
    /// the enemy pass.
    /// </summary>
    internal RainbowBeamCameraCheckpoint RainbowBeamCameraCheckpoint { get; }

    /// <summary>
    /// The installation's services reproducing what Kraid's fingernails and Crocomire's volley read past their data
    /// (#1500).
    /// </summary>
    internal BossAndIntroOverreadServices BossOverreads { get; }

    /// <summary>
    /// The look each of the room's encounters shows (#1538), which the frames its looks change and the Ceres
    /// Ridley arena's backgrounds follow. Each room load starts it afresh.
    /// </summary>
    internal EncounterLooks Looks { get; } = new();

    /// <summary>The installed pre-coloured enemy sheets every room's enemies draw from.</summary>
    internal IEnemySheetSource Sheets { get; }

    /// <summary>Every installed enemy frame's display, which drawing an enemy reads.</summary>
    internal EnemyFrameDisplays FrameDisplays { get; }

    /// <summary>
    /// The installation's services reproducing what enemies read past their authored data (#1500), consulted only on the
    /// path that reads past an enemy's data.
    /// </summary>
    internal EnemyOverreadServices Overreads { get; }
}
