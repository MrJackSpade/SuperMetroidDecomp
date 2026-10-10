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
    // Native cinematic sprite slots: Mother Brain owns slot 0 and the text caret slot 15,
    // so the spawner and its Rinkas live in 1..14. $8B:938A spawns into the highest free
    // slot and $8B:93EF/$8B:9746 walk from the highest slot down.
    private const int SpawnerSlot = 14;
    private const int LowestSlot = 1;

    private readonly IntroDiscoverySprite?[] slots = new IntroDiscoverySprite?[SpawnerSlot + 1];
    private readonly IntroDiscoverySprite spawner;
    private bool spawnsForbidden;

    public IntroRinkaSystem()
    {
        // $8B:AEB8 spawns the invisible spawner while slots 1..14 are all free. Reusing the
        // focused ROM-list interpreter preserves its exact $4A then $80 frame waits.
        spawner = new(0, 0, 0, IntroRinkaDefinitions.SpawnerActor.InstructionList);
        slots[SpawnerSlot] = spawner;
    }

    /// <summary>One descending generic-handler pass over the spawner and Rinka slots.</summary>
    /// <param name="bus">Address space the spawner and Rinka instruction lists step through.</param>
    /// <param name="samus">Samus, whose X position a hitting Rinka crosses before applying its knockback.</param>
    /// <param name="motherBrainExploding">True once Mother Brain's exploding routine deletes the Rinkas.</param>
    /// <param name="explosionsAllocated">
    /// True once Mother Brain's explosions occupy cinematic slots. Their slots then share
    /// this range, so a Rinka spawn would need the whole table to allocate faithfully.
    /// </param>
    public void Step(ISnesAddressSpace bus, SamusState samus, bool motherBrainExploding,
        bool explosionsAllocated)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);

        spawnsForbidden = explosionsAllocated;
        // A slot below the one being processed is still ahead in this pass, so a Rinka the
        // spawner allocates there runs its first pre-instruction and list step immediately.
        for (int slot = SpawnerSlot; slot >= LowestSlot; slot--)
        {
            IntroDiscoverySprite? actor = slots[slot];
            if (actor is null)
                continue;

            if (ReferenceEquals(actor, spawner))
            {
                spawner.Step<IntroRinkaSpawnerInstruction>(HandleSpawnerInstruction,
                    IntroRinkaInstructionDefinitions.ReadWord);
            }
            else
            {
                RunPreInstruction(actor, samus, motherBrainExploding);
                if (actor.IsActive)
                {
                    actor.Step((IntroRinkaInstruction opcode, ushort next) => HandleRinkaInstruction(actor, opcode, next),
                        IntroRinkaInstructionDefinitions.ReadWord);
                }
            }

            // Deletion clears the slot's list pointer, freeing it for the next spawn.
            if (!actor.IsActive)
                slots[slot] = null;
        }
    }

    /// <summary>Adds each visible Rinka in the native descending slot order.</summary>
    public void Draw(OamBuffer oam,
        IntroRinkaSpritePresentation? installedArt = null)
    {
        for (int slot = SpawnerSlot; slot >= LowestSlot; slot--)
        {
            if (slots[slot] is { } actor && !ReferenceEquals(actor, spawner))
                actor.Draw(oam, installedArt: installedArt);
        }
    }

    private ushort HandleSpawnerInstruction(IntroRinkaSpawnerInstruction opcode, ushort next)
    {
        switch (opcode)
        {
            case IntroRinkaSpawnerInstruction.SpawnRinkas0And1:
                Spawn(0);
                Spawn(1);
                return next;

            case IntroRinkaSpawnerInstruction.SpawnRinkas2And3:
                Spawn(2);
                Spawn(3);
                return next;

            default:
                throw new InvalidOperationException($"Undefined IntroRinkaSpawnerInstruction {opcode}.");
        }
    }

    private static ushort HandleRinkaInstruction(
        IntroDiscoverySprite rinka,
        IntroRinkaInstruction opcode,
        ushort next)
    {
        if (opcode != IntroRinkaInstruction.StartMoving)
            throw new InvalidOperationException($"Undefined IntroRinkaInstruction {opcode}.");

        // Init parameter zero is the sole “hits Samus” route. Parameters one through three
        // select the miss routine and remain in GeneralTimer for its velocity table lookup.
        rinka.PreInstructionPointerForDiscovery(
            (ushort)(rinka.GeneralTimer == 0
                ? IntroRinkaPreInstruction.MovingHitsSamus
                : IntroRinkaPreInstruction.MovingMissesSamus));
        return next;
    }

    private void Spawn(int parameter)
    {
        if (spawnsForbidden)
        {
            throw new InvalidOperationException(
                "An intro Rinka spawn while Mother Brain's explosions hold cinematic slots needs the shared slot table.");
        }

        int slot = SpawnerSlot;
        while (slot >= LowestSlot && slots[slot] is not null)
            slot--;
        if (slot < LowestSlot)
            throw new InvalidOperationException("No free cinematic sprite slot for an intro Rinka.");

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
        slots[slot] = rinka;
    }

    private static void RunPreInstruction(
        IntroDiscoverySprite rinka,
        SamusState samus,
        bool motherBrainExploding)
    {
        switch (CinematicInstructionWords.Decode<IntroRinkaPreInstruction>(
                    rinka.PreInstructionPointer, rinka.InstructionPointer))
        {
            case IntroRinkaPreInstruction.None:
            case IntroRinkaPreInstruction.NoOp:
                return;

            case IntroRinkaPreInstruction.MovingHitsSamus:
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

            case IntroRinkaPreInstruction.MovingMissesSamus:
                MoveHalfPixelX(rinka,
                    IntroRinkaDefinitions.Rinka(rinka.GeneralTimer).XWholeVelocity);
                MoveHalfPixelY(rinka);
                short y = unchecked((short)rinka.YPosition);
                if (y < 0x0010 || y >= 0x00d0 || motherBrainExploding)
                    rinka.Delete();
                return;

            default:
                throw new InvalidOperationException($"Undefined IntroRinkaPreInstruction {rinka.PreInstructionPointer}.");
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
