using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Crocomire melting in the acid: the melt images and the dissolve passes.</summary>
[RoomLifetime]
internal sealed class CrocomireMelting
{
    /// <summary>Crocomire's body layer: where it is placed, the canvas his death assembles it on, the tongue placed with it and the acid smoke drawn with it.</summary>
    private readonly CrocomireBodyLayer _bodyLayer;
    /// <summary>Crocomire's state lookups, body program start, PLMs, dust and random numbers, and the sound and music it last requested.</summary>
    private readonly CrocomireEncounterAccess _access;
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;
    /// <summary>Sprite and background presentation, whose room's animation frames publish the picture each band shows.</summary>
    private readonly EnemyPresentation _presentation;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal CrocomireMelting(EnemyPresentation presentation, CrocomireBodyLayer bodyLayer, CrocomireEncounterAccess access, BossArtworkBinding bossArtwork)
    {
        _presentation = presentation;
        _bossArtwork = bossArtwork;
        _bodyLayer = bodyLayer;
        _access = access;
    }

    /// <summary>
    /// <c>MainAI_Crocomire_DeathSequence_10_Hop_3_LoadMeltingTilemap</c> ($A4:9341). Only
    /// this first melt hands the tongue its melting program and moves it onto the body.
    /// </summary>
    internal void ShowFirstCrocomireMeltImage(CrocomireEnemyState state)
    {
        _ = RequireCrocomireMeltingImage(CrocomireMeltingIllustration.First);
        StartCrocomireMeltingBodyProgram(state, CrocomireBodyProgram.MeltingOneTopRow);

        if (state.Tongue is { } tongue)
        {
            CrocomireBodyLayer.StartCrocomireTongueProgram(state, CrocomireTongueProgram.Melting, WrappingCountdown.Start(1));
            tongue.Properties = tongue.Properties.Replace(
                EnemyProperties.ProcessInstructions |
                    EnemyProperties.ProcessOffScreen |
                    EnemyProperties.IgnoreSamusCollision |
                    EnemyProperties.Invisible,
                EnemyProperties.ProcessInstructions |
                    EnemyProperties.ProcessOffScreen |
                    EnemyProperties.IgnoreSamusCollision);
            tongue.Position = tongue.Position.WithWhole(new(state.Body.Position.X.Whole, unchecked((ushort)(state.Body.Position.Y.Whole + 16))));
        }

        // $A4:938D-93A2 clears the first sixteen rows.
        ShowCrocomireMeltingImage(CrocomireMeltingIllustration.First, clearedCells: 512);
    }

    /// <summary>
    /// <c>MainAI_Crocomire_DeathSequence_2C_Hop_6_LoadMeltingTilemap</c> ($A4:93ED). It
    /// first refills the melt's per-line BG2 vertical scroll and leaves the tongue alone.
    /// </summary>
    internal void ShowSecondCrocomireMeltImage(CrocomireEnemyState state)
    {
        _ = RequireCrocomireMeltingImage(CrocomireMeltingIllustration.Second);
        _bodyLayer.LevelCrocomireMeltDistortion(_bodyLayer.Placement.Y);
        StartCrocomireMeltingBodyProgram(state, CrocomireBodyProgram.MeltingTwoTopRow);
        // $A4:940E-941D clears the first thirty-two rows.
        ShowCrocomireMeltingImage(CrocomireMeltingIllustration.Second, clearedCells: 1024);
    }

    /// <summary>
    /// Requires melt image <paramref name="illustration"/> in Crocomire's installed body sheet before any phase, actor or
    /// body state changes (#1466).
    /// </summary>
    /// <exception cref="InvalidOperationException">No boss body sheets are installed.</exception>
    /// <exception cref="InvalidDataException">His body sheet has no such melt image.</exception>
    internal BossBodyFrame RequireCrocomireMeltingImage(CrocomireMeltingIllustration illustration) =>
        _bossArtwork.Body(BossBodyOwner.Crocomire).Frame(CrocomireMeltingFrameNames.Of(illustration));

    /// <summary>Starts the selected body melting list, which the melt image just shown on his body layer goes with.</summary>
    private static void StartCrocomireMeltingBodyProgram(CrocomireEnemyState state, CrocomireBodyProgram bodyProgram)
    {
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
        CrocomireEncounterAccess.StartCrocomireBodyProgram(state, bodyProgram);
    }

    /// <summary>
    /// Assembles melt image <paramref name="illustration"/> on the working canvas below its first
    /// <paramref name="clearedCells"/> cleared cells and shows it, with the first sixteen rows of the canvas before it.
    /// </summary>
    private void ShowCrocomireMeltingImage(CrocomireMeltingIllustration illustration, int clearedCells)
    {
        BossBodyCanvas working = _bodyLayer.WorkingBody;
        working.Clear(0, clearedCells);
        working.Draw(RequireCrocomireMeltingImage(illustration));
        // The image is shown with the sixteen rows the cartridge's transfer leads it with, and what follows it there.
        _bodyLayer.ShowCrocomireWorkingCells(0, CrocomireMeltingDefinitions.ShownMeltCells);
    }

