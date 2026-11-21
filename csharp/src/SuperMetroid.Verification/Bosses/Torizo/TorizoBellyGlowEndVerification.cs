using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.World;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>
/// Confirms that a Torizo's belly glow ends on the update the Torizo it lights has no health left (#1700), as the
/// original's does. The original's glow ends on the first update its handler finds the first enemy place's health zero;
/// only the Bomb and Golden Torizo start the glow, as their fights start, and every retail room that holds either holds it
/// alone in its first place, so the Torizo the glow now watches is the enemy the original's test read.
/// </summary>
internal static class TorizoBellyGlowEndVerification
{
    /// <summary>The update on which the Torizo is left one health point, the glow still running.</summary>
    private const int LastHealthUpdate = 40;

    /// <summary>The update on which the Torizo's health first reads zero.</summary>
    private const int DefeatUpdate = 47;

    /// <summary>The updates the glow is followed for, past the defeat.</summary>
    private const int Updates = 60;

    /// <summary>Checks the retail Torizo rooms and the glow's end in each.</summary>
    internal static void VerifyTorizoBellyGlowEnd()
    {
        VerifyTorizoRooms();
        VerifyGlowEnd(EnemyDefinitionId.BombTorizo, TorizoCueKind.StartBombTorizoFight, ColourEffectId.BombTorizoBelly, TorizoVariant.Bomb);
        VerifyGlowEnd(EnemyDefinitionId.GoldenTorizo, TorizoCueKind.StartGoldenTorizoFight, ColourEffectId.GoldenTorizoBelly, TorizoVariant.Golden);
        Console.WriteLine("Torizo belly glow: the Bomb and Golden Torizo rooms hold their Torizo alone in the first place, and each glow ends on the update its Torizo has no health left, as the original's first-place test ends it.");
    }

    /// <summary>
    /// Checks that the retail rooms that hold a Torizo are the Bomb Torizo's and the Golden Torizo's, each holding its
    /// Torizo alone in its first place.
    /// </summary>
    private static void VerifyTorizoRooms()
    {
        RoomEnemyPopulationDefinition[] torizoRooms = [.. RoomEnemyPopulationDefinitions.Definitions.Where(population =>
            population.Placements.ToArray().Any(record => record.DefinitionId is EnemyDefinitionId.BombTorizo or EnemyDefinitionId.GoldenTorizo))];
        AssertTrue(torizoRooms.Select(population => population.Name).Order()
                .SequenceEqual([nameof(RoomEnemyPopulationDefinitions.BombTorizoBeforeTimebomb), nameof(RoomEnemyPopulationDefinitions.GoldenTorizo)]),
            "only the Bomb Torizo's and the Golden Torizo's rooms hold a Torizo");
        foreach (RoomEnemyPopulationDefinition population in torizoRooms)
        {
            RoomEnemyPlacement[] records = population.Placements.ToArray();
            AssertEqual(1, records.Length, $"{population.Name} holds its Torizo alone");
            AssertTrue(records[0].DefinitionId is EnemyDefinitionId.BombTorizo or EnemyDefinitionId.GoldenTorizo,
                $"{population.Name} holds its Torizo in its first place");
        }
    }

    /// <summary>
    /// Starts <paramref name="definition"/>'s fight in the room's first enemy place and runs its glow for
    /// <see cref="Updates"/> updates, its health dropping to one and then to zero, and checks each update against the
    /// original's test: the glow runs until the first update the first place's health reads zero, and ends on it.
    /// </summary>
    private static void VerifyGlowEnd(EnemyDefinitionId definition, TorizoCueKind fightStart, ColourEffectId glow, TorizoVariant variant)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(),
            definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace torizo = enemies.Places.InUpdateOrder[0];
        torizo.Bind(definition, RoomEnemyDefinitionLookup.Get(definition));
        torizo.Health = 800;
        var state = new TorizoEnemyState(variant) { Phase = TorizoPhase.Falling };
        torizo.SetSpeciesState(state);
        enemies.PassEvents.Begin();
        AssertTrue(enemies.TorizoBehavior.ApplyTorizoCue(
                torizo.HeldOccupant(), torizo, state, new TorizoCueInputs { Torizo = torizo.Entity as ILitTorizo ?? throw new InvalidOperationException($"{definition} binds no Torizo."), Samus = SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.StandaloneSamus(), Level = SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), ControllerInput = 0, FrameCounter = 0 },
                new TorizoCue.Signal(fightStart), TorizoAnimations.Start(TorizoAnimation.BombTorizoAwakening)) is null,
            $"{definition}'s fight start plays on");
        TorizoBellyGlowRequest request = enemies.LastEnemyFrame.All<TorizoBellyGlowRequest>().LastOrDefault()
            ?? throw new InvalidOperationException($"{definition}'s fight start asks for no belly glow.");
        AssertEqual(glow, request.Glow, $"{definition}'s fight start asks for its own belly glow");
        AssertTrue(ReferenceEquals(torizo.Entity, request.Torizo), $"{definition}'s glow lights the Torizo whose fight started it");

        var fx = new global::SuperMetroid.Core.Fx.ColourEffectSystem(new ColourEffectSessionState(), SuperMetroid.Core.Audio.GameplayAudioRequests.Standalone());
        fx.StartTorizoBellyGlow(request.Glow, request.Torizo, equippedItems: 0);
        var conditions = new ColourEffectInputs { Samus = FixtureRoomSamus.Uninstalled, SamusY = 0, EquippedItems = 0, AreaMiniBossDefeated = false };
        bool originalEnded = false;
        for (int update = 0; update < Updates; update++)
        {
            if (update == LastHealthUpdate)
                torizo.Health = 1;
            if (update == DefeatUpdate)
                torizo.Health = 0;
            // The original's glow tests the first enemy place's health at the start of its turn.
            originalEnded |= enemies.Places.InUpdateOrder[0].Health == 0;
            fx.Step(conditions);
            AssertEqual(!originalEnded, fx.IsRunning(glow), $"{definition}'s glow at update {update}");
            AssertEqual(update < DefeatUpdate, fx.IsRunning(glow), $"{definition}'s glow at update {update} runs until its Torizo's defeat");
        }
    }
}
