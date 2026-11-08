using SuperMetroid.Core.Enemies.Animals;
namespace SuperMetroid.Verification.Enemies.Animals;
using SuperMetroid.Verification.Harness;

/// <summary>
/// Checks the Dachora palette cycle's word wrap (#1419): a cleared cycle counted down borrows to frame 255 with 255
/// ticks, and the next step of either cycle carries past the word back to the bottom instead of growing past it.
/// </summary>
internal static class DachoraPaletteCycleWrapVerification
{
    /// <summary>Steps a cycle that a tick from zero left at the top of the word, for the speed and the shine cycles.</summary>
    internal static void VerifyDachoraPaletteCycleWrap()
    {
        var speed = new DachoraEnemyState();
        speed.SetGlowCycle(frame: 0, ticks: 0);
        AssertTrue(!speed.TickGlowCycle(), "a tick from a cleared cycle does not reach zero ticks");
        AssertEqual(255, speed.GlowFrame, "a tick from a cleared cycle borrows to frame 255");
        AssertEqual(255, speed.GlowTicks, "a tick from a cleared cycle leaves 255 ticks");
        speed.AdvanceSpeedGlow();
        AssertEqual(1, speed.GlowFrame, "the speed step from the top of the word wraps to frame one");
        AssertEqual(15, speed.GlowTicks, "the speed step from the top of the word keeps the wrapped ticks");

        var shine = new DachoraEnemyState();
        shine.SetGlowCycle(frame: 0, ticks: 0);
        _ = shine.TickGlowCycle();
        shine.AdvanceShineGlow();
        AssertEqual(0, shine.GlowFrame, "the shine step from the top of the word wraps to frame zero");
        AssertEqual(255, shine.GlowTicks, "the shine step from the top of the word keeps 255 ticks");
        Console.WriteLine("Dachora palette cycle: a cycle borrowed to the top of the word wraps on its next step.");
    }

    /// <summary>Runs the focused Dachora palette-cycle wrap check.</summary>
    [VerificationCommand("--dachora-palette-cycle-wrap")]
    internal static int DachoraPaletteCycleWrapCommand()
    {
        VerifyDachoraPaletteCycleWrap();
        return 0;
    }
}
