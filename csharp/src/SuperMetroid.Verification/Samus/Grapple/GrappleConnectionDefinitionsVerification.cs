using SuperMetroid.Core.Input;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Samus.Grapple.GrappleFiringDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.LevelData.SharedSlopeHeightsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Samus.Grapple;

/// <summary>Retail oracle and guarded runtime checks for grapple connection, special-angle, cancellation, and drop tables.</summary>
internal static class GrappleConnectionDefinitionsVerification
{
    /// <summary>Invokes the private special-angle dispatcher with its previous body position and receives its camera movement result.</summary>
    internal delegate bool SpecialGrappleHandler(SamusState samus,
        SamusGrappleState grapple, Position16<RoomPixels> previous, out GrappleMovementResult result);

    /// <summary>Checks authored connection words and exact address ownership, then exercises normal, special-angle, cancellation, and drop dispatch.</summary>
    internal static void VerifyGrappleConnectionDefinitions(SuperMetroidAddressSpace rom)
    {
        // Combine two adjacent retail bytes into a bank-$9B reference word.
        ushort Word(int address) => SnesLittleEndian.Word(GrappleConnectionDefinitionsRomByte(rom, address), GrappleConnectionDefinitionsRomByte(rom, address + 1));
        var guard = NewGrappleConnectionReadGuard(rom);
        // All three table bases and every serialized direction byte, including native
        // reads into the next authored table. Other addresses fail explicitly.
        int compiled = 0;
        foreach (int table in new[] { 0x9bc3c6, 0x9bc3ee, 0x9bc416 })
        for (int direction = 0; direction <= byte.MaxValue; direction++)
        for (int alignment = 0; alignment < 4; alignment++)
        {
            int address = table + direction * 4 + alignment;
            bool authored = address < 0x9bc43e && alignment == 0;
            if (authored)
            {
                var connection = GrappleConnectionDefinitions.ResolveConnection(address);
                AssertEqual((Word(address), Word(address + 2)), ((ushort)connection.Function, (ushort)connection.Handler),
                    "Both native connection words");
                compiled++;
            }
            else
            {
                AssertThrows<InvalidDataException>(
                    () => GrappleConnectionDefinitions.ResolveConnection(address),
                    "Unknown or unaligned connection address fails instead of reading adjacent ROM");
            }
        }
        AssertEqual(60, compiled, "Default 30 + vertical 20 + crouching 10 authored reads");
        Suite(nameof(VerifyGrappleConnectionDispatch), () => VerifyGrappleConnectionDispatch(rom));
        Suite(nameof(VerifyGrappleSpecialConnections), () => VerifyGrappleSpecialConnections(rom));
        Suite(nameof(VerifyGrappleCancellationAndDrop), () => VerifyGrappleCancellationAndDrop(rom));
        Console.WriteLine("Grapple connection definitions: 100 native words and 48 policy bytes, 3072 address classifications with loud non-catalog rejection, and real connection/angle/cancel/drop dispatch pass with migrated reads forbidden.");
    }

