using SuperMetroid.AssetExtraction.Samus.Artwork;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Animals.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Enemies.Corpses.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.FaceBlocks.Artwork;
using SuperMetroid.Core.Enemies.Magdollite.Artwork;
using SuperMetroid.Core.Enemies.SpacePirates.Artwork;
using SuperMetroid.Core.Enemies.WorkRobots;
using SuperMetroid.Core.Enemies.WorkRobots.Artwork;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Areas.Ceres.Artwork;
using SuperMetroid.AssetExtraction.Areas.Tourian.Artwork;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.Fx.Artwork;

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
    /// <summary>Each catalogued frame colouring by the sheet frame name its frame takes.</summary>
    private static readonly Lazy<Dictionary<string, FrameColouring>> FrameColourings = new(() => new Dictionary<string, FrameColouring>(
        FrameColouringDefinitions.Projectiles.Select(entry => KeyValuePair.Create(EnemySheetDefinitions.ProjectileFrameName(entry.Key), entry.Value))
            .Concat(FrameColouringDefinitions.Frames.Select(entry => KeyValuePair.Create(EnemySheetDefinitions.FrameName(entry.Key), entry.Value))),
        StringComparer.Ordinal));

    /// <summary>Each catalogued common spawner's frame colouring by the sheet frame name its frame takes.</summary>
    private static readonly Lazy<Dictionary<string, CommonSpawnerShades>> CommonFrameColourings = new(() =>
        FrameColouringDefinitions.CommonProjectiles.ToDictionary(entry => EnemySheetDefinitions.ProjectileFrameName(entry.Key), entry => entry.Value,
            StringComparer.Ordinal));

    /// <summary>The installed artwork the effects' colours come from.</summary>
    private readonly EnemySheetInputs inputs;

    /// <summary>The colours the set's load leaves.</summary>
    private readonly LoadedSetColours colours;

    /// <summary>The placements the rooms drawing under the set place each enemy with, by enemy.</summary>
    private readonly IReadOnlyDictionary<EnemyDefinitionId, IReadOnlySet<EnemySpawnSettings>> placements;

    /// <summary>
    /// Each enemy's effects by shade, built once: a sheet's frame colourings look up one shade at a time, and building
    /// an enemy's effects composes every one of its shades (the Torizos' run to thousands of fades).
    /// </summary>
    private readonly System.Collections.Concurrent.ConcurrentDictionary<EnemyDefinitionId, Lazy<Dictionary<PictureShade, SheetShade>>> effects = new();

    /// <summary>
    /// Builds the shades of a set whose load leaves <paramref name="colours"/> and whose rooms place each enemy with the
    /// <paramref name="placements"/> named for it.
    /// </summary>
    internal EnemySheetShades(EnemySheetInputs inputs, LoadedSetColours colours,
        IReadOnlyDictionary<EnemyDefinitionId, IReadOnlySet<EnemySpawnSettings>> placements)
    {
        this.inputs = Ensure.NotNull(inputs);
        this.colours = Ensure.NotNull(colours);
        this.placements = Ensure.NotNull(placements);
    }

    /// <summary>
    /// The shades laid before <paramref name="enemy"/>'s frames first show under the set, in laying order: those its
    /// encounter lays (<see cref="EnemyFirstShownColourDefinitions.Shades"/>), or those its initialization lays for the
    /// placement the set's rooms place it with (<see cref="EnemyFirstShownColourDefinitions.ByPlacement"/>, #1642); none for
    /// an enemy that shows the colours the set's load leaves.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// The enemy is catalogued both by enemy and by placement, or the set's rooms place it so that its frames would first
    /// show in two colourings: with two placements the catalogue colours apart, or with one it colours and one it does not.
    /// Its sheet under the set holds one picture per frame.
    /// </exception>
    private PictureShade[] FirstShownShades(EnemyDefinitionId enemy)
    {
        IReadOnlySet<EnemySpawnSettings> placed = placements.TryGetValue(enemy, out IReadOnlySet<EnemySpawnSettings>? settings) ? settings
            : new HashSet<EnemySpawnSettings>();
        PictureShade[][] byPlacement = [.. placed.Where(EnemyFirstShownColourDefinitions.ByPlacement.ContainsKey)
            .Select(placement => EnemyFirstShownColourDefinitions.ByPlacement[placement])];
        if (EnemyFirstShownColourDefinitions.Shades.TryGetValue(enemy, out PictureShade[]? laid))
        {
            return byPlacement.Length == 0 ? laid
                : throw new InvalidDataException($"{enemy}'s first colouring is catalogued both by enemy and by placement.");
        }
        if (byPlacement.Length == 0)
            return [];
        if (byPlacement.Length != placed.Count || byPlacement.Any(shades => !shades.SequenceEqual(byPlacement[0])))
        {
            throw new InvalidDataException(
                $"The rooms of one graphics set place {enemy} as {string.Join(", ", placed)}, which first show it in different colourings; its sheet holds one picture per frame.");
        }
        return byPlacement[0];
    }

    /// <summary>
    /// The sprite colours a sheet drawing <paramref name="enemies"/> is coloured in (#1513): the set's colours with what
    /// the shades each enemy whose encounter or initialization sets colours in code lays before it first shows
    /// (<see cref="FirstShownShades"/>) leave in the rows they write, the colours the room holds there when its frames,
    /// and the objects drawn in those rows, first show. The shades are an enemy's own effects or its fixed ones, laid in
    /// order, so a later one wins.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// An enemy's effects lack a shade the catalogue names, an enemy's first colouring is not one (see
    /// <see cref="FirstShownShades"/>), or two enemies would leave one colour two colourings.
    /// </exception>
    internal RgbColor[] FirstShownColours(IEnumerable<EnemyDefinitionId> enemies)
    {
        Ensure.NotNull(enemies);
        RgbColor[] shown = [.. colours.SpriteColours];
        var recoloured = new Dictionary<int, RgbColor>();
        var recolouredBy = new Dictionary<int, EnemyDefinitionId>();
        foreach (EnemyDefinitionId enemy in enemies.Distinct())
        {
            PictureShade[] laid = FirstShownShades(enemy);
            if (laid.Length == 0)
                continue;
            var left = new Dictionary<int, RgbColor>();
            foreach (PictureShade name in laid)
            {
                SheetShade shade = EffectOf(enemy, name);
                for (int index = 0; index < shade.Inks.Length; index++)
                    left[shade.Inks[index]] = shade.Colours[index];
            }
            foreach ((int ink, RgbColor colour) in left)
            {
                if (recoloured.TryGetValue(ink, out RgbColor other) && other != colour)
                    throw new InvalidDataException(
                        $"A sheet's enemies first show sprite colour {ink} in two colourings: {recolouredBy[ink]}'s and {enemy}'s.");
                recoloured[ink] = colour;
                recolouredBy[ink] = enemy;
                shown[ink] = colour;
            }
        }
        return shown;
    }

    /// <summary>
    /// <paramref name="sheetColours"/> with each of <paramref name="frames"/> coloured in the colouring it is drawn in
    /// (#1513): a frame whose family <see cref="FrameColouringDefinitions"/> names a colouring in that one; any other
    /// frame an enemy spawns onto a set's shared sheet with the room's graphics, which <paramref name="sharedSpawners"/>
    /// names, in each such spawner's first colouring under the set (<see cref="FirstShownShades"/>). A colouring's shades are
    /// laid over only the rows the frame's parts ink, its parts' own rows with <paramref name="paletteBits"/> ORed in, so
    /// two spawners whose colourings differ in rows their objects never ink colour one sheet. A frame neither names keeps
    /// the sheet's colours.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// A colouring names a shade its owner's effects lack, or two colourings of the sheet's frames, two spawners' of one
    /// frame among them, need one colour in two colours; the sheet holds one picture per frame, drawn in one colour per ink.
    /// </exception>
    internal RgbColor[] WithFrameColourings(RgbColor[] sheetColours, IEnumerable<FramePictures> frames, int paletteBits,
        SharedSpawnerFrames sharedSpawners)
    {
        Ensure.NotNull(sheetColours);
        Ensure.NotNull(frames);
        Ensure.NotNull(sharedSpawners);
        RgbColor[] shown = [.. sheetColours];
        var recoloured = new Dictionary<int, RgbColor>();
        var recolouredBy = new Dictionary<int, string>();
        foreach (FramePictures pictures in frames)
        {
            HashSet<int> rows = [.. pictures.Sources.SelectMany(source => source.Frame.Parts.Select(part => (part.Palette ?? source.Palette ?? 0) | paletteBits))];
            if (FrameColourings.Value.TryGetValue(pictures.Name, out FrameColouring? colouring))
                Lay([.. colouring.Shades.Select(name => EffectOf(colouring.Owner, name))], rows, $"frame {pictures.Name}'s family colouring");
            else if (CommonFrameColourings.Value.TryGetValue(pictures.Name, out CommonSpawnerShades common))
                Lay(Common([common]).Inks, rows, $"frame {pictures.Name}'s common spawner colouring");
            else
            {
                foreach (EnemyDefinitionId spawner in sharedSpawners.SpawnersOf(pictures.Name))
                {
                    PictureShade[] firstShown = FirstShownShades(spawner);
                    if (firstShown.Length > 0)
                        Lay([.. firstShown.Select(name => EffectOf(spawner, name))], rows, $"frame {pictures.Name} as {spawner} first shows it");
                }
            }
        }
        return shown;

        // The shades are laid in order, so a later one wins, over the rows the frame inks only.
        void Lay(IReadOnlyList<SheetShade> laid, HashSet<int> rows, string colouredBy)
        {
            var left = new Dictionary<int, RgbColor>();
            foreach (SheetShade shade in laid)
            {
                for (int index = 0; index < shade.Inks.Length; index++)
                {
                    if (rows.Contains(shade.Inks[index] / LoadedSetColours.RowColours))
                        left[shade.Inks[index]] = shade.Colours[index];
                }
            }
            foreach ((int ink, RgbColor colour) in left)
            {
                if (recoloured.TryGetValue(ink, out RgbColor other) && other != colour)
                    throw new InvalidDataException(
                        $"A sheet's frames are drawn with sprite colour {ink} in two colourings: {recolouredBy[ink]} and {colouredBy}.");
                recoloured[ink] = colour;
                recolouredBy[ink] = colouredBy;
                shown[ink] = colour;
            }
        }
    }

    /// <summary>
    /// The shade <paramref name="name"/> among <paramref name="enemy"/>'s effects: its own row's, its fixed ones and those
    /// of the objects it spawns.
    /// </summary>
    /// <exception cref="InvalidDataException">None of them is named so.</exception>
    private SheetShade EffectOf(EnemyDefinitionId enemy, PictureShade name)
    {
        return effects.GetOrAdd(enemy, key => new(() => EffectsOf(key))).Value.TryGetValue(name, out SheetShade? shade) ? shade
            : throw new InvalidDataException($"{enemy}'s effects have no shade {name.Key} to colour its frames in.");
    }

    /// <summary>
    /// <paramref name="enemy"/>'s effects by shade, in order: its own row's, its fixed ones and those of the objects it
    /// spawns, the first of any shade named twice standing for it.
    /// </summary>
    private Dictionary<PictureShade, SheetShade> EffectsOf(EnemyDefinitionId enemy)
    {
        var byShade = new Dictionary<PictureShade, SheetShade>();
        foreach (SheetShade shade in OwnRow(enemy, colours.RowOf(enemy)).Concat(Fixed(enemy).Inks).Concat(SpawnedBy(enemy).Inks))
            byShade.TryAdd(shade.Shade, shade);
        return byShade;
    }

    /// <summary>
    /// The selection shades every spawned-object sheet takes, and every enemy sheet whose enemy's draws select them
    /// (<see cref="SharedSelectedBy"/>): the hit flash, the frozen look and the enemy projectiles' colours.
    /// </summary>
    internal IReadOnlyList<SheetRowShade> Shared() =>
    [
        new(EnemyShades.HurtFlash, EnemySheetRowDefinitions.Common, colours.SpriteColours),
        new(EnemyShades.Frozen, EnemySheetRowDefinitions.Beams, colours.With(EnemySheetRowDefinitions.Beams, 0, FrozenColours())),
        new(EnemyShades.ProjectileLook, EnemySheetRowDefinitions.EnemyProjectiles, colours.SpriteColours),
    ];

    /// <summary>
    /// The selection shades <paramref name="enemy"/>'s sheets take (#1643): the shared ones (<see cref="Shared"/>) but those
    /// its draws never select (<see cref="EnemyUnselectedShadeDefinitions.NeverSelected"/>), which no draw of it shows.
    /// </summary>
    /// <exception cref="InvalidDataException">The catalogue names a shade for the enemy that is no shared one.</exception>
    internal IReadOnlyList<SheetRowShade> SharedSelectedBy(EnemyDefinitionId enemy)
    {
        IReadOnlyList<SheetRowShade> shared = Shared();
        if (!EnemyUnselectedShadeDefinitions.NeverSelected.TryGetValue(enemy, out PictureShade[]? unselected))
            return shared;
        foreach (PictureShade shade in unselected)
        {
            if (!shared.Any(entry => entry.Shade == shade))
                throw new InvalidDataException($"{enemy}'s draws are catalogued as never selecting shade {shade.Key}, which is no shared enemy shade.");
        }
        return [.. shared.Where(entry => !unselected.Contains(entry.Shade))];
    }

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
                shades.Add(SheetShade.Run(DachoraShades.Of(DachoraColourPhase.Default, 0), first, Frame(16, colour => dachora.Resolve(DachoraColourPhase.Default, 0, colour))));
                for (int frame = 0; frame < DachoraColorDefinitions.AnimatedFrameCount; frame++)
                {
                    int step = frame;
                    shades.Add(SheetShade.Run(DachoraShades.Of(DachoraColourPhase.Speed, step), first, Frame(16, colour => dachora.Resolve(DachoraColourPhase.Speed, step, colour))));
                    shades.Add(SheetShade.Run(DachoraShades.Of(DachoraColourPhase.Shine, step), first, Frame(16, colour => dachora.Resolve(DachoraColourPhase.Shine, step, colour))));
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
                for (int frame = 0; frame < MagdolliteShades.GlowFrames; frame++)
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
                for (int record = 0; record < WorkRobotGlowDefinitions.StepCount; record++)
                {
                    int step = record;
                    shades.Add(SheetShade.Run(WorkRobotShades.Glow(step), first + WorkRobotPaletteDefinitions.FirstAnimatedColor,
                        Frame(WorkRobotPaletteDefinitions.ColorCount, colour => cycle.Resolve(step, colour))));
                }
                break;
            }
            case EnemyDefinitionId.ShutterDestroyable:
                // The first escape room's shutters show the escape's red flash over their own row.
                shades.AddRange(PaletteFxSpriteShades.Of(PaletteFxObject.TourianShutterRedFlashing, PaletteFxColours.TourianShutters,
                    first, (inputs.Maps ?? throw Missing("Tourian shutter flash colours")).RoomPaletteFx));
                break;
            case EnemyDefinitionId.Coven:
                foreach (ShadeColours step in GhostFadeSteps())
                    shades.Add(SheetShade.Run(step.Shade, first, step.Colours));
                break;
            case EnemyDefinitionId.BombTorizo or EnemyDefinitionId.GoldenTorizo or EnemyDefinitionId.BombTorizoOrb or EnemyDefinitionId.GoldenTorizoOrb:
                // The Torizos' colourings over their own rows (#1512).
                shades.AddRange(SuperMetroid.AssetExtraction.Bosses.TorizoSheetShades.Of(inputs.Enemies.AuxiliaryColors ?? throw Missing("Golden Torizo health colours"),
                    (inputs.Maps ?? throw Missing("Torizo belly glow colours")).RoomPaletteFx,
                    enemy is EnemyDefinitionId.GoldenTorizo or EnemyDefinitionId.GoldenTorizoOrb ? PaletteFxObject.GoldenTorizoBelly : PaletteFxObject.BombTorizoBelly));
                break;
            case EnemyDefinitionId.SporeSpawn or EnemyDefinitionId.DraygonBody or EnemyDefinitionId.DraygonEye or EnemyDefinitionId.DraygonTail or EnemyDefinitionId.DraygonArms
                or EnemyDefinitionId.Crocomire or EnemyDefinitionId.CrocomireTongue or EnemyDefinitionId.Botwoon or EnemyDefinitionId.Kraid or EnemyDefinitionId.KraidArm or EnemyDefinitionId.KraidLintTop or EnemyDefinitionId.KraidLintMiddle
                or EnemyDefinitionId.KraidLintBottom or EnemyDefinitionId.KraidFoot or EnemyDefinitionId.KraidNail or EnemyDefinitionId.KraidNailBad:
                // The bosses' own effects over the row they draw in (#1512).
                shades.AddRange(SuperMetroid.AssetExtraction.Bosses.BossSheetShades.OwnRow(inputs.Bosses, enemy, first));
                break;
            case EnemyDefinitionId.RidleyCeres or EnemyDefinitionId.Ridley or EnemyDefinitionId.RidleyExplosion or EnemyDefinitionId.CeresDoor:
                // The Ridley encounters' colour loads over Ridley's, the doors', the container's and the Baby's rows (#1512).
                shades.AddRange(SuperMetroid.AssetExtraction.Bosses.RidleySheetShades.Sprites(inputs.Maps?.CeresRidleyColors, inputs.Bosses));
                break;
            case EnemyDefinitionId.MotherBrainHead or EnemyDefinitionId.MotherBrainBody or EnemyDefinitionId.BabyMetroidCutscene:
                // Mother Brain's encounter's colour loads over her brain's, attack's, legs' and the Baby's rows (#1512).
                SuperMetroid.AssetExtraction.Bosses.MotherBrainSheetShades.RequireDrawRow(enemy, row);
                shades.AddRange(SuperMetroid.AssetExtraction.Bosses.MotherBrainSheetShades.Sprites(inputs.Maps, inputs.Bosses));
                break;
            case EnemyDefinitionId.BabyMetroid:
                SuperMetroid.AssetExtraction.Bosses.MotherBrainSheetShades.RequireDrawRow(enemy, row);
                shades.AddRange(SuperMetroid.AssetExtraction.Bosses.MotherBrainSheetShades.Shitroid(inputs.Bosses));
                break;
            case EnemyDefinitionId.Zebetite:
                SuperMetroid.AssetExtraction.Bosses.MotherBrainSheetShades.RequireDrawRow(enemy, row);
                shades.AddRange(SuperMetroid.AssetExtraction.Bosses.MotherBrainSheetShades.Zebetites(inputs.Bosses));
                break;
            case EnemyDefinitionId.CorpseSidehopper:
            {
                // The Shitroid's victim shows the encounter's entry colours before it is drained (#1512).
                shades.AddRange(SuperMetroid.AssetExtraction.Bosses.MotherBrainSheetShades.Shitroid(inputs.Bosses));
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
            case EnemyDefinitionId.None or EnemyDefinitionId.Boyon or EnemyDefinitionId.Stoke or EnemyDefinitionId.MamaTurtle
                or EnemyDefinitionId.BabyTurtle or EnemyDefinitionId.Puyo or EnemyDefinitionId.Cacatac or EnemyDefinitionId.Owtch
                or EnemyDefinitionId.ShipTop or EnemyDefinitionId.ShipBottomEntrance or EnemyDefinitionId.Mellow or EnemyDefinitionId.Mella
                or EnemyDefinitionId.Menu or EnemyDefinitionId.Multiviola or EnemyDefinitionId.LavaRocks or EnemyDefinitionId.Rinka
                or EnemyDefinitionId.Rio or EnemyDefinitionId.Squeept or EnemyDefinitionId.Geruta or EnemyDefinitionId.Holtz or EnemyDefinitionId.Oum
                or EnemyDefinitionId.Choot or EnemyDefinitionId.GRipper or EnemyDefinitionId.Ripper2 or EnemyDefinitionId.Ripper
                or EnemyDefinitionId.Dragon or EnemyDefinitionId.ShutterGrowing or EnemyDefinitionId.ShutterShootable
                or EnemyDefinitionId.ShutterHorizShootable or EnemyDefinitionId.Kamer or EnemyDefinitionId.Waver or EnemyDefinitionId.Metaree
                or EnemyDefinitionId.Fireflea or EnemyDefinitionId.Skultera or EnemyDefinitionId.Elevator or EnemyDefinitionId.Sciser
                or EnemyDefinitionId.Zero or EnemyDefinitionId.Tripper or EnemyDefinitionId.Kamer2 or EnemyDefinitionId.Sbug
                or EnemyDefinitionId.Sbug2 or EnemyDefinitionId.Mochtroid or EnemyDefinitionId.Sidehopper or EnemyDefinitionId.Dessgeega
                or EnemyDefinitionId.SidehopperLarge or EnemyDefinitionId.SidehopperTourian or EnemyDefinitionId.DessgeegaLarge
                or EnemyDefinitionId.Zoa or EnemyDefinitionId.Viola or EnemyDefinitionId.Respawn or EnemyDefinitionId.Skree
                or EnemyDefinitionId.Yard or EnemyDefinitionId.Reflec or EnemyDefinitionId.HZoomer or EnemyDefinitionId.Zeela
                or EnemyDefinitionId.Sova or EnemyDefinitionId.Zoomer or EnemyDefinitionId.MZoomer or EnemyDefinitionId.Metroid
                or EnemyDefinitionId.SporeSpawnStalk or EnemyDefinitionId.Boulder or EnemyDefinitionId.KzanTop or EnemyDefinitionId.KzanBottom
                or EnemyDefinitionId.Hibashi or EnemyDefinitionId.Puromi or EnemyDefinitionId.MiniKraid or EnemyDefinitionId.Steam
                or EnemyDefinitionId.PhantoonBody or EnemyDefinitionId.PhantoonEye or EnemyDefinitionId.PhantoonTentacles
                or EnemyDefinitionId.PhantoonMouth or EnemyDefinitionId.Etecoon or EnemyDefinitionId.Evir or EnemyDefinitionId.EvirProjectile
                or EnemyDefinitionId.Eye or EnemyDefinitionId.Fune or EnemyDefinitionId.Namihe or EnemyDefinitionId.YappingMaw
                or EnemyDefinitionId.Kago or EnemyDefinitionId.Beetom or EnemyDefinitionId.Powamp or EnemyDefinitionId.RobotNoPower
                or EnemyDefinitionId.Bull or EnemyDefinitionId.Alcoon or EnemyDefinitionId.Atomic or EnemyDefinitionId.Spark
                or EnemyDefinitionId.KihunterGreen or EnemyDefinitionId.KihunterGreenWings or EnemyDefinitionId.KihunterYellow
                or EnemyDefinitionId.KihunterYellowWings or EnemyDefinitionId.KihunterRed or EnemyDefinitionId.KihunterRedWings
                or EnemyDefinitionId.MotherBrainTubes or EnemyDefinitionId.CorpseTorizo or EnemyDefinitionId.CorpseSidehopper2
                or EnemyDefinitionId.CorpseZoomer or EnemyDefinitionId.CorpseRipper or EnemyDefinitionId.CorpseSkree
                or EnemyDefinitionId.TourianStatue or EnemyDefinitionId.TourianStatueGhost or EnemyDefinitionId.Shaktool
                or EnemyDefinitionId.NoobTubeCrack or EnemyDefinitionId.Chozo or EnemyDefinitionId.Zeb
                or EnemyDefinitionId.Zebbo or EnemyDefinitionId.Gamet or EnemyDefinitionId.Geega or EnemyDefinitionId.EtecoonEscape
                or EnemyDefinitionId.DachoraEscape or EnemyDefinitionId.PirateGreyWall or EnemyDefinitionId.PirateGreenWall
                or EnemyDefinitionId.PirateRedWall or EnemyDefinitionId.PirateGoldWall or EnemyDefinitionId.PirateMagentaWall
                or EnemyDefinitionId.PirateSilverWall or EnemyDefinitionId.PirateGreyNinja or EnemyDefinitionId.PirateGreenNinja
                or EnemyDefinitionId.PirateRedNinja or EnemyDefinitionId.PirateGoldNinja or EnemyDefinitionId.PirateMagentaNinja
                or EnemyDefinitionId.PirateSilverNinja or EnemyDefinitionId.PirateGreyWalking or EnemyDefinitionId.PirateGreenWalking
                or EnemyDefinitionId.PirateRedWalking or EnemyDefinitionId.PirateGoldWalking or EnemyDefinitionId.PirateMagentaWalking
                or EnemyDefinitionId.PirateSilverWalking:
                // Every other enemy shows no effect over the row it draws in.
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(enemy), enemy, "Undefined enemy definition.");
        }
        return shades;
    }

    /// <summary>
    /// The shades of the effects <paramref name="enemy"/> shows by recolouring fixed rows, by ink, and by selecting another
    /// row, which hold wherever its sheets lie: the Chozo statues', the drained corpses', the Ninja Pirates' gold wall and
    /// the Ceres doors'.
    /// </summary>
    internal SheetShadeSet Fixed(EnemyDefinitionId enemy)
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
        return new SheetShadeSet(inks, rows);
    }

    /// <summary>
    /// The shades of the objects <paramref name="enemy"/> spawns (#1513): the colours the bosses' fights lay over their
    /// objects' rows (Kraid's rocks, Botwoon's, Draygon's and Spore Spawn's sprites, Crocomire's wall and projectiles), and
    /// the Ridleys' and Mother Brain's sprite colours, which their objects draw in; and the Tourian statue scene's looks, which
    /// the entrance statue's decorations and sprite statues show on the shared sheet they spawn onto (#1534); none for any
    /// other enemy.
    /// </summary>
    internal SheetShadeSet SpawnedBy(EnemyDefinitionId enemy)
    {
        IEnumerable<SheetShade> inks = SuperMetroid.AssetExtraction.Bosses.BossSheetShades.SpawnedObjects(inputs.Bosses, enemy);
        if (enemy == EnemyDefinitionId.TourianStatue)
            inks = inks.Concat(TourianStatueScene());
        if (enemy is EnemyDefinitionId.Ridley or EnemyDefinitionId.RidleyCeres or EnemyDefinitionId.RidleyExplosion)
            inks = inks.Concat(RidleySprites());
        if (enemy is EnemyDefinitionId.MotherBrainBody or EnemyDefinitionId.MotherBrainHead or EnemyDefinitionId.MotherBrainTubes
            or EnemyDefinitionId.BabyMetroidCutscene)
            inks = inks.Concat(SuperMetroid.AssetExtraction.Bosses.MotherBrainSheetShades.Sprites(inputs.Maps, inputs.Bosses));
        return new SheetShadeSet([.. inks], SuperMetroid.AssetExtraction.Bosses.BossSheetShades.SpawnedRows(inputs.Bosses, colours, enemy));
    }

    /// <summary>
    /// The shades the objects of a common spawner show on the shared sheet (#1513), as its presence names them: the n00b
    /// tube's cracks, the Ceres elevator concealer's cleared colour, the Ceres doors' looks the escape's falling tiles show,
    /// and Ridley's sprite colours.
    /// </summary>
    internal SheetShadeSet Common(IEnumerable<CommonSpawnerShades> named)
    {
        Ensure.NotNull(named);
        var inks = new List<SheetShade>();
        var rows = new List<SheetRowShade>();
        foreach (CommonSpawnerShades shades in named)
        {
            switch (shades)
            {
                case CommonSpawnerShades.TubeCracks:
                    ChozoAndTubeColorCatalog tube = inputs.Enemies.ChozoAndTubeColors ?? throw Missing("n00b tube crack colours");
                    inks.Add(SheetShade.Run(ChozoAndTubeShades.TubeCracks, EnemySheetRowDefinitions.ChozoAndTubeFirst * LoadedSetColours.RowColours,
                        Frame(ChozoAndTubeColorDefinitions.ColorCount, tube.ResolveTubeCracks)));
                    break;
                case CommonSpawnerShades.CeresElevatorConcealer:
                    // A new game clears the enemy projectiles' last colour, the Ceres elevator concealer's solid pixels.
                    inks.Add(SheetShade.Run(CeresElevatorShades.ConcealerCleared,
                        EnemySheetRowDefinitions.EnemyProjectiles * LoadedSetColours.RowColours + CeresConcealerColour, [RgbColor.Black]));
                    break;
                case CommonSpawnerShades.CeresDoorLooks:
                    rows.AddRange(CeresDoorLooks());
                    break;
                case CommonSpawnerShades.RidleySprites:
                    inks.AddRange(RidleySprites());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(named), shades, "Undefined common spawner shades.");
            }
        }
        return new SheetShadeSet(inks, rows);
    }

    /// <summary>The Ridley encounters' sprite colours.</summary>
    private IReadOnlyList<SheetShade> RidleySprites() =>
        SuperMetroid.AssetExtraction.Bosses.RidleySheetShades.Sprites(inputs.Maps?.CeresRidleyColors, inputs.Bosses);

    /// <summary>The enemy projectiles' colour the Ceres elevator concealer's solid pixels show.</summary>
    private const int CeresConcealerColour = 15;

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
        // The sprite statues grey out through palette-FX records over their own rows, Ridley's and the statue body's.
        IPaletteFxColorSource paletteFx = (inputs.Maps ?? throw Missing("Tourian statue grey-out colours")).RoomPaletteFx;
        foreach (SheetShade greyOut in PaletteFxSpriteShades.Of(PaletteFxObject.GreyOutTourianStatueRidley, PaletteFxColours.RidleyStatue,
                     ridley, paletteFx))
            yield return greyOut;
        foreach (SheetShade greyOut in PaletteFxSpriteShades.Of(PaletteFxObject.GreyOutTourianStatuePhantoon, PaletteFxColours.PhantoonStatue,
                     body, paletteFx))
            yield return greyOut;
    }

    /// <summary>
    /// Each step's colours the Wrecked Ship ghost's fade passes through, by its shade: run along the ghost's one path,
    /// darkened, brightened, materialized, vanished and materialized again from white, as its AI drives it.
    /// </summary>
    /// <exception cref="InvalidDataException">A step's shade names two different colourings.</exception>
    internal static IReadOnlyList<ShadeColours> GhostFadeSteps()
    {
        var steps = new Dictionary<PictureShade, RgbColor[]>();
        WreckedShipGhostFade fade = WreckedShipGhostFade.Dark;
        Record(steps, fade);
        for (WreckedShipGhostFadeStep step = fade.Brightened(); step.Changes != 0; step = fade.Brightened())
        {
            fade = step.Next;
            Record(steps, fade);
        }
        for (int cycle = 0; cycle < 2; cycle++)
        {
            fade = fade.Materializing(WreckedShipGhostAppearance.Colours);
            fade = RecordFade(steps, fade);
            fade = fade.Vanishing();
            fade = RecordFade(steps, fade);
        }
        return [.. steps.Select(step => new ShadeColours(step.Key, step.Value))];
    }

    /// <summary>Steps <paramref name="fade"/> toward its target until a step moves nothing, recording each step's colours.</summary>
    /// <returns>The fade at its target.</returns>
    private static WreckedShipGhostFade RecordFade(Dictionary<PictureShade, RgbColor[]> steps, WreckedShipGhostFade fade)
    {
        for (WreckedShipGhostFadeStep step = fade.SteppedTowardTarget(); step.Changes != 0; step = fade.SteppedTowardTarget())
        {
            fade = step.Next;
            Record(steps, fade);
        }
        return fade;
    }

    /// <summary>Records the fade's colours under its shade.</summary>
    private static void Record(Dictionary<PictureShade, RgbColor[]> steps, WreckedShipGhostFade fade)
    {
        RgbColor[] shown = [.. fade.Shown];
        PictureShade shade = WreckedShipGhostFadeShade.Of(fade);
        if (steps.TryGetValue(shade, out RgbColor[]? known) && !known.SequenceEqual(shown))
            throw new InvalidDataException($"Wrecked Ship ghost shade {shade.Key} names two colourings.");
        steps[shade] = shown;
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
