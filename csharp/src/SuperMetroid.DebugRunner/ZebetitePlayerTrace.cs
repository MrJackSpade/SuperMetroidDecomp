using SuperMetroid.Core.Runtime;

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
            Shots = runtime.Projectiles.Slots.Select(shot => new { shot.Type, shot.XPosition, shot.YPosition }).ToArray(),
            Barriers = runtime.Enemies.Slots.Where(slot => slot.EnemyDefinitionPointer == 0xe27f)
                .Select(slot => new { slot.Health, slot.FlashTimer, slot.AiHandlerBits }).ToArray(),
        }));
    }
}
