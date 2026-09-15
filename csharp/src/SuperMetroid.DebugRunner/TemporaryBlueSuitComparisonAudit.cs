using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

/// <summary>Room-local, controller-earned temporary boost retention compared with original CPU execution.</summary>
internal static class TemporaryBlueSuitComparisonAudit
{
    public static int Run(string rom, string trace, TemporaryBlueAuditKind kind = TemporaryBlueAuditKind.Retention)
    {
        bool carry = kind == TemporaryBlueAuditKind.Carry, bounce = kind == TemporaryBlueAuditKind.Bounce;
        bool cancel = kind == TemporaryBlueAuditKind.Cancellation;
        bool sand = kind == TemporaryBlueAuditKind.Sand;
        bool terrain = kind == TemporaryBlueAuditKind.Terrain;
        bool chain = kind == TemporaryBlueAuditKind.Chain;
        bool menu = kind == TemporaryBlueAuditKind.Menu;
        bool midair = kind == TemporaryBlueAuditKind.DraygonMidair;
        bool echoes = midair || kind == TemporaryBlueAuditKind.DraygonEcho;
        bool draygon = echoes || kind == TemporaryBlueAuditKind.DraygonDeath;
        bool grab = kind == TemporaryBlueAuditKind.DraygonGrab;
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        if (Convert.ToHexString(SHA256.HashData(bus.Rom)) != "12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72")
            throw new InvalidDataException("Temporary Blue Suit audit requires the pinned Japan/USA ROM.");
        string text = File.ReadAllText(trace).Replace("\r\n", "\n", StringComparison.Ordinal);
        var (expectedCases, expectedFrames, expectedHash) = TemporaryBlueAuditDefinitions.Describe(kind);
        if (Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))) != expectedHash)
            throw new InvalidDataException("Use the accepted original-CPU temporary Blue Suit trace.");
        var rows = text.Split('\n', StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != expectedCases * expectedFrames || rows.Any(row => row.Length != (echoes ? 26 : sand ? 20 : terrain ? 22 : menu ? 19 : 18)))
            throw new InvalidDataException("Incomplete native temporary boost matrix.");
        int cases = 0, mismatches = 0;
        foreach (var group in rows.GroupBy(row => $"{row[0]},{row[1]},{row[2]}"))
        {
            var seed = group.First();
            bool left = seed[0] == "1";
            int stop = int.Parse(seed[1]), aim = int.Parse(seed[2]);
            if (seed[0] is not ("0" or "1") || (terrain ? stop is not (60 or 140) || aim is < 0 or > 7 : carry || bounce || cancel || sand || chain || menu || draygon || grab ? stop != 140 || aim < 0 || aim >= (draygon ? 12 : menu ? 6 : carry ? 40 : 8) : stop is not (60 or 100 or 140 or 180) || aim is < 0 or > 3))
                throw new InvalidDataException("Invalid temporary boost seed.");
            var game = menu ? PauseChargeCarryAudit.CreateFixture(rom, out _) : null;
            var runtime = game?.RuntimeForVerification ?? FlatFloorMovementFixture.Create(bus, water: false, wideRunway: true);
            var level = runtime.LevelData ?? throw new InvalidDataException("Missing fixture level.");
            for (int y = 0; y < level.HeightInBlocks; y++)
            for (int x = 0; x < level.WidthInBlocks; x++)
            {
                int index = y * level.WidthInBlocks + x;
                level.SetForegroundEntry(index, y == 32 ? (ushort)0x8000 : (ushort)0);
                level.SetBehavior(index, 0);
            }
            foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
            foreach (var actor in runtime.Enemies.EnemyProjectiles) actor.Clear();
            var samus = runtime.Samus ?? throw new InvalidDataException("Missing fixture Samus.");
            if (echoes) runtime.GameTime.Load(0, 0, 0, 0);
            samus.EquippedItems = samus.CollectedItems = (ushort)(SamusEquipmentFlags.SpeedBooster | SamusEquipmentFlags.MorphBall);
            if (bounce && aim >= 4) samus.EquippedItems = samus.CollectedItems = (ushort)(samus.EquippedItems | (ushort)SamusEquipmentFlags.SpringBall);
            samus.EquippedBeams = samus.CollectedBeams = 0;
            samus.Health = samus.MaxHealth = 99;
            samus.XPosition = (ushort)(left ? 2100 : 200); samus.YPosition = 491;
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.Pose = left ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose;
            samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(0, 1);
            samus.PoseHistory.PreviousPose = samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement = (ushort)(left ? 4 : 8);
            samus.PoseHistory.LastDifferentPose = samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
            runtime.Controller1.Latch(0);
            int frame = 0;
            bool reported = false;
            var audio = new CartridgeAudioState();
            var menuController = game is null ? null : new TemporaryBlueMenuController(game, samus);
            var grabProbe = grab ? new DraygonGrabBlueSuitProbe(bus, samus, left, aim) : null;
            foreach (var row in group)
            {
                ushort input = ushort.Parse(row[4], NumberStyles.HexNumber);
                int expectedInput = frame < stop ? 0x8000 | (left ? 0x200 : 0x100) : aim * 0x10;
                if (frame == stop) expectedInput |= 0x400;
                if (carry) expectedInput = TemporaryBlueCarryInputs.At(frame, left, aim);
                if (chain) expectedInput = TemporaryBlueChainInputs.At(frame, left, aim);
                if (menu) expectedInput = TemporaryBlueMenuController.At(frame, left, aim);
                if (draygon) expectedInput = DraygonBlueSuitProbe.InputAt(frame, left, aim);
                if (grab) expectedInput = DraygonGrabBlueSuitProbe.InputAt(frame, left, aim);
                if (midair) expectedInput = DraygonMidairInputs.At(frame, left, aim);
                if (bounce) expectedInput = TemporaryBlueBounceInputs.At(frame, left, aim);
                if (cancel) expectedInput = TemporaryBlueCancellationInputs.At(frame, left, aim);
                if (sand) expectedInput = TemporaryBlueCancellationInputs.At(frame, left, 0);
                if (terrain) expectedInput = frame < stop ? 0x8000 | (left ? 0x200 : 0x100) : frame == stop ? 0x410 : 0x10;
                if (int.Parse(row[3]) != frame || input != expectedInput)
                    throw new InvalidDataException("Changed controller sequence or reordered trace.");
                var publication = new GameplayAudioFramePublication(audio);
                if (cancel) TemporaryBlueCancellationInputs.BeforeFrame(samus, frame, aim);
                grabProbe?.BeforeFrame(frame);
                string terrainResult = "0000,0000,0000,0000";
                if (menuController is not null && frame >= 400)
                    menuController.Step(frame, aim, input);
                else if (terrain && frame == 400)
                    terrainResult = TemporaryBlueTerrainProbe.Run(bus, level, samus, aim, stop == 140);
                else if (sand && frame == 400)
                    TemporaryBlueSandProbe.Run(bus, level, samus, aim);
                else
                {
                    runtime.StepFrame(input, queueEchoSound: () => publication.QueueEcho(runtime));
                    publication.PublishPrefix(runtime);
                }
                if (draygon && frame == (midair ? 180 : DraygonBlueSuitProbe.DeathFrame(aim)))
                    DraygonBlueSuitProbe.KillThroughEye(bus, samus);
                grabProbe?.AfterFrame(frame);
                grabProbe?.Verify(frame);
                var speed = samus.HorizontalSpeed;
                if (stop >= 100 && frame is 89 or 90 &&
                    (speed.SpeedBoostCounter != 0x0401 || speed.ContactDamageIndex != (frame == 89 ? 0 : 1)))
                    throw new InvalidDataException("Full-boost animation must precede contact damage by one movement frame.");
                if (!carry && !bounce && !cancel && !sand && !terrain && !chain && !menu && !draygon && !grab && frame == 399 &&
                    (samus.Shinespark.ShineTimer != 0 || samus.Shinespark.PaletteType != 0 ||
                     speed.SpeedBoostCounter != (aim == 0 ? 0 : stop == 60 ? 0x0201 : stop == 100 ? 0x0402 : 0x0401)))
                    throw new InvalidDataException("Aim-held full/partial retention or no-aim cancellation changed after charge expiry.");
                if (carry) TemporaryBlueCarryInputs.Verify(samus, frame, aim);
                if (chain) TemporaryBlueChainInputs.Verify(samus, frame, aim);
                if (bounce) TemporaryBlueBounceInputs.Verify(samus, frame, aim);
                if (cancel) TemporaryBlueCancellationInputs.Verify(samus, frame, aim);
                if (draygon && !midair) DraygonBlueSuitProbe.Verify(samus, frame, aim);
                if (midair) DraygonMidairInputs.Verify(samus, frame, aim);
                if (stop >= 100 && frame == stop && samus.Shinespark.ShineTimer != 179)
                    throw new InvalidDataException("Controller crouch must earn and tick the 180-frame charge.");
                if (stop >= 100 && frame == stop + 179 && samus.Shinespark.ShineTimer != 0)
                    throw new InvalidDataException("Stored charge must expire on its native frame.");
                string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2}," +
                    $"{samus.AnimationFrame:X4},{samus.AnimationFrameTimer:X4},{speed.BaseFixed:X8}," +
                    $"{speed.ExtraRunSpeed:X4}{speed.ExtraRunSubspeed:X4},{speed.SpeedBoostCounter:X4},{speed.ContactDamageIndex:X4}," +
                    $"{samus.Shinespark.ShineTimer:X4},{samus.Shinespark.PaletteType:X4},{samus.Kinematics.VerticalSpeedFixed:X8},{samus.Kinematics.YDirection:X4}";
                if (sand) actual += $",{samus.Kinematics.ExtraXFixed:X8},{samus.Kinematics.ExtraYFixed:X8}";
                if (terrain) actual += $",{terrainResult}";
                if (menu) actual += $",{samus.EquippedItems:X4}";
                string expected = string.Join(',', echoes ? row[5..18] : row[5..]);
                if (echoes)
                {
                    string? mismatch = DraygonEchoComparison.FindMismatch(bus, samus, runtime.NmiFrameCounter, frame, row);
                    if (mismatch is not null)
                    {
                        mismatches++;
                        if (!reported) Console.WriteLine($"ECHO {group.Key} frame {frame}: {mismatch}");
                        reported = true;
                    }
                }
                if (actual != expected)
                {
                    if (draygon && frame == DraygonBlueSuitProbe.DeathFrame(aim))
                        throw new InvalidDataException($"Draygon death mismatch ({group.Key}) frame {frame}, phase {samus.Shinespark.Phase}: {actual} != {expected}");
                    mismatches++;
                    if (!reported) Console.WriteLine($"TEMP-BLUE {group.Key} frame {frame}: {actual} != {expected}");
                    reported = true;
                }
                frame++;
            }
            if (frame != expectedFrames) throw new InvalidDataException("Incomplete temporary boost sequence.");
            cases++;
        }
        if (cases != expectedCases) throw new InvalidDataException("Incomplete temporary boost matrix.");
        Console.WriteLine($"Temporary Blue Suit: {cases} cases, {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
