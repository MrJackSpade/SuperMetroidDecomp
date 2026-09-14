using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyChargeFlarePlacement(SuperMetroidAddressSpace bus)
    {
        VerifyChargeFlareCompositions(bus);
        byte[] json = ChargeFlarePlacementExtractor.Extract(bus);
        var stock = ChargeFlarePlacementCatalog.Load(new MemoryStream(json));
        var document = JsonNode.Parse(json)!;
        foreach (var entry in document["offsets"]!.AsObject())
            entry.Value!["x"] = entry.Value["x"]!.GetValue<int>() + 7;
        var edited = Load(document);
        VerifyRuntimeFlarePlacement(bus, stock, edited);
        var system = new SamusProjectileSystem();
        var draw = typeof(SamusProjectileSystem).GetMethod("DrawFlareComponent", BindingFlags.Instance | BindingFlags.NonPublic)!
            .CreateDelegate<Action<ISnesAddressSpace, OamBuffer, SamusState, ushort, ushort, int, SamusMode7Transform?, ChargeFlarePlacementCatalog?, ChargeFlareSpriteCatalog?>>(system);
        int cases = 0;
        foreach (byte pose in new byte[] { 1, 2, 9, 10 })
        for (byte direction = 0; direction < 16; direction++)
        foreach (ushort coordinate in new ushort[] { 0, 100, 255, ushort.MaxValue })
        foreach (SamusMode7Transform? transform in new SamusMode7Transform?[] { null, new(240, 16, 65520, 128, 112) })
        for (int component = 0; component < 3; component++)
        {
            // Non-authored metadata makes the deliberate overread directions testable
            // without rewriting an authored pose's immutable aiming rules.
            var samus = new SamusState { Pose = 0xfd, XPosition = coordinate, YPosition = coordinate };
            var native = new OamBuffer(); var actual = new OamBuffer();
            draw(new FlarePlacementGuard(bus, pose, direction, false), native, samus, 0, 0, component, transform, null, null);
            draw(new FlarePlacementGuard(bus, pose, direction, true), actual, samus, 0, 0, component, transform, stock, null);
            AssertTrue(native.LowTable.SequenceEqual(actual.LowTable) && native.HighTable.SequenceEqual(actual.HighTable), "Flare placement stock preserves native OAM/culling for running, facing, overread directions and Mode7");
            AssertEqual(native.NextByteOffset, actual.NextByteOffset, "Flare placement preserves OAM admission");
            cases++;
        }
        var subject = new SamusState { Pose = 1, XPosition = 100, YPosition = 100 };
        byte[] beforeSamus = Save(subject), beforeSystem = Save(system);
        var original = new OamBuffer(); var changed = new OamBuffer();
        draw(bus, original, subject, 0, 0, 0, null, stock, null);
        draw(bus, changed, subject, 0, 0, 0, null, edited, null);
        AssertTrue(original.NextByteOffset > 0, "Edited flare placement fixture draws visible sprites");
        AssertEqual(original.NextByteOffset, changed.NextByteOffset, "Visual shift retains sprite count");
        for (int i = 0; i < original.NextByteOffset / 4; i++)
        {
            AssertEqual((original.GetEntry(i).X + 7) & 511, changed.GetEntry(i).X, "Editable placement shifts the actual flare OBJ seven pixels");
            AssertEqual(original.GetEntry(i).Y, changed.GetEntry(i).Y, "X-only flare edit preserves Y");
        }
        AssertTrue(beforeSamus.SequenceEqual(Save(subject)) && beforeSystem.SequenceEqual(Save(system)), "Flare art edit changes neither Samus physics nor any projectile/timer state");
        foreach (ushort hyper in new ushort[] { 0, 1 })
        {
            var originalSystem = new SamusProjectileSystem(); var editedSystem = new SamusProjectileSystem();
            foreach (var target in new[] { originalSystem, editedSystem })
            {
                typeof(SamusProjectileSystem).GetProperty("FlareCounter")!.SetValue(target, (ushort)30);
                var frames = (ushort[])typeof(SamusProjectileSystem).GetField("_flareFrames", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(target)!;
                Array.Fill(frames, (ushort)2);
            }
            subject.HyperBeam = hyper;
            original = new OamBuffer(); changed = new OamBuffer();
            var guarded = new FlarePlacementGuard(bus, subject.Pose, 2, true);
            originalSystem.HandleChargeFlareAndDraw(guarded, original, subject, 0, 0, placement: stock);
            editedSystem.HandleChargeFlareAndDraw(guarded, changed, subject, 0, 0, placement: edited);
            AssertTrue(original.NextByteOffset > 0, "Public normal/Hyper flare producer emits visible components");
            AssertEqual(original.NextByteOffset, changed.NextByteOffset, "Public flare placement preserves component admission");
            for (int i = 0; i < original.NextByteOffset / 4; i++)
                AssertEqual((original.GetEntry(i).X + 7) & 511, changed.GetEntry(i).X, "Public normal/Hyper path forwards selected placement to all components");
            AssertTrue(Save(originalSystem).SequenceEqual(Save(editedSystem)), "Normal/Hyper flare visual edits retain identical complete post-tick simulation state");
        }
        document["offsets"]!.AsObject().Remove(ChargeFlarePlacementDefinitions.Key(false, 0));
        AssertThrows<InvalidDataException>(() => Load(document), "Missing flare placement rejected");
        document = JsonNode.Parse(json)!;
        document["offsets"]![ChargeFlarePlacementDefinitions.Key(false, 0)]!["x"] = 32768;
        AssertThrows<InvalidDataException>(() => Load(document), "Out-of-range flare position rejected");
        document = JsonNode.Parse(json)!; document["damage"] = 1;
        AssertThrows<InvalidDataException>(() => Load(document), "Flare placement cannot contain mechanics fields");
        AssertThrows<InvalidDataException>(() => ChargeFlarePlacementCatalog.Load(new MemoryStream(Encoding.UTF8.GetBytes("{\"version\":1,\"version\":1}"))), "Duplicate flare properties rejected");
        Console.WriteLine($"Flare placement: {cases} native OAM cases with position ROM forbidden, visible edited displacement, whole-state isolation and invalid-resource rejection pass.");
        static ChargeFlarePlacementCatalog Load(JsonNode node) => ChargeFlarePlacementCatalog.Load(new MemoryStream(Encoding.UTF8.GetBytes(node.ToJsonString())));
        static byte[] Save(object value)
        {
            using var stream = new MemoryStream();
            SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Serialize(stream, value);
            return stream.ToArray();
        }
    }
    private sealed class FlarePlacementGuard(ISnesAddressSpace source, byte pose, byte direction, bool forbid) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            int syntheticOffset = address - (SamusMovementRomData.Poses.Definitions + 0xfd * 8);
            if ((uint)syntheticOffset < 8)
                return syntheticOffset == 3 ? direction : source.ReadByte(SamusMovementRomData.Poses.Definitions + pose * 8 + syntheticOffset);
            if (address == SamusMovementRomData.Poses.Definitions + pose * 8 + 3) return direction;
            if (forbid && address is >= 0x90c1a8 and < 0x90c210)
                throw new InvalidDataException("Flare placement still reads origin ROM.");
            return source.ReadByte(address);
        }
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }

    private static void VerifyRuntimeFlarePlacement(SuperMetroidAddressSpace bus, ChargeFlarePlacementCatalog stock, ChargeFlarePlacementCatalog edited)
    {
        var runtime = new SuperMetroid.Core.Runtime.SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.Samus!.Pose = 1;
        runtime.Samus.XPosition = (ushort)(runtime.Camera!.XPosition + 100);
        runtime.Samus.YPosition = (ushort)(runtime.Camera.YPosition + 100);
        runtime.Samus.InitializeAnimation(bus); runtime.Samus.PrimeGraphics(bus);
        typeof(SamusProjectileSystem).GetProperty("FlareCounter")!.SetValue(runtime.Projectiles, (ushort)30);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        Array.Fill((ushort[])typeof(SamusProjectileSystem).GetField("_flareTimers", flags)!.GetValue(runtime.Projectiles)!, (ushort)100);
        var game = new SuperMetroid.Core.Frontend.SuperMetroidGame(bus);
        var field = game.GetType().GetField("runtime", flags)!;
        field.SetValue(game, runtime);
        byte[] Draw(SuperMetroid.Core.Runtime.SuperMetroidRuntime target)
        {
            target.Oam.BeginFrame();
            target.GetType().GetMethod("DrawGameplayActors", flags)!.Invoke(target, new object?[] { false, null, null, false });
            return target.Oam.LowTable.ToArray();
        }
        byte[] native = Draw(runtime);
        game.BindChargeFlarePlacement(stock);
        AssertTrue(native.SequenceEqual(Draw(runtime)), "Runtime stock flare placement preserves complete actor OAM");
        game.BindChargeFlarePlacement(edited);
        AssertTrue(!native.SequenceEqual(Draw(runtime)), "Runtime actor pass consumes edited flare placement");
        game.BindChargeFlarePlacement(null);
        using var without = new MemoryStream();
        SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Serialize(without, game);
        game.BindChargeFlarePlacement(stock);
        using var with = new MemoryStream();
        SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Serialize(with, game);
        AssertTrue(without.ToArray().SequenceEqual(with.ToArray()), "Flare catalog does not enter frontend/runtime saved graphs");
        with.Position = 0;
        var restored = SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Deserialize<SuperMetroid.Core.Frontend.SuperMetroidGame>(with);
        var restoredRuntime = (SuperMetroid.Core.Runtime.SuperMetroidRuntime)field.GetValue(restored)!;
        AssertTrue(restoredRuntime.ChargeFlarePlacement is null, "Restored flare catalog requires host rebind");
        game.BindChargeFlarePlacement(edited); restored.BindChargeFlarePlacement(edited);
        AssertTrue(Draw(runtime).SequenceEqual(Draw(restoredRuntime)), "Restored runtime emits current flare content at the saved animation state");
    }
}
