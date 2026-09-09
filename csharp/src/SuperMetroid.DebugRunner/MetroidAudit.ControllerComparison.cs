using System.Globalization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;

internal static partial class MetroidAudit
{
    /// <summary>Compares real bomb placement, Samus movement and Metroid state with native EnemyMain.</summary>
    public static int CompareControllerBombs(string rom, string trace)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        using (var capture = File.OpenRead(trace))
        {
            string hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(capture));
            if (hash != "2E2BD83117CF54C04F180CCE4DF54570F563426C9C365602FCE98E80C964D6B8")
                throw new InvalidDataException("Use the accepted full-alpha Metroid capture; the historical v2 oracle had an invalid phase order.");
        }
        var rows = File.ReadLines(trace).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 7200 || rows.Any(row => row.Length != 15))
            throw new InvalidDataException("Unexpected Metroid controller capture dimensions.");
        int cases = 0, mismatches = 0;
        foreach (var group in rows.GroupBy(row => string.Join(',', row[..3])))
        {
            var seed = group.First();
            bool left = seed[0] == "1";
            int travel = int.Parse(seed[1]), schedule = int.Parse(seed[2]);
            if (seed[0] is not ("0" or "1") || travel is < 0 or > 4 || schedule is < 0 or > 3)
                throw new InvalidDataException("Invalid Metroid controller seed.");
            var runtime = new SuperMetroidRuntime(bus);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RunNmi(0, true);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(MetroidRoomHeader, 0, 0);
            var level = runtime.LevelData!;
            for (int y = 0; y < level.HeightInBlocks; y++)
            for (int x = 0; x < level.WidthInBlocks; x++)
            {
                level.SetForegroundEntry(y * level.WidthInBlocks + x, y >= 10 ? (ushort)0x8000 : (ushort)0);
                level.SetBehavior(y * level.WidthInBlocks + x, 0);
            }
            runtime.InitializeDebugGroundedSamus(128, 100, 10);
            var samus = runtime.Samus!;
            samus.Pose = left ? SamusPoseIds.MorphBallGroundLeftPose : SamusPoseIds.MorphBallGroundRightPose;
            samus.EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs);
            samus.Health = samus.MaxHealth = 999;
            samus.XPosition = 128; samus.YPosition = 153;
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(0, 1);
            samus.PoseHistory.PreviousPose = samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement = (ushort)(0x400 | (left ? 4 : 8));
            samus.PoseHistory.LastDifferentPose = samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
            samus.InputLocked = false;
            var actor = runtime.Enemies.Slots[0];
            foreach (var other in runtime.Enemies.Slots.Skip(1)) other.Clear();
            actor.XPosition = 128; actor.YPosition = 145;
            actor.XSubposition = actor.YSubposition = 0;
            actor.Properties = 0x2000;
            var state = RequireState(runtime.Enemies, actor);
            runtime.Controller1.Latch(0);
            int frame = 0;
            int? firstDetach = null, firstReattach = null;
            bool reported = false;
            foreach (var row in group)
            {
                if (int.Parse(row[3]) != frame) throw new InvalidDataException("Reordered Metroid capture.");
                ushort input = ushort.Parse(row[4], NumberStyles.HexNumber);
                int[] gaps = [0, 16, 24, 48];
                int gap = gaps[schedule];
                int expectedInput = frame == 1 || gap != 0 && (frame == 1 + gap || frame == 1 + 2 * gap) ? 0x40 : 0;
                if (travel != 0 && frame >= 46 && frame < 46 + travel * 4) expectedInput |= left ? 0x200 : 0x100;
                if (input != expectedInput) throw new InvalidDataException("Changed Metroid controller input.");
                runtime.StepFrame(input);
                if (firstDetach is null && state.Function == MetroidAiFunction.PowerBombEscape)
                {
                    firstDetach = frame;
                    if (state.EscapeTimer != 3)
                        throw new InvalidDataException("Detachment must execute its first escape AI update in the same frame.");
                }
                if (firstDetach is not null && firstReattach is null && state.Function == MetroidAiFunction.AttachedToSamus)
                    firstReattach = frame;
                string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2},{samus.BombJumpDirection:X4}," +
                    $"{actor.XPosition:X4}{actor.XSubposition:X4},{actor.YPosition:X4}{actor.YSubposition:X4},{(ushort)state.Function:X4},{state.EscapeTimer:X4},{samus.Health:X4},{runtime.BombProjectiles.BombCounter:X4}";
                if (actual != string.Join(',', row[5..]))
                {
                    mismatches++;
                    if (!reported) Console.WriteLine($"METROID {group.Key} frame {frame}: {actual} != {string.Join(',', row[5..])}");
                    reported = true;
                }
                frame++;
            }
            if (frame != 180) throw new InvalidDataException("Incomplete Metroid case.");
            // Explicitly distinguish native misses from a successful detach that
            // reattaches four frames later. Radius inflation must not turn misses
            // into successes, and late AI must not extend the escape window.
            int? expectedDetach = travel == 2 ? 62 : travel == 0 && schedule >= 2 ? 108 :
                travel == 1 && schedule == 3 ? 156 : null;
            if (firstDetach != expectedDetach || firstReattach != expectedDetach + 4)
                throw new InvalidDataException($"Changed Metroid detach/reattach timing: {group.Key}, {firstDetach}/{firstReattach}.");
            cases++;
        }
        if (cases != 40) throw new InvalidDataException("Incomplete Metroid matrix.");
        Console.WriteLine($"Metroid controller: {cases} cases, {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
