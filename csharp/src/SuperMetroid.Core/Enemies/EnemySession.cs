using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Elevator;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemy-side state that outlives each room, as the game keeps it across enemy loading (#1329): the
/// elevator journey, Ceres's escape progress, the critical-energy drop bias, the language
/// flag, the count of enemy passes, the death quota an empty population keeps and how far the
/// room's encounters have played their artwork; and what it receives when it is built: the compatibility services its
/// enemies consult (the installation's enemy and boss over-read services and the runtime's rainbow-beam camera checkpoint)
/// and the installed enemy sheets and enemy frame displays every room's enemies draw from (#1616).
/// </summary>
public sealed partial class EnemySession
{
    /// <summary>
    /// Creates the enemy state a fresh power-on holds, bound to the compatibility services its enemies consult.
    /// </summary>
    /// <param name="audioRequests">The update's audio requests of the runtime the enemies run in, which every room's enemies join (#2057).</param>
    /// <param name="overreads">The installation's services reproducing what enemies read past their authored data (#1500).</param>
    /// <param name="bossOverreads">
    /// The installation's services reproducing what Kraid's fingernails and Crocomire's volley read past their data (#1500).
    /// </param>
    /// <param name="sheets">The installed pre-coloured enemy sheets every room's enemies draw from.</param>
    /// <param name="frameDisplays">Every installed enemy frame's display, which drawing an enemy reads.</param>
    internal EnemySession(
        GameplayAudioRequests audioRequests,
        EnemyOverreadServices overreads,
        BossAndIntroOverreadServices bossOverreads,
        IEnemySheetSource sheets,
        EnemyFrameDisplays frameDisplays)
    {
        AudioRequests = Ensure.NotNull(audioRequests);
        Overreads = Ensure.NotNull(overreads);
        BossOverreads = Ensure.NotNull(bossOverreads);
        Sheets = Ensure.NotNull(sheets);
        FrameDisplays = Ensure.NotNull(frameDisplays);
    }

    /// <summary>The update's audio requests of the runtime the enemies run in, which every room's enemies' sounds and music join.</summary>
    internal GameplayAudioRequests AudioRequests { get; }

    /// <summary>The elevator journey state carried through an elevator door.</summary>
    internal ElevatorJourneyState Elevator { get; } = new();

    /// <summary>
    /// How far Ceres's escape has progressed, consumed by the Ceres doors and the elevator shaft. A fresh station load
    /// begins before Ridley escapes; Ceres Ridley publishes his escape and the self-destruct, and the elevator door's
    /// destruction publishes the last stage.
    /// </summary>
    public CeresEscapeStatus CeresStatus { get; set; }

    /// <summary>
    /// Biases drops in the low-energy grace band (<see cref="EnemyDropSelectionDefinitions.CriticalEnergy"/> up to <see cref="EnemyDropSelectionDefinitions.RecoveredEnergy"/>); room transitions
    /// retain it, and false is the game's cleared initial value.
    /// </summary>
    internal bool CriticalEnergyDropBias { get; set; }

    /// <summary>Language flag sampled when the Ceres warning-text phase begins and by Mother Brain's alternate escape-text selection.</summary>
    public TextLanguage Language { get; set; }

    /// <summary>The count of enemy passes (<see cref="EnemyFrameContext.EnemyPassCount"/>), kept from room to room.</summary>
    internal int EnemyPassCount { get; set; }

    /// <summary>
    /// The kills that clear the room for its grey doors and clearance watches: the room population's own quota
    /// (<see cref="PlacedRoomEnemyPopulationDefinition.DeathQuota"/>) when the room places enemies; a room that places none
    /// keeps the quota of the last room that did.
    /// </summary>
    internal int DeathQuota { get; set; }

    /// <summary>
    /// The installation's services reproducing what Kraid's fingernails and Crocomire's volley read past their data
    /// (#1500).
    /// </summary>
    internal BossAndIntroOverreadServices BossOverreads { get; }

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
