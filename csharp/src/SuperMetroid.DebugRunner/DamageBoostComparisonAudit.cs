using System.Globalization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Compares seeded hurt handoffs or live projectile contact against original CPU traces.</summary>
internal static class DamageBoostComparisonAudit
{
    public static int Run(string rom, string trace, bool hurtPrefixOnly = false)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var rows = File.ReadLines(trace).Skip(1).Select(line => line.Split(',')).ToArray();
        int contactKind = rows.Length > 0 && rows[0].Length == 26 ? int.Parse(rows[0][24]) : 0;
        if (contactKind is < 0 or > 4) throw new InvalidDataException("Unknown contact source.");
        bool contact = contactKind != 0;
        if (rows.Length != (contact ? 5952 : 11904) || rows.Any(row => row.Length != rows[0].Length) || rows[0].Length is not (22 or 24 or 26))
            throw new InvalidDataException("Unexpected damage-boost capture dimensions.");
        int samples = 0, mismatches = 0, initialMismatches = 0;
        int motionMismatches = 0, stateMismatches = 0, historyMismatches = 0;
        int healthMismatches = 0;
        int humanoidSamples = 0, humanoidMismatches = 0;
        var reportedGroups = new HashSet<string>();
        foreach (var group in rows.GroupBy(row => string.Join(',', row[..6])))
        {
            var seed = group.First();
            int timer = int.Parse(seed[0]);
            bool ball = seed[1] == "1", left = seed[2] == "1", forward = seed[4] == "1";
            if (hurtPrefixOnly && ball) continue;
            ushort source = ushort.Parse(seed[3]);
            int delay = int.Parse(seed[5]);
            int medium = seed.Length >= 24 ? int.Parse(seed[22]) : 0;
            int release = seed.Length >= 24 ? int.Parse(seed[23]) : 0;
            if (medium is < 0 or > 2 || release is < 0 or > 1 ||
                group.Any(row => row.Length >= 24 && (row[22] != seed[22] || row[23] != seed[23])) ||
                group.Any(row => row.Length == 26 && row[24] != contactKind.ToString(CultureInfo.InvariantCulture)))
                throw new InvalidDataException("Changed medium/release within hurt sequence.");
            var runtime = FlatFloorMovementFixture.Create(bus, water: false);
            var level = runtime.LevelData!;
            for (int x = 0; x < level.WidthInBlocks; x++) level.SetForegroundEntry(x, 0x8000);
            for (int y = 0; y <= 16; y++)
            {
                level.SetForegroundEntry(y * level.WidthInBlocks, 0x8000);
                level.SetForegroundEntry(y * level.WidthInBlocks + 15, 0x8000);
            }
            var samus = runtime.Samus!;
            if (medium == 1) samus.LiquidPhysics.ConfigureWater(8);
            if (medium == 2) samus.LiquidPhysics.ConfigureLavaAcid(8);
            samus.EquippedItems = (ushort)SamusEquipmentFlags.MorphBall;
            samus.Health = 99;
            samus.Pose = ball ? left ? SamusPoseIds.MorphBallGroundLeftPose : SamusPoseIds.MorphBallGroundRightPose
                : left ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose;
            samus.XPosition = 128; samus.YPosition = 160;
            if (contactKind == 3) samus.YPosition = ball ? (ushort)169 : (ushort)155;
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(0, 1);
            samus.PoseHistory.PreviousPose = samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement = (ushort)((ball ? 0x0400 : 0) | (left ? 4 : 8));
            samus.PoseHistory.LastDifferentPose = samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
            ushort initialInput = forward ? (ushort)(left ? 0x200 : 0x100) : (ushort)0;
            runtime.Controller1.Latch(initialInput);
            if (!contact)
            {
                SamusKnockbackMovement.Start(bus, samus, initialInput, source, (ushort)timer);
                samus.CommitPoseHistory(bus);
            }
            else if (contactKind == 1)
            {
                foreach (var enemy in runtime.Enemies.Slots)
                    enemy.Properties = enemy.Properties.With(EnemyProperties.Deleted);
                foreach (var actor in runtime.Enemies.EnemyProjectiles) actor.Clear();
                var projectile = runtime.Enemies.EnemyProjectiles[^1];
                projectile.Kind = RoomEnemyProjectileKind.CeresRidleyFireball;
                projectile.PreInstruction = EnemyProjectileCodePointers.RTS_8684FB;
                projectile.InstructionTimer = 2;
                projectile.XPosition = source == 1 ? (ushort)120 : (ushort)136;
                projectile.YPosition = 160;
                projectile.XRadius = projectile.YRadius = 8;
                projectile.Damage = 20;
                projectile.InvincibilityFrames = 96;
                projectile.CanDamageSamus = true;
            }
            else if (contactKind == 2)
            {
                int block = (source == 1 ? 9 : 10) * level.WidthInBlocks + 8;
                level.SetForegroundEntry(block, 0x2000);
                level.SetBehavior(block, SamusTerrainHazardRomData.DamagingSpikeAirBehavior);
            }
            else if (contactKind == 3)
            {
                for (int x = 1; x < 15; x++)
                {
                    int block = 11 * level.WidthInBlocks + x;
                    level.SetForegroundEntry(block, 0xa000);
                    level.SetBehavior(block, (byte)source);
                }
            }
            else
            {
                // Keep a real, stationary Ripper in the production enemy frame, with
                // a live spritemap so its ordinary overlap/touch route is eligible.
                foreach (var actor in runtime.Enemies.Slots) actor.Clear();
                foreach (var actor in runtime.Enemies.EnemyProjectiles) actor.Clear();
                var enemy = runtime.Enemies.Slots[0];
                enemy.EnemyDefinitionPointer = RoomEnemySystem.RipperDefinition;
                enemy.Definition = RoomEnemySystem.ReadDefinition(bus, enemy.EnemyDefinitionPointer);
                enemy.AiBank = enemy.Definition.Bank;
                enemy.XPosition = source == 1 ? (ushort)120 : (ushort)136;
                enemy.YPosition = 160;
                enemy.XRadius = enemy.Definition.XRadius;
                enemy.YRadius = enemy.Definition.YRadius;
                enemy.Health = enemy.Definition.Health;
                enemy.SpritemapPointer = MovementContactFixtureData.RipperRightSpritemap;
                enemy.CurrentInstruction = MovementContactFixtureData.RipperRightInstructionList;
                enemy.InstructionTimer = ushort.MaxValue;
            }
            int frame = -1;
            foreach (var row in group)
            {
                if (int.Parse(row[6]) != frame) throw new InvalidDataException("Reordered hurt trace.");
                ushort input = ushort.Parse(row[7], NumberStyles.HexNumber);
                ushort expectedInput = frame < 0 ? initialInput : frame >= delay ? (ushort)((left ? 0x100 : 0x200) | 0x80) : (ushort)0;
                if (frame >= 0 && release != 0 && frame >= delay + 3) expectedInput = 0x80;
                if (input != expectedInput) throw new InvalidDataException("Changed boost input sequence.");
                if (frame >= 0) runtime.StepFrame(input);
                // Isolate the ordinary hurt fallback from the separately failing boost
                // initializer and expiry handoff. Still compare every recorded word,
                // not just history, within this explicitly bounded acceptance gate.
                if (hurtPrefixOnly && frame >= 0 && (frame >= delay || frame >= timer))
                {
                    frame++;
                    continue;
                }
                var h = samus.PoseHistory;
                var s = samus.HorizontalSpeed;
                string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2},{samus.AnimationFrame:X4}," +
                    $"{samus.KnockbackTimer:X4},{samus.KnockbackDirection:X4},{samus.Kinematics.YSpeed:X4}{samus.Kinematics.YSubspeed:X4},{samus.Kinematics.YDirection:X4}," +
                    $"{s.BaseFixed:X8},{s.ExtraRunSpeed:X4}{s.ExtraRunSubspeed:X4},{h.PreviousPose:X4},{h.PreviousDirectionAndMovement:X4},{h.LastDifferentPose:X4},{h.LastDifferentDirectionAndMovement:X4}";
                string expected = string.Join(',', row[8..22]);
                var actualWords = actual.Split(',');
                if (!actualWords[..4].SequenceEqual(row[8..12])) motionMismatches++;
                if (!actualWords[4..10].SequenceEqual(row[12..18])) stateMismatches++;
                if (!actualWords[10..].SequenceEqual(row[18..22])) historyMismatches++;
                bool healthMatches = row.Length != 26 || samus.Health == ushort.Parse(row[25], NumberStyles.HexNumber);
                if (!healthMatches) healthMismatches++;
                if (actual != expected || !healthMatches)
                {
                    if (!ball) humanoidMismatches++;
                    if (frame < 0) initialMismatches++;
                    mismatches++;
                    if (reportedGroups.Add(group.Key) && reportedGroups.Count <= 16)
                        Console.WriteLine($"DAMAGE {group.Key} frame={frame}: {actual} != {expected}; health={samus.Health:X4}/{(row.Length == 26 ? row[25] : "unrecorded")}");
                }
                if (!ball) humanoidSamples++;
                samples++; frame++;
            }
            if (frame != 30) throw new InvalidDataException("Incomplete hurt sequence.");
        }
        if (hurtPrefixOnly && samples != 1072)
            throw new InvalidDataException("Incomplete humanoid hurt-prefix coverage.");
        Console.WriteLine($"Damage boost{(hurtPrefixOnly ? " hurt prefix" : "")}: {samples} samples, {mismatches} mismatches ({initialMismatches} at initialization).");
        Console.WriteLine($"Motion/pose/animation={motionMismatches}; timers/direction/speeds={stateMismatches}; history={historyMismatches}; health={healthMismatches}.");
        Console.WriteLine($"Humanoid: {humanoidSamples} samples, {humanoidMismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
