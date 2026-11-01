using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemy-side words that outlive each room, as the game keeps them across enemy loading (#1329): the
/// elevator journey, the shared fade counter, Ceres's escape progress, the critical-energy drop bias, the language
/// flag, the random enemy counter, the death quota an empty population keeps, the staging surfaces and the sprite looks
/// shown; the session's unauthored Speed Booster cadence, whose delays Ceres Ridley's getaway leaves behind; and the
/// host's installed enemy artwork, which every room's enemies draw from.
/// </summary>
public sealed class EnemySession : ISpriteObjectPage
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
    /// The shared <c>PaletteChangeNumerator</c>. Enemies and the runtime's door and death
    /// fades advance this one counter, as the game's fades all advance one shared counter.
    /// </summary>
    public GradualColorChangeCounter GradualColorChange { get; } = new();

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
    /// The sprite looks gameplay has shown, which enemy frames select their pictures from (#1463): the standard sprites,
    /// each room's graphics-set members and its encounters' looks, in the order shown. They outlive each room, as what a
    /// room showed stays shown until something replaces it.
    /// </summary>
    internal ShownSpriteArtwork SpriteLooks { get; } = new();

    /// <summary>
    /// Installed enemy presentation. Drawing or a named artwork transfer requires its corresponding catalog; null
    /// does not enable a fallback. The host reinstalls it after a debugger restore.
    /// </summary>
    [field: NonSerialized]
    internal EnemyTileArtworkCatalog? TileArtwork { get; set; }

    /// <summary>The installed pre-coloured enemy sheets; artwork, never saved. The host reinstalls them after a restore.</summary>
    [field: NonSerialized]
    internal IEnemySheetSource? Sheets { get; set; }

    /// <summary>Shows an encounter's <paramref name="look"/> over what showed before it (#1406, #1463).</summary>
    /// <exception cref="InvalidOperationException">No enemy sheets are installed.</exception>
    /// <exception cref="InvalidDataException">No catalogued encounter shows the look, so no sheet pictures it.</exception>
    internal void ShowLook(SpriteArtworkSource look)
    {
        IEncounterLookCatalog catalog = (Sheets ?? throw new InvalidOperationException("Showing an encounter's look requires installed enemy sheets.")).Looks;
        if (!catalog.IsEncounterLook(Ensure.NotNull(look)))
            throw new InvalidDataException($"No catalogued encounter shows {look}.");
        SpriteLooks.Show(look);
    }

    /// <summary>
    /// Whether <paramref name="look"/> shows on the sprites' artwork the Ceres Ridley arena's backgrounds show (#1463,
    /// #1466): it was shown and no later look has hidden it.
    /// </summary>
    /// <exception cref="InvalidOperationException">No enemy sheets are installed.</exception>
    internal bool Shows(SpriteArtworkSource look) =>
        SpriteLooks.VisibleNewestFirst((Sheets ?? throw new InvalidOperationException("Showing the sprites' artwork as a background requires installed enemy sheets.")).Looks)
            .Contains(Ensure.NotNull(look));

    /// <inheritdoc />
    bool ISpriteObjectPage.Shows(SpriteArtworkSource look) => Shows(look);

}
