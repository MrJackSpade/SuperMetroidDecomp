using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.SpriteObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Progression;
namespace SuperMetroid.Verification.Enemies;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the common shot reaction an exploding bomb runs on an enemy (#1704).</summary>
internal static class EnemyBombShotReactionVerification
{
    /// <summary>The place the Mellow and the bomb share.</summary>
    private static readonly Position16<RoomPixels> Place = new(0x0100, 0x0100);

    /// <summary>
    /// An exploding bomb on a Mellow, through the ordinary bomb pass: a Mellow the bomb hurts makes its cry at low
    /// priority and loses the bomb's damage, and shows no dud; a frozen Mellow loses it without a cry; a Mellow the bomb
    /// cannot hurt keeps its health and does not flash, and the bomb shows the dud flash where it lies and the dud sound
    /// plays at low priority, as the original's common shot reaction does for a bomb.
    /// </summary>
    internal static void VerifyBombHitCriesOrDuds()
    {
        RoomEnemyDefinition mellow = RoomEnemyDefinitionCatalog.ById[EnemyDefinitionId.Mellow];
        SoundEffectId cry = mellow.HurtSoundEffect ?? throw new InvalidOperationException("The Mellow has a cry.");
        foreach ((string kind, RoomEnemyDefinition definition, int frozenTimer) in new[]
        {
            ("hurt", mellow, 0),
            ("frozen", mellow, 300),
            ("immune", mellow with { Vulnerability = EnemyVulnerabilityDefinitions.Invulnerable }, 0),
        })
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot) = LoadedRoomWithMellow(definition, frozenTimer);
            var bombs = new SamusBombProjectileSystem();
            SamusBombProjectileSlot bomb = bombs.Slots[0];
            bomb.Type = SamusBombProjectileSystem.NormalBombType;
            bomb.Damage = 30;
            bomb.Radius = new(4, 4);
            bomb.Position = FixedPosition<RoomPixels>.FromWhole(Place);
            var shots = new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads,
                new LeftMuzzleSubpixelCarry(SamusProjectileSystem.SlotCount));

            enemies.Audio.BeginFrame();
            AssertEqual(1, enemies.Collision.ResolveOrdinaryBombHits(bombs, shots, samus: null, onlyEnemy: slot),
                $"{kind}: the exploding bomb hits the Mellow");

            bool immune = kind == "immune";
            AssertTrue(bomb.IsSpent, $"{kind}: the hit marks the bomb");
            AssertEqual(immune ? 1000 : 1000 - 30, slot.Health, $"{kind}: the Mellow's health after the bomb");
            AssertEqual(immune ? 0 : slot.HurtFlashFrames + 8, slot.FlashTimer, $"{kind}: the Mellow's hurt flash");
            SoundRequest[] expected = kind switch
            {
                "hurt" => [new SoundRequest(cry, SoundRequestPriority.Low)],
                "frozen" => [],
                _ => [new SoundRequest(SamusSound.DudShot, SoundRequestPriority.Low)],
            };
            AssertSequenceEqual(expected, enemies.Audio.SoundRequests, $"{kind}: the sounds the bomb's hit queues");
            RoomSpriteObject[] duds = [.. enemies.SpriteObjects.Live.Where(spawned => spawned.Kind == RoomSpriteObjectKind.EnemyProjectileDud)];
            AssertEqual(immune ? 1 : 0, duds.Length, $"{kind}: the dud flashes the bomb's hit shows");
            if (immune)
                AssertEqual(Place, duds[0].Position.Whole, "the dud flash shows where the bomb lies");
        }
        Console.WriteLine("  Bomb shot reaction: a hurt enemy cries unless frozen; an immune one leaves the bomb's dud flash and sound.");
    }

    /// <summary>
    /// An exploding bomb on the room's leftmost pixel column (<see cref="LeftEdgeBombMissesFrameHitboxes"/>): a Mellow that
    /// collides by its frames' hitboxes, showing the empty frame whose one hitbox is the point at its origin, passes over a
    /// bomb lying on that point on the leftmost column and is hurt by the same bomb one column to the right, while a Mellow
    /// that collides by its radius is hurt by the bomb on the leftmost column.
    /// </summary>
    internal static void VerifyLeftEdgeBombMissesFrameHitboxes()
    {
        RoomEnemyDefinition mellow = RoomEnemyDefinitionCatalog.ById[EnemyDefinitionId.Mellow];
        foreach ((string kind, bool frameHitboxes, int column, bool hit) in new[]
        {
            ("frame hitboxes, leftmost column", true, LeftEdgeBombMissesFrameHitboxes.PassedColumn, false),
            ("frame hitboxes, next column", true, LeftEdgeBombMissesFrameHitboxes.PassedColumn + 1, true),
            ("radius, leftmost column", false, LeftEdgeBombMissesFrameHitboxes.PassedColumn, true),
        })
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot) = LoadedRoomWithMellow(mellow, frozenTimer: 0);
            var place = new Position16<RoomPixels>(column, Place.Y);
            slot.Position = FixedPosition<RoomPixels>.FromWhole(place);
            slot.CollidesByFrameHitboxes = frameHitboxes;
            if (frameHitboxes)
                slot.Frame = EnemyFrameId.Empty;
            var bombs = new SamusBombProjectileSystem();
            SamusBombProjectileSlot bomb = bombs.Slots[0];
            bomb.Type = SamusBombProjectileSystem.NormalBombType;
            bomb.Damage = 30;
            bomb.Radius = new(4, 4);
            bomb.Position = FixedPosition<RoomPixels>.FromWhole(place);
            var shots = new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads,
                new LeftMuzzleSubpixelCarry(SamusProjectileSystem.SlotCount));

            enemies.Audio.BeginFrame();
            AssertEqual(hit ? 1 : 0, enemies.Collision.ResolveOrdinaryBombHits(bombs, shots, samus: null, onlyEnemy: slot),
                $"{kind}: the bomb's hits");
            AssertEqual(hit, bomb.IsSpent, $"{kind}: the hit marks the bomb");
            AssertEqual(hit ? 1000 - 30 : 1000, slot.Health, $"{kind}: the Mellow's health after the bomb");
        }
        Console.WriteLine("  Left-edge bomb: frame-hitbox enemies pass over a bomb on the room's leftmost column; radius enemies meet it.");
    }

    /// <summary>A loaded room with no population, a Mellow of <paramref name="definition"/> placed at <see cref="Place"/>.</summary>
    private static (RoomEnemySystem Enemies, RoomEnemySlot Slot) LoadedRoomWithMellow(RoomEnemyDefinition definition, int frozenTimer)
    {
        const ushort populationPointer = 0x9400;
        var bus = new TestAddressSpace();
        bus.WriteByte(0xa10000 | populationPointer, 0xff);
        bus.WriteByte((0xa10000 | populationPointer) + 1, 0xff);
        bus.WriteByte((0xa10000 | populationPointer) + 2, 0);
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(),
            definitions: bus, services: new StandaloneRoomEnemyServices(() => 0));
        enemies.Load(new RoomEnemyLoadRequest(bus.ReadEnemyPopulation(populationPointer), BgArtworkFixtures.For(new SnesVram())));
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Mellow, definition);
        slot.Position = FixedPosition<RoomPixels>.FromWhole(Place);
        slot.Radius = definition.Radius;
        slot.Health = 1000;
        slot.FrozenTimer = frozenTimer;
        slot.Frame = EnemyFrameId.SpriteObjectDudShotFirst;
        return (enemies, slot);
    }

    /// <summary>Runs the bomb shot reaction check.</summary>
    [VerificationCommand("--enemy-bomb-shot-reaction")]
    internal static int EnemyBombShotReactionCommand()
    {
        Suite(nameof(VerifyBombHitCriesOrDuds), () => VerifyBombHitCriesOrDuds());
        Suite(nameof(VerifyLeftEdgeBombMissesFrameHitboxes), () => VerifyLeftEdgeBombMissesFrameHitboxes());
        return 0;
    }
}
