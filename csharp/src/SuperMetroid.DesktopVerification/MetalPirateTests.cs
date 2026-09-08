using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    /// <summary>
    /// Replays the player's stalled Metal Pirate through the real frontend. The local
    /// fixture is deliberately separate from the mutable live slot and contains private
    /// cartridge data. The first assertion failed before correcting the instruction return.
    /// </summary>
    private static void VerifyMetalPirates()
    {
        var loaded = DebuggerFixtureLoader.Load("metal-pirates-stall", 1);
        var runtime = loaded.Game.RuntimeForVerification!;
        int initialClaws = runtime.Enemies.NinjaSpacePirateStates[1]!.SpawnedClawCount;
        for (int frame = 0; frame <= 300; frame++)
        {
            if (frame % 60 == 0)
            {
                Console.WriteLine($"frame={frame} Samus={runtime.Samus!.XPosition},{runtime.Samus.YPosition}");
                int index = 0;
                foreach (var entry in runtime.Enemies.NinjaSpacePirateStates)
                {
                    var slot = runtime.Enemies.Slots[index++];
                    if (entry is null) continue;
                    Console.WriteLine($"slot={index-1} function={entry.Function} pc={slot.CurrentInstruction:X4} x={slot.XPosition} midpoint={entry.PostsMidpointX} claws={entry.SpawnedClawCount}");
                }
            }
            loaded.Game.Step(0);
        }
        if (runtime.Enemies.NinjaSpacePirateStates[1]!.SpawnedClawCount <= initialClaws)
            throw new InvalidOperationException("The saved pirate never resumed its claw attack after its active animation loop.");
        bool jumped = false;
        for (int frame = 0; frame < 240; frame++)
        {
            loaded.Game.Step((ushort)SnesButton.Left);
            jumped |= runtime.Enemies.NinjaSpacePirateStates.Any(state => state?.Function is
                NinjaSpacePirateFunction.SpinJumpLeftRising or NinjaSpacePirateFunction.SpinJumpRightRising or
                NinjaSpacePirateFunction.DivekickLeftJump or NinjaSpacePirateFunction.DivekickRightJump);
        }
        if (!jumped)
            throw new InvalidOperationException("Approaching the pirates after recovery never triggered a jump attack.");
        Console.WriteLine("Saved pirate resumed claw attacks and proximity-driven jump attacks.");
    }
}
