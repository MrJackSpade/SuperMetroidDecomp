using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies.Corpses.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// The shades of Mother Brain's, the Shitroid's and the Zebetites' encounters (#1512, #1544), which the import pre-colours
/// their sprite sheets, her body layer and her room's pictures in, from each encounter's installed colours over the rows
/// and colour groups its loads write: her brain's, attack's, legs' and the Baby's rows, her body layer's colours and her
/// room's. Each owner names only its own shades: her body layer's are not her room's.
/// </summary>
internal static class MotherBrainSheetShades
{
    /// <summary>
    /// The row each enemy whose shades this places draws in, as Mother Brain's encounter writes it: her brain's, her body's
    /// common row (her body's parts name their own rows), the Baby's, and the attack row the Shitroid and the Zebetites
    /// draw in.
    /// </summary>
    private static readonly Dictionary<EnemyDefinitionId, int> DrawRows = new()
    {
        [EnemyDefinitionId.MotherBrainHead] = BrainRow / Group,
        [EnemyDefinitionId.MotherBrainBody] = 0,
        [EnemyDefinitionId.BabyMetroidCutscene] = SeventhRow / Group,
        [EnemyDefinitionId.BabyMetroid] = AttackRow / Group,
        [EnemyDefinitionId.Zebetite] = AttackRow / Group,
    };

    /// <summary>
    /// Rejects a sheet whose enemy the graphics-set data places in another row than the one these shades recolour for it,
    /// as its shades would then recolour colours it does not draw in.
    /// </summary>
    /// <exception cref="InvalidDataException">The data places <paramref name="enemy"/> in another row, or it takes no such row.</exception>
    internal static void RequireDrawRow(EnemyDefinitionId enemy, int row)
    {
        int expected = DrawRows.TryGetValue(enemy, out int known) ? known
            : throw new InvalidDataException($"Mother Brain's encounter shades place no row for {enemy}.");
        if (row != expected)
            throw new InvalidDataException($"The graphics-set data draws {enemy} in row {row}, but Mother Brain's encounter writes its colours to row {expected}.");
    }

    /// <summary>The colours of one row or colour group.</summary>
    private const int Group = 16;

    /// <summary>The first colour each of her loads writes: colour zero is the transparent backdrop.</summary>
    private const int FirstColour = 1;

    /// <summary>The first ink of her brain's row, sprite row one.</summary>
    private const int BrainRow = 1 * Group;

    /// <summary>The first ink of her attack's row, sprite row two, where the Shitroid and the Zebetites draw.</summary>
    private const int AttackRow = 2 * Group;

    /// <summary>The first ink of her rear legs' and the glass shards' row, sprite row three.</summary>
    private const int LegsRow = 3 * Group;

    /// <summary>The colour of her legs' row the drain to grey starts at.</summary>
    private const int DrainedLegsFirstColour = 4;

    /// <summary>The first ink of the tube projectiles', the Baby's and the corpse's row, sprite row seven.</summary>
    private const int SeventhRow = 7 * Group;

    /// <summary>The first ink of her body's background colours, the third tile group.</summary>
    private const int BodyGroup = 4 * Group;

    /// <summary>The first inks of the room flash's three slices: the second, fourth and sixth tile groups' colour four, three and three.</summary>
    private static readonly int[] FlashFirstInks = [3 * Group + 4, 5 * Group + 3, 7 * Group + 3];

    /// <summary>The first inks of the room lights' two slices: the second and fourth tile groups' colour one.</summary>
    private static readonly int[] LightsFirstInks = [3 * Group + 1, 5 * Group + 1];

    /// <summary>The colours of the Shitroid's membrane its pulse recolours, from its row's colour five.</summary>
    private const int MembraneFirstColour = 5;

    /// <summary>The colours of the Zebetites' row their pulse recolours, from its colour twelve.</summary>
    private const int PulseFirstColour = 12;

