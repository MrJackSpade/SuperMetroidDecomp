using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Animals.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Enemies.Corpses.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.FaceBlocks.Artwork;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.Enemies.Magdollite.Artwork;
using SuperMetroid.Core.Enemies.SpacePirates.Artwork;
using SuperMetroid.Core.Enemies.WorkRobots;
using SuperMetroid.Core.Enemies.WorkRobots.Artwork;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Enemies.WreckedShip.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The shades the import pre-colours one graphics set's enemy and spawned-object sheets in (#1512), from the cartridge's
/// colour data for each effect: every enemy's hit flash, frozen look and enemy-projectile colours; each enemy's own
/// effects, which recolour the row it draws in (glows, the ghost's fade, the dead Sidehopper's drain); and the effects
/// that recolour fixed rows or select another row (the statue scenes, the Ninja Pirates' gold wall, the Ceres doors, the
/// drained corpses and the n00b tube's cracks).
/// </summary>
internal sealed class EnemySheetShades
{
    /// <summary>The installed artwork the effects' colours come from.</summary>
    private readonly EnemySheetInputs inputs;

    /// <summary>The colours the set's load leaves.</summary>
    private readonly LoadedSetColours colours;

    /// <summary>Builds the shades of a set whose load leaves <paramref name="colours"/>.</summary>
    internal EnemySheetShades(EnemySheetInputs inputs, LoadedSetColours colours)
    {
        this.inputs = Ensure.NotNull(inputs);
        this.colours = Ensure.NotNull(colours);
    }

    /// <summary>The selection shades every enemy and spawned-object sheet takes: the hit flash, the frozen look and the enemy projectiles' colours.</summary>
    internal IReadOnlyList<SheetRowShade> Shared() =>
    [
        new(EnemyShades.HitFlash, EnemySheetRowDefinitions.Common, colours.SpriteColours),
        new(EnemyShades.Frozen, EnemySheetRowDefinitions.Beams, colours.With(EnemySheetRowDefinitions.Beams, 0, FrozenColours())),
        new(EnemyShades.EnemyProjectileColours, EnemySheetRowDefinitions.EnemyProjectiles, colours.SpriteColours),
    ];

