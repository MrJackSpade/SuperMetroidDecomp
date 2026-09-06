using System.Globalization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rendering;

/// <summary>Reads the reported retail sand rooms and verifies their real animation bytes and cadence.</summary>
internal static class SandRoomAudit
{
    public static int Run(string romPath, bool verifyPhysics = false)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var rooms = File.ReadLines("upstream-sm/assets/names.txt")
            .Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Where(fields => fields.Length == 2 && fields[0].StartsWith("0x8f") && fields[1].StartsWith("kRoom_"))
            .Select(fields => ushort.TryParse(fields[1][6..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ushort p) ? p : (ushort)0)
            .Where(p => p != 0 && p != 0xe82c).Distinct()
            .Select(p => CartridgeRoomHeader.Load(bus, p))
            .Where(room => room.AreaIndex == AreaId.Maridia && room.RoomIndex is 0x1a or 0x24).ToArray();
        if (rooms.Length != 2) throw new InvalidDataException("Expected both reported Maridia sand rooms.");
        foreach (var room in rooms)
        {
            var runtime = new SuperMetroidRuntime(bus);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RunNmi(0, true);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(room.Pointer, 0, 0);
            runtime.Samus!.InputLocked = true;
            var level = runtime.LevelData!;
            if (verifyPhysics && room.RoomIndex == 0x1a)
            {
                // The first surface block is outside the falling-sand columns. Place the
                // bottom sample inside that real block, not on an artificial solid floor.
                var surface = Enumerable.Range(0, level.ForegroundEntries.Length)
                    .Select(i => level.GetCollisionBlockOrPrefilledSolid(i % level.WidthInBlocks, i / level.WidthInBlocks))
                    .First(b => b.CollisionType == RoomCollisionType.SpecialAir && b.Behavior == 0x82);
                var samus = runtime.Samus!;
                samus.InputLocked = false;
                samus.Pose = 1;
                samus.InitializeAnimation(bus);
                samus.EquippedItems = 0;
                samus.Kinematics.YRadius = 16;
                samus.XPosition = (ushort)((surface.Index % level.WidthInBlocks) * 16 + 8);
                samus.YPosition = (ushort)((surface.Index / level.WidthInBlocks) * 16 - 15);
                samus.Kinematics.YDirection = 0;
                samus.Kinematics.YSpeed = 0;
                samus.Kinematics.YSubspeed = 0;
                uint before = samus.Kinematics.YFixed;
                runtime.StepFrame(0);
                Console.WriteLine($"Quicksand surface block {surface.Index}: Y delta={(int)(samus.Kinematics.YFixed - before) / 65536.0:F6}, extra Y={samus.Kinematics.ExtraYFixed / 65536.0:F6}");
                // $84:B447 selects $0120 in 8.8 fixed point for a stationary body
                // without Gravity Suit. The inside-block handler must publish it in alpha.
                if (samus.Kinematics.ExtraYFixed != 0x12000)
                    throw new InvalidDataException("Quicksand alpha did not publish the cartridge's 1.125-pixel sinking displacement.");
                return 0;
            }
            Console.WriteLine($"Room {room.Identity} ${room.Pointer:X4}: {level.WidthInBlocks}x{level.HeightInBlocks}; sand animations={runtime.SandAnimatedTiles.Count}");
            foreach (var group in Enumerable.Range(0, level.ForegroundEntries.Length)
                .Select(i => level.GetCollisionBlockOrPrefilledSolid(i % level.WidthInBlocks, i / level.WidthInBlocks))
                .Where(b => b.CollisionType is RoomCollisionType.SpecialAir or RoomCollisionType.DoorBlock)
                .GroupBy(b => (b.CollisionType, b.Behavior)))
                Console.WriteLine($"  {group.Key}: {group.Count()} blocks; first={group.First().Index}");
            if (room.RoomIndex == 0x24)
            {
                if (runtime.SandAnimatedTiles.Count != 0)
                    throw new InvalidDataException("$04/$24 has no sand animation bits and must not inherit the preceding room's objects.");
                continue;
            }
            if (runtime.SandAnimatedTiles.Count != 2)
                throw new InvalidDataException($"{room.Identity} did not load both cartridge sand animation objects.");
            var seen = new HashSet<string>();
            for (int frame = 0; frame < 80; frame++)
            {
                runtime.StepFrame(0);
                runtime.RunNmi(0, true);
                VerifyFrame(AnimatedTileObjectPointers.MaridiaSandCeiling, frame);
                VerifyFrame(AnimatedTileObjectPointers.MaridiaSandFalling, frame);
                seen.Add(Convert.ToHexString(runtime.Vram.Bytes.Slice(0x2000, 0x60)));
                if (frame == 10)
                {
                    // Compare only the changed sand characters in the exact same rendered
                    // scene; moving enemies or palette effects cannot satisfy this check.
                    var frozenSand = new SnesVram();
                    frozenSand.LoadBytes(0, runtime.Vram.Bytes);
                    frozenSand.ExecuteHardwareDmaWrite(bus, 0x8791e4, 0x40, 0x1000);
                    frozenSand.ExecuteHardwareDmaWrite(bus, 0x879164, 0x20, 0x1020);
                    var ppu = runtime.DisplayedGameplayPpu;
                    Rgba32[] Render(SnesVram vram) => SnesGameplayFrameRenderer.RenderHudOrdinaryBackgroundsAndObjs(
                        vram, runtime.Cgram, runtime.DisplayedOam, ppu.Bg1HorizontalScroll,
                        ppu.Bg1VerticalScroll, ppu.Bg2HorizontalScroll, ppu.Bg2VerticalScroll);
                    var live = Render(runtime.Vram);
                    var frozen = Render(frozenSand);
                    int changedPixels = live.Zip(frozen).Count(pair => pair.First != pair.Second);
                    if (changedPixels == 0) throw new InvalidDataException("Sand character animation changes no visible room pixels.");
                    Console.WriteLine($"  Visible animated sand: {changedPixels} changed pixels with all other render inputs fixed.");
                }
            }
            if (seen.Count != 4) throw new InvalidDataException($"Expected four distinct sand frames, got {seen.Count}.");
            Console.WriteLine("  PASS: two complete 40-frame loops; every VRAM byte matches its ROM frame.");

            void VerifyFrame(ushort definition, int frame)
            {
                ushort Word(int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
                ushort list = Word(0x870000 | definition);
                int size = Word(0x870000 | (definition + 2));
                int destination = Word(0x870000 | (definition + 4)) * 2;
                int entry = list + frame / 10 % 4 * 4;
                if (Word(0x870000 | entry) != 10)
                    throw new InvalidDataException("Retail sand frame duration differs from the verified ten-frame program.");
                ushort source = Word(0x870000 | (entry + 2));
                for (int i = 0; i < size; i++)
                    if (runtime.Vram.ReadByte(destination + i) != bus.ReadByte(0x870000 | (source + i)))
                        throw new InvalidDataException($"Sand ${definition:X4} frame {frame}: wrong character byte {i}.");
            }
        }
        return 0;
    }
}