    /// <summary>
    /// The shades of the sheets Mother Brain's encounter draws: her brain and neck, the Baby, and the objects she
    /// spawns, from her installed colours in <paramref name="maps"/> and <paramref name="bosses"/>.
    /// </summary>
    /// <exception cref="InvalidDataException">Her colours are not installed.</exception>
    internal static IReadOnlyList<SheetShade> Sprites(ImportAreaMapPresentation? maps, BossSourceArtwork bosses)
    {
        Ensure.NotNull(bosses);
        ImportAreaMapPresentation presentation = maps ?? throw Missing("Mother Brain's map-installed colours");
        MotherBrainHealthPalettePresentation health = presentation.MotherBrainHealthPalette;
        InstalledMotherBrainRainbowPalette rainbow = presentation.MotherBrainRainbowColours;
        MotherBrainRoomColorPresentation room = presentation.MotherBrainRoomColors;
        MotherBrainDeathColorCatalog death = bosses.MotherBrainDeathColors ?? throw Missing("Mother Brain's death colours");
        BabyMetroidCutsceneColorCatalog baby = bosses.BabyMetroidCutsceneColors ?? throw Missing("the Baby's cutscene colours");

        var shades = new List<SheetShade>([.. HealthSprites(health), .. RainbowSprites(rainbow)]);
        shades.AddRange(RoomSprites(room));
        for (int frame = 0; frame < death.BodyFadeFrameCount; frame++)
        {
            int shown = frame;
            shades.Add(BrainAndLegs(MotherBrainShades.DeathBody(shown), LegsRow + FirstColour,
                Colours(MotherBrainDeathColorFormat.BodyColorCount, colour => death.BodyColor(shown, colour)),
                Colours(MotherBrainDeathColorFormat.BodyColorCount, colour => death.LegColor(shown, colour))));
        }
        // Her detached brain fades as a corpse over its own colours.
        for (int frame = 0; frame < death.CorpseFadeFrameCount; frame++)
        {
            int shown = frame;
            shades.Add(SheetShade.Run(MotherBrainShades.CorpseFade(shown), BrainRow + FirstColour,
                Colours(MotherBrainDeathColorFormat.CorpseColorCount, colour => death.CorpseColor(shown, colour))));
        }
        shades.Add(SheetShade.Run(MotherBrainShades.BabyArrives, SeventhRow + FirstColour,
            Colours(BabyMetroidCutsceneColorFormat.InitialColorCount, baby.InitialColor)));
        for (int shade = 1; shade <= BabyMetroidCutsceneColorDefinitions.FadeFrameCount; shade++)
        {
            int shown = shade;
            shades.Add(SheetShade.Run(MotherBrainShades.BabyFade(shown), SeventhRow + FirstColour,
                Colours(BabyMetroidCutsceneColorFormat.FadeColorCount, colour => baby.FadeColor(shown, colour))));
        }
        return shades;
    }

    /// <summary>The shades of her body layer: her body's health bands, rainbow, grey and death fades.</summary>
    /// <exception cref="InvalidDataException">Her colours are not installed.</exception>
    internal static IReadOnlyList<SheetShade> BodyLayer(ImportAreaMapPresentation maps, BossSourceArtwork bosses)
    {
        Ensure.NotNull(maps);
        Ensure.NotNull(bosses);
        MotherBrainHealthPalettePresentation health = maps.MotherBrainHealthPalette;
        InstalledMotherBrainRainbowPalette rainbow = maps.MotherBrainRainbowColours;
        MotherBrainDeathColorCatalog death = bosses.MotherBrainDeathColors ?? throw Missing("Mother Brain's death colours");
        var shades = new List<SheetShade>([.. HealthBackground(health), .. RainbowBackground(rainbow)]);
        for (int frame = 0; frame < death.BodyFadeFrameCount; frame++)
        {
            int shown = frame;
            shades.Add(Body(MotherBrainShades.DeathBody(shown), Colours(MotherBrainDeathColorFormat.BodyColorCount, colour => death.BodyColor(shown, colour))));
        }
        return shades;
    }

    /// <summary>The shades her fight lays over her room's pictures: its flash and steady colours, and its lights going out and recovering.</summary>
    internal static IReadOnlyList<SheetShade> Room(ImportAreaMapPresentation maps)
    {
        Ensure.NotNull(maps);
        return RoomBackground(maps.MotherBrainRoomColors);
    }

