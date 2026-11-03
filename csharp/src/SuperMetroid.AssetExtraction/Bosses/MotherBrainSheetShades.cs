using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.World.RoomStates;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// The shades of Mother Brain's, the Shitroid's and the Zebetites' encounters (#1512), which the import pre-colours their
/// sprite sheets, her body layer and her room's pictures in, from each encounter's installed colours over the rows and
/// colour groups its loads write: her brain's, attack's, legs' and the Baby's rows and the background colours her body
/// layer shares with her room.
/// </summary>
internal static class MotherBrainSheetShades
{
    /// <summary>
    /// The row each enemy whose shades this places draws in, as Mother Brain's encounter writes it: her brain's, her body's
    /// common row (her body's parts name their own rows), the Baby's, and the attack row the Shitroid and the Zebetites
    /// draw in.
    /// </summary>
    private static readonly IReadOnlyDictionary<EnemyDefinitionId, int> DrawRows = new Dictionary<EnemyDefinitionId, int>
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

    /// <summary>The two colours of her brain's row the escape's typing copies room colours into.</summary>
    private const int TypewriterFirstColour = 13;

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
    /// spawns, from her installed colours in <paramref name="maps"/> and <paramref name="bosses"/> and her room's tile
    /// colours in <paramref name="roomPalettes"/>.
    /// </summary>
    /// <exception cref="InvalidDataException">Her colours are not installed.</exception>
    internal static IReadOnlyList<SheetShade> Sprites(AreaMapPresentation? maps, BossSourceArtwork bosses, RoomStaticPalettes? roomPalettes)
    {
        Ensure.NotNull(bosses);
        AreaMapPresentation presentation = maps ?? throw Missing("Mother Brain's map-installed colours");
        MotherBrainHealthPalettePresentation health = presentation.MotherBrainHealthPalette;
        MotherBrainRainbowPalettePresentation rainbow = presentation.MotherBrainRainbowPalette;
        MotherBrainRoomColorPresentation room = presentation.MotherBrainRoomColors;
        MotherBrainDeathColorCatalog death = bosses.MotherBrainDeathColors ?? throw Missing("Mother Brain's death colours");
        BabyMetroidCutsceneColorCatalog baby = bosses.BabyMetroidCutsceneColors ?? throw Missing("the Baby's cutscene colours");
        RgbColor[] standardRoom = RoomStaticPaletteImage.Colours((roomPalettes ?? throw Missing("Mother Brain's room colours")).Get(RoomPaletteOf(EnemyDefinitionId.MotherBrainBody)));

        var shades = new List<SheetShade>([.. HealthSprites(health), .. RainbowSprites(rainbow)]);
        shades.AddRange(RoomSprites(room));
        for (int frame = 0; frame < death.selection.BodyFade.FrameCount; frame++)
        {
            int shown = frame;
            shades.Add(BrainAndLegs(MotherBrainShades.DeathBody(shown), LegsRow + FirstColour,
                Colours(MotherBrainDeathDefinitions.BodyColorCount, colour => death.BodyColor(shown, colour)),
                Colours(MotherBrainDeathDefinitions.BodyColorCount, colour => death.LegColor(shown, colour))));
        }
        // The corpse's colours are her brain's, copied into the seventh row, so her brain's pictures show them.
        for (int frame = 0; frame < death.selection.CorpseFade.FrameCount; frame++)
        {
            int shown = frame;
            shades.Add(SheetShade.Run(MotherBrainShades.CorpseFade(shown), BrainRow + FirstColour,
                Colours(MotherBrainDeathDefinitions.CorpseColorCount, colour => death.CorpseColor(shown, colour))));
        }
        shades.Add(SheetShade.Run(MotherBrainShades.ExplodedDoor, BrainRow + FirstColour,
            Colours(MotherBrainDeathDefinitions.BodyColorCount, death.ExplodedDoorColor)));
        // The escape's shutter flash recolours the exploded door's colours nine to fourteen in her brain's row.
        shades.AddRange(PaletteFxSpriteShades.Of(PaletteFxObject.TourianShutterRedFlashing, PaletteFxColours.TourianShutters,
            BrainRow, presentation.RoomPaletteFx));
        shades.Add(SheetShade.Run(EscapeTypewriterShades.Text, BrainRow + TypewriterFirstColour,
            standardRoom.AsSpan(7 * Group + TypewriterFirstColour, 2)));
        shades.Add(SheetShade.Run(MotherBrainShades.BabyArrives, SeventhRow + FirstColour,
            Colours(BabyMetroidCutsceneColorDefinitions.InitialColorCount, baby.InitialColor)));
        for (int shade = 1; shade <= BabyMetroidCutsceneColorDefinitions.FadeFrameCount; shade++)
        {
            int shown = shade;
            shades.Add(SheetShade.Run(MotherBrainShades.BabyFade(shown), SeventhRow + FirstColour,
                Colours(BabyMetroidCutsceneColorDefinitions.FadeColorCount, colour => baby.FadeColor(shown, colour))));
        }
        return shades;
    }

