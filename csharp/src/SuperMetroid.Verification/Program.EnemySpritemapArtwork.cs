using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyInstalledAlcoonInstructionFrames(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog stock)
    {
        for (int index = 0;
             index < AlcoonInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = AlcoonInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                    RoomEnemySystem.AlcoonDefinition, operand, out ushort frame),
                $"Alcoon visual operand $A8:{operand:X4} is compiled");
            AssertEqual(ReadAlcoonInstructionWord(rom, operand), frame,
                $"Alcoon frame selector $A8:{operand:X4} matches the pinned cartridge");
        }
        AssertThrows<InvalidDataException>(
            () => EnemySpritemapDefinitions.AlcoonFrameAt(0xdcc7),
            "Alcoon rejects adjacent control data as presentation");
        VerifyAlcoonInstructionProgramDefinitions(rom, stock);
    }

    private static void VerifyInstalledBeetomInstructionFrames(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog stock)
    {
        for (int index = 0;
             index < BeetomInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = BeetomInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                    RoomEnemySystem.BeetomDefinition, operand, out ushort frame),
                $"Beetom visual operand $A8:{operand:X4} is compiled");
            AssertEqual(ReadBeetomInstructionWord(rom, operand), frame,
                $"Beetom selector $A8:{operand:X4} matches the pinned cartridge");
        }
        AssertThrows<InvalidDataException>(
            () => EnemySpritemapDefinitions.BeetomFrameAt(0xb6c0),
            "Beetom rejects the adjacent unused hop program as presentation");
        VerifyBeetomInstructionProgramDefinitions(rom, stock);
    }

    private static void VerifyInstalledHopperInstructionFrames(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog stock)
    {
        ushort[] definitions =
        [
            RoomEnemySystem.SidehopperDefinition,
            RoomEnemySystem.DessgeegaDefinition,
            RoomEnemySystem.LargeSidehopperDefinition,
            RoomEnemySystem.TourianSidehopperDefinition,
            RoomEnemySystem.LargeDessgeegaDefinition,
        ];
        for (int index = 0;
             index < HopperInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = HopperInstructionProgramDefinitions.PresentationWordAddress(index);
            ushort compiled = EnemySpritemapDefinitions.HopperFrameAt(operand);
            AssertEqual(ReadHopperAnimationWord(rom, 0xa30000 | operand), compiled,
                $"Hopper visual selector $A3:{operand:X4} matches the pinned cartridge");
            foreach (ushort definition in definitions)
            {
                AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                        definition, operand, out ushort selected) && selected == compiled,
                    $"Hopper family ${definition:X4} compiles visual operand $A3:{operand:X4}");
            }
        }
        AssertThrows<InvalidDataException>(
            () => EnemySpritemapDefinitions.HopperFrameAt(
                HopperInstructionProgramDefinitions.LastAdjacentPhysicsWord),
            "Hopper rejects adjacent physics data as a visual selector");
        VerifyHopperAnimationDefinitions(rom, stock);
    }

    private static void VerifyInstalledChootInstructionFrames(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog stock)
    {
        for (int index = 0;
             index < ChootInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = ChootInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                    RoomEnemySystem.ChootDefinition, operand, out ushort frame),
                $"Choot visual operand $A2:{operand:X4} is compiled");
            AssertEqual(ReadChootInstructionWord(rom, 0xa20000 | operand), frame,
                $"Choot selector $A2:{operand:X4} matches the pinned cartridge");
        }
        AssertThrows<InvalidDataException>(
            () => EnemySpritemapDefinitions.ChootFrameAt(0xd84c),
            "Choot rejects adjacent path data as a visual selector");
        VerifyChootInstructionProgramDefinitions(rom, stock);
    }

    private static void VerifyInstalledBullInstructionFrames(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog stock,
        BindingFlags flags)
    {
        for (int index = 0;
             index < BullInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = BullInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                    RoomEnemySystem.BullDefinition, operand, out ushort frame),
                $"Bull visual operand $A8:{operand:X4} is compiled");
            AssertEqual(ReadBullInstructionWord(rom, 0xa80000 | operand), frame,
                $"Bull frame selector $A8:{operand:X4} matches the pinned cartridge");
        }
        AssertThrows<InvalidDataException>(
            () => EnemySpritemapDefinitions.BullFrameAt(0xd871),
            "Bull rejects adjacent shot-angle data as presentation");

        var guard = new BullInstructionProgramReadGuard(rom, forbidPresentation: true);
        var enemies = new RoomEnemySystem { TileArtwork = stock };
        Type type = typeof(RoomEnemySystem);
        type.GetField("_bus", flags)!.SetValue(enemies, guard);
        RoomEnemySlot bull = enemies.Slots[0];
        bull.EnemyDefinitionPointer = RoomEnemySystem.BullDefinition;
        bull.Definition = default(RoomEnemyDefinition) with
            { Bank = EnemySpritemapDefinitions.BullBank };
        type.GetMethod("InitializeBull", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies)(bull);
        MethodInfo process = type.GetMethod("ProcessInstructions", flags)!;
        object?[] arguments =
            [bull, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        for (int frame = 0; frame < 41; frame++)
            process.Invoke(enemies, arguments);
        AssertEqual(unchecked((ushort)(BullInstructionProgramDefinitions.Normal + 4)),
            bull.CurrentInstruction,
            "installed Bull normal animation retains its cursor and timing");
        BullEnemyState state = enemies.BullStates[0] ?? throw new InvalidDataException(
            "Bull initializer did not publish typed state.");
        type.GetMethod("ResolveBullImmuneShot", flags | BindingFlags.Static)!
            .Invoke(null, [bull, state, (ushort)2]);
        for (int frame = 0; frame < 61; frame++)
            process.Invoke(enemies, arguments);
        AssertEqual(unchecked((ushort)(BullInstructionProgramDefinitions.Normal + 4)),
            bull.CurrentInstruction,
            "installed Bull immune-shot program returns to the normal loop");
        AssertEqual((ushort)0, bull.Timer,
            "installed Bull immune-shot loop exhausts its native repeat timer");
        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "installed Bull programs read no cartridge visual selectors");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "installed Bull programs retain compiled timing and control words");
    }

    private static void VerifyInstalledPuyoInstructionFrames(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog stock,
        BindingFlags flags)
    {
        for (int index = 0;
             index < PuyoInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = PuyoInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                    RoomEnemySystem.PuyoDefinition, operand, out ushort frame),
                $"Puyo visual operand $A2:{operand:X4} is compiled");
            AssertEqual(ReadPuyoInstructionWord(rom, operand), frame,
                $"Puyo frame selector $A2:{operand:X4} matches the pinned cartridge");
        }
        AssertThrows<InvalidDataException>(
            () => EnemySpritemapDefinitions.PuyoFrameAt(
                PuyoInstructionProgramDefinitions.FirstAdjacentDefinition),
            "Puyo rejects adjacent hop definitions as presentation operands");

        var guard = new PuyoInstructionReadGuard(rom, forbidPresentation: true);
        var enemies = new RoomEnemySystem { TileArtwork = stock };
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        RoomEnemySlot puyo = enemies.Slots[0];
        puyo.EnemyDefinitionPointer = RoomEnemySystem.PuyoDefinition;
        puyo.Definition = default(RoomEnemyDefinition) with
            { Bank = EnemySpritemapDefinitions.PuyoBank };
        typeof(RoomEnemySystem).GetMethod("InitializePuyo", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies)(puyo);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessInstructions", flags)!;
        object?[] arguments =
            [puyo, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        foreach (ushort program in PuyoGroundedInstructionPrograms)
        {
            puyo.CurrentInstruction = program;
            ExecutePuyoInstructionCalls(enemies, process, arguments, puyo, 5);
        }
        foreach (ushort program in PuyoAirborneInstructionPrograms)
        {
            puyo.CurrentInstruction = program;
            ExecutePuyoInstructionCalls(enemies, process, arguments, puyo, 2);
        }
        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "installed Puyo programs read no cartridge visual selectors");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "installed Puyo programs retain compiled timing and control words");
    }

    private static void VerifyInstalledEnemySpritemaps(
        SuperMetroidAddressSpace rom, string stockDirectory,
        EnemyTileArtworkCatalog stock)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        AssertTrue(stock.Spritemaps is not null,
            "installed enemy catalog contains named OAM compositions");
        AssertEqual(EnemySpritemapDefinitions.PreCeresDoorFrameCount,
            EnemySpritemapDefinitions.PreDisplayBindingsFrameCount,
            "Ceres-door expansion retains the previous display-binding identities");
        AssertEqual(EnemySpritemapDefinitions.PreCeresDoorFrameCount + 15,
            EnemySpritemapDefinitions.PreCeresBabyFrameCount,
            "Ceres door adds its thirteen selected poses, initial pose, and private overlay");
        AssertEqual(EnemySpritemapDefinitions.PreCeresBabyFrameCount + 3,
            EnemySpritemapDefinitions.PreRioFrameCount,
            "Ceres Baby adds its three authored OAM poses");
        AssertEqual(EnemySpritemapDefinitions.PreRioFrameCount + 8,
            EnemySpritemapDefinitions.PreLowerNorfairRioFrameCount,
            "Rio adds its eight distinct OAM compositions");
        AssertEqual(EnemySpritemapDefinitions.PreLowerNorfairRioFrameCount + 18,
            EnemySpritemapDefinitions.PreNorfairRioFrameCount,
            "Lower Norfair Rio adds its eighteen parent and flame compositions");
        AssertEqual(EnemySpritemapDefinitions.PreNorfairRioFrameCount + 20,
            EnemySpritemapDefinitions.PrePuyoFrameCount,
            "Norfair Rio adds its twenty parent and flame compositions");
        AssertEqual(EnemySpritemapDefinitions.PrePuyoFrameCount + 8,
            EnemySpritemapDefinitions.PreBullFrameCount,
            "Puyo adds its eight distinct ground and airborne compositions");
        AssertEqual(EnemySpritemapDefinitions.PreBullFrameCount + 3,
            EnemySpritemapDefinitions.PreAlcoonFrameCount,
            "Bull adds its three distinct normal and immune-shot compositions");
        AssertEqual(EnemySpritemapDefinitions.PreAlcoonFrameCount + 18,
            EnemySpritemapDefinitions.PreBeetomFrameCount,
            "Alcoon adds eighteen left/right walking, firing, and airborne compositions");
        AssertEqual(EnemySpritemapDefinitions.PreBeetomFrameCount + 22,
            EnemySpritemapDefinitions.PreHopperFrameCount,
            "Beetom adds twenty-two distinct crawl, hop, and drain compositions");
        AssertEqual(EnemySpritemapDefinitions.PreHopperFrameCount + 24,
            EnemySpritemapDefinitions.PreChootFrameCount,
            "Hoppers add twenty-four distinct floor/ceiling compositions");
        AssertEqual(EnemySpritemapDefinitions.PreChootFrameCount + 4,
            EnemySpritemapDefinitions.PreHZoomerFrameCount,
            "Choot adds four idle, jumping, and falling compositions");
        AssertEqual(EnemySpritemapDefinitions.PreHZoomerFrameCount + 20,
            EnemySpritemapDefinitions.PreSbugFrameCount,
            "HZoomer adds five frames in each of four surface orientations");
        AssertEqual(EnemySpritemapDefinitions.PreSbugFrameCount + 24,
            EnemySpritemapDefinitions.PreFuneNamiheFrameCount,
            "Sbug adds three distinct frames in each of eight facing directions");
        AssertEqual(EnemySpritemapDefinitions.PreFuneNamiheFrameCount + 22,
            EnemySpritemapDefinitions.PreKamerFrameCount,
            "Fune/Namihe add all left/right idle and active OAM frames");
        AssertEqual(EnemySpritemapDefinitions.PreKamerFrameCount + 4,
            EnemySpritemapDefinitions.PreElevatorFrameCount,
            "Kamer platform adds its four native OAM frames");
        AssertEqual(EnemySpritemapDefinitions.PreElevatorFrameCount + 2,
            EnemySpritemapDefinitions.PreDraygonIntroFrameCount,
            "elevator adds its two native OAM frames");
        AssertEqual(EnemySpritemapDefinitions.PreDraygonIntroFrameCount + 4,
            EnemySpritemapDefinitions.PreDraygonBreathFrameCount,
            "Draygon intro adds its four native Evir sprite-object frames");
        AssertEqual(EnemySpritemapDefinitions.PreDraygonBreathFrameCount + 9,
            EnemySpritemapDefinitions.PreRoomSpriteObjectFrameCount,
            "Draygon breath bubble adds its nine native sprite-object frames");
        AssertEqual(EnemySpritemapDefinitions.PreRoomSpriteObjectFrameCount + 263,
            EnemySpritemapDefinitions.PreYappingMawFrameCount,
            "remaining room sprite-object programs add 263 distinct visual frames");
        AssertEqual(EnemySpritemapDefinitions.PreYappingMawFrameCount + 24,
            EnemySpritemapDefinitions.PreKiHunterFrameCount,
            "Yapping Maw adds 24 distinct OAM frames");
        AssertEqual(EnemySpritemapDefinitions.PreKiHunterFrameCount + 41,
            EnemySpritemapDefinitions.PreMotherBrainFrameCount,
            "KiHunter adds 41 distinct body and wing OAM frames");
        AssertEqual(EnemySpritemapDefinitions.PreMotherBrainFrameCount + 18,
            EnemySpritemapDefinitions.PreDeadTorizoFrameCount,
            "Mother Brain adds eighteen head, neck, and falling-tube OAM frames");
        AssertEqual(EnemySpritemapDefinitions.PreDeadTorizoFrameCount + 1,
            EnemySpritemapDefinitions.PreRidleySupplementFrameCount,
            "Dead Torizo adds its private corpse OAM frame");
        AssertEqual(EnemySpritemapDefinitions.PreRidleySupplementFrameCount + 31,
            EnemySpritemapDefinitions.PreSciserFrameCount,
            "Ridley adds sixteen tips, twelve wings, and three tail segment OAM frames");
        AssertEqual(EnemySpritemapDefinitions.PreSciserFrameCount + 12,
            EnemySpritemapDefinitions.PreFlyFrameCount,
            "Sciser adds twelve distinct surface-animation OAM frames");
        AssertEqual(EnemySpritemapDefinitions.PreFlyFrameCount + 4,
            EnemySpritemapDefinitions.PreKagoFrameCount,
            "Mellow, Mella and Memu share four flight OAM frames");
        AssertEqual(EnemySpritemapDefinitions.PreKagoFrameCount + 3,
            EnemySpritemapDefinitions.PreFaceBlockFrameCount,
            "Kago adds three OAM frames shared by its slow and fast loops");
        AssertEqual(EnemySpritemapDefinitions.PreFaceBlockFrameCount + 5,
            EnemySpritemapDefinitions.PreMorphBallEyeFrameCount,
            "face block adds five distinct neutral and directional OAM frames");
        AssertEqual(EnemySpritemapDefinitions.PreMorphBallEyeFrameCount + 22,
            EnemySpritemapDefinitions.PreShutterFrameCount,
            "Morph Ball eye and mount add twenty-two distinct OAM frames");
        AssertEqual(EnemySpritemapDefinitions.PreShutterFrameCount + 5,
            EnemySpritemapDefinitions.PreMetroidFrameCount,
            "growing, vertical and horizontal shutters share five distinct OAM frames");
        AssertEqual(EnemySpritemapDefinitions.PreMetroidFrameCount + 4,
            EnemySpritemapDefinitions.PreShaktoolFrameCount,
            "both ordinary-Metroid loops share four distinct body OAM frames");
        AssertEqual(EnemySpritemapDefinitions.PreShaktoolFrameCount + 15,
            EnemySpritemapDefinitions.PreTripperKamerFrameCount,
            "Shaktool's saw, arm, and head programs select fifteen OAM frames");
        AssertEqual(EnemySpritemapDefinitions.PreTripperKamerFrameCount + 18,
            EnemySpritemapDefinitions.PreDragonFrameCount,
            "Tripper/Kamer add sixteen animated and two frozen OAM frames");
        AssertEqual(EnemySpritemapDefinitions.PreDragonFrameCount + 12,
            EnemySpritemapDefinitions.PreMultiviolaFrameCount,
            "Dragon adds twelve distinct body and wing OAM frames");
        AssertEqual(EnemySpritemapDefinitions.PreMultiviolaFrameCount +
                MultiviolaVisualDefinitions.FrameCount,
            EnemySpritemapDefinitions.PreNorfairLavaJumperFrameCount,
            "Multiviola adds eight distinct spinning OAM frames");
        AssertEqual(EnemySpritemapDefinitions.PreNorfairLavaJumperFrameCount +
                NorfairLavaJumperVisualDefinitions.FrameCount,
            EnemySpritemapDefinitions.PreChozoStatueFrameCount,
            "Norfair lava jumper adds ten visible and one empty OAM frame");
        AssertEqual(EnemySpritemapDefinitions.PreChozoStatueFrameCount +
                ChozoStatueVisualDefinitions.FrameCount,
            EnemySpritemapDefinitions.PreViolaFrameCount,
            "Lower Norfair and Wrecked Ship Chozo share twenty-six OAM frames");
        AssertEqual(EnemySpritemapDefinitions.PreViolaFrameCount +
                ViolaVisualDefinitions.FrameCount,
            EnemySpritemapDefinitions.PreRinkaFrameCount,
            "Viola adds eight distinct OAM frames");
        AssertEqual(EnemySpritemapDefinitions.PreRinkaFrameCount +
                RinkaVisualDefinitions.FrameCount,
            EnemySpritemapDefinitions.PreDeadTorizoStationaryFrameCount,
            "Rinka adds five OAM frames shared by ordinary and boss loops");
        AssertEqual(EnemySpritemapDefinitions.PreDeadTorizoStationaryFrameCount + 1,
            EnemySpritemapDefinitions.PreDeadTourianCorpseFrameCount,
            "Dead Torizo adds its stationary corpse composition after version 53");
        AssertEqual(EnemySpritemapDefinitions.PreDeadTourianCorpseFrameCount +
                DeadTourianCorpseVisualDefinitions.FrameCount,
            EnemySpritemapDefinitions.PreMochtroidFrameCount,
            "dead Zoomer, Ripper, Skree, and Sidehopper add thirteen compositions");
        AssertEqual(EnemySpritemapDefinitions.PreMochtroidFrameCount +
                MochtroidVisualDefinitions.FrameCount,
            EnemySpritemapDefinitions.Frames.Length,
            "Mochtroid adds six free-flight and attached OAM compositions");
        HashSet<ushort> installedHunterPointers = EnemySpritemapDefinitions.Frames
            .ToArray()
            .Where(frame => frame.Name.StartsWith("ki_hunter_a8_", StringComparison.Ordinal))
            .Select(frame => frame.Pointer)
            .ToHashSet();
        for (int index = 0;
             index < KiHunterInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = KiHunterInstructionProgramDefinitions
                .PresentationWordAddress(index);
            ushort native = unchecked((ushort)(rom.ReadByte(0xa80000 | operand) |
                rom.ReadByte(0xa80000 | unchecked((ushort)(operand + 1))) << 8));
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                    RoomEnemySystem.KiHunterDefinition, operand, out ushort selected),
                $"KiHunter visual selector $A8:{operand:X4} is installed");
            AssertEqual(native, selected,
                $"KiHunter visual selector $A8:{operand:X4} matches cartridge");
            AssertTrue(installedHunterPointers.Contains(native),
                $"KiHunter visual target $A8:{native:X4} has installed art");
        }
        AssertEqual(41, installedHunterPointers.Count,
            "KiHunter's visual operands select 41 distinct OAM frames");
        AssertThrows<InvalidDataException>(
            () => KiHunterVisualDefinitions.FrameAt(
                KiHunterInstructionProgramDefinitions.FlyingLeft),
            "KiHunter rejects adjacent instruction mechanics as presentation");
        HashSet<ushort> installedMawPointers = EnemySpritemapDefinitions.Frames
            .ToArray()
            .Where(frame => frame.Name.StartsWith("yapping_maw_a8_", StringComparison.Ordinal))
            .Select(frame => frame.Pointer)
            .ToHashSet();
        for (int index = 0;
             index < YappingMawInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = YappingMawInstructionProgramDefinitions
                .PresentationWordAddress(index);
            ushort native = unchecked((ushort)(rom.ReadByte(0xa80000 | operand) |
                rom.ReadByte(0xa80000 | unchecked((ushort)(operand + 1))) << 8));
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                    RoomEnemySystem.YappingMawDefinition, operand, out ushort selected),
                $"Yapping Maw visual selector $A8:{operand:X4} is installed");
            AssertEqual(native, selected,
                $"Yapping Maw visual selector $A8:{operand:X4} matches cartridge");
            AssertTrue(installedMawPointers.Contains(native),
                $"Yapping Maw visual target $A8:{native:X4} has installed art");
        }
        AssertEqual(24, installedMawPointers.Count,
            "Yapping Maw's 52 visual operands select 24 distinct frames");
        AssertThrows<InvalidDataException>(
            () => YappingMawVisualDefinitions.FrameAt(
                YappingMawInstructionProgramDefinitions.AttackingFacingUp),
            "Yapping Maw rejects adjacent instruction mechanics as presentation");
        AssertEqual(276, EnemySpritemapDefinitions.Frames.ToArray().Count(
                frame => frame.Bank == EnemySpritemapDefinitions.RoomSpriteObjectBank),
            "all bank-B4 sprite-object programs select 276 distinct visual frames");
        HashSet<ushort> installedSpritePointers = EnemySpritemapDefinitions.Frames
            .ToArray()
            .Where(frame => frame.Bank == EnemySpritemapDefinitions.RoomSpriteObjectBank)
            .Select(frame => frame.Pointer)
            .ToHashSet();
        for (int index = 0;
             index < RoomSpriteObjectInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = RoomSpriteObjectInstructionProgramDefinitions
                .PresentationWordAddress(index);
            ushort native = unchecked((ushort)(rom.ReadByte(0xb40000 | operand) |
                rom.ReadByte(0xb40000 | unchecked((ushort)(operand + 1))) << 8));
            AssertEqual(native, RoomSpriteObjectVisualDefinitions.FrameAt(operand),
                $"room sprite-object visual selector $B4:{operand:X4}");
            AssertTrue(installedSpritePointers.Contains(native),
                $"room sprite-object visual target $B4:{native:X4} has installed art");
        }
        AssertThrows<InvalidDataException>(
            () => RoomSpriteObjectVisualDefinitions.FrameAt(
                RoomSpriteObjectDefinitions.InstructionPointer(
                    RoomSpriteObjectKind.DraygonIntroEvir)),
            "room sprite-object mechanics are not presentation selectors");
        for (int index = 0; index < ElevatorInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = ElevatorInstructionProgramDefinitions
                .PresentationWordAddress(index);
            ushort native = unchecked((ushort)(rom.ReadByte(0xa30000 | operand) |
                rom.ReadByte(0xa30000 | unchecked((ushort)(operand + 1))) << 8));
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                    RoomEnemySystem.ElevatorDefinition, operand, out ushort selected),
                $"elevator visual selector $A3:{operand:X4} is installed");
            AssertEqual(native, selected,
                $"elevator visual selector $A3:{operand:X4} matches cartridge");
        }
        AssertThrows<InvalidDataException>(
            () => EnemySpritemapDefinitions.ElevatorFrameAt(
                ElevatorInstructionProgramDefinitions.Loop),
            "elevator rejects adjacent instruction mechanics as presentation");
        for (int index = 0; index < 4; index++)
        {
            ushort operand = VerticalShutterInstructionProgramDefinitions
                .PresentationWordAddress(index + 1);
            ushort native = unchecked((ushort)(rom.ReadByte(0xa20000 | operand) |
                rom.ReadByte(0xa20000 | unchecked((ushort)(operand + 1))) << 8));
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                    RoomEnemySystem.KamerVerticalPlatformDefinition,
                    operand, out ushort selected),
                $"Kamer visual selector $A2:{operand:X4} is installed");
            AssertEqual(native, selected,
                $"Kamer visual selector $A2:{operand:X4} matches cartridge");
        }
        AssertThrows<InvalidDataException>(
            () => EnemySpritemapDefinitions.KamerPlatformFrameAt(
                VerticalShutterInstructionProgramDefinitions.KamerPlatform),
            "Kamer rejects adjacent instruction mechanics as presentation");
        for (int index = 0;
             index < FuneNamiheInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort operand = FuneNamiheInstructionProgramDefinitions.PresentationWordAddress(index);
            ushort definition = operand < FuneNamiheInstructionProgramDefinitions.NamiheIdleLeft
                ? FuneNamiheDefinitions.FuneEnemyDefinition
                : FuneNamiheDefinitions.NamiheEnemyDefinition;
            ushort native = unchecked((ushort)(rom.ReadByte(0xa80000 | operand) |
                rom.ReadByte(0xa80000 | unchecked((ushort)(operand + 1))) << 8));
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                    definition, operand, out ushort selected),
                $"Fune/Namihe visual selector $A8:{operand:X4} is installed");
            AssertEqual(native, selected,
                $"Fune/Namihe visual selector $A8:{operand:X4} matches cartridge");
        }
        AssertThrows<InvalidDataException>(
            () => EnemySpritemapDefinitions.FuneNamiheFrameAt(
                FuneNamiheInstructionProgramDefinitions.FuneIdleRight),
            "Fune/Namihe rejects adjacent mechanics as presentation");
        for (int index = 0; index <
             SbugInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort operand = SbugInstructionProgramDefinitions
                .PresentationWordAddress(index);
            ushort native = unchecked((ushort)(rom.ReadByte(0xa30000 | operand) |
                rom.ReadByte(0xa30000 | unchecked((ushort)(operand + 1))) << 8));
            AssertEqual(native, EnemySpritemapDefinitions.SbugFrameAt(operand),
                $"Sbug visual selector $A3:{operand:X4} matches native");
        }
        AssertThrows<InvalidDataException>(
            () => EnemySpritemapDefinitions.SbugFrameAt(
                SbugInstructionProgramDefinitions.UpLeft),
            "Sbug rejects an adjacent mechanics operand as presentation");
        AssertTrue(!EnemySpritemapDefinitions.TryFrameAt(0xffff, 0xe312, out _),
            "unknown enemy family keeps the existing cartridge selector path");
        for (int index = 0; index < RioInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = RioInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                    RoomEnemySystem.RioDefinition, operand, out ushort frame),
                $"Rio presentation operand $A2:{operand:X4} uses the installed selector");
            AssertEqual(ReadRioInstructionWord(rom, operand), frame,
                $"Rio selector $A2:{operand:X4} matches the pinned cartridge");
        }
        AssertThrows<InvalidDataException>(
            () => EnemySpritemapDefinitions.RioFrameAt(
                RioInstructionProgramDefinitions.FirstAdjacentMechanicsData),
            "Rio rejects adjacent data as a presentation operand");
        var rioGuard = new RioInstructionReadGuard(rom, forbidPresentation: true);
        var installedRio = new RoomEnemySystem { TileArtwork = stock };
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(installedRio, rioGuard);
        RoomEnemySlot rioSlot = installedRio.Slots[0];
        rioSlot.EnemyDefinitionPointer = RoomEnemySystem.RioDefinition;
        rioSlot.Definition = default(RoomEnemyDefinition) with
            { Bank = EnemySpritemapDefinitions.RioBank };
        typeof(RoomEnemySystem).GetMethod("InitializeRio", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(installedRio)(rioSlot);
        MethodInfo rioProcess = typeof(RoomEnemySystem).GetMethod(
            "ProcessInstructions", flags)!;
        object?[] rioArguments =
            [rioSlot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        ExecuteRioProgram(installedRio, rioProcess, rioArguments, rioSlot,
            RioInstructionProgramDefinitions.Idle, callCount: 13);
        ExecuteRioProgram(installedRio, rioProcess, rioArguments, rioSlot,
            RioInstructionProgramDefinitions.SwoopingPart1, callCount: 6);
        ExecuteRioProgram(installedRio, rioProcess, rioArguments, rioSlot,
            RioInstructionProgramDefinitions.SwoopingPart2, callCount: 3);
        ExecuteRioProgram(installedRio, rioProcess, rioArguments, rioSlot,
            RioInstructionProgramDefinitions.SwoopCooldown, callCount: 6);
        AssertEqual(0, rioGuard.ObservedPresentationWords.Count,
            "installed Rio programs never read ROM visual selectors");
        for (int index = 0;
             index < LowerNorfairRioInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = LowerNorfairRioInstructionProgramDefinitions
                .PresentationWordAddress(index);
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                    RoomEnemySystem.LowerNorfairRioDefinition, operand, out ushort frame),
                $"Lower Norfair Rio presentation operand $A2:{operand:X4} is compiled");
            AssertEqual(ReadLowerNorfairRioInstructionWord(rom, operand), frame,
                $"Lower Norfair Rio selector $A2:{operand:X4} matches the cartridge");
        }
        AssertThrows<InvalidDataException>(
            () => EnemySpritemapDefinitions.LowerNorfairRioFrameAt(
                LowerNorfairRioInstructionProgramDefinitions.AdjacentMovementDefinitions),
            "Lower Norfair Rio rejects adjacent movement data as presentation");
        var lowerRioGuard = new LowerNorfairRioInstructionReadGuard(
            rom, forbidPresentation: true);
        MethodInfo lowerRioProcess = typeof(RoomEnemySystem).GetMethod(
            "ProcessInstructions", flags)!;
        foreach ((ushort entry, int calls) in new (ushort, int)[]
                 {
                     (LowerNorfairRioInstructionProgramDefinitions.Idle, 5),
                     (LowerNorfairRioInstructionProgramDefinitions.PrepareToSwoop, 10),
                     (LowerNorfairRioInstructionProgramDefinitions.Descending, 2),
                     (LowerNorfairRioInstructionProgramDefinitions.AscendingPart1, 4),
                     (LowerNorfairRioInstructionProgramDefinitions.AscendingPart2, 4),
                     (LowerNorfairRioInstructionProgramDefinitions.Cooldown, 10),
                     (LowerNorfairRioInstructionProgramDefinitions.Flames, 4),
                 })
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, _) =
                NewLowerNorfairRioInstructionSystem(lowerRioGuard, flags, entry, stock);
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            RunLowerNorfairRioInstructionFrames(
                lowerRioProcess, enemies, arguments, slot, calls);
        }
        AssertEqual(0, lowerRioGuard.ObservedPresentationWords.Count,
            "installed Lower Norfair Rio programs never read ROM visual selectors");
        for (int index = 0;
             index < NorfairRioInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = NorfairRioInstructionProgramDefinitions
                .PresentationWordAddress(index);
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                    RoomEnemySystem.NorfairRioDefinition, operand, out ushort frame),
                $"Norfair Rio presentation operand $A2:{operand:X4} is compiled");
            AssertEqual(ReadNorfairRioInstructionWord(rom, operand), frame,
                $"Norfair Rio selector $A2:{operand:X4} matches the cartridge");
        }
        AssertThrows<InvalidDataException>(
            () => EnemySpritemapDefinitions.NorfairRioFrameAt(
                NorfairRioInstructionProgramDefinitions.AdjacentMovementDefinitions),
            "Norfair Rio rejects adjacent movement data as presentation");
        var norfairRioGuard = new NorfairRioInstructionReadGuard(
            rom, forbidPresentation: true);
        MethodInfo norfairRioProcess = typeof(RoomEnemySystem).GetMethod(
            "ProcessInstructions", flags)!;
        foreach ((ushort entry, int calls) in new (ushort, int)[]
                 {
                     (NorfairRioInstructionProgramDefinitions.Idle, 5),
                     (NorfairRioInstructionProgramDefinitions.StartDescending, 7),
                     (NorfairRioInstructionProgramDefinitions.Descending, 5),
                     (NorfairRioInstructionProgramDefinitions.StartAscending, 9),
                     (NorfairRioInstructionProgramDefinitions.Ascending, 5),
                     (NorfairRioInstructionProgramDefinitions.FlamesAscending, 5),
                     (NorfairRioInstructionProgramDefinitions.FlamesDescending, 5),
                 })
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, _) =
                NewNorfairRioInstructionSystem(norfairRioGuard, flags, entry, stock);
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            RunNorfairRioInstructionFrames(
                norfairRioProcess, enemies, arguments, slot, calls);
        }
        AssertEqual(0, norfairRioGuard.ObservedPresentationWords.Count,
            "installed Norfair Rio programs never read ROM visual selectors");
        VerifyInstalledPuyoInstructionFrames(rom, stock, flags);
        VerifyInstalledBullInstructionFrames(rom, stock, flags);
        VerifyInstalledAlcoonInstructionFrames(rom, stock);
        VerifyInstalledMochtroidVisuals(rom, stock);
        VerifyInstalledBeetomInstructionFrames(rom, stock);
        VerifyInstalledHopperInstructionFrames(rom, stock);
        VerifyInstalledChootInstructionFrames(rom, stock);
        VerifyInstalledHZoomerInstructionFrames(rom, stock);
        VerifyInstalledSharedCrawlerFrames(rom, stock);
        for (int index = 0; index < SciserInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = SciserInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                    RoomEnemySystem.SciserDefinition, operand, out ushort selected),
                $"Sciser visual operand $A3:{operand:X4} is compiled");
            AssertEqual(ReadSciserInstructionWord(rom, operand), selected,
                $"Sciser selector $A3:{operand:X4} matches the pinned cartridge");
        }
        AssertThrows<InvalidDataException>(
            () => SciserVisualDefinitions.FrameAt(SciserInstructionProgramDefinitions.UpsideUp),
            "Sciser rejects adjacent mechanics as a visual selector");
        for (int index = 0; index < FlyInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = FlyInstructionProgramDefinitions.PresentationWordAddress(index);
            ushort expected = ReadFlyInstructionWord(rom, 0xa20000 | operand);
            foreach (ushort definition in new ushort[]
                     { RoomEnemySystem.MellowDefinition, RoomEnemySystem.MellaDefinition,
                       RoomEnemySystem.MemuDefinition })
            {
                AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                        definition, operand, out ushort selected),
                    $"fly ${definition:X4} visual operand $A2:{operand:X4} is compiled");
                AssertEqual(expected, selected,
                    $"fly ${definition:X4} selector $A2:{operand:X4} matches the cartridge");
            }
        }
        AssertThrows<InvalidDataException>(
            () => FlyVisualDefinitions.FrameAt(FlyInstructionProgramDefinitions.Flight),
            "fly family rejects adjacent mechanics as presentation");
        for (int index = 0; index < KagoInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = KagoInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                    RoomEnemySystem.KagoDefinition, operand, out ushort selected),
                $"Kago visual operand $A8:{operand:X4} is compiled");
            AssertEqual(ReadKagoInstructionWord(rom, 0xa80000 | operand), selected,
                $"Kago selector $A8:{operand:X4} matches the cartridge");
        }
        AssertThrows<InvalidDataException>(
            () => KagoVisualDefinitions.FrameAt(KagoInstructionProgramDefinitions.Slow),
            "Kago rejects adjacent mechanics as presentation");
        for (int index = 0;
             index < BlueBrinstarFaceBlockInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = BlueBrinstarFaceBlockInstructionProgramDefinitions
                .PresentationWordAddress(index);
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                    RoomEnemySystem.BlueBrinstarFaceBlockDefinition, operand,
                    out ushort selected),
                $"face-block visual operand $A8:{operand:X4} is compiled");
            AssertEqual(ReadBlueBrinstarFaceBlockWord(rom, 0xa80000 | operand), selected,
                $"face-block selector $A8:{operand:X4} matches the cartridge");
        }
        AssertThrows<InvalidDataException>(
            () => BlueBrinstarFaceBlockVisualDefinitions.FrameAt(
                BlueBrinstarFaceBlockInstructionProgramDefinitions.Initial),
            "face block rejects adjacent timing as presentation");
        for (int index = 0;
             index < MorphBallEyeInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = MorphBallEyeInstructionProgramDefinitions
                .PresentationWordAddress(index);
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                    RoomEnemySystem.MorphBallEyeDefinition, operand,
                    out ushort selected),
                $"Morph Ball eye visual operand $A8:{operand:X4} is compiled");
            AssertEqual(ReadMorphBallEyeInstructionWord(rom, operand), selected,
                $"Morph Ball eye selector $A8:{operand:X4} matches the cartridge");
        }
        AssertThrows<InvalidDataException>(
            () => MorphBallEyeVisualDefinitions.FrameAt(
                MorphBallEyeInstructionProgramDefinitions.AdjacentProximityDefinitions),
            "eye visual selector rejects adjacent proximity definitions");
        for (int index = 0;
             index < GrowingShutterInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = GrowingShutterInstructionProgramDefinitions
                .PresentationWordAddress(index);
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                    RoomEnemySystem.GrowingShutterDefinition, operand,
                    out ushort selected),
                $"growing-shutter visual operand $A2:{operand:X4} is compiled");
            AssertEqual(ReadGrowingShutterInstructionWord(rom, 0xa20000 | operand),
                selected, $"growing-shutter selector $A2:{operand:X4} matches the cartridge");
        }
        ushort verticalOperand =
            VerticalShutterInstructionProgramDefinitions.PresentationWordAddress(0);
        foreach (ushort definition in new ushort[]
                 { RoomEnemySystem.ShootableVerticalShutterDefinition,
                   RoomEnemySystem.DestroyableVerticalShutterDefinition })
        {
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(definition,
                    verticalOperand, out ushort selected),
                $"vertical shutter ${definition:X4} selector is compiled");
            AssertEqual(ReadVerticalShutterInstructionWord(rom,
                    0xa20000 | verticalOperand), selected,
                $"vertical shutter ${definition:X4} selector matches the cartridge");
        }
        ushort horizontalOperand =
            HorizontalShutterInstructionProgramDefinitions.PresentationWordAddress(0);
        AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                RoomEnemySystem.ShootableHorizontalShutterDefinition,
                horizontalOperand, out ushort horizontalSelected),
            "horizontal-shutter visual selector is compiled");
        AssertEqual(ReadHorizontalShutterInstructionWord(rom,
                0xa20000 | horizontalOperand), horizontalSelected,
            "horizontal-shutter selector matches the cartridge");
        AssertThrows<InvalidDataException>(
            () => ShutterVisualDefinitions.FrameAt(
                RoomEnemySystem.GrowingShutterDefinition,
                GrowingShutterInstructionProgramDefinitions.ProgramEntryPoint(0)),
            "shutter visual selector rejects adjacent frame timing");
        for (int index = 0;
             index < MetroidInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = MetroidInstructionProgramDefinitions
                .PresentationWordAddress(index);
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                    RoomEnemySystem.MetroidDefinition, operand, out ushort selected),
                $"ordinary-Metroid visual operand $A3:{operand:X4} is compiled");
            AssertEqual(ReadMetroidInstructionWord(rom, operand), selected,
                $"ordinary-Metroid selector $A3:{operand:X4} matches the cartridge");
        }
        AssertThrows<InvalidDataException>(
            () => MetroidVisualDefinitions.FrameAt(
                MetroidInstructionProgramDefinitions.AdjacentBombedOffVelocities),
            "Metroid visual selector rejects neighboring bomb-off physics data");
        HashSet<ushort> shaktoolFrames = ShaktoolVisualDefinitions.Frames()
            .Select(frame => frame.Pointer).ToHashSet();
        AssertEqual(15, shaktoolFrames.Count,
            "Shaktool's visual frame identities are distinct");
        for (int index = 0;
             index < ShaktoolInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = ShaktoolInstructionProgramDefinitions
                .PresentationWordAddress(index);
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                    RoomEnemySystem.ShaktoolDefinition, operand, out ushort selected),
                $"Shaktool visual operand $AA:{operand:X4} is compiled");
            ushort native = unchecked((ushort)(rom.ReadByte(0xaa0000 | operand) |
                rom.ReadByte(0xaa0000 | unchecked((ushort)(operand + 1))) << 8));
            AssertEqual(native, selected,
                $"Shaktool visual selector $AA:{operand:X4} matches the cartridge");
            AssertTrue(shaktoolFrames.Remove(selected),
                $"Shaktool visual selector $AA:{operand:X4} has unique installed art");
        }
        AssertEqual(0, shaktoolFrames.Count,
            "every installed Shaktool frame is selected by a native program");
        AssertThrows<InvalidDataException>(
            () => ShaktoolVisualDefinitions.FrameAt(
                ShaktoolInstructionProgramDefinitions.HeadAimingLeft),
            "Shaktool visual selector rejects neighboring mechanics data");
        HashSet<ushort> platformFramePointers = TripperKamerVisualDefinitions.Frames()
            .Select(frame => frame.Pointer).ToHashSet();
        AssertEqual(18, platformFramePointers.Count,
            "Tripper/Kamer visual frame identities are distinct");
        var selectedPlatformFrames = new HashSet<ushort>();
        for (int index = 0;
             index < PlatformInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = PlatformInstructionProgramDefinitions
                .PresentationWordAddress(index);
            ushort native = unchecked((ushort)(rom.ReadByte(0xa30000 | operand) |
                rom.ReadByte(0xa30000 | unchecked((ushort)(operand + 1))) << 8));
            foreach (ushort definition in new[]
                     { RoomEnemySystem.TripperDefinition, RoomEnemySystem.KamerDefinition })
            {
                AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                        definition, operand, out ushort selected),
                    $"platform ${definition:X4} visual operand $A3:{operand:X4} is compiled");
                AssertEqual(native, selected,
                    $"platform ${definition:X4} selector $A3:{operand:X4} matches cartridge");
            }
            AssertTrue(platformFramePointers.Contains(native),
                $"platform selector $A3:{operand:X4} has installed art");
            selectedPlatformFrames.Add(native);
        }
        AssertEqual(16, selectedPlatformFrames.Count,
            "platform programs select sixteen distinct animated compositions");
        AssertTrue(platformFramePointers.Except(selectedPlatformFrames).ToHashSet()
                .SetEquals(new[]
                {
                    RoomEnemySystem.TripperFrozenMovingLeftSpritemap,
                    RoomEnemySystem.TripperFrozenMovingRightSpritemap,
                }),
            "only Tripper's two direct shot-AI frozen compositions lack animation selectors");
        AssertThrows<InvalidDataException>(
            () => TripperKamerVisualDefinitions.FrameAt(
                PlatformInstructionProgramDefinitions.FirstAdjacentCallback),
            "platform visual selector rejects neighboring movement callback");
        HashSet<ushort> dragonFrames = DragonVisualDefinitions.Frames()
            .Select(frame => frame.Pointer).ToHashSet();
        AssertEqual(12, dragonFrames.Count,
            "Dragon has twelve distinct installed body and wing frames");
        for (int index = 0;
             index < DragonInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = DragonInstructionProgramDefinitions
                .PresentationWordAddress(index);
            ushort native = unchecked((ushort)(rom.ReadByte(0xa20000 | operand) |
                rom.ReadByte(0xa20000 | unchecked((ushort)(operand + 1))) << 8));
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                    RoomEnemySystem.DragonDefinition, operand, out ushort selected),
                $"Dragon visual selector $A2:{operand:X4} is installed");
            AssertEqual(native, selected,
                $"Dragon visual selector $A2:{operand:X4} matches the cartridge");
            AssertTrue(dragonFrames.Remove(selected) ||
                DragonVisualDefinitions.Frames().Any(frame => frame.Pointer == selected),
                $"Dragon visual target $A2:{selected:X4} has installed art");
        }
        AssertEqual(0, dragonFrames.Count,
            "every installed Dragon body and wing frame is selected");
        AssertThrows<InvalidDataException>(
            () => DragonVisualDefinitions.FrameAt(
                DragonInstructionProgramDefinitions.AttackFinishedCallback),
            "Dragon visual selector rejects neighboring attack callback");
        HashSet<ushort> multiviolaFrames = MultiviolaVisualDefinitions.Frames()
            .Select(frame => frame.Pointer).ToHashSet();
        AssertEqual(MultiviolaVisualDefinitions.FrameCount, multiviolaFrames.Count,
            "Multiviola's fourteen-frame loop selects eight distinct OAM frames");
        var selectedMultiviolaFrames = new HashSet<ushort>();
        for (int index = 0;
             index < MultiviolaInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = MultiviolaInstructionProgramDefinitions
                .PresentationWordAddress(index);
            ushort native = unchecked((ushort)(rom.ReadByte(0xa20000 | operand) |
                rom.ReadByte(0xa20000 | unchecked((ushort)(operand + 1))) << 8));
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                    RoomEnemySystem.MultiviolaDefinition, operand, out ushort selected),
                $"Multiviola visual selector $A2:{operand:X4} is installed");
            AssertEqual(native, selected,
                $"Multiviola visual selector $A2:{operand:X4} matches the cartridge");
            AssertTrue(multiviolaFrames.Contains(selected),
                $"Multiviola visual target $A2:{selected:X4} has installed art");
            selectedMultiviolaFrames.Add(selected);
        }
        AssertTrue(selectedMultiviolaFrames.SetEquals(multiviolaFrames),
            "every installed Multiviola composition is selected by its loop");
        AssertThrows<InvalidDataException>(
            () => MultiviolaVisualDefinitions.FrameAt(
                MultiviolaInstructionProgramDefinitions.Flying),
            "Multiviola visual selector rejects its neighboring duration word");
        HashSet<ushort> lavaJumperFrames = NorfairLavaJumperVisualDefinitions.Frames()
            .Select(frame => frame.Pointer).ToHashSet();
        AssertEqual(NorfairLavaJumperVisualDefinitions.FrameCount,
            lavaJumperFrames.Count,
            "Norfair lava jumper has eleven distinct installed identities");
        AssertTrue(lavaJumperFrames.Contains(CommonEnemyEmptyExtendedFrameDefinitions.EmptySpritemap),
            "Norfair lava jumper retains its native bank-local empty frame");
        var selectedLavaJumperFrames = new HashSet<ushort>();
        for (int index = 0;
             index < NorfairLavaJumperInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = NorfairLavaJumperInstructionProgramDefinitions
                .PresentationWordAddress(index);
            ushort native = unchecked((ushort)(rom.ReadByte(0xa20000 | operand) |
                rom.ReadByte(0xa20000 | unchecked((ushort)(operand + 1))) << 8));
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                    RoomEnemySystem.NorfairLavaJumpingEnemyDefinition,
                    operand, out ushort selected),
                $"lava-jumper visual selector $A2:{operand:X4} is installed");
            AssertEqual(native, selected,
                $"lava-jumper visual selector $A2:{operand:X4} matches the cartridge");
            AssertTrue(lavaJumperFrames.Contains(selected),
                $"lava-jumper visual target $A2:{selected:X4} has installed art");
            selectedLavaJumperFrames.Add(selected);
        }
        AssertTrue(selectedLavaJumperFrames.SetEquals(lavaJumperFrames),
            "every installed lava-jumper frame is selected by a native program");
        AssertThrows<InvalidDataException>(
            () => NorfairLavaJumperVisualDefinitions.FrameAt(
                NorfairLavaJumperInstructionProgramDefinitions.Hidden),
            "lava-jumper visual selector rejects a neighboring duration word");
        HashSet<ushort> chozoFrames = ChozoStatueVisualDefinitions.Frames()
            .Select(frame => frame.Pointer).ToHashSet();
        AssertEqual(ChozoStatueVisualDefinitions.FrameCount, chozoFrames.Count,
            "the two Chozo sequences select twenty-six distinct installed frames");
        var selectedChozoFrames = new HashSet<ushort>();
        for (int index = 0;
             index < ChozoStatueInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = ChozoStatueInstructionProgramDefinitions
                .PresentationWordAddress(index);
            ushort native = unchecked((ushort)(rom.ReadByte(0xaa0000 | operand) |
                rom.ReadByte(0xaa0000 | unchecked((ushort)(operand + 1))) << 8));
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                    ChozoStatueEnemyDefinitions.EnemyDefinitionPointer,
                    operand, out ushort selected),
                $"Chozo statue visual selector $AA:{operand:X4} is installed");
            AssertEqual(native, selected,
                $"Chozo statue visual selector $AA:{operand:X4} matches the cartridge");
            AssertTrue(chozoFrames.Contains(selected),
                $"Chozo statue visual target $AA:{selected:X4} has installed art");
            selectedChozoFrames.Add(selected);
        }
        AssertTrue(selectedChozoFrames.SetEquals(chozoFrames),
            "every installed Chozo composition is selected by a native program");
        AssertThrows<InvalidDataException>(
            () => ChozoStatueVisualDefinitions.FrameAt(
                ChozoStatueInstructionProgramDefinitions.LowerNorfairInitial),
            "Chozo visual selector rejects neighboring mechanics data");
        HashSet<ushort> violaFrames = ViolaVisualDefinitions.Frames()
            .Select(frame => frame.Pointer).ToHashSet();
        AssertEqual(ViolaVisualDefinitions.FrameCount, violaFrames.Count,
            "Viola's fourteen-frame loop selects eight distinct OAM frames");
        var selectedViolaFrames = new HashSet<ushort>();
        for (int index = 0; index < ViolaInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = ViolaInstructionProgramDefinitions.PresentationWordAddress(index);
            ushort native = unchecked((ushort)(rom.ReadByte(0xa30000 | operand) |
                rom.ReadByte(0xa30000 | unchecked((ushort)(operand + 1))) << 8));
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                    RoomEnemySystem.ViolaDefinition, operand, out ushort selected),
                $"Viola visual selector $A3:{operand:X4} is installed");
            AssertEqual(native, selected,
                $"Viola visual selector $A3:{operand:X4} matches the cartridge");
            AssertTrue(violaFrames.Contains(selected),
                $"Viola visual target $A3:{selected:X4} has installed art");
            selectedViolaFrames.Add(selected);
        }
        AssertTrue(selectedViolaFrames.SetEquals(violaFrames),
            "every installed Viola composition is selected by its native loop");
        AssertThrows<InvalidDataException>(
            () => ViolaVisualDefinitions.FrameAt(ViolaInstructionProgramDefinitions.NormalLoop),
            "Viola visual selector rejects neighboring mechanics data");
        HashSet<ushort> rinkaFrames = RinkaVisualDefinitions.Frames()
            .Select(frame => frame.Pointer).ToHashSet();
        AssertEqual(RinkaVisualDefinitions.FrameCount, rinkaFrames.Count,
            "both Rinka loops select five distinct OAM frames");
        var selectedRinkaFrames = new HashSet<ushort>();
        for (int index = 0; index < RinkaInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = RinkaInstructionProgramDefinitions.PresentationWordAddress(index);
            ushort native = unchecked((ushort)(rom.ReadByte(0xa20000 | operand) |
                rom.ReadByte(0xa20000 | unchecked((ushort)(operand + 1))) << 8));
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                    RoomEnemySystem.RinkaDefinition, operand, out ushort selected),
                $"Rinka visual selector $A2:{operand:X4} is installed");
            AssertEqual(native, selected,
                $"Rinka visual selector $A2:{operand:X4} matches the cartridge");
            AssertTrue(rinkaFrames.Contains(selected),
                $"Rinka visual target $A2:{selected:X4} has installed art");
            selectedRinkaFrames.Add(selected);
        }
        AssertTrue(selectedRinkaFrames.SetEquals(rinkaFrames),
            "every installed Rinka composition is selected by a native loop");
        AssertThrows<InvalidDataException>(
            () => RinkaVisualDefinitions.FrameAt(RinkaInstructionProgramDefinitions.OrdinaryInitial),
            "Rinka visual selector rejects neighboring mechanics data");
        foreach (EnemySpritemapDefinition frame in EnemySpritemapDefinitions.Frames)
        {
            AssertTrue(stock.Spritemaps!.TryGet(frame.Bank, frame.Pointer, out var parts),
                $"installed enemy frame {frame.Name} exists");
            AssertTrue(stock.Spritemaps.TryGetDisplay(frame.Bank, frame.Pointer, out _),
                $"installed enemy frame {frame.Name} has a display binding");
            foreach ((ushort x, ushort y, ushort palette, ushort baseTile) in
                     new (ushort, ushort, ushort, ushort)[]
                     {
                         (0x0010, 0x0020, 0, 0),
                         (0x01f8, 0x00fc, 0x0c00, 0x01ff),
                         (0x0000, 0x0000, 0x0400, 0x0010),
                     })
            {
                var native = new OamBuffer();
                var installed = new OamBuffer();
                native.AddEnemySpritemap(rom, frame.Bank, frame.Pointer,
                    x, y, palette, baseTile);
                installed.AddEnemySpritemap(parts.Span, x, y, palette, baseTile);
                AssertTrue(native.LowTable.SequenceEqual(installed.LowTable) &&
                           native.HighTable.SequenceEqual(installed.HighTable) &&
                           native.NextByteOffset == installed.NextByteOffset,
                    $"installed {frame.Name} OAM matches native at {x:X4},{y:X4}");
            }
            // Bank-$B4 frames belong to the room sprite-object renderer, not an
            // enemy slot whose definition selects an $A0-$B3 spritemap bank.
            if (frame.Bank == EnemySpritemapDefinitions.RoomSpriteObjectBank)
                continue;
            // The head/neck use a private world-space hook, and the falling tubes
            // use Mother Brain's encounter route. Test that route separately below.
            if (frame.Bank == MotherBrainVisualDefinitions.Bank)
                continue;
            if (frame.Name.StartsWith("ridley_supplement_", StringComparison.Ordinal))
                continue;
            var room = DrawEnemy(stock, new FrameReadGuard(rom, frame.Name), frame.Pointer,
                frame.Name.StartsWith("boyon_", StringComparison.Ordinal)
                    ? RoomEnemySystem.BoyonDefinition
                    : frame.Name.StartsWith("mochtroid_", StringComparison.Ordinal)
                        ? EnemyDefinitionPointers.Mochtroid
                    : frame.Name.StartsWith("sciser_", StringComparison.Ordinal)
                        ? RoomEnemySystem.SciserDefinition
                    : frame.Name.StartsWith("fly_shared_", StringComparison.Ordinal)
                        ? RoomEnemySystem.MellowDefinition
                    : frame.Name.StartsWith("kago_", StringComparison.Ordinal)
                        ? RoomEnemySystem.KagoDefinition
                    : frame.Name.StartsWith("face_block_", StringComparison.Ordinal)
                        ? RoomEnemySystem.BlueBrinstarFaceBlockDefinition
                    : frame.Name.StartsWith("morph_eye_", StringComparison.Ordinal)
                        ? RoomEnemySystem.MorphBallEyeDefinition
                    : frame.Name.StartsWith("metroid_body_", StringComparison.Ordinal)
                        ? RoomEnemySystem.MetroidDefinition
                    : frame.Name.StartsWith("shaktool_", StringComparison.Ordinal)
                        ? RoomEnemySystem.ShaktoolDefinition
                    : frame.Name.StartsWith("tripper_kamer_", StringComparison.Ordinal)
                        ? RoomEnemySystem.KamerDefinition
                    : frame.Name.StartsWith("tripper_", StringComparison.Ordinal)
                        ? RoomEnemySystem.TripperDefinition
                    : frame.Name.StartsWith("dragon_", StringComparison.Ordinal)
                        ? RoomEnemySystem.DragonDefinition
                    : frame.Name.StartsWith("multiviola_", StringComparison.Ordinal)
                        ? RoomEnemySystem.MultiviolaDefinition
                    : frame.Name.StartsWith("norfair_lava_jumper_", StringComparison.Ordinal)
                        ? RoomEnemySystem.NorfairLavaJumpingEnemyDefinition
                    : frame.Name.StartsWith("chozo_statue_aa_", StringComparison.Ordinal)
                        ? ChozoStatueEnemyDefinitions.EnemyDefinitionPointer
                    : frame.Name.StartsWith("viola_spin_", StringComparison.Ordinal)
                        ? RoomEnemySystem.ViolaDefinition
                    : frame.Name.StartsWith("rinka_spin_", StringComparison.Ordinal)
                        ? RoomEnemySystem.RinkaDefinition
                    : frame.Name.StartsWith("shutter_growing_", StringComparison.Ordinal)
                        ? RoomEnemySystem.GrowingShutterDefinition
                    : frame.Name.StartsWith("shutter_vertical_", StringComparison.Ordinal)
                        ? RoomEnemySystem.ShootableVerticalShutterDefinition
                    : frame.Name.StartsWith("shutter_horizontal", StringComparison.Ordinal)
                        ? RoomEnemySystem.ShootableHorizontalShutterDefinition
                    : frame.Name.StartsWith("norfair_rio_", StringComparison.Ordinal)
                        ? RoomEnemySystem.NorfairRioDefinition
                    : frame.Name.StartsWith("lower_norfair_rio_", StringComparison.Ordinal)
                        ? RoomEnemySystem.LowerNorfairRioDefinition
                    : frame.Name.StartsWith("rio_", StringComparison.Ordinal)
                        ? RoomEnemySystem.RioDefinition
                    : frame.Name.StartsWith("puyo_", StringComparison.Ordinal)
                        ? RoomEnemySystem.PuyoDefinition
                    : frame.Name.StartsWith("bull_", StringComparison.Ordinal)
                        ? RoomEnemySystem.BullDefinition
                    : frame.Name.StartsWith("alcoon_", StringComparison.Ordinal)
                        ? RoomEnemySystem.AlcoonDefinition
                    : frame.Name.StartsWith("beetom_", StringComparison.Ordinal)
                        ? RoomEnemySystem.BeetomDefinition
                    : frame.Name.StartsWith("large_sidehopper_", StringComparison.Ordinal)
                        ? RoomEnemySystem.LargeSidehopperDefinition
                    : frame.Name.StartsWith("sidehopper_", StringComparison.Ordinal)
                        ? RoomEnemySystem.SidehopperDefinition
                    : frame.Name.StartsWith("large_dessgeega_", StringComparison.Ordinal)
                        ? RoomEnemySystem.LargeDessgeegaDefinition
                    : frame.Name.StartsWith("dessgeega_", StringComparison.Ordinal)
                        ? RoomEnemySystem.DessgeegaDefinition
                    : frame.Name.StartsWith("choot_", StringComparison.Ordinal)
                        ? RoomEnemySystem.ChootDefinition
                    : frame.Name.StartsWith("hzoomer_", StringComparison.Ordinal)
                        ? RoomEnemySystem.HZoomerDefinition
                    : frame.Name.StartsWith("sbug_", StringComparison.Ordinal)
                        ? RoomEnemySystem.SbugDefinition
                    : frame.Name.StartsWith("fune_", StringComparison.Ordinal)
                        ? FuneNamiheDefinitions.FuneEnemyDefinition
                    : frame.Name.StartsWith("namihe_", StringComparison.Ordinal)
                        ? FuneNamiheDefinitions.NamiheEnemyDefinition
                    : frame.Name.StartsWith("kamer_platform_", StringComparison.Ordinal)
                        ? RoomEnemySystem.KamerVerticalPlatformDefinition
                    : frame.Name.StartsWith("elevator_platform_", StringComparison.Ordinal)
                        ? RoomEnemySystem.ElevatorDefinition
                    : frame.Name.StartsWith("cacatac_", StringComparison.Ordinal)
                        ? RoomEnemySystem.CacatacDefinition
                        : frame.Name.StartsWith("boulder_", StringComparison.Ordinal)
                            ? RoomEnemySystem.BoulderDefinition
                            : frame.Name.StartsWith("skultera_", StringComparison.Ordinal)
                                ? RoomEnemySystem.SkulteraDefinition
                                : frame.Name.StartsWith("waver_", StringComparison.Ordinal)
                                    ? RoomEnemySystem.WaverDefinition
                                    : frame.Name.StartsWith("zoa_", StringComparison.Ordinal)
                                        ? RoomEnemySystem.ZoaDefinition
                                        : frame.Name.StartsWith("metaree_", StringComparison.Ordinal)
                                            ? RoomEnemySystem.MetareeDefinition
                                            : frame.Name.StartsWith("skree_", StringComparison.Ordinal)
                                                ? RoomEnemySystem.SkreeDefinition
                                                : frame.Name.StartsWith("pipe_brinstar_strong_", StringComparison.Ordinal)
                                                    ? PipeBugDefinitions.StrongBrinstarEnemyDefinition
                                                    : frame.Name.StartsWith("pipe_brinstar_", StringComparison.Ordinal)
                                                        ? PipeBugDefinitions.BrinstarEnemyDefinition
                                                        : frame.Name.StartsWith("pipe_norfair_", StringComparison.Ordinal)
                                                            ? PipeBugDefinitions.NorfairEnemyDefinition
                                                            : frame.Name.StartsWith("pipe_yellow_", StringComparison.Ordinal)
                                                                ? PipeBugDefinitions.YellowEnemyDefinition
                                                                : frame.Name.StartsWith("fake_kraid_", StringComparison.Ordinal)
                                                                    ? RoomEnemySystem.FakeKraidDefinition
                                                                : frame.Name.StartsWith("kraid_nail_", StringComparison.Ordinal)
                                                                        ? RoomEnemySystem.KraidGoodNailDefinition
                                                                        : frame.Name.StartsWith("owtch_", StringComparison.Ordinal)
                                                                            ? RoomEnemySystem.OwtchDefinition
                                                                            : frame.Name.StartsWith("stoke_", StringComparison.Ordinal)
                                                                                ? RoomEnemySystem.StokeDefinition
                                                                                : frame.Name.StartsWith("ripper_shared_", StringComparison.Ordinal)
                                                                                    ? RoomEnemySystem.GRipperDefinition
                                                                                    : frame.Name.StartsWith("ripper_", StringComparison.Ordinal)
                                                                                        ? RoomEnemySystem.RipperDefinition
                                                                                        : frame.Name.StartsWith("fireflea_", StringComparison.Ordinal)
                                                                                            ? RoomEnemySystem.FirefleaDefinition
                                                                : frame.Name.StartsWith("ceres_door_", StringComparison.Ordinal) ||
                                                                  frame.Name.StartsWith("ceres_baby_", StringComparison.Ordinal)
                                                                    ? CeresDoorInstructionProgramDefinitions.EnemyDefinitionPointer
                                                                : frame.Name.StartsWith("magdollite_", StringComparison.Ordinal)
                                                                                                ? RoomEnemySystem.MagdolliteDefinition
                                : RoomEnemySystem.AtomicDefinition);
            var nativeRoom = new OamBuffer();
            nativeRoom.AddEnemySpritemap(rom, frame.Bank, frame.Pointer,
                0x0040, 0x0080, 0, 0);
            AssertTrue(room.LowTable.SequenceEqual(nativeRoom.LowTable) &&
                       room.HighTable.SequenceEqual(nativeRoom.HighTable),
                $"production room draws installed {frame.Name} without visual ROM reads");
        }

        VerifyInstalledMotherBrainDrawHook(rom, stock);
        VerifyInstalledRidleySupplementalArtwork(rom, stock);

        string fileName = EnemySpritemapDefinitions.FileName;
        string stockPath = Path.Combine(stockDirectory, fileName);
        byte[] original = File.ReadAllBytes(stockPath);
        File.WriteAllBytes(stockPath, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, null),
            "stock enemy compositions are manifest-hash checked");
        File.WriteAllBytes(stockPath, original);

        EnemySpritemapDocument document = JsonSerializer.Deserialize<EnemySpritemapDocument>(
            original, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        HashSet<string> mochtroidNames = MochtroidVisualDefinitions.Frames()
            .Select(frame => frame.Name).ToHashSet(StringComparer.Ordinal);
        var preMochtroidFrames = document.Frames
            .Where(pair => !mochtroidNames.Contains(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        var preMochtroidBindings = document.DisplayFrames!
            .Where(pair => preMochtroidFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreMochtroidFrameCount,
            preMochtroidFrames.Count, "version-fifty-five composition schema count");
        using (var preMochtroidJson = new MemoryStream(
            JsonSerializer.SerializeToUtf8Bytes(new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreMochtroidVersion,
                Frames = preMochtroidFrames,
                DisplayFrames = preMochtroidBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })))
        {
            EnemySpritemapCatalog mochtroidUpgraded = EnemySpritemapCatalog.Load(
                preMochtroidJson, stock.Spritemaps);
            foreach (EnemySpritemapDefinition frame in MochtroidVisualDefinitions.Frames())
                AssertTrue(mochtroidUpgraded.TryGetDisplay(frame.Bank, frame.Pointer, out _),
                    $"version-fifty-five override inherits {frame.Name}");
        }
        HashSet<string> deadCorpseNames = DeadTourianCorpseVisualDefinitions.Frames()
            .Select(frame => frame.Name).ToHashSet(StringComparer.Ordinal);
        var preCorpsesFrames = preMochtroidFrames
            .Where(pair => !deadCorpseNames.Contains(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        var preCorpsesBindings = preMochtroidBindings
            .Where(pair => preCorpsesFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreDeadTourianCorpseFrameCount,
            preCorpsesFrames.Count, "version-fifty-four composition schema count");
        using (var preCorpsesJson = new MemoryStream(
            JsonSerializer.SerializeToUtf8Bytes(new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreDeadTourianCorpseVersion,
                Frames = preCorpsesFrames,
                DisplayFrames = preCorpsesBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })))
        {
            EnemySpritemapCatalog corpsesUpgraded = EnemySpritemapCatalog.Load(
                preCorpsesJson, stock.Spritemaps);
            foreach (EnemySpritemapDefinition frame in
                     DeadTourianCorpseVisualDefinitions.Frames())
                AssertTrue(corpsesUpgraded.TryGetDisplay(frame.Bank, frame.Pointer, out _),
                    $"version-fifty-four override inherits {frame.Name}");
        }
        var preStationaryFrames = preCorpsesFrames
            .Where(pair => pair.Key != "dead_torizo_stationary_a9_d6e2")
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        var preStationaryBindings = preCorpsesBindings
            .Where(pair => preStationaryFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreDeadTorizoStationaryFrameCount,
            preStationaryFrames.Count, "version-fifty-three composition schema count");
        using (var preDeadTorizoJson = new MemoryStream(
            JsonSerializer.SerializeToUtf8Bytes(new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreDeadTorizoStationaryVersion,
                Frames = preStationaryFrames,
                DisplayFrames = preStationaryBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })))
        {
            EnemySpritemapCatalog stationaryUpgraded = EnemySpritemapCatalog.Load(
                preDeadTorizoJson, stock.Spritemaps);
            AssertTrue(stationaryUpgraded.TryGetDisplay(
                    DeadTorizoArtworkDefinitions.SpritemapBank,
                    DeadTorizoArtworkDefinitions.StationarySpritemap, out _),
                "version-fifty-three override inherits Dead Torizo stationary OAM");
        }
        var preRinkaFrames = preStationaryFrames
            .Where(pair => !pair.Key.StartsWith("rinka_spin_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        var preRinkaBindings = preStationaryBindings
            .Where(pair => preRinkaFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreRinkaFrameCount, preRinkaFrames.Count,
            "version-fifty-two composition schema count");
        using (var preRinkaJson = new MemoryStream(
            JsonSerializer.SerializeToUtf8Bytes(new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreRinkaVersion,
                Frames = preRinkaFrames,
                DisplayFrames = preRinkaBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })))
        {
            EnemySpritemapCatalog rinkaUpgraded = EnemySpritemapCatalog.Load(
                preRinkaJson, stock.Spritemaps);
            foreach (EnemySpritemapDefinition frame in RinkaVisualDefinitions.Frames())
                AssertTrue(rinkaUpgraded.TryGetDisplay(frame.Bank, frame.Pointer, out _),
                    $"version-fifty-two override inherits Rinka frame {frame.Name}");
        }
        var preViolaFrames = preRinkaFrames
            .Where(pair => !pair.Key.StartsWith("viola_spin_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        var preViolaBindings = document.DisplayFrames!
            .Where(pair => preViolaFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreViolaFrameCount, preViolaFrames.Count,
            "version-fifty-one composition schema count");
        using (var preViolaJson = new MemoryStream(
            JsonSerializer.SerializeToUtf8Bytes(new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreViolaVersion,
                Frames = preViolaFrames,
                DisplayFrames = preViolaBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })))
        {
            EnemySpritemapCatalog violaUpgraded = EnemySpritemapCatalog.Load(
                preViolaJson, stock.Spritemaps);
            foreach (EnemySpritemapDefinition frame in ViolaVisualDefinitions.Frames())
                AssertTrue(violaUpgraded.TryGetDisplay(frame.Bank, frame.Pointer, out _),
                    $"version-fifty-one override inherits Viola frame {frame.Name}");
        }
        var preChozoStatueFrames = preViolaFrames
            .Where(pair => !pair.Key.StartsWith("chozo_statue_aa_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        var preChozoStatueBindings = document.DisplayFrames!
            .Where(pair => preChozoStatueFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreChozoStatueFrameCount,
            preChozoStatueFrames.Count,
            "version-fifty composition schema count");
        using (var preChozoStatueJson = new MemoryStream(
            JsonSerializer.SerializeToUtf8Bytes(new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreChozoStatueVersion,
                Frames = preChozoStatueFrames,
                DisplayFrames = preChozoStatueBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })))
        {
            EnemySpritemapCatalog chozoUpgraded = EnemySpritemapCatalog.Load(
                preChozoStatueJson, stock.Spritemaps);
            foreach (EnemySpritemapDefinition frame in ChozoStatueVisualDefinitions.Frames())
                AssertTrue(chozoUpgraded.TryGetDisplay(frame.Bank, frame.Pointer, out _),
                    $"version-fifty override inherits Chozo frame {frame.Name}");
        }
        var preNorfairLavaJumperFrames = preChozoStatueFrames
            .Where(pair => !pair.Key.StartsWith("norfair_lava_jumper_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        var preNorfairLavaJumperBindings = document.DisplayFrames!
            .Where(pair => preNorfairLavaJumperFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreNorfairLavaJumperFrameCount,
            preNorfairLavaJumperFrames.Count,
            "version-forty-nine composition schema count");
        using (var preNorfairLavaJumperJson = new MemoryStream(
            JsonSerializer.SerializeToUtf8Bytes(new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreNorfairLavaJumperVersion,
                Frames = preNorfairLavaJumperFrames,
                DisplayFrames = preNorfairLavaJumperBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })))
        {
            EnemySpritemapCatalog lavaJumperUpgraded = EnemySpritemapCatalog.Load(
                preNorfairLavaJumperJson, stock.Spritemaps);
            foreach (EnemySpritemapDefinition frame in NorfairLavaJumperVisualDefinitions.Frames())
                AssertTrue(lavaJumperUpgraded.TryGetDisplay(frame.Bank, frame.Pointer, out _),
                    $"version-forty-nine override inherits lava-jumper frame {frame.Name}");
        }
        var preMultiviolaFrames = preNorfairLavaJumperFrames
            .Where(pair => !pair.Key.StartsWith("multiviola_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        var preMultiviolaBindings = document.DisplayFrames!
            .Where(pair => preMultiviolaFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreMultiviolaFrameCount,
            preMultiviolaFrames.Count, "version-forty-eight composition schema count");
        using (var preMultiviolaJson = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreMultiviolaVersion,
                Frames = preMultiviolaFrames,
                DisplayFrames = preMultiviolaBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })))
        {
            EnemySpritemapCatalog multiviolaUpgraded = EnemySpritemapCatalog.Load(
                preMultiviolaJson, stock.Spritemaps);
            foreach (EnemySpritemapDefinition frame in MultiviolaVisualDefinitions.Frames())
                AssertTrue(multiviolaUpgraded.TryGetDisplay(frame.Bank, frame.Pointer, out _),
                    $"version-forty-eight override inherits Multiviola frame {frame.Name}");
        }
        var preDragonFrames = preMultiviolaFrames
            .Where(pair => !pair.Key.StartsWith("dragon_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        var preDragonBindings = document.DisplayFrames!
            .Where(pair => preDragonFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreDragonFrameCount,
            preDragonFrames.Count, "version-forty-seven composition schema count");
        using (var preDragonJson = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreDragonVersion,
                Frames = preDragonFrames,
                DisplayFrames = preDragonBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })))
        {
            EnemySpritemapCatalog dragonUpgraded = EnemySpritemapCatalog.Load(
                preDragonJson, stock.Spritemaps);
            foreach (EnemySpritemapDefinition frame in DragonVisualDefinitions.Frames())
                AssertTrue(dragonUpgraded.TryGetDisplay(frame.Bank, frame.Pointer, out _),
                    $"version-forty-seven override inherits Dragon frame {frame.Name}");
        }
        var preTripperKamerFrames = preDragonFrames
            .Where(pair => !pair.Key.StartsWith("tripper_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        var preTripperKamerBindings = document.DisplayFrames!
            .Where(pair => preTripperKamerFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreTripperKamerFrameCount,
            preTripperKamerFrames.Count, "version-forty-six composition schema count");
        using (var preTripperKamerJson = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreTripperKamerVersion,
                Frames = preTripperKamerFrames,
                DisplayFrames = preTripperKamerBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })))
        {
            EnemySpritemapCatalog platformUpgraded = EnemySpritemapCatalog.Load(
                preTripperKamerJson, stock.Spritemaps);
            foreach (EnemySpritemapDefinition frame in TripperKamerVisualDefinitions.Frames())
                AssertTrue(platformUpgraded.TryGetDisplay(frame.Bank, frame.Pointer, out _),
                    $"version-forty-six override inherits platform frame {frame.Name}");
        }
        var preShaktoolFrames = preDragonFrames
            .Where(pair => !pair.Key.StartsWith("shaktool_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("tripper_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        var preShaktoolBindings = document.DisplayFrames!
            .Where(pair => preShaktoolFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreShaktoolFrameCount,
            preShaktoolFrames.Count, "version-forty-five composition schema count");
        using (var preShaktoolJson = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreShaktoolVersion,
                Frames = preShaktoolFrames,
                DisplayFrames = preShaktoolBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })))
        {
            EnemySpritemapCatalog shaktoolUpgraded = EnemySpritemapCatalog.Load(
                preShaktoolJson, stock.Spritemaps);
            foreach (EnemySpritemapDefinition frame in ShaktoolVisualDefinitions.Frames())
                AssertTrue(shaktoolUpgraded.TryGetDisplay(frame.Bank, frame.Pointer, out _),
                    $"version-forty-five override inherits Shaktool frame {frame.Name}");
        }
        SpriteVisualPart originalPart = document.Frames["boyon_idle_0"][0];
        document.Frames["boyon_idle_0"][0] = originalPart with
        {
            OffsetX = originalPart.OffsetX + 1,
            TileColumn = originalPart.TileColumn + 1,
        };
        SpriteVisualPart cacatacPart = document.Frames["cacatac_upright_idle_0"][0];
        document.Frames["cacatac_upright_idle_0"][0] = cacatacPart with
        {
            OffsetY = cacatacPart.OffsetY + 1,
        };
        SpriteVisualPart boulderPart = document.Frames["boulder_roll_0"][0];
        document.Frames["boulder_roll_0"][0] = boulderPart with
        {
            OffsetY = boulderPart.OffsetY + 1,
        };
        SpriteVisualPart atomicPart = document.Frames["atomic_up_right_0"][0];
        document.Frames["atomic_up_right_0"][0] = atomicPart with
        {
            OffsetY = atomicPart.OffsetY + 1,
        };
        SpriteVisualPart skulteraPart = document.Frames["skultera_swim_left_0"][0];
        document.Frames["skultera_swim_left_0"][0] = skulteraPart with
        {
            OffsetY = skulteraPart.OffsetY + 1,
        };
        SpriteVisualPart waverPart = document.Frames["waver_steady_left"][0];
        document.Frames["waver_steady_left"][0] = waverPart with
        {
            OffsetY = waverPart.OffsetY + 1,
        };
        foreach (string frameName in new[]
                 { "zoa_shoot_left_0", "metaree_idle_0", "skree_idle_0" })
        {
            SpriteVisualPart part = document.Frames[frameName][0];
            document.Frames[frameName][0] = part with { OffsetY = part.OffsetY + 1 };
        }
        foreach (string frameName in new[]
                 {
                     "pipe_brinstar_normal_left_0", "pipe_brinstar_strong_rise_left_0",
                     "pipe_norfair_left_0", "pipe_yellow_fly_left_0",
                 })
        {
            SpriteVisualPart part = document.Frames[frameName][0];
            document.Frames[frameName][0] = part with { OffsetY = part.OffsetY + 1 };
        }
        foreach (string frameName in new[]
                 { "fake_kraid_walk_left_0", "kraid_nail_0",
                   "owtch_left_0", "stoke_walk_left_0" })
        {
            SpriteVisualPart part = document.Frames[frameName][0];
            document.Frames[frameName][0] = part with { OffsetY = part.OffsetY + 1 };
        }
        foreach (string frameName in new[]
                 { "ripper_shared_left_0", "ripper_shared_frozen_left",
                   "ripper_right_0" })
        {
            SpriteVisualPart part = document.Frames[frameName][0];
            document.Frames[frameName][0] = part with { OffsetY = part.OffsetY + 1 };
        }
        SpriteVisualPart firefleaPart = document.Frames["fireflea_cycle_0"][0];
        document.Frames["fireflea_cycle_0"][0] = firefleaPart with
        {
            OffsetY = firefleaPart.OffsetY + 1,
        };
        SpriteVisualPart magdollitePart = document.Frames["magdollite_left_idle_0"][0];
        document.Frames["magdollite_left_idle_0"][0] = magdollitePart with
        {
            OffsetY = magdollitePart.OffsetY + 1,
        };
        SpriteVisualPart ceresDoorPart = document.Frames["ceres_door_right_hold"][0];
        document.Frames["ceres_door_right_hold"][0] = ceresDoorPart with
        {
            OffsetY = ceresDoorPart.OffsetY + 1,
        };
        SpriteVisualPart ceresBabyPart = document.Frames["ceres_baby_round"][0];
        document.Frames["ceres_baby_round"][0] = ceresBabyPart with
        {
            OffsetY = ceresBabyPart.OffsetY + 1,
        };
        SpriteVisualPart rioPart = document.Frames["rio_bd6c"][0];
        document.Frames["rio_bd6c"][0] = rioPart with
        {
            OffsetY = rioPart.OffsetY + 1,
        };
        SpriteVisualPart lowerRioPart = document.Frames["lower_norfair_rio_c8bd"][0];
        document.Frames["lower_norfair_rio_c8bd"][0] = lowerRioPart with
        {
            OffsetY = lowerRioPart.OffsetY + 1,
        };
        SpriteVisualPart norfairRioPart = document.Frames["norfair_rio_c442"][0];
        document.Frames["norfair_rio_c442"][0] = norfairRioPart with
        {
            OffsetY = norfairRioPart.OffsetY + 1,
        };
        SpriteVisualPart puyoPart = document.Frames["puyo_ground_0"][0];
        document.Frames["puyo_ground_0"][0] = puyoPart with
        {
            OffsetY = puyoPart.OffsetY + 1,
        };
        SpriteVisualPart bullPart = document.Frames["bull_idle_0"][0];
        document.Frames["bull_idle_0"][0] = bullPart with
        {
            OffsetY = bullPart.OffsetY + 1,
        };
        SpriteVisualPart alcoonPart = document.Frames["alcoon_left_walk_0"][0];
        document.Frames["alcoon_left_walk_0"][0] = alcoonPart with
        {
            OffsetY = alcoonPart.OffsetY + 1,
        };
        SpriteVisualPart beetomPart = document.Frames["beetom_left_crawl_0"][0];
        document.Frames["beetom_left_crawl_0"][0] = beetomPart with
        {
            OffsetY = beetomPart.OffsetY + 1,
        };
        SpriteVisualPart hopperPart = document.Frames["sidehopper_jump_floor"][0];
        document.Frames["sidehopper_jump_floor"][0] = hopperPart with
        {
            OffsetY = hopperPart.OffsetY + 1,
        };
        SpriteVisualPart chootPart = document.Frames["choot_idle"][0];
        document.Frames["choot_idle"][0] = chootPart with
        {
            OffsetY = chootPart.OffsetY + 1,
        };
        SpriteVisualPart hzoomerPart = document.Frames["hzoomer_upside_right_0"][0];
        document.Frames["hzoomer_upside_right_0"][0] = hzoomerPart with
        {
            OffsetY = hzoomerPart.OffsetY + 1,
        };
        SpriteVisualPart sbugPart = document.Frames["sbug_right_0"][0];
        document.Frames["sbug_right_0"][0] = sbugPart with
        {
            OffsetY = sbugPart.OffsetY + 1,
        };
        SpriteVisualPart funePart = document.Frames["fune_right_idle"][0];
        document.Frames["fune_right_idle"][0] = funePart with
        {
            OffsetY = funePart.OffsetY + 1,
        };
        SpriteVisualPart kamerPart = document.Frames["kamer_platform_0"][0];
        document.Frames["kamer_platform_0"][0] = kamerPart with
        {
            OffsetY = kamerPart.OffsetY + 1,
        };
        SpriteVisualPart elevatorPart = document.Frames["elevator_platform_0"][0];
        document.Frames["elevator_platform_0"][0] = elevatorPart with
        {
            OffsetY = elevatorPart.OffsetY + 1,
        };
        SpriteVisualPart draygonIntroPart = document.Frames["draygon_intro_evir_0"][0];
        document.Frames["draygon_intro_evir_0"][0] = draygonIntroPart with
        {
            OffsetX = draygonIntroPart.OffsetX + 1,
        };
        SpriteVisualPart sharedSpritePart = document.Frames["room_sprite_b4_dc3f"][0];
        document.Frames["room_sprite_b4_dc3f"][0] = sharedSpritePart with
        {
            OffsetY = sharedSpritePart.OffsetY + 1,
        };
        ushort mawIdlePointer = YappingMawVisualDefinitions.FrameAt(
            YappingMawInstructionProgramDefinitions.PresentationWordAddress(0));
        string mawIdleName = $"yapping_maw_a8_{mawIdlePointer:x4}";
        SpriteVisualPart mawPart = document.Frames[mawIdleName][0];
        document.Frames[mawIdleName][0] = mawPart with
        {
            OffsetY = mawPart.OffsetY + 1,
        };
        ushort hunterIdlePointer = KiHunterVisualDefinitions.FrameAt(
            KiHunterInstructionProgramDefinitions.PresentationWordAddress(0));
        string hunterIdleName = $"ki_hunter_a8_{hunterIdlePointer:x4}";
        SpriteVisualPart hunterPart = document.Frames[hunterIdleName][0];
        document.Frames[hunterIdleName][0] = hunterPart with
        {
            OffsetY = hunterPart.OffsetY + 1,
        };
        const string motherBrainHeadName = "mother_brain_a9_a586";
        SpriteVisualPart motherBrainHeadPart = document.Frames[motherBrainHeadName][0];
        document.Frames[motherBrainHeadName][0] = motherBrainHeadPart with
        {
            OffsetX = motherBrainHeadPart.OffsetX + 1,
        };
        const string deadTorizoName = "dead_torizo_corpse_a9_d761";
        SpriteVisualPart deadTorizoPart = document.Frames[deadTorizoName][0];
        document.Frames[deadTorizoName][0] = deadTorizoPart with
        {
            OffsetX = deadTorizoPart.OffsetX + 1,
        };
        const string ridleyWingName = "ridley_supplement_a6_dd4a";
        SpriteVisualPart ridleyWingPart = document.Frames[ridleyWingName][0];
        document.Frames[ridleyWingName][0] = ridleyWingPart with
        {
            OffsetX = ridleyWingPart.OffsetX + 1,
        };
        const string sciserFrameName = "sciser_upside_up_0";
        SpriteVisualPart sciserPart = document.Frames[sciserFrameName][0];
        document.Frames[sciserFrameName][0] = sciserPart with
        {
            OffsetX = sciserPart.OffsetX + 1,
        };
        const string flyFrameName = "fly_shared_0";
        SpriteVisualPart flyPart = document.Frames[flyFrameName][0];
        document.Frames[flyFrameName][0] = flyPart with
        {
            OffsetX = flyPart.OffsetX + 1,
        };
        const string kagoFrameName = "kago_cycle_0";
        SpriteVisualPart kagoPart = document.Frames[kagoFrameName][0];
        document.Frames[kagoFrameName][0] = kagoPart with
        {
            OffsetX = kagoPart.OffsetX + 1,
        };
        const string faceBlockFrameName = "face_block_neutral";
        SpriteVisualPart faceBlockPart = document.Frames[faceBlockFrameName][0];
        document.Frames[faceBlockFrameName][0] = faceBlockPart with
        {
            OffsetX = faceBlockPart.OffsetX + 1,
        };
        const string morphEyeFrameName = "morph_eye_mount_left";
        SpriteVisualPart morphEyePart = document.Frames[morphEyeFrameName][0];
        document.Frames[morphEyeFrameName][0] = morphEyePart with
        {
            OffsetX = morphEyePart.OffsetX + 1,
        };
        const string shutterFrameName = "shutter_vertical_40px";
        SpriteVisualPart shutterPart = document.Frames[shutterFrameName][0];
        document.Frames[shutterFrameName][0] = shutterPart with
        {
            OffsetX = shutterPart.OffsetX + 1,
        };
        const string metroidFrameName = "metroid_body_0";
        SpriteVisualPart metroidPart = document.Frames[metroidFrameName][0];
        document.Frames[metroidFrameName][0] = metroidPart with
        {
            OffsetX = metroidPart.OffsetX + 1,
        };
        const string dragonFrameName = "dragon_body_idle_left";
        SpriteVisualPart dragonPart = document.Frames[dragonFrameName][0];
        document.Frames[dragonFrameName][0] = dragonPart with
        {
            OffsetX = dragonPart.OffsetX + 1,
        };
        const string multiviolaFrameName = "multiviola_spin_0";
        SpriteVisualPart multiviolaPart = document.Frames[multiviolaFrameName][0];
        document.Frames[multiviolaFrameName][0] = multiviolaPart with
        {
            OffsetX = multiviolaPart.OffsetX + 1,
        };
        const string lavaJumperFrameName = "norfair_lava_jumper_c02c";
        SpriteVisualPart lavaJumperPart = document.Frames[lavaJumperFrameName][0];
        document.Frames[lavaJumperFrameName][0] = lavaJumperPart with
        {
            OffsetX = lavaJumperPart.OffsetX + 1,
        };
        const string chozoFrameName = "chozo_statue_aa_efd8";
        SpriteVisualPart chozoPart = document.Frames[chozoFrameName][0];
        document.Frames[chozoFrameName][0] = chozoPart with
        {
            OffsetX = chozoPart.OffsetX + 1,
        };
        const string violaFrameName = "viola_spin_b689";
        SpriteVisualPart violaPart = document.Frames[violaFrameName][0];
        document.Frames[violaFrameName][0] = violaPart with
        {
            OffsetX = violaPart.OffsetX + 1,
        };
        const string rinkaFrameName = "rinka_spin_ba38";
        SpriteVisualPart rinkaPart = document.Frames[rinkaFrameName][0];
        document.Frames[rinkaFrameName][0] = rinkaPart with
        {
            OffsetX = rinkaPart.OffsetX + 1,
        };
        string overrideDirectory = Path.Combine(stockDirectory, "spritemap-overrides");
        Directory.CreateDirectory(overrideDirectory);
        string overridePath = Path.Combine(overrideDirectory, fileName);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(document,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        OamBuffer stockDragon = DrawEnemy(stock, new FrameReadGuard(rom),
            DragonVisualDefinitions.FrameAt(
                DragonInstructionProgramDefinitions.PresentationWordAddress(0)),
            RoomEnemySystem.DragonDefinition);
        OamBuffer editedDragon = DrawEnemy(edited, new FrameReadGuard(rom),
            DragonVisualDefinitions.FrameAt(
                DragonInstructionProgramDefinitions.PresentationWordAddress(0)),
            RoomEnemySystem.DragonDefinition);
        AssertEqual(unchecked((byte)(stockDragon.LowTable[0] + 1)),
            editedDragon.LowTable[0],
            "Dragon authored body offset changes installed room OAM");
        OamBuffer stockMultiviola = DrawEnemy(stock, new FrameReadGuard(rom),
            MultiviolaVisualDefinitions.FrameAt(
                MultiviolaInstructionProgramDefinitions.PresentationWordAddress(0)),
            RoomEnemySystem.MultiviolaDefinition);
        OamBuffer editedMultiviola = DrawEnemy(edited, new FrameReadGuard(rom),
            MultiviolaVisualDefinitions.FrameAt(
                MultiviolaInstructionProgramDefinitions.PresentationWordAddress(0)),
            RoomEnemySystem.MultiviolaDefinition);
        AssertEqual(unchecked((byte)(stockMultiviola.LowTable[0] + 1)),
            editedMultiviola.LowTable[0],
            "Multiviola authored body offset changes installed room OAM");
        OamBuffer stockLavaJumper = DrawEnemy(stock, new FrameReadGuard(rom),
            NorfairLavaJumperVisualDefinitions.FrameAt(
                NorfairLavaJumperInstructionProgramDefinitions.PresentationWordAddress(0)),
            RoomEnemySystem.NorfairLavaJumpingEnemyDefinition);
        OamBuffer editedLavaJumper = DrawEnemy(edited, new FrameReadGuard(rom),
            NorfairLavaJumperVisualDefinitions.FrameAt(
                NorfairLavaJumperInstructionProgramDefinitions.PresentationWordAddress(0)),
            RoomEnemySystem.NorfairLavaJumpingEnemyDefinition);
        AssertEqual(unchecked((byte)(stockLavaJumper.LowTable[0] + 1)),
            editedLavaJumper.LowTable[0],
            "lava-jumper authored OAM offset changes installed room presentation");
        OamBuffer stockChozo = DrawEnemy(stock, new FrameReadGuard(rom),
            ChozoStatueVisualDefinitions.FrameAt(
                ChozoStatueInstructionProgramDefinitions.PresentationWordAddress(0)),
            ChozoStatueEnemyDefinitions.EnemyDefinitionPointer);
        OamBuffer editedChozo = DrawEnemy(edited, new FrameReadGuard(rom),
            ChozoStatueVisualDefinitions.FrameAt(
                ChozoStatueInstructionProgramDefinitions.PresentationWordAddress(0)),
            ChozoStatueEnemyDefinitions.EnemyDefinitionPointer);
        AssertEqual(unchecked((byte)(stockChozo.LowTable[0] + 1)),
            editedChozo.LowTable[0],
            "Chozo authored body offset changes installed room OAM");
        OamBuffer stockViola = DrawEnemy(stock, new FrameReadGuard(rom),
            ViolaVisualDefinitions.FrameAt(
                ViolaInstructionProgramDefinitions.PresentationWordAddress(0)),
            RoomEnemySystem.ViolaDefinition);
        OamBuffer editedViola = DrawEnemy(edited, new FrameReadGuard(rom),
            ViolaVisualDefinitions.FrameAt(
                ViolaInstructionProgramDefinitions.PresentationWordAddress(0)),
            RoomEnemySystem.ViolaDefinition);
        AssertEqual(unchecked((byte)(stockViola.LowTable[0] + 1)),
            editedViola.LowTable[0],
            "Viola authored body offset changes installed room OAM");
        OamBuffer stockRinka = DrawEnemy(stock, new FrameReadGuard(rom),
            RinkaVisualDefinitions.FrameAt(
                RinkaInstructionProgramDefinitions.PresentationWordAddress(0)),
            RoomEnemySystem.RinkaDefinition);
        OamBuffer editedRinka = DrawEnemy(edited, new FrameReadGuard(rom),
            RinkaVisualDefinitions.FrameAt(
                RinkaInstructionProgramDefinitions.PresentationWordAddress(0)),
            RoomEnemySystem.RinkaDefinition);
        AssertEqual(unchecked((byte)(stockRinka.LowTable[0] + 1)),
            editedRinka.LowTable[0],
            "Rinka authored body offset changes installed room OAM");
        OamBuffer stockSciser = DrawEnemy(stock, new FrameReadGuard(rom),
            SciserVisualDefinitions.FrameAt(
                SciserInstructionProgramDefinitions.PresentationWordAddress(12)),
            RoomEnemySystem.SciserDefinition);
        OamBuffer editedSciser = DrawEnemy(edited, new FrameReadGuard(rom),
            SciserVisualDefinitions.FrameAt(
                SciserInstructionProgramDefinitions.PresentationWordAddress(12)),
            RoomEnemySystem.SciserDefinition);
        AssertEqual(unchecked((byte)(stockSciser.LowTable[0] + 1)),
            editedSciser.LowTable[0],
            "Sciser authored OAM offset changes installed room presentation");
        foreach (ushort definition in new ushort[]
                 { RoomEnemySystem.MellowDefinition, RoomEnemySystem.MellaDefinition,
                   RoomEnemySystem.MemuDefinition })
        {
            OamBuffer stockFly = DrawEnemy(stock, new FrameReadGuard(rom),
                FlyVisualDefinitions.FrameAt(
                    FlyInstructionProgramDefinitions.PresentationWordAddress(0)), definition);
            OamBuffer editedFly = DrawEnemy(edited, new FrameReadGuard(rom),
                FlyVisualDefinitions.FrameAt(
                    FlyInstructionProgramDefinitions.PresentationWordAddress(0)), definition);
            AssertEqual(unchecked((byte)(stockFly.LowTable[0] + 1)),
                editedFly.LowTable[0],
                $"fly ${definition:X4} uses edited shared presentation");
        }
        OamBuffer stockKago = DrawEnemy(stock, new FrameReadGuard(rom),
            KagoVisualDefinitions.FrameAt(
                KagoInstructionProgramDefinitions.PresentationWordAddress(0)),
            RoomEnemySystem.KagoDefinition);
        OamBuffer editedKago = DrawEnemy(edited, new FrameReadGuard(rom),
            KagoVisualDefinitions.FrameAt(
                KagoInstructionProgramDefinitions.PresentationWordAddress(0)),
            RoomEnemySystem.KagoDefinition);
        AssertEqual(unchecked((byte)(stockKago.LowTable[0] + 1)),
            editedKago.LowTable[0],
            "Kago authored offset changes installed presentation");
        ushort faceBlockPointer = BlueBrinstarFaceBlockVisualDefinitions.FrameAt(
            BlueBrinstarFaceBlockInstructionProgramDefinitions.PresentationWordAddress(6));
        OamBuffer stockFaceBlock = DrawEnemy(stock, new FrameReadGuard(rom),
            faceBlockPointer, RoomEnemySystem.BlueBrinstarFaceBlockDefinition);
        OamBuffer editedFaceBlock = DrawEnemy(edited, new FrameReadGuard(rom),
            faceBlockPointer, RoomEnemySystem.BlueBrinstarFaceBlockDefinition);
        AssertEqual(unchecked((byte)(stockFaceBlock.LowTable[0] + 1)),
            editedFaceBlock.LowTable[0],
            "face-block authored offset changes installed presentation");
        ushort morphEyePointer = MorphBallEyeVisualDefinitions.FrameAt(
            MorphBallEyeInstructionProgramDefinitions.PresentationWordAddress(34));
        OamBuffer stockMorphEye = DrawEnemy(stock, new FrameReadGuard(rom),
            morphEyePointer, RoomEnemySystem.MorphBallEyeDefinition);
        OamBuffer editedMorphEye = DrawEnemy(edited, new FrameReadGuard(rom),
            morphEyePointer, RoomEnemySystem.MorphBallEyeDefinition);
        AssertEqual(unchecked((byte)(stockMorphEye.LowTable[0] + 1)),
            editedMorphEye.LowTable[0],
            "Morph Ball eye mount art edit changes installed presentation");
        ushort shutterPointer = ShutterVisualDefinitions.FrameAt(
            RoomEnemySystem.GrowingShutterDefinition,
            GrowingShutterInstructionProgramDefinitions.PresentationWordAddress(3));
        foreach (ushort definition in new ushort[]
                 { RoomEnemySystem.GrowingShutterDefinition,
                   RoomEnemySystem.ShootableVerticalShutterDefinition,
                   RoomEnemySystem.DestroyableVerticalShutterDefinition })
        {
            OamBuffer stockShutter = DrawEnemy(stock, new FrameReadGuard(rom),
                shutterPointer, definition);
            OamBuffer editedShutter = DrawEnemy(edited, new FrameReadGuard(rom),
                shutterPointer, definition);
            AssertEqual(unchecked((byte)(stockShutter.LowTable[0] + 1)),
                editedShutter.LowTable[0],
                $"shutter ${definition:X4} uses edited shared forty-pixel art");
            AssertEqual(stockShutter.LowTable[1], editedShutter.LowTable[1],
                $"shutter ${definition:X4} visual edit leaves physical Y unchanged");
        }
        ushort metroidPointer = MetroidVisualDefinitions.FrameAt(
            MetroidInstructionProgramDefinitions.PresentationWordAddress(0));
        OamBuffer stockMetroid = DrawEnemy(stock, new FrameReadGuard(rom),
            metroidPointer, RoomEnemySystem.MetroidDefinition);
        OamBuffer editedMetroid = DrawEnemy(edited, new FrameReadGuard(rom),
            metroidPointer, RoomEnemySystem.MetroidDefinition);
        AssertEqual(unchecked((byte)(stockMetroid.LowTable[0] + 1)),
            editedMetroid.LowTable[0],
            "ordinary-Metroid body art edit changes live OAM X without a ROM read");
        AssertEqual(stockMetroid.LowTable[1], editedMetroid.LowTable[1],
            "ordinary-Metroid body art edit leaves physical Y unchanged");
        OamBuffer stockBrain = DrawInstalledMotherBrainFrame(
            stock, rom, 0xa586, 0x0140, 0x00a0);
        OamBuffer editedBrain = DrawInstalledMotherBrainFrame(
            edited, rom, 0xa586, 0x0140, 0x00a0);
        AssertEqual(unchecked((byte)(stockBrain.LowTable[0] + 1)),
            editedBrain.LowTable[0],
            "Mother Brain head artwork edit changes live OAM X without ROM reads");
        AssertEqual(stockBrain.LowTable[1], editedBrain.LowTable[1],
            "Mother Brain head artwork edit leaves physical Y placement unchanged");
        OamBuffer stockDeadTorizo = DrawDeadTorizoCorpseFrame(stock, rom, 0, 0);
        OamBuffer editedDeadTorizo = DrawDeadTorizoCorpseFrame(edited, rom, 0, 0);
        AssertEqual(unchecked((byte)(stockDeadTorizo.LowTable[0] + 1)),
            editedDeadTorizo.LowTable[0],
            "Dead Torizo corpse composition edit changes live OAM X without ROM reads");
        AssertEqual(stockDeadTorizo.LowTable[1], editedDeadTorizo.LowTable[1],
            "Dead Torizo corpse composition edit leaves physical Y placement unchanged");
        OamBuffer stockRidleyWing = DrawInstalledRidleySupplement(
            stock, rom, facing: 0, wingFrame: 0, tailDirection: null);
        OamBuffer editedRidleyWing = DrawInstalledRidleySupplement(
            edited, rom, facing: 0, wingFrame: 0, tailDirection: null);
        AssertEqual(unchecked((byte)(stockRidleyWing.LowTable[0] + 1)),
            editedRidleyWing.LowTable[0],
            "Ridley wing artwork edit changes live OAM X without ROM reads");
        AssertEqual(stockRidleyWing.LowTable[1], editedRidleyWing.LowTable[1],
            "Ridley wing artwork edit leaves physical Y placement unchanged");
        OamBuffer stockMaw = DrawEnemy(stock, new FrameReadGuard(rom),
            mawIdlePointer, RoomEnemySystem.YappingMawDefinition);
        OamBuffer editedMaw = DrawEnemy(edited, new FrameReadGuard(rom),
            mawIdlePointer, RoomEnemySystem.YappingMawDefinition);
        AssertEqual(unchecked((byte)(stockMaw.LowTable[1] + 1)),
            editedMaw.LowTable[1],
            "editable Yapping Maw OAM offset changes its displayed frame");
        AssertEqual(stockMaw.LowTable[0], editedMaw.LowTable[0],
            "Yapping Maw visual edit leaves physical X placement unchanged");
        OamBuffer stockHunter = DrawEnemy(stock, new FrameReadGuard(rom),
            hunterIdlePointer, RoomEnemySystem.KiHunterDefinition);
        OamBuffer editedHunter = DrawEnemy(edited, new FrameReadGuard(rom),
            hunterIdlePointer, RoomEnemySystem.KiHunterDefinition);
        AssertEqual(unchecked((byte)(stockHunter.LowTable[1] + 1)),
            editedHunter.LowTable[1],
            "editable KiHunter OAM offset changes its displayed frame");
        AssertEqual(stockHunter.LowTable[0], editedHunter.LowTable[0],
            "KiHunter visual edit leaves physical X placement unchanged");
        OamBuffer stockIntroEvir = DrawRoomSpriteObject(stock, new BankB4ReadGuard(rom),
            RoomSpriteObjectKind.DraygonIntroEvir);
        OamBuffer editedIntroEvir = DrawRoomSpriteObject(edited, new BankB4ReadGuard(rom),
            RoomSpriteObjectKind.DraygonIntroEvir);
        AssertEqual(unchecked((byte)(stockIntroEvir.LowTable[0] + 1)),
            editedIntroEvir.LowTable[0],
            "editable Draygon intro Evir offset changes live room sprite-object OAM");
        AssertEqual(stockIntroEvir.LowTable[1], editedIntroEvir.LowTable[1],
            "Draygon intro Evir edit leaves vertical placement unchanged");
        OamBuffer stockDeathEvir = DrawRoomSpriteObject(stock, new BankB4ReadGuard(rom),
            RoomSpriteObjectKind.DraygonDeathEvirFacingRight);
        OamBuffer editedDeathEvir = DrawRoomSpriteObject(edited, new BankB4ReadGuard(rom),
            RoomSpriteObjectKind.DraygonDeathEvirFacingRight);
        AssertEqual(unchecked((byte)(stockDeathEvir.LowTable[1] + 1)),
            editedDeathEvir.LowTable[1],
            "address-named shared sprite-object edit changes live room OAM");
        AssertEqual(stockDeathEvir.LowTable[0], editedDeathEvir.LowTable[0],
            "shared sprite-object edit leaves physical X placement unchanged");
        OamBuffer stockFune = DrawEnemy(stock, new FrameReadGuard(rom),
            0x94cb, FuneNamiheDefinitions.FuneEnemyDefinition);
        OamBuffer editedFune = DrawEnemy(edited, new FrameReadGuard(rom),
            0x94cb, FuneNamiheDefinitions.FuneEnemyDefinition);
        AssertEqual(unchecked((byte)(stockFune.LowTable[1] + 1)),
            editedFune.LowTable[1],
            "editable Fune idle OAM offset changes the displayed enemy frame");
        AssertEqual(stockFune.LowTable[0], editedFune.LowTable[0],
            "Fune visual edit does not change physical X position");
        OamBuffer stockKamer = DrawEnemy(stock, new FrameReadGuard(rom),
            0xf468, RoomEnemySystem.KamerVerticalPlatformDefinition);
        OamBuffer editedKamer = DrawEnemy(edited, new FrameReadGuard(rom),
            0xf468, RoomEnemySystem.KamerVerticalPlatformDefinition);
        AssertEqual(unchecked((byte)(stockKamer.LowTable[1] + 1)),
            editedKamer.LowTable[1],
            "editable Kamer platform OAM offset changes its displayed frame");
        AssertEqual(stockKamer.LowTable[0], editedKamer.LowTable[0],
            "Kamer visual edit leaves its physical X position unchanged");
        OamBuffer stockElevator = DrawEnemy(stock, new FrameReadGuard(rom),
            0x962f, RoomEnemySystem.ElevatorDefinition);
        OamBuffer editedElevator = DrawEnemy(edited, new FrameReadGuard(rom),
            0x962f, RoomEnemySystem.ElevatorDefinition);
        AssertEqual(unchecked((byte)(stockElevator.LowTable[1] + 1)),
            editedElevator.LowTable[1],
            "editable elevator OAM offset changes its displayed frame");
        AssertEqual(stockElevator.LowTable[0], editedElevator.LowTable[0],
            "elevator visual edit leaves its physical X position unchanged");
        ushort framePointer = EnemySpritemapDefinitions.BoyonFrameAt(0x86ad);
        var stockOam = DrawEnemy(stock, new FrameReadGuard(rom), framePointer,
            RoomEnemySystem.BoyonDefinition);
        var editedOam = DrawEnemy(edited, new FrameReadGuard(rom), framePointer,
            RoomEnemySystem.BoyonDefinition);
        AssertEqual(unchecked((byte)(stockOam.LowTable[0] + 1)),
            editedOam.LowTable[0], "authored Boyon X offset changes live room OAM");
        AssertEqual(unchecked((byte)(stockOam.LowTable[2] + 1)),
            editedOam.LowTable[2], "authored Boyon tile changes live room OAM");
        AssertEqual(stockOam.LowTable[1], editedOam.LowTable[1],
            "visual override leaves Boyon Y unchanged");
        OamBuffer stockCeresDoor = DrawEnemy(stock, new FrameReadGuard(rom),
            0xfa13, CeresDoorInstructionProgramDefinitions.EnemyDefinitionPointer);
        OamBuffer editedCeresDoor = DrawEnemy(edited, new FrameReadGuard(rom),
            0xfa13, CeresDoorInstructionProgramDefinitions.EnemyDefinitionPointer,
            slot =>
            {
                AssertEqual((ushort)0xfa13, slot.SpritemapPointer,
                    "Ceres door art edit retains the native selected pose");
                AssertEqual((ushort)7, slot.InstructionTimer,
                    "Ceres door art edit preserves instruction timing");
            });
        AssertEqual(unchecked((byte)(stockCeresDoor.LowTable[1] + 1)),
            editedCeresDoor.LowTable[1],
            "authored Ceres door Y offset changes live room OAM");
        AssertEqual(stockCeresDoor.LowTable[0], editedCeresDoor.LowTable[0],
            "Ceres door visual edit leaves physical X unchanged");
        OamBuffer stockCeresBaby = DrawEnemy(stock, new FrameReadGuard(rom),
            CeresBabyInstructionProgramDefinitions.RoundFrame,
            CeresDoorInstructionProgramDefinitions.EnemyDefinitionPointer);
        OamBuffer editedCeresBaby = DrawEnemy(edited, new FrameReadGuard(rom),
            CeresBabyInstructionProgramDefinitions.RoundFrame,
            CeresDoorInstructionProgramDefinitions.EnemyDefinitionPointer);
        AssertEqual(unchecked((byte)(stockCeresBaby.LowTable[1] + 1)),
            editedCeresBaby.LowTable[1],
            "authored Ceres Baby Y offset changes installed OAM");
        ushort cacatacPointer = EnemySpritemapDefinitions.CacatacFrameAt(0x9e8e);
        var stockCacatac = DrawEnemy(stock, new FrameReadGuard(rom),
            cacatacPointer, RoomEnemySystem.CacatacDefinition);
        var editedCacatac = DrawEnemy(edited, new FrameReadGuard(rom),
            cacatacPointer, RoomEnemySystem.CacatacDefinition);
        AssertEqual(unchecked((byte)(stockCacatac.LowTable[1] + 1)),
            editedCacatac.LowTable[1],
            "authored Cacatac Y offset changes live room OAM");
        AssertEqual(stockCacatac.LowTable[0], editedCacatac.LowTable[0],
            "Cacatac visual override leaves X unchanged");
        ushort boulderPointer = EnemySpritemapDefinitions.BoulderFrameAt(0x86a9);
        var stockBoulder = DrawEnemy(stock, new FrameReadGuard(rom),
            boulderPointer, RoomEnemySystem.BoulderDefinition);
        var editedBoulder = DrawEnemy(edited, new FrameReadGuard(rom),
            boulderPointer, RoomEnemySystem.BoulderDefinition);
        AssertEqual(unchecked((byte)(stockBoulder.LowTable[1] + 1)),
            editedBoulder.LowTable[1],
            "authored Boulder Y offset changes live room OAM");
        AssertEqual(stockBoulder.LowTable[0], editedBoulder.LowTable[0],
            "Boulder visual override leaves X unchanged");
        ushort atomicPointer = EnemySpritemapDefinitions.AtomicFrameAt(0xe312);
        var stockAtomic = DrawEnemy(stock, new FrameReadGuard(rom),
            atomicPointer, RoomEnemySystem.AtomicDefinition);
        var editedAtomic = DrawEnemy(edited, new FrameReadGuard(rom),
            atomicPointer, RoomEnemySystem.AtomicDefinition);
        AssertEqual(unchecked((byte)(stockAtomic.LowTable[1] + 1)),
            editedAtomic.LowTable[1],
            "authored Atomic Y offset changes live room OAM");
        AssertEqual(stockAtomic.LowTable[0], editedAtomic.LowTable[0],
            "Atomic visual override leaves X unchanged");
        ushort skulteraPointer = EnemySpritemapDefinitions.SkulteraFrameAt(0x902e);
        var stockSkultera = DrawEnemy(stock, new FrameReadGuard(rom),
            skulteraPointer, RoomEnemySystem.SkulteraDefinition);
        var editedSkultera = DrawEnemy(edited, new FrameReadGuard(rom),
            skulteraPointer, RoomEnemySystem.SkulteraDefinition);
        AssertEqual(unchecked((byte)(stockSkultera.LowTable[1] + 1)),
            editedSkultera.LowTable[1],
            "authored Skultera Y offset changes live room OAM");
        AssertEqual(stockSkultera.LowTable[0], editedSkultera.LowTable[0],
            "Skultera visual override leaves X unchanged");
        ushort waverPointer = EnemySpritemapDefinitions.WaverFrameAt(0x86a9);
        var stockWaver = DrawEnemy(stock, new FrameReadGuard(rom),
            waverPointer, RoomEnemySystem.WaverDefinition);
        var editedWaver = DrawEnemy(edited, new FrameReadGuard(rom),
            waverPointer, RoomEnemySystem.WaverDefinition);
        AssertEqual(unchecked((byte)(stockWaver.LowTable[1] + 1)),
            editedWaver.LowTable[1],
            "authored Waver Y offset changes live room OAM");
        AssertEqual(stockWaver.LowTable[0], editedWaver.LowTable[0],
            "Waver visual override leaves X unchanged");
        foreach ((ushort pointer, ushort definition, string name) in new[]
                 {
                     (EnemySpritemapDefinitions.ZoaFrameAt(0xb3c5),
                         RoomEnemySystem.ZoaDefinition, "Zoa"),
                     (EnemySpritemapDefinitions.SkreeMetareeFrameAt(true, 0x8912),
                         RoomEnemySystem.MetareeDefinition, "Metaree"),
                     (EnemySpritemapDefinitions.SkreeMetareeFrameAt(false, 0xc660),
                         RoomEnemySystem.SkreeDefinition, "Skree"),
                 })
        {
            OamBuffer nativeFrame = DrawEnemy(stock, new FrameReadGuard(rom),
                pointer, definition);
            OamBuffer editedFrame = DrawEnemy(edited, new FrameReadGuard(rom),
                pointer, definition);
            AssertEqual(unchecked((byte)(nativeFrame.LowTable[1] + 1)),
                editedFrame.LowTable[1],
                $"authored {name} Y offset changes live room OAM");
            AssertEqual(nativeFrame.LowTable[0], editedFrame.LowTable[0],
                $"{name} visual override leaves X unchanged");
        }
        foreach ((ushort definition, ushort operand, string name) in new[]
                 {
                     (PipeBugDefinitions.BrinstarEnemyDefinition, (ushort)0x87ad,
                         "Brinstar Pipe Bug"),
                     (PipeBugDefinitions.StrongBrinstarEnemyDefinition, (ushort)0x8a1f,
                         "strong Brinstar Pipe Bug"),
                     (PipeBugDefinitions.NorfairEnemyDefinition, (ushort)0x8ae3,
                         "Norfair Pipe Bug"),
                     (PipeBugDefinitions.YellowEnemyDefinition, (ushort)0x8efe,
                         "yellow Pipe Bug"),
                 })
        {
            ushort pointer = PipeBugVisualDefinitions.FrameAt(definition, operand);
            OamBuffer nativeFrame = DrawEnemy(stock, new FrameReadGuard(rom),
                pointer, definition);
            OamBuffer editedFrame = DrawEnemy(edited, new FrameReadGuard(rom),
                pointer, definition);
            AssertEqual(unchecked((byte)(nativeFrame.LowTable[1] + 1)),
                editedFrame.LowTable[1],
                $"authored {name} Y offset changes live room OAM");
            AssertEqual(nativeFrame.LowTable[0], editedFrame.LowTable[0],
                $"{name} visual override leaves X unchanged");
        }
        foreach ((ushort definition, ushort operand, string name) in new[]
                 {
                     (RoomEnemySystem.FakeKraidDefinition, (ushort)0x99b0, "Fake Kraid"),
                     (RoomEnemySystem.KraidGoodNailDefinition, (ushort)0x8b0c,
                         "Kraid fingernail"),
                 })
        {
            ushort pointer = KraidVisualDefinitions.FrameAt(definition, operand);
            OamBuffer nativeFrame = DrawEnemy(stock, new FrameReadGuard(rom),
                pointer, definition);
            OamBuffer editedFrame = DrawEnemy(edited, new FrameReadGuard(rom),
                pointer, definition);
            AssertEqual(unchecked((byte)(nativeFrame.LowTable[1] + 1)),
                editedFrame.LowTable[1],
                $"authored {name} Y offset changes live room OAM");
            AssertEqual(nativeFrame.LowTable[0], editedFrame.LowTable[0],
                $"{name} visual override leaves X unchanged");
        }
        foreach ((ushort definition, ushort operand, string name) in new[]
                 {
                     (RoomEnemySystem.OwtchDefinition, (ushort)0xa3af, "Owtch"),
                     (RoomEnemySystem.StokeDefinition, (ushort)0x8936, "Stoke"),
                 })
        {
            ushort pointer = OwtchStokeVisualDefinitions.FrameAt(definition, operand);
            OamBuffer nativeFrame = DrawEnemy(stock, new FrameReadGuard(rom),
                pointer, definition);
            OamBuffer editedFrame = DrawEnemy(edited, new FrameReadGuard(rom),
                pointer, definition);
            AssertEqual(unchecked((byte)(nativeFrame.LowTable[1] + 1)),
                editedFrame.LowTable[1],
                $"authored {name} Y offset changes live room OAM");
            AssertEqual(nativeFrame.LowTable[0], editedFrame.LowTable[0],
                $"{name} visual override leaves X unchanged");
        }
        foreach ((ushort definition, ushort operand, string name) in new[]
                 {
                     (RoomEnemySystem.GRipperDefinition, (ushort)0xe19d, "GRipper"),
                     (RoomEnemySystem.Ripper2Definition, (ushort)0xe2e2, "Ripper II"),
                     (RoomEnemySystem.RipperDefinition, (ushort)0xe479, "Ripper"),
                 })
        {
            ushort pointer = RipperVisualDefinitions.FrameAt(definition, operand);
            OamBuffer nativeFrame = DrawEnemy(stock, new FrameReadGuard(rom),
                pointer, definition);
            OamBuffer editedFrame = DrawEnemy(edited, new FrameReadGuard(rom),
                pointer, definition);
            AssertEqual(unchecked((byte)(nativeFrame.LowTable[1] + 1)),
                editedFrame.LowTable[1],
                $"authored {name} Y offset changes live room OAM");
            AssertEqual(nativeFrame.LowTable[0], editedFrame.LowTable[0],
                $"{name} visual override leaves X unchanged");
        }
        OamBuffer stockFrozen = DrawEnemy(stock, new FrameReadGuard(rom),
            RipperInstructionProgramDefinitions.FrozenFacingLeftSpritemap,
            RoomEnemySystem.GRipperDefinition);
        OamBuffer editedFrozen = DrawEnemy(edited, new FrameReadGuard(rom),
            RipperInstructionProgramDefinitions.FrozenFacingLeftSpritemap,
            RoomEnemySystem.GRipperDefinition);
        AssertEqual(unchecked((byte)(stockFrozen.LowTable[1] + 1)),
            editedFrozen.LowTable[1], "authored frozen GRipper Y offset changes live room OAM");
        ushort firefleaPointer = EnemySpritemapDefinitions.FirefleaFrameAt(
            FirefleaInstructionProgramDefinitions.PresentationWordAddress(0));
        OamBuffer stockFireflea = DrawEnemy(stock, new FrameReadGuard(rom),
            firefleaPointer, RoomEnemySystem.FirefleaDefinition);
        OamBuffer editedFireflea = DrawEnemy(edited, new FrameReadGuard(rom),
            firefleaPointer, RoomEnemySystem.FirefleaDefinition);
        AssertEqual(unchecked((byte)(stockFireflea.LowTable[1] + 1)),
            editedFireflea.LowTable[1],
            "authored Fireflea Y offset changes live room OAM");
        AssertEqual(stockFireflea.LowTable[0], editedFireflea.LowTable[0],
            "Fireflea visual edit leaves X position unchanged");
        ushort magdollitePointer = EnemySpritemapDefinitions.MagdolliteFrameAt(
            MagdolliteInstructionProgramDefinitions.PresentationWordAddress(0));
        OamBuffer stockMagdollite = DrawEnemy(stock, new FrameReadGuard(rom),
            magdollitePointer, RoomEnemySystem.MagdolliteDefinition);
        OamBuffer editedMagdollite = DrawEnemy(edited, new FrameReadGuard(rom),
            magdollitePointer, RoomEnemySystem.MagdolliteDefinition);
        AssertEqual(unchecked((byte)(stockMagdollite.LowTable[1] + 1)),
            editedMagdollite.LowTable[1],
            "authored Magdollite Y offset changes live room OAM");
        AssertEqual(stockMagdollite.LowTable[0], editedMagdollite.LowTable[0],
            "Magdollite visual edit leaves X position unchanged");
        ushort rioPointer = EnemySpritemapDefinitions.RioFrameAt(
            RioInstructionProgramDefinitions.PresentationWordAddress(0));
        OamBuffer stockRio = DrawEnemy(stock, new FrameReadGuard(rom),
            rioPointer, RoomEnemySystem.RioDefinition);
        OamBuffer editedRio = DrawEnemy(edited, new FrameReadGuard(rom),
            rioPointer, RoomEnemySystem.RioDefinition);
        AssertEqual(unchecked((byte)(stockRio.LowTable[1] + 1)),
            editedRio.LowTable[1],
            "authored Rio Y offset changes installed room OAM");
        AssertEqual(stockRio.LowTable[0], editedRio.LowTable[0],
            "Rio visual edit leaves physical X unchanged");
        ushort lowerRioPointer = EnemySpritemapDefinitions.LowerNorfairRioFrameAt(
            LowerNorfairRioInstructionProgramDefinitions.PresentationWordAddress(0));
        OamBuffer stockLowerRio = DrawEnemy(stock, new FrameReadGuard(rom),
            lowerRioPointer, RoomEnemySystem.LowerNorfairRioDefinition);
        OamBuffer editedLowerRio = DrawEnemy(edited, new FrameReadGuard(rom),
            lowerRioPointer, RoomEnemySystem.LowerNorfairRioDefinition);
        AssertEqual(unchecked((byte)(stockLowerRio.LowTable[1] + 1)),
            editedLowerRio.LowTable[1],
            "authored Lower Norfair Rio Y offset changes installed room OAM");
        AssertEqual(stockLowerRio.LowTable[0], editedLowerRio.LowTable[0],
            "Lower Norfair Rio visual edit leaves physical X unchanged");
        ushort norfairRioPointer = EnemySpritemapDefinitions.NorfairRioFrameAt(
            NorfairRioInstructionProgramDefinitions.PresentationWordAddress(0));
        OamBuffer stockNorfairRio = DrawEnemy(stock, new FrameReadGuard(rom),
            norfairRioPointer, RoomEnemySystem.NorfairRioDefinition);
        OamBuffer editedNorfairRio = DrawEnemy(edited, new FrameReadGuard(rom),
            norfairRioPointer, RoomEnemySystem.NorfairRioDefinition);
        AssertEqual(unchecked((byte)(stockNorfairRio.LowTable[1] + 1)),
            editedNorfairRio.LowTable[1],
            "authored Norfair Rio Y offset changes installed room OAM");
        AssertEqual(stockNorfairRio.LowTable[0], editedNorfairRio.LowTable[0],
            "Norfair Rio visual edit leaves physical X unchanged");
        ushort puyoPointer = EnemySpritemapDefinitions.PuyoFrameAt(
            PuyoInstructionProgramDefinitions.PresentationWordAddress(0));
        OamBuffer stockPuyo = DrawEnemy(stock, new FrameReadGuard(rom),
            puyoPointer, RoomEnemySystem.PuyoDefinition);
        OamBuffer editedPuyo = DrawEnemy(edited, new FrameReadGuard(rom),
            puyoPointer, RoomEnemySystem.PuyoDefinition);
        AssertEqual(unchecked((byte)(stockPuyo.LowTable[1] + 1)),
            editedPuyo.LowTable[1],
            "authored Puyo Y offset changes installed room OAM");
        AssertEqual(stockPuyo.LowTable[0], editedPuyo.LowTable[0],
            "Puyo visual edit leaves physical X unchanged");
        ushort bullPointer = EnemySpritemapDefinitions.BullFrameAt(
            BullInstructionProgramDefinitions.PresentationWordAddress(0));
        OamBuffer stockBull = DrawEnemy(stock, new FrameReadGuard(rom),
            bullPointer, RoomEnemySystem.BullDefinition);
        OamBuffer editedBull = DrawEnemy(edited, new FrameReadGuard(rom),
            bullPointer, RoomEnemySystem.BullDefinition);
        AssertEqual(unchecked((byte)(stockBull.LowTable[1] + 1)),
            editedBull.LowTable[1],
            "authored Bull Y offset changes installed room OAM");
        AssertEqual(stockBull.LowTable[0], editedBull.LowTable[0],
            "Bull visual edit leaves physical X unchanged");
        ushort alcoonPointer = EnemySpritemapDefinitions.AlcoonFrameAt(
            AlcoonInstructionProgramDefinitions.PresentationWordAddress(0));
        OamBuffer stockAlcoon = DrawEnemy(stock, new FrameReadGuard(rom),
            alcoonPointer, RoomEnemySystem.AlcoonDefinition);
        OamBuffer editedAlcoon = DrawEnemy(edited, new FrameReadGuard(rom),
            alcoonPointer, RoomEnemySystem.AlcoonDefinition);
        AssertEqual(unchecked((byte)(stockAlcoon.LowTable[1] + 1)),
            editedAlcoon.LowTable[1],
            "authored Alcoon Y offset changes installed room OAM");
        AssertEqual(stockAlcoon.LowTable[0], editedAlcoon.LowTable[0],
            "Alcoon visual edit leaves physical X unchanged");
        ushort beetomPointer = EnemySpritemapDefinitions.BeetomFrameAt(
            BeetomInstructionProgramDefinitions.PresentationWordAddress(0));
        OamBuffer stockBeetom = DrawEnemy(stock, new FrameReadGuard(rom),
            beetomPointer, RoomEnemySystem.BeetomDefinition);
        OamBuffer editedBeetom = DrawEnemy(edited, new FrameReadGuard(rom),
            beetomPointer, RoomEnemySystem.BeetomDefinition);
        AssertEqual(unchecked((byte)(stockBeetom.LowTable[1] + 1)),
            editedBeetom.LowTable[1],
            "authored Beetom Y offset changes installed room OAM");
        AssertEqual(stockBeetom.LowTable[0], editedBeetom.LowTable[0],
            "Beetom visual edit leaves physical X unchanged");
        ushort hopperPointer = EnemySpritemapDefinitions.HopperFrameAt(
            HopperInstructionProgramDefinitions.PresentationWordAddress(0));
        OamBuffer stockHopper = DrawEnemy(stock, new FrameReadGuard(rom),
            hopperPointer, RoomEnemySystem.SidehopperDefinition);
        OamBuffer editedHopper = DrawEnemy(edited, new FrameReadGuard(rom),
            hopperPointer, RoomEnemySystem.SidehopperDefinition);
        AssertEqual(unchecked((byte)(stockHopper.LowTable[1] + 1)),
            editedHopper.LowTable[1],
            "authored Sidehopper Y offset changes installed room OAM");
        AssertEqual(stockHopper.LowTable[0], editedHopper.LowTable[0],
            "Sidehopper visual edit leaves physical X unchanged");
        ushort chootPointer = EnemySpritemapDefinitions.ChootFrameAt(
            ChootInstructionProgramDefinitions.PresentationWordAddress(0));
        OamBuffer stockChoot = DrawEnemy(stock, new FrameReadGuard(rom),
            chootPointer, RoomEnemySystem.ChootDefinition);
        OamBuffer editedChoot = DrawEnemy(edited, new FrameReadGuard(rom),
            chootPointer, RoomEnemySystem.ChootDefinition);
        AssertEqual(unchecked((byte)(stockChoot.LowTable[1] + 1)),
            editedChoot.LowTable[1],
            "authored Choot Y offset changes installed room OAM");
        AssertEqual(stockChoot.LowTable[0], editedChoot.LowTable[0],
            "Choot visual edit leaves physical X unchanged");
        ushort hzoomerPointer = EnemySpritemapDefinitions.HZoomerFrameAt(
            HZoomerInstructionProgramDefinitions.PresentationWordAddress(0));
        OamBuffer stockHZoomer = DrawEnemy(stock, new FrameReadGuard(rom),
            hzoomerPointer, RoomEnemySystem.HZoomerDefinition);
        OamBuffer editedHZoomer = DrawEnemy(edited, new FrameReadGuard(rom),
            hzoomerPointer, RoomEnemySystem.HZoomerDefinition);
        AssertEqual(unchecked((byte)(stockHZoomer.LowTable[1] + 1)),
            editedHZoomer.LowTable[1],
            "authored HZoomer Y offset changes installed room OAM");
        AssertEqual(stockHZoomer.LowTable[0], editedHZoomer.LowTable[0],
            "HZoomer visual edit leaves physical X unchanged");
        ushort sbugPointer = EnemySpritemapDefinitions.SbugFrameAt(
            SbugInstructionProgramDefinitions.PresentationWordAddress(0));
        foreach (ushort definition in new ushort[]
                 { RoomEnemySystem.SbugDefinition, RoomEnemySystem.Sbug2Definition })
        {
            OamBuffer stockSbug = DrawEnemy(stock, new FrameReadGuard(rom),
                sbugPointer, definition);
            OamBuffer editedSbug = DrawEnemy(edited, new FrameReadGuard(rom),
                sbugPointer, definition);
            AssertEqual(unchecked((byte)(stockSbug.LowTable[1] + 1)),
                editedSbug.LowTable[1],
                $"authored Sbug Y offset changes installed enemy ${definition:X4}");
            AssertEqual(stockSbug.LowTable[0], editedSbug.LowTable[0],
                $"Sbug visual edit leaves enemy ${definition:X4} physical X unchanged");
        }
        VerifyHZoomerInstructionProgramDefinitions(rom, edited);
        foreach (ushort definition in new ushort[]
                 {
                     RoomEnemySystem.ZeelaDefinition,
                     RoomEnemySystem.SovaDefinition,
                     RoomEnemySystem.ZoomerDefinition,
                     RoomEnemySystem.StoneZoomerDefinition,
                 })
        {
            OamBuffer stockCrawler = DrawEnemy(stock, new FrameReadGuard(rom),
                hzoomerPointer, definition);
            OamBuffer editedCrawler = DrawEnemy(edited, new FrameReadGuard(rom),
                hzoomerPointer, definition);
            AssertEqual(unchecked((byte)(stockCrawler.LowTable[1] + 1)),
                editedCrawler.LowTable[1],
                $"shared crawler ${definition:X4} uses the edited visual Y offset");
            AssertEqual(stockCrawler.LowTable[0], editedCrawler.LowTable[0],
                $"shared crawler ${definition:X4} retains physical X");
        }
        VerifySharedCrawlerInstructionProgramDefinitions(rom, edited);
        AssertTrue(EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory)
                .Spritemaps!.TryGet(EnemySpritemapDefinitions.BoyonBank, framePointer, out _),
            "enemy composition override survives catalog reload");
        IEnumerable<KeyValuePair<string, SpriteVisualPart[]>> HistoricalFrames() =>
            document.Frames.Where(pair =>
                !pair.Key.StartsWith("mochtroid_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("dead_zoomer_corpse_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("dead_ripper_corpse_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("dead_skree_corpse_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("dead_sidehopper_", StringComparison.Ordinal) &&
                pair.Key != "dead_torizo_stationary_a9_d6e2" &&
                !pair.Key.StartsWith("dragon_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("multiviola_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("norfair_lava_jumper_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("chozo_statue_aa_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("viola_spin_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("rinka_spin_", StringComparison.Ordinal));
        var preMetroidFrames = HistoricalFrames()
            .Where(pair => !pair.Key.StartsWith("metroid_body_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("shaktool_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("tripper_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreMetroidFrameCount,
            preMetroidFrames.Count, "pre-Metroid composition schema count");
        var preMetroidBindings = document.DisplayFrames!
            .Where(pair => preMetroidFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreMetroidVersion,
                Frames = preMetroidFrames,
                DisplayFrames = preMetroidBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog preMetroidUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        foreach (EnemySpritemapDefinition frame in MetroidVisualDefinitions.Frames())
            AssertTrue(preMetroidUpgraded.Spritemaps!.TryGetDisplay(
                    frame.Bank, frame.Pointer, out _),
                $"version-forty-four override inherits Metroid frame {frame.Name}");
        document = new EnemySpritemapDocument
        {
            Version = EnemySpritemapDefinitions.PreMetroidVersion,
            Frames = preMetroidFrames,
            DisplayFrames = preMetroidBindings,
        };
        var preShutterFrames = HistoricalFrames()
            .Where(pair => !pair.Key.StartsWith("shutter_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreShutterFrameCount,
            preShutterFrames.Count, "pre-shutter composition schema count");
        var preShutterBindings = document.DisplayFrames!
            .Where(pair => preShutterFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreShutterVersion,
                Frames = preShutterFrames,
                DisplayFrames = preShutterBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog preShutterUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        foreach (EnemySpritemapDefinition frame in ShutterVisualDefinitions.Frames())
            AssertTrue(preShutterUpgraded.Spritemaps!.TryGetDisplay(
                    frame.Bank, frame.Pointer, out _),
                $"version-forty-three override inherits shutter frame {frame.Name}");
        document = new EnemySpritemapDocument
        {
            Version = EnemySpritemapDefinitions.PreShutterVersion,
            Frames = preShutterFrames,
            DisplayFrames = preShutterBindings,
        };
        var preMorphBallEyeFrames = HistoricalFrames()
            .Where(pair => !pair.Key.StartsWith("morph_eye_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreMorphBallEyeFrameCount,
            preMorphBallEyeFrames.Count, "pre-Morph-Ball-eye composition schema count");
        var preMorphBallEyeBindings = document.DisplayFrames!
            .Where(pair => preMorphBallEyeFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreMorphBallEyeVersion,
                Frames = preMorphBallEyeFrames,
                DisplayFrames = preMorphBallEyeBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog preMorphBallEyeUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        foreach (EnemySpritemapDefinition frame in MorphBallEyeVisualDefinitions.Frames())
            AssertTrue(preMorphBallEyeUpgraded.Spritemaps!.TryGetDisplay(
                    frame.Bank, frame.Pointer, out _),
                $"version-forty-two override inherits eye frame {frame.Name}");
        document = new EnemySpritemapDocument
        {
            Version = EnemySpritemapDefinitions.PreMorphBallEyeVersion,
            Frames = preMorphBallEyeFrames,
            DisplayFrames = preMorphBallEyeBindings,
        };
        var preFaceBlockFrames = HistoricalFrames()
            .Where(pair => !pair.Key.StartsWith("face_block_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreFaceBlockFrameCount,
            preFaceBlockFrames.Count, "pre-face-block composition schema frame count");
        var preFaceBlockBindings = document.DisplayFrames!
            .Where(pair => preFaceBlockFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreFaceBlockVersion,
                Frames = preFaceBlockFrames,
                DisplayFrames = preFaceBlockBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog preFaceBlockUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        foreach (EnemySpritemapDefinition frame in BlueBrinstarFaceBlockVisualDefinitions.Frames())
            AssertTrue(preFaceBlockUpgraded.Spritemaps!.TryGetDisplay(
                    frame.Bank, frame.Pointer, out _),
                $"version-forty-one override inherits face-block frame {frame.Name}");
        document = new EnemySpritemapDocument
        {
            Version = EnemySpritemapDefinitions.PreFaceBlockVersion,
            Frames = preFaceBlockFrames,
            DisplayFrames = preFaceBlockBindings,
        };
        var preKagoFrames = HistoricalFrames()
            .Where(pair => !pair.Key.StartsWith("kago_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreKagoFrameCount,
            preKagoFrames.Count, "pre-Kago composition schema frame count");
        var preKagoBindings = document.DisplayFrames!
            .Where(pair => preKagoFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreKagoVersion,
                Frames = preKagoFrames,
                DisplayFrames = preKagoBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog preKagoUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        foreach (EnemySpritemapDefinition frame in KagoVisualDefinitions.Frames())
            AssertTrue(preKagoUpgraded.Spritemaps!.TryGetDisplay(
                    frame.Bank, frame.Pointer, out _),
                $"version-forty override inherits Kago frame {frame.Name}");
        document = new EnemySpritemapDocument
        {
            Version = EnemySpritemapDefinitions.PreKagoVersion,
            Frames = preKagoFrames,
            DisplayFrames = preKagoBindings,
        };
        var preFlyFrames = HistoricalFrames()
            .Where(pair => !pair.Key.StartsWith("fly_shared_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreFlyFrameCount,
            preFlyFrames.Count, "pre-fly composition schema frame count");
        var preFlyBindings = document.DisplayFrames!
            .Where(pair => preFlyFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreFlyVersion,
                Frames = preFlyFrames,
                DisplayFrames = preFlyBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog preFlyUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        foreach (EnemySpritemapDefinition frame in FlyVisualDefinitions.Frames())
            AssertTrue(preFlyUpgraded.Spritemaps!.TryGetDisplay(
                    frame.Bank, frame.Pointer, out _),
                $"version-thirty-nine override inherits fly frame {frame.Name}");
        document = new EnemySpritemapDocument
        {
            Version = EnemySpritemapDefinitions.PreFlyVersion,
            Frames = preFlyFrames,
            DisplayFrames = preFlyBindings,
        };
        var preSciserFrames = HistoricalFrames()
            .Where(pair => !pair.Key.StartsWith("sciser_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreSciserFrameCount,
            preSciserFrames.Count, "pre-Sciser composition schema frame count");
        var preSciserBindings = document.DisplayFrames!
            .Where(pair => preSciserFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreSciserVersion,
                Frames = preSciserFrames,
                DisplayFrames = preSciserBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog preSciserUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        foreach (EnemySpritemapDefinition frame in SciserVisualDefinitions.Frames())
            AssertTrue(preSciserUpgraded.Spritemaps!.TryGetDisplay(
                    frame.Bank, frame.Pointer, out _),
                $"version-thirty-eight override inherits Sciser {frame.Name}");
        document = new EnemySpritemapDocument
        {
            Version = EnemySpritemapDefinitions.PreSciserVersion,
            Frames = preSciserFrames,
            DisplayFrames = preSciserBindings,
        };
        var preRidleyFrames = HistoricalFrames()
            .Where(pair => !pair.Key.StartsWith("ridley_supplement_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreRidleySupplementFrameCount,
            preRidleyFrames.Count, "pre-Ridley-supplement composition schema frame count");
        var preRidleyBindings = document.DisplayFrames!
            .Where(pair => preRidleyFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreRidleySupplementVersion,
                Frames = preRidleyFrames,
                DisplayFrames = preRidleyBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog preRidleyUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        foreach (EnemySpritemapDefinition frame in RidleySupplementalVisualDefinitions.Frames())
            AssertTrue(preRidleyUpgraded.Spritemaps!.TryGetDisplay(
                    frame.Bank, frame.Pointer, out _),
                $"version-thirty-seven override inherits Ridley supplement {frame.Name}");
        document = new EnemySpritemapDocument
        {
            Version = EnemySpritemapDefinitions.PreRidleySupplementVersion,
            Frames = preRidleyFrames,
            DisplayFrames = preRidleyBindings,
        };
        var preDeadTorizoFrames = HistoricalFrames()
            .Where(pair => !pair.Key.StartsWith("dead_torizo_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreDeadTorizoFrameCount,
            preDeadTorizoFrames.Count, "pre-Dead-Torizo composition schema frame count");
        var preDeadTorizoBindings = document.DisplayFrames!
            .Where(pair => preDeadTorizoFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreDeadTorizoVersion,
                Frames = preDeadTorizoFrames,
                DisplayFrames = preDeadTorizoBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog preDeadTorizoUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        AssertTrue(preDeadTorizoUpgraded.Spritemaps!.TryGetDisplay(
                DeadTorizoArtworkDefinitions.SpritemapBank,
                DeadTorizoArtworkDefinitions.HookSpritemap, out _),
            "version-thirty-six override inherits Dead Torizo corpse frame");
        document = new EnemySpritemapDocument
        {
            Version = EnemySpritemapDefinitions.PreDeadTorizoVersion,
            Frames = preDeadTorizoFrames,
            DisplayFrames = preDeadTorizoBindings,
        };
        var preMotherBrainFrames = HistoricalFrames()
            .Where(pair => !pair.Key.StartsWith("mother_brain_a9_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreMotherBrainFrameCount,
            preMotherBrainFrames.Count, "pre-Mother-Brain composition schema frame count");
        var preMotherBrainBindings = document.DisplayFrames!
            .Where(pair => preMotherBrainFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreMotherBrainVersion,
                Frames = preMotherBrainFrames,
                DisplayFrames = preMotherBrainBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog preMotherBrainUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        foreach (EnemySpritemapDefinition frame in MotherBrainVisualDefinitions.Frames())
            AssertTrue(preMotherBrainUpgraded.Spritemaps!.TryGetDisplay(
                    frame.Bank, frame.Pointer, out _),
                $"version-thirty-five override inherits Mother Brain frame {frame.Name}");
        // All earlier migration fixtures start from the previous complete schema.
        document = new EnemySpritemapDocument
        {
            Version = EnemySpritemapDefinitions.PreMotherBrainVersion,
            Frames = preMotherBrainFrames,
            DisplayFrames = preMotherBrainBindings,
        };
        var preHunterFrames = HistoricalFrames()
            .Where(pair => !pair.Key.StartsWith("ki_hunter_a8_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreKiHunterFrameCount,
            preHunterFrames.Count, "pre-KiHunter composition schema frame count");
        var preHunterBindings = document.DisplayFrames!
            .Where(pair => preHunterFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreKiHunterVersion,
                Frames = preHunterFrames,
                DisplayFrames = preHunterBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog preHunterUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        foreach (ushort pointer in installedHunterPointers)
        {
            AssertTrue(preHunterUpgraded.Spritemaps!.TryGetDisplay(
                    KiHunterVisualDefinitions.Bank, pointer, out _),
                $"version-thirty-four override inherits KiHunter frame ${pointer:X4}");
        }
        var preMawFrames = HistoricalFrames()
            .Where(pair => !pair.Key.StartsWith("yapping_maw_a8_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("ki_hunter_a8_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreYappingMawFrameCount,
            preMawFrames.Count, "pre-Yapping-Maw composition schema frame count");
        var preMawBindings = document.DisplayFrames!
            .Where(pair => preMawFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreYappingMawVersion,
                Frames = preMawFrames,
                DisplayFrames = preMawBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog preMawUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        foreach (ushort pointer in installedMawPointers)
        {
            AssertTrue(preMawUpgraded.Spritemaps!.TryGetDisplay(
                    YappingMawVisualDefinitions.Bank, pointer, out _),
                $"version-thirty-three override inherits Yapping Maw frame ${pointer:X4}");
        }
        var preRoomSpriteFrames = HistoricalFrames()
            .Where(pair => !pair.Key.StartsWith("room_sprite_b4_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("yapping_maw_a8_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("ki_hunter_a8_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreRoomSpriteObjectFrameCount,
            preRoomSpriteFrames.Count, "pre-shared-sprite-object composition schema frame count");
        var preRoomSpriteBindings = document.DisplayFrames!
            .Where(pair => preRoomSpriteFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreRoomSpriteObjectVersion,
                Frames = preRoomSpriteFrames,
                DisplayFrames = preRoomSpriteBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog preRoomSpriteUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        foreach (EnemySpritemapDefinition frame in EnemySpritemapDefinitions.Frames)
        {
            if (!frame.Name.StartsWith("room_sprite_b4_", StringComparison.Ordinal))
                continue;
            AssertTrue(preRoomSpriteUpgraded.Spritemaps!.TryGetDisplay(
                    EnemySpritemapDefinitions.RoomSpriteObjectBank, frame.Pointer, out _),
                $"version-thirty-two override inherits stock sprite-object frame {frame.Name}");
        }
        var preBreathFrames = preRoomSpriteFrames
            .Where(pair => !pair.Key.StartsWith("draygon_breath_bubble_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreDraygonBreathFrameCount,
            preBreathFrames.Count, "pre-Draygon-breath composition schema frame count");
        var preBreathBindings = preRoomSpriteBindings
            .Where(pair => preBreathFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreDraygonBreathVersion,
                Frames = preBreathFrames,
                DisplayFrames = preBreathBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog preBreathUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        foreach (ushort pointer in new ushort[]
                 { 0xc920, 0xc927, 0xc938, 0xc949, 0xc95a, 0xc96b, 0xc97c, 0xc98d, 0xc999 })
        {
            AssertTrue(preBreathUpgraded.Spritemaps!.TryGetDisplay(
                    EnemySpritemapDefinitions.RoomSpriteObjectBank, pointer, out _),
                $"version-thirty-one override inherits stock Draygon breath frame ${pointer:X4}");
        }
        var preDraygonFrames = preBreathFrames
            .Where(pair => !pair.Key.StartsWith("draygon_intro_evir_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreDraygonIntroFrameCount,
            preDraygonFrames.Count, "pre-Draygon composition schema frame count");
        var preDraygonBindings = preBreathBindings
            .Where(pair => preDraygonFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreDraygonIntroVersion,
                Frames = preDraygonFrames,
                DisplayFrames = preDraygonBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog preDraygonUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        foreach (ushort pointer in new ushort[] { 0xdb42, 0xdb80, 0xdbbe, 0xdbfc })
        {
            AssertTrue(preDraygonUpgraded.Spritemaps!.TryGetDisplay(
                    EnemySpritemapDefinitions.RoomSpriteObjectBank, pointer, out _),
                $"version-thirty override inherits stock Draygon intro frame ${pointer:X4}");
        }
        var preElevatorFrames = preDraygonFrames
            .Where(pair => !pair.Key.StartsWith("elevator_platform_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreElevatorFrameCount,
            preElevatorFrames.Count, "pre-elevator composition schema frame count");
        var preElevatorBindings = document.DisplayFrames!
            .Where(pair => preElevatorFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreElevatorVersion,
                Frames = preElevatorFrames,
                DisplayFrames = preElevatorBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog preElevatorUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        AssertTrue(DrawEnemy(preElevatorUpgraded, new FrameReadGuard(rom), 0x962f,
                RoomEnemySystem.ElevatorDefinition).LowTable
            .SequenceEqual(DrawEnemy(stock, new FrameReadGuard(rom), 0x962f,
                RoomEnemySystem.ElevatorDefinition).LowTable),
            "version-twenty-nine override inherits stock elevator OAM without ROM reads");
        var preKamerFrames = preDraygonFrames
            .Where(pair => !pair.Key.StartsWith("kamer_platform_", StringComparison.Ordinal))
            .Where(pair => !pair.Key.StartsWith("elevator_platform_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreKamerFrameCount,
            preKamerFrames.Count, "pre-Kamer composition schema frame count");
        var preKamerBindings = document.DisplayFrames!
            .Where(pair => preKamerFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreKamerVersion,
                Frames = preKamerFrames,
                DisplayFrames = preKamerBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog preKamerUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        AssertTrue(DrawEnemy(preKamerUpgraded, new FrameReadGuard(rom), 0xf468,
                RoomEnemySystem.KamerVerticalPlatformDefinition).LowTable
            .SequenceEqual(DrawEnemy(stock, new FrameReadGuard(rom), 0xf468,
                RoomEnemySystem.KamerVerticalPlatformDefinition).LowTable),
            "version-twenty-eight override inherits stock Kamer OAM without ROM reads");
        var preFuneFrames = preDraygonFrames
            .Where(pair => !pair.Key.StartsWith("fune_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("namihe_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("kamer_platform_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("elevator_platform_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreFuneNamiheFrameCount,
            preFuneFrames.Count, "pre-Fune/Namihe composition schema frame count");
        var preFuneBindings = document.DisplayFrames!
            .Where(pair => preFuneFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreFuneNamiheVersion,
                Frames = preFuneFrames,
                DisplayFrames = preFuneBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog preFuneUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        OamBuffer inheritedFune = DrawEnemy(preFuneUpgraded,
            new FrameReadGuard(rom), 0x94cb,
            FuneNamiheDefinitions.FuneEnemyDefinition);
        AssertTrue(inheritedFune.LowTable.SequenceEqual(stockFune.LowTable),
            "version-twenty-seven override inherits stock Fune OAM without ROM reads");
        AssertTrue(DrawEnemy(preFuneUpgraded, new FrameReadGuard(rom), 0x88da,
                RoomEnemySystem.BoyonDefinition).LowTable
            .SequenceEqual(DrawEnemy(edited, new FrameReadGuard(rom), 0x88da,
                RoomEnemySystem.BoyonDefinition).LowTable),
            "version-twenty-seven override retains existing Boyon edits");
        var preSbugFrames = preDraygonFrames
            .Where(pair => !pair.Key.StartsWith("sbug_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("fune_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("namihe_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("kamer_platform_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("elevator_platform_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreSbugFrameCount,
            preSbugFrames.Count, "pre-Sbug composition schema frame count");
        var preSbugBindings = document.DisplayFrames!
            .Where(pair => preSbugFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        preSbugBindings["boyon_idle_0"] = "boyon_idle_1";
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreSbugVersion,
                Frames = preSbugFrames,
                DisplayFrames = preSbugBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog preSbugUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        AssertTrue(preSbugUpgraded.Spritemaps!.TryGet(
                EnemySpritemapDefinitions.SbugBank, sbugPointer, out _),
            "version-twenty-six override inherits stock Sbug artwork");
        AssertTrue(DrawEnemy(preSbugUpgraded, new FrameReadGuard(rom),
                0x88da, RoomEnemySystem.BoyonDefinition).LowTable
            .SequenceEqual(DrawEnemy(stock, new FrameReadGuard(rom),
                0x88e1, RoomEnemySystem.BoyonDefinition).LowTable),
            "version-twenty-six override retains edited display binding");
        var preHZoomerFrames = preDraygonFrames
            .Where(pair => !pair.Key.StartsWith("hzoomer_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("sbug_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("fune_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("namihe_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("kamer_platform_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("elevator_platform_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreHZoomerFrameCount,
            preHZoomerFrames.Count, "pre-HZoomer composition schema frame count");
        var preHZoomerBindings = document.DisplayFrames!
            .Where(pair => preHZoomerFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        preHZoomerBindings["boyon_idle_0"] = "boyon_idle_1";
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreHZoomerVersion,
                Frames = preHZoomerFrames,
                DisplayFrames = preHZoomerBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog preHZoomerUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        OamBuffer retainedPreHZoomerEdit = DrawEnemy(preHZoomerUpgraded,
            new FrameReadGuard(rom), chootPointer, RoomEnemySystem.ChootDefinition);
        AssertEqual(editedChoot.LowTable[1], retainedPreHZoomerEdit.LowTable[1],
            "version-twenty-five override retains edited Choot artwork");
        AssertTrue(DrawEnemy(preHZoomerUpgraded, new FrameReadGuard(rom), 0x88da,
                RoomEnemySystem.BoyonDefinition).LowTable
            .SequenceEqual(DrawEnemy(stock, new FrameReadGuard(rom), 0x88e1,
                RoomEnemySystem.BoyonDefinition).LowTable),
            "version-twenty-five override retains edited display binding");
        AssertTrue(preHZoomerUpgraded.Spritemaps!.TryGet(
                EnemySpritemapDefinitions.HZoomerBank, hzoomerPointer, out _),
            "version-twenty-five override gains stock HZoomer artwork");
        var preChootFrames = preHZoomerFrames
            .Where(pair => !pair.Key.StartsWith("choot_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreChootFrameCount,
            preChootFrames.Count, "pre-Choot composition schema frame count");
        var preChootBindings = preHZoomerBindings
            .Where(pair => preChootFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        preChootBindings["boyon_idle_0"] = "boyon_idle_1";
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreChootVersion,
                Frames = preChootFrames,
                DisplayFrames = preChootBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog preChootUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        OamBuffer retainedPreChootEdit = DrawEnemy(preChootUpgraded,
            new FrameReadGuard(rom), hopperPointer, RoomEnemySystem.SidehopperDefinition);
        AssertEqual(editedHopper.LowTable[1], retainedPreChootEdit.LowTable[1],
            "version-twenty-four override retains edited Sidehopper artwork");
        AssertTrue(DrawEnemy(preChootUpgraded, new FrameReadGuard(rom), 0x88da,
                RoomEnemySystem.BoyonDefinition).LowTable
            .SequenceEqual(DrawEnemy(stock, new FrameReadGuard(rom), 0x88e1,
                RoomEnemySystem.BoyonDefinition).LowTable),
            "version-twenty-four override retains edited display binding");
        AssertTrue(preChootUpgraded.Spritemaps!.TryGet(
                EnemySpritemapDefinitions.ChootBank, chootPointer, out _),
            "version-twenty-four override gains stock Choot artwork");
        var preHopperFrames = preChootFrames
            .Where(pair => !pair.Key.StartsWith("sidehopper_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("dessgeega_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("large_sidehopper_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("large_dessgeega_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreHopperFrameCount,
            preHopperFrames.Count, "pre-Hopper composition schema frame count");
        var preHopperBindings = preChootBindings
            .Where(pair => preHopperFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        preHopperBindings["boyon_idle_0"] = "boyon_idle_1";
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreHopperVersion,
                Frames = preHopperFrames,
                DisplayFrames = preHopperBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog preHopperUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        OamBuffer retainedPreHopperEdit = DrawEnemy(preHopperUpgraded,
            new FrameReadGuard(rom), beetomPointer, RoomEnemySystem.BeetomDefinition);
        AssertEqual(editedBeetom.LowTable[1], retainedPreHopperEdit.LowTable[1],
            "version-twenty-three override retains edited Beetom artwork");
        AssertTrue(DrawEnemy(preHopperUpgraded, new FrameReadGuard(rom), 0x88da,
                RoomEnemySystem.BoyonDefinition).LowTable
            .SequenceEqual(DrawEnemy(stock, new FrameReadGuard(rom), 0x88e1,
                RoomEnemySystem.BoyonDefinition).LowTable),
            "version-twenty-three override retains edited display binding");
        AssertTrue(preHopperUpgraded.Spritemaps!.TryGet(
                EnemySpritemapDefinitions.HopperBank, hopperPointer, out _),
            "version-twenty-three override gains stock Hopper artwork");
        var preBeetomFrames = preHopperFrames
            .Where(pair => !pair.Key.StartsWith("beetom_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreBeetomFrameCount,
            preBeetomFrames.Count, "pre-Beetom composition schema frame count");
        var preBeetomBindings = preHopperBindings
            .Where(pair => !pair.Key.StartsWith("beetom_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        preBeetomBindings["boyon_idle_0"] = "boyon_idle_1";
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreBeetomVersion,
                Frames = preBeetomFrames,
                DisplayFrames = preBeetomBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog preBeetomUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        OamBuffer retainedPreBeetomEdit = DrawEnemy(preBeetomUpgraded,
            new FrameReadGuard(rom), alcoonPointer, RoomEnemySystem.AlcoonDefinition);
        AssertEqual(editedAlcoon.LowTable[1], retainedPreBeetomEdit.LowTable[1],
            "version-twenty-two override retains an edited Alcoon frame");
        AssertTrue(DrawEnemy(preBeetomUpgraded, new FrameReadGuard(rom), 0x88da,
                RoomEnemySystem.BoyonDefinition).LowTable
            .SequenceEqual(DrawEnemy(stock, new FrameReadGuard(rom), 0x88e1,
                RoomEnemySystem.BoyonDefinition).LowTable),
            "version-twenty-two override retains its edited display binding");
        AssertTrue(preBeetomUpgraded.Spritemaps!.TryGet(
                EnemySpritemapDefinitions.BeetomBank, beetomPointer, out _),
            "version-twenty-two override gains stock Beetom artwork");
        var preAlcoonFrames = preBeetomFrames
            .Where(pair => !pair.Key.StartsWith("alcoon_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreAlcoonFrameCount,
            preAlcoonFrames.Count, "pre-Alcoon composition schema frame count");
        var preAlcoonBindings = preBeetomBindings
            .Where(pair => !pair.Key.StartsWith("alcoon_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreAlcoonVersion,
                Frames = preAlcoonFrames,
                DisplayFrames = preAlcoonBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog preAlcoonUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        OamBuffer retainedPreAlcoonEdit = DrawEnemy(preAlcoonUpgraded,
            new FrameReadGuard(rom), bullPointer, RoomEnemySystem.BullDefinition);
        AssertEqual(editedBull.LowTable[1], retainedPreAlcoonEdit.LowTable[1],
            "version-twenty-one override retains an edited Bull frame");
        AssertTrue(preAlcoonUpgraded.Spritemaps!.TryGet(
                EnemySpritemapDefinitions.AlcoonBank, alcoonPointer, out _),
            "version-twenty-one override gains stock Alcoon artwork");
        var preBullFrames = preAlcoonFrames
            .Where(pair => !pair.Key.StartsWith("bull_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreBullFrameCount,
            preBullFrames.Count, "pre-Bull composition schema frame count");
        var preBullBindings = preAlcoonBindings
            .Where(pair => !pair.Key.StartsWith("bull_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreBullVersion,
                Frames = preBullFrames,
                DisplayFrames = preBullBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog preBullUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        OamBuffer retainedPreBullEdit = DrawEnemy(preBullUpgraded,
            new FrameReadGuard(rom), puyoPointer, RoomEnemySystem.PuyoDefinition);
        AssertEqual(editedPuyo.LowTable[1], retainedPreBullEdit.LowTable[1],
            "version-twenty override retains an edited Puyo frame");
        AssertTrue(preBullUpgraded.Spritemaps!.TryGet(
                EnemySpritemapDefinitions.BullBank, bullPointer, out _),
            "version-twenty override gains stock Bull artwork");
        var prePuyoFrames = preBullFrames
            .Where(pair => !pair.Key.StartsWith("puyo_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PrePuyoFrameCount,
            prePuyoFrames.Count, "pre-Puyo composition schema frame count");
        var prePuyoBindings = preBullBindings
            .Where(pair => !pair.Key.StartsWith("puyo_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PrePuyoVersion,
                Frames = prePuyoFrames,
                DisplayFrames = prePuyoBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog prePuyoUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        OamBuffer retainedPrePuyoEdit = DrawEnemy(prePuyoUpgraded,
            new FrameReadGuard(rom), framePointer, RoomEnemySystem.BoyonDefinition);
        AssertEqual(editedOam.LowTable[1], retainedPrePuyoEdit.LowTable[1],
            "version-nineteen override retains an edited older enemy frame");
        AssertTrue(prePuyoUpgraded.Spritemaps!.TryGet(
                EnemySpritemapDefinitions.PuyoBank, puyoPointer, out _),
            "version-nineteen override gains stock Puyo artwork");
        var preNorfairRioFrames = prePuyoFrames
            .Where(pair => !pair.Key.StartsWith("norfair_rio_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreNorfairRioFrameCount,
            preNorfairRioFrames.Count, "pre-Norfair-Rio composition schema frame count");
        var preNorfairRioBindings = prePuyoBindings
            .Where(pair => !pair.Key.StartsWith("norfair_rio_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        preNorfairRioBindings["boyon_idle_0"] = "boyon_idle_1";
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreNorfairRioVersion,
                Frames = preNorfairRioFrames,
                DisplayFrames = preNorfairRioBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog preNorfairRioUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        OamBuffer priorNorfairRioEraRemap = DrawEnemy(preNorfairRioUpgraded,
            new FrameReadGuard(rom), 0x88da, RoomEnemySystem.BoyonDefinition);
        OamBuffer priorNorfairRioEraSelected = DrawEnemy(stock,
            new FrameReadGuard(rom), 0x88e1, RoomEnemySystem.BoyonDefinition);
        AssertTrue(priorNorfairRioEraSelected.LowTable.SequenceEqual(
                priorNorfairRioEraRemap.LowTable),
            "version-eighteen override retains its display binding");
        AssertTrue(preNorfairRioUpgraded.Spritemaps!.TryGet(
                EnemySpritemapDefinitions.NorfairRioBank, norfairRioPointer, out _),
            "version-eighteen override gains stock Norfair Rio artwork");
        var preLowerRioFrames = prePuyoFrames
            .Where(pair => !pair.Key.StartsWith("lower_norfair_rio_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("norfair_rio_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreLowerNorfairRioFrameCount,
            preLowerRioFrames.Count, "pre-Lower-Norfair-Rio composition schema frame count");
        var preLowerRioBindings = prePuyoBindings
            .Where(pair => !pair.Key.StartsWith("lower_norfair_rio_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("norfair_rio_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        preLowerRioBindings["boyon_idle_0"] = "boyon_idle_1";
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreLowerNorfairRioVersion,
                Frames = preLowerRioFrames,
                DisplayFrames = preLowerRioBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog preLowerRioUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        OamBuffer priorLowerRioEraRemap = DrawEnemy(preLowerRioUpgraded,
            new FrameReadGuard(rom), 0x88da, RoomEnemySystem.BoyonDefinition);
        OamBuffer priorLowerRioEraSelected = DrawEnemy(stock,
            new FrameReadGuard(rom), 0x88e1, RoomEnemySystem.BoyonDefinition);
        AssertTrue(priorLowerRioEraSelected.LowTable.SequenceEqual(
                priorLowerRioEraRemap.LowTable),
            "version-seventeen override retains its display binding");
        AssertTrue(preLowerRioUpgraded.Spritemaps!.TryGet(
                EnemySpritemapDefinitions.LowerNorfairRioBank, lowerRioPointer, out _),
            "version-seventeen override gains stock Lower Norfair Rio artwork");
        var preRioFrames = prePuyoFrames
            .Where(pair => !pair.Key.StartsWith("rio_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("lower_norfair_rio_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("norfair_rio_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreRioFrameCount,
            preRioFrames.Count, "pre-Rio composition schema frame count");
        var preRioBindings = prePuyoBindings
            .Where(pair => !pair.Key.StartsWith("rio_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("lower_norfair_rio_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("norfair_rio_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        preRioBindings["boyon_idle_0"] = "boyon_idle_1";
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreRioVersion,
                Frames = preRioFrames,
                DisplayFrames = preRioBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog preRioUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        OamBuffer priorRioEraRemap = DrawEnemy(preRioUpgraded,
            new FrameReadGuard(rom), 0x88da, RoomEnemySystem.BoyonDefinition);
        OamBuffer priorRioEraSelected = DrawEnemy(stock,
            new FrameReadGuard(rom), 0x88e1, RoomEnemySystem.BoyonDefinition);
        AssertTrue(priorRioEraSelected.LowTable.SequenceEqual(priorRioEraRemap.LowTable),
            "version-sixteen override retains its display binding");
        AssertTrue(preRioUpgraded.Spritemaps!.TryGet(
                EnemySpritemapDefinitions.RioBank, rioPointer, out _),
            "version-sixteen override gains stock Rio artwork");
        var preCeresBabyFrames = prePuyoFrames
            .Where(pair => !pair.Key.StartsWith("ceres_baby_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("rio_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("lower_norfair_rio_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("norfair_rio_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreCeresBabyFrameCount,
            preCeresBabyFrames.Count, "pre-Ceres-Baby composition schema frame count");
        var preCeresBabyBindings = prePuyoBindings
            .Where(pair => !pair.Key.StartsWith("ceres_baby_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("rio_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("lower_norfair_rio_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("norfair_rio_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        preCeresBabyBindings["ceres_door_right_hold"] =
            "ceres_door_right_transition_0";
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreCeresBabyVersion,
                Frames = preCeresBabyFrames,
                DisplayFrames = preCeresBabyBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog preCeresBabyUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        OamBuffer priorCeresRemap = DrawEnemy(preCeresBabyUpgraded,
            new FrameReadGuard(rom), 0xfa13,
            CeresDoorInstructionProgramDefinitions.EnemyDefinitionPointer);
        OamBuffer priorCeresSelected = DrawEnemy(stock, new FrameReadGuard(rom),
            0xfa3d, CeresDoorInstructionProgramDefinitions.EnemyDefinitionPointer);
        AssertTrue(priorCeresSelected.LowTable.SequenceEqual(priorCeresRemap.LowTable),
            "version-fifteen override retains its Ceres door display binding");
        AssertTrue(preCeresBabyUpgraded.Spritemaps!.TryGet(
                CeresBabyInstructionProgramDefinitions.Bank,
                CeresBabyInstructionProgramDefinitions.RoundFrame, out _),
            "version-fifteen override gains stock Ceres Baby artwork");

        var preCeresDoorFrames = preCeresBabyFrames
            .Where(pair => !pair.Key.StartsWith("ceres_door_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreCeresDoorFrameCount,
            preCeresDoorFrames.Count, "pre-Ceres-door composition schema frame count");
        var preCeresDoorBindings = preCeresBabyBindings
            .Where(pair => !pair.Key.StartsWith("ceres_door_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        preCeresDoorBindings["boyon_idle_0"] = "boyon_idle_1";
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreCeresDoorVersion,
                Frames = preCeresDoorFrames,
                DisplayFrames = preCeresDoorBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog preCeresDoorUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        OamBuffer priorRemap = DrawEnemy(preCeresDoorUpgraded, new FrameReadGuard(rom),
            0x88da, RoomEnemySystem.BoyonDefinition);
        OamBuffer priorSelected = DrawEnemy(stock, new FrameReadGuard(rom),
            0x88e1, RoomEnemySystem.BoyonDefinition);
        AssertTrue(priorSelected.LowTable.SequenceEqual(priorRemap.LowTable),
            "version-fourteen override retains its edited display binding");
        OamBuffer priorEditedCacatac = DrawEnemy(preCeresDoorUpgraded,
            new FrameReadGuard(rom), cacatacPointer, RoomEnemySystem.CacatacDefinition);
        AssertTrue(editedCacatac.LowTable.SequenceEqual(priorEditedCacatac.LowTable),
            "version-fourteen override also retains its edited frame parts");
        AssertTrue(preCeresDoorUpgraded.Spritemaps!.TryGet(
                CeresDoorInstructionProgramDefinitions.Bank, 0xfa13, out _),
            "version-fourteen override gains stock Ceres door artwork");
        preCeresDoorBindings.Remove("boyon_idle_0");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreCeresDoorVersion,
                Frames = preCeresDoorFrames,
                DisplayFrames = preCeresDoorBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory),
            "version-fourteen override rejects a missing authored display binding");

        var preMagdolliteFrames = preCeresDoorFrames
            .Where(pair => !pair.Key.StartsWith("magdollite_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreMagdolliteFrameCount,
            preMagdolliteFrames.Count, "pre-Magdollite composition schema frame count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreMagdolliteVersion,
                Frames = preMagdolliteFrames,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog preMagdolliteUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        OamBuffer upgradedMagdollite = DrawEnemy(preMagdolliteUpgraded,
            new FrameReadGuard(rom), magdollitePointer,
            RoomEnemySystem.MagdolliteDefinition);
        AssertTrue(stockMagdollite.LowTable.SequenceEqual(upgradedMagdollite.LowTable),
            "version-twelve override gains stock Magdollite composition");
        OamBuffer retainedFireflea = DrawEnemy(preMagdolliteUpgraded,
            new FrameReadGuard(rom), firefleaPointer,
            RoomEnemySystem.FirefleaDefinition);
        AssertEqual(editedFireflea.LowTable[1], retainedFireflea.LowTable[1],
            "version-twelve override retains edited Fireflea composition");
        var preFirefleaFrames = preMagdolliteFrames
            .Where(pair => !pair.Key.StartsWith("fireflea_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreFirefleaFrameCount,
            preFirefleaFrames.Count, "pre-Fireflea composition schema frame count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreFirefleaVersion,
                Frames = preFirefleaFrames,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog preFirefleaUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        OamBuffer upgradedFireflea = DrawEnemy(preFirefleaUpgraded,
            new FrameReadGuard(rom), firefleaPointer, RoomEnemySystem.FirefleaDefinition);
        AssertTrue(stockFireflea.LowTable.SequenceEqual(upgradedFireflea.LowTable),
            "version-eleven override gains stock Fireflea composition");
        OamBuffer retainedRipper = DrawEnemy(preFirefleaUpgraded,
            new FrameReadGuard(rom), 0xe54b, RoomEnemySystem.RipperDefinition);
        OamBuffer editedRipper = DrawEnemy(edited,
            new FrameReadGuard(rom), 0xe54b, RoomEnemySystem.RipperDefinition);
        AssertEqual(editedRipper.LowTable[1], retainedRipper.LowTable[1],
            "version-eleven override retains edited Ripper composition");
        var preRipperFrames = preFirefleaFrames
            .Where(pair => !pair.Key.StartsWith("ripper_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreRipperFrameCount,
            preRipperFrames.Count, "pre-Ripper composition schema frame count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreRipperVersion,
                Frames = preRipperFrames,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog preRipperUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        OamBuffer upgradedRipper = DrawEnemy(preRipperUpgraded, new FrameReadGuard(rom),
            0xe54b, RoomEnemySystem.RipperDefinition);
        OamBuffer stockRipper = DrawEnemy(stock, new FrameReadGuard(rom),
            0xe54b, RoomEnemySystem.RipperDefinition);
        AssertTrue(stockRipper.LowTable.SequenceEqual(upgradedRipper.LowTable),
            "version-ten override gains stock Ripper composition");
        OamBuffer retainedStoke = DrawEnemy(preRipperUpgraded,
            new FrameReadGuard(rom), 0x8aca, RoomEnemySystem.StokeDefinition);
        OamBuffer editedStoke = DrawEnemy(edited, new FrameReadGuard(rom),
            0x8aca, RoomEnemySystem.StokeDefinition);
        AssertEqual(editedStoke.LowTable[1], retainedStoke.LowTable[1],
            "version-ten override retains edited Stoke composition");
        var preOwtchStokeFrames = preRipperFrames
            .Where(pair => !pair.Key.StartsWith("owtch_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("stoke_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreOwtchStokeFrameCount,
            preOwtchStokeFrames.Count, "pre-Owtch/Stoke composition schema frame count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreOwtchStokeVersion,
                Frames = preOwtchStokeFrames,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog preOwtchStokeUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        ushort owtchPointer = OwtchStokeVisualDefinitions.FrameAt(
            RoomEnemySystem.OwtchDefinition, 0xa3af);
        OamBuffer stockOwtch = DrawEnemy(stock, new FrameReadGuard(rom),
            owtchPointer, RoomEnemySystem.OwtchDefinition);
        OamBuffer upgradedOwtch = DrawEnemy(preOwtchStokeUpgraded,
            new FrameReadGuard(rom), owtchPointer, RoomEnemySystem.OwtchDefinition);
        AssertTrue(stockOwtch.LowTable.SequenceEqual(upgradedOwtch.LowTable),
            "version-nine override gains stock Owtch composition");
        var previousFrames = preOwtchStokeFrames
            .Where(pair => !pair.Key.StartsWith("fake_kraid_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("kraid_nail_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreviousFrameCount,
            previousFrames.Count, "previous enemy composition schema frame count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreviousVersion,
                Frames = previousFrames,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog upgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        ushort nailPointer = KraidVisualDefinitions.InitialNailFrame;
        OamBuffer stockNail = DrawEnemy(stock, new FrameReadGuard(rom),
            nailPointer, RoomEnemySystem.KraidGoodNailDefinition);
        OamBuffer upgradedNail = DrawEnemy(upgraded, new FrameReadGuard(rom),
            nailPointer, RoomEnemySystem.KraidGoodNailDefinition);
        AssertTrue(stockNail.LowTable.SequenceEqual(upgradedNail.LowTable),
            "previous-version override gains stock Kraid fingernail composition");
        OamBuffer retainedWaver = DrawEnemy(upgraded, new FrameReadGuard(rom),
            waverPointer, RoomEnemySystem.WaverDefinition);
        AssertEqual(editedWaver.LowTable[1], retainedWaver.LowTable[1],
            "previous-version override retains edited Waver composition");
        ushort retainedPipePointer = PipeBugVisualDefinitions.FrameAt(
            PipeBugDefinitions.NorfairEnemyDefinition, 0x8ae3);
        OamBuffer editedPipe = DrawEnemy(edited, new FrameReadGuard(rom),
            retainedPipePointer, PipeBugDefinitions.NorfairEnemyDefinition);
        OamBuffer retainedPipe = DrawEnemy(upgraded, new FrameReadGuard(rom),
            retainedPipePointer, PipeBugDefinitions.NorfairEnemyDefinition);
        AssertEqual(editedPipe.LowTable[1], retainedPipe.LowTable[1],
            "previous-version override retains edited Pipe Bug composition");
        var priorFrames = previousFrames
            .Where(pair => !pair.Key.StartsWith("pipe_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PriorFrameCount,
            priorFrames.Count, "prior enemy composition schema frame count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PriorVersion,
                Frames = priorFrames,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog priorUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        ushort pipePointer = PipeBugVisualDefinitions.FrameAt(
            PipeBugDefinitions.NorfairEnemyDefinition, 0x8ae3);
        OamBuffer stockPipe = DrawEnemy(stock, new FrameReadGuard(rom),
            pipePointer, PipeBugDefinitions.NorfairEnemyDefinition);
        OamBuffer upgradedPipe = DrawEnemy(priorUpgraded, new FrameReadGuard(rom),
            pipePointer, PipeBugDefinitions.NorfairEnemyDefinition);
        AssertTrue(stockPipe.LowTable.SequenceEqual(upgradedPipe.LowTable),
            "prior-version override gains stock Pipe Bug composition");
        var earlierFrames = priorFrames
            .Where(pair => !pair.Key.StartsWith("zoa_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("metaree_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("skree_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.EarlierFrameCount,
            earlierFrames.Count, "earlier enemy composition schema frame count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.EarlierVersion,
                Frames = earlierFrames,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog earlierUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        var upgradedBoyon = DrawEnemy(earlierUpgraded, new FrameReadGuard(rom),
            framePointer, RoomEnemySystem.BoyonDefinition);
        var upgradedSkultera = DrawEnemy(earlierUpgraded, new FrameReadGuard(rom),
            skulteraPointer, RoomEnemySystem.SkulteraDefinition);
        AssertEqual(editedOam.LowTable[0], upgradedBoyon.LowTable[0],
            "previous-version override retains edited Boyon composition");
        AssertEqual(editedSkultera.LowTable[1], upgradedSkultera.LowTable[1],
            "previous-version override retains edited Skultera composition");
        var upgradedWaver = DrawEnemy(earlierUpgraded, new FrameReadGuard(rom),
            waverPointer, RoomEnemySystem.WaverDefinition);
        AssertEqual(editedWaver.LowTable[1], upgradedWaver.LowTable[1],
            "previous-version override retains edited Waver composition");
        ushort zoaPointer = EnemySpritemapDefinitions.ZoaFrameAt(0xb3c5);
        var stockZoa = DrawEnemy(stock, new FrameReadGuard(rom),
            zoaPointer, RoomEnemySystem.ZoaDefinition);
        var upgradedZoa = DrawEnemy(earlierUpgraded, new FrameReadGuard(rom),
            zoaPointer, RoomEnemySystem.ZoaDefinition);
        AssertTrue(stockZoa.LowTable.SequenceEqual(upgradedZoa.LowTable),
            "previous-version override gains stock Zoa composition");
        var intermediateFrames = earlierFrames
            .Where(pair => !pair.Key.StartsWith("waver_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.IntermediateFrameCount,
            intermediateFrames.Count, "intermediate enemy composition schema frame count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.IntermediateVersion,
                Frames = intermediateFrames,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog intermediateUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        var intermediateBoyon = DrawEnemy(intermediateUpgraded, new FrameReadGuard(rom),
            framePointer, RoomEnemySystem.BoyonDefinition);
        var intermediateWaver = DrawEnemy(intermediateUpgraded, new FrameReadGuard(rom),
            waverPointer, RoomEnemySystem.WaverDefinition);
        AssertEqual(editedOam.LowTable[0], intermediateBoyon.LowTable[0],
            "intermediate override retains edited Boyon composition");
        AssertTrue(stockWaver.LowTable.SequenceEqual(intermediateWaver.LowTable),
            "intermediate override gains stock Waver composition");
        var legacyFrames = intermediateFrames
            .Where(pair => !pair.Key.StartsWith("skultera_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.LegacyFrameCount,
            legacyFrames.Count, "legacy enemy composition schema frame count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.LegacyVersion,
                Frames = legacyFrames,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog legacyUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        var legacyBoyon = DrawEnemy(legacyUpgraded, new FrameReadGuard(rom),
            framePointer, RoomEnemySystem.BoyonDefinition);
        var legacySkultera = DrawEnemy(legacyUpgraded, new FrameReadGuard(rom),
            skulteraPointer, RoomEnemySystem.SkulteraDefinition);
        AssertEqual(editedOam.LowTable[0], legacyBoyon.LowTable[0],
            "legacy override retains edited Boyon composition");
        AssertTrue(stockSkultera.LowTable.SequenceEqual(legacySkultera.LowTable),
            "legacy override gains stock Skultera composition");
        var preBindings = new EnemySpritemapDocument
        {
            Version = EnemySpritemapDefinitions.PreDisplayBindingsVersion,
            Frames = preCeresDoorFrames,
        };
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            preBindings, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog upgradedBindings = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        AssertEqual(editedOam.LowTable[0],
            DrawEnemy(upgradedBindings, new FrameReadGuard(rom), framePointer,
                RoomEnemySystem.BoyonDefinition).LowTable[0],
            "version-thirteen art override retains edits with stock display bindings");

        // A binding may change only the presentation frame. The native pointer,
        // instruction timer, and AI timer must remain untouched by drawing.
        EnemySpritemapDocument remapped = JsonSerializer.Deserialize<EnemySpritemapDocument>(
            original, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        remapped.DisplayFrames!["boyon_idle_0"] = "boyon_idle_1";
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            remapped, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog swappedArt = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        OamBuffer nativeFirst = DrawEnemy(stock, new FrameReadGuard(rom), 0x88da,
            RoomEnemySystem.BoyonDefinition);
        OamBuffer nativeSecond = DrawEnemy(stock, new FrameReadGuard(rom), 0x88e1,
            RoomEnemySystem.BoyonDefinition);
        OamBuffer displayedSecond = DrawEnemy(swappedArt, new FrameReadGuard(rom),
            0x88da, RoomEnemySystem.BoyonDefinition, slot =>
            {
                AssertEqual((ushort)0x88da, slot.SpritemapPointer,
                    "display override retains native collision frame pointer");
                AssertEqual((ushort)7, slot.InstructionTimer,
                    "display override does not move an instruction frame boundary");
                AssertEqual((ushort)9, slot.Timer,
                    "display override does not move an enemy AI timer");
            });
        AssertTrue(!nativeFirst.LowTable.SequenceEqual(nativeSecond.LowTable),
            "selected Boyon test frames are visually distinct");
        AssertTrue(nativeSecond.LowTable.SequenceEqual(displayedSecond.LowTable) &&
                   nativeSecond.HighTable.SequenceEqual(displayedSecond.HighTable),
            "authored frame binding changes live OAM without a ROM visual read");
        AssertTrue(EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory)
                .Spritemaps!.TryGetDisplay(EnemySpritemapDefinitions.BoyonBank,
                    0x88da, out _),
            "display override survives a catalog reload");
        remapped.DisplayFrames["boyon_idle_0"] = "magdollite_left_idle_0";
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            remapped, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory),
            "cross-bank display binding fails loudly");
        remapped.DisplayFrames.Remove("boyon_idle_0");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            remapped, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory),
            "missing display binding fails loudly");
        File.WriteAllText(overridePath, "{\"version\":1,\"version\":1,\"frames\":{}}");
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory),
            "duplicate enemy composition keys fail loudly");
        File.WriteAllBytes(overridePath, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory),
            "malformed enemy composition override fails loudly");

        static OamBuffer DrawEnemy(EnemyTileArtworkCatalog art,
            ISnesAddressSpace guard, ushort pointer, ushort definition,
            Action<RoomEnemySlot>? inspect = null)
        {
            var enemies = new RoomEnemySystem { TileArtwork = art };
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            var queues = (List<ushort>[])typeof(RoomEnemySystem)
                .GetField("_drawQueues", flags)!.GetValue(enemies)!;
            queues[0].Add(0);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = definition;
            slot.Definition = default(RoomEnemyDefinition) with
                { Bank = definition == RoomEnemySystem.YappingMawDefinition
                    ? YappingMawVisualDefinitions.Bank
                    : definition is RoomEnemySystem.KiHunterDefinition or
                        RoomEnemySystem.KiHunterWingsDefinition or
                        RoomEnemySystem.RedKiHunterDefinition or
                        RoomEnemySystem.RedKiHunterWingsDefinition or
                        RoomEnemySystem.GoldKiHunterDefinition or
                        RoomEnemySystem.GoldKiHunterWingsDefinition
                    ? KiHunterVisualDefinitions.Bank
                    : definition == RoomEnemySystem.FakeKraidDefinition
                    ? EnemySpritemapDefinitions.FakeKraidBank
                    : definition is RoomEnemySystem.KraidGoodNailDefinition or
                        RoomEnemySystem.KraidBadNailDefinition
                        ? EnemySpritemapDefinitions.KraidNailBank
                    : definition is PipeBugDefinitions.BrinstarEnemyDefinition or
                    PipeBugDefinitions.StrongBrinstarEnemyDefinition or
                    PipeBugDefinitions.NorfairEnemyDefinition or
                    PipeBugDefinitions.YellowEnemyDefinition
                    ? EnemySpritemapDefinitions.PipeBugBank
                    : definition == CeresDoorInstructionProgramDefinitions.EnemyDefinitionPointer
                        ? CeresDoorInstructionProgramDefinitions.Bank
                    : definition == RoomEnemySystem.BoulderDefinition
                    ? EnemySpritemapDefinitions.BoulderBank
                    : definition == RoomEnemySystem.AtomicDefinition
                        ? EnemySpritemapDefinitions.AtomicBank
                    : definition == RoomEnemySystem.MagdolliteDefinition
                        ? EnemySpritemapDefinitions.MagdolliteBank
                    : definition is FuneNamiheDefinitions.FuneEnemyDefinition or
                        FuneNamiheDefinitions.NamiheEnemyDefinition
                        ? EnemySpritemapDefinitions.FuneNamiheBank
                    : definition == RoomEnemySystem.BullDefinition
                        ? EnemySpritemapDefinitions.BullBank
                    : definition == RoomEnemySystem.AlcoonDefinition
                        ? EnemySpritemapDefinitions.AlcoonBank
                    : definition == RoomEnemySystem.BeetomDefinition
                        ? EnemySpritemapDefinitions.BeetomBank
                    : definition is RoomEnemySystem.SidehopperDefinition or
                        RoomEnemySystem.DessgeegaDefinition or
                        RoomEnemySystem.LargeSidehopperDefinition or
                        RoomEnemySystem.TourianSidehopperDefinition or
                        RoomEnemySystem.LargeDessgeegaDefinition
                        ? EnemySpritemapDefinitions.HopperBank
                    : definition == RoomEnemySystem.ChootDefinition
                        ? EnemySpritemapDefinitions.ChootBank
                    : definition is RoomEnemySystem.MellowDefinition or
                        RoomEnemySystem.MellaDefinition or RoomEnemySystem.MemuDefinition
                        ? FlyVisualDefinitions.Bank
                    : definition == RoomEnemySystem.KagoDefinition
                        ? KagoVisualDefinitions.Bank
                    : definition == RoomEnemySystem.BlueBrinstarFaceBlockDefinition
                        ? BlueBrinstarFaceBlockVisualDefinitions.Bank
                    : definition == RoomEnemySystem.MorphBallEyeDefinition
                        ? MorphBallEyeVisualDefinitions.Bank
                    : definition == RoomEnemySystem.MetroidDefinition
                        ? MetroidVisualDefinitions.Bank
                    : definition == EnemyDefinitionPointers.Mochtroid
                        ? MochtroidVisualDefinitions.Bank
                    : definition == RoomEnemySystem.ShaktoolDefinition
                        ? ShaktoolVisualDefinitions.Bank
                    : definition == ChozoStatueEnemyDefinitions.EnemyDefinitionPointer
                        ? ChozoStatueVisualDefinitions.Bank
                    : definition == RoomEnemySystem.ViolaDefinition
                        ? ViolaVisualDefinitions.Bank
                    : definition == RoomEnemySystem.RinkaDefinition
                        ? RinkaVisualDefinitions.Bank
                    : definition is RoomEnemySystem.TripperDefinition or
                        RoomEnemySystem.KamerDefinition
                        ? TripperKamerVisualDefinitions.Bank
                    : definition is RoomEnemySystem.GrowingShutterDefinition or
                        RoomEnemySystem.ShootableVerticalShutterDefinition or
                        RoomEnemySystem.DestroyableVerticalShutterDefinition or
                        RoomEnemySystem.ShootableHorizontalShutterDefinition
                        ? ShutterVisualDefinitions.Bank
                    : definition is RoomEnemySystem.HZoomerDefinition or
                        RoomEnemySystem.SciserDefinition or
                        RoomEnemySystem.ZeelaDefinition or
                        RoomEnemySystem.SovaDefinition or
                        RoomEnemySystem.ZoomerDefinition or
                        RoomEnemySystem.StoneZoomerDefinition
                        ? EnemySpritemapDefinitions.HZoomerBank
                    : definition is RoomEnemySystem.SbugDefinition or
                        RoomEnemySystem.Sbug2Definition
                        ? EnemySpritemapDefinitions.SbugBank
                        : definition == RoomEnemySystem.ElevatorDefinition ||
                          definition == RoomEnemySystem.SkulteraDefinition ||
                          definition == RoomEnemySystem.WaverDefinition ||
                          definition == RoomEnemySystem.FirefleaDefinition ||
                          definition == RoomEnemySystem.ZoaDefinition ||
                          definition == RoomEnemySystem.MetareeDefinition ||
                          definition == RoomEnemySystem.SkreeDefinition
                            ? EnemySpritemapDefinitions.SkulteraBank
                        : EnemySpritemapDefinitions.BoyonBank };
            slot.SpritemapPointer = pointer;
            slot.InstructionTimer = 7;
            slot.Timer = 9;
            slot.XPosition = 0x0040;
            slot.YPosition = 0x0080;
            var oam = new OamBuffer();
            enemies.DrawLayers(oam, 0, 0, 0, 0);
            inspect?.Invoke(slot);
            return oam;
        }
    }

    private static void VerifyInstalledMotherBrainDrawHook(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog stock)
    {
        foreach (EnemySpritemapDefinition frame in MotherBrainVisualDefinitions.Frames())
        {
            OamBuffer actual = DrawInstalledMotherBrainFrame(
                stock, rom, frame.Pointer, 0x0140, 0x00a0);
            var expected = new OamBuffer();
            expected.AddEnemySpritemap(rom, frame.Bank, frame.Pointer,
                0x0040, 0x0090, 0x0400, 0x0010);
            AssertTrue(actual.LowTable.SequenceEqual(expected.LowTable) &&
                       actual.HighTable.SequenceEqual(expected.HighTable) &&
                       actual.NextByteOffset == expected.NextByteOffset,
                $"Mother Brain private draw uses installed {frame.Name} with native OAM");
        }

        OamBuffer aboveScreen = DrawInstalledMotherBrainFrame(
            stock, rom, 0xa586, 0x0140, 0x000f);
        AssertEqual(0, aboveScreen.NextByteOffset,
            "Mother Brain private draw still culls a head above the viewport");
    }

    private static OamBuffer DrawInstalledMotherBrainFrame(
        EnemyTileArtworkCatalog art, ISnesAddressSpace rom, ushort pointer,
        ushort worldX, ushort worldY)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem { TileArtwork = art };
        typeof(RoomEnemySystem).GetField("_bus", flags)!
            .SetValue(enemies, new MotherBrainFrameReadGuard(rom));
        var oam = new OamBuffer();
        typeof(RoomEnemySystem).GetMethod("DrawMotherBrainWorldSpritemap", flags)!
            .Invoke(enemies,
                [oam, pointer, worldX, worldY,
                    (ushort)0x0400, (ushort)0x0010, (ushort)0x0100, (ushort)0x0010]);
        return oam;
    }

    private static OamBuffer DrawRoomSpriteObject(EnemyTileArtworkCatalog art,
        ISnesAddressSpace bus, RoomSpriteObjectKind kind)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem { TileArtwork = art };
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        var spawn = typeof(RoomEnemySystem).GetMethod("SpawnRoomSpriteObject", flags)!
            .CreateDelegate<Func<RoomEnemySystem, ushort, ushort, RoomSpriteObjectKind,
                ushort, RoomSpriteObjectSlot?>>();
        var draw = typeof(RoomEnemySystem).GetMethod("DrawRoomSpriteObjects", flags)!
            .CreateDelegate<Action<RoomEnemySystem, OamBuffer, ushort, ushort>>();
        AssertTrue(spawn(enemies, 0x0040, 0x0080, kind, 0x0e00) is not null,
            "installed room sprite object allocated");
        var oam = new OamBuffer();
        draw(enemies, oam, 0, 0);
        AssertTrue(oam.NextByteOffset > 0, "installed room sprite object drew OAM");
        return oam;
    }

    private sealed class BankB4ReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address) => (address >> 16) == 0xb4
            ? throw new InvalidOperationException(
                $"Installed room sprite object reread bank-$B4 byte ${address:X6}.")
            : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }

    private sealed class MotherBrainFrameReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0xa9a586 and < 0xa9a7c2 or
                >= 0xa9ad3e and < 0xa9aee4
                ? throw new InvalidOperationException(
                    $"Mother Brain draw reread native artwork byte ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }

    private sealed class FrameReadGuard(ISnesAddressSpace source,
        string? frameName = null) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if (address is >= 0xa288da and < 0xa2890b or
                >= 0xa2a0bb and < 0xa2a377 or
                >= 0xa68a59 and < 0xa68b09 or
                >= 0xa8e489 and < 0xa8e587 or
                >= 0xa3881e and < 0xa388f0 or
                >= 0xa38b65 and < 0xa38c0f or
                >= 0xa3b55f and < 0xa3b5b3 or
                >= 0xa3c842 and < 0xa3c8a6 or
                >= 0xa3928a and < 0xa394aa or
                >= 0xb389b7 and < 0xb389fd or
                >= 0xb38a6d and < 0xb38ac1 or
                >= 0xb38e96 and < 0xb38edc or
                >= 0xb392ad and < 0xb39301 or
                >= 0xa69c64 and < 0xa6a0e0 or
                >= 0xa7a617 and < 0xa7a69f or
                >= 0xa2a589 and < 0xa2a59e or
                >= 0xa28aca and < 0xa28b60 or
                >= 0xa2e3c5 and < 0xa2e457 or
                >= 0xa2e527 and < 0xa2e56f or
                >= 0xa38ea5 and < 0xa3900a or
                >= 0xa8b448 and < 0xa8b65e or
                >= 0xa6f921 and < 0xa6fb72 or
                >= 0xa6a329 and < 0xa6a353 or
                >= 0xa6bffd and < 0xa6c04e or
                >= 0xa29df6 and < 0xa29e80 or
                >= 0xa8db76 and < 0xa8dbb8 or
                >= 0xa8dfa2 and < 0xa8e214 or
                >= 0xa8bed3 and < 0xa8c143 or
                >= 0xa3aee3 and < 0xa3b390 or
                >= 0xa2e146 and < 0xa2e180 or
                >= 0xa3e2e8 and < 0xa3e580 or
                >= 0xa893f9 and < 0xa8959d or
                >= 0xa897b4 and < 0xa899ac or
                >= 0xaadf5c and < 0xaae03d or
                >= 0xa39f29 and < 0xa3a051 or
                >= 0xa2f468 and < 0xa2f498 or
                >= 0xa3962f and < 0xa3965b)
                throw new InvalidOperationException(
                    $"Installed enemy draw for {frameName ?? "an unnamed frame"} read native visual byte ${address:X6}.");
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
