using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Hud;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the reported Reserve Mode X-Ray freeze branch and subsequent palette ownership handoffs.</summary>
internal static class ReserveModeVerification
{
    /// <summary>Compares the original CPU trace with ordinary and reserve X-Ray cleanup, Crystal Flash, and crouch storage.</summary>
    internal static void VerifyReserveMode(string nativeTracePath)
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        int[][] native = File.ReadLines(nativeTracePath).Skip(1)
            .Select(line => line.Split(',')
                .Select(value => Convert.ToInt32(value, 16))
                .ToArray())
            .ToArray();
        AssertEqual(2, native.Length, "Reserve Mode native control count");

        // `$88:8A08` deletes an ordinary X-Ray object only while shared WRAM $0A78
        // remains nonzero. The original-CPU trace's adjacent zero-word case instead
        // retains channel bit four and dispatcher phase five: this is greyout mode.
        AssertEqual(0, native[0][1], "native cleared-freeze case stays clear");
        AssertEqual(4, native[0][4], "native cleared-freeze case retains HDMA channel");
        AssertEqual(5, native[0][5], "native cleared-freeze case retains phase five");
        AssertEqual(8, native[0][7], "native cleared-freeze case retains X-Ray palette");
        AssertEqual(0, native[1][1], "native ordinary X-Ray cleanup clears freeze");
        AssertEqual(0, native[1][4], "native ordinary X-Ray cleanup deletes HDMA channel");
        AssertEqual(0, native[1][5], "native ordinary X-Ray cleanup resets phase");

        SamusState ordinary = CreateXrayReserveFixture(bus, reserveEnergy: 0);
        AdvanceXrayToFinish(bus, ordinary);
        ordinary.Xray.StepBeam(bus, ordinary, controllerInput: 0);
        AssertTrue(!ordinary.Xray.IsActive && !ordinary.Xray.IsReserveMode,
            "ordinary frozen X-Ray phase five performs cartridge cleanup");
        AssertTrue(ordinary.Xray.ConsumeDeactivationSoundRequest(),
            "ordinary X-Ray cleanup publishes its sound request");

        SamusState reserve = CreateXrayReserveFixture(bus, reserveEnergy: 5);
        var recovery = new SamusReserveAutoRecoveryState();
        recovery.Begin(reserve);
        AssertTrue(!reserve.Xray.OwnsSamusControl && reserve.Xray.TimeIsFrozen,
            "command $1B replaces X-Ray Samus handlers but retains shared freeze");
        for (ushort frame = 0; recovery.IsActive; frame++)
            recovery.StepAfterNmi(reserve, frame);
        AssertTrue(reserve.Xray.IsReserveMode && !reserve.ScriptLocks.StationaryScriptControlLocked,
            "reserve exhaustion restores normal control under the live X-Ray object");

        AdvanceXrayToFinish(bus, reserve);
        reserve.Xray.StepBeam(bus, reserve, controllerInput: 0);
        AssertTrue(reserve.Xray.IsActive && reserve.Xray.IsReserveMode,
            "cleared shared freeze strands X-Ray in phase five");
        AssertEqual(XrayBeamPhase.Finish, reserve.Xray.BeamPhase,
            "Reserve Mode keeps X-Ray cleanup dispatcher at phase five");
        AssertTrue(!reserve.Xray.ConsumeDeactivationSoundRequest(),
            "stranded phase five does not invent X-Ray teardown sound");

        // Crystal Flash replaces shared palette handler eight with handler seven without
        // deleting the stranded HDMA object. This is the prerequisite for the documented
        // Reserve Mode *CF Shinespark-Suit route; the interruption/suit timing itself is
        // covered by the original-CPU Crystal Flash matrices from #434.
        reserve.Health = 5;
        reserve.Missiles = reserve.SuperMissiles = reserve.PowerBombs = 10;
        AssertTrue(reserve.CrystalFlash.TryBegin(
                bus,
                reserve,
                (ushort)(SnesButton.Down | SnesButton.L | SnesButton.R | SnesButton.X)),
            "Crystal Flash admits under Reserve Mode");
        AssertTrue(reserve.Xray.IsReserveMode,
            "Crystal Flash retains Reserve Mode HDMA ownership");
        AssertEqual(SamusSpecialPaletteType.None, reserve.Xray.SpecialPaletteKind,
            "Crystal Flash replaces stranded X-Ray palette handler");
        AssertEqual(SamusSpecialPaletteType.CrystalFlash, reserve.CrystalFlash.SpecialPaletteKind,
            "Crystal Flash owns shared palette handler seven");

