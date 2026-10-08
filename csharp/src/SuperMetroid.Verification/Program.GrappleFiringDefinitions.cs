using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    private static void VerifyGrappleFiringDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyGrappleLaunchXSelection), () => VerifyGrappleLaunchXSelection(rom));
        Suite(nameof(VerifyGrappleLaunchYSelection), () => VerifyGrappleLaunchYSelection(rom));
        Suite(nameof(VerifyGrappleLaunchAngleAlgorithm), () => VerifyGrappleLaunchAngleAlgorithm(rom));
        short Word(int address) => unchecked((short)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8));
        var refresh = typeof(SamusGrappleMovement).GetMethod("RefreshFiringDrawOrigins", BindingFlags.NonPublic | BindingFlags.Static)!
            .CreateDelegate<Action<ISnesAddressSpace, SamusState, SamusGrappleState>>();
        Func<ISnesAddressSpace, SamusState, SamusGrappleState, ushort, ushort, bool, bool, GrappleMovementResult> connect = SamusGrappleMovement.ConnectAcceptedFiring;
        var bus = new GrappleFiringReadGuard(rom);
        var samus = PrepareRetailSamusFixture(new SamusState { Pose = SamusPoseIds.FacingRightNormalPose });
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
        byte[] movementPoses = Enumerable.Range(0, 253).GroupBy(pose =>
            rom.ReadByte(SamusMovementRomData.Poses.Definitions + pose * 8 + 1))
            .OrderBy(group => group.Key).Select(group => (byte)group.First()).ToArray();
        // The graphics-Y byte only enters the flare as a signed offset, so the signed byte
        // classes cover it; each needs one full artwork fixture.
        ushort[] positions = [0x0000, 0x0001, 0x7fff, 0x8000, 0xfffe, 0xffff];
        for (byte direction = 0; direction < 10; direction++)
        {
            byte[] aimingPoses = Enumerable.Range(0, 253).Where(pose =>
                rom.ReadByte(SamusMovementRomData.Poses.Definitions + pose * 8 + 3) == direction &&
                pose is not (SamusPoseIds.DraygonGrabbedMovingLeftPose or SamusPoseIds.DraygonGrabbedMovingRightPose))
                .Select(pose => (byte)pose).ToArray();
            int offset = direction * 2;
            short vx = Word(0x9bc0db + offset), vy = Word(0x9bc0ef + offset);
            ushort angle = unchecked((ushort)Word(0x9bc104 + offset));
            foreach (byte graphicsY in SignedByteClasses)
            {
            bus.GraphicsY = graphicsY;
            samus.TileTransfers.BindArtwork(SamusGraphicsOffsetFixture(unchecked((sbyte)graphicsY)));
            foreach (byte pose in aimingPoses)
            foreach (ushort raw in positions)
            {
                // Every authored aim/movement pose for the direction, each signed graphics-Y
                // class and the X/Y position wrap boundaries.
                samus.Pose = bus.SourcePose = pose;
                byte movement = rom.ReadByte(SamusMovementRomData.Poses.Definitions + samus.Pose * 8 + 1);
                bus.Direction = direction;
                bool running = movement == 1;
                short x = Word((running ? 0x9bc172 : 0x9bc122) + offset);
                short y = Word((running ? 0x9bc186 : 0x9bc136) + offset);
                int correctedY = y - rom.ReadByte(0x91b629 + samus.Pose * 8 + 4);
                samus.XPosition = (ushort)raw;
                samus.YPosition = unchecked((ushort)~raw);
                grapple.Phase = GrapplePhase.Inactive;
                samus.LiquidPhysics.BeginFrameSoundRequests();
                SamusGrappleMovement.BeginFiring(bus, samus);
                AssertEqual(vx, grapple.ExtensionXVelocity, "Actual launch X velocity");
                AssertEqual(vy, grapple.ExtensionYVelocity, "Actual launch Y velocity");
                AssertEqual(angle, grapple.Angle.RawValue, "Actual launch angle");
                AssertEqual(angle, grapple.MirroredAngle.RawValue, "Actual mirrored angle");
                AssertEqual(x, grapple.OriginXOffset, "Actual physical origin X");
                AssertEqual(unchecked((short)correctedY), grapple.OriginYOffset, "Native physical correction independent of graphics");
                AssertEqual(unchecked((ushort)(samus.XPosition + x)), grapple.AnchorX, "Launch anchor wraps X");
                AssertEqual(unchecked((ushort)(samus.YPosition + correctedY)), grapple.AnchorY, "Launch anchor wraps Y");
                ushort anchorX = grapple.AnchorX, anchorY = grapple.AnchorY;
                // Late drawing recomputes the physical hand after Samus moves; it must
                // not reuse stale offsets or move the independent collision endpoint.
                samus.XPosition = unchecked((ushort)(samus.XPosition + 7));
                samus.YPosition = unchecked((ushort)(samus.YPosition - 5));
                grapple.OriginXOffset = grapple.OriginYOffset = 1234;
                refresh(bus, samus, grapple);
                AssertEqual(unchecked((ushort)(samus.XPosition + x)), grapple.RopeStartX, "Late physical Start X");
                AssertEqual(unchecked((ushort)(samus.YPosition + correctedY)), grapple.RopeStartY, "Late physical Start Y");
                AssertEqual(anchorX, grapple.AnchorX, "Late draw retains endpoint X");
                AssertEqual(anchorY, grapple.AnchorY, "Late draw retains endpoint Y");
                short flareX = Word((running ? 0x9bc19a : 0x9bc14a) + offset);
                short flareY = Word((running ? 0x9bc1ae : 0x9bc15e) + offset);
                AssertEqual(unchecked((ushort)(samus.XPosition + flareX)), grapple.BeamStartX, "Flare X remains separate");
                AssertEqual(unchecked((ushort)(samus.YPosition + flareY - unchecked((sbyte)bus.GraphicsY))), grapple.BeamStartY, "Flare Y remains separate");
            }
            }
        }
        // Run the real four-substep extension path in empty terrain, not just its setup.
        // Presentation overrides must change Flare without changing trajectory or Start.
        var empty = CreateRoom(64, 64, new ushort[4096], new byte[4096]);
        foreach (bool replaceFlare in new[] { false, true })
        for (byte direction = 0; direction < 10; direction++)
        {
            grapple.FlarePlacement = replaceFlare ? editedFlare : stockFlare;
            samus.Pose = bus.SourcePose = (byte)Enumerable.Range(0, 253).First(pose =>
                rom.ReadByte(SamusMovementRomData.Poses.Definitions + pose * 8 + 3) == direction &&
                rom.ReadByte(SamusMovementRomData.Poses.Definitions + pose * 8 + 1) != 1);
            bus.GraphicsY = 0; bus.Direction = direction;
            samus.TileTransfers.BindArtwork(SamusGraphicsOffsetFixture(0));
            samus.XPosition = samus.YPosition = 512;
            grapple.Phase = GrapplePhase.Inactive;
            samus.LiquidPhysics.BeginFrameSoundRequests();
            SamusGrappleMovement.BeginFiring(bus, samus);
            if (replaceFlare)
                AssertEqual(0x55aa, grapple.FlareXOffset, "Presentation override is actually consumed");
            for (int frame = 1; frame <= 10; frame++)
            {
                SamusGrappleMovement.StepFiring(bus, empty, samus, (ushort)SnesButton.X);
                int dx = Word(0x9bc0db + direction * 2) * 256 * frame;
                int dy = Word(0x9bc0ef + direction * 2) * 256 * frame;
                AssertEqual(dx, grapple.EndpointXOffsetFixed, "Native extension X trajectory");
                AssertEqual(dy, grapple.EndpointYOffsetFixed, "Native extension Y trajectory");
                AssertEqual(unchecked((ushort)(512 + Word(0x9bc122 + direction * 2) + (dx >> 16))), grapple.AnchorX, "Native endpoint X");
                AssertEqual(unchecked((ushort)(512 + Word(0x9bc136 + direction * 2) - rom.ReadByte(SamusMovementRomData.Poses.Definitions + samus.Pose * 8 + 4) + (dy >> 16))), grapple.AnchorY, "Native endpoint Y with authored physical correction");
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
            samus.Pose = left ? SamusPoseIds.DraygonGrabbedMovingLeftPose : SamusPoseIds.DraygonGrabbedMovingRightPose;
            samus.XPosition = samus.YPosition = 512;
            bool outward = (input & (int)(left ? SnesButton.Left : SnesButton.Right)) != 0;
            int direction = outward && (input & (int)SnesButton.Down) != 0 ? (left ? 6 : 3) :
                outward && (input & (int)SnesButton.Up) != 0 ? (left ? 8 : 1) : (left ? 7 : 2);
            grapple.Phase = GrapplePhase.Inactive;
            samus.LiquidPhysics.BeginFrameSoundRequests();
            SamusGrappleMovement.BeginFiring(bus, samus, (ushort)input);
            AssertEqual(direction, grapple.FireDirection, "Held aim selection");
            AssertEqual(Word(0x9bc0db + direction * 2), grapple.ExtensionXVelocity, "Held launch X");
            AssertEqual(Word(0x9bc0ef + direction * 2), grapple.ExtensionYVelocity, "Held launch Y");
            AssertEqual(unchecked((ushort)Word(0x9bc104 + direction * 2)), grapple.Angle.RawValue, "Held launch angle");
            AssertEqual(512 + Word(0x9bc122 + direction * 2), grapple.AnchorX, "Held anchor X");
            AssertEqual(506 + Word(0x9bc136 + direction * 2), grapple.AnchorY, "Held anchor Y correction");
        }
        // Actual locked connection subtracts the raw no-run origin, without graphics-Y
        // correction. Native connection/pose/animation records remain on the real bus.
        int locked = 0;
        foreach (byte movement in new byte[] { 0, 1, 5 })
        for (byte direction = 0; direction < 10; direction++)
        foreach (ushort coordinate in new ushort[] { 0, 512, ushort.MaxValue })
        {
            samus.Pose = bus.SourcePose = movementPoses[movement];
            bus.Direction = direction; bus.GraphicsY = 127;
            samus.TileTransfers.BindArtwork(SamusGraphicsOffsetFixture(127));
            samus.XPosition = coordinate; samus.YPosition = coordinate;
            samus.Kinematics.YSpeed = samus.Kinematics.YSubspeed = 0;
            grapple.FireDirection = direction;
            grapple.AnchorX = unchecked((ushort)(coordinate + 24));
            grapple.AnchorY = unchecked((ushort)(coordinate + 8));
            grapple.RopeLength = 32;
            samus.LiquidPhysics.BeginFrameSoundRequests();
            var result = connect(bus, samus, grapple, coordinate, coordinate, true, false);
            if (!result.LockedInPlace) continue;
            locked++;
            AssertEqual(unchecked((ushort)(grapple.RopeStartX - Word(0x9bc122 + direction * 2))), samus.XPosition, "Locked connection body X");
            AssertEqual(unchecked((ushort)(grapple.RopeStartY - Word(0x9bc136 + direction * 2))), samus.YPosition, "Locked connection body Y ignores graphics correction");
        }
        AssertEqual(54, locked, "All six locked directions in three source movement families at three boundaries");
        Suite(nameof(VerifyGrappleOriginXSelection), () => VerifyGrappleOriginXSelection(rom));
        Suite(nameof(VerifyGrappleOriginDefaultYSelection), () => VerifyGrappleOriginDefaultYSelection(rom));
        Suite(nameof(VerifyGrappleOriginRunningYSelection), () => VerifyGrappleOriginRunningYSelection(rom));
        Console.WriteLine("Grapple firing definitions: 70 native words, loud non-catalog origin rejection, every direction and aiming pose with every signed graphics-Y class and position wrap boundary, every held direction combination, 200 trajectory frames with flare overrides and 54 locked snaps pass; authored mechanics reads forbidden.");
    }

    private static void VerifyGrappleOriginXSelection(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyGrappleOriginField), () => VerifyGrappleOriginField(rom, GrappleFiringDefinitions.OriginXReferenceAddress,
            false, origin => origin.X, "default X alias"));
        Suite(nameof(VerifyGrappleOriginField), () => VerifyGrappleOriginField(rom, GrappleFiringDefinitions.RunningOriginXReferenceAddress,
            true, origin => origin.X, "running X alias"));
    }

    private static void VerifyGrappleOriginDefaultYSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyGrappleOriginField), () => VerifyGrappleOriginField(rom, GrappleFiringDefinitions.OriginYReferenceAddress,
            false, origin => origin.Y, "default Y"));

    private static void VerifyGrappleOriginRunningYSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyGrappleOriginField), () => VerifyGrappleOriginField(rom, GrappleFiringDefinitions.RunningOriginYReferenceAddress,
            true, origin => origin.Y, "running Y"));

    private static void VerifyGrappleOriginField(SuperMetroidAddressSpace rom, int address,
        bool running, Func<(short X, short Y), short> select, string label)
    {
        for (byte direction = 0; direction < 10; direction++)
        {
            int source = address + 2 * direction;
            short expected = unchecked((short)(rom.ReadByte(source) | rom.ReadByte(source + 1) << 8));
            AssertEqual(expected, select(GrappleFiringDefinitions.Origin(direction, running)),
                $"Grapple origin {label} direction {direction}");
        }
        foreach (byte direction in new byte[] { 10, byte.MaxValue })
            AssertThrows<InvalidDataException>(() => GrappleFiringDefinitions.Origin(direction, running),
                $"Grapple origin {label} bounds");
    }
    private static void VerifyGrappleLaunchXSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyGrappleLaunchField), () => VerifyGrappleLaunchField(rom, GrappleFiringDefinitions.XVelocityReferenceAddress,
            direction => unchecked((ushort)GrappleFiringDefinitions.Launch(direction).XVelocity), "X velocity"));

    private static void VerifyGrappleLaunchYSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyGrappleLaunchField), () => VerifyGrappleLaunchField(rom, GrappleFiringDefinitions.YVelocityReferenceAddress,
            direction => unchecked((ushort)GrappleFiringDefinitions.Launch(direction).YVelocity), "Y velocity"));

    private static void VerifyGrappleLaunchAngleAlgorithm(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyGrappleLaunchField), () => VerifyGrappleLaunchField(rom, GrappleFiringDefinitions.AngleReferenceAddress,
            direction => GrappleFiringDefinitions.Launch(direction).Angle, "angle"));

    private static void VerifyGrappleLaunchField(SuperMetroidAddressSpace rom, int source,
        Func<byte, ushort> select, string label)
    {
        for (byte direction = 0; direction < 10; direction++)
        {
            int address = source + 2 * direction;
            ushort original = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
            AssertEqual(original, select(direction), $"Original grapple {label}, direction {direction}");
        }
        AssertThrows<IndexOutOfRangeException>(() => select(10), $"Grapple {label} upper bound");
        AssertThrows<IndexOutOfRangeException>(() => select(byte.MaxValue), $"Grapple {label} invalid maximum");
    }
    private sealed class GrappleFiringReadGuard(ISnesAddressSpace source) : ISnesAddressSpace,
        IImportCartridgeSource, ISnesMutableMemory
    {
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadWorkRamByte(int address) =>
            ((ISnesMutableMemory)source).ReadWorkRamByte(address);

        public byte ReadSaveRamByte(int address) =>
            ((ISnesMutableMemory)source).ReadSaveRamByte(address);

        public byte GraphicsY, Direction;
        public byte SourcePose = SamusPoseIds.FacingRightNormalPose;
        public byte ReadByte(int address)
        {
            int pose = SamusMovementRomData.Poses.Definitions + SourcePose * 8;
            if (address == pose + 1) throw new InvalidOperationException("Compiled pose movement read ROM.");
            if (address == pose + 3) return Direction;
            if (address == pose + 4) throw new InvalidOperationException("Installed graphics offset read ROM.");
            if (address is >= 0x9bc14a and < 0x9bc172 or >= 0x9bc19a and < 0x9bc1c2)
                throw new InvalidOperationException("Installed Grapple flare read ROM.");
            if (address is >= 0x9bc0db and < 0x9bc103 or >= 0x9bc104 and < 0x9bc118 or
                >= 0x9bc122 and < 0x9bc14a or >= 0x9bc172 and < 0x9bc19a)
                throw new InvalidOperationException($"Compiled Grapple mechanics read ROM ${address:X6}.");
            return source.ReadByte(address);
        }
        public void WriteByte(int address, byte value) => throw new InvalidOperationException("Unexpected Grapple definition write.");
    }
}
