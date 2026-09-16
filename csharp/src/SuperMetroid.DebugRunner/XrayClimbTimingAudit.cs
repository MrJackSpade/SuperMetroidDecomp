using System.Globalization;
using System.IO.Compression;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>Compares X-Ray climb release windows frame-for-frame with the original CPU.</summary>
internal static class XrayClimbTimingAudit
{
    private const string AcceptedCaptureSha256 =
        "C8F15D9C669E5559686E12D5E848013A76D79967DB3EDB8BA725E92E397A6DC5";
    private const int TurnFrame = 16;
    private const int FramesPerCase = 48;

    public static int Run(string rom, string capture)
    {
        byte[] captureBytes = ReadCapture(capture);
        if (Convert.ToHexString(SHA256.HashData(captureBytes)) != AcceptedCaptureSha256)
            throw new InvalidDataException("Use the accepted native X-Ray timing v4 capture for issue 437.");

        string[] lines = new StreamReader(new MemoryStream(captureBytes))
            .ReadToEnd()
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        const string header = "left,water,offset,frame,input,x,y,pose,xradius,yradius,xdir,movement,anim,timer,phase,frozen";
        const int caseCount = 2 * 2 * 21;
        if (lines.Length != 1 + caseCount * FramesPerCase || lines[0] != header)
            throw new InvalidDataException("Incomplete X-Ray timing matrix.");

        string[][] rows = lines.Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Any(row => row.Length != 16))
            throw new InvalidDataException("Malformed X-Ray timing row.");

        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        int mismatches = 0;
        int comparedFrames = 0;
        int successfulCases = 0;
        for (int left = 0; left < 2; left++)
        for (int water = 0; water < 2; water++)
        for (int releaseOffset = -8; releaseOffset <= 12; releaseOffset++)
        {
            SamusState samus = CreateActiveFullBeam(bus, left != 0, water != 0);
            bool finished = false;
            for (int frame = 0; frame < FramesPerCase; frame++)
            {
                int rowIndex = (((left * 2 + water) * 21 + releaseOffset + 8) * FramesPerCase) + frame;
                string[] expected = rows[rowIndex];
                ValidateCoordinates(expected, left, water, releaseOffset, frame);

                int releaseFrame = TurnFrame + releaseOffset;
                ushort input = frame < releaseFrame ? (ushort)SnesButton.B : (ushort)0;
                if (frame == TurnFrame)
                    input |= left != 0 ? (ushort)SnesButton.Right : (ushort)SnesButton.Left;

                // The native main loop runs HDMA before gameplay. X-Ray state five can
                // therefore restore the normal handler pair before alpha samples input.
                if (samus.Xray.IsActive)
                    samus.Xray.StepBeam(bus, samus, input);
                if (samus.Xray.IsActive)
                {
                    samus.Xray.HandlePoseInput(bus, samus, input);
                    samus.Xray.StepMovement(bus, samus);
                }
                else
                {
                    samus.RefreshCollisionRadii(bus);
                }
                samus.AnimateNoFx(bus, input);

                string actual = $"{left},{water},{releaseOffset},{frame},{input:X4}," +
                    $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2}," +
                    $"{samus.Kinematics.XRadius:X4},{samus.Kinematics.YRadius:X4}," +
                    $"{samus.ReadPoseXDirection(bus):X2},{(byte)samus.ReadMovementType(bus):X2}," +
                    $"{samus.AnimationFrame:X4},{samus.AnimationFrameTimer:X4}," +
                    $"{(ushort)samus.Xray.BeamPhase:X4},{(samus.Xray.TimeIsFrozen ? 1 : 0):X4}";
                string expectedLine = string.Join(',', expected);
                comparedFrames++;
                if (actual != expectedLine)
                {
                    mismatches++;
                    if (mismatches <= 20)
                        Console.WriteLine($"XRAY TIMING {left},{water},{releaseOffset},{frame}: {actual} != {expectedLine}");
                }

                if (!samus.Xray.TimeIsFrozen)
                {
                    if (samus.Kinematics.YFixed == 0x01fb0000)
                        successfulCases++;
                    finished = true;
                    break;
                }
            }

            if (!finished)
                throw new InvalidDataException($"X-Ray timing case {left},{water},{releaseOffset} did not finish.");
        }

        // Air's five-frame release window is -2..+2. Suitless water lengthens the
        // crouched turn and extends the successful upper edge through +11.
        if (successfulCases != 38)
            throw new InvalidDataException($"Native timing witness expected 38 successful cases, observed {successfulCases}.");

        Console.WriteLine($"X-Ray climb timing: {comparedFrames} frames, {mismatches} mismatches; {successfulCases} successful cases.");
        return mismatches == 0 ? 0 : 1;
    }

    private static SamusState CreateActiveFullBeam(
        ISnesAddressSpace bus,
        bool facingLeft,
        bool underwater)
    {
        var samus = new SamusState
        {
            XPosition = 0x0100,
            YPosition = 0x0200,
            Health = 99,
            MaxHealth = 99,
            EquippedItems = (ushort)SamusEquipmentFlags.XrayScope,
            Pose = facingLeft ? SamusPoseIds.CrouchingLeftPose : SamusPoseIds.CrouchingRightPose,
        };
        if (underwater)
            samus.LiquidPhysics.ConfigureWater(8);
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.PoseHistory.PreviousPose = samus.Pose;
        samus.PoseHistory.PreviousDirectionAndMovement =
            (ushort)(((byte)samus.ReadMovementType(bus) << 8) | samus.ReadPoseXDirection(bus));
        if (!samus.Xray.TryBegin(bus, samus, SamusMovementType.Crouching))
            throw new InvalidDataException("Constructed X-Ray timing case did not activate.");

        for (int stage = 0; stage < 8; stage++)
            samus.Xray.StepBeam(bus, samus, (ushort)SnesButton.B);
        while (samus.Xray.BeamPhase != XrayBeamPhase.Full)
            samus.Xray.StepBeam(bus, samus, (ushort)SnesButton.B);
        samus.SetAnimationFrameFromSpecialHandler(frame: 2, timer: 15);
        return samus;
    }

    private static void ValidateCoordinates(
        string[] row,
        int left,
        int water,
        int releaseOffset,
        int frame)
    {
        if (int.Parse(row[0], CultureInfo.InvariantCulture) != left ||
            int.Parse(row[1], CultureInfo.InvariantCulture) != water ||
            int.Parse(row[2], CultureInfo.InvariantCulture) != releaseOffset ||
            int.Parse(row[3], CultureInfo.InvariantCulture) != frame)
        {
            throw new InvalidDataException("Reordered X-Ray timing cases.");
        }
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
            throw new InvalidDataException("X-Ray timing archive must contain exactly one CSV.");
        using Stream source = entries[0].Open();
        using var destination = new MemoryStream();
        source.CopyTo(destination);
        return destination.ToArray();
    }
}