    /// <summary>
    /// Begins <paramref name="pass"/>'s melt (<c>$A4:943D</c>): resets the BG2 scroll table and the distortion, and starts
    /// the pass with no column erased and its first band to show. Each band keeps the previous pass's picture until this
    /// pass shows its own.
    /// </summary>
    /// <exception cref="InvalidDataException">The installed boss artwork has no melt images.</exception>
    internal void BeginCrocomireMeltPass(CrocomireEnemyState state, CrocomireMeltingIllustration pass)
    {
        CrocomireDeathState death = _access.RequireCrocomireDeath();
        _ = RequireCrocomireMeltingImage(pass);
        _bodyLayer.LevelCrocomireMeltDistortion(_bodyLayer.Placement.Y);
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
        death.DistortionStep = 256;
        death.LowestDistortionSourceRow = CrocomireMeltingDefinitions.LowestDistortionSourceRow;
        death.DistortionSourceRow = death.LowestDistortionSourceRow;
        death.DistortionEndY = CrocomireMeltingDefinitions.DistortionEndY;
        death.MeltPass = pass;
        death.MeltColumnsErased = 0;
        death.NextMeltBand = 0;
    }

    /// <summary>
    /// Shows the pass's next band at its image before any column is erased, one band an update; the update after the
    /// last band moves to the next death phase and starts the bands over.
    /// </summary>
    internal void ShowNextCrocomireMeltBand(CrocomireEnemyState state)
    {
        CrocomireDeathState death = _access.RequireCrocomireDeath();
        if (death.NextMeltBand == BandCount(death.MeltPass))
        {
            state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
            death.NextMeltBand = 0;
            return;
        }
        ShowMeltBand(death, death.NextMeltBand++);
    }

    /// <summary>The bands <paramref name="pass"/>'s melt image is shown in.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="pass"/> is not a melt image.</exception>
    internal static int BandCount(CrocomireMeltingIllustration pass) => pass switch
    {
        CrocomireMeltingIllustration.First => CrocomireMeltingDefinitions.FirstPassBandCount,
        CrocomireMeltingIllustration.Second => CrocomireMeltingDefinitions.SecondPassBandCount,
        _ => throw new ArgumentOutOfRangeException(nameof(pass), pass, $"Undefined {nameof(CrocomireMeltingIllustration)}."),
    };

    /// <summary>
    /// Selects, at this update, the picture <paramref name="band"/> shows (#1406): the pass's image after the columns erased
    /// so far, which the band draws from the room's tile word sheet. A band keeps the picture until it is shown again.
    /// </summary>
    private void ShowMeltBand(CrocomireDeathState death, int band) =>
        _presentation.ArtworkFrames.Select(MeltBandFrame(new CrocomireMeltBandPicture(death.MeltPass, death.MeltColumnsErased), band));

    /// <summary>The animation band <paramref name="band"/> of the melt image is, whose frames are its pictures (#1406).</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="band"/> is not a band of either melt image.</exception>
    internal static ArtworkAnimation MeltBandAnimation(int band) =>
        (uint)band < CrocomireMeltingDefinitions.SecondPassBandCount ? new($"Crocomire melt band {band}")
            : throw new ArgumentOutOfRangeException(nameof(band), band, "Crocomire's melt images have no such band.");

    /// <summary>
    /// The frame of <paramref name="band"/>'s animation that shows <paramref name="picture"/>, which the extraction pictures
    /// his tile words over (#1392, #1406).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="band"/> is not a band of either melt image.</exception>
    internal static ArtworkFrame MeltBandFrame(CrocomireMeltBandPicture picture, int band) =>
        new(MeltBandAnimation(band), $"{picture.Pass} step {picture.Step}");

    /// <summary>Starts Crocomire's dissolve once his melt image shows.</summary>
    internal void BeginCrocomireMelting(CrocomireEnemyState state)
    {
        CrocomireDeathState death = _access.RequireCrocomireDeath();
        death.MeltingScrollActive = true;
        _bodyLayer.LevelCrocomireMeltDistortion(_bodyLayer.Placement.Y);

        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
        death.BodyXBeforeMelting = state.Body.Position.X.Whole;
    }

