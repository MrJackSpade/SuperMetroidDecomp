using SuperMetroid.AssetExtraction.CartridgeTables;
using static SuperMetroid.Verification.Samus.Grapple.SamusGrappleVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Samus.Movement.SamusMovementDefinitionsVerification;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Samus.Grapple;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the cartridge ranges and catalogued callback identities used by Grapple firing, swing, and release.</summary>
internal static class GrappleBeamDefinitionsVerification
{
    /// <summary>
    /// Validates all Grapple direction tables, connection records, swing art, and tile
    /// sources through the last record consumed by the translated retail routines.
    /// </summary>
    internal static void VerifySamusGrappleRomData()
    {
        AssertTrue(GrappleBeamDefinitions.Physics.GravityMagnitude > 0,
            "Grapple gravity magnitude is positive");
        AssertTrue(GrappleBeamDefinitions.Physics.MaximumAngularVelocity >
            GrappleBeamDefinitions.Physics.JumpImpulseMagnitude,
            "Grapple maximum angular velocity admits a jump impulse");

        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine("  Samus Grapple ROM data: retail range audit skipped (private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        foreach (int directionTable in new[]
        {
            GrappleBeamDefinitions.Firing.XVelocities,
            GrappleBeamDefinitions.Firing.YVelocities,
            GrappleBeamDefinitions.Firing.Angles,
            GrappleBeamDefinitions.Firing.DefaultOriginX,
            GrappleBeamDefinitions.Firing.DefaultOriginY,
            SamusCartridgeTables.GrappleDefaultFlareX,
            SamusCartridgeTables.GrappleDefaultFlareY,
            GrappleBeamDefinitions.Firing.RunningOriginX,
            GrappleBeamDefinitions.Firing.RunningOriginY,
            SamusCartridgeTables.GrappleRunningFlareX,
            SamusCartridgeTables.GrappleRunningFlareY,
        })
        {
            TouchRange(bus, directionTable,
                GrappleBeamDefinitions.Firing.DirectionCount * sizeof(ushort),
                $"Grapple direction table ${directionTable:X6}");
        }

        TouchRange(bus, GrappleBeamDefinitions.Firing.MainFlareAnimationDelays,
            6, "Grapple flare animation delays");
        TouchRange(bus, GrappleBeamDefinitions.Firing.RightFlareSpritemapOffsets,
            6, "right Grapple flare spritemap offsets");
        TouchRange(bus, GrappleBeamDefinitions.Firing.LeftFlareSpritemapOffsets,
            6, "left Grapple flare spritemap offsets");
        TouchRange(bus, GrappleBeamDefinitions.Physics.SignedSineTable,
            256 * sizeof(ushort), "Grapple signed sine table");

        TouchRange(bus, SamusCartridgeTables.GrappleSwingFrameByAngle,
            256, "Grapple swing-frame table");
        TouchRange(bus, GrappleBeamDefinitions.Rendering.LeftPoseOffsetsByFrame,
            GrappleBeamDefinitions.Rendering.RightPoseOffsetsByFrame -
                GrappleBeamDefinitions.Rendering.LeftPoseOffsetsByFrame,
            "left Grapple pose offsets");
        TouchRange(bus, GrappleBeamDefinitions.Rendering.RightPoseOffsetsByFrame,
            GrappleBeamDefinitions.Rendering.PointTilePointers -
                GrappleBeamDefinitions.Rendering.RightPoseOffsetsByFrame,
            "right Grapple pose offsets");

        // The point table contains two animation frames. The segment table contains one
        // pointer for each folded two-degree angle pair through the next connection table.
        for (int point = 0; point < 2; point++)
        {
            ushort pointer = CartridgeImportSource.Require(bus).ReadCartridgeWord((GrappleBeamDefinitions.Rendering.PointTilePointers + point * sizeof(ushort)));
            TouchRange(bus, GrapplePointAnimationNative.CharacterDataBank | pointer,
                GrappleBeamDefinitionsRenderingConstants.PointTileByteCount,
                $"Grapple point character frame {point}");
        }

        int segmentPointerCount =
            (GrappleBeamDefinitionsConnectionsConstants.DefaultTable -
                GrappleBeamDefinitions.Rendering.SegmentTilePointers) / sizeof(ushort);
        for (int segment = 0; segment < segmentPointerCount; segment++)
        {
            ushort pointer = CartridgeImportSource.Require(bus).ReadCartridgeWord((GrappleBeamDefinitions.Rendering.SegmentTilePointers + segment * sizeof(ushort)));
            TouchRange(bus, GrapplePointAnimationNative.CharacterDataBank | pointer,
                GrappleBeamDefinitions.Rendering.SegmentTileByteCount,
                $"Grapple segment character set {segment}");
        }

        foreach (int connectionTable in new[]
        {
            GrappleBeamDefinitionsConnectionsConstants.DefaultTable,
            GrappleBeamDefinitionsConnectionsConstants.MovingVerticallyTable,
            GrappleBeamDefinitionsConnectionsConstants.CrouchingTable,
        })
        {
            TouchRange(bus, connectionTable,
                GrappleBeamDefinitionsConnectionsConstants.DirectionCount * 4,
                $"Grapple connection table ${connectionTable:X6}");
            for (int direction = 0;
                direction < GrappleBeamDefinitionsConnectionsConstants.DirectionCount;
                direction++)
            {
                ushort function = CartridgeImportSource.Require(bus).ReadCartridgeWord((connectionTable + direction * 4));
                ushort handler = CartridgeImportSource.Require(bus).ReadCartridgeWord((connectionTable + direction * 4 + 2));
                AssertTrue((GrappleConnectionFunction)function is
                    GrappleConnectionFunction.Swinging or
                    GrappleConnectionFunction.LockedInPlace,
                    $"Grapple connection function ${function:X4} is catalogued");
                AssertTrue(Enum.IsDefined((GrappleConnectionHandler)handler),
                    $"Grapple connection handler ${handler:X4} is catalogued");
            }
        }

        TouchRange(bus, GrappleBeamDefinitionsConnectionsConstants.SpecialAngleTable,
            GrappleBeamDefinitionsConnectionsConstants.SpecialAngleRecordCount *
                GrappleBeamDefinitionsConnectionsConstants.SpecialAngleRecordByteCount,
            "Grapple special-angle records");
        TouchRange(bus, GrappleBeamDefinitionsRelease.StandingPoseTable,
            GrappleBeamDefinitions.Firing.DirectionCount, "Grapple standing release poses");
        TouchRange(bus, GrappleBeamDefinitionsRelease.CrouchingPoseTable,
            GrappleBeamDefinitions.Firing.DirectionCount, "Grapple crouching release poses");

        Console.WriteLine(
            $"  Samus Grapple ROM data: {GrappleBeamDefinitions.Firing.DirectionCount} directions, " +
            $"{segmentPointerCount} segment sets, three connection tables, and special angles are in range.");
    }

    /// <summary>Runs Grapple data-range and swing/release checks against the repository ROM.</summary>
    [VerificationCommand("--grapple-movement")]
    internal static int GrappleMovementCommand()
    {
        Suite(nameof(VerifySamusGrappleRomData), () => VerifySamusGrappleRomData());
        Suite(nameof(VerifySamusGrappleSwingAndRelease), () => VerifySamusGrappleSwingAndRelease());
        return 0;
    }
}
