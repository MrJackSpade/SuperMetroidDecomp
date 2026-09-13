using SuperMetroid.Core.Runtime;

/// <summary>Private supplemental state for #442's two-actor collision-only CPU interval.</summary>
internal static class ZebetiteSkipSeed
{
    public static void Write(SuperMetroidRuntime runtime, string path)
    {
        var s = runtime.Samus!;
        using var output = new BinaryWriter(File.Create(path));
        output.Write("ZSK1"u8);
        ushort[] words = [runtime.NmiFrameCounter, runtime.System.RandomNumber,
            s.PoseHistory.PreviousPose, s.PoseHistory.PreviousDirectionAndMovement,
            s.PoseHistory.LastDifferentPose, s.PoseHistory.LastDifferentDirectionAndMovement,
            s.Health, s.InvincibilityTimer, runtime.Camera!.XPosition, runtime.Camera.YPosition];
        foreach (ushort word in words) output.Write(word);
        foreach (var e in runtime.Enemies.Slots.Where(e => e.NativeIndex is 128 or 192))
        {
            // Exact EnemyData byte order, including the paired bank/hurt-AI bytes.
            ushort[] actor = [e.EnemyDefinitionPointer, e.XPosition, e.XSubposition, e.YPosition,
                e.YSubposition, e.XRadius, e.YRadius, e.Properties, e.ExtraProperties, e.AiHandlerBits,
                e.Health, e.SpritemapPointer, e.Timer, e.CurrentInstruction, e.InstructionTimer,
                e.PaletteIndex, e.VramTilesIndex, e.Layer, e.FlashTimer, e.FrozenTimer,
                e.InvincibilityTimer, e.ShakeTimer, e.FrameCounter, (ushort)(e.AiBank | e.HurtAiTime << 8),
                e.VariableA, e.VariableB, e.VariableC, e.VariableD, e.VariableE, e.VariableF,
                e.Parameter1, e.Parameter2];
            foreach (ushort word in actor) output.Write(word);
        }
    }
}
