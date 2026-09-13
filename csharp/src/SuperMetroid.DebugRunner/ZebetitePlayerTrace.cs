using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Game;

/// <summary>Selected per-frame observations for the isolated original-CPU comparison.</summary>
internal static class ZebetitePlayerTrace
{
    public static void Write(TextWriter output, SuperMetroidRuntime runtime, int frame, ushort input)
    {
        var samus = runtime.Samus!;
        output.WriteLine(System.Text.Json.JsonSerializer.Serialize(new
        {
            Frame = frame, Input = input,
            samus.Kinematics.XFixed, samus.Kinematics.YFixed, samus.Pose,
            samus.AnimationFrame, samus.AnimationFrameTimer,
            CameraX = runtime.Camera!.XPosition, CameraY = runtime.Camera.YPosition,
            samus.Missiles, samus.Health,
            GenerationEventBits = (runtime.System.HasEvent(EventNumber.ZebetiteDestroyedBit0) ? 8 : 0) |
                (runtime.System.HasEvent(EventNumber.ZebetiteDestroyedBit1) ? 16 : 0) |
                (runtime.System.HasEvent(EventNumber.ZebetiteDestroyedBit2) ? 32 : 0),
            Shots = runtime.Projectiles.Slots.Select(shot => new { shot.Type, shot.XPosition, shot.YPosition }).ToArray(),
            // Keep observing the seeded physical indices after their actors die;
            // filtering by live header would silently change the comparison target.
            Barriers = runtime.Enemies.Slots.Where(slot => slot.NativeIndex is 128 or 384)
                .Select(slot => new { slot.NativeIndex, slot.EnemyDefinitionPointer, slot.Health, slot.FlashTimer, slot.AiHandlerBits }).ToArray(),
        }));
    }
}