    /// <summary>The sprite shades of her health bands: her brain's and rear legs' colours in each.</summary>
    internal static IReadOnlyList<SheetShade> HealthSprites(MotherBrainHealthPalettePresentation health)
    {
        Ensure.NotNull(health);
        var shades = new List<SheetShade>();
        for (int band = 0; band < MotherBrainHealthPaletteFormat.StateCount; band++)
        {
            int shown = band;
            shades.Add(BrainAndLegs(MotherBrainShades.Health(shown), LegsRow + FirstColour,
                Colours(MotherBrainRainbowPaletteGeometry.ColorCount, colour => health.body.Colour(shown, colour)),
                Colours(MotherBrainRainbowPaletteGeometry.ColorCount, colour => health.backLegs.Colour(shown, colour))));
        }
        return shades;
    }

    /// <summary>
    /// The sprite shades of her rainbow colours: her brain's and rear legs' colours in each rainbow phase, restored, in
    /// each step of the drain to grey and the revival from it, and her brain's in each step of the fake death's fades.
    /// </summary>
    internal static IReadOnlyList<SheetShade> RainbowSprites(InstalledMotherBrainRainbowPalette rainbow)
    {
        Ensure.NotNull(rainbow);
        var shades = new List<SheetShade>();
        for (int frame = 0; frame < rainbow.rainbow.Length; frame++)
            shades.Add(Phase(MotherBrainShades.Rainbow(frame), rainbow.rainbow[frame]));
        shades.Add(Phase(MotherBrainShades.Restored, rainbow.normal));
        for (int frame = 0; frame < rainbow.toGrey.Length; frame++)
        {
            int shown = frame;
            shades.Add(BrainAndLegs(MotherBrainShades.ToGrey(shown), LegsRow + DrainedLegsFirstColour,
                Colours(rainbow.toGrey.BodyCount, colour => rainbow.toGrey.Body(shown, colour)),
                Colours(rainbow.toGrey.LegCount, colour => rainbow.toGrey.Leg(shown, colour))));
        }
        for (int frame = 0; frame < rainbow.fromGrey.Length; frame++)
        {
            int shown = frame;
            shades.Add(BrainAndLegs(MotherBrainShades.FromGrey(shown), LegsRow + DrainedLegsFirstColour,
                Colours(rainbow.fromGrey.BodyCount, colour => rainbow.fromGrey.Body(shown, colour)),
                Colours(rainbow.fromGrey.LegCount, colour => rainbow.fromGrey.Leg(shown, colour))));
        }
        for (int frame = 0; frame < MotherBrainFakeDeathPaletteDefinitions.FrameCount; frame++)
        {
            int shown = frame;
            shades.Add(SheetShade.Run(MotherBrainShades.FakeDeathToGrey(shown), BrainRow + FirstColour,
                Colours(MotherBrainRainbowPaletteGeometry.FakeDeathColors, colour => rainbow.fakeDeathToGrey.Body(shown, colour))));
            shades.Add(SheetShade.Run(MotherBrainShades.FakeDeathFromGrey(shown), BrainRow + FirstColour,
                Colours(MotherBrainRainbowPaletteGeometry.FakeDeathColors, colour => rainbow.fromGrey.Body(shown, colour))));
        }
        return shades;

        SheetShade Phase(PictureShade shade, InstalledMotherBrainRainbowPalette.PaletteFrame phase) =>
            BrainAndLegs(shade, LegsRow + FirstColour, Colours(phase.Body.Length, colour => phase.Body[colour]), Colours(phase.LegCount, phase.Leg));
    }

    /// <summary>The background shades of her health bands: her body's colours in each.</summary>
    internal static IReadOnlyList<SheetShade> HealthBackground(MotherBrainHealthPalettePresentation health)
    {
        Ensure.NotNull(health);
        var shades = new List<SheetShade>();
        for (int band = 0; band < MotherBrainHealthPaletteFormat.StateCount; band++)
        {
            int shown = band;
            shades.Add(Body(MotherBrainShades.Health(shown), Colours(MotherBrainRainbowPaletteGeometry.ColorCount, colour => health.body.Colour(shown, colour))));
        }
        return shades;
    }

