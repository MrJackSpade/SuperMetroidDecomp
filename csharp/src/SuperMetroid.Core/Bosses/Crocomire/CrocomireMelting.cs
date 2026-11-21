using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Crocomire melting in the acid: the melt images and the dissolve passes.</summary>
[RoomLifetime]
internal sealed class CrocomireMelting
{
    /// <summary>Crocomire's body layer: where it is placed, what his body shows on it, the tongue placed with it and the acid smoke drawn with it.</summary>
    private readonly CrocomireBodyLayer _bodyLayer;
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal CrocomireMelting(CrocomireBodyLayer bodyLayer, BossArtworkBinding bossArtwork)
    {
        _bossArtwork = bossArtwork;
        _bodyLayer = bodyLayer;
    }

    /// <summary>Shows <paramref name="illustration"/>'s melt image on his body and hands his death over to its next stage.</summary>
    /// <exception cref="InvalidDataException">His body sheet has no such melt image.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="illustration"/> is not one of his two melting illustrations.</exception>
    internal void ShowCrocomireMeltImage(CrocomireEnemyState state, CrocomireMeltingIllustration illustration)
    {
        switch (illustration)
        {
            case CrocomireMeltingIllustration.Flesh:
                ShowFirstCrocomireMeltImage(state);
                return;
            case CrocomireMeltingIllustration.BareSkull:
                ShowSecondCrocomireMeltImage(state);
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(illustration), illustration, "Crocomire melts through his first and second illustrations only.");
        }
    }

    /// <summary>
    /// Shows his flesh's melt image. Only this first melt hands the tongue its melting animation and moves it onto the
    /// body.
    /// </summary>
    private void ShowFirstCrocomireMeltImage(CrocomireEnemyState state)
    {
        BossBodyFrame image = RequireCrocomireMeltingImage(CrocomireMeltingIllustration.Flesh);
        StartCrocomireMeltingBodyAnimation(state, CrocomireBodyAnimation.MeltingFleshTopRow);

        CrocomireBodyLayer.StartCrocomireTongueAnimation(state, CrocomireTongueSegment.Melting);
        RoomEnemyPlace tongue = state.Tongue;
        tongue.Processing = tongue.Processing.With(EnemyProcessing.Animates | EnemyProcessing.OffScreen);
        tongue.Contact = tongue.Contact.With(EnemyContact.IgnoresSamus);
        tongue.Presence = tongue.Presence.Without(EnemyPresence.Hidden);
        tongue.Position = tongue.Position.WithWhole(new(state.Body.Position.X.Whole, state.Body.Position.Y.Whole + CrocomireMeltingDefinitions.TongueBelowBody));

        _bodyLayer.ShowCrocomireBodyFrame(image);
    }

    /// <summary>Shows his bare skull's melt image. It first levels the melt's distortion and leaves the tongue alone.</summary>
    private void ShowSecondCrocomireMeltImage(CrocomireEnemyState state)
    {
        BossBodyFrame image = RequireCrocomireMeltingImage(CrocomireMeltingIllustration.BareSkull);
        CrocomireBodyLayer.LevelCrocomireMeltDistortion(state);
        StartCrocomireMeltingBodyAnimation(state, CrocomireBodyAnimation.MeltingBareSkullTopRow);
        _bodyLayer.ShowCrocomireBodyFrame(image);
    }

    /// <summary>
    /// Requires melt image <paramref name="illustration"/> in Crocomire's installed body sheet before any stage, actor or
    /// body state changes (#1466).
    /// </summary>
    /// <exception cref="InvalidOperationException">No boss body sheets are installed.</exception>
    /// <exception cref="InvalidDataException">His body sheet has no such melt image.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="illustration"/> is not one of his two melt images.</exception>
    internal BossBodyFrame RequireCrocomireMeltingImage(CrocomireMeltingIllustration illustration) =>
        _bossArtwork.Body(BossBodyOwner.Crocomire).Frame(CrocomireMeltingFrameNames.Of(illustration));

    /// <summary>Starts the selected body melting animation, which the melt image just shown on his body layer goes with.</summary>
    private static void StartCrocomireMeltingBodyAnimation(CrocomireEnemyState state, CrocomireBodyAnimation bodyAnimation)
    {
        state.AdvanceDeathStage();
        CrocomireEncounterAccess.StartCrocomireBodyAnimation(state, bodyAnimation);
    }

    /// <summary>
    /// Begins <paramref name="pass"/>'s melt: levels the melt's stretch, resets its distortion, and starts
    /// the pass with no column erased and its first band to show. Each band keeps the previous pass's picture until this
    /// pass shows its own. The installation composes both melt images into his body sheet (#1466), and the pass's erase steps
    /// show its image from there.
    /// </summary>
    internal static void BeginCrocomireMeltPass(CrocomireEnemyState state, CrocomireMeltingIllustration pass)
    {
        CrocomireDeathState death = state.Death;
        CrocomireBodyLayer.LevelCrocomireMeltDistortion(state);
        state.AdvanceDeathStage();
        death.DistortionStep = CrocomireMeltingDefinitions.FirstDistortionSlope;
        death.LowestDistortionSourceRow = CrocomireMeltingDefinitions.LowestDistortionSourceRow;
        death.DistortionSourceRow = death.LowestDistortionSourceRow;
        death.MeltPass = pass;
        death.MeltColumnsErased = 0;
        death.NextMeltBand = 0;
        Array.Clear(death.MeltBandSteps);
    }

    /// <summary>
    /// Shows the pass's next band at its image before any column is erased, one band an update through the pass's round;
    /// the update after the round moves to the next death stage and starts the bands over.
    /// </summary>
    internal static void ShowNextCrocomireMeltBand(CrocomireEnemyState state)
    {
        CrocomireDeathState death = state.Death;
        if (death.NextMeltBand == BandRound(death.MeltPass))
        {
            state.AdvanceDeathStage();
            death.NextMeltBand = 0;
            return;
        }
        ShowMeltBand(death, death.NextMeltBand++);
    }

    /// <summary>
    /// The updates of one round of <paramref name="pass"/>'s bands: its image's six bands, one an update, and for the bare
    /// skull an update after them that shows none.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="pass"/> is not a melt image.</exception>
    internal static int BandRound(CrocomireMeltingIllustration pass) => pass switch
    {
        CrocomireMeltingIllustration.Flesh => CrocomireMeltingDefinitions.FirstPassBandRound,
        CrocomireMeltingIllustration.BareSkull => CrocomireMeltingDefinitions.SecondPassBandRound,
        _ => throw new ArgumentOutOfRangeException(nameof(pass), pass, $"Undefined {nameof(CrocomireMeltingIllustration)}."),
    };

    /// <summary>
    /// Shows <paramref name="band"/> at this update (#1406, #1652): from now on the band shows the pass's image after the
    /// columns erased so far, until it is shown again. The place in a round past the image's six bands shows none.
    /// </summary>
    private static void ShowMeltBand(CrocomireDeathState death, int band)
    {
        if (band < CrocomireMeltingDefinitions.MeltImageBandCount)
            death.MeltBandSteps[band] = death.MeltColumnsErased;
    }

    /// <summary>Starts Crocomire's dissolve once his melt image shows.</summary>
    internal static void BeginCrocomireMelting(CrocomireEnemyState state)
    {
        CrocomireDeathState death = state.Death;
        death.MeltingScrollActive = true;
        CrocomireBodyLayer.LevelCrocomireMeltDistortion(state);

        state.AdvanceDeathStage();
        death.BodyXBeforeMelting = state.Body.Position.X.Whole;
    }

    /// <summary>
    /// Updates his melting: the acid smokes, his body shakes and dissolves a column, the tongue is placed and the
    /// melt's stretch steepens.
    /// </summary>
    internal void RunCrocomireMelting(CrocomireEnemyState state)
    {
        _bodyLayer.SpawnCrocomireAcidSmoke(state);
        CrocomireDeathState death = state.Death;
        death.MeltShakeCounter = death.MeltShakeCounter.Decrement().Remaining;
        state.Body.Position = state.Body.Position.WithWholeX(
            death.BodyXBeforeMelting + (death.MeltShakeCounter.IsInAlternatePhase(CrocomireMeltingDefinitions.ShakeHalfPeriod) ? CrocomireMeltingDefinitions.ShakeOffset : 0));

        EraseNextCrocomireMeltingColumn(death);

        // The melting's step places his tongue onto the body again.
        CrocomireBodyLayer.PlaceCrocomireTongue(state);
        int nextAdjustedY = death.DistortionSourceRow - CrocomireMeltingDefinitions.DistortionSourceClimb;
        if (death.DistortionSourceRow < CrocomireMeltingDefinitions.DistortionSourceTop + CrocomireMeltingDefinitions.DistortionSourceClimb)
        {
            if (!death.DistortionStep.IsSlowerThan(CrocomireMeltingDefinitions.SteepestDistortionSlope))
            {
                CompleteCrocomireMeltingDissolve(state);
                return;
            }
            nextAdjustedY = CrocomireMeltingDefinitions.DistortionSourceTop;
        }
        death.DistortionSourceRow = nextAdjustedY;
        UnsignedSixteenSixteen steeper = death.DistortionStep + CrocomireMeltingDefinitions.DistortionSlopeGrowth;
        death.DistortionStep = steeper.IsSlowerThan(CrocomireMeltingDefinitions.SteepestDistortionSlope)
            ? steeper
            : CrocomireMeltingDefinitions.SteepestDistortionSlope;
        StretchCrocomireMeltDistortion(state, death);
    }

    /// <summary>
    /// Takes the dissolve's next step: erases the next column of the melt image and shows the pass's next
    /// band at the new step, starting the bands over after the last, and his body shows the whole melt image as the step
    /// leaves it (#1513). Once every column is erased it shows nothing more, while the distortion runs on to its end.
    /// </summary>
    internal void EraseNextCrocomireMeltingColumn(CrocomireDeathState death)
    {
        Ensure.NotNull(death);
        if (death.MeltColumnsErased == CrocomireMeltingDefinitions.ColumnCount)
            return;
        death.MeltColumnsErased++;
        if (death.NextMeltBand == BandRound(death.MeltPass))
            death.NextMeltBand = 0;
        ShowMeltBand(death, death.NextMeltBand++);
        // The image is stored once; each band hides the pixels its steps had erased when it was last shown (#1652, #2478).
        _bodyLayer.ShowCrocomireBodyFrame(RequireCrocomireMeltingImage(death.MeltPass).Dissolved(death.MeltBandSteps));
    }

    /// <summary>
    /// Stretches his body through the melt's band (#1468, #1802): from the line below the tongue, each line falls a further
    /// <see cref="CrocomireDeathState.DistortionStep"/> of a row behind where his body lies, while the band's source row
    /// climbs one row a line less that fraction, until it reaches
    /// <see cref="CrocomireDeathState.LowestDistortionSourceRow"/> or the band reaches the distortion's last line. The
    /// lines above the band show his body where the melt was last levelled; the lines below show it where it lies.
    /// </summary>
    /// <remarks>
    /// The band's first line follows the tongue, which rides his body; the body only shakes sideways while it melts, so
    /// the band never starts lower than the last stretch's, and the lines above it keep the levelled body.
    /// </remarks>
    private static void StretchCrocomireMeltDistortion(
        CrocomireEnemyState state,
        CrocomireDeathState death)
    {
        int firstLine = Math.Clamp(state.Tongue.Position.Y.Whole - CrocomireMeltingDefinitions.BandAboveTongue, 0,
            CrocomireMeltingDefinitions.DistortionLines);
        int rows = death.LowestDistortionSourceRow - death.DistortionSourceRow;
        // The source row rises a whole row less the slope each line, so it first reaches the lowest row on the line after
        // its rise passes one row short of it.
        UnsignedSixteenSixteen slope = death.DistortionStep;
        int bandLines = rows <= 0 ? 0 : (UnsignedSixteenSixteen.OnePixel - slope).WholeStepsWithin(rows - 1) + 1;
        int endLine = Math.Min(firstLine + bandLines, CrocomireMeltingDefinitions.DistortionLines);
        death.MeltDistortion = new CrocomireMeltDistortion(death.MeltDistortion.LevelledTop, state.Body.Position.Y.Whole,
            ScreenLine(firstLine), ScreenLine(endLine), slope);

        // The distortion's lines start at the first gameplay line; those below the screen are never shown.
        static int ScreenLine(int distortionLine) =>
            Math.Min(distortionLine + ScreenGeometry.GameplayHudHeightPixels, ScreenGeometry.ScreenHeightPixels);
    }

    /// <summary>Ends the current dissolve and hands his death over to its next stage.</summary>
    internal static void CompleteCrocomireMeltingDissolve(CrocomireEnemyState state)
    {
        CrocomireDeathState death = state.Death;
        death.MeltingScrollActive = false;
        state.AdvanceDeathStage();
        death.NextMeltBand = 0;
    }

    /// <summary>Clears his body layer and moves the actor on after the final melting pass.</summary>
    internal void FinishCrocomireMeltingPass(CrocomireEnemyState state)
    {
        state.StartVerticalMotion(CrocomireBridgeCollapseDefinitions.SinkStartingAcceleration);
        _bodyLayer.ClearCrocomireBody();
        state.AdvanceDeathStage();
    }
}
