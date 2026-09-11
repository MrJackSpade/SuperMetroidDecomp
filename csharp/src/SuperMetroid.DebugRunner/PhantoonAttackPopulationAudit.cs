using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rendering;

/// <summary>Normal no-input encounter observation: distinguish absent attacks from later rendering/motion failures.</summary>
internal static class PhantoonAttackPopulationAudit
{
    public static int Run(string rom, string directory)
    {
        Directory.CreateDirectory(directory);
        var runtime = PhantoonMaterializationAudit.CreateEncounter(rom);
        var maximum = new Dictionary<ushort, int>();
        var captured = new HashSet<ushort>();
        var firstFrames = new Dictionary<string, int>();
        using var trace = new StreamWriter(Path.Combine(directory, "attacks.csv"));
        trace.WriteLine("frame,phase,slot,preInstruction,x,y,angle,radius,delay,spritemap,canDamage,shootable");
        for (int frame = 0; frame < 3600; frame++)
        {
            runtime.StepFrame(0);
            foreach (var (name, first) in firstFrames)
            {
                if (frame - first is not (16 or 32 or 48)) continue;
                var scene = GameplayDisplayCapture.TryCaptureFrame(runtime)!;
                PngWriter.WriteRgba(Path.Combine(directory, $"{name}-after-{frame - first}.png"), 256, 224, SoftwareLayeredSnapshotRenderer.Render(scene));
                File.WriteAllBytes(Path.Combine(directory, $"{name}-after-{frame - first}.smframe"),
                    RenderFrameSnapshotCodec.Serialize(new(new(frame, 1, runtime.NmiFrameCounter), scene)));
            }
            foreach (var group in runtime.Enemies.EnemyProjectiles.Where(p => p.IsActive &&
                p.Kind == RoomEnemyProjectileKind.PhantoonDestroyableFlame).GroupBy(p => p.PreInstruction))
            {
                maximum[group.Key] = Math.Max(maximum.GetValueOrDefault(group.Key), group.Count());
                foreach (var flame in group)
                    trace.WriteLine($"{frame},{runtime.Enemies.Phantoon!.Body.VariableF:X4},{flame.SlotIndex},{flame.PreInstruction:X4},{flame.XPosition},{flame.YPosition},{flame.Variable0},{flame.YVelocity},{flame.XVelocity},{flame.SpritemapPointer:X4},{flame.CanDamageSamus},{flame.BlocksSamusProjectiles}");
                if (group.Key is not (EnemyProjectileCodePointers.PreInst_EnemyProj_PhantoonDestroyableFlame_Rain or
                    EnemyProjectileCodePointers.PreInst_EnemyProj_PhantoonDestroyableFlame_Spiral) ||
                    group.Count() != 8 || group.Any(p => p.SpritemapPointer == 0) || !captured.Add(group.Key)) continue;
                var full = GameplayDisplayCapture.TryCaptureFrame(runtime)!;
                string name = group.Key == EnemyProjectileCodePointers.PreInst_EnemyProj_PhantoonDestroyableFlame_Rain ? "rain" : "spiral";
                firstFrames.Add(name, frame);
                PngWriter.WriteRgba(Path.Combine(directory, name + ".png"), 256, 224, SoftwareLayeredSnapshotRenderer.Render(full));
                File.WriteAllBytes(Path.Combine(directory, name + ".smframe"),
                    RenderFrameSnapshotCodec.Serialize(new(new(frame, 1, runtime.NmiFrameCounter), full)));
                Console.WriteLine($"{name}: eight active flames with sprite maps at frame {frame}, phase {runtime.Enemies.Phantoon!.Body.VariableF:X4}.");
            }
        }
        foreach (ushort pre in new[] { EnemyProjectileCodePointers.PreInst_EnemyProj_PhantoonDestroyableFlame_Rain,
            EnemyProjectileCodePointers.PreInst_EnemyProj_PhantoonDestroyableFlame_Spiral })
            if (maximum.GetValueOrDefault(pre) != 8 || !captured.Contains(pre))
                throw new InvalidDataException($"Attack {pre:X4} never produced eight drawable flame records; max={maximum.GetValueOrDefault(pre)}.");
        Console.WriteLine("No-input encounter produces both eight-flame rain and spiral populations; motion/render/collision parity needs separate checks.");
        return 0;
    }
}
