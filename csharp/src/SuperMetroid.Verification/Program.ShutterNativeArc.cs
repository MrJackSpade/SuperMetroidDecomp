using System.Buffers.Binary;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>Minimal native WRAM seed for the actual room's frame-117 bomb ascent.</summary>
    private static void ExportShutterBombArcSeed(SuperMetroidRuntime runtime)
    {
        var ram = new byte[0x20000];
        var samus = runtime.Samus!;
        var k = samus.Kinematics;
        var speed = samus.HorizontalSpeed;
        var level = runtime.LevelData!;
        Word(0x7A5, level.WidthInBlocks);
        Word(0x7A7, level.HeightInBlocks);
        for (int i = 0; i < level.WidthInBlocks * level.HeightInBlocks; i++)
        {
            var block = level.GetCollisionBlockByIndex(i);
            Word(0x10002 + i * 2, block.LevelWord);
            ram[0x16402 + i] = block.Behavior;
        }
        Word(0xAF6, k.XPosition); Word(0xAF8, k.XSubposition);
        Word(0xAFA, k.YPosition); Word(0xAFC, k.YSubposition);
        Word(0xAFE, k.XRadius); Word(0xB00, k.YRadius);
        Word(0xA1C, samus.Pose);
        // Native pose record carries direction/movement type in its first two bytes.
        int pose = 0x91B629 + samus.Pose * 8;
        ram[0xA1E] = runtime.AddressSpace.ReadByte(pose);
        ram[0xA1F] = runtime.AddressSpace.ReadByte(pose + 1);
        Word(0xA56, samus.BombJumpDirection); Word(0xA58, 0xE032); Word(0xA60, 0xE90E);
        Word(0xB2C, k.YSubspeed); Word(0xB2E, k.YSpeed);
        Word(0xB32, k.YSubacceleration); Word(0xB34, k.YAcceleration); Word(0xB36, k.YDirection);
        Word(0xB42, speed.ExtraRunSpeed); Word(0xB44, speed.ExtraRunSubspeed);
        Word(0xB46, speed.BaseSpeed); Word(0xB48, speed.BaseSubspeed); Word(0xB4A, speed.AccelerationMode);
        Word(0x195E, 0xFFFF); Word(0x1962, 0xFFFF);
        Word(0x5B6, runtime.NmiFrameCounter);
        // Retain both solid platforms, even though only slot zero is moving here.
        Word(0x17A6, 4); Word(0x17EC, 0); Word(0x17EE, 64); Word(0x17F0, 0xFFFF);
        for (int i = 0; i < 2; i++)
        {
            int offset = i * 64;
            var slot = runtime.Enemies.Slots[i];
            Word(0xF7A + offset, slot.XPosition); Word(0xF7E + offset, slot.YPosition);
            Word(0xF80 + offset, slot.YSubposition); Word(0xF82 + offset, slot.XRadius);
            Word(0xF84 + offset, slot.YRadius); Word(0xF86 + offset, 0x8000);
        }
        var shutter = runtime.Enemies.VerticalShutterStates[0]!;
        Word(0xFB0, shutter.UpSubvelocity); Word(0xFB2, unchecked((ushort)shutter.UpVelocity));
        Word(0x781E, shutter.MinimumYPosition); Word(0x7810, shutter.MovedUpRestTime);
        File.WriteAllBytes("csharp/test-fixtures/issue-347-repeated-bombs/bomb-arc.wram", ram);

        void Word(int address, int value) => BinaryPrimitives.WriteUInt16LittleEndian(ram.AsSpan(address, 2), unchecked((ushort)value));
    }
}