    /// <summary>Reads one cartridge byte only for comparison with compiled grapple connection entries.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for grapple connection definitions")]
    private static byte GrappleConnectionDefinitionsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks accepted grapple connections across authored movement, aim, vertical-speed, and anchor-owner states with compiled reads forbidden.</summary>
    private static void VerifyGrappleConnectionDispatch(SuperMetroidAddressSpace rom)
    {
        // Read the native function, handler, or pose word used as a dispatch oracle.
        ushort Word(int address) => SnesLittleEndian.Word(GrappleConnectionDispatchRomByte(rom, address), GrappleConnectionDispatchRomByte(rom, address + 1));
        Func<SamusState, SamusGrappleState, Position16<RoomPixels>, GrappleAnchorKind, GrappleMovementResult> connect =
            SamusGrappleMovement.ConnectAcceptedFiring;
        SamusPoseId metadataPose = SamusPoseId.FacingRightNormalPose;
        var metadata = NewGrappleFiringReadGuard(rom, () => metadataPose);
        var guard = NewGrappleConnectionReadGuard(metadata);
        var samus = new SamusState();
        samus.Grapple.FlarePlacement = SuperMetroid.Core.Samus.Artwork.ChargeFlarePlacementCatalog.Load(
            new MemoryStream(SuperMetroid.AssetExtraction.Samus.GrappleFlarePlacementExtractor.Extract(rom)));
        samus.Grapple.SwingFrames = SuperMetroid.Core.Samus.Artwork.GrappleSwingFrameCatalog.Load(
            new MemoryStream(SuperMetroid.AssetExtraction.Samus.GrappleSwingFrameExtractor.Extract(rom)));
        for (byte movement = 0; movement < 28; movement++)
        for (byte direction = 0; direction < 10; direction++)
        for (int vertical = 0; vertical < 3; vertical++)
        foreach (bool enemy in new[] { false, true })
        {
            int sourcePose = Enumerable.Range(0, 253).FirstOrDefault(pose =>
                GrappleConnectionDispatchRomByte(rom, SamusMovementRomData.Poses.Definitions + pose * 8 + 1) == movement, -1);
            if (sourcePose < 0) continue; // Native unused movement $0C has no authored pose.
            metadataPose = samus.Pose = (SamusPoseId)sourcePose;
            samus.Position = samus.Position.WithWhole(new(512, 512));
            samus.Kinematics.YSpeed = new SnesUnsignedSixteenSixteen(vertical == 1 ? (ushort)1 : (ushort)0, vertical == 2 ? (ushort)1 : (ushort)0);
            var g = samus.Grapple;
            g.FireDirection = (SuperMetroid.Core.Samus.Projectiles.SamusProjectileDirection)direction; g.Anchor = g.Anchor.WithX(536); g.Anchor = g.Anchor.WithY(520); g.RopeLength = 32;
            samus.LiquidPhysics.BeginFrameSoundRequests();
            var result = connect(samus, g, new(512, 512), enemy ? GrappleAnchorKind.Enemy : GrappleAnchorKind.Block);
            AssertEqual(!enemy, g.ValidateAnchorBlock, "Connection preserves block owner");
            AssertEqual(enemy, g.ValidateAnchorEnemy, "Connection preserves enemy owner");
            AssertEqual(-8, g.RopeLengthDelta, "Connection caller begins native retraction");
            if (movement == 26)
            {
                AssertEqual(GrapplePhase.ConnectedLocked, g.Phase, "Draygon bypass phase");
                AssertEqual(metadataPose, samus.Pose, "Draygon retains held body pose");
                AssertEqual(512, samus.Position.X.Whole, "Draygon retains body position");
                continue;
            }
            int table = vertical != 0 ? 0x9bc3ee : movement == 5 ? 0x9bc416 : 0x9bc3c6;
            ushort function = Word(table + direction * 4), handler = Word(table + direction * 4 + 2);
            AssertEqual(0xa9, GrappleConnectionDispatchRomByte(rom, 0x9b0000 | handler), "Pinned handler begins LDA immediate pose");
            SamusPoseId pose = (SamusPoseId)Word((0x9b0000 | handler) + 1);
            AssertEqual(pose, samus.Pose, "Actual native connection pose");
            bool locked = function == 0xc77e;
            AssertEqual(locked ? GrapplePhase.ConnectedLocked : GrapplePhase.ConnectedSwinging, g.Phase, "Actual next connection phase");
            AssertEqual(locked, g.Phase == GrapplePhase.ConnectedLocked, "Connection locks Samus in place");
            AssertEqual(0, samus.Kinematics.YSpeed.Whole, "Connection clears Y whole speed");
            AssertEqual(0, samus.Kinematics.YSpeed.Fraction, "Connection clears Y fractional speed");
        }
    }

