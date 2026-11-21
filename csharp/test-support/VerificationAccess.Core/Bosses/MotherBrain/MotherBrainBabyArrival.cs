using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The Baby Metroid's arrival in Mother Brain's late fight, as the game brings it in (#2554): her body's main AI runs the
/// last update of the wait that charges her final beam, whose step spawns the Baby in the room's first free place.
/// </summary>
internal static class MotherBrainBabyArrival
{
    /// <summary>
    /// Brings in the Baby of <paramref name="state"/>'s late fight, which it begins if it has not, with
    /// <paramref name="samus"/> as her body's main AI sees her.
    /// </summary>
    /// <param name="state">Mother Brain's encounter, which her room's body place holds.</param>
    /// <param name="samus">Samus.</param>
    /// <returns>The late fight, whose Baby has arrived.</returns>
    /// <exception cref="InvalidOperationException">The room had no free place for the Baby.</exception>
    internal static MotherBrainRainbowBeamAttackSequence BringInBaby(MotherBrainEnemyState state, SamusState samus)
    {
        MotherBrainRainbowBeamAttackSequence lateFight = Ensure.NotNull(state).RainbowBeamSequence ?? state.BeginLateFight();
        state.FinishOffPhase = MotherBrainFinishOffPhase.BringInBaby;
        state.PhaseTimer.LastFor(1);
        state.Body.RunMainAi(samus);
        return lateFight.Baby is not null
            ? lateFight
            : throw new InvalidOperationException("Mother Brain brought in her final beam's Baby with no free place to spawn it in.");
    }

    /// <summary>
    /// Places Mother Brain in <paramref name="enemies"/>' first two places as her room's population places her: the room's
    /// load of her room's first two entries, her body and then her brain, which joins the encounter her body begins, sets
    /// each up with her definitions and her entries' placement and hit areas, beside <paramref name="samus"/> (#2554). The
    /// enemies load a room once, so a check places her in enemies of their own.
    /// </summary>
    /// <returns>The encounter her body began.</returns>
    internal static MotherBrainEnemyState PlaceMotherBrain(World.RoomEnemySystem enemies, SamusState samus)
    {
        Ensure.NotNull(enemies);
        RoomEnemyPopulationDefinition room = World.RoomStates.RoomEnemyPopulationDefinitions.MotherBrainBeforeDefeat;
        RoomEnemyPlacement[] bodyAndBrain = room.Placements.Span[..2].ToArray();
        enemies.Load(RoomEnemyFrameFixtures.StandaloneLoad(new PlacedRoomEnemyPopulationDefinition(room.Name, bodyAndBrain, deathQuota: 0),
            Hardware.BgArtworkFixtures.For(new SuperMetroid.AssetExtraction.Video.SnesVram()), RoomEnemyFrameFixtures.OpenRoom(new(64, 64)),
            Ensure.NotNull(samus)));
        IReadOnlyList<RoomEnemyPlace> places = enemies.Places.InUpdateOrder;
        MotherBrainEnemyState state = places[0].EntityOf<MotherBrainBodyEntity>().Encounter;
        _ = places[1].EntityOf<MotherBrainHeadEntity>();
        return state;
    }

    /// <summary>The Baby the late fight brought in, as its place holds it.</summary>
    /// <exception cref="InvalidOperationException">The late fight has brought in no Baby.</exception>
    internal static BabyMetroidCutsceneEntity BabyOf(MotherBrainRainbowBeamAttackSequence lateFight) =>
        (Ensure.NotNull(lateFight).Baby ?? throw new InvalidOperationException("The late fight has brought in no Baby.")).Place.EntityOf<BabyMetroidCutsceneEntity>();
}
