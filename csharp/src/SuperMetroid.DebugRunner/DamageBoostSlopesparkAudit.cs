using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// Compares damage-boost slopespark handoff with original-CPU traces on the
/// authored Parlor/Alcatraz slope.
/// </summary>
/// <remarks>
/// The supplied retail movie proves the dry success path. Its exact pre-input
/// checkpoint is then replayed by the original 65816 routines in dry and fully
/// submerged suitless-water variants. The audit deliberately begins after damage
/// publication because ordinary contact is already covered by the full damage-
/// boost matrix; this fixture owns the slope landing and Shinespark Suit handoff.
/// </remarks>
internal static class DamageBoostSlopesparkAudit
{
    private const string RetailRomSha256 =
        "12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72";
    private const string TraceSha256 =
        "70CFEDB97C179652EFE0C0F58EA0CD06591F3D86B1398322870CCE167700FAA6";

    private static readonly ushort[] AirSuccessInputs =
    [
        (ushort)(SnesButton.Left | SnesButton.A),
        (ushort)(SnesButton.Left | SnesButton.A),
        0,
        (ushort)SnesButton.A,
        (ushort)SnesButton.Left,
        0,
    ];

    private static readonly ushort[] AirDelayedInputs =
    [
        (ushort)(SnesButton.Left | SnesButton.A),
        (ushort)(SnesButton.Left | SnesButton.A),
        0,
        0,
        (ushort)SnesButton.A,
        (ushort)SnesButton.Left,
        0,
    ];

    private static readonly ushort[] WaterSuccessInputs =
    [
        (ushort)(SnesButton.Left | SnesButton.A),
        (ushort)(SnesButton.Left | SnesButton.A),
        0,
        (ushort)SnesButton.A,
        (ushort)SnesButton.Left,
        (ushort)SnesButton.Left,
        (ushort)SnesButton.Left,
        (ushort)SnesButton.Left,
        0,
    ];

    private static readonly ushort[] WaterDelayedInputs =
    [
        (ushort)(SnesButton.Left | SnesButton.A),
        (ushort)(SnesButton.Left | SnesButton.A),
        0,
        0,
        (ushort)SnesButton.A,
        (ushort)SnesButton.Left,
        (ushort)SnesButton.Left,
        (ushort)SnesButton.Left,
        (ushort)SnesButton.Left,
        0,
    ];

