using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling;
using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static RetailRuntimeFixtureVerification;
using static SamusRenderingFixtureVerification;
using static SuiteTimingVerification;
using static VerificationHelpersVerification;
using static WordSamplesVerification;

/// <summary>Represents grapple firing definitions verification.</summary>
internal static class GrappleFiringDefinitionsVerification
{
    /// <summary>verify grapple firing definitions.</summary>
    internal static void VerifyGrappleFiringDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyGrappleLaunchXSelection), () => VerifyGrappleLaunchXSelection(rom));
        Suite(nameof(VerifyGrappleLaunchYSelection), () => VerifyGrappleLaunchYSelection(rom));
        Suite(nameof(VerifyGrappleLaunchAngleAlgorithm), () => VerifyGrappleLaunchAngleAlgorithm(rom));
        short Word(int address) => unchecked((short)(SnesLittleEndian.Word(GrappleFiringDefinitionsRomByte(rom, address), GrappleFiringDefinitionsRomByte(rom, address + 1))));
        var refresh = typeof(SamusGrappleMovement).GetMethod("RefreshFiringDrawOrigins", BindingFlags.NonPublic | BindingFlags.Static)!
            .CreateDelegate<Action<ISnesAddressSpace, SamusState, SamusGrappleState>>();
        Func<ISnesAddressSpace, SamusState, SamusGrappleState, Position16<RoomPixels>, bool, bool, GrappleMovementResult> connect = SamusGrappleMovement.ConnectAcceptedFiring;
        SamusPoseId sourcePose = SamusPoseId.FacingRightNormalPose;
        var bus = NewGrappleFiringReadGuard(rom, () => sourcePose);
        var samus = PrepareRetailSamusFixture(new SamusState { Pose = SamusPoseId.FacingRightNormalPose });
        var grapple = samus.Grapple;
        var stockFlare = grapple.FlarePlacement;
        var editedFlare = ChargeFlarePlacementCatalog.Load(new MemoryStream(
            ChargeFlarePlacementCatalog.Write(new()
            {
                Version = ChargeFlarePlacementDefinitions.Version,
                Offsets = new[] { false, true }.SelectMany(running => Enumerable.Range(0, 16)
                    .Select(direction => (running, direction))).ToDictionary(
                        entry => ChargeFlarePlacementDefinitions.Key(entry.running, entry.direction),
                        _ => new ChargeFlareOffset { X = 0x55aa, Y = 0x55aa }),
            })));
        // Exercise each compiled movement family with a real pose instead of rewriting
        // the immutable movement byte of standing pose $01.
        SamusPoseId[] movementPoses = Enumerable.Range(0, 253).GroupBy(pose =>
            GrappleFiringDefinitionsRomByte(rom, SamusMovementRomData.Poses.Definitions + pose * 8 + 1))
            .OrderBy(group => group.Key).Select(group => (SamusPoseId)group.First()).ToArray();
        // The graphics-Y byte only enters the flare as a signed offset, so the signed byte
        // classes cover it; each needs one full artwork fixture.
        ushort[] positions = [0x0000, 0x0001, 0x7fff, 0x8000, 0xfffe, 0xffff];
        for (byte direction = 0; direction < 10; direction++)
        {
            SamusPoseId[] aimingPoses = Enumerable.Range(0, 253).Where(pose =>
                GrappleFiringDefinitionsRomByte(rom, SamusMovementRomData.Poses.Definitions + pose * 8 + 3) == direction &&
                pose is not ((int)SamusPoseId.DraygonGrabbedMovingLeftPose or (int)SamusPoseId.DraygonGrabbedMovingRightPose))
                .Select(pose => (SamusPoseId)pose).ToArray();
            int offset = direction * 2;
            short vx = Word(0x9bc0db + offset), vy = Word(0x9bc0ef + offset);
            ushort angle = unchecked((ushort)Word(0x9bc104 + offset));
            foreach (byte graphicsY in SignedByteClasses)
            {
            samus.Presentation.TileTransfers.BindArtwork(SamusGraphicsOffsetFixture(unchecked((sbyte)graphicsY)));
            foreach (SamusPoseId pose in aimingPoses)
            foreach (ushort raw in positions)
            {
                // Every authored aim/movement pose for the direction, each signed graphics-Y
                // class and the X/Y position wrap boundaries.
                samus.Pose = sourcePose = pose;
                byte movement = GrappleFiringDefinitionsRomByte(rom, SamusMovementRomData.Poses.Definitions + (int)samus.Pose * 8 + 1);
                bool running = movement == 1;
                short x = Word((running ? 0x9bc172 : 0x9bc122) + offset);
                short y = Word((running ? 0x9bc186 : 0x9bc136) + offset);
                int correctedY = y - GrappleFiringDefinitionsRomByte(rom, 0x91b629 + (int)samus.Pose * 8 + 4);
                samus.Position = samus.Position.WithWhole(new((ushort)raw, unchecked((ushort)~raw)));
                grapple.Phase = GrapplePhase.Inactive;
                samus.LiquidPhysics.BeginFrameSoundRequests();
                SamusGrappleMovement.BeginFiring(bus, samus);
                AssertEqual(vx, grapple.ExtensionVelocity.X.SignedRawValue, "Actual launch X velocity");
                AssertEqual(vy, grapple.ExtensionVelocity.Y.SignedRawValue, "Actual launch Y velocity");
                AssertEqual(angle, grapple.Angle.RawValue, "Actual launch angle");
                AssertEqual(angle, grapple.MirroredAngle.RawValue, "Actual mirrored angle");
                AssertEqual(x, grapple.OriginOffset.X, "Actual physical origin X");
                AssertEqual(unchecked((short)correctedY), grapple.OriginOffset.Y, "Native physical correction independent of graphics");
                AssertEqual(unchecked((ushort)(samus.Position.X.Whole + x)), grapple.Anchor.X, "Launch anchor wraps X");
                AssertEqual(unchecked((ushort)(samus.Position.Y.Whole + correctedY)), grapple.Anchor.Y, "Launch anchor wraps Y");
                ushort anchorX = grapple.Anchor.X, anchorY = grapple.Anchor.Y;
                // Late drawing recomputes the physical hand after Samus moves; it must
                // not reuse stale offsets or move the independent collision endpoint.
                samus.Position += new Offset16<RoomPixels>(7, -5);
                grapple.OriginOffset = new(1234, 1234);
                refresh(bus, samus, grapple);
                AssertEqual(unchecked((ushort)(samus.Position.X.Whole + x)), grapple.RopeStart.X, "Late physical Start X");
                AssertEqual(unchecked((ushort)(samus.Position.Y.Whole + correctedY)), grapple.RopeStart.Y, "Late physical Start Y");
                AssertEqual(anchorX, grapple.Anchor.X, "Late draw retains endpoint X");
                AssertEqual(anchorY, grapple.Anchor.Y, "Late draw retains endpoint Y");
                short flareX = Word((running ? 0x9bc19a : 0x9bc14a) + offset);
                short flareY = Word((running ? 0x9bc1ae : 0x9bc15e) + offset);
                AssertEqual(unchecked((ushort)(samus.Position.X.Whole + flareX)), grapple.BeamStart.X, "Flare X remains separate");
                AssertEqual(unchecked((ushort)(samus.Position.Y.Whole + flareY - unchecked((sbyte)graphicsY))), grapple.BeamStart.Y, "Flare Y remains separate");
            }
            }
        }
        // Run the real four-substep extension path in empty terrain, not just its setup.
        // Presentation overrides must change Flare without changing trajectory or Start.
        var empty = CreateRoom(new(64, 64), new ushort[4096], new byte[4096]);
        foreach (bool replaceFlare in new[] { false, true })
        for (byte direction = 0; direction < 10; direction++)
        {
            grapple.FlarePlacement = replaceFlare ? editedFlare : stockFlare;
            samus.Pose = sourcePose = (SamusPoseId)Enumerable.Range(0, 253).First(pose =>
                GrappleFiringDefinitionsRomByte(rom, SamusMovementRomData.Poses.Definitions + pose * 8 + 3) == direction &&
                GrappleFiringDefinitionsRomByte(rom, SamusMovementRomData.Poses.Definitions + pose * 8 + 1) != 1);
            samus.Presentation.TileTransfers.BindArtwork(SamusGraphicsOffsetFixture(0));
            samus.Position = samus.Position.WithWhole(new(512, 512));
            grapple.Phase = GrapplePhase.Inactive;
            samus.LiquidPhysics.BeginFrameSoundRequests();
            SamusGrappleMovement.BeginFiring(bus, samus);
            if (replaceFlare)
                AssertEqual(0x55aa, grapple.FlareOffset.X, "Presentation override is actually consumed");
            for (int frame = 1; frame <= 10; frame++)
            {
                SamusGrappleMovement.StepFiring(bus, empty, samus, (ushort)SnesButton.X);
                int dx = Word(0x9bc0db + direction * 2) * 256 * frame;
                int dy = Word(0x9bc0ef + direction * 2) * 256 * frame;
                AssertEqual(dx, grapple.EndpointOffset.X.RawValue, "Native extension X trajectory");
                AssertEqual(dy, grapple.EndpointOffset.Y.RawValue, "Native extension Y trajectory");
                AssertEqual(unchecked((ushort)(512 + Word(0x9bc122 + direction * 2) + (dx >> 16))), grapple.Anchor.X, "Native endpoint X");
                AssertEqual(unchecked((ushort)(512 + Word(0x9bc136 + direction * 2) - GrappleFiringDefinitionsRomByte(rom, SamusMovementRomData.Poses.Definitions + (int)samus.Pose * 8 + 4) + (dy >> 16))), grapple.Anchor.Y, "Native endpoint Y with authored physical correction");
            }
        }
        grapple.FlarePlacement = stockFlare;
        // Moving Draygon-held poses use controller-derived direction and a fixed six-
        // pixel correction. Only the four direction bits select the aim, so exercise all
        // sixteen combinations with the other buttons clear, all set, and mixed.
        int directionBits = (int)(SnesButton.Left | SnesButton.Right | SnesButton.Up | SnesButton.Down);
        var heldInputs = new List<int>();
        for (int combination = 0; combination < 16; combination++)
        {
            int bits = 0;
            if ((combination & 1) != 0) bits |= (int)SnesButton.Left;
            if ((combination & 2) != 0) bits |= (int)SnesButton.Right;
            if ((combination & 4) != 0) bits |= (int)SnesButton.Up;
            if ((combination & 8) != 0) bits |= (int)SnesButton.Down;
            foreach (int others in new[] { 0, 0xffff & ~directionBits, 0x5555 & ~directionBits })
                heldInputs.Add(bits | others);
        }
        foreach (bool left in new[] { false, true })
        foreach (int input in heldInputs)
        {
            samus.Pose = left ? SamusPoseId.DraygonGrabbedMovingLeftPose : SamusPoseId.DraygonGrabbedMovingRightPose;
            samus.Position = samus.Position.WithWhole(new(512, 512));
            bool outward = (input & (int)(left ? SnesButton.Left : SnesButton.Right)) != 0;
            int direction = outward && (input & (int)SnesButton.Down) != 0 ? (left ? 6 : 3) :
                outward && (input & (int)SnesButton.Up) != 0 ? (left ? 8 : 1) : (left ? 7 : 2);
            grapple.Phase = GrapplePhase.Inactive;
            samus.LiquidPhysics.BeginFrameSoundRequests();
            SamusGrappleMovement.BeginFiring(bus, samus, (ushort)input);
            AssertEqual(direction, grapple.FireDirection, "Held aim selection");
            AssertEqual(Word(0x9bc0db + direction * 2), grapple.ExtensionVelocity.X.SignedRawValue, "Held launch X");
            AssertEqual(Word(0x9bc0ef + direction * 2), grapple.ExtensionVelocity.Y.SignedRawValue, "Held launch Y");
            AssertEqual(unchecked((ushort)Word(0x9bc104 + direction * 2)), grapple.Angle.RawValue, "Held launch angle");
            AssertEqual(512 + Word(0x9bc122 + direction * 2), grapple.Anchor.X, "Held anchor X");
            AssertEqual(506 + Word(0x9bc136 + direction * 2), grapple.Anchor.Y, "Held anchor Y correction");
        }
        // Actual locked connection subtracts the raw no-run origin, without graphics-Y
        // correction. Native connection/pose/animation records remain on the real bus.
        int locked = 0;
        foreach (byte movement in new byte[] { 0, 1, 5 })
        for (byte direction = 0; direction < 10; direction++)
        foreach (ushort coordinate in new ushort[] { 0, 512, ushort.MaxValue })
        {
            samus.Pose = sourcePose = movementPoses[movement];
            samus.Presentation.TileTransfers.BindArtwork(SamusGraphicsOffsetFixture(127));
            samus.Position = samus.Position.WithWhole(new(coordinate, coordinate));
            samus.Kinematics.YSpeed = samus.Kinematics.YSubspeed = 0;
            grapple.FireDirection = direction;
            grapple.Anchor = grapple.Anchor.WithX(unchecked((ushort)(coordinate + 24)));
            grapple.Anchor = grapple.Anchor.WithY(unchecked((ushort)(coordinate + 8)));
            grapple.RopeLength = 32;
            samus.LiquidPhysics.BeginFrameSoundRequests();
            connect(bus, samus, grapple, new(coordinate, coordinate), true, false);
            if (grapple.Phase != GrapplePhase.ConnectedLocked) continue;
            locked++;
            AssertEqual(unchecked((ushort)(grapple.RopeStart.X - Word(0x9bc122 + direction * 2))), samus.Position.X.Whole, "Locked connection body X");
            AssertEqual(unchecked((ushort)(grapple.RopeStart.Y - Word(0x9bc136 + direction * 2))), samus.Position.Y.Whole, "Locked connection body Y ignores graphics correction");
        }
        AssertEqual(54, locked, "All six locked directions in three source movement families at three boundaries");
        Suite(nameof(VerifyGrappleOriginXSelection), () => VerifyGrappleOriginXSelection(rom));
        Suite(nameof(VerifyGrappleOriginDefaultYSelection), () => VerifyGrappleOriginDefaultYSelection(rom));
        Suite(nameof(VerifyGrappleOriginRunningYSelection), () => VerifyGrappleOriginRunningYSelection(rom));
        Console.WriteLine("Grapple firing definitions: 70 native words, loud non-catalog origin rejection, every direction and aiming pose with every signed graphics-Y class and position wrap boundary, every held direction combination, 200 trajectory frames with flare overrides and 54 locked snaps pass; authored mechanics reads forbidden.");
    }

    /// <summary>grapple firing definitions rom byte.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for grapple firing definitions")]
    private static byte GrappleFiringDefinitionsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>verify grapple origin xselection.</summary>
    internal static void VerifyGrappleOriginXSelection(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyGrappleOriginField), () => VerifyGrappleOriginField(rom, GrappleFiringDefinitions.OriginXReferenceAddress,
            false, origin => origin.X, "default X alias"));
        Suite(nameof(VerifyGrappleOriginField), () => VerifyGrappleOriginField(rom, GrappleFiringDefinitions.RunningOriginXReferenceAddress,
            true, origin => origin.X, "running X alias"));
    }

    /// <summary>verify grapple origin default yselection.</summary>
    internal static void VerifyGrappleOriginDefaultYSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyGrappleOriginField), () => VerifyGrappleOriginField(rom, GrappleFiringDefinitions.OriginYReferenceAddress,
            false, origin => origin.Y, "default Y"));

    /// <summary>verify grapple origin running yselection.</summary>
    internal static void VerifyGrappleOriginRunningYSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyGrappleOriginField), () => VerifyGrappleOriginField(rom, GrappleFiringDefinitions.RunningOriginYReferenceAddress,
            true, origin => origin.Y, "running Y"));

    /// <summary>verify grapple origin field.</summary>
    private static void VerifyGrappleOriginField(SuperMetroidAddressSpace rom, int address,
        bool running, Func<(short X, short Y), short> select, string label)
    {
        for (byte direction = 0; direction < 10; direction++)
        {
            int source = address + 2 * direction;
            short expected = unchecked((short)(SnesLittleEndian.Word(GrappleOriginFieldRomByte(rom, source), GrappleOriginFieldRomByte(rom, source + 1))));
            AssertEqual(expected, select(GrappleFiringDefinitions.Origin(direction, running)),
                $"Grapple origin {label} direction {direction}");
        }
        foreach (byte direction in new byte[] { 10, byte.MaxValue })
            AssertThrows<InvalidDataException>(() => GrappleFiringDefinitions.Origin(direction, running),
                $"Grapple origin {label} bounds");
    }

    /// <summary>grapple origin field rom byte.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for grapple origin field")]
    private static byte GrappleOriginFieldRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>verify grapple launch xselection.</summary>
    internal static void VerifyGrappleLaunchXSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyGrappleLaunchField), () => VerifyGrappleLaunchField(rom, GrappleFiringDefinitions.XVelocityReferenceAddress,
            direction => unchecked((ushort)GrappleFiringDefinitions.Launch(direction).XVelocity), "X velocity"));

    /// <summary>verify grapple launch yselection.</summary>
    internal static void VerifyGrappleLaunchYSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyGrappleLaunchField), () => VerifyGrappleLaunchField(rom, GrappleFiringDefinitions.YVelocityReferenceAddress,
            direction => unchecked((ushort)GrappleFiringDefinitions.Launch(direction).YVelocity), "Y velocity"));

    /// <summary>verify grapple launch angle algorithm.</summary>
    internal static void VerifyGrappleLaunchAngleAlgorithm(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyGrappleLaunchField), () => VerifyGrappleLaunchField(rom, GrappleFiringDefinitions.AngleReferenceAddress,
            direction => GrappleFiringDefinitions.Launch(direction).Angle, "angle"));

    /// <summary>verify grapple launch field.</summary>
    private static void VerifyGrappleLaunchField(SuperMetroidAddressSpace rom, int source,
        Func<byte, ushort> select, string label)
    {
        for (byte direction = 0; direction < 10; direction++)
        {
            int address = source + 2 * direction;
            ushort original = (ushort)(SnesLittleEndian.Word(GrappleLaunchFieldRomByte(rom, address), GrappleLaunchFieldRomByte(rom, address + 1)));
            AssertEqual(original, select(direction), $"Original grapple {label}, direction {direction}");
        }
        AssertThrows<IndexOutOfRangeException>(() => select(10), $"Grapple {label} upper bound");
        AssertThrows<IndexOutOfRangeException>(() => select(byte.MaxValue), $"Grapple {label} invalid maximum");
    }

    /// <summary>grapple launch field rom byte.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for grapple launch field")]
    private static byte GrappleLaunchFieldRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Forbids the selected source pose's compiled movement and graphics-offset bytes and every migrated Grapple firing table.</summary>
    internal static MutableCartridgeReadGuard NewGrappleFiringReadGuard(ISnesAddressSpace source,
        Func<SamusPoseId> sourcePose)
    {
        int PoseRecord() => SamusMovementRomData.Poses.Definitions + (int)sourcePose() * 8;
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(address => address == PoseRecord() + 1, _ => "Compiled pose movement read ROM.")
            .Forbid(address => address == PoseRecord() + 4, _ => "Installed graphics offset read ROM.")
            .Forbid(address => address is >= 0x9bc14a and < 0x9bc172 or >= 0x9bc19a and < 0x9bc1c2,
                _ => "Installed Grapple flare read ROM.")
            .Forbid(address => address is >= 0x9bc0db and < 0x9bc103 or >= 0x9bc104 and < 0x9bc118 or
                    >= 0x9bc122 and < 0x9bc14a or >= 0x9bc172 and < 0x9bc19a,
                address => $"Compiled Grapple mechanics read ROM ${address:X6}.")
            .RejectWrites(_ => "Unexpected Grapple definition write.");
        return guard;
    }

    /// <summary>grapple firing definitions command.</summary>
    [VerificationCommand("--grapple-firing-definitions")]
    internal static int GrappleFiringDefinitionsCommand()
    {
        Suite(nameof(VerifyGrappleFiringDefinitions), () => VerifyGrappleFiringDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
