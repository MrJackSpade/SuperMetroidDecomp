using SuperMetroid.Core.Frontend;

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
            game.RuntimeForVerification.Samus.YPosition = 0x0150;
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
}
