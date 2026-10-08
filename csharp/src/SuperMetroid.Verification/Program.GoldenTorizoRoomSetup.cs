using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;

internal static partial class Program
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
            game.RuntimeForVerification!.Samus!.XPosition = 0x0180;
            game.RuntimeForVerification!.Samus!.YPosition = 0x0150;
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
    private static void ForceGoldenTorizoLeftTurn(
        SuperMetroidGame native, SuperMetroidGame installed, ushort instruction)
    {
        PrimeGoldenTorizoAwakenedRoom(native, installed);
        foreach (SuperMetroidGame game in new[] { native, installed })
        {
            RoomEnemySlot boss = game.RuntimeForVerification!.Enemies.Slots.Single(
                slot => slot.EnemyDefinitionPointer ==
                    RoomEnemySystem.GoldenTorizoDefinition);
            AssertTrue(boss.SpritemapPointer !=
                    GoldenTorizoLeftTurnInstructionProgramDefinitions.FacingScreenFrame,
                "forced left-turn starts before the shared facing-screen frame");
            boss.CurrentInstruction = instruction;
            boss.InstructionTimer = 1;
        }
    }
}
