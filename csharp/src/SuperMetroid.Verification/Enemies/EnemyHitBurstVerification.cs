using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Enemies.SpriteObjects;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Progression;
namespace SuperMetroid.Verification.Enemies;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>
/// Checks the burst the ordinary shot reaction shows at an enemy a shot or bomb hurts (#1713): when it spawns, where, the
/// frames it shows and for how long, and the sprite-object place it takes, which Kraid's mouth pass and a Puyo's ceiling
/// strike read.
/// </summary>
internal static class EnemyHitBurstVerification
{
    /// <summary>Where every case's enemy, shot and bomb lie.</summary>
    private static readonly Position16<RoomPixels> Place = new(256, 256);

    /// <summary>The frames the burst shows on each update from its spawn until it ends, two updates each.</summary>
    private static readonly RoomSpriteObjectFrame[] BurstFrames =
    [
        RoomSpriteObjectFrame.EnemyHit0, RoomSpriteObjectFrame.EnemyHit0,
        RoomSpriteObjectFrame.EnemyHit1, RoomSpriteObjectFrame.EnemyHit1,
        RoomSpriteObjectFrame.EnemyHit2, RoomSpriteObjectFrame.EnemyHit2,
        RoomSpriteObjectFrame.EnemyHit3, RoomSpriteObjectFrame.EnemyHit3,
    ];