    /// <summary>
    /// The shades of the effects <paramref name="enemy"/> shows by recolouring the row it draws in, <paramref name="row"/>
    /// on the sheet: Dachora's colours, the face blocks', Magdollites' and Work Robots' glows, the ghost's fade and the
    /// Shitroid victim's drain.
    /// </summary>
    internal IReadOnlyList<SheetShade> OwnRow(EnemyDefinitionId enemy, int row)
    {
        int first = row * LoadedSetColours.RowColours;
        var shades = new List<SheetShade>();
        switch (enemy)
        {
            case EnemyDefinitionId.Dachora:
            {
                DachoraColorCatalog dachora = inputs.Enemies.DachoraColors ?? throw Missing("Dachora colours");
                shades.Add(SheetShade.Run(DachoraShades.Of(DachoraPalettePhase.Default, 0), first, Frame(16, colour => dachora.Resolve(DachoraPalettePhase.Default, 0, colour))));
                for (int frame = 0; frame < DachoraColorDefinitions.AnimatedFrameCount; frame++)
                {
                    int step = frame;
                    shades.Add(SheetShade.Run(DachoraShades.Of(DachoraPalettePhase.Speed, step), first, Frame(16, colour => dachora.Resolve(DachoraPalettePhase.Speed, step, colour))));
                    shades.Add(SheetShade.Run(DachoraShades.Of(DachoraPalettePhase.Shine, step), first, Frame(16, colour => dachora.Resolve(DachoraPalettePhase.Shine, step, colour))));
                }
                break;
            }
            case EnemyDefinitionId.FaceBlock:
            {
                EnemyAuxiliaryColorCatalog auxiliary = inputs.Enemies.AuxiliaryColors ?? throw Missing("face-block glow colours");
                for (int frame = 0; frame < FaceBlockShades.GlowFrames; frame++)
                {
                    int step = frame;
                    shades.Add(SheetShade.Run(FaceBlockShades.Glow(step), first + EnemySheetRowDefinitions.GlowFirstColour,
                        Frame(4, colour => auxiliary.Resolve(EnemyAuxiliaryPalette.FaceBlock, step, colour))));
                }
                break;
            }
            case EnemyDefinitionId.Magdollite:
            {
                MagdollitePaletteCycle cycle = inputs.Enemies.MagdollitePaletteCycle ?? throw Missing("Magdollite glow colours");
                for (int frame = 0; frame < MagdollitePaletteDefinitions.FrameCount; frame++)
                {
                    int step = frame;
                    shades.Add(SheetShade.Run(MagdolliteShades.Glow(step), first + MagdollitePaletteCycle.FirstAnimatedColor,
                        Frame(MagdollitePaletteCycle.AnimatedColorCount, colour => cycle.Resolve(step, colour))));
                }
                break;
            }
            case EnemyDefinitionId.Robot:
            {
                WorkRobotPaletteCycle cycle = inputs.Enemies.WorkRobotPaletteCycle ?? throw Missing("Work Robot glow colours");
                for (int record = 0; record < WorkRobotPaletteTimingDefinitions.RecordCount; record++)
                {
                    int step = record;
                    shades.Add(SheetShade.Run(WorkRobotShades.Glow(step), first + WorkRobotPaletteDefinitions.FirstAnimatedColor,
                        Frame(WorkRobotPaletteDefinitions.ColorCount, colour => cycle.Resolve(step, colour))));
                }
                break;
            }
            case EnemyDefinitionId.Coven:
                foreach ((PictureShade shade, RgbColor[] fade) in GhostFadeSteps())
                    shades.Add(SheetShade.Run(shade, first, fade));
                break;
            case EnemyDefinitionId.Botwoon or EnemyDefinitionId.Kraid or EnemyDefinitionId.KraidArm or EnemyDefinitionId.KraidLintTop or EnemyDefinitionId.KraidLintMiddle
                or EnemyDefinitionId.KraidLintBottom or EnemyDefinitionId.KraidFoot or EnemyDefinitionId.KraidNail or EnemyDefinitionId.KraidNailBad:
                // The bosses' own effects over the row they draw in (#1512).
                shades.AddRange(SuperMetroid.AssetExtraction.Bosses.BossSheetShades.OwnRow(inputs.Bosses, enemy, first));
                break;
            case EnemyDefinitionId.CorpseSidehopper:
            {
                EnemyAuxiliaryColorCatalog auxiliary = inputs.Enemies.AuxiliaryColors ?? throw Missing("dead Sidehopper drain colours");
                for (int drain = 0; drain < TourianCorpseShades.SidehopperDrainSteps; drain++)
                {
                    int step = drain;
                    shades.Add(SheetShade.Run(TourianCorpseShades.SidehopperDrain(step), first + EnemySheetRowDefinitions.SidehopperDrainFirstColour,
                        Frame(LoadedSetColours.RowColours - EnemySheetRowDefinitions.SidehopperDrainFirstColour,
                            colour => auxiliary.Resolve(EnemyAuxiliaryPalette.DeadSidehopper, step, colour))));
                }
                break;
            }
        }
        return shades;
    }

