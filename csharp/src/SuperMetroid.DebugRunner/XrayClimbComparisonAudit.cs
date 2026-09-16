using System.Globalization;
using System.IO.Compression;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>Compares X-Ray teardown's five-pixel stand-up primitive with the original CPU.</summary>
internal static class XrayClimbComparisonAudit
{
    private const string AcceptedCaptureSha256 =
        "215B1880DD5E235F967214B6393C70219089D3594B31788221EB407FA5295DCE";

    public static int Run(string rom, string capture)
    {
        byte[] captureBytes = ReadCapture(capture);
        if (Convert.ToHexString(SHA256.HashData(captureBytes)) != AcceptedCaptureSha256)
            throw new InvalidDataException("Use the accepted native X-Ray climb v3 capture for issue 438.");

        string[] lines = new StreamReader(new MemoryStream(captureBytes))
            .ReadToEnd()
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length != 385 ||
            lines[0] != "kind,left,medium,depth,x,y,pose,xradius,yradius,xdir,movement")
            throw new InvalidDataException("Incomplete X-Ray climb matrix.");

        string[][] rows = lines.Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Any(row => row.Length != 11))
            throw new InvalidDataException("Malformed X-Ray climb row.");

        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        int mismatches = 0;
        for (int rowIndex = 0; rowIndex < rows.Length; rowIndex++)
        {
            string[] row = rows[rowIndex];
            int kind = int.Parse(row[0], CultureInfo.InvariantCulture);
            bool left = row[1] == "1";
            int medium = int.Parse(row[2], CultureInfo.InvariantCulture);
            int depth = int.Parse(row[3], CultureInfo.InvariantCulture);
            if (((kind * 2 + (left ? 1 : 0)) * 3 + medium) * 16 + depth - 1 != rowIndex ||
                kind is < 0 or > 3 || medium is < 0 or > 2 || depth is < 1 or > 16)
                throw new InvalidDataException("Reordered or invalid X-Ray climb cases.");

            SamusState samus = CreateSamus(bus, kind, left, medium, depth);
            CompleteXray(bus, samus);
            string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8}," +
                $"{samus.Pose:X2},{samus.Kinematics.XRadius:X4},{samus.Kinematics.YRadius:X4}," +
                $"{samus.ReadPoseXDirection(bus):X2},{(byte)samus.ReadMovementType(bus):X2}";
            string expected = string.Join(',', row[4..]);
            if (actual == expected)
                continue;

            mismatches++;
            if (mismatches <= 20)
                Console.WriteLine($"XRAY CLIMB {kind},{(left ? 1 : 0)},{medium},{depth}: {actual} != {expected}");
        }

        if (rows.Count(row => row[0] == "0" && row[5] == "01FB0000") != 96)
            throw new InvalidDataException("The native climb witness must move every crouched turn upward five pixels.");
        if (rows.Count(row => row[0] != "0" && row[5] == "02000000") != 288)
            throw new InvalidDataException("Stable-crouch and standing controls must retain their original height.");

        Console.WriteLine($"X-Ray climb: {rows.Length} cases, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }

    private static SamusState CreateSamus(
        ISnesAddressSpace bus,
        int kind,
        bool finalFacingLeft,
        int medium,
        int depth)
    {
        bool beginsCrouched = kind is 0 or 1;
        bool turns = kind is 0 or 2;
        bool initialFacingLeft = turns ? !finalFacingLeft : finalFacingLeft;
        var samus = new SamusState
        {
            XPosition = unchecked((ushort)(0x0100 + (finalFacingLeft ? -depth : depth))),
            YPosition = 0x0200,
            Health = 99,
            MaxHealth = 99,
            EquippedItems = (ushort)SamusEquipmentFlags.XrayScope,
            Pose = beginsCrouched
                ? initialFacingLeft ? SamusPoseIds.CrouchingLeftPose : SamusPoseIds.CrouchingRightPose
                : initialFacingLeft ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.PoseHistory.PreviousPose = samus.Pose;
        samus.PoseHistory.PreviousDirectionAndMovement =
            (ushort)(((byte)samus.ReadMovementType(bus) << 8) | samus.ReadPoseXDirection(bus));
        if (medium == 1)
            samus.LiquidPhysics.ConfigureWater(8);
        else if (medium == 2)
            samus.LiquidPhysics.ConfigureLavaAcid(8);

        SamusMovementType movement = samus.ReadMovementType(bus);
        if (!samus.Xray.TryBegin(bus, samus, movement))
            throw new InvalidDataException("Constructed X-Ray climb case did not activate.");
        if (turns)
        {
            ushort input = finalFacingLeft ? (ushort)SnesButton.Left : (ushort)SnesButton.Right;
            XrayPoseInputResult turn = samus.Xray.HandlePoseInput(bus, samus, input);
            if (!turn.StartedTurn)
                throw new InvalidDataException("Constructed X-Ray climb turn did not start.");
        }
        return samus;
    }

    private static void CompleteXray(ISnesAddressSpace bus, SamusState samus)
    {
        for (int frame = 0; samus.Xray.IsActive && frame < 16; frame++)
            samus.Xray.StepBeam(bus, samus, controllerInput: 0);
        if (samus.Xray.IsActive)
            throw new InvalidDataException("X-Ray teardown did not complete within its bounded phase count.");
    }

    private static byte[] ReadCapture(string capture)
    {
        if (!capture.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            return File.ReadAllBytes(capture);

        using ZipArchive archive = ZipFile.OpenRead(capture);
        ZipArchiveEntry[] entries = archive.Entries
            .Where(entry => entry.FullName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (entries.Length != 1)
            throw new InvalidDataException("X-Ray climb archive must contain exactly one CSV.");
        using Stream source = entries[0].Open();
        using var destination = new MemoryStream();
        source.CopyTo(destination);
        return destination.ToArray();
    }
}