    /// <summary>
    /// A beam on a Mellow through the ordinary shot reaction: a hit that hurts it shows one burst, spawned at the Mellow's
    /// position with the room's graphics, taking the highest free sprite-object place, also when the hit kills it or a lethal
    /// Ice hit freezes it instead; a hit on a frozen Mellow, a hit it is immune to and a Skree's hit, whose own reaction runs
    /// the common damage without the burst, show none.
    /// </summary>
    internal static void VerifyShotHitBursts()
    {
        RoomEnemyDefinition mellow = RoomEnemyDefinitionCatalog.ById[EnemyDefinitionId.Mellow];
        foreach ((string kind, EnemyDefinitionId id, RoomEnemyDefinition definition, int health, int frozenTimer, SamusBeamCombination beam, bool shows) in new[]
        {
            ("hurt", EnemyDefinitionId.Mellow, mellow, 1000, 0, SamusBeamCombination.Power, true),
            ("killed", EnemyDefinitionId.Mellow, mellow, 1, 0, SamusBeamCombination.Power, true),
            ("frozen instead of killed", EnemyDefinitionId.Mellow, mellow, 1, 0, SamusBeamCombination.Ice, true),
            ("frozen", EnemyDefinitionId.Mellow, mellow, 1000, 300, SamusBeamCombination.Power, false),
            ("immune", EnemyDefinitionId.Mellow, mellow with { Vulnerability = EnemyVulnerabilityDefinitions.Invulnerable }, 1000, 0, SamusBeamCombination.Power, false),
            ("Skree", EnemyDefinitionId.Skree, RoomEnemyDefinitionCatalog.ById[EnemyDefinitionId.Skree], 1000, 0, SamusBeamCombination.Power, false),
        })
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot) = RoomWith(id, definition, health, frozenTimer);
            (SamusProjectileSystem shots, SamusBombProjectileSystem bombs) = ShotAt(beam);
            AssertEqual(1, enemies.Collision.ResolveOrdinaryProjectileHits(shots, bombs, SuperMetroid.Core.Progression.AreaId.Crateria, samus: null, onlyEnemy: slot), $"{kind}: the beam hits the enemy");
            AssertTrue(kind == "immune" ? slot.Health == health : slot.Health < health || slot.FrozenTimer != frozenTimer,
                $"{kind}: the hit's damage or freeze lands");
            if (kind == "frozen instead of killed")
                AssertEqual(400, slot.FrozenTimer, "the lethal Ice hit freezes the Mellow instead of killing it");
            if (kind == "killed")
                AssertEqual(0, slot.Health, "the lethal hit kills the Mellow");
            RoomSpriteObject[] bursts = Bursts(enemies);
            AssertEqual(shows ? 1 : 0, bursts.Length, $"{kind}: the bursts the hit shows");
            if (shows)
                AssertBurst(enemies, bursts[0], kind);
        }
        Console.WriteLine("  Shot hit burst: a hurting hit shows one burst at the enemy, lethal and Ice-frozen hits too; frozen, immune and Skree hits show none.");
    }

    /// <summary>
    /// An exploding bomb on a Mellow through the bomb pass runs the same shot reaction: a bomb that hurts it shows one burst
    /// at the Mellow; a frozen Mellow, an immune one and a Skree show none.
    /// </summary>
    internal static void VerifyBombHitBursts()
    {
        RoomEnemyDefinition mellow = RoomEnemyDefinitionCatalog.ById[EnemyDefinitionId.Mellow];
        foreach ((string kind, EnemyDefinitionId id, RoomEnemyDefinition definition, int frozenTimer, bool shows) in new[]
        {
            ("hurt", EnemyDefinitionId.Mellow, mellow, 0, true),
            ("frozen", EnemyDefinitionId.Mellow, mellow, 300, false),
            ("immune", EnemyDefinitionId.Mellow, mellow with { Vulnerability = EnemyVulnerabilityDefinitions.Invulnerable }, 0, false),
            ("Skree", EnemyDefinitionId.Skree, RoomEnemyDefinitionCatalog.ById[EnemyDefinitionId.Skree], 0, false),
        })
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot) = RoomWith(id, definition, health: 1000, frozenTimer);
            var bombs = new SamusBombProjectileSystem();
            SamusBombProjectileSlot bomb = bombs.Slots[0];
            bomb.Type = SamusBombProjectileSystem.NormalBombType;
            bomb.Damage = 30;
            bomb.Radius = new(4, 4);
            bomb.Position = FixedPosition<RoomPixels>.FromWhole(Place);
            (SamusProjectileSystem shots, _) = ShotAt(SamusBeamCombination.Power);
            shots.Slots[0].Type = SamusProjectileType.None;
            AssertEqual(1, enemies.Collision.ResolveOrdinaryBombHits(bombs, shots, samus: null, onlyEnemy: slot), $"{kind} bomb: the bomb hits the enemy");
            AssertEqual(kind == "immune" ? 1000 : 1000 - 30, slot.Health, $"{kind} bomb: the enemy's health after the bomb");
            RoomSpriteObject[] bursts = Bursts(enemies);
            AssertEqual(shows ? 1 : 0, bursts.Length, $"{kind} bomb: the bursts the bomb shows");
            if (shows)
                AssertBurst(enemies, bursts[0], $"{kind} bomb");
        }
        Console.WriteLine("  Bomb hit burst: a bomb that hurts an enemy shows the burst; frozen, immune and Skree hits show none.");
    }

    /// <summary>
    /// The Pirates' shot reactions decide the burst by the reaction a hit runs: the common reaction shows it on every
    /// Pirate, the Pirates' own on every Pirate but the gold Ninja, and both Ninja body reactions on none.
    /// </summary>
    internal static void VerifyPirateHitBursts()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(),
            definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        RoomEnemySlot silver = enemies.SlotTable.Slots[0];
        silver.Bind(EnemyDefinitionId.PirateSilverNinja, RoomEnemyDefinitionLookup.Get(EnemyDefinitionId.PirateSilverNinja));
        RoomEnemySlot gold = enemies.SlotTable.Slots[1];
        gold.Bind(EnemyDefinitionId.PirateGoldNinja, RoomEnemyDefinitionLookup.Get(EnemyDefinitionId.PirateGoldNinja));
        foreach ((RoomEnemySlot pirate, EnemyReaction reaction, bool shows) in new[]
        {
            (silver, EnemyReaction.NormalEnemyShot, true),
            (gold, EnemyReaction.NormalEnemyShot, true),
            (silver, SpacePirateReactions.Shot, true),
            (gold, SpacePirateReactions.Shot, false),
            (silver, SpacePirateReactions.GoldNinjaVulnerableHitboxShot, false),
            (silver, SpacePirateReactions.GoldNinjaInvincibleHitboxShot, false),
            (gold, SpacePirateReactions.GoldNinjaVulnerableHitboxShot, false),
        })
        {
            AssertEqual(shows, PirateHitboxShotActions.ShowsHitBurst(pirate, reaction), $"{pirate.DefinitionId} through {reaction} shows the burst");
        }
        Console.WriteLine("  Pirate hit burst: the common and Pirate reactions show it, but not on the gold Ninja nor through the Ninja body reactions.");
    }

    /// <summary>
    /// The burst takes a sprite-object place like every sprite object, so a dud flash after it takes the next place down,
    /// which is what Kraid's mouth pass leaves as its bound; it ends after its eight updates, freeing its place for the next
    /// spawn; and drawn inside the view it is a sprite-object draw that leaves a Puyo's ceiling strike not turning back,
    /// drawn as one of Samus's draws from the installed burst picture on the sheet of her equipped beams (#1713).
    /// </summary>
    internal static void VerifyHitBurstPlaces()
    {
        (RoomEnemySystem enemies, RoomEnemySlot slot) = RoomWith(EnemyDefinitionId.Mellow, RoomEnemyDefinitionCatalog.ById[EnemyDefinitionId.Mellow], 1000, 0);
        (SamusProjectileSystem shots, SamusBombProjectileSystem bombs) = ShotAt(SamusBeamCombination.Power);
        AssertEqual(1, enemies.Collision.ResolveOrdinaryProjectileHits(shots, bombs, SuperMetroid.Core.Progression.AreaId.Crateria, samus: null, onlyEnemy: slot), "the beam hurts the Mellow");
        RoomSpriteObject burst = Bursts(enemies).Single();
        AssertEqual<int?>(SpriteObjectPlaces.PlaceCount - 1, enemies.SpriteObjects.PlaceOf(burst), "the burst takes the highest place");

        RoomSpriteObject dud = SpawnDud(enemies);
        int? dudPlace = enemies.SpriteObjects.PlaceOf(dud);
        AssertEqual<int?>(SpriteObjectPlaces.PlaceCount - 2, dudPlace, "a dud flash after the burst takes the next place down");
        KraidPassBounds bounds = KraidPassBounds.Placed(left: 0, top: 0, bottom: 0);
        bounds.LeaveAfterDud(dudPlace, Place);
        AssertEqual((SpriteObjectPlaces.PlaceCount - 2) * 2, bounds.Bottom, "Kraid's mouth pass reads twice the place the burst left its dud");

        // Drawn inside the view, the burst writes the value a Puyo's ceiling strike reads, which the HUD's redraw had left
        // naming its table, so a strike no longer turns the Puyo back.
        enemies.PuyoCeilingTurn.HudRedrawn();
        AssertTrue(enemies.PuyoCeilingTurn.TurnsBack, "after the HUD's redraw a strike turns the Puyo back");
        var camera = new Position16<RoomPixels>(Place.X - 128, Place.Y - 112);
        var drawn = new SpriteFrameBuffer();
        drawn.BeginFrame();
        dud.End();
        enemies.SpriteObjects.DrawRoomSpriteObjects(drawn, camera, HitBurstArtwork(SamusBeamCombination.Power));
        drawn.FinalizeFrame();
        AssertTrue(!enemies.PuyoCeilingTurn.TurnsBack, "the burst's draw inside the view leaves a strike not turning back");
        ImageSpriteDraw draw = drawn.Draws.OfType<ImageSpriteDraw>().Single(candidate => candidate.Source.Kind == SpriteSourceKind.SamusHitBurst);
        AssertEqual(new SpriteSource(SpriteSourceKind.SamusHitBurst, 0), draw.Source, "the burst is the first live sprite object drawn");
        AssertEqual(Place.ToScreen(camera), draw.Origin, "the burst draws centred on the enemy's position");
        SpriteSheetFrame installed = ProjectileSheets.Gameplay(InstalledFixtures.SpriteSheets).For(SamusBeamCombination.Power)
            .Frame(ProjectileSheetFrames.HitBurstFrame(HitBurstSpriteDefinitions.All[0]));
        AssertEqual(installed, draw.Image, "the burst draws its first installed picture from the sheet of Samus's equipped beams");
        AssertTrue(!draw.Image.IsEmpty, "the burst's first picture shows its pieces");

        for (int update = 1; update <= BurstFrames.Length; update++)
            enemies.SpriteObjects.StepRoomSpriteObjects();
        AssertTrue(!burst.IsActive, "the burst has ended after its eight updates");
        RoomSpriteObject next = SpawnDud(enemies);
        AssertEqual<int?>(SpriteObjectPlaces.PlaceCount - 1, enemies.SpriteObjects.PlaceOf(next), "the ended burst's place is the next one taken");
        Console.WriteLine("  Hit burst places: the burst takes a place before a later dud, frees it as it ends, and its draw is a Puyo-visible sprite-object draw.");
    }

    /// <summary>
    /// The burst is one of Samus's weapon effects (#1713): with each beam combination the game shows a beam colouring for
    /// equipped, retail and glitched, and under the grapple's, the Crystal Flash's, the Hyper Beam cycle's and the death
    /// whiteout's colourings laid over her beams, it draws from the sheet of her equipped beams as one of her draws, so it
    /// shows her shading, and every colour it shows is the colour her beams show at the place the burst's pixel was coloured
    /// from. Power and Plasma show it in different colours.
    /// </summary>
    internal static void VerifyHitBurstColouring()
    {
        (RoomEnemySystem enemies, RoomEnemySlot slot) = RoomWith(EnemyDefinitionId.Mellow, RoomEnemyDefinitionCatalog.ById[EnemyDefinitionId.Mellow], 1000, 0);
        (SamusProjectileSystem shots, SamusBombProjectileSystem bombs) = ShotAt(SamusBeamCombination.Power);
        AssertEqual(1, enemies.Collision.ResolveOrdinaryProjectileHits(shots, bombs, SuperMetroid.Core.Progression.AreaId.Crateria, samus: null, onlyEnemy: slot), "the beam hurts the Mellow");
        _ = Bursts(enemies).Single();
        var camera = new Position16<RoomPixels>(Place.X - 128, Place.Y - 112);
        // Every beam combination the game shows a beam colouring for, retail and glitched: the import's beam shades.
        var cases = new List<(string Name, SamusBeamCombination Equipped, Action<SamusColours> Lay)>();
        foreach (SamusBeamCombination beams in SuperMetroid.AssetExtraction.Samus.SamusBeamColours.Shown)
            cases.Add(($"{beams} equipped", beams, _ => { }));
        cases.Add(("the grapple firing", SamusBeamCombination.Power,
            colours => colours.ShowBeams(SamusShades.Beam(GrappleBeamDefinitions.Palettes.FiringSelection))));
        cases.Add(("the Crystal Flash", SamusBeamCombination.Plasma, colours =>
        {
            colours.LayOverBeams(SamusShades.CrystalFlashBody(1));
            colours.LayOverBeams(SamusShades.CrystalFlashBubble(2));
        }));
        cases.Add(("the Hyper Beam cycle", SamusBeamCombination.Power, colours => colours.LayOverBeams(SamusShades.HyperBeamCycle(3))));
        cases.Add(("the death whiteout", SamusBeamCombination.Wave, colours => colours.ShowBeams(SamusShades.DeathWhiteout(5))));
        var shownBy = new Dictionary<string, RgbColor[]>(StringComparer.Ordinal);
        foreach ((string name, SamusBeamCombination equipped, Action<SamusColours> lay) in cases)
        {
            var colours = new SamusColours();
            SamusProjectileSystem.LoadBeamPalette(colours, equipped);
            lay(colours);
            var drawn = new SpriteFrameBuffer();
            drawn.BeginFrame();
            enemies.SpriteObjects.DrawRoomSpriteObjects(drawn, camera, HitBurstArtwork(equipped));
            drawn.FinalizeFrame();
            ImageSpriteDraw draw = colours.ShadeOwnDraws(drawn.Draws).OfType<ImageSpriteDraw>()
                .Single(candidate => candidate.Source.Kind == SpriteSourceKind.SamusHitBurst);
            SpriteSheetFrame expected = ProjectileSheets.Gameplay(InstalledFixtures.SpriteSheets).For(equipped)
                .Frame(ProjectileSheetFrames.HitBurstFrame(HitBurstSpriteDefinitions.All[0]));
            AssertEqual(expected, draw.Image, $"{name}: the burst draws from the sheet of the equipped beams");
            AssertEqual(colours.Shading, draw.Shading, $"{name}: the burst shows Samus's shading");
            shownBy[name] = ShownColours(draw, colours, name);
        }
        AssertTrue(!shownBy["Power equipped"].SequenceEqual(shownBy["Plasma equipped"]),
            "the burst shows the Power beam's colours and the Plasma beam's differently");
        Console.WriteLine($"  Hit burst colouring: {cases.Count} beam colourings each show the burst in Samus's beams' colours from her equipped beams' sheet.");
    }

    /// <summary>
    /// The colours <paramref name="draw"/>'s burst shows, in the order its pixels first show them, each checked to be the
    /// colour Samus's beams show under <paramref name="colours"/> at every place its key was coloured from.
    /// </summary>
    private static RgbColor[] ShownColours(ImageSpriteDraw draw, SamusColours colours, string name)
    {
        SpriteSheetFrame frame = draw.Image;
        SpriteImage sheet = frame.Sheet;
        RgbColor[] keyColours = (draw.Shading ?? throw new InvalidOperationException($"{name}: the burst's draw names a shading.")).KeyColours(sheet);
        RgbColor[] beams = colours.ShownBeams(InstalledFixtures.SamusShadeSources);
        var shown = new List<RgbColor>();
        var seen = new HashSet<ColourKey>();
        ReadOnlySpan<RgbaPixel> pixels = sheet.Pixels;
        for (int row = 0; row < frame.Size.Y; row++)
        {
            for (int column = 0; column < frame.Size.X; column++)
            {
                int at = (frame.Corner.Y + row) * sheet.Size.X + frame.Corner.X + column;
                if (pixels[at].A == 0)
                    continue;
                ColourKey key = sheet.KeyAt(at);
                if (!seen.Add(key))
                    continue;
                RgbColor colour = keyColours[sheet.ColourKeys.IndexOf(key)];
                foreach (int place in InstalledPictureColoursConstants.CartridgeColoursOf(sheet, key))
                {
                    AssertTrue(place >= GameplayShownColours.SamusBeamsFirst && place < GameplayShownColours.SamusBeamsFirst + beams.Length,
                        $"{name}: the burst's colour {key} was coloured from her beams' colours, not place {place}");
                    AssertEqual(beams[place - GameplayShownColours.SamusBeamsFirst], colour,
                        $"{name}: the burst's colour from place {place} shows her beams' colour there");
                }
                shown.Add(colour);
            }
        }
        AssertTrue(shown.Count > 1, $"{name}: the burst shows more than one colour");
        return [.. shown];
    }

    /// <summary>What the burst draws with <paramref name="beams"/> equipped: the installed projectile sheets, as gameplay draws it.</summary>
    private static SamusHitBurstArtwork HitBurstArtwork(SamusBeamCombination beams) =>
        new(new ProjectileDrawArtwork(ProjectileSheets.Gameplay(InstalledFixtures.SpriteSheets)), beams);

    /// <summary>
    /// <paramref name="burst"/> lies at <see cref="Place"/>, draws with the room's graphics, took the highest place, shows
    /// <see cref="BurstFrames"/> for two updates each and ends on its eighth update.
    /// </summary>
    private static void AssertBurst(RoomEnemySystem enemies, RoomSpriteObject burst, string kind)
    {
        AssertEqual(Place, burst.Position.Whole, $"{kind}: the burst lies at the enemy's position");
        AssertEqual(default(SpriteGraphics), burst.Graphics, $"{kind}: the burst draws with the room's graphics");
        AssertEqual<int?>(SpriteObjectPlaces.PlaceCount - 1, enemies.SpriteObjects.PlaceOf(burst), $"{kind}: the burst takes the highest free place");
        for (int update = 0; update < BurstFrames.Length; update++)
        {
            AssertTrue(burst.IsActive, $"{kind}: the burst shows on update {update}");
            AssertEqual(BurstFrames[update], burst.Frame, $"{kind}: the burst's frame on update {update}");
            enemies.SpriteObjects.StepRoomSpriteObjects();
        }
        AssertTrue(!burst.IsActive, $"{kind}: the burst ends on its eighth update");
    }

    /// <summary>The live bursts of <paramref name="enemies"/>' room.</summary>
    private static RoomSpriteObject[] Bursts(RoomEnemySystem enemies) =>
        [.. enemies.SpriteObjects.Live.Where(spawned => spawned.Kind == RoomSpriteObjectKind.EnemyHit)];

    /// <summary>A dud flash at <see cref="Place"/>, as a shot an enemy part shrugs off leaves.</summary>
    private static RoomSpriteObject SpawnDud(RoomEnemySystem enemies)
    {
        (SamusProjectileSystem shots, _) = ShotAt(SamusBeamCombination.Power);
        enemies.Projectiles.CreateEnemyProjectileDudShot(shots.Slots[0]);
        return enemies.SpriteObjects.Live.Last();
    }

    /// <summary>A room with an enemy of <paramref name="definition"/> placed at <see cref="Place"/> among its interactive enemies.</summary>
    private static (RoomEnemySystem Enemies, RoomEnemySlot Slot) RoomWith(EnemyDefinitionId id, RoomEnemyDefinition definition, int health, int frozenTimer)
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(InstalledFixtures.EnemyTiles), RoomEnemyCompatibilityFixtures.ForNewRoom(),
            definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(id, definition);
        slot.Position = FixedPosition<RoomPixels>.FromWhole(Place);
        slot.Radius = definition.Radius;
        slot.Health = health;
        slot.FrozenTimer = frozenTimer;
        // Shots reach only an enemy showing a frame.
        slot.Frame = EnemyFrameId.SpriteObjectDudShotFirst;
        ((List<RoomEnemySlot>)enemies.ProcessingWindow.InteractiveEnemies).Add(slot);
        return (enemies, slot);
    }

    /// <summary>One uncharged beam of <paramref name="beam"/> at <see cref="Place"/>, flying right.</summary>
    private static (SamusProjectileSystem Shots, SamusBombProjectileSystem Bombs) ShotAt(SamusBeamCombination beam)
    {
        var shots = new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftMuzzleSubpixelCarry(SamusProjectileSystem.SlotCount));
        SamusProjectile shot = shots.Slots[0];
        shot.Type = SamusProjectileType.Beam(beam, charged: false);
        shot.Damage = 20;
        shot.Direction = SamusProjectileDirection.Right;
        shot.Position = shot.Position.WithWhole(Place);
        shot.Radius = new(4, 4);
        return (shots, new SamusBombProjectileSystem());
    }

    /// <summary>Runs the enemy hit burst checks.</summary>
    [VerificationCommand("--enemy-hit-burst")]
    internal static int EnemyHitBurstCommand()
    {
        Suite(nameof(VerifyShotHitBursts), () => VerifyShotHitBursts());
        Suite(nameof(VerifyBombHitBursts), () => VerifyBombHitBursts());
        Suite(nameof(VerifyPirateHitBursts), () => VerifyPirateHitBursts());
        Suite(nameof(VerifyHitBurstPlaces), () => VerifyHitBurstPlaces());
        Suite(nameof(VerifyHitBurstColouring), () => VerifyHitBurstColouring());
        return 0;
    }
}