    /// <summary>Reads a retail dispatch or pose byte for comparison with production connection results.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for grapple connection dispatch")]
    private static byte GrappleConnectionDispatchRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks eight native special-angle rows, exact 16-bit angle matches, wrapped body offsets, phase changes, and camera clamps.</summary>
    private static void VerifyGrappleSpecialConnections(SuperMetroidAddressSpace rom)
    {
        // Read one native special-angle record word from two cartridge bytes.
        ushort Word(int address) => SnesLittleEndian.Word(GrappleSpecialConnectionsRomByte(rom, address), GrappleSpecialConnectionsRomByte(rom, address + 1));
        var special = new SpecialGrappleHandler(SamusGrappleMovement.TryHandleSpecialAngle);
        var native = Enumerable.Range(0, 8).Select(i =>
        {
            int address = 0x9bc43e + i * 10;
            AssertEqual(0, Word(address + 2) >> 8, "Native special pose word has no discarded high byte");
            return new GrappleConnectionDefinitions.SpecialConnection(SnesAngle.FromRaw(checked(Word(address))), (SamusPoseId)Word(address + 2),
                new Offset16<RoomPixels>(unchecked((short)Word(address + 4)), unchecked((short)Word(address + 6))),
                (GrappleConnectionFunction)Word(address + 8));
        }).ToArray();
        AssertEqual(native.Length, GrappleConnectionDefinitions.SpecialAngleCount, "Native special-angle record count");
        for (int record = 0; record < native.Length; record++)
            AssertEqual(native[record], GrappleConnectionDefinitions.SpecialAngle(record), $"Native special-angle record {record}");
        AssertThrows<ArgumentOutOfRangeException>(() => GrappleConnectionDefinitions.SpecialAngle(native.Length), "Special-angle bound");
        var samus = new SamusState();
        samus.Grapple.FlarePlacement = SuperMetroid.Core.Samus.Artwork.ChargeFlarePlacementCatalog.Load(
            new MemoryStream(SuperMetroid.AssetExtraction.Samus.GrappleFlarePlacementExtractor.Extract(rom)));
        var g = samus.Grapple;
        for (int angle = 0; angle <= ushort.MaxValue; angle++)
        {
            samus.Pose = SamusPoseId.FacingRightNormalPose; samus.Position = samus.Position.WithWhole(new(100, 200));
            g.Phase = GrapplePhase.ConnectedSwinging; g.Anchor = g.Anchor.WithY(512).WithX(512);
            g.Angle = SnesAngle.FromRaw((ushort)angle);
            int index = Array.FindLastIndex(native, row => row.Angle.RawValue == angle);
            AssertEqual(index >= 0, special(samus, g, new(100, 200), out _), "Exact native special angle, no tolerance");
            if (index < 0)
            {
                AssertEqual(100, samus.Position.X.Whole, "Non-match retains body X");
                AssertEqual(200, samus.Position.Y.Whole, "Non-match retains body Y");
                AssertEqual(GrapplePhase.ConnectedSwinging, g.Phase, "Non-match retains phase");
            }
        }
        // Every anchor word and signed camera-displacement word for each native record.
        foreach (var row in native)
        for (int coordinate = 0; coordinate <= ushort.MaxValue; coordinate++)
        {
            g.Anchor = g.Anchor.WithX((ushort)coordinate); g.Anchor = g.Anchor.WithY(unchecked((ushort)~coordinate));
            g.Angle = row.Angle; g.WallJumpTimer = NativeCountdown.Start(77);
            AssertTrue(special(samus, g, new(0, ushort.MaxValue), out var result), "Native special record dispatch");
            ushort x = unchecked((ushort)(g.Anchor.X + row.SamusOffset.X)), y = unchecked((ushort)(g.Anchor.Y + row.SamusOffset.Y));
            AssertEqual(x, samus.Position.X.Whole, "Special wrapped body X");
            AssertEqual(y, samus.Position.Y.Whole, "Special wrapped body Y");
            AssertEqual(row.Pose, samus.Pose, "Special body pose");
            AssertEqual(row.Function == GrappleConnectionFunction.LockedInPlace ? GrapplePhase.ConnectedLocked : GrapplePhase.WallGrab, g.Phase, "Special next phase");
            AssertEqual(0, g.WallJumpTimer.RawValue, "Special resets wall-jump timer");
            AssertEqual(Clamp(x, 0), (result.CameraPrevious ?? throw new InvalidOperationException("Verification requires result.CameraPrevious.")).X, "Special previous-camera X clamp");
            AssertEqual(Clamp(y, ushort.MaxValue), result.CameraPrevious.Value.Y, "Special previous-camera Y clamp");
        }
        // Limit wrapped camera movement to twelve pixels toward the new body coordinate.
        static ushort Clamp(ushort current, ushort previous)
        {
            int delta = unchecked((short)(current - previous));
            return delta > 12 ? unchecked((ushort)(current - 12)) : delta < -12 ? unchecked((ushort)(current + 12)) : previous;
        }
    }

