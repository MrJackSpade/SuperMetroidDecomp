using SuperMetroid.Core.Hardware;
using System.Reflection;
using System.Globalization;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Bosses.Ridley;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the reported first Ridley-room trajectory against a captured player movie segment.</summary>
internal static class RidleyPlayerOpeningVerification
{
    /// <summary>Checks arrival collision edges, then compares boss motion while feeding recorded Samus position and RNG as isolated inputs.</summary>
    internal static void VerifyRidleyPlayerOpening()
    {
        Func<RoomEnemySlot, Position16<RoomPixels>, Extent16<RoomPixels>, bool> overlaps = CeresRidleyBehavior.IsWithinRidleyRectangle;
        var bounds = new RoomEnemySystem().SlotTable.Slots[0];
        bounds.Radius = new(8, 8);
        // Native $A9:EF06 adds both radii plus one before its unsigned comparison.
        foreach (ushort targetRadius in new ushort[] { 2, 4, 8 })
        {
            ushort extent = (ushort)(8 + targetRadius);
            bounds.Position = bounds.Position.WithWhole(new((ushort)(128 + extent), (ushort)(256 - extent)));
            AssertTrue(overlaps(bounds, new(128, 256), new(targetRadius, targetRadius)), "Ridley arrival includes touching actor edges");
            bounds.Position = bounds.Position.WithWholeX(unchecked((ushort)(bounds.Position.X.Whole + 1)));
            AssertTrue(!overlaps(bounds, new(128, 256), new(targetRadius, targetRadius)), "Ridley arrival rejects one pixel past X edge");
            bounds.Position = bounds.Position.WithWholeX(128); bounds.Position = bounds.Position.WithWholeY(unchecked((ushort)(bounds.Position.Y.Whole - 1)));
            AssertTrue(!overlaps(bounds, new(128, 256), new(targetRadius, targetRadius)), "Ridley arrival rejects one pixel past Y edge");
        }

        // The player's first movie, from the completed entry into Ridley's room.
        // Feed the recorded Samus state and RNG to isolate the reported boss trajectory.
        int[] addresses = [0xfa8,0xf7a,0xf7c,0xf7e,0xf80,0xfaa,0xfac,0xaf6,0xafa,0xa1c,0x5e5,
            0x911,0x915,0x8b,0xf92,0xf94,0xf90,0xfa4,0xf8e,0xfb2];
        var rows = File.ReadAllLines("csharp/test-fixtures/issue-1266-ridley/opening-native.csv").Skip(1)
            .Select(line => line.Split(',')).ToDictionary(parts => int.Parse(parts[0], CultureInfo.InvariantCulture),
                parts => parts.Skip(1).Select(value => ushort.Parse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture)).ToArray());
        ushort[] Read(int frame) => rows[frame];
        ushort W(ushort[] words, int address) => words[Array.IndexOf(addresses, address)];
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xb32e, camera: new Position16<RoomPixels>(0, 0));
        var enemies = runtime.Enemies;
        var body = enemies.SlotTable.Slots[0]; var state = enemies.RidleyEncounter.Ridley; var samus = runtime.Player.IntroducedSamus;
        var seed = Read(375);
        body.CurrentInstruction = W(seed, 0xf92); body.InstructionTimer = NativeCountdown.FromRaw(W(seed, 0xf94));
        body.Timer = NativeCountdown.FromRaw(W(seed, 0xf90)); body.FrameCounter = W(seed, 0xfa4);
        body.SpritemapPointer = W(seed, 0xf8e);
        body.Position = body.Position.WithFractionX(W(seed, 0xf7c)); body.Position = body.Position.WithFractionY(W(seed, 0xf80));
        ushort random = 0;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        enemies.VerificationServices().ReadRandom = (Func<ushort>)(() => random);
        enemies.VerificationServices().NextRandom = (Func<ushort>)(() => random);
        for (int frame = 376; frame <= 744; frame++)
        {
            var expected = Read(frame);
            samus.Position = samus.Position.WithWhole(new(W(expected, 0xaf6), W(expected, 0xafa)));
            samus.Pose = (SamusPoseId)W(expected, 0xa1c); samus.PoseMetadata.RefreshCollisionRadii(bus);
            random = W(expected, 0x5e5);
            enemies.StepFrame(new RoomEnemyFrame { Camera = new(W(expected, 0x911), W(expected, 0x915)), TimeIsFrozen = false, Samus = samus, Level = runtime.Room.LevelData, ControllerInput = W(expected, 0x8b) });
            string actual = $"{(ushort)(state ?? throw new InvalidOperationException("Verification requires state.")).Function:X4},{body.Position.X.Whole:X4},{body.Position.X.Fraction:X4},{body.Position.Y.Whole:X4},{body.Position.Y.Fraction:X4},{state.Velocity.X.RawValue:X4},{state.Velocity.Y.RawValue:X4}";
            string native = string.Join(",", new[] {0xfa8,0xf7a,0xf7c,0xf7e,0xf80,0xfaa,0xfac}.Select(a => W(expected,a).ToString("X4")));
            if (actual != native)
                throw new InvalidDataException($"Ridley movie first divergence at frame {frame}: native {native}; port {actual}; instructions native={W(expected,0xf92):X4}/{W(expected,0xf94)} port={body.CurrentInstruction:X4}/{body.InstructionTimer.RawValue}; timer={state.FunctionTimer.RawValue} native={W(expected,0xfb2)}");
        }
        Console.WriteLine("Ridley player opening trajectory matches native movie frames 375..744.");
    }

    /// <summary>Runs the captured Ridley opening trajectory comparison.</summary>
    [VerificationCommand("--ridley-player-opening")]
    internal static int RidleyPlayerOpeningCommand()
    {
     VerifyRidleyPlayerOpening(); return 0; 
    }
}
