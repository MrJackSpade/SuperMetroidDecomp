using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Shutters;
using SuperMetroid.Core.Enemies.SpriteObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
namespace SuperMetroid.Verification.Enemies.Shutters;

using SuperMetroid.Verification.Harness;

/// <summary>
/// Checks what a bomb and a Power Bomb do to the horizontal shutter (#1718). The bomb pass runs the shutter's shot
/// reaction, which runs the common damage with no death check and no hit burst and then the shutter's reaction; the
/// shutter's bomb vulnerability is zero, so the bomb leaves the dud flash and sound at itself. The Power Bomb pass runs the
/// shutter's own Power Bomb reaction, which is the shutter's reaction alone, and then lets it process off screen. No
/// retail room places the shutter, so the port compiles no definition for it; each case decodes the shutter's header from
/// the cartridge.
/// </summary>
internal static class HorizontalShutterBombVerification
{
    /// <summary>Where the shutter, the bomb and the Power Bomb's centre lie.</summary>
    private static readonly Position16<RoomPixels> Place = new(256, 256);

    /// <summary>
    /// An exploding bomb on an idle repeatable horizontal shutter: the bomb pass reaches it, the bomb is spent, the shutter
    /// takes no damage, flash or hurt state, shows no hit burst, leaves one dud flash at the bomb, and starts moving the
    /// way it first heads, its reaction direction turned to that way.
    /// </summary>
    internal static void VerifyBombReachesHorizontalShutter(CartridgeImportAddressSpace rom)
    {
        (RoomEnemySystem enemies, RoomEnemySlot shutter, HorizontalShutterEnemyState state) = RoomWithIdleShutter(rom);
        AssertEqual(0, EnemyVulnerabilities.Of(shutter.Definition).Ordnance.Bomb.Multiplier, "the horizontal shutter's bomb vulnerability is zero");

        var bombs = new SamusBombProjectileSystem();
        SamusBombProjectileSlot bomb = bombs.Slots[0];
        bomb.Type = SamusBombProjectileSystem.NormalBombType;
        bomb.Damage = 30;
        bomb.Radius = new(4, 4);
        bomb.Position = FixedPosition<RoomPixels>.FromWhole(Place);
        var shots = new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftMuzzleSubpixelCarry(SamusProjectileSystem.SlotCount));

