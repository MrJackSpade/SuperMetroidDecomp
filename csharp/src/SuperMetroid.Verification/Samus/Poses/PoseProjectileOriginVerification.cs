using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Samus.SamusContentIdentityVerification;
using static SuperMetroid.Verification.Hardware.WordSamplesVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Samus.Poses;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that the Samus body graphics offset never moves physical shot origins.</summary>
internal static class PoseProjectileOriginVerification
{
    /// <summary>Compares every pose and direction across signed graphics-offset classes for beams and Grapple.</summary>
    internal static void VerifyPoseProjectileOrigin(CartridgeImportAddressSpace rom)
    {
        short Word(int a) => unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(a));
        var initialize = new Action<SamusState, SamusProjectile>(BeamFiring.InitializePosition);
        var refresh = new Action<SamusState, SamusGrappleState>(SamusGrappleBeamDrawing.RefreshFiringDrawOrigins);
        SamusPoseId guardedPose = default;
        var bus = new CartridgeReadGuard(rom)
            .Forbid(address => address == 0x91b629 + SamusPoseNumbers.Number(guardedPose) * 8 + 3,
                _ => "Compiled pose aim read artwork metadata.")
            .Forbid(address => address == 0x91b629 + SamusPoseNumbers.Number(guardedPose) * 8 + 4,
                _ => "Installed graphics offset read ROM.");
        var samus = PrepareRetailSamusFixture(new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()));
        var slot = new SamusProjectile(0);
        SamusBodySources template = CreateSamusIdentityFixture();
        // The graphics-Y byte only enters as a signed offset, so one artwork fixture per signed
        // byte class covers it.
        var artwork = SignedByteClasses.Select(art => new SamusBodySources(new SamusBodyArtworkParts { TopSetStarts = template.TopSetStarts.ToArray(), BottomSetStarts = template.BottomSetStarts.ToArray(), PoseFirstFrames = template.PoseFirstFrames.ToDictionary(), Placements = template.Placements.ToDictionary(pose => pose.Key, pose => pose.Value with { GraphicsYOffset = unchecked((sbyte)art) }), Frames = template.Frames.ToArray(), Spritemaps = template.Spritemaps, Atmosphere = template.Atmosphere, DeathSteps = template.DeathSteps, ArmCannon = template.ArmCannon })).ToArray();
        for (int pose = 0; pose <= byte.MaxValue; pose++)
        {
            if (!SamusPoseNumbering.NamesPose(pose)) continue;
            byte expected = rom.ReadCartridgeByte(0x91b629 + pose * 8 + 4);
            AssertEqual(expected, SamusPoseProjectileOrigins.ReadYOffset(SamusPoseNumbering.FromNumber(pose)),
                "Every byte-valued correction is compiled, including three adjacent-code indexes");
        }
        // Each artwork class is bound once and every pose and direction checked under it. Only the body's tables are
        // bound: nothing here draws, so composing each class's body sheets would be work no assertion reads.
        for (int artIndex = 0; artIndex < artwork.Length; artIndex++)
        {
            int art = SignedByteClasses[artIndex];
            samus.Presentation.BindArtwork(artwork[artIndex].Gameplay);
            foreach (int pose in Enumerable.Range(0, SamusPoseNumbering.RecordCount).Where(SamusPoseNumbering.NamesPose))
            for (byte direction = 0; direction < 10; direction++)
            {
                guardedPose = samus.Pose = SamusPoseNumbering.FromNumber(pose);
                samus.Position = samus.Position.WithWholeX(unchecked((ushort)(pose * 251 + art)));
                samus.Position = samus.Position.WithWholeY(unchecked((ushort)~samus.Position.X.Whole));
                byte physicalY = rom.ReadCartridgeByte(0x91b629 + pose * 8 + 4);
                bool running = rom.ReadCartridgeByte(0x91b629 + pose * 8 + 1) == 1;
                bool beamRunning = running || pose is 0x75 or 0x76;
                slot.Direction = SamusProjectileDirectionCodec.FromAimIndex(direction);
                initialize(samus, slot);
                AssertEqual(unchecked((ushort)(samus.Position.X.Whole + Word((beamRunning ? SamusSourceDefinitionsConstants.RunningXOrigins : SamusSourceDefinitionsConstants.DefaultXOrigins) + direction * 2))), slot.Position.X.Whole, "Pose art leaves projectile X unchanged");
                AssertEqual(unchecked((ushort)(samus.Position.Y.Whole + Word((beamRunning ? SamusSourceDefinitionsConstants.RunningYOrigins : SamusSourceDefinitionsConstants.DefaultYOrigins) + direction * 2) - physicalY)), slot.Position.Y.Whole, "Pose art leaves projectile collision Y unchanged");

                var g = samus.Grapple;
                g.Phase = GrapplePhase.Inactive;
                samus.LiquidPhysics.BeginFrameSoundRequests();
                SamusGrappleMovement.BeginFiring(samus);
                SamusPoseId poseId = SamusPoseNumbering.FromNumber(pose);
                bool held = poseId is SamusPoseId.DraygonGrabbedMovingLeftPose or SamusPoseId.DraygonGrabbedMovingRightPose;
                int firingDirection = held ? (poseId == SamusPoseId.DraygonGrabbedMovingLeftPose ? 7 : 2) : rom.ReadCartridgeByte(0x91b629 + pose * 8 + 3);
                if ((firingDirection & 0xf0) != 0)
                {
                    AssertEqual(GrapplePhase.CancelPending, g.Phase, "Artwork cannot enable Grapple in a restricted authored pose");
                    continue;
                }
                int physicalCorrection = held ? 6 : physicalY;
                int visualCorrection = held ? 6 : unchecked((sbyte)art);
                short originY = Word((running ? 0x9bc186 : 0x9bc136) + firingDirection * 2);
                short flareY = Word((running ? 0x9bc1ae : 0x9bc15e) + firingDirection * 2);
                AssertEqual(unchecked((ushort)(samus.Position.Y.Whole + originY - physicalCorrection)), g.Anchor.Y, "Grapple physical endpoint ignores art correction");
                AssertEqual(unchecked((ushort)(samus.Position.Y.Whole + flareY - visualCorrection)), g.BeamStart.Y, "Grapple flare retains visual correction");
                ushort endpoint = g.Anchor.Y;
                samus.Position += new Offset16<RoomPixels>(0, 5);
                refresh(samus, g);
                AssertEqual(unchecked((ushort)(samus.Position.Y.Whole + originY - physicalY)), g.RopeStart.Y, "Late Grapple physical start ignores art correction");
                AssertEqual(unchecked((ushort)(samus.Position.Y.Whole + flareY - unchecked((sbyte)art))), g.BeamStart.Y, "Late Grapple flare uses current artwork");
                AssertEqual(endpoint, g.Anchor.Y, "Late draw does not move collision endpoint");
            }
        }
        Console.WriteLine("Pose projectile origins: every pose and direction with each signed graphics-Y class, and authored Grapple launch/cancellation checks, preserve physics while replacing the graphics-Y byte.");
    }

    /// <summary>Runs the pose-based projectile origin verification against the retail cartridge.</summary>
    [VerificationCommand("--pose-projectile-origins")]
    internal static int PoseProjectileOriginsCommand()
    {
        Suite(nameof(VerifyPoseProjectileOrigin), () => VerifyPoseProjectileOrigin(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