        // The non-CF route stores a shine by crouching after Reserve Mode is established.
        // Use the real posture transition so palette ownership changes at the native
        // `$91:F7B0` seam rather than by manipulating the Shinespark state directly.
        SamusState knockbackVariant = CreateXrayReserveFixture(bus, reserveEnergy: 5);
        var secondRecovery = new SamusReserveAutoRecoveryState();
        secondRecovery.Begin(knockbackVariant);
        while (secondRecovery.IsActive)
            secondRecovery.StepAfterNmi(knockbackVariant, 0);
        knockbackVariant.Pose = SamusPoseId.FacingRightNormalPose;
        knockbackVariant.PoseMetadata.RefreshCollisionRadii(bus);
        knockbackVariant.Presentation.InitializeAnimation(bus);
        knockbackVariant.HorizontalSpeed.SpeedBoostCounter =
            SamusSpecialSequenceRomData.Shinespark.ActiveSpeedBoostCounter;
        var level = new RoomLevelData(new Extent16<RoomBlocks>(16, 16), new RoomLevelPlanes(new ushort[256], new byte[256], new ushort[256]), new byte[8]);
        AssertTrue(knockbackVariant.PoseTransitions.TryApplyPostureTransition(
                bus,
                level,
                SamusPoseId.CrouchingTransitionRightPose,
                nmiFrameCounter: 0),
            "Reserve Mode non-CF route admits cartridge crouch storage");
        AssertEqual(ShinesparkPhase.Stored, knockbackVariant.Shinespark.Phase,
            "Reserve Mode crouch installs stored-shine owner");
        AssertEqual(SamusSpecialPaletteType.SpeedBoosterShine, knockbackVariant.Shinespark.PaletteType,
            "Reserve Mode crouch installs stored-shine palette handler");
        AssertEqual(SamusSpecialPaletteType.None, knockbackVariant.Xray.SpecialPaletteKind,
            "stored shine replaces stranded X-Ray palette handler");
        AssertTrue(knockbackVariant.Xray.IsReserveMode,
            "stored shine retains the stranded greyout HDMA object");

        Console.WriteLine(
            "Reserve Mode: original phase-five freeze-word branch, greyout ownership, Crystal Flash and crouch-storage handoffs pass.");
    }

    /// <summary>Starts X-Ray with either ordinary health or exhausted health and available auto reserves.</summary>
    private static SamusState CreateXrayReserveFixture(
        ISnesAddressSpace bus,
        ushort reserveEnergy)
    {
        var samus = new SamusState
        {
            Pose = SamusPoseId.FacingRightNormalPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
            Health = reserveEnergy == 0 ? (ushort)99 : (ushort)0,
            MaxHealth = 99,
            ReserveEnergy = reserveEnergy,
            MaxReserveEnergy = 100,
            ReserveTankMode = reserveEnergy == 0 ? (ushort)0 : (ushort)1,
            EquippedItems = (ushort)SamusEquipmentFlags.XrayScope,
        };
        samus.PoseMetadata.RefreshCollisionRadii(bus);
        samus.Presentation.InitializeAnimation(bus);
        AssertTrue(samus.Xray.TryBegin(bus, samus, SamusMovementType.Standing, new XrayBeginConditions { GameState = (ushort)SuperMetroidGameState.MainGameplay }),
            "X-Ray fixture activation");
        return samus;
    }

    /// <summary>Advances the live X-Ray object through setup and beam phases to its cleanup entry.</summary>
    private static void AdvanceXrayToFinish(ISnesAddressSpace bus, SamusState samus)
    {
        while (samus.Xray.SetupStage != XraySetupStage.Complete)
            samus.Xray.StepBeam(bus, samus, controllerInput: 0);
        while (samus.Xray.BeamPhase != XrayBeamPhase.Finish)
            samus.Xray.StepBeam(bus, samus, controllerInput: 0);
    }

    /// <summary>Dispatches the Reserve Mode comparison using the recorded native trace.</summary>
    [VerificationCommand("--reserve-mode")]
    internal static int ReserveModeCommand()
    {
        Suite(nameof(VerifyReserveMode), () => VerifyReserveMode("csharp/test-fixtures/movement-release/reserve-mode-433.csv"));
        return 0;
    }
}