    /// <summary>The background shades of her rainbow colours: her body's colours in each rainbow phase, restored, and in each step of the drain and revival.</summary>
    internal static IReadOnlyList<SheetShade> RainbowBackground(InstalledMotherBrainRainbowPalette rainbow)
    {
        Ensure.NotNull(rainbow);
        var shades = new List<SheetShade>();
        for (int frame = 0; frame < rainbow.rainbow.Length; frame++)
        {
            InstalledMotherBrainRainbowPalette.PaletteFrame phase = rainbow.rainbow[frame];
            shades.Add(Body(MotherBrainShades.Rainbow(frame), Colours(phase.Body.Length, colour => phase.Body[colour])));
        }
        shades.Add(Body(MotherBrainShades.Restored, Colours(rainbow.normal.Body.Length, colour => rainbow.normal.Body[colour])));
        for (int frame = 0; frame < rainbow.toGrey.Length; frame++)
        {
            int shown = frame;
            shades.Add(Body(MotherBrainShades.ToGrey(shown), Colours(rainbow.toGrey.BodyCount, colour => rainbow.toGrey.Body(shown, colour))));
        }
        for (int frame = 0; frame < rainbow.fromGrey.Length; frame++)
        {
            int shown = frame;
            shades.Add(Body(MotherBrainShades.FromGrey(shown), Colours(rainbow.fromGrey.BodyCount, colour => rainbow.fromGrey.Body(shown, colour))));
        }
        return shades;
    }

    /// <summary>The sprite shades of her room's colours: her phase-two attack and rear-leg colours and the room entry's glass and tubes.</summary>
    internal static IReadOnlyList<SheetShade> RoomSprites(MotherBrainRoomColorPresentation room)
    {
        Ensure.NotNull(room);
        return
        [
            Rows(MotherBrainShades.PhaseTwo,
                new ColourRun(AttackRow + FirstColour, Colours(MotherBrainRoomColorGeometry.PhaseTwoColors, room.phaseTwoAttack.Colour)),
                new ColourRun(LegsRow + FirstColour, Colours(MotherBrainRoomColorGeometry.PhaseTwoColors, room.phaseTwoRearLeg.Colour))),
            Rows(MotherBrainShades.RoomEntry,
                new ColourRun(LegsRow + FirstColour, Colours(MotherBrainRoomColorGeometry.InitialColors, room.initialGlassShard.Colour)),
                new ColourRun(SeventhRow + FirstColour, Colours(MotherBrainRoomColorGeometry.InitialColors, room.initialTubeProjectile.Colour))),
        ];
    }

    /// <summary>The background shades of her room's colours: each step of its flash, its steady grey, and its lights going out and recovering.</summary>
    internal static IReadOnlyList<SheetShade> RoomBackground(MotherBrainRoomColorPresentation room)
    {
        Ensure.NotNull(room);
        var shades = new List<SheetShade>();
        for (int frame = 0; frame < MotherBrainRoomPaletteProgramDefinitions.FlashFrameCount; frame++)
        {
            int shown = frame;
            shades.Add(Flash(MotherBrainShades.Flash(shown), colour => room.flash.Colour(shown, colour)));
        }
        shades.Add(Flash(MotherBrainShades.FinalRoom, colour => room.finalRoom.Colour(colour)));
        shades.Add(Lights(MotherBrainShades.LightsOut, _ => RgbColor.Black));
        for (int frame = 0; frame < MotherBrainRoomColorDefinitions.RecoveryLightsFrames; frame++)
        {
            int shown = frame;
            shades.Add(Lights(MotherBrainShades.RecoveryLights(shown), colour => room.recoveryLights.Colour(shown, colour)));
        }
        return shades;
    }

