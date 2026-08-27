using SuperMetroid.Core.Hardware;
using static SuperMetroid.Core.Hardware.SnesAddressMath;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Projectile instruction bytecode, flare animation, draw helpers, and palette reads.
/// </summary>
public sealed partial class SamusProjectileSystem
{
    private bool RunProjectileInstructionHandler(
        ISnesAddressSpace bus,
        SamusProjectileSlot slot)
    {
        slot.InstructionTimer = unchecked((ushort)(slot.InstructionTimer - 1));
        if (slot.InstructionTimer != 0)
            return false;

        ushort pointer = slot.InstructionPointer;
        while (true)
        {
            ushort instructionOrTimer = ReadWord(bus, 0x930000 | pointer);
            if ((instructionOrTimer & 0x8000) == 0)
            {
                slot.InstructionTimer = instructionOrTimer;
                slot.SpritemapPointer = ReadWord(bus, 0x930000 | AddWithinBank(pointer, 2));
                slot.XRadius = bus.ReadByte(0x930000 | AddWithinBank(pointer, 4));
                slot.YRadius = bus.ReadByte(0x930000 | AddWithinBank(pointer, 5));
                slot.AnimationFrame = ReadWord(bus, 0x930000 | AddWithinBank(pointer, 6));
                slot.InstructionPointer = unchecked((ushort)(pointer + 8));
                return false;
            }

            if (instructionOrTimer == ProjectileInstructionDelete)
            {
                ClearProjectile(slot);
                return true;
            }

            if (instructionOrTimer == ProjectileInstructionGoto)
            {
                pointer = ReadWord(bus, 0x930000 | AddWithinBank(pointer, 2));
                continue;
            }

            throw new InvalidOperationException(
                $"Unsupported bank-$93 projectile instruction ${instructionOrTimer:X4} " +
                $"at $93:{pointer:X4}.");
        }
    }

    private void ClearProjectile(SamusProjectileSlot slot)
    {
        slot.ClearFields();
        ProjectileCounter = ProjectileCounter == 0
            ? (ushort)0
            : unchecked((ushort)(ProjectileCounter - 1));
    }

    private static void DrawSlot(
        ISnesAddressSpace bus,
        OamBuffer oam,
        SamusProjectileSlot slot,
        ushort layer1X,
        ushort layer1Y,
        int horizontalMargin)
    {
        short screenX = unchecked((short)(slot.XPosition - layer1X));
        ushort screenY = unchecked((ushort)(slot.YPosition - layer1Y));
        if (screenX < -horizontalMargin || screenX >= 256 + horizontalMargin ||
            (screenY & 0xff00) != 0)
        {
            return;
        }

        oam.AddProjectileSpritemap(
            bus,
            slot.SpritemapPointer,
            unchecked((ushort)screenX),
            screenY);
    }

    private void AdvanceFlareComponent(ISnesAddressSpace bus, int component)
    {
        // The assembly advances only when 16-bit DEC crosses zero into `$FFFF`. A timer
        // value of zero therefore survives one visible call; testing equality here would
        // make every ROM-authored delay one frame too short.
        _flareTimers[component] = unchecked((ushort)(_flareTimers[component] - 1));
        if ((_flareTimers[component] & 0x8000) == 0)
            return;

        ushort frame = unchecked((ushort)(_flareFrames[component] + 1));
        ushort delayList = ReadWord(bus, 0x90c481 + component * 2);
        byte delay = bus.ReadByte(0x900000 | unchecked((ushort)(delayList + frame)));
        if (delay == 0xff)
        {
            frame = 0;
            delay = bus.ReadByte(0x900000 | delayList);
        }
        else if (delay == 0xfe)
        {
            byte rewind = bus.ReadByte(0x900000 | unchecked((ushort)(delayList + frame + 1)));
            frame = unchecked((ushort)(frame - rewind));
            delay = bus.ReadByte(0x900000 | unchecked((ushort)(delayList + frame)));
        }

        _flareFrames[component] = frame;
        _flareTimers[component] = delay;
    }

