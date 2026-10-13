using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Prepares an awakened retail Golden Torizo room before selecting a combat-only turn for comparison.</summary>
internal static class GoldenTorizoRoomSetupVerification
{
    /// <summary>
    /// A newly loaded Golden Torizo is still an immobile statue. Injecting a
    /// combat-only turn there would test an invalid starting state. Advance
    /// the authored wake rectangle into active combat before selecting a
    /// callable list; the later $AA:BC78 falling handoff still occurs.
    /// </summary>
    private static void PrimeGoldenTorizoAwakenedRoom(
        SuperMetroidGame native, SuperMetroidGame installed)
    {
        foreach (SuperMetroidGame game in new[] { native, installed })
        {
            game.GameplayRuntimeForVerification.Player.IntroducedSamus.Position = game.GameplayRuntimeForVerification.Player.IntroducedSamus.Position.WithWhole(new(0x0180, 0x0150));
        }
        for (int frame = 0; frame < 450; frame++)
        {
            FrontendFrame expected = native.Step(0);
            FrontendFrame actual = installed.Step(0);
            AssertEqual(SuperMetroidGameState.MainGameplay, actual.GameState,
                $"Golden Torizo turn preparation remains active at frame {frame}");
            AssertTrue(actual.Pixels.AsSpan().SequenceEqual(expected.Pixels),
                $"Golden Torizo turn preparation matches native pixels at frame {frame}");
        }
    }

    /// <summary>
    /// Start both real-room instances from the same awakened boss state before
    /// selecting a callable turn. The fixture preserves the cartridge's room
    /// and enemy population; only the instruction cursor is chosen for coverage.
    /// </summary>
    internal static void ForceGoldenTorizoLeftTurn(
        SuperMetroidGame native, SuperMetroidGame installed, ushort instruction)
    {
        PrimeGoldenTorizoAwakenedRoom(native, installed);
        foreach (SuperMetroidGame game in new[] { native, installed })
        {
            RoomEnemySlot boss = game.GameplayRuntimeForVerification.Enemies.SlotTable.Slots.Single(
                slot => slot.EnemyDefinitionPointer ==
                    EnemyDefinitionId.GoldenTorizo);
            AssertTrue(boss.SpritemapPointer !=
                    GoldenTorizoLeftTurnInstructionProgramDefinitions.FacingScreenFrame,
                "forced left-turn starts before the shared facing-screen frame");
            boss.CurrentInstruction = instruction;
            boss.InstructionTimer = NativeCountdown.Start(1);
        }
    }
}
