using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyChargeFlareProduction(SuperMetroidAddressSpace bus, ChargeFlareSpriteCatalog stock, ChargeFlareSpriteCatalog edited)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var placement = ChargeFlarePlacementCatalog.Load(new MemoryStream(ChargeFlarePlacementExtractor.Extract(bus)));
        var guarded = new ChargeFlareCompositionGuard(bus);
        int ticks = 0;
        foreach (byte pose in new byte[] { 1, 2, 9, 10 })
        foreach (ushort hyper in new ushort[] { 0, 1 })
        {
            var samus = new SamusState { Pose = pose, XPosition = 100, YPosition = 100, HyperBeam = hyper };
            SamusProjectileSystem native = Create(), compiled = Create(), changed = Create();
            for (int tick = 0; tick < 160 && native.FlareCounter != 0; tick++)
            {
                var a = new OamBuffer(); var b = new OamBuffer(); var c = new OamBuffer();
                native.HandleChargeFlareAndDraw(bus, a, samus, 0, 0, placement: placement);
                compiled.HandleChargeFlareAndDraw(guarded, b, samus, 0, 0, placement: placement, compositions: stock);
                changed.HandleChargeFlareAndDraw(guarded, c, samus, 0, 0, placement: placement, compositions: edited);
                AssertTrue(a.LowTable.SequenceEqual(b.LowTable) && a.HighTable.SequenceEqual(b.HighTable), "Normal/Hyper flare producer preserves complete native sprite output");
                AssertEqual(a.NextByteOffset, b.NextByteOffset, "Flare producer preserves OAM cursor");
                AssertEqual(a.NextByteOffset, c.NextByteOffset, "Edited flare producer preserves component count");
                for (int i = 0; i < a.NextByteOffset / 4; i++)
                {
                    AssertEqual((a.GetEntry(i).X + 7) & 511, c.GetEntry(i).X, "All emitted normal/Hyper components consume edited composition");
                    AssertEqual(a.GetEntry(i).Y, c.GetEntry(i).Y, "Composition X edit preserves emitted Y");
                }
                AssertTrue(Save(native).SequenceEqual(Save(compiled)) && Save(native).SequenceEqual(Save(changed)), "All projectile, flare-counter and animation state remains identical after visual replacement");
                ticks++;
            }
        }
        // An externally restored adjacent selector must retain its native result,
        // not become a newly fatal charge-only catalog lookup.
        var adjacentNative = Create(); var adjacentSelected = Create();
        foreach (var system in new[] { adjacentNative, adjacentSelected })
        {
            typeof(SamusProjectileSystem).GetProperty("FlareCounter")!.SetValue(system, (ushort)15);
            ((ushort[])typeof(SamusProjectileSystem).GetField("_flareFrames", flags)!.GetValue(system)!)[0] = 54;
            Array.Fill((ushort[])typeof(SamusProjectileSystem).GetField("_flareTimers", flags)!.GetValue(system)!, (ushort)100);
        }
        var first = new OamBuffer(); var second = new OamBuffer();
        var subject = new SamusState { Pose = 1, XPosition = 100, YPosition = 100 };
        adjacentNative.HandleChargeFlareAndDraw(bus, first, subject, 0, 0);
        adjacentSelected.HandleChargeFlareAndDraw(bus, second, subject, 0, 0, compositions: edited);
        AssertTrue(first.LowTable.SequenceEqual(second.LowTable) && first.HighTable.SequenceEqual(second.HighTable), "Non-catalog flare selector retains native adjacent-table output");

        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.Samus!.Pose = 1;
        runtime.Samus.XPosition = (ushort)(runtime.Camera!.XPosition + 100);
        runtime.Samus.YPosition = (ushort)(runtime.Camera.YPosition + 100);
        runtime.Samus.InitializeAnimation(bus); runtime.Samus.PrimeGraphics(bus);
        typeof(SamusProjectileSystem).GetProperty("FlareCounter")!.SetValue(runtime.Projectiles, (ushort)30);
        Array.Fill((ushort[])typeof(SamusProjectileSystem).GetField("_flareTimers", flags)!.GetValue(runtime.Projectiles)!, (ushort)100);
        var game = new SuperMetroidGame(bus);
        var runtimeField = typeof(SuperMetroidGame).GetField("runtime", flags)!;
        runtimeField.SetValue(game, runtime);
        byte[] baseline = Draw(runtime);
        game.BindChargeFlareCompositions(stock);
        AssertTrue(baseline.SequenceEqual(Draw(runtime)), "Runtime actor pass preserves stock flare composition");
        game.BindChargeFlareCompositions(edited);
        AssertTrue(!baseline.SequenceEqual(Draw(runtime)), "Runtime actor pass emits edited flare composition");
        game.BindChargeFlareCompositions(null);
        byte[] unbound = Save(game);
        game.BindChargeFlareCompositions(stock);
        byte[] bound = Save(game);
        AssertTrue(unbound.SequenceEqual(bound), "Charge-flare compositions are excluded from saved frontend/runtime graphs");
        using var stream = new MemoryStream(bound);
        var restored = SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Deserialize<SuperMetroidGame>(stream);
        var restoredRuntime = (SuperMetroidRuntime)runtimeField.GetValue(restored)!;
        AssertTrue(restoredRuntime.ChargeFlareCompositions is null, "Saved state does not retain old composition content");
        game.BindChargeFlareCompositions(edited); restored.BindChargeFlareCompositions(edited);
        AssertTrue(Draw(runtime).SequenceEqual(Draw(restoredRuntime)), "Restored actor pass uses current composition at saved animation state");
        Console.WriteLine($"Charge-flare production: {ticks} native/compiled/edited normal and Hyper ticks, ROM guard, state isolation and real actor restore/rebind pass.");

        static byte[] Save(object target)
        {
            using var output = new MemoryStream();
            SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Serialize(output, target);
            return output.ToArray();
        }
        static byte[] Draw(SuperMetroidRuntime target)
        {
            target.Oam.BeginFrame();
            typeof(SuperMetroidRuntime).GetMethod("DrawGameplayActors", flags)!.Invoke(target, new object?[] { false, null, null, false });
            return target.Oam.LowTable.ToArray().Concat(target.Oam.HighTable.ToArray()).Concat(BitConverter.GetBytes(target.Oam.NextByteOffset)).ToArray();
        }
        static SamusProjectileSystem Create()
        {
            var system = new SamusProjectileSystem();
            typeof(SamusProjectileSystem).GetProperty("FlareCounter")!.SetValue(system, (ushort)30);
            var frames = (ushort[])typeof(SamusProjectileSystem).GetField("_flareFrames", flags)!.GetValue(system)!;
            Array.Fill(frames, (ushort)3);
            Array.Fill((ushort[])typeof(SamusProjectileSystem).GetField("_flareTimers", flags)!.GetValue(system)!, (ushort)3);
            return system;
        }
    }

    private sealed class ChargeFlareCompositionGuard : ISnesAddressSpace
    {
        private readonly ISnesAddressSpace source;
        private readonly HashSet<int> forbidden = new();
        public ChargeFlareCompositionGuard(ISnesAddressSpace source)
        {
            this.source = source;
            for (int i = 0; i < ChargeFlareSpriteDefinitions.Selectors.Length * 2; i++)
                forbidden.Add(ChargeFlareSpriteDefinitions.SelectorTable + i);
            foreach (ushort pointer in ChargeFlareSpriteDefinitions.NativePointers)
            {
                int address = 0x930000 | pointer;
                int size = 2 + 5 * RomDataReader.ReadWordFixedBank(source, address);
                for (int i = 0; i < size; i++) forbidden.Add(address + i);
            }
        }
        public byte ReadByte(int address) => forbidden.Contains(address)
            ? throw new InvalidOperationException($"Flare composition read from ROM at {address:X6}.") : source.ReadByte(address);
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