    private void DrawFlareComponent(
        ISnesAddressSpace bus,
        OamBuffer oam,
        SamusState samus,
        ushort layer1X,
        ushort layer1Y,
        int component,
        SamusMode7Transform? mode7Transform)
    {
        byte direction = ReadPoseByte(bus, samus.Pose, PoseDirectionOffset);
        if (direction is 0xff or 0x10 || (direction & 0xf0) != 0)
            return;

        int directionOffset = (direction & 0x0f) * 2;
        bool running = samus.ReadMovementKind(bus) == SamusMovementType.Running;
        int xTable = running ? 0x90c1dc : 0x90c1a8;
        int yTable = running ? 0x90c1f0 : 0x90c1c2;
        short xOffset = unchecked((short)ReadWord(bus, xTable + directionOffset));
        short yOffset = unchecked((short)ReadWord(bus, yTable + directionOffset));
        byte poseYOffset = ReadPoseByte(bus, samus.Pose, PoseYOffsetOffset);

        // `$90:BBE1` calls `$8B:8A52` under the same Ceres-status high bit used by the
        // body renderer. Only Samus's center is transformed; the pose-selected muzzle
        // offsets are added afterward. Retail then restores `$0AF6/$0AFA`, so this path
        // must never write the public physics coordinates merely to reuse the arithmetic.
        SamusMode7Point renderPoint = mode7Transform is { } transform
            ? transform.Transform(samus.XPosition, samus.YPosition)
            : new SamusMode7Point(samus.XPosition, samus.YPosition);
        ushort screenX = unchecked((ushort)(renderPoint.X + xOffset - layer1X));
        ushort screenY = unchecked((ushort)(renderPoint.Y + yOffset - poseYOffset - layer1Y));

        // `$90:BC98` clips only by the origin's Y high byte. The shared bank-$81 loader
        // deliberately allows individual entries to wrap, matching charge sparks near an
        // edge instead of applying the generic on-screen-origin parking rule.
        if ((screenY & 0xff00) != 0)
            return;

        bool facingLeft = samus.IsFacingLeft(bus);
        ushort indexOffset = unchecked((ushort)(facingLeft
            ? component switch { 0 => 0, 1 => 0x2a, _ => 0x30 }
            : component switch { 0 => 0, 1 => 0x1e, _ => 0x24 }));
        ushort tableIndex = unchecked((ushort)(indexOffset + _flareFrames[component]));
        oam.AddFlareSpritemap(bus, tableIndex, screenX, screenY);
    }

    private void ClearFlareAnimationState()
    {
        Array.Clear(_flareFrames);
        Array.Clear(_flareTimers);
    }

    private static byte ReadPoseByte(ISnesAddressSpace bus, byte pose, int fieldOffset) =>
        bus.ReadByte(PoseDefinitions + pose * 8 + fieldOffset);

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(AddWithinBank(address, 1)) << 8)));

    private static ushort LoadNormalSuitPalette(
        ISnesAddressSpace bus,
        SnesCgram cgram,
        ushort equippedItems)
    {
        // `SuitPaletteIndex` is a byte offset, not an ordinal: Power=0, Varia=2,
        // Gravity=4. Gravity wins when externally stimulated state contains both bits.
        ushort suitOffset = GetSuitPaletteOffset(equippedItems);
        ushort pointer = ReadWord(bus, NormalSuitPalettePointers + suitOffset);
        cgram.LoadFromBus(
            bus,
            0x9b0000 | pointer,
            colorCount: 16,
            destinationIndex: SamusPaletteCgramIndex);
        return pointer;
    }

    private static ushort GetSuitPaletteOffset(ushort equippedItems) =>
        (equippedItems & 0x0020) != 0
            ? (ushort)4
            : (equippedItems & 0x0001) != 0
                ? (ushort)2
                : (ushort)0;

}
