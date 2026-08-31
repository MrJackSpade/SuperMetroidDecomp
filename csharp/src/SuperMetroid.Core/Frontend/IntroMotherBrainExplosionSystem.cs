using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The eight cinematic sprite objects spawned by Mother Brain's fourth intro missile hit.
/// </summary>
/// <remarks>
/// This is not a desktop particle approximation. It is the small common subset of the
/// bank-$8B cinematic-object interpreter used by definitions $CF15 and $CF1B: initializer
/// tables, delayed instruction timers, six cartridge spritemaps, one blank frame, and the
/// $94BC loop opcode. Keeping it separate from Mother Brain makes each native object retain
/// its own timer and instruction pointer, which is what creates the staggered blast pattern.
/// </remarks>
internal sealed class IntroMotherBrainExplosionSystem
{
    private readonly List<ExplosionActor> actors = [];

    /// <summary>All eight actors remain allocated until the later page-two crossfade.</summary>
    public int ActiveCount => actors.Count(actor => actor.IsActive);

    /// <summary>
    /// Replays the eight spawn calls at $8B:B7C6-$B80B in their original order.
    /// </summary>
    public void SpawnFourthHitExplosions()
    {
        if (actors.Count != 0)
            throw new InvalidOperationException("The intro Mother Brain explosions were already spawned.");

        // Despite their adjacent definition addresses, the retail routine explicitly
        // spawns three small actors first and five big actors second. Each initializer uses
        // its parameter as an index into independent position and start-delay tables.
        for (ushort parameter = 0; parameter < 3; parameter++)
            actors.Add(ExplosionActor.CreateSmall(parameter));
        for (ushort parameter = 0; parameter < 5; parameter++)
            actors.Add(ExplosionActor.CreateBig(parameter));
    }

    /// <summary>Advances every allocated object through the exact bank-$8B list format.</summary>
    public void Step(ISnesAddressSpace bus, ushort introCrossfadeTimer)
    {
        ArgumentNullException.ThrowIfNull(bus);
        foreach (ExplosionActor actor in actors)
            actor.Step(bus, introCrossfadeTimer);
    }

    /// <summary>Adds each visible ROM spritemap to the current cinematic OAM frame.</summary>
    public void Draw(ISnesAddressSpace bus, OamBuffer oam)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(oam);
        foreach (ExplosionActor actor in actors)
        {
            if (!actor.IsActive || actor.SpriteMapPointer == 0)
                continue;

            oam.AddOnScreenSpritemap(
                bus,
                0x8c0000 | actor.SpriteMapPointer,
                actor.XPosition,
                actor.YPosition,
                paletteBits: 0x0a00);
        }
    }

    private sealed class ExplosionActor
    {
        private const ushort DeleteInstruction = 0x9438;
        private const ushort GotoInstruction = 0x94bc;
        private const ushort DeleteInstructionList = 0xce53;

        private ushort instructionPointer;
        private ushort instructionTimer;

        private ExplosionActor(
            ushort xPosition,
            ushort yPosition,
            ushort instructionPointer,
            ushort instructionTimer)
        {
            XPosition = xPosition;
            YPosition = yPosition;
            this.instructionPointer = instructionPointer;
            this.instructionTimer = instructionTimer;
        }

        public ushort XPosition { get; }

        public ushort YPosition { get; }

        public ushort SpriteMapPointer { get; private set; }

        public bool IsActive { get; private set; } = true;

        public static ExplosionActor CreateBig(ushort parameter)
        {
            // $8B:B98D indexes five signed offsets from Mother Brain's fixed (56,111)
            // position, then staggers the actors by 1, 16, 32, 48, and 64 frames.
            ReadOnlySpan<short> xOffsets = [0, 16, -16, -8, 8];
            ReadOnlySpan<short> yOffsets = [0, -16, 8, -16, 8];
            ReadOnlySpan<ushort> startTimers = [1, 16, 32, 48, 64];
            if (parameter >= xOffsets.Length)
                throw new ArgumentOutOfRangeException(nameof(parameter));
            return new ExplosionActor(
                AddSigned(0x0038, xOffsets[parameter]),
                AddSigned(0x006f, yOffsets[parameter]),
                instructionPointer: 0xcdab,
                instructionTimer: startTimers[parameter]);
        }

        public static ExplosionActor CreateSmall(ushort parameter)
        {
            // $8B:B9D4 uses its own three-entry layout and shorter 1/8/16-frame stagger.
            ReadOnlySpan<short> xOffsets = [16, -16, -16];
            ReadOnlySpan<short> yOffsets = [0, 4, -8];
            ReadOnlySpan<ushort> startTimers = [1, 8, 16];
            if (parameter >= xOffsets.Length)
                throw new ArgumentOutOfRangeException(nameof(parameter));
            return new ExplosionActor(
                AddSigned(0x0038, xOffsets[parameter]),
                AddSigned(0x006f, yOffsets[parameter]),
                instructionPointer: 0xcdcb,
                instructionTimer: startTimers[parameter]);
        }

        public void Step(ISnesAddressSpace bus, ushort introCrossfadeTimer)
        {
            if (!IsActive)
                return;

            // $8B:BA0F deletes these actors only when the later page-two crossfade timer
            // reaches zero. Writing timer one and list $CE53 before the generic decrement
            // makes deletion occur in this same handler call.
            if (introCrossfadeTimer == 0)
            {
                instructionTimer = 1;
                instructionPointer = DeleteInstructionList;
            }

            instructionTimer = unchecked((ushort)(instructionTimer - 1));
            if (instructionTimer != 0)
                return;

            ushort pointer = instructionPointer;
            while (true)
            {
                ushort instructionOrDuration = ReadWord(bus, pointer);
                if ((instructionOrDuration & 0x8000) == 0)
                {
                    instructionTimer = instructionOrDuration;
                    SpriteMapPointer = ReadWord(bus, Add(pointer, 2));
                    instructionPointer = Add(pointer, 4);
                    return;
                }

                if (instructionOrDuration == GotoInstruction)
                {
                    pointer = ReadWord(bus, Add(pointer, 2));
                    continue;
                }

                if (instructionOrDuration == DeleteInstruction)
                {
                    IsActive = false;
                    SpriteMapPointer = 0;
                    instructionPointer = 0;
                    return;
                }

                throw new InvalidDataException(
                    $"Intro Mother Brain explosion opcode $8B:{instructionOrDuration:X4} at $8B:{pointer:X4} is invalid.");
            }
        }

        private static ushort ReadWord(ISnesAddressSpace bus, ushort pointer) =>
            RomDataReader.ReadWordFixedBank(bus, 0x8b0000 | pointer);

        private static ushort Add(ushort pointer, int byteCount) =>
            unchecked((ushort)(pointer + byteCount));

        private static ushort AddSigned(ushort value, short offset) =>
            unchecked((ushort)(value + offset));
    }
}
