using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Input;

/// <summary>Production missile-hit branches in the real room, followed by unmodified frame-driven AI.</summary>
internal static class PhantoonHitFadeAudit
{
    public static int Run(string rom, string directory)
    {
        Directory.CreateDirectory(directory);
        foreach (bool super in new[] { false, true })
        {
            string name = super ? "rage" : "swoop";
            var runtime = PhantoonMaterializationAudit.CreateEncounter(rom);
            var boss = runtime.Enemies.Phantoon!;
            var samus = runtime.Samus!;
            samus.SelectedHudItem = super ? (ushort)2 : (ushort)1;
            samus.Missiles = samus.MaxMissiles = samus.SuperMissiles = samus.MaxSuperMissiles = 10;
            bool hit = false;
            var phases = new Dictionary<PhantoonAiFunction, HashSet<int>>();
            using var trace = new StreamWriter(Path.Combine(directory, name + ".csv"));
            trace.WriteLine("frame,phase,hits,health,paletteEnergy,additive,contributingPixels");
            for (int frame = 0; frame < 3600; frame++)
            {
                runtime.StepFrame(0);
                if (!hit && boss.Body.VariableF == (ushort)PhantoonAiFunction.EyeTracksSamus &&
                    !boss.Body.Properties.HasAny(EnemyProperties.IgnoreSamusCollision) &&
                    TryFindEyeContact(runtime.Enemies, boss.Body, out ushort hitX, out ushort hitY))
                {
                    // Construct contact, not an AI transition: use the real missile
                    // initializer and extended-spritemap hitbox/damage dispatcher.
                    // This test does not measure controller aiming or missile travel.
                    var fired = runtime.Projectiles.TryFireMissile(runtime.AddressSpace, samus,
                        (ushort)SnesButton.X, 0, runtime.BombProjectiles);
                    if (fired.Slot is not { } index) throw new InvalidDataException("Could not initialize fixture missile.");
                    var projectile = runtime.Projectiles.Slots[index];
                    projectile.XPosition = hitX;
                    projectile.YPosition = hitY;
                    runtime.Enemies.ResolvePhantoonProjectileHits(runtime.AddressSpace, runtime.Projectiles, runtime.BombProjectiles);
                    if (boss.AcceptedProjectileHits != 1 || boss.LastProjectileDamage != (super ? 600 : 100))
                        throw new InvalidDataException($"{name}: contact did not reach damaging eye hitbox: hits={boss.AcceptedProjectileHits}, damage={boss.LastProjectileDamage}.");
                    hit = true;
                }
                if (!hit) continue;
                var phase = (PhantoonAiFunction)boss.Body.VariableF;
                if (!IsFade(phase)) continue;
                var full = GameplayDisplayCapture.TryCaptureFrame(runtime)!;
                var basis = GameplayDisplayCapture.CaptureOrdinaryBase(runtime);
                var ordinary = (OrdinaryGameplayRenderLayer)basis.Layers[0];
                var without = new OrdinaryGameplayRenderLayer(ordinary.Registers with
                    { MainScreenLayers = ordinary.Registers.MainScreenLayers & ~SnesMainScreenLayers.Bg2 },
                    ordinary.HorizontalScrolls, ordinary.VerticalScrolls);
                var referenceLayers = full.Layers.ToArray();
                referenceLayers[0] = without;
                var background = SoftwareLayeredSnapshotRenderer.Render(new(full.Memory, referenceLayers, full.ObjectSelection, full.Brightness));
                var actual = SoftwareLayeredSnapshotRenderer.Render(full);
                bool additive = full.Layers[0] is XrayGameplayRenderLayer { SubscreenUsesBg2: true };
                int contributing = 0;
                if (additive)
                {
                    for (int i = 32 * 256; i < actual.Length; i++)
                    {
                        if (actual[i].R < background[i].R || actual[i].G < background[i].G || actual[i].B < background[i].B)
                            throw new InvalidDataException($"{name} {phase}: additive body erased scenery.");
                        if (actual[i] != background[i]) contributing++;
                    }
                    if (!actual.AsSpan(0, 32 * 256).SequenceEqual(background.AsSpan(0, 32 * 256)))
                        throw new InvalidDataException("Fade altered HUD.");
                }
                int energy = 0;
                foreach (ushort color in runtime.Cgram.Colors.Slice(112, 16))
                    energy += (color & 31) + ((color >> 5) & 31) + ((color >> 10) & 31);
                if (!phases.TryGetValue(phase, out var values)) phases[phase] = values = new();
                values.Add(contributing);
                trace.WriteLine($"{frame},{phase},{boss.AcceptedProjectileHits},{boss.Body.Health},{energy},{additive},{contributing}");
                if (values.Count == 3)
                    File.WriteAllBytes(Path.Combine(directory, $"{name}-{phase}.smframe"),
                        RenderFrameSnapshotCodec.Serialize(new(new(frame, 1, runtime.NmiFrameCounter), full)));
            }
            var required = super
                ? new[] { PhantoonAiFunction.FadeOutBeforeRage, PhantoonAiFunction.FadeInForRage, PhantoonAiFunction.FadeOutAfterRage }
                : new[] { PhantoonAiFunction.FadeOutWhileSwooping, PhantoonAiFunction.FadeInBeforeFigureEight };
            foreach (var phase in required)
                if (!phases.TryGetValue(phase, out var values) || values.Count < 3 || values.Max() == 0)
                    throw new InvalidDataException($"{name}: no visibly varying fade coverage for {phase}.");
            Console.WriteLine($"{name}: accepted {boss.LastProjectileDamage}-damage production hit; visible fades verified for {string.Join(", ", required)}.");
        }
        return 0;
    }

    private static bool IsFade(PhantoonAiFunction phase) => phase is
        PhantoonAiFunction.FadeOutWhileSwooping or PhantoonAiFunction.FadeInBeforeFigureEight or
        PhantoonAiFunction.FadeOutBeforeRage or PhantoonAiFunction.FadeInForRage or PhantoonAiFunction.FadeOutAfterRage;

    private static bool TryFindEyeContact(RoomEnemySystem enemies, RoomEnemySlot body, out ushort x, out ushort y)
    {
        const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var query = typeof(RoomEnemySystem).GetMethod("TryFindExtendedHitboxCallback", flags)!;
        ushort damageCallback = (ushort)typeof(RoomEnemySystem).GetField("PhantoonShotHitboxCallback",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!.GetRawConstantValue()!;
        for (int dy = -48; dy <= 48; dy += 4)
        for (int dx = -48; dx <= 48; dx += 4)
        {
            x = unchecked((ushort)(body.XPosition + dx)); y = unchecked((ushort)(body.YPosition + dy));
            object[] args = [body, x, y, (ushort)4, (ushort)4, true, (ushort)0];
            if ((bool)query.Invoke(enemies, args)! && (ushort)args[6] == damageCallback) return true;
        }
        x = y = 0;
        return false;
    }
}
