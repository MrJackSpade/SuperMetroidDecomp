using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyGrappleFlarePresentation(SuperMetroidAddressSpace bus, ChargeFlareSpriteCatalog stock, ChargeFlareSpriteCatalog edited)
    {
        var guard = new GrappleFlarePresentationGuard(new ChargeFlareCompositionGuard(bus));
        foreach (int address in new[] { SamusGrappleRomData.Firing.RightFlareSpritemapOffsets, SamusGrappleRomData.Firing.LeftFlareSpritemapOffsets })
            AssertEqual(RomDataReader.ReadWordFixedBank(bus, address), ChargeFlareSpriteDefinitions.MainFlareSelectorOffset, "Both native Grapple-facing rows select the same main flare");
        int cases = 0;
        foreach (GrapplePhase phase in Enum.GetValues<GrapplePhase>())
        foreach (byte pose in new byte[] { 1, 2, 9, 10 })
        foreach (ushort counter in new ushort[] { 0, 1, 2 })
        foreach (ushort coordinate in new ushort[] { 0, 1, 100, 255, 256, 511, 32768, 65535 })
        {
            SamusState native = Seed(), actual = Seed(), changed = Seed();
            Check(native, actual, changed);
            SamusState Seed()
            {
                var samus = new SamusState { Pose = pose, XPosition = coordinate, YPosition = coordinate };
                samus.Grapple.Phase = phase;
                samus.Grapple.FireDirection = (byte)(pose is 2 or 10 ? 7 : 2);
                samus.Grapple.FlareCounter = counter;
                samus.Grapple.FlareAnimationFrame = 29;
                samus.Grapple.FlareAnimationTimer = 0;
                samus.Grapple.BeamStartX = coordinate; samus.Grapple.BeamStartY = coordinate;
                samus.Grapple.RopeStartX = 70; samus.Grapple.RopeStartY = 80;
                samus.Grapple.AnchorX = 200; samus.Grapple.AnchorY = 100;
                samus.Grapple.RopeLength = 40; samus.Grapple.AngularVelocity = 123;
                return samus;
            }
        }
        foreach (byte pose in new byte[] { 9, 10 })
        {
            var native = new SamusState { Pose = pose, YPosition = 100 };
            native.Grapple.Phase = GrapplePhase.Firing;
            native.Grapple.FireDirection = (byte)(pose == 10 ? 7 : 2);
            native.Grapple.FlareCounter = 1;
            var actual = Clone(native); var changed = Clone(native);
            for (int tick = 0; tick < 512; tick++)
            {
                native.XPosition = actual.XPosition = changed.XPosition = (ushort)(tick % 256);
                Check(native, actual, changed);
                // Native post-Samus tile upload owns this counter, not the flare.
                native.Grapple.FlareCounter = actual.Grapple.FlareCounter = changed.Grapple.FlareCounter = (ushort)Math.Min(120, native.Grapple.FlareCounter + 1);
            }
        }
        // Exercise negative timers and non-main delay bytes while vertically culled,
        // so adjacent instruction data is tested without inventing valid sprite IDs.
        foreach (ushort timer in new ushort[] { 0, 1, 2, 32768, 65535 })
        foreach (ushort frame in new ushort[] { 0, 15, 16, 29, 30, 31, 36, 37, 43, 44, 45, 255, 65535 })
        {
            var samus = new SamusState { Pose = 1 };
            samus.Grapple.Phase = GrapplePhase.ConnectedLocked;
            samus.Grapple.FlareCounter = 2;
            samus.Grapple.FlareAnimationFrame = frame;
            samus.Grapple.FlareAnimationTimer = timer;
            samus.Grapple.BeamStartY = 256;
            Check(samus, Clone(samus), Clone(samus));
        }
        VerifyGrappleFlareActorBinding(bus, stock, edited);
        Console.WriteLine($"Grapple flare: {cases} phase, position, sentinel, rewind and timer-boundary cases preserve native cadence/OAM and complete Samus state with visual/cadence ROM forbidden; actual actor override/rebind passes.");

        void Check(SamusState native, SamusState actual, SamusState changed)
        {
            ushort expectedFrame = native.Grapple.FlareAnimationFrame, expectedTimer = native.Grapple.FlareAnimationTimer;
            if (SamusGrappleMovement.UsesBeamSpecificDrawingPath(native.Grapple.Phase) && native.Grapple.FlareCounter != 0)
            {
                if (native.Grapple.FlareCounter == 1) { expectedFrame = 16; expectedTimer = 3; }
                expectedTimer = unchecked((ushort)(expectedTimer - 1));
                if ((short)expectedTimer < 0)
                {
                    expectedFrame = unchecked((ushort)(expectedFrame + 1));
                    byte delay = bus.ReadByte(SamusGrappleRomData.Firing.MainFlareAnimationDelays + expectedFrame);
                    if (delay == 254)
                    {
                        byte rewind = bus.ReadByte(SamusGrappleRomData.Firing.MainFlareAnimationDelays + unchecked((ushort)(expectedFrame + 1)));
                        expectedFrame = unchecked((ushort)(expectedFrame - rewind));
                        delay = bus.ReadByte(SamusGrappleRomData.Firing.MainFlareAnimationDelays + expectedFrame);
                    }
                    expectedTimer = delay;
                }
            }
            var a = new OamBuffer(); var b = new OamBuffer(); var c = new OamBuffer();
            bool visible = SamusGrappleMovement.DrawFlareBeforeSamus(bus, native, a, 0, 0);
            AssertEqual(visible, SamusGrappleMovement.DrawFlareBeforeSamus(guard, actual, b, 0, 0, stock), "Grapple catalog retains native flare visibility");
            AssertEqual(visible, SamusGrappleMovement.DrawFlareBeforeSamus(guard, changed, c, 0, 0, edited), "Grapple art edit retains origin visibility gate");
            AssertEqual((expectedFrame, expectedTimer), (actual.Grapple.FlareAnimationFrame, actual.Grapple.FlareAnimationTimer), "Grapple shared compiled cadence matches independent native reads");
            AssertTrue(a.LowTable.SequenceEqual(b.LowTable) && a.HighTable.SequenceEqual(b.HighTable) && a.NextByteOffset == b.NextByteOffset, "Grapple stock parts preserve native complete OAM");
            AssertEqual(a.NextByteOffset, c.NextByteOffset, "Grapple art edit preserves sprite count");
            for (int i = 0; i < a.NextByteOffset / 4; i++)
            {
                AssertEqual((a.GetEntry(i).X + 7) & 511, c.GetEntry(i).X, "Grapple emits edited visual part offsets");
                AssertEqual(a.GetEntry(i).Y, c.GetEntry(i).Y, "Grapple X-only visual edit preserves Y");
            }
            AssertTrue(SaveGrappleFixture(native).SequenceEqual(SaveGrappleFixture(actual)) && SaveGrappleFixture(native).SequenceEqual(SaveGrappleFixture(changed)), "Flare edits preserve complete Samus/Grapple state including physical origins and timers");
            cases++;
        }
        static SamusState Clone(SamusState source)
        {
            using var stream = new MemoryStream(SaveGrappleFixture(source));
            return SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Deserialize<SamusState>(stream);
        }
    }

    private static byte[] SaveGrappleFixture(object value)
    {
        using var stream = new MemoryStream();
        SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Serialize(stream, value);
        return stream.ToArray();
    }

    private static void VerifyGrappleFlareActorBinding(SuperMetroidAddressSpace bus, ChargeFlareSpriteCatalog stock, ChargeFlareSpriteCatalog edited)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug); runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        var samus = runtime.Samus!;
        samus.Pose = 1; samus.XPosition = (ushort)(runtime.Camera!.XPosition + 100); samus.YPosition = (ushort)(runtime.Camera.YPosition + 100);
        samus.InitializeAnimation(bus); samus.PrimeGraphics(bus);
        samus.Grapple.Phase = GrapplePhase.ConnectedLocked;
        samus.Grapple.BeamStartX = samus.XPosition; samus.Grapple.BeamStartY = samus.YPosition;
        samus.Grapple.FlareCounter = 2; samus.Grapple.FlareAnimationFrame = 16; samus.Grapple.FlareAnimationTimer = 100;
        var game = new SuperMetroidGame(bus);
        var field = typeof(SuperMetroidGame).GetField("runtime", flags)!;
        field.SetValue(game, runtime);
        byte[] original = Draw(runtime);
        AssertTrue(runtime.LastGrappleFlareDrawn, "Real actor fixture reaches Grapple flare path");
        game.BindChargeFlareCompositions(stock);
        AssertTrue(original.SequenceEqual(Draw(runtime)), "Real Grapple actor stock composition preserves OAM");
        game.BindChargeFlareCompositions(edited);
        AssertTrue(!original.SequenceEqual(Draw(runtime)), "Real Grapple actor consumes current composition override");
        using var stream = new MemoryStream(SaveGrappleFixture(game));
        var restored = SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Deserialize<SuperMetroidGame>(stream);
        restored.BindChargeFlareCompositions(edited);
        AssertTrue(Draw(runtime).SequenceEqual(Draw((SuperMetroidRuntime)field.GetValue(restored)!)), "Restored Grapple actor uses current art at the saved timing state");
        static byte[] Draw(SuperMetroidRuntime target)
        {
            target.Oam.BeginFrame();
            typeof(SuperMetroidRuntime).GetMethod("DrawGameplayActors", flags)!.Invoke(target, new object?[] { false, null, null, false });
            return target.Oam.LowTable.ToArray().Concat(target.Oam.HighTable.ToArray()).Concat(BitConverter.GetBytes(target.Oam.NextByteOffset)).ToArray();
        }
    }

    private sealed class GrappleFlarePresentationGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address) => address is >= 0x90c481 and < 0x90c4b5 or >= 0x93a225 and < 0x93a231
            ? throw new InvalidOperationException($"Grapple flare still reads compiled cadence/selector at {address:X6}.") : source.ReadByte(address);
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