    public static int Run(string rom, string trace)
    {
        SuperMetroidAddressSpace verificationBus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        if (Convert.ToHexString(SHA256.HashData(verificationBus.Rom)) != RetailRomSha256)
            throw new InvalidDataException("Use the pinned Japan/USA retail ROM for the slopespark audit.");

        string text = File.ReadAllText(trace).Replace("\r\n", "\n", StringComparison.Ordinal);
        string traceHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
        if (traceHash != TraceSha256)
        {
            throw new InvalidDataException(
                $"Slopespark trace hash {traceHash} does not match {TraceSha256}.");
        }

        string[][] rows = text.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Skip(1)
            .Select(line => line.Split(','))
            .ToArray();
        if (rows.Length != 36 || rows.Any(row => row.Length != 17))
            throw new InvalidDataException("Expected 36 complete seventeen-column slopespark rows.");

        int mismatches = 0;
        int samples = 0;
        foreach (IGrouping<string, string[]> group in rows.GroupBy(row => $"{row[0]},{row[1]}"))
        {
            bool underwater = group.First()[0] == "1";
            bool delayedLandingJump = group.First()[1] == "1";
            ushort[] authoredInputs = SelectAuthoredInputs(underwater, delayedLandingJump);
            (SuperMetroidRuntime runtime, SamusState samus, ISnesAddressSpace bus) =
                CreateRuntime(rom, underwater);

            int expectedFrame = -1;
            foreach (string[] row in group)
            {
                int frame = int.Parse(row[2], CultureInfo.InvariantCulture);
                if (frame != expectedFrame++)
                    throw new InvalidDataException($"Reordered slopespark trace in case {group.Key}.");

                ushort tracedInput = ushort.Parse(
                    row[3], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                ushort expectedInput = frame < 0
                    ? (ushort)SnesButton.Right
                    : authoredInputs[frame];
                if (tracedInput != expectedInput)
                {
                    throw new InvalidDataException(
                        $"Changed slopespark input in case {group.Key}, frame {frame}.");
                }

                if (frame >= 0)
                    runtime.StepFrame(expectedInput);

                string actual = BuildComparableFrame(bus, samus);
                string expected = string.Join(',', row[4..17]);
                if (actual != expected)
                {
                    mismatches++;
                    Console.WriteLine(
                        $"SLOPESPARK {group.Key} frame {frame}: {actual} != {expected}");
                }
                samples++;
            }

            AssertFinalSemantics(samus, underwater, delayedLandingJump);
        }

        Console.WriteLine(
            $"Damage-boost slopespark: {samples} original-CPU states, {mismatches} mismatches; " +
            $"trace SHA-256 {traceHash}.");
        return mismatches == 0 ? 0 : 1;
    }

    private static ushort[] SelectAuthoredInputs(bool underwater, bool delayedLandingJump) =>
        (underwater, delayedLandingJump) switch
        {
            (false, false) => AirSuccessInputs,
            (false, true) => AirDelayedInputs,
            (true, false) => WaterSuccessInputs,
            (true, true) => WaterDelayedInputs,
        };

    private static (SuperMetroidRuntime Runtime, SamusState Samus, ISnesAddressSpace Bus) CreateRuntime(
        string rom,
        bool underwater)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebesAwake);
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.ParlorAndAlcatraz);

        // The movie contact is caused by an enemy projectile. Once the native
        // damage state has been published, actors must not perturb this focused
        // movement comparison or reapply contact on a later fixture frame.
        foreach (RoomEnemySlot enemy in runtime.Enemies.Slots)
            enemy.Clear();
        foreach (RoomEnemyProjectileSlot projectile in runtime.Enemies.EnemyProjectiles)
            projectile.Clear();

        SamusState samus = runtime.Samus ?? throw new InvalidDataException("Missing slopespark Samus.");
        samus.EquippedItems = samus.CollectedItems = (ushort)SamusEquipmentFlags.SpeedBooster;
        samus.Health = samus.MaxHealth = 725;
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.XPosition = 0x028d;
        samus.Kinematics.XSubposition = 0xbbff;
        samus.YPosition = 0x00b9;
        samus.Kinematics.YSubposition = 0xffff;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.PoseHistory.PreviousPose = samus.Pose;
        samus.PoseHistory.PreviousDirectionAndMovement = 0x0008;
        runtime.Controller1.Latch((ushort)SnesButton.Right);

        if (underwater)
            samus.LiquidPhysics.ConfigureWater(0);

        if (!samus.Shinespark.TryStoreFromSpeedBooster(
                SamusSpecialSequenceRomData.Shinespark.ActiveSpeedBoostCounter))
        {
            throw new InvalidDataException("Slopespark fixture could not seed cartridge stored shine.");
        }

        // The retained movie checkpoint is immediately before controller sample
        // 220. Its stored-shine palette owner has already advanced to 53 ticks.
        while (samus.Shinespark.ShineTimer > 53)
            samus.Shinespark.UpdatePalette(bus, runtime.Cgram, samus.EquippedItems);

        SamusKnockbackMovement.Start(
            bus,
            samus,
            (ushort)SnesButton.Right,
            knockbackXDirection: 1,
            knockbackTimer: underwater ? (ushort)7 : (ushort)4,
            runtime.LevelData);
        return (runtime, samus, bus);
    }

    private static string BuildComparableFrame(ISnesAddressSpace bus, SamusState samus)
    {
        ushort nativeMovementHandler = samus.KnockbackActive
            ? (ushort)0xdf38
            : samus.Shinespark.Phase == ShinesparkPhase.Windup
                ? (ushort)0xd068
                : (ushort)0xa337;
        return $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8}," +
            $"{samus.Pose:X2},{(byte)samus.ReadMovementType(bus):X2}," +
            $"{samus.HorizontalSpeed.BaseFixed:X8}," +
            $"{samus.HorizontalSpeed.ExtraRunSpeed:X4}{samus.HorizontalSpeed.ExtraRunSubspeed:X4}," +
            $"{samus.Kinematics.VerticalSpeedFixed:X8}," +
            $"{samus.KnockbackTimer:X4},{samus.KnockbackDirection:X4}," +
            $"{samus.SharedShineTimer:X4},{samus.Shinespark.PaletteType:X4}," +
            $"{samus.HorizontalSpeed.SpeedBoostCounter:X4},{nativeMovementHandler:X4}";
    }

    private static void AssertFinalSemantics(
        SamusState samus,
        bool underwater,
        bool delayedLandingJump)
    {
        bool retainedSuit = samus.Pose == SamusPoseIds.ShinesparkWindupLeftPose &&
            samus.Shinespark.Phase == ShinesparkPhase.Inactive &&
            samus.Shinespark.PaletteType == 6 &&
            samus.SharedShineTimer == 58 &&
            samus.HorizontalSpeed.ExtraRunSpeed == 8;
        if (!delayedLandingJump && !retainedSuit)
        {
            throw new InvalidDataException(
                $"The {(underwater ? "underwater" : "dry")} landing window did not retain " +
                "the cartridge's usable Shinespark Suit state.");
        }

        if (delayedLandingJump && underwater &&
            samus.Shinespark.Phase != ShinesparkPhase.Windup)
        {
            throw new InvalidDataException(
                "The adjacent underwater failure no longer enters ordinary Shinespark windup.");
        }

        if (delayedLandingJump && !underwater &&
            samus.Shinespark.Phase != ShinesparkPhase.Stored)
        {
            throw new InvalidDataException(
                "The adjacent dry failure no longer remains an ordinary stored shine.");
        }
    }
}