    /// <summary>The shades of the Shitroid's and its victim's sheets: its entry's colours and each phase of its membrane's pulse.</summary>
    /// <exception cref="InvalidDataException">Its colours are not installed.</exception>
    internal static IReadOnlyList<SheetShade> Shitroid(BossSourceArtwork bosses)
    {
        Ensure.NotNull(bosses);
        ShitroidColorCatalog shitroid = bosses.ShitroidColors ?? throw Missing("the Shitroid's colours");
        var shades = new List<SheetShade>
        {
            Rows(TourianCorpseShades.ShitroidTargets,
                new ColourRun(BrainRow + FirstColour, Colours(Group - FirstColour, colour => shitroid.TargetColor(ShitroidColorTarget.Sidehopper, colour + FirstColour))),
                new ColourRun(AttackRow + FirstColour, Colours(Group - FirstColour, colour => shitroid.TargetColor(ShitroidColorTarget.Shitroid, colour + FirstColour)))),
        };
        for (int phase = 0; phase < ShitroidColorDefinitions.NormalFrameCount; phase++)
        {
            int shown = phase;
            shades.Add(SheetShade.Run(ShitroidShades.Membrane(shown), AttackRow + MembraneFirstColour,
                Colours(ShitroidColorDefinitions.NormalColorsPerFrame, colour => shitroid.NormalColor(shown, colour))));
        }
        return shades;
    }

    /// <summary>The shades of the Zebetites' sheets: each frame of their shared pulse.</summary>
    /// <exception cref="InvalidDataException">Their colours are not installed.</exception>
    internal static IReadOnlyList<SheetShade> Zebetites(BossSourceArtwork bosses)
    {
        Ensure.NotNull(bosses);
        ZebetiteColorCatalog zebetites = bosses.ZebetiteColors ?? throw Missing("the Zebetites' colours");
        return [.. Enumerable.Range(0, ZebetiteColorFormat.FrameCount).Select(frame => SheetShade.Run(ZebetiteShades.Pulse(frame),
            AttackRow + PulseFirstColour, Colours(ZebetiteColorFormat.ColorsPerFrame, colour => zebetites.Resolve(frame, colour))))];
    }

    /// <summary>A load of the same colours into her brain's row and of <paramref name="legs"/> into her legs' row from <paramref name="legsFirstInk"/>.</summary>
    private static SheetShade BrainAndLegs(PictureShade shade, int legsFirstInk, RgbColor[] brain, RgbColor[] legs) =>
        Rows(shade, new ColourRun(BrainRow + FirstColour, brain), new ColourRun(legsFirstInk, legs));

    /// <summary>A load of her body's background colours.</summary>
    private static SheetShade Body(PictureShade shade, RgbColor[] body) => SheetShade.Run(shade, BodyGroup + FirstColour, body);

    /// <summary>A load of the room flash's three slices: twelve colours, then twelve more mirrored into the third slice.</summary>
    private static SheetShade Flash(PictureShade shade, Func<int, RgbColor> colour)
    {
        RgbColor[] first = Colours(MotherBrainRoomColorGeometry.SliceColors, colour);
        RgbColor[] second = Colours(MotherBrainRoomColorGeometry.SliceColors, index => colour(MotherBrainRoomColorGeometry.SliceColors + index));
        return Rows(shade, new ColourRun(FlashFirstInks[0], first), new ColourRun(FlashFirstInks[1], second), new ColourRun(FlashFirstInks[2], second));
    }

    /// <summary>A load of the room lights' two fourteen-colour slices.</summary>
    private static SheetShade Lights(PictureShade shade, Func<int, RgbColor> colour) =>
        Rows(shade, new ColourRun(LightsFirstInks[0], Colours(MotherBrainRoomColorGeometry.RecoveryLightsSliceColors, colour)),
            new ColourRun(LightsFirstInks[1], Colours(MotherBrainRoomColorGeometry.RecoveryLightsSliceColors,
                index => colour(MotherBrainRoomColorGeometry.RecoveryLightsSliceColors + index))));

    /// <summary>A shade of several runs of colours, each from its first ink, in rising order.</summary>
    private static SheetShade Rows(PictureShade shade, params ColourRun[] runs) =>
        new(shade, [.. runs.SelectMany(run => Enumerable.Range(run.FirstInk, run.Colours.Length))], [.. runs.SelectMany(run => run.Colours)]);

    /// <summary><paramref name="count"/> colours, each from <paramref name="colour"/>.</summary>
    private static RgbColor[] Colours(int count, Func<int, RgbColor> colour) => [.. Enumerable.Range(0, count).Select(colour)];

    /// <summary>The error for an encounter whose colours are not installed.</summary>
    private static InvalidDataException Missing(string colours) => new($"The sheets are shaded from {colours}, which are not installed.");
}
