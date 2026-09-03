using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Validates all Grapple direction tables, connection records, swing art, and tile
    /// sources through the last record consumed by the translated retail routines.
    /// </summary>
    static void VerifySamusGrappleRomData()
    {
        AssertTrue(SamusGrappleRomData.Physics.GravityMagnitude > 0,
            "Grapple gravity magnitude is positive");
        AssertTrue(SamusGrappleRomData.Physics.MaximumAngularVelocity >
            SamusGrappleRomData.Physics.JumpImpulseMagnitude,
            "Grapple maximum angular velocity admits a jump impulse");

        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine("  Samus Grapple ROM data: retail range audit skipped (private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        foreach (int directionTable in new[]
        {
            SamusGrappleRomData.Firing.XVelocities,
            SamusGrappleRomData.Firing.YVelocities,
            SamusGrappleRomData.Firing.Angles,
            SamusGrappleRomData.Firing.DefaultOriginX,
            SamusGrappleRomData.Firing.DefaultOriginY,
            SamusGrappleRomData.Firing.DefaultFlareX,
            SamusGrappleRomData.Firing.DefaultFlareY,
            SamusGrappleRomData.Firing.RunningOriginX,
            SamusGrappleRomData.Firing.RunningOriginY,
            SamusGrappleRomData.Firing.RunningFlareX,
            SamusGrappleRomData.Firing.RunningFlareY,
        })
        {
            TouchRange(bus, directionTable,
                SamusGrappleRomData.Firing.DirectionCount * sizeof(ushort),
                $"Grapple direction table ${directionTable:X6}");
        }

        TouchRange(bus, SamusGrappleRomData.Firing.MainFlareAnimationDelays,
            6, "Grapple flare animation delays");
        TouchRange(bus, SamusGrappleRomData.Firing.RightFlareSpritemapOffsets,
            6, "right Grapple flare spritemap offsets");
        TouchRange(bus, SamusGrappleRomData.Firing.LeftFlareSpritemapOffsets,
            6, "left Grapple flare spritemap offsets");
        TouchRange(bus, SamusGrappleRomData.Physics.SignedSineTable,
            256 * sizeof(ushort), "Grapple signed sine table");

        TouchRange(bus, SamusGrappleRomData.Rendering.SwingFrameByAngle,
            256, "Grapple swing-frame table");
        TouchRange(bus, SamusGrappleRomData.Rendering.LeftPoseOffsetsByFrame,
            SamusGrappleRomData.Rendering.RightPoseOffsetsByFrame -
                SamusGrappleRomData.Rendering.LeftPoseOffsetsByFrame,
            "left Grapple pose offsets");
        TouchRange(bus, SamusGrappleRomData.Rendering.RightPoseOffsetsByFrame,
            SamusGrappleRomData.Rendering.PointTilePointers -
                SamusGrappleRomData.Rendering.RightPoseOffsetsByFrame,
            "right Grapple pose offsets");

        // The point table contains two animation frames. The segment table contains one
        // pointer for each folded two-degree angle pair through the next connection table.
        for (int point = 0; point < 2; point++)
        {
            ushort pointer = ReadGrappleWord(
                bus,
                SamusGrappleRomData.Rendering.PointTilePointers + point * sizeof(ushort));
            TouchRange(bus, SamusGrappleRomData.Banks.CharacterData | pointer,
                SamusGrappleRomData.Rendering.PointTileByteCount,
                $"Grapple point character frame {point}");
        }

        int segmentPointerCount =
            (SamusGrappleRomData.Connections.DefaultTable -
                SamusGrappleRomData.Rendering.SegmentTilePointers) / sizeof(ushort);
        for (int segment = 0; segment < segmentPointerCount; segment++)
        {
            ushort pointer = ReadGrappleWord(
                bus,
                SamusGrappleRomData.Rendering.SegmentTilePointers + segment * sizeof(ushort));
            TouchRange(bus, SamusGrappleRomData.Banks.CharacterData | pointer,
                SamusGrappleRomData.Rendering.SegmentTileByteCount,
                $"Grapple segment character set {segment}");
        }

        var connectionHandlers = new HashSet<ushort>
        {
            SamusGrappleRomData.Connections.SwingClockwiseHandler,
            SamusGrappleRomData.Connections.SwingAnticlockwiseHandler,
            SamusGrappleRomData.Connections.StandingUpRightHandler,
            SamusGrappleRomData.Connections.StandingRightHandler,
            SamusGrappleRomData.Connections.StandingDownHandler,
            SamusGrappleRomData.Connections.StandingUpLeftHandler,
            SamusGrappleRomData.Connections.CrouchingUpRightHandler,
            SamusGrappleRomData.Connections.CrouchingRightHandler,
            SamusGrappleRomData.Connections.CrouchingDownLeftHandler,
            SamusGrappleRomData.Connections.CrouchingUpLeftHandler,
        };
        foreach (int connectionTable in new[]
        {
            SamusGrappleRomData.Connections.DefaultTable,
            SamusGrappleRomData.Connections.MovingVerticallyTable,
            SamusGrappleRomData.Connections.CrouchingTable,
        })
        {
            TouchRange(bus, connectionTable,
                SamusGrappleRomData.Connections.DirectionCount * 4,
                $"Grapple connection table ${connectionTable:X6}");
            for (int direction = 0;
                direction < SamusGrappleRomData.Connections.DirectionCount;
                direction++)
            {
                ushort function = ReadGrappleWord(bus, connectionTable + direction * 4);
                ushort handler = ReadGrappleWord(bus, connectionTable + direction * 4 + 2);
                AssertTrue(function is
                    SamusGrappleRomData.Connections.SwingingHandler or
                    SamusGrappleRomData.Connections.LockedInPlaceHandler,
                    $"Grapple connection function ${function:X4} is catalogued");
                AssertTrue(connectionHandlers.Contains(handler),
                    $"Grapple connection handler ${handler:X4} is catalogued");
            }
        }

        TouchRange(bus, SamusGrappleRomData.Connections.SpecialAngleTable,
            SamusGrappleRomData.Connections.SpecialAngleRecordCount *
                SamusGrappleRomData.Connections.SpecialAngleRecordByteCount,
            "Grapple special-angle records");
        TouchRange(bus, SamusGrappleRomData.Release.StandingPoseTable,
            SamusGrappleRomData.Firing.DirectionCount, "Grapple standing release poses");
        TouchRange(bus, SamusGrappleRomData.Release.CrouchingPoseTable,
            SamusGrappleRomData.Firing.DirectionCount, "Grapple crouching release poses");

        Console.WriteLine(
            $"  Samus Grapple ROM data: {SamusGrappleRomData.Firing.DirectionCount} directions, " +
            $"{segmentPointerCount} segment sets, three connection tables, and special angles are in range.");
    }

    private static ushort ReadGrappleWord(SuperMetroidAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}