    /// <summary>Updates the acid dissolve, BG2 distortion, tongue placement, and scheduled melting graphics.</summary>
    internal void RunCrocomireMelting(CrocomireEnemyState state, SamusState? samus)
    {
        _bodyLayer.SpawnCrocomireAcidSmoke(state, samus);
        CrocomireDeathState death = _access.RequireCrocomireDeath();
        death.MeltShakeCounter = death.MeltShakeCounter.Decrement().Remaining;
        state.Body.Position = state.Body.Position.WithWholeX(unchecked((ushort)(
            death.BodyXBeforeMelting + (death.MeltShakeCounter.IsInAlternatePhase(2) ? 4 : 0))));

        EraseNextCrocomireMeltingColumn();

        // $A4:95D5 places his body layer again, which also moves the tongue onto the body.
        _bodyLayer.PlaceCrocomireBodyLayer(state);
        int nextAdjustedY = death.DistortionSourceRow - 3;
        if (death.DistortionSourceRow < 19)
        {
            if (death.DistortionStep >= CrocomireBehavior.MaximumCrocomireDistortionStep)
            {
                CompleteCrocomireMeltingDissolve(state);
                return;
            }
            nextAdjustedY = 16;
        }
        death.DistortionSourceRow = nextAdjustedY;
        death.DistortionStep = Math.Min(
            death.DistortionStep + 384,
            CrocomireBehavior.MaximumCrocomireDistortionStep);
        StretchCrocomireMeltDistortion(state, death);
    }

    /// <summary>
    /// Takes the dissolve's next step (<c>$A4:96C8</c>): erases the next column of the melt image and shows the pass's next
    /// band at the new step, starting the bands over after the last. Once every column is erased it shows nothing more,
    /// while the distortion runs on to its end.
    /// </summary>
    internal void EraseNextCrocomireMeltingColumn()
    {
        CrocomireDeathState death = _access.RequireCrocomireDeath();
        if (death.MeltColumnsErased == CrocomireMeltingDefinitions.ColumnCount)
            return;
        death.MeltColumnsErased++;
        if (death.NextMeltBand == BandCount(death.MeltPass))
            death.NextMeltBand = 0;
        ShowMeltBand(death, death.NextMeltBand++);
    }

    /// <summary>
    /// Stretches his body layer through the melt's band (#1468): from the line below the tongue, each line falls a further
    /// <see cref="CrocomireDeathState.DistortionStep"/> of a row behind his body layer's placement, while the band's source
    /// row climbs one row a line less that fraction, until it reaches
    /// <see cref="CrocomireDeathState.LowestDistortionSourceRow"/> or the band reaches the distortion's last line. The
    /// lines above the band keep the scroll the melt was last levelled at; the lines below show the placement.
    /// </summary>
    /// <remarks>
    /// The band's first line follows the tongue, which rides his body; the body only shakes sideways while it melts, so
    /// the band never starts lower than the last stretch's, and the lines above it keep the levelled scroll.
    /// </remarks>
    private void StretchCrocomireMeltDistortion(
        CrocomireEnemyState state,
        CrocomireDeathState death)
    {
        int firstLine = Math.Clamp((state.Tongue?.Position.Y.Whole ?? state.Body.Position.Y.Whole) - 72, 0,
            CrocomireMeltingDefinitions.DistortionLines);
        int rows = death.LowestDistortionSourceRow - death.DistortionSourceRow;
        // The source row rises a whole row less the slope's fraction each line, so it first reaches the lowest row on the
        // line after its rise passes one row short of it.
        int row = SixteenSixteen.FromWhole(1);
        int bandLines = rows <= 0 ? 0 : (rows - 1) * row / (row - death.DistortionStep) + 1;
        int endLine = Math.Min(firstLine + bandLines, CrocomireMeltingDefinitions.DistortionLines);
        death.MeltDistortion = new CrocomireMeltDistortion(death.MeltDistortion.ScrollAbove, _bodyLayer.Placement.Y,
            ScreenLine(firstLine), ScreenLine(endLine), death.DistortionStep);

        // The distortion's lines start at the first gameplay line; those below the screen are never shown.
        static int ScreenLine(int distortionLine) =>
            Math.Min(distortionLine + ScreenGeometry.GameplayHudHeightPixels, ScreenGeometry.ScreenHeightPixels);
    }

    /// <summary>Ends the current dissolve and selects the next death-scene instruction phase.</summary>
    internal void CompleteCrocomireMeltingDissolve(CrocomireEnemyState state)
    {
        CrocomireDeathState death = _access.RequireCrocomireDeath();
        death.MeltingScrollActive = false;
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
        death.NextMeltBand = 0;
    }

    /// <summary>Clears his body layer and moves the actor on after the final melting pass.</summary>
    internal void FinishCrocomireMeltingPass(CrocomireEnemyState state)
    {
        state.StartVerticalMotion(EightEight.FromWhole(CrocomireBehavior.SinkStartingAccelerationWhole));
        _bodyLayer.ClearCrocomireWorkingBody();
        _bodyLayer.ShowCrocomireWorkingCells(0, BossBodyDefinitions.PageRows * BossBodyDefinitions.ColumnCount);
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
    }
}
