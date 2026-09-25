using SuperMetroid.Core.Game;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The four Rinkas (the ring-shaped “Cheerios”) fired during the Mother Brain flashback.
/// </summary>
/// <remarks>
/// This translates definition $8B:CF21, spawner $CF27, and their bank-$8B instruction
/// lists. Only Rinka zero is scripted to strike Samus; the other three deliberately miss.
/// The hit routine does not subtract health in the retail cinematic. It publishes eleven
/// frames of invincibility and knockback, which makes Samus flash and visibly react.
/// </remarks>
internal sealed class IntroRinkaSystem
{
    // The spawner itself is an invisible ordinary cinematic sprite. Reusing the focused
    // ROM-list interpreter preserves the exact $4A then $80 frame waits and avoids a host
    // countdown that would be subtly off by one generic-handler invocation.
    private readonly IntroDiscoverySprite spawner = new(
        0, 0, 0, IntroRinkaDefinitions.SpawnerActor.InstructionList);
    private readonly List<IntroDiscoverySprite> rinkas = [];

    public int ActiveCount => rinkas.Count(static rinka => rinka.IsActive);

    public int SpawnedCount => rinkas.Count;

    /// <summary>Runs pre-instructions first, then each actor's generic list handler.</summary>
    public void Step(ISnesAddressSpace bus, SamusState samus, bool motherBrainExploding)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);

        // Iterate only actors that existed at frame start. The native slot walker has
        // already passed newly allocated Rinka slots when the spawner creates each pair.
        int existingCount = rinkas.Count;
        for (int index = 0; index < existingCount; index++)
        {
            IntroDiscoverySprite rinka = rinkas[index];
            if (!rinka.IsActive)
                continue;

            RunPreInstruction(rinka, samus, motherBrainExploding);
            rinka.Step(bus, (opcode, next) => HandleRinkaInstruction(rinka, opcode, next),
                IntroRinkaInstructionDefinitions.ReadWord);
        }

        spawner.Step(bus, HandleSpawnerInstruction,
            IntroRinkaInstructionDefinitions.ReadWord);
    }

    /// <summary>Adds each visible Rinka with native origin clipping and insertion order.</summary>
    public void Draw(ISnesAddressSpace bus, OamBuffer oam,
        IntroRinkaSpritePresentation? installedArt = null)
    {
        foreach (IntroDiscoverySprite rinka in rinkas)
            rinka.Draw(bus, oam, installedArt: installedArt);
    }

    private ushort? HandleSpawnerInstruction(ushort opcode, ushort next)
    {
        switch (opcode)
        {
            case CinematicCodePointers.Instruction_Spawn_IntroRinkas_0_1:
                Spawn(0);
                Spawn(1);
                return next;

            case CinematicCodePointers.Instruction_Spawn_IntroRinkas_2_3:
                Spawn(2);
                Spawn(3);
                return next;

            default:
                return null;
        }
    }

    private static ushort? HandleRinkaInstruction(
        IntroDiscoverySprite rinka,
        ushort opcode,
        ushort next)
    {
        if (opcode != CinematicCodePointers.Instruction_StartMoving_IntroRinka)
            return null;

        // Init parameter zero is the sole “hits Samus” route. Parameters one through three
        // select the miss routine and remain in GeneralTimer for its velocity table lookup.
        rinka.PreInstructionPointerForDiscovery(
            rinka.GeneralTimer == 0
                ? CinematicCodePointers.PreInstruction_IntroRinka_Moving_HitsSamus
                : CinematicCodePointers.PreInstruction_IntroRinka_Moving_MissesSamus);
        return next;
    }

    private void Spawn(int parameter)
    {
        IntroRinkaPhysicalDefinition definition = IntroRinkaDefinitions.Rinka(parameter);
        var rinka = new IntroDiscoverySprite(
            definition.X,
            definition.Y,
            paletteBits: IntroCinematicRomData.Objects.DiscoveryPalette.Raw,
            instructionPointer: IntroRinkaDefinitions.RinkaActor.InstructionList)
        {
            GeneralTimer = (ushort)parameter,
        };
        rinka.PreInstructionPointerForDiscovery(
            IntroRinkaDefinitions.RinkaActor.PreInstruction);
        rinkas.Add(rinka);
    }

    private static void RunPreInstruction(
        IntroDiscoverySprite rinka,
        SamusState samus,
        bool motherBrainExploding)
    {
        switch (rinka.PreInstructionPointer)
        {
            case 0:
            case IntroRinkaDefinitions.SharedNoOp:
                return;

            case CinematicCodePointers.PreInstruction_IntroRinka_Moving_HitsSamus:
                MoveHalfPixelX(rinka, IntroRinkaDefinitions.Rinka(rinka.GeneralTimer).XWholeVelocity);
                MoveHalfPixelY(rinka);

                // $B90B compares (Rinka X + 8) against (Samus X - 5). Until that crossing,
                // the actor survives unless Mother Brain has entered her exploding routine.
                if (unchecked((short)(rinka.XPosition + 8)) <
                    unchecked((short)(samus.XPosition - 5)))
                {
                    if (motherBrainExploding)
                        rinka.Delete();
                    return;
                }

                samus.InvincibilityTimer = 0x000b;
                samus.KnockbackTimer = 0x000b;
                samus.KnockbackXDirection = 1;
                rinka.Delete();
                return;

            case CinematicCodePointers.PreInstruction_IntroRinka_Moving_MissesSamus:
                MoveHalfPixelX(rinka,
                    IntroRinkaDefinitions.Rinka(rinka.GeneralTimer).XWholeVelocity);
                MoveHalfPixelY(rinka);
                short y = unchecked((short)rinka.YPosition);
                if (y < 0x0010 || y >= 0x00d0 || motherBrainExploding)
                    rinka.Delete();
                return;

            default:
                throw new InvalidDataException(
                    $"Intro Rinka names invalid pre-instruction $8B:{rinka.PreInstructionPointer:X4}.");
        }
    }

    private static void MoveHalfPixelX(IntroDiscoverySprite rinka, int wholeDelta)
    {
        uint subSum = (uint)rinka.XSubPosition + IntroRinkaDefinitions.HalfPixelFraction;
        rinka.XSubPosition = unchecked((ushort)subSum);
        int carry = (int)(subSum >> 16);
        rinka.XPosition = unchecked((ushort)(rinka.XPosition + wholeDelta + carry));
    }

    private static void MoveHalfPixelY(IntroDiscoverySprite rinka)
    {
        uint subSum = (uint)rinka.YSubPosition + IntroRinkaDefinitions.HalfPixelFraction;
        rinka.YSubPosition = unchecked((ushort)subSum);
        rinka.YPosition = unchecked((ushort)(rinka.YPosition + (subSum >> 16)));
    }
}
