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

    /// <summary>
    /// Shows the first melting image of Crocomire's death hop. Only
    /// this first melt hands the tongue its melting animation and moves it onto the body.
    /// </summary>
    internal void ShowFirstCrocomireMeltImage(CrocomireEnemyState state)
    {
        _ = RequireCrocomireMeltingImage(CrocomireMeltingIllustration.Flesh);
        StartCrocomireMeltingBodyAnimation(state, CrocomireBodyAnimation.MeltingFleshTopRow);

        CrocomireBodyLayer.StartCrocomireTongueAnimation(state, CrocomireTongueSegment.Melting, 1);
        RoomEnemyPlace tongue = state.Tongue;
        tongue.Processing = tongue.Processing.With(EnemyProcessing.Animates | EnemyProcessing.OffScreen);
        tongue.Contact = tongue.Contact.With(EnemyContact.IgnoresSamus);
        tongue.Presence = tongue.Presence.Without(EnemyPresence.Hidden);
        tongue.Position = tongue.Position.WithWhole(new(state.Body.Position.X.Whole, state.Body.Position.Y.Whole + CrocomireMeltingDefinitions.TongueBelowBody));

        ShowCrocomireMeltingImage(CrocomireMeltingIllustration.Flesh);
    }

    /// <summary>
    /// Shows the second melting image of Crocomire's death hop. It first levels the melt's distortion and leaves the
    /// tongue alone.
    /// </summary>
    internal void ShowSecondCrocomireMeltImage(CrocomireEnemyState state)
    {
        _ = RequireCrocomireMeltingImage(CrocomireMeltingIllustration.BareSkull);
        CrocomireBodyLayer.LevelCrocomireMeltDistortion(state);
        StartCrocomireMeltingBodyAnimation(state, CrocomireBodyAnimation.MeltingBareSkullTopRow);
        ShowCrocomireMeltingImage(CrocomireMeltingIllustration.BareSkull);
    }

    /// <summary>
    /// Requires melt image <paramref name="illustration"/> in Crocomire's installed body sheet before any phase, actor or
    /// body state changes (#1466).
    /// </summary>
    /// <exception cref="InvalidOperationException">No boss body sheets are installed.</exception>
    /// <exception cref="InvalidDataException">His body sheet has no such melt image.</exception>
    internal BossBodyFrame RequireCrocomireMeltingImage(CrocomireMeltingIllustration illustration) =>
        _bossArtwork.Body(BossBodyOwner.Crocomire).Frame(CrocomireMeltingFrameNames.Of(illustration));

    /// <summary>Starts the selected body melting animation, which the melt image just shown on his body layer goes with.</summary>
    private static void StartCrocomireMeltingBodyAnimation(CrocomireEnemyState state, CrocomireBodyAnimation bodyAnimation)
    {
        state.DeathPhase = state.DeathPhase.Next();
        CrocomireEncounterAccess.StartCrocomireBodyAnimation(state, bodyAnimation);
    }

    /// <summary>Shows melt image <paramref name="illustration"/> as his whole body, in place of the body frame it showed (#1515).</summary>
    private void ShowCrocomireMeltingImage(CrocomireMeltingIllustration illustration) =>
        _bodyLayer.ShowCrocomireBodyFrame(RequireCrocomireMeltingImage(illustration));

    /// <summary>
    /// Begins <paramref name="pass"/>'s melt: levels the melt's stretch, resets its distortion, and starts
    /// the pass with no column erased and its first band to show. Each band keeps the previous pass's picture until this
    /// pass shows its own.
    /// </summary>
    /// <exception cref="InvalidDataException">The installed boss artwork has no melt images.</exception>
    internal void BeginCrocomireMeltPass(CrocomireEnemyState state, CrocomireMeltingIllustration pass)
    {
        CrocomireDeathState death = state.Death;
        _ = RequireCrocomireMeltingImage(pass);
        CrocomireBodyLayer.LevelCrocomireMeltDistortion(state);
        state.DeathPhase = state.DeathPhase.Next();
        death.DistortionStep = CrocomireMeltingDefinitions.FirstDistortionSlope;
        death.LowestDistortionSourceRow = CrocomireMeltingDefinitions.LowestDistortionSourceRow;
        death.DistortionSourceRow = death.LowestDistortionSourceRow;
        death.MeltPass = pass;
        death.MeltColumnsErased = 0;
        death.NextMeltBand = 0;
        Array.Clear(death.MeltBandSteps);
    }

    /// <summary>
    /// Shows the pass's next band at its image before any column is erased, one band an update; the update after the
    /// last band moves to the next death phase and starts the bands over.
    /// </summary>
    internal static void ShowNextCrocomireMeltBand(CrocomireEnemyState state)
    {
        CrocomireDeathState death = state.Death;
        if (death.NextMeltBand == BandCount(death.MeltPass))
        {
            state.DeathPhase = state.DeathPhase.Next();
            death.NextMeltBand = 0;
            return;
        }
        ShowMeltBand(death, death.NextMeltBand++);
    }

    /// <summary>The bands <paramref name="pass"/>'s melt image is shown in.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="pass"/> is not a melt image.</exception>
    internal static int BandCount(CrocomireMeltingIllustration pass) => pass switch
    {
        CrocomireMeltingIllustration.Flesh => CrocomireMeltingDefinitions.FirstPassBandCount,
        CrocomireMeltingIllustration.BareSkull => CrocomireMeltingDefinitions.SecondPassBandCount,
        _ => throw new ArgumentOutOfRangeException(nameof(pass), pass, $"Undefined {nameof(CrocomireMeltingIllustration)}."),
    };

    /// <summary>
    /// Shows <paramref name="band"/> at this update (#1406, #1652): from now on the band shows the pass's image after the
    /// columns erased so far, until it is shown again.
    /// </summary>
    private static void ShowMeltBand(CrocomireDeathState death, int band) => death.MeltBandSteps[band] = death.MeltColumnsErased;

    /// <summary>Starts Crocomire's dissolve once his melt image shows.</summary>
    internal static void BeginCrocomireMelting(CrocomireEnemyState state)
    {
        CrocomireDeathState death = state.Death;
        death.MeltingScrollActive = true;
        CrocomireBodyLayer.LevelCrocomireMeltDistortion(state);

        state.DeathPhase = state.DeathPhase.Next();
        death.BodyXBeforeMelting = state.Body.Position.X.Whole;
    }

    /// <summary>Updates the acid dissolve, background distortion, tongue placement, and scheduled melting graphics.</summary>
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
        if (death.NextMeltBand == BandCount(death.MeltPass))
            death.NextMeltBand = 0;
        ShowMeltBand(death, death.NextMeltBand++);
        // The image is stored once; each band hides the columns its step had erased when it was last shown (#1652).
        BossBodyFrame image = RequireCrocomireMeltingImage(death.MeltPass);
        _bodyLayer.ShowCrocomireBodyFrame(image.Hiding(CrocomireMeltMask.Of(image, death.MeltBandSteps)));
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
        int bandLines = rows <= 0 ? 0 : (new UnsignedSixteenSixteen(1, 0) - slope).WholeStepsWithin(rows - 1) + 1;
        int endLine = Math.Min(firstLine + bandLines, CrocomireMeltingDefinitions.DistortionLines);
        death.MeltDistortion = new CrocomireMeltDistortion(death.MeltDistortion.LevelledTop, state.Body.Position.Y.Whole,
            ScreenLine(firstLine), ScreenLine(endLine), slope);

        // The distortion's lines start at the first gameplay line; those below the screen are never shown.
        static int ScreenLine(int distortionLine) =>
            Math.Min(distortionLine + ScreenGeometry.GameplayHudHeightPixels, ScreenGeometry.ScreenHeightPixels);
    }

    /// <summary>Ends the current dissolve and selects the next death-scene phase.</summary>
    internal static void CompleteCrocomireMeltingDissolve(CrocomireEnemyState state)
    {
        CrocomireDeathState death = state.Death;
        death.MeltingScrollActive = false;
        state.DeathPhase = state.DeathPhase.Next();
        death.NextMeltBand = 0;
    }

    /// <summary>Clears his body layer and moves the actor on after the final melting pass.</summary>
    internal void FinishCrocomireMeltingPass(CrocomireEnemyState state)
    {
        state.StartVerticalMotion(CrocomireBridgeCollapseDefinitions.SinkStartingAcceleration);
        _bodyLayer.ClearCrocomireBody();
        state.DeathPhase = state.DeathPhase.Next();
    }
}
