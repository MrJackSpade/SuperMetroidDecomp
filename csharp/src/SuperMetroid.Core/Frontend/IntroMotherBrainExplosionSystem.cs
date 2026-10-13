using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

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
    /// <summary>Three small and five big actors retained in native spawn and OAM order.</summary>
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
        Ensure.NotNull(bus);
        foreach (ExplosionActor actor in actors)
            actor.Step(introCrossfadeTimer);
    }

    /// <summary>Adds each visible spritemap in native actor order to cinematic OAM.</summary>
    public void Draw(OamBuffer oam, IntroMotherBrainExplosionSpritePresentation installedArt)
    {
        Ensure.NotNull(oam);
        Ensure.NotNull(installedArt);
        foreach (ExplosionActor actor in actors)
        {
            if (!actor.IsActive || actor.SpriteMapPointer == 0)
                continue;

            installedArt.Draw(actor.SpriteMapPointer, oam, actor.Position.ToOam(), IntroCinematicRomData.Objects.ExplosionPalette.Raw);
        }
    }

    /// <summary>One bank-$8B cinematic object's own delayed instruction cursor and visible frame.</summary>
    private sealed class ExplosionActor
    {
        /// <summary>Next native duration or command word to decode.</summary>
        private ushort instructionPointer;
        /// <summary>Remaining handler calls before the next instruction word is read.</summary>
        private ushort instructionTimer;

        /// <summary>Starts one explosion at its authored offset and independent launch delay.</summary>
        private ExplosionActor(
            Position16<ScreenPixels> position,
            ushort instructionPointer,
            ushort instructionTimer)
        {
            Position = position;
            this.instructionPointer = instructionPointer;
            this.instructionTimer = instructionTimer;
        }

        /// <summary>Fixed cinematic screen origin selected by the small or big placement table.</summary>
        public Position16<ScreenPixels> Position { get; }

        /// <summary>Current bank-$8B visual frame pointer, zero before the first timed pose or after deletion.</summary>
        public ushort SpriteMapPointer { get; private set; }

        /// <summary>Cleared by the native delete command after the later crossfade or list completion.</summary>
        public bool IsActive { get; private set; } = true;

        /// <summary>Starts a big explosion from its five-entry position and delay table.</summary>
        public static ExplosionActor CreateBig(ushort parameter)
        {
            IntroMotherBrainExplosionPlacement placement =
                IntroMotherBrainDefinitions.BigExplosion(parameter);
            return new ExplosionActor(
                IntroMotherBrainDefinitions.MotherBrainOrigin + placement.Offset,
                instructionPointer: IntroMotherBrainDefinitions.BigExplosionActor.InstructionList,
                instructionTimer: placement.StartTimer);
        }

        /// <summary>Starts a small explosion from its three-entry position and delay table.</summary>
        public static ExplosionActor CreateSmall(ushort parameter)
        {
            IntroMotherBrainExplosionPlacement placement =
                IntroMotherBrainDefinitions.SmallExplosion(parameter);
            return new ExplosionActor(
                IntroMotherBrainDefinitions.MotherBrainOrigin + placement.Offset,
                instructionPointer: IntroMotherBrainDefinitions.SmallExplosionActor.InstructionList,
                instructionTimer: placement.StartTimer);
        }

        /// <summary>Consumes one cinematic handler call, following timed frames, loop branches, or deletion.</summary>
        public void Step(ushort introCrossfadeTimer)
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

                if (instructionOrDuration == (ushort)CinematicSpriteInstruction.Goto)
                {
                    pointer = IntroMotherBrainExplosionInstructionDefinitions.ReadWord(
                        Add(pointer, 2));
                    continue;
                }

                if (instructionOrDuration == (ushort)CinematicSpriteInstruction.Delete)
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

        /// <summary>Advances a bank-$8B instruction pointer with native sixteen-bit wrap.</summary>
        private static ushort Add(ushort pointer, int byteCount) =>
            unchecked((ushort)(pointer + byteCount));

    }
}