    /// <summary>Reads retail special-angle bytes only to construct the comparison records.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for grapple special connections")]
    private static byte GrappleSpecialConnectionsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks firing cancellation and directional drop pose choices against native policy bytes for reachable poses and radius boundaries.</summary>
    private static void VerifyGrappleCancellationAndDrop(SuperMetroidAddressSpace rom)
    {
        var cancel = new Func<RoomLevelData, SamusState, SnesButton, GrappleMovementResult?>(SamusGrappleMovement.HandleFiringPoseChange);
        var drop = (ISnesAddressSpace memory, SamusState player) => (byte)SamusGrappleMovement.SelectDroppedPose(player);
        // Exercise only combinations owned by actual pose records. Older coverage routed
        // invented movement/aim pairs through pose $FD and rewrote the adjacent executable
        // bytes in a fake bus; that did not represent a cartridge-reachable state.
        var guard = NewGrappleConnectionReadGuard(rom);
        var empty = CreateRoom(new(64, 64), new ushort[4096], new byte[4096]);
        var samus = new SamusState { Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(512, 512)) };
        using var artworkDirectory = new TestScratchDirectory("map-catalog");
        SuperMetroid.AssetExtraction.Samus.SamusBodyArtworkFiles.Extract(rom, artworkDirectory.Root,
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(Path.GetFullPath("Super Metroid.smc")))));
        samus.Presentation.TileTransfers.BindArtwork(SuperMetroid.AssetExtraction.Samus.SamusBodyArtworkFiles.Load(artworkDirectory.Root, null));
        samus.Grapple.FlarePlacement = SuperMetroid.Core.Samus.Artwork.ChargeFlarePlacementCatalog.Load(
            new MemoryStream(SuperMetroid.AssetExtraction.Samus.GrappleFlarePlacementExtractor.Extract(rom)));
        for (byte movement = 0; movement < 28; movement++)
        {
            bool banned = GrappleCancellationAndDropRomByte(rom, 0x9bb8b8 + movement) != 0;
            AssertEqual(banned, GrappleConnectionDefinitions.CancelsFiring((SamusMovementType)movement), "Every native cancellation byte");
        }

        for (byte movement = 0; movement < 28; movement++)
        foreach (int sourcePose in Enumerable.Range(0, 253).Where(pose =>
            GrappleCancellationAndDropRomByte(rom, SamusMovementRomData.Poses.Definitions + pose * 8 + 1) == movement))
        foreach (ushort timer in new ushort[] { 0, 1, 2, 9, 10, ushort.MaxValue })
        {
            bool banned = GrappleCancellationAndDropRomByte(rom, 0x9bb8b8 + movement) != 0;
            byte direction = GrappleCancellationAndDropRomByte(rom,
                SamusMovementRomData.Poses.Definitions + sourcePose * 8 + 3);
            samus.Pose = (SamusPoseId)sourcePose;
            samus.Grapple.Phase = GrapplePhase.Firing; samus.Grapple.FireDirection = SuperMetroid.Core.Samus.Projectiles.SamusProjectileDirection.Right;
            samus.Grapple.PoseChangeAutoFireTimer = NativeCountdown.FromRaw(timer);
            samus.LiquidPhysics.BeginFrameSoundRequests();
            var result = cancel(empty, samus, 0);
            bool invalidDirection = (direction & 0xf0) != 0;
            bool cancelled = banned || invalidDirection || (direction != 2 && timer <= 1);
            AssertEqual(cancelled ? GrapplePhase.Inactive : GrapplePhase.Firing, samus.Grapple.Phase, "Actual cancellation/refire phase");
            AssertEqual(!cancelled && direction == 2, !result.HasValue, "Unchanged aim retains current dispatch");
            AssertEqual(cancelled ? 0 : direction != 2 ? 10 : Math.Max(0, timer - 1), samus.Grapple.PoseChangeAutoFireTimer.RawValue, "Cancellation/refire timer ordering");
        }
        for (byte direction = 0; direction < 10; direction++)
        for (int radius = 0; radius <= ushort.MaxValue; radius++)
        {
            int sourcePose = Enumerable.Range(0, 253).First(pose =>
                GrappleCancellationAndDropRomByte(rom, SamusMovementRomData.Poses.Definitions + pose * 8 + 3) == direction &&
                pose is not ((int)SamusPoseId.GrappleSwingRightPose or (int)SamusPoseId.GrappleSwingLeftPose));
            samus.Pose = (SamusPoseId)sourcePose;
            samus.Kinematics.Radius = samus.Kinematics.Radius.WithY((ushort)radius);
            AssertEqual(GrappleCancellationAndDropRomByte(rom, (radius < 17 ? 0x9bc9c4 : 0x9bc9ba) + direction), drop(guard, samus), "Actual directional drop at every radius");
        }
        foreach (int sourcePose in Enumerable.Range(0, 253).Where(pose =>
            GrappleCancellationAndDropRomByte(rom, SamusMovementRomData.Poses.Definitions + pose * 8 + 3) >= 10 &&
            pose is not ((int)SamusPoseId.GrappleSwingRightPose or (int)SamusPoseId.GrappleSwingLeftPose)))
        foreach (ushort radius in new ushort[] { 0, 16, 17, ushort.MaxValue })
        {
            samus.Pose = (SamusPoseId)sourcePose;
            samus.Kinematics.Radius = samus.Kinematics.Radius.WithY(radius);
            bool left = SamusPoseMetadata.IsFacingLeft((SamusPoseId)sourcePose);
            int expected = radius < 17 ? (left ? 0x28 : 0x27) : (left ? 2 : 1);
            AssertEqual(expected, drop(guard, samus), "Non-fireable drop direction retains facing fallback");
        }
        foreach (bool left in new[] { false, true })
        foreach (ushort radius in new ushort[] { 0, 16, 17, ushort.MaxValue })
        {
            samus.Pose = left ? SamusPoseId.GrappleSwingLeftPose : SamusPoseId.GrappleSwingRightPose;
            samus.Kinematics.Radius = samus.Kinematics.Radius.WithY(radius);
            AssertEqual(left ? 2 : 1, drop(NewSlopeHeightNoReadBus(), samus), "Swing-pose drop bypasses all metadata");
        }
    }

    /// <summary>Reads retail policy and pose metadata bytes used as the cancellation/drop oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for grapple cancellation and drop")]
    private static byte GrappleCancellationAndDropRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Forbids runtime reads of the compiled firing-cancel, connection, special-angle, and drop-pose ranges in bank $9B.</summary>
    private static CartridgeReadGuard NewGrappleConnectionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0x9bb8b8 and < 0x9bb8d4 or >= 0x9bc3c6 and < 0x9bc48e or >= 0x9bc9ba and < 0x9bc9ce,
                address => $"Compiled Grapple connection read ROM ${address:X6}.");
}