        AssertEqual(1, enemies.Collision.ResolveOrdinaryBombHits(bombs, shots, samus: null, onlyEnemy: shutter), "the bomb pass reaches the horizontal shutter");
        AssertTrue(bomb.IsSpent, "the bomb that reached the shutter is spent");
        AssertNoDamage(shutter, "bomb");
        RoomSpriteObject[] live = [.. enemies.SpriteObjects.Live];
        AssertEqual(0, live.Count(spawned => spawned.Kind == RoomSpriteObjectKind.EnemyHit), "the bomb shows no hit burst");
        RoomSpriteObject dud = live.Single(spawned => spawned.Kind == RoomSpriteObjectKind.EnemyProjectileDud);
        AssertEqual(Place, dud.Position.Whole, "the bomb's dud flash lies at the bomb");
        AssertReacted(state, "bomb");
        Console.WriteLine("  Horizontal shutter bomb: the bomb runs its shot reaction, a dud with no damage and no burst, and the shutter moves.");
    }

    /// <summary>
    /// A Power Bomb whose blast holds an idle repeatable horizontal shutter: the pass runs the shutter's reaction alone, so
    /// the shutter takes no damage, flash, hurt state or invincibility and shows no sprite object, starts moving, and is
    /// let process off screen.
    /// </summary>
    internal static void VerifyPowerBombReachesHorizontalShutter(CartridgeImportAddressSpace rom)
    {
        (RoomEnemySystem enemies, RoomEnemySlot shutter, HorizontalShutterEnemyState state) = RoomWithIdleShutter(rom);
        AssertTrue(EnemyVulnerabilities.Of(shutter.Definition).Ordnance.PowerBomb.Multiplier != 0, "the horizontal shutter's Power Bomb vulnerability admits the blast");

        AssertEqual(1, enemies.Collision.ResolveOrdinaryPowerBombHits(Place, explosionRadius: 32, samus: null), "the Power Bomb reaches the horizontal shutter");
        AssertNoDamage(shutter, "Power Bomb");
        AssertEqual(0, shutter.InvincibilityTimer, "the Power Bomb leaves the shutter without invincibility");
        AssertEqual(0, enemies.SpriteObjects.Live.Count(), "the Power Bomb shows no sprite object at the shutter");
        AssertTrue(shutter.Processing.HasAny(EnemyProcessing.OffScreen), "the Power Bomb lets the shutter process off screen");
        AssertReacted(state, "Power Bomb");
        Console.WriteLine("  Horizontal shutter Power Bomb: the blast runs the shutter's reaction alone and the shutter moves.");
    }

    /// <summary>The shutter kept its health and took no flash or hurt state.</summary>
    private static void AssertNoDamage(RoomEnemySlot shutter, string weapon)
    {
        AssertEqual(shutter.Definition.Health, shutter.Health, $"the {weapon} leaves the shutter's health");
        AssertEqual(0, shutter.FlashTimer, $"the {weapon} does not flash the shutter");
        AssertEqual(EnemyAiStates.None, shutter.AiStates & EnemyAiStates.Hurt,$"the {weapon} does not hurt the shutter");
        AssertTrue(!shutter.Presence.HasAny(EnemyPresence.Deleted), $"the {weapon} leaves the shutter in its place");
    }

    /// <summary>The idle shutter reacted: it heads right, the way it first heads, and its reaction direction turned to right.</summary>
    private static void AssertReacted(HorizontalShutterEnemyState state, string weapon)
    {
        AssertEqual<HorizontalShutterAction?>(HorizontalShutterAction.MovingRight, state.Action, $"the {weapon} starts the idle shutter moving right");
        AssertEqual(ShutterHeading.RightOrDown, state.ReactionDirection, $"the {weapon} turns the shutter's reaction direction");
    }

    /// <summary>
    /// A room whose first place holds an idle horizontal shutter, of the header <paramref name="rom"/> holds, at
    /// <see cref="Place"/> among its interactive enemies: it reacts every time, first heads right, and has not yet reacted,
    /// so its reaction direction is the opposite end.
    /// </summary>
    private static (RoomEnemySystem Enemies, RoomEnemySlot Shutter, HorizontalShutterEnemyState State) RoomWithIdleShutter(CartridgeImportAddressSpace rom)
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(InstalledFixtures.EnemyTiles), RoomEnemyCompatibilityFixtures.ForNewRoom(),
            definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        RoomEnemyDefinition definition = RoomEnemyDefinitionImporter.Load(rom, EnemyDefinitionId.ShutterHorizShootable);
        AssertEqual<EnemyReaction?>(EnemyReaction.HorizontalShutterShot, definition.Shot, "the shutter's header names its shot reaction");
        AssertEqual<EnemyReaction?>(EnemyReaction.HorizontalShutterPowerBomb, definition.PowerBombReaction, "the shutter's header names its Power Bomb reaction");
        RoomEnemySlot shutter = enemies.SlotTable.Slots[0];
        shutter.Bind(EnemyDefinitionId.ShutterHorizShootable, definition);
        shutter.Position = FixedPosition<RoomPixels>.FromWhole(Place);
        shutter.Radius = definition.Radius;
        shutter.Health = definition.Health;
        // Bombs reach only an enemy showing a frame.
        shutter.Frame = EnemyFrameId.SpriteObjectDudShotFirst;
        var state = new HorizontalShutterEnemyState
        {
            TriggerMode = ShutterTriggerMode.RepeatableReaction,
            PrimaryDirection = ShutterHeading.RightOrDown,
            ReactionDirection = ShutterHeading.LeftOrUp,
            Action = HorizontalShutterAction.InitialNoOp,
        };
        shutter.SetSpeciesState(state);
        ((List<RoomEnemySlot>)enemies.ProcessingWindow.InteractiveEnemies).Add(shutter);
        return (enemies, shutter, state);
    }

    /// <summary>Runs the horizontal shutter's bomb and Power Bomb checks.</summary>
    [VerificationCommand("--horizontal-shutter-bombs")]
    internal static int HorizontalShutterBombsCommand()
    {
        CartridgeImportAddressSpace rom = RepositoryInputsVerification.LoadRepositoryRom();
        Suite(nameof(VerifyBombReachesHorizontalShutter), () => VerifyBombReachesHorizontalShutter(rom));
        Suite(nameof(VerifyPowerBombReachesHorizontalShutter), () => VerifyPowerBombReachesHorizontalShutter(rom));
        return 0;
    }
}