    /// <summary>
    /// The shades of the effects <paramref name="enemy"/> shows by recolouring fixed rows, by ink, and by selecting another
    /// row, which hold wherever its sheets lie: the Chozo statues', the Tourian statue scene's, the drained corpses', the
    /// Ninja Pirates' gold wall and the Ceres doors'.
    /// </summary>
    internal (IReadOnlyList<SheetShade> Inks, IReadOnlyList<SheetRowShade> Rows) Fixed(EnemyDefinitionId enemy)
    {
        var inks = new List<SheetShade>();
        var rows = new List<SheetRowShade>();
        if (enemy == EnemyDefinitionId.Chozo)
        {
            ChozoAndTubeColorCatalog chozo = inputs.Enemies.ChozoAndTubeColors ?? throw Missing("Chozo statue colours");
            inks.Add(SheetShade.Run(ChozoAndTubeShades.WreckedShipStatue, EnemySheetRowDefinitions.ChozoAndTubeFirst * LoadedSetColours.RowColours,
                Frame(ChozoAndTubeColorDefinitions.ColorCount, colour => chozo.ResolveStatue(ChozoStatuePalette.WreckedShip, colour))));
            inks.Add(SheetShade.Run(ChozoAndTubeShades.LowerNorfairStatue, EnemySheetRowDefinitions.ChozoAndTubeFirst * LoadedSetColours.RowColours,
                Frame(ChozoAndTubeColorDefinitions.ColorCount, colour => chozo.ResolveStatue(ChozoStatuePalette.LowerNorfair, colour))));
        }
        if (enemy == EnemyDefinitionId.TourianStatue)
            inks.AddRange(TourianStatueScene());
        if (enemy == EnemyDefinitionId.CorpseSidehopper || DeadTourianPartDefinitions.Parts.ContainsKey(enemy))
        {
            ShitroidColorCatalog shitroid = inputs.Bosses.ShitroidColors ?? throw Missing("drained corpse colours");
            rows.Add(new(TourianCorpseShades.Drained, EnemySheetRowDefinitions.DrainedCorpses, colours.With(EnemySheetRowDefinitions.DrainedCorpses, 0,
                Frame(ShitroidColorDefinitions.TargetColorCount, colour => shitroid.TargetColor(ShitroidColorTarget.DeadSidehopper, colour)))));
        }
        if (enemy is EnemyDefinitionId.PirateGoldNinja or EnemyDefinitionId.PirateGreenNinja or EnemyDefinitionId.PirateGreyNinja
            or EnemyDefinitionId.PirateMagentaNinja or EnemyDefinitionId.PirateRedNinja or EnemyDefinitionId.PirateSilverNinja)
        {
            rows.Add(new(NinjaSpacePirateShades.GoldWall, EnemySheetRowDefinitions.NinjaGoldWall, colours.With(EnemySheetRowDefinitions.NinjaGoldWall, 0,
                inputs.Enemies.PaletteOf(EnemyDefinitionId.PirateGoldWall).Colours)));
        }
        if (enemy == EnemyDefinitionId.CeresDoor)
            rows.AddRange(CeresDoorLooks());
        return (inks, rows);
    }

    /// <summary>
    /// The shades every character-zero spawned-object sheet takes, for the objects any room spawns there: the n00b tube's
    /// cracks and the Ceres doors' looks, which the escape's falling tiles show.
    /// </summary>
    internal (IReadOnlyList<SheetShade> Inks, IReadOnlyList<SheetRowShade> Rows) Anywhere()
    {
        ChozoAndTubeColorCatalog tube = inputs.Enemies.ChozoAndTubeColors ?? throw Missing("n00b tube crack colours");
        return ([SheetShade.Run(ChozoAndTubeShades.TubeCracks, EnemySheetRowDefinitions.ChozoAndTubeFirst * LoadedSetColours.RowColours,
            Frame(ChozoAndTubeColorDefinitions.ColorCount, tube.ResolveTubeCracks)),
            .. SuperMetroid.AssetExtraction.Bosses.BossSheetShades.SpawnedObjects(inputs.Bosses)], CeresDoorLooks());
    }

    /// <summary>The Ceres doors' normal and escaping looks: their colours one to fifteen in the active doors' row.</summary>
    private SheetRowShade[] CeresDoorLooks()
    {
        CeresDoorVisuals door = inputs.Areas.CeresDoorVisual ?? throw Missing("Ceres door colours");
        int row = EnemySheetRowDefinitions.CeresDoorActive;
        return
        [
            new(CeresDoorShades.Normal, row, colours.With(row, 1, Frame(CeresDoorVisualDefinitions.SetupColorCount, door.NormalColor))),
            new(CeresDoorShades.Escaping, row, colours.With(row, 1, Frame(CeresDoorVisualDefinitions.SetupColorCount, door.EscapeColor))),
        ];
    }

