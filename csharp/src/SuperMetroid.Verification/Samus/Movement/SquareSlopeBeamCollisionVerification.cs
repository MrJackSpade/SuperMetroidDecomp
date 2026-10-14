using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
namespace SuperMetroid.Verification.Samus.Movement;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that beam collision samples solid quarter cells of square slopes.</summary>
internal static class SquareSlopeBeamCollisionVerification
{
    /// <summary>
    /// A beam meeting a square slope tests its quarter cells ($94:A66A) rather than passing
    /// through it as air. The 100% movie's falling beams hit such a slope at one height and
    /// pass it at another.
    /// </summary>
    internal static void VerifySquareSlopeBeamCollision()
    {
        // Samus at Y 490 fires a beam spanning Y 482-489: it crosses the slope row's midline.
        AssertEqual(true, BeamExplodesAtSlope(slopeBts: 0x00),
            "half-height slope (lower half solid) stops a beam reaching its lower half");
        // BTS $80 flips it vertically: the upper half is solid, which the beam also reaches.
        AssertEqual(true, BeamExplodesAtSlope(slopeBts: 0x80),
            "flipped half-height slope (upper half solid) stops the same beam");
        // Shape 2 is one solid quarter (lower right). The rightward beam's leading edge
        // enters the column's left half and moves on into the right half, reaching it.
        AssertEqual(true, BeamExplodesAtSlope(slopeBts: 0x02),
            "a lower-right quarter slope stops the beam once its edge reaches the right half");
        Console.WriteLine("  Square-slope beam collision: half-height, flipped and quarter slopes stop the beam.");
    }

    /// <summary>Fires a real beam across a selected square slope and distinguishes its impact from the later wall.</summary>
    private static bool BeamExplodesAtSlope(byte slopeBts)
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite);
        RoomLevelData level = runtime.Room.LoadedLevelData;
        const int SlopeColumn = 40, BeamRow = 30, WallColumn = 44;
        for (int y = 16; y < 36; y++)
        for (int x = 16; x < 64; x++)
        {
            int index = y * level.Size.X + x;
            RoomCollisionType type = y >= 32 || x == WallColumn ? RoomCollisionType.SolidBlock
                : x == SlopeColumn && y == BeamRow ? RoomCollisionType.Slope
                : RoomCollisionType.Air;
            level.SetForegroundEntry(index, RoomLevelWord.Create(0, 0, type).Raw);
            level.SetBehavior(index, type == RoomCollisionType.Slope ? slopeBts : (byte)0);
        }
        SamusState samus = runtime.Player.IntroducedSamus;
        samus.InputLocked = false;
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        samus.Position = samus.Position.WithWhole(new(512, 490));
        samus.PoseMetadata.RefreshCollisionRadii(bus);
        samus.Presentation.InitializeAnimation(bus);
        runtime.Room.LoadedCamera.SetPosition(new Position16<RoomPixels>(400, 350));
        for (int frame = 0; frame < 64; frame++)
            runtime.StepFrame(0);

        runtime.StepFrame((ushort)SnesButton.X);
        for (int frame = 0; frame < 120; frame++)
        {
            runtime.StepFrame(0);
            SamusProjectileSlot shot = runtime.Projectiles.Slots[0];
            if (shot.PackedType.IsFamily(SamusProjectileFamily.BeamExplosion))
            {
                // The explosion is anchored at the beam's leading edge: the slope column
                // or, for a beam that passed it, the wall four blocks further on.
                int column = shot.Position.X.Whole >> 4;
                if (column is not (SlopeColumn or WallColumn))
                    throw new InvalidOperationException($"Beam exploded at column {column}, neither the slope nor the wall.");
                return column == SlopeColumn;
            }
        }
        throw new InvalidOperationException("The beam reached neither the slope nor the wall.");
    }

    /// <summary>Runs the focused half-height and quarter-slope beam checks.</summary>
    [VerificationCommand("--square-slope-beam-collision")]
    internal static int SquareSlopeBeamCollisionCommand()
    {
        Suite(nameof(VerifySquareSlopeBeamCollision), () => VerifySquareSlopeBeamCollision());
        return 0;
    }
}
