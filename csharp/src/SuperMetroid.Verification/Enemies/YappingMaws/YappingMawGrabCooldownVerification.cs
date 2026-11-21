using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.YappingMaws;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.World;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.YappingMaws;

/// <summary>
/// Checks when a Yapping Maw may grab Samus (#2454) through its production update and touch, against the original's
/// cooldown word: decremented on every update with no clamp, set to 48 by the point-blank wait, and grabbing only while
/// negative.
/// </summary>
internal static class YappingMawGrabCooldownVerification
{
    /// <summary>The cooldown word's span: a decrement from zero wraps to one less than this.</summary>
    private const int CooldownWordSpan = 1 << 16;

    /// <summary>Where the Maw is rooted.</summary>
    private static readonly Position16<RoomPixels> Root = new(128, 128);

    /// <summary>Where Samus waits: further from the Maw than its reach and than 32 pixels, so it neither bites nor restarts its cooldown.</summary>
    private static readonly Position16<RoomPixels> OutOfReach = new(128 + 300, 128);

    /// <summary>
    /// A newly placed Maw, and the same Maw after one update with Samus at its root, each counted through two full rounds
    /// of 65536 updates with Samus out of its reach, to where the word stands at none again: after each update it may grab exactly while the original's word,
    /// started at none or at 48, reads negative. A touch while it refuses leaves Samus free; a touch while it allows grabs
    /// her and spends the cooldown.
    /// </summary>
    internal static void VerifyYappingMawGrabCooldown()
    {
        (RoomEnemySystem enemies, RoomEnemyPlace place, YappingMawEnemyState state) = NewYappingMaw();
        SamusState samus = NewSamus();
        var frame = new ClockedEnemyAiFrame(
            RoomEnemyFrameFixtures.Standalone(RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), 0) with { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, Samus = samus },
            new FrameCounters(SuperMetroid.Core.Compatibility.ShortFrameCount.Zero));

        samus.Position = samus.Position.WithWhole(OutOfReach);
        CountRounds(enemies, place, state, frame, startedAt: 0, "a newly placed Maw");

        samus.Position = samus.Position.WithWhole(Root);
        enemies.YappingMawBehavior.RunYappingMawMainAi(place.HeldOccupant().SpawnSettings<YappingMawSpawnSettings>(), place, place.RequireSpeciesState<YappingMawEnemyState>(), frame);
        AssertEqual(YappingMawAction.WaitingForSamus, state.Action, "Samus at the Maw's root starts no bite");
        AssertTrue(!state.GrabCooldown.AllowsGrab, "Samus at the Maw's root starts its cooldown");
        samus.Position = samus.Position.WithWhole(OutOfReach);
        CountRounds(enemies, place, state, frame, startedAt: YappingMawAttackDefinitions.GrabCooldown, "a Maw whose cooldown Samus started");

        // The word now stands at none: a touch is refused, and the next update, which counts it past none, allows one.
        AssertTrue(!state.GrabCooldown.AllowsGrab, "the cooldown refuses at the end of its second round");
        YappingMawBehavior.ReactToYappingMawTouch(Touch(place, samus), place.RequireSpeciesState<YappingMawEnemyState>());
        AssertTrue(!state.HasGrabbedSamus, "a touch while the cooldown refuses leaves Samus free");
        enemies.YappingMawBehavior.RunYappingMawMainAi(place.HeldOccupant().SpawnSettings<YappingMawSpawnSettings>(), place, place.RequireSpeciesState<YappingMawEnemyState>(), frame);
        AssertTrue(state.GrabCooldown.AllowsGrab, "the next update lets the Maw grab again");
        YappingMawBehavior.ReactToYappingMawTouch(Touch(place, samus), place.RequireSpeciesState<YappingMawEnemyState>());
        AssertTrue(state.HasGrabbedSamus, "a touch while the cooldown allows grabs Samus");
        AssertEqual(YappingMawGrabCooldown.Spent, state.GrabCooldown, "the grab spends the cooldown");
        Console.WriteLine("  Yapping Maw grab cooldown: two rounds from none and from 48 grab exactly while the original's word is negative.");
    }

    /// <summary>
    /// Runs the Maw's updates with Samus out of reach until the original's word, started at <paramref name="startedAt"/>,
    /// has counted down to none for the third time, comparing after each update whether the Maw may grab with the word's
    /// sign.
    /// </summary>
    private static void CountRounds(RoomEnemySystem enemies, RoomEnemyPlace place, YappingMawEnemyState state,
        ClockedEnemyAiFrame frame, int startedAt, string context)
    {
        for (int update = 1; update <= 2 * CooldownWordSpan + startedAt; update++)
        {
            enemies.YappingMawBehavior.RunYappingMawMainAi(place.HeldOccupant().SpawnSettings<YappingMawSpawnSettings>(), place, place.RequireSpeciesState<YappingMawEnemyState>(), frame);
            int word = ((startedAt - update) % CooldownWordSpan + CooldownWordSpan) % CooldownWordSpan;
            bool negative = word >= CooldownWordSpan / 2;
            if (state.GrabCooldown.AllowsGrab != negative)
                throw new InvalidOperationException(
                    $"{context}: after update {update} the Maw {(state.GrabCooldown.AllowsGrab ? "allows" : "refuses")} a grab where the original's word {word} is {(negative ? "negative" : "not negative")}.");
        }
        AssertEqual(YappingMawAction.WaitingForSamus, state.Action, $"{context}: Samus out of reach starts no bite");
    }

    /// <summary>Samus touching the Maw's mouth.</summary>
    private static EnemyTouchContext Touch(RoomEnemyPlace place, SamusState samus) =>
        new(place.HeldOccupant(), samus, RoomEnemyFrameFixtures.StandalonePseudoScrew(), default, RoomEnemyFrameFixtures.OpenRoom(new(64, 64)),
            UsesFrameHitboxes: false, HitboxTouch: place.Definition.Touch, Position16<RoomPixels>.Zero);

    /// <summary>A standing Samus.</summary>
    private static SamusState NewSamus() =>
        new(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new UnauthoredBoostCadence(),
            SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), InstalledFixtures.SamusArtwork());

    /// <summary>A Yapping Maw reaching down from <see cref="Root"/> with a reach of 48 pixels, through the production initializer.</summary>
    private static NewInitializedYappingMawResult NewYappingMaw()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.YappingMaw, place.KeptDefinition);
        place.Bind(place.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
        place.Position = place.Position.WithWhole(Root);
        place.SetSpawnParameter1(EnemyDefinitionId.YappingMaw, 48);
        place.SetSpawnParameter2(EnemyDefinitionId.YappingMaw, 0);
        enemies.YappingMawBehavior.InitializeYappingMaw(place.HeldOccupant(), place.HeldOccupant().SpawnSettings<YappingMawSpawnSettings>(), place);
        YappingMawEnemyState state = enemies.YappingMawStates[0] ?? throw new InvalidOperationException("Yapping Maw has no state.");
        return new NewInitializedYappingMawResult(enemies, place, state);
    }
}
