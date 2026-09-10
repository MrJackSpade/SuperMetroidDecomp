using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

/// <summary>Native movement-handler boundary matrix for charged contact publication, not a full input route.</summary>
internal static class PseudoScrewLiquidAudit
{
    public static int Run(string rom, string trace)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trace))) !=
            "2F59678185E1161C9B7384BF45BFE6A67D7F580242448ED4D4786E5C8DE91584")
            throw new InvalidDataException("Use the accepted pseudo-liquid native v1 capture.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var level = new RoomLevelData(16, 16, new ushort[256], new byte[256], new ushort[256], []);
        int cases = 0, failures = 0;
        var seen = new HashSet<string>();
        foreach (string line in File.ReadLines(trace).Skip(1))
        {
            string[] row = line.Split(',');
            if (row.Length != 8 || !seen.Add(string.Join(',', row[..7])))
                throw new InvalidDataException("Malformed or repeated liquid gate case.");
            byte pose = byte.Parse(row[0], NumberStyles.HexNumber);
            ushort animation = ushort.Parse(row[1]), charge = ushort.Parse(row[2]);
            int gravity = int.Parse(row[3]), medium = int.Parse(row[4]), offset = int.Parse(row[5]), disabled = int.Parse(row[6]);
            var samus = new SamusState { Pose = pose, XPosition = 128, YPosition = 128,
                EquippedItems = gravity == 1 ? (ushort)SamusEquipmentFlags.GravitySuit : (ushort)0,
                ProjectileFlareCounter = charge };
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(animation, 2);
            samus.Kinematics.YDirection = 2;
            ushort surface = checked((ushort)(samus.Kinematics.TopBoundary + offset));
            if (medium == 1) samus.LiquidPhysics.ConfigureWater(surface);
            else if (medium >= 2) samus.LiquidPhysics.ConfigureLavaAcid(surface, acid: medium == 3);
            samus.LiquidPhysics.LiquidOptions = disabled == 1 ? (ushort)4 : (ushort)0;
            switch (samus.ReadMovementKind(bus))
            {
                case SamusMovementType.SpinJumping:
                    SamusAerialMovement.StepSpinJump(bus, level, samus, (ushort)SnesButton.A, 0);
                    break;
                case SamusMovementType.WallJumping:
                    SamusAerialMovement.StepWallJump(bus, level, samus, (ushort)SnesButton.A, 0);
                    break;
                case SamusMovementType.NormalJumping:
                    SamusAerialMovement.StepNormalJump(bus, level, samus, (ushort)SnesButton.A, 0);
                    break;
                default: throw new InvalidDataException("Unexpected pose in liquid gate capture.");
            }
            ushort expected = ushort.Parse(row[7], NumberStyles.HexNumber);
            // Independent witnesses make the native boundary observable, not merely a
            // count of agreeing rows. Equality at the top surface is not submerged.
            if (pose == SamusPoseIds.SpinJumpRightPose && charge == 60 && medium == 1)
            {
                ushort witness = gravity == 1 || disabled == 1 || offset >= 0 ? (ushort)4 : (ushort)0;
                if (expected != witness)
                    throw new InvalidDataException("Native submerged-spin boundary witness changed.");
            }
            if (pose == SamusPoseIds.WallJumpRightPose && charge == 60)
            {
                ushort witness = animation < 3 ? (ushort)0 : animation < 23 ? (ushort)4 : (ushort)3;
                if (expected != witness)
                    throw new InvalidDataException("Native walljump animation gate witness changed.");
            }
            if (samus.HorizontalSpeed.ContactDamageIndex != expected)
            {
                if (failures++ < 12) Console.WriteLine($"LIQUID GATE {line}: actual={samus.HorizontalSpeed.ContactDamageIndex:X4}");
            }
            cases++;
        }
        if (cases != 5760) throw new InvalidDataException("Incomplete liquid gate matrix.");
        Console.WriteLine($"Pseudo Screw liquid: {cases} handler cases, {failures} mismatches.");
        return failures == 0 ? 0 : 1;
    }
}
