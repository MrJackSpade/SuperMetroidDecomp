using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

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
        for (ushort parameter = 0; parameter < IntroMotherBrainDefinitions.SmallExplosionCount; parameter++)
            actors.Add(ExplosionActor.CreateSmall(parameter));
        for (ushort parameter = 0; parameter < IntroMotherBrainDefinitions.BigExplosionCount; parameter++)
            actors.Add(ExplosionActor.CreateBig(parameter));
    }

    /// <summary>Advances every allocated object through the exact bank-$8B list format.</summary>
    public void Step(ISnesAddressSpace bus, ushort introCrossfadeTimer)
    {
        ArgumentNullException.ThrowIfNull(bus);
        foreach (ExplosionActor actor in actors)
            actor.Step(bus, introCrossfadeTimer);
    }

    /// <summary>Adds each visible spritemap in native actor order to cinematic OAM.</summary>
    public void Draw(OamBuffer oam, IntroMotherBrainExplosionSpritePresentation installedArt)
    {
        ArgumentNullException.ThrowIfNull(oam);
        ArgumentNullException.ThrowIfNull(installedArt);
        foreach (ExplosionActor actor in actors)
        {
            if (!actor.IsActive || actor.SpriteMapPointer == 0)
                continue;

            installedArt.Draw(actor.SpriteMapPointer, oam, actor.XPosition,
                actor.YPosition, IntroCinematicRomData.Objects.ExplosionPalette.Raw);
        }
    }

    /// <summary>One independently timed cinematic sprite object running a native bank-$8B instruction list.</summary>
    private sealed class ExplosionActor
    {
        /// <summary>Next bank-$8B instruction-list word to interpret.</summary>
        private ushort instructionPointer;
        /// <summary>Updates remaining before the current list entry is consumed.</summary>
        private ushort instructionTimer;

        /// <summary>Creates one cinematic object with its initial position and list state.</summary>
        /// <param name="xPosition">Initial room X coordinate.</param>
        /// <param name="yPosition">Initial room Y coordinate.</param>
        /// <param name="instructionPointer">Native instruction-list address to begin executing.</param>
        /// <param name="instructionTimer">Initial delay before reading the first list entry.</param>
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

        /// <summary>Room X coordinate used when drawing the current explosion frame.</summary>
        public ushort XPosition { get; }

        /// <summary>Room Y coordinate used when drawing the current explosion frame.</summary>
        public ushort YPosition { get; }

        /// <summary>Native spritemap pointer selected by the current timed list entry, or zero for a blank frame.</summary>
        public ushort SpriteMapPointer { get; private set; }

        /// <summary>Whether the object remains allocated for stepping and presentation.</summary>
        public bool IsActive { get; private set; } = true;

        /// <summary>Creates a large blast actor at the indexed offset and delay from its native initializer tables.</summary>
        /// <param name="parameter">Initializer-table index for the large explosion.</param>
        /// <returns>The initialized actor using the large explosion instruction list.</returns>
        public static ExplosionActor CreateBig(ushort parameter)
        {
            IntroMotherBrainExplosionPlacement placement =
                IntroMotherBrainDefinitions.BigExplosion(parameter);
            (ushort originX, ushort originY) = IntroMotherBrainDefinitions.MotherBrainOrigin;
            return new ExplosionActor(
                AddSigned(originX, placement.XOffset),
                AddSigned(originY, placement.YOffset),
                instructionPointer: IntroMotherBrainDefinitions.BigExplosionActor.InstructionList,
                instructionTimer: placement.StartTimer);
        }

        /// <summary>Creates a small blast actor at the indexed offset and delay from its native initializer tables.</summary>
        /// <param name="parameter">Initializer-table index for the small explosion.</param>
        /// <returns>The initialized actor using the small explosion instruction list.</returns>
        public static ExplosionActor CreateSmall(ushort parameter)
        {
            IntroMotherBrainExplosionPlacement placement =
                IntroMotherBrainDefinitions.SmallExplosion(parameter);
            (ushort originX, ushort originY) = IntroMotherBrainDefinitions.MotherBrainOrigin;
            return new ExplosionActor(
                AddSigned(originX, placement.XOffset),
                AddSigned(originY, placement.YOffset),
                instructionPointer: IntroMotherBrainDefinitions.SmallExplosionActor.InstructionList,
                instructionTimer: placement.StartTimer);
        }

        /// <summary>Consumes a due list command, updates the displayed spritemap, or retires the actor.</summary>
        /// <param name="bus">Address space required by the actor update contract.</param>
        /// <param name="introCrossfadeTimer">Page-two crossfade timer; zero schedules native deletion.</param>
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
                instructionPointer = CinematicCodePointers.Lists.Delete;
            }

            instructionTimer = unchecked((ushort)(instructionTimer - 1));
            if (instructionTimer != 0)
                return;

            ushort pointer = instructionPointer;
            while (true)
            {
                ushort instructionOrDuration =
                    IntroMotherBrainExplosionInstructionDefinitions.ReadWord(pointer);
                if ((instructionOrDuration & CinematicCodePointers.InstructionCommandBit) == 0)
                {
                    instructionTimer = instructionOrDuration;
                    SpriteMapPointer = IntroMotherBrainExplosionInstructionDefinitions.ReadWord(
                        Add(pointer, 2));
                    instructionPointer = Add(pointer, 4);
                    return;
                }

                if (instructionOrDuration == CinematicCodePointers.CinematicSpriteObject_Instruction_Goto)
                {
                    pointer = IntroMotherBrainExplosionInstructionDefinitions.ReadWord(
                        Add(pointer, 2));
                    continue;
                }

                if (instructionOrDuration == CinematicCodePointers.CinematicSpriteObject_Instruction_Delete)
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

        /// <summary>Advances a bank-local instruction pointer with native 16-bit wrapping.</summary>
        /// <param name="pointer">Current bank offset.</param>
        /// <param name="byteCount">Number of bytes to advance.</param>
        /// <returns>The wrapped 16-bit bank offset.</returns>
        private static ushort Add(ushort pointer, int byteCount) =>
            unchecked((ushort)(pointer + byteCount));

        /// <summary>Adds a signed placement offset to a room coordinate with 16-bit wrapping.</summary>
        /// <param name="value">Unsigned coordinate before applying the offset.</param>
        /// <param name="offset">Signed room-pixel displacement.</param>
        /// <returns>The resulting coordinate represented as an unsigned word.</returns>
        private static ushort AddSigned(ushort value, short offset) =>
            unchecked((ushort)(value + offset));
    }
}
