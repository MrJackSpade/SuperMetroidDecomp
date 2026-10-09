using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Invokes the production special-angle grapple handler for one anchor and reports whether it handled the angle.</summary>
    /// <param name="bus">Address space used by the grapple handler.</param>
    /// <param name="samus">Player state whose pose and position may be updated.</param>
    /// <param name="grapple">Grapple state containing the anchor and current angle.</param>
    /// <param name="previousX">Player X position before handling.</param>
    /// <param name="previousY">Player Y position before handling.</param>
    /// <param name="result">Camera movement result produced when a special connection is handled.</param>
    /// <returns><see langword="true"/> when a native special-angle record matches.</returns>
    private delegate bool SpecialGrappleHandler(ISnesAddressSpace bus, SamusState samus,
        SamusGrappleState grapple, ushort previousX, ushort previousY, out GrappleMovementResult result);

    /// <summary>Compares compiled grapple connection words with the cartridge and registers dispatch, special-angle, and drop checks.</summary>
    /// <param name="rom">Retail address space containing the native connection tables.</param>
    private static void VerifyGrappleConnectionDefinitions(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var guard = new GrappleConnectionReadGuard(rom);
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
                AssertEqual((Word(address), Word(address + 2)), GrappleConnectionDefinitions.ResolveConnection(address),
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
        guard.ForbidReads = true;
        Suite(nameof(VerifyGrappleConnectionDispatch), () => VerifyGrappleConnectionDispatch(rom));
        Suite(nameof(VerifyGrappleSpecialConnections), () => VerifyGrappleSpecialConnections(rom, guard));
        Suite(nameof(VerifyGrappleCancellationAndDrop), () => VerifyGrappleCancellationAndDrop(rom));
        Console.WriteLine("Grapple connection definitions: 100 native words and 48 policy bytes, 3072 address classifications with loud non-catalog rejection, and real connection/angle/cancel/drop dispatch pass with migrated reads forbidden.");
    }

    /// <summary>Exercises accepted grapple firing across authored poses, directions, and vertical motion to verify native dispatch.</summary>
    /// <param name="rom">Retail address space supplying the source pose and connection-handler data.</param>
    private static void VerifyGrappleConnectionDispatch(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        Func<ISnesAddressSpace, SamusState, SamusGrappleState, ushort, ushort, bool, bool, GrappleMovementResult> connect =
            SamusGrappleMovement.ConnectAcceptedFiring;
        var metadata = new GrappleFiringReadGuard(rom);
        var guard = new GrappleConnectionReadGuard(metadata);
        var samus = new SamusState();
        samus.Grapple.FlarePlacement = SuperMetroid.Core.Assets.ChargeFlarePlacementCatalog.Load(
            new MemoryStream(SuperMetroid.AssetExtraction.GrappleFlarePlacementExtractor.Extract(rom)));
        samus.Grapple.SwingFrames = SuperMetroid.Core.Assets.GrappleSwingFrameCatalog.Load(
            new MemoryStream(SuperMetroid.AssetExtraction.GrappleSwingFrameExtractor.Extract(rom)));
        for (byte movement = 0; movement < 28; movement++)
        for (byte direction = 0; direction < 10; direction++)
        for (int vertical = 0; vertical < 3; vertical++)
        foreach (bool enemy in new[] { false, true })
        {
            int sourcePose = Enumerable.Range(0, 253).FirstOrDefault(pose =>
                rom.ReadByte(SamusMovementRomData.Poses.Definitions + pose * 8 + 1) == movement, -1);
            if (sourcePose < 0) continue; // Native unused movement $0C has no authored pose.
            metadata.SourcePose = samus.Pose = (byte)sourcePose;
            metadata.Direction = direction;
            samus.XPosition = samus.YPosition = 512;
            samus.Kinematics.YSpeed = vertical == 1 ? (ushort)1 : (ushort)0;
            samus.Kinematics.YSubspeed = vertical == 2 ? (ushort)1 : (ushort)0;
            var g = samus.Grapple;
            g.FireDirection = direction; g.AnchorX = 536; g.AnchorY = 520; g.RopeLength = 32;
            samus.LiquidPhysics.BeginFrameSoundRequests();
            var result = connect(guard, samus, g, 512, 512, !enemy, enemy);
            AssertEqual(!enemy, g.ValidateAnchorBlock, "Connection preserves block owner");
            AssertEqual(enemy, g.ValidateAnchorEnemy, "Connection preserves enemy owner");
            AssertEqual(-8, g.RopeLengthDelta, "Connection caller begins native retraction");
            if (movement == 26)
            {
                AssertEqual(GrapplePhase.ConnectedLocked, g.Phase, "Draygon bypass phase");
                AssertEqual(metadata.SourcePose, samus.Pose, "Draygon retains held body pose");
                AssertEqual(512, samus.XPosition, "Draygon retains body position");
                continue;
            }
            int table = vertical != 0 ? 0x9bc3ee : movement == 5 ? 0x9bc416 : 0x9bc3c6;
            ushort function = Word(table + direction * 4), handler = Word(table + direction * 4 + 2);
            AssertEqual(0xa9, rom.ReadByte(0x9b0000 | handler), "Pinned handler begins LDA immediate pose");
            byte pose = (byte)Word((0x9b0000 | handler) + 1);
            AssertEqual(pose, samus.Pose, "Actual native connection pose");
            bool locked = function == 0xc77e;
            AssertEqual(locked ? GrapplePhase.ConnectedLocked : GrapplePhase.ConnectedSwinging, g.Phase, "Actual next connection phase");
            AssertEqual(locked, g.Phase == GrapplePhase.ConnectedLocked, "Connection locks Samus in place");
            AssertEqual(0, samus.Kinematics.YSpeed, "Connection clears Y whole speed");
            AssertEqual(0, samus.Kinematics.YSubspeed, "Connection clears Y fractional speed");
        }
    }

    /// <summary>Checks each exact special-angle record against the production handler, including wrapped anchor and camera results.</summary>
    /// <param name="rom">Retail address space used to read the native special-angle records.</param>
    /// <param name="guard">Address space that rejects reads of migrated grapple connection tables.</param>
    private static void VerifyGrappleSpecialConnections(SuperMetroidAddressSpace rom, ISnesAddressSpace guard)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var special = typeof(SamusGrappleMovement).GetMethod("TryHandleSpecialAngle", BindingFlags.NonPublic | BindingFlags.Static)!
            .CreateDelegate<SpecialGrappleHandler>();
        var native = Enumerable.Range(0, 8).Select(i =>
        {
            int address = 0x9bc43e + i * 10;
            AssertEqual(0, Word(address + 2) >> 8, "Native special pose word has no discarded high byte");
            return new GrappleConnectionDefinitions.SpecialConnection(Word(address), (byte)Word(address + 2),
                unchecked((short)Word(address + 4)), unchecked((short)Word(address + 6)), Word(address + 8));
        }).ToArray();
        AssertEqual(native.Length, GrappleConnectionDefinitions.SpecialAngleCount, "Native special-angle record count");
        for (int record = 0; record < native.Length; record++)
            AssertEqual(native[record], GrappleConnectionDefinitions.SpecialAngle(record), $"Native special-angle record {record}");
        AssertThrows<IndexOutOfRangeException>(() => GrappleConnectionDefinitions.SpecialAngle(native.Length), "Special-angle bound");
        var samus = new SamusState();
        samus.Grapple.FlarePlacement = SuperMetroid.Core.Assets.ChargeFlarePlacementCatalog.Load(
            new MemoryStream(SuperMetroid.AssetExtraction.GrappleFlarePlacementExtractor.Extract(rom)));
        var g = samus.Grapple;
        for (int angle = 0; angle <= ushort.MaxValue; angle++)
        {
            samus.Pose = 1; samus.XPosition = 100; samus.YPosition = 200;
            g.Phase = GrapplePhase.ConnectedSwinging; g.AnchorX = g.AnchorY = 512;
            g.Angle = SnesAngle.FromRaw((ushort)angle);
            int index = Array.FindLastIndex(native, row => row.Angle == angle);
            AssertEqual(index >= 0, special(guard, samus, g, 100, 200, out _), "Exact native special angle, no tolerance");
            if (index < 0)
            {
                AssertEqual(100, samus.XPosition, "Non-match retains body X");
                AssertEqual(200, samus.YPosition, "Non-match retains body Y");
                AssertEqual(GrapplePhase.ConnectedSwinging, g.Phase, "Non-match retains phase");
            }
        }
        // Every anchor word and signed camera-displacement word for each native record.
        foreach (var row in native)
        for (int coordinate = 0; coordinate <= ushort.MaxValue; coordinate++)
        {
            g.AnchorX = (ushort)coordinate; g.AnchorY = unchecked((ushort)~coordinate);
            g.Angle = SnesAngle.FromRaw(row.Angle); g.WallJumpTimer = 77;
            AssertTrue(special(guard, samus, g, 0, ushort.MaxValue, out var result), "Native special record dispatch");
            ushort x = unchecked((ushort)(g.AnchorX + row.X)), y = unchecked((ushort)(g.AnchorY + row.Y));
            AssertEqual(x, samus.XPosition, "Special wrapped body X");
            AssertEqual(y, samus.YPosition, "Special wrapped body Y");
            AssertEqual(row.Pose, samus.Pose, "Special body pose");
            AssertEqual(row.Function == 0xc77e ? GrapplePhase.ConnectedLocked : GrapplePhase.WallGrab, g.Phase, "Special next phase");
            AssertEqual(0, g.WallJumpTimer, "Special resets wall-jump timer");
            AssertEqual(Clamp(x, 0), result.CameraPreviousX!.Value, "Special previous-camera X clamp");
            AssertEqual(Clamp(y, ushort.MaxValue), result.CameraPreviousY!.Value, "Special previous-camera Y clamp");
        }
        static ushort Clamp(ushort current, ushort previous)
        {
            int delta = unchecked((short)(current - previous));
            return delta > 12 ? unchecked((ushort)(current - 12)) : delta < -12 ? unchecked((ushort)(current + 12)) : previous;
        }
    }

    /// <summary>Compares grapple cancellation and dropped-pose selection with native movement, pose, and radius tables.</summary>
    /// <param name="rom">Retail address space containing the source movement metadata and native lookup tables.</param>
    private static void VerifyGrappleCancellationAndDrop(SuperMetroidAddressSpace rom)
    {
        var cancel = typeof(SamusGrappleMovement).GetMethod("HandleFiringPoseChange", BindingFlags.NonPublic | BindingFlags.Static)!
            .CreateDelegate<Func<ISnesAddressSpace, RoomLevelData, SamusState, ushort, GrappleMovementResult?>>();
        var drop = typeof(SamusGrappleMovement).GetMethod("SelectDroppedPose", BindingFlags.NonPublic | BindingFlags.Static)!
            .CreateDelegate<Func<ISnesAddressSpace, SamusState, byte>>();
        // Exercise only combinations owned by actual pose records. Older coverage routed
        // invented movement/aim pairs through pose $FD and rewrote the adjacent executable
        // bytes in a fake bus; that did not represent a cartridge-reachable state.
        var guard = new GrappleConnectionReadGuard(rom);
        var empty = CreateRoom(64, 64, new ushort[4096], new byte[4096]);
        var samus = new SamusState { Pose = SamusPoseIds.FacingRightNormalPose, XPosition = 512, YPosition = 512 };
        using var artworkDirectory = new TestTempDirectory("map-catalog");
        SuperMetroid.AssetExtraction.SamusBodyArtworkFiles.Extract(rom, artworkDirectory.Root,
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(Path.GetFullPath("Super Metroid.smc")))));
        samus.TileTransfers.BindArtwork(SuperMetroid.AssetExtraction.SamusBodyArtworkFiles.Load(artworkDirectory.Root, null));
        samus.Grapple.FlarePlacement = SuperMetroid.Core.Assets.ChargeFlarePlacementCatalog.Load(
            new MemoryStream(SuperMetroid.AssetExtraction.GrappleFlarePlacementExtractor.Extract(rom)));
        for (byte movement = 0; movement < 28; movement++)
        {
            bool banned = rom.ReadByte(0x9bb8b8 + movement) != 0;
            AssertEqual(banned, GrappleConnectionDefinitions.CancelsFiring((SamusMovementType)movement), "Every native cancellation byte");
        }

        for (byte movement = 0; movement < 28; movement++)
        foreach (int sourcePose in Enumerable.Range(0, 253).Where(pose =>
            rom.ReadByte(SamusMovementRomData.Poses.Definitions + pose * 8 + 1) == movement))
        foreach (ushort timer in new ushort[] { 0, 1, 2, 9, 10, ushort.MaxValue })
        {
            bool banned = rom.ReadByte(0x9bb8b8 + movement) != 0;
            byte direction = rom.ReadByte(
                SamusMovementRomData.Poses.Definitions + sourcePose * 8 + 3);
            samus.Pose = (byte)sourcePose;
            samus.Grapple.Phase = GrapplePhase.Firing; samus.Grapple.FireDirection = 2;
            samus.Grapple.PoseChangeAutoFireTimer = timer;
            samus.LiquidPhysics.BeginFrameSoundRequests();
            var result = cancel(guard, empty, samus, 0);
            bool invalidDirection = (direction & 0xf0) != 0;
            bool cancelled = banned || invalidDirection || (direction != 2 && timer <= 1);
            AssertEqual(cancelled ? GrapplePhase.Inactive : GrapplePhase.Firing, samus.Grapple.Phase, "Actual cancellation/refire phase");
            AssertEqual(!cancelled && direction == 2, !result.HasValue, "Unchanged aim retains current dispatch");
            AssertEqual(cancelled ? 0 : direction != 2 ? 10 : Math.Max(0, timer - 1), samus.Grapple.PoseChangeAutoFireTimer, "Cancellation/refire timer ordering");
        }
        for (byte direction = 0; direction < 10; direction++)
        for (int radius = 0; radius <= ushort.MaxValue; radius++)
        {
            int sourcePose = Enumerable.Range(0, 253).First(pose =>
                rom.ReadByte(SamusMovementRomData.Poses.Definitions + pose * 8 + 3) == direction &&
                pose is not (SamusPoseIds.GrappleSwingRightPose or SamusPoseIds.GrappleSwingLeftPose));
            samus.Pose = (byte)sourcePose;
            samus.Kinematics.YRadius = (ushort)radius;
            AssertEqual(rom.ReadByte((radius < 17 ? 0x9bc9c4 : 0x9bc9ba) + direction), drop(guard, samus), "Actual directional drop at every radius");
        }
        foreach (int sourcePose in Enumerable.Range(0, 253).Where(pose =>
            rom.ReadByte(SamusMovementRomData.Poses.Definitions + pose * 8 + 3) >= 10 &&
            pose is not (SamusPoseIds.GrappleSwingRightPose or SamusPoseIds.GrappleSwingLeftPose)))
        foreach (ushort radius in new ushort[] { 0, 16, 17, ushort.MaxValue })
        {
            samus.Pose = (byte)sourcePose;
            samus.Kinematics.YRadius = radius;
            bool left = SamusState.IsFacingLeft(rom, (byte)sourcePose);
            int expected = radius < 17 ? (left ? 0x28 : 0x27) : (left ? 2 : 1);
            AssertEqual(expected, drop(guard, samus), "Non-fireable drop direction retains facing fallback");
        }
        foreach (bool left in new[] { false, true })
        foreach (ushort radius in new ushort[] { 0, 16, 17, ushort.MaxValue })
        {
            samus.Pose = left ? SamusPoseIds.GrappleSwingLeftPose : SamusPoseIds.GrappleSwingRightPose;
            samus.Kinematics.YRadius = radius;
            AssertEqual(left ? 2 : 1, drop(new SlopeHeightNoReadBus(), samus), "Swing-pose drop bypasses all metadata");
        }
    }

    /// <summary>Prevents production code from reading the compiled grapple connection, cancellation, and drop tables from ROM.</summary>
    /// <param name="source">Address space used for reads outside the guarded ranges and for all writes.</param>
    private sealed class GrappleConnectionReadGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Controls whether reads of migrated native table ranges are rejected.</summary>
        public bool ForbidReads = true;

        /// <summary>Routes importer reads through the same guarded address-space path as runtime reads.</summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>The source byte when the address is permitted.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads from migrated grapple tables and forwards other addresses.</summary>
        /// <param name="address">Cartridge address requested by production code.</param>
        /// <returns>The source byte for an address outside the guarded ranges.</returns>
        /// <exception cref="InvalidOperationException">Reads are forbidden and the address belongs to a migrated table.</exception>
        public byte ReadByte(int address)
        {
            if (ForbidReads && (address is >= 0x9bb8b8 and < 0x9bb8d4 or >= 0x9bc3c6 and < 0x9bc48e or >= 0x9bc9ba and < 0x9bc9ce))
                throw new InvalidOperationException($"Compiled Grapple connection read ROM ${address:X6}.");
            return source.ReadByte(address);
        }
        /// <summary>Forwards writes unchanged because the guard only restricts reads.</summary>
        /// <param name="address">Cartridge address to write.</param>
        /// <param name="value">Byte to store at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