    /// <summary>
    /// The shades of the background colours her body layer and her room's pictures share: her body's health bands,
    /// rainbow, grey and death fades, the room's flash and steady colours, and its lights going out and recovering.
    /// </summary>
    /// <exception cref="InvalidDataException">Her colours are not installed.</exception>
    internal static IReadOnlyList<SheetShade> Background(AreaMapPresentation maps, BossSourceArtwork bosses)
    {
        Ensure.NotNull(maps);
        Ensure.NotNull(bosses);
        MotherBrainHealthPalettePresentation health = maps.MotherBrainHealthPalette;
        MotherBrainRainbowPalettePresentation rainbow = maps.MotherBrainRainbowPalette;
        MotherBrainRoomColorPresentation room = maps.MotherBrainRoomColors;
        MotherBrainDeathColorCatalog death = bosses.MotherBrainDeathColors ?? throw Missing("Mother Brain's death colours");
        var shades = new List<SheetShade>([.. HealthBackground(health), .. RainbowBackground(rainbow)]);
        for (int frame = 0; frame < death.selection.BodyFade.FrameCount; frame++)
        {
            int shown = frame;
            shades.Add(Body(MotherBrainShades.DeathBody(shown), Colours(MotherBrainDeathDefinitions.BodyColorCount, colour => death.BodyColor(shown, colour))));
        }
        shades.AddRange(RoomBackground(room));
        return shades;
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
                Colours(MotherBrainRainbowPaletteDefinitions.ColorCount, colour => health.body.Color(shown, colour)),
                Colours(MotherBrainRainbowPaletteDefinitions.ColorCount, colour => health.backLegs.Color(shown, colour))));
        }
        return shades;
    }

    /// <summary>
    /// The sprite shades of her rainbow colours: her brain's and rear legs' colours in each rainbow phase, restored, in
    /// each step of the drain to grey and the revival from it, and her brain's in each step of the fake death's fades.
    /// </summary>
    internal static IReadOnlyList<SheetShade> RainbowSprites(MotherBrainRainbowPalettePresentation rainbow)
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
                Colours(MotherBrainFakeDeathPaletteDefinitions.ColorCount, colour => rainbow.fakeDeathToGrey.Body(shown, colour))));
            shades.Add(SheetShade.Run(MotherBrainShades.FakeDeathFromGrey(shown), BrainRow + FirstColour,
                Colours(MotherBrainFakeDeathPaletteDefinitions.ColorCount, colour => rainbow.fromGrey.Body(shown, colour))));
        }
        return shades;

        SheetShade Phase(PictureShade shade, MotherBrainRainbowPalettePresentation.PaletteFrame phase) =>
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
            shades.Add(Body(MotherBrainShades.Health(shown), Colours(MotherBrainRainbowPaletteDefinitions.ColorCount, colour => health.body.Color(shown, colour))));
        }
        return shades;
    }

    /// <summary>The background shades of her rainbow colours: her body's colours in each rainbow phase, restored, and in each step of the drain and revival.</summary>
    internal static IReadOnlyList<SheetShade> RainbowBackground(MotherBrainRainbowPalettePresentation rainbow)
    {
        Ensure.NotNull(rainbow);
        var shades = new List<SheetShade>();
        for (int frame = 0; frame < rainbow.rainbow.Length; frame++)
        {
            MotherBrainRainbowPalettePresentation.PaletteFrame phase = rainbow.rainbow[frame];
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
                (AttackRow + FirstColour, Colours(MotherBrainRoomColorDefinitions.PhaseTwoColors, room.phaseTwoAttack.Color)),
                (LegsRow + FirstColour, Colours(MotherBrainRoomColorDefinitions.PhaseTwoColors, colour =>
                    room.phaseTwoRearLeg?[colour] ?? MotherBrainHealthPalettePresentation.StockBaseColor(MotherBrainPalettePart.BackLeg, colour)))),
            Rows(MotherBrainShades.RoomEntry,
                (LegsRow + FirstColour, Colours(MotherBrainRoomColorDefinitions.InitialColors, room.initialGlassShard.Color)),
                (SeventhRow + FirstColour, Colours(MotherBrainRoomColorDefinitions.InitialColors, room.initialTubeProjectile.Color))),
        ];
    }

    /// <summary>The background shades of her room's colours: each step of its flash, its steady grey, and its lights going out and recovering.</summary>
    internal static IReadOnlyList<SheetShade> RoomBackground(MotherBrainRoomColorPresentation room)
    {
        Ensure.NotNull(room);
        var shades = new List<SheetShade>();
        for (int frame = 0; frame < MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount; frame++)
        {
            int shown = frame;
            shades.Add(Flash(MotherBrainShades.Flash(shown), colour => room.flash.Color(shown, colour)));
        }
        shades.Add(Flash(MotherBrainShades.FinalRoom, colour => room.finalRoom[colour]));
        shades.Add(Lights(MotherBrainShades.LightsOut, _ => RgbColor.Black));
        for (int frame = 0; frame < MotherBrainRoomColorDefinitions.RecoveryLightsFrames; frame++)
        {
            int shown = frame;
            shades.Add(Lights(MotherBrainShades.RecoveryLights(shown), colour => room.recoveryLights.Color(shown, colour)));
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
            Rows(ShitroidShades.Targets,
                (BrainRow + FirstColour, Colours(Group - FirstColour, colour => shitroid.TargetColor(ShitroidColorTarget.Sidehopper, colour + FirstColour))),
                (AttackRow + FirstColour, Colours(Group - FirstColour, colour => shitroid.TargetColor(ShitroidColorTarget.Shitroid, colour + FirstColour)))),
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
        Rows(shade, (BrainRow + FirstColour, brain), (legsFirstInk, legs));

    /// <summary>A load of her body's background colours.</summary>
    private static SheetShade Body(PictureShade shade, RgbColor[] body) => SheetShade.Run(shade, BodyGroup + FirstColour, body);

    /// <summary>A load of the room flash's three slices: twelve colours, then twelve more mirrored into the third slice.</summary>
    private static SheetShade Flash(PictureShade shade, Func<int, RgbColor> colour)
    {
        RgbColor[] first = Colours(MotherBrainRoomColorDefinitions.SliceColors, colour);
        RgbColor[] second = Colours(MotherBrainRoomColorDefinitions.SliceColors, index => colour(MotherBrainRoomColorDefinitions.SliceColors + index));
        return Rows(shade, (FlashFirstInks[0], first), (FlashFirstInks[1], second), (FlashFirstInks[2], second));
    }

    /// <summary>A load of the room lights' two fourteen-colour slices.</summary>
    private static SheetShade Lights(PictureShade shade, Func<int, RgbColor> colour) =>
        Rows(shade, (LightsFirstInks[0], Colours(MotherBrainRoomColorDefinitions.RecoveryLightsSliceColors, colour)),
            (LightsFirstInks[1], Colours(MotherBrainRoomColorDefinitions.RecoveryLightsSliceColors,
                index => colour(MotherBrainRoomColorDefinitions.RecoveryLightsSliceColors + index))));

    /// <summary>A shade of several runs of colours, each from its first ink, in rising order.</summary>
    private static SheetShade Rows(PictureShade shade, params (int FirstInk, RgbColor[] Colours)[] runs) =>
        new(shade, [.. runs.SelectMany(run => Enumerable.Range(run.FirstInk, run.Colours.Length))], [.. runs.SelectMany(run => run.Colours)]);

    /// <summary>The palette of the tileset of the room that places <paramref name="enemy"/>.</summary>
    /// <exception cref="InvalidDataException">No room, or rooms in differently coloured tilesets, place it.</exception>
    private static RoomPaletteSet RoomPaletteOf(EnemyDefinitionId enemy)
    {
        RoomPaletteSet[] palettes = [.. RetailRoomHeaders.All.SelectMany(RoomStateLists.GetStates)
            .Where(state => state.EnemyPopulation.Records.Span.ToArray().Any(record => record.DefinitionId == enemy))
            .Select(state => RoomTilesets.Get(state.GraphicsSet).Palette).Distinct()];
        return palettes.Length == 1 ? palettes[0]
            : throw new InvalidDataException($"{enemy}'s room colours are those of one tileset; {palettes.Length} place it.");
    }

    /// <summary><paramref name="count"/> colours, each from <paramref name="colour"/>.</summary>
    private static RgbColor[] Colours(int count, Func<int, RgbColor> colour) => [.. Enumerable.Range(0, count).Select(colour)];

    /// <summary>The error for an encounter whose colours are not installed.</summary>
    private static InvalidDataException Missing(string colours) => new($"The sheets are shaded from {colours}, which are not installed.");
}