    /// <summary>
    /// The Tourian statue scene's shades: its entrance colours in the statue and base rows, each boss's eye glow over the
    /// base's colours nine to twelve, and the sprite statues' blacked-out eyes and greys; the statues drawn in the room's
    /// tiles show theirs over the room's pictures instead.
    /// </summary>
    private IEnumerable<SheetShade> TourianStatueScene()
    {
        TourianStatuePalettes statue = inputs.Areas.TourianStatueColors ?? throw Missing("Tourian statue colours");
        int body = EnemySheetRowDefinitions.TourianStatueBody * LoadedSetColours.RowColours;
        int decoration = EnemySheetRowDefinitions.TourianStatueBase * LoadedSetColours.RowColours;
        int ridley = EnemySheetRowDefinitions.TourianStatueRidley * LoadedSetColours.RowColours;
        yield return new SheetShade(TourianStatueShades.Entrance,
            [.. Enumerable.Range(body, 16), .. Enumerable.Range(decoration, 16)],
            [.. Frame(16, statue.StatueColor), .. Frame(16, statue.BaseColor)]);
        for (int glow = 0; glow < TourianStatueShades.EyeGlows; glow++)
        {
            int boss = glow;
            yield return SheetShade.Run(TourianStatueShades.EyeGlow(boss), decoration + 9,
                Frame(TourianStatueColorDefinitions.EyeColorCount, colour => statue.ResolveEye(boss, colour)));
        }
        yield return SheetShade.Run(TourianStatueShades.PhantoonCleared, body + 12, Frame(3, _ => RgbColor.Black));
        yield return SheetShade.Run(TourianStatueShades.PhantoonGrey, body, Frame(TourianStatueColorDefinitions.GreyColorCount, statue.GreyColor));
        yield return SheetShade.Run(TourianStatueShades.RidleyCleared, ridley + 9, Frame(3, _ => RgbColor.Black));
        yield return SheetShade.Run(TourianStatueShades.RidleyGrey, ridley, Frame(TourianStatueColorDefinitions.GreyColorCount, statue.GreyColor));
    }

    /// <summary>
    /// Each step's colours the Wrecked Ship ghost's fade passes through, by its shade: run along the ghost's one path,
    /// darkened, brightened, materialized, vanished and materialized again from white, as its AI drives it.
    /// </summary>
    /// <exception cref="InvalidDataException">A step's shade names two different colourings.</exception>
    internal static IReadOnlyList<(PictureShade Shade, RgbColor[] Colours)> GhostFadeSteps()
    {
        var steps = new Dictionary<PictureShade, RgbColor[]>();
        var fade = new WreckedShipGhostFade();
        fade.Darken();
        Record(steps, fade);
        while (fade.Brighten() != 0)
            Record(steps, fade);
        for (int cycle = 0; cycle < 2; cycle++)
        {
            fade.BeginMaterializing();
            while (fade.StepTowardTarget() != 0)
                Record(steps, fade);
            fade.BeginVanishing();
            while (fade.StepTowardTarget() != 0)
                Record(steps, fade);
        }
        return [.. steps.Select(step => (step.Key, step.Value))];
    }

    /// <summary>Records the fade's colours under its shade.</summary>
    private static void Record(Dictionary<PictureShade, RgbColor[]> steps, WreckedShipGhostFade fade)
    {
        RgbColor[] shown = fade.Shown.ToArray();
        if (steps.TryGetValue(fade.Shade, out RgbColor[]? known) && !known.AsSpan().SequenceEqual(shown))
            throw new InvalidDataException($"Wrecked Ship ghost shade {fade.Shade.Key} names two colourings.");
        steps[fade.Shade] = shown;
    }

    /// <summary>The ice beam's colours, which a frozen enemy shows.</summary>
    private RgbColor[] FrozenColours()
    {
        InstalledBeamPalettes beams = inputs.BeamPalettes ?? throw Missing("beam colours");
        return Frame(16, colour => beams.ColourOf(SamusBeamCombination.Ice, colour));
    }

    /// <summary><paramref name="count"/> colours, colour after colour.</summary>
    private static RgbColor[] Frame(int count, Func<int, RgbColor> colour) => [.. Enumerable.Range(0, count).Select(colour)];

    /// <summary>The error for installed colours a shade needs and the inputs lack.</summary>
    private static InvalidDataException Missing(string what) => new($"Enemy sheet shades require the installed {what}.");
}
