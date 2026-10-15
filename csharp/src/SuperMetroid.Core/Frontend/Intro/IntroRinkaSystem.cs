using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Intro;

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
    /// <summary>Highest freeable cinematic slot; the invisible spawner starts here while Mother Brain owns slot zero.</summary>
    private const int SpawnerSlot = 14;
    /// <summary>Lowest slot available to Rinkas; the native pass visits slots 14 down through one.</summary>
    private const int LowestSlot = 1;

    /// <summary>Native cinematic slots shared by the spawner and its four Rinkas.</summary>
    internal readonly IntroDiscoverySprite?[] slots = new IntroDiscoverySprite?[SpawnerSlot + 1];
    /// <summary>Invisible actor whose instruction list schedules the two spawn pairs.</summary>
    internal readonly IntroDiscoverySprite spawner;
    /// <summary>Prevents allocating Rinkas after Mother Brain's explosions occupy the shared slot range.</summary>
    private bool spawnsForbidden;

    /// <summary>Creates the invisible spawner in the highest free native slot before its timed instruction list runs.</summary>
    public IntroRinkaSystem()
    {
        // $8B:AEB8 spawns the invisible spawner while slots 1..14 are all free. Reusing the
        // focused ROM-list interpreter preserves its exact $4A then $80 frame waits.
        spawner = new(new(0, 0), 0, IntroRinkaDefinitions.SpawnerActor.Program);
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
        Ensure.NotNull(bus);
        Ensure.NotNull(samus);

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
                spawner.Step(HandleSpawnerCue);
            }
            else
            {
                RunPreInstruction(actor, samus, motherBrainExploding);
                if (actor.IsActive)
                {
                    actor.Step(cue => HandleRinkaCue(actor, cue));
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

    /// <summary>Handles the two spawner commands, allocating Rinkas zero and one or two and three.</summary>
    private void HandleSpawnerCue(CinematicSpriteCueKind cue)
    {
        switch (cue)
        {
            case CinematicSpriteCueKind.SpawnRinkas0And1:
                Spawn(0);
                Spawn(1);
                return;

            case CinematicSpriteCueKind.SpawnRinkas2And3:
                Spawn(2);
                Spawn(3);
                return;

            default:
                throw new InvalidOperationException($"The Rinka spawner program publishes no {cue} cue.");
        }
    }

    /// <summary>Starts a Rinka's movement, selecting the sole hit route for parameter zero and miss routes for the others.</summary>
    private static void HandleRinkaCue(
        IntroDiscoverySprite rinka,
        CinematicSpriteCueKind cue)
    {
        if (cue != CinematicSpriteCueKind.StartRinkaMoving)
            throw new InvalidOperationException($"The Rinka program publishes no {cue} cue.");

        // Init parameter zero is the sole “hits Samus” route. Parameters one through three
        // select the miss routine and remain in GeneralTimer for its velocity table lookup.
        rinka.PreInstructionPointerForDiscovery(
            (ushort)(rinka.GeneralTimer == 0
                ? IntroRinkaPreInstruction.MovingHitsSamus
                : IntroRinkaPreInstruction.MovingMissesSamus));
    }

    /// <summary>Allocates one definition-selected Rinka in the highest available cinematic slot.</summary>
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
            definition.Position,
            paletteBits: IntroCinematicRomData.Objects.DiscoveryPalette.Raw,
            program: IntroRinkaDefinitions.RinkaActor.Program)
        {
            GeneralTimer = (ushort)parameter,
        };
        rinka.PreInstructionPointerForDiscovery(
            IntroRinkaDefinitions.RinkaActor.PreInstruction);
        slots[slot] = rinka;
    }

    /// <summary>Moves a Rinka, publishes Samus knockback on the hit route, or deletes it at its exit bounds.</summary>
    private static void RunPreInstruction(
        IntroDiscoverySprite rinka,
        SamusState samus,
        bool motherBrainExploding)
    {
        switch (CinematicInstructionWords.DecodePreInstruction<IntroRinkaPreInstruction>(
                    rinka.PreInstructionPointer, rinka.CurrentProgram))
        {
            case IntroRinkaPreInstruction.None:
            case IntroRinkaPreInstruction.NoOp:
                return;

            case IntroRinkaPreInstruction.MovingHitsSamus:
                MoveHalfPixelX(rinka, IntroRinkaDefinitions.Rinka(rinka.GeneralTimer).XWholeVelocity);
                MoveHalfPixelY(rinka);

                // $B90B compares (Rinka X + 8) against (Samus X - 5). Until that crossing,
                // the actor survives unless Mother Brain has entered her exploding routine.
                if (unchecked((short)(rinka.WholePosition.X + 8)) <
                    unchecked((short)(samus.Position.X.Whole - 5)))
                {
                    if (motherBrainExploding)
                        rinka.Delete();
                    return;
                }

                samus.InvincibilityTimer = NativeCountdown.Start(11);
                samus.KnockbackTimer = NativeCountdown.Start(11);
                samus.KnockbackXDirection = 1;
                rinka.Delete();
                return;

            case IntroRinkaPreInstruction.MovingMissesSamus:
                MoveHalfPixelX(rinka,
                    IntroRinkaDefinitions.Rinka(rinka.GeneralTimer).XWholeVelocity);
                MoveHalfPixelY(rinka);
                short y = unchecked((short)rinka.WholePosition.Y);
                if (y < 16 || y >= 208 || motherBrainExploding)
                    rinka.Delete();
                return;

            default:
                throw new InvalidOperationException($"Undefined IntroRinkaPreInstruction {rinka.PreInstructionPointer}.");
        }
    }

    /// <summary>Advances horizontal whole motion plus a half-pixel fractional carry with native word wrapping.</summary>
    private static void MoveHalfPixelX(IntroDiscoverySprite rinka, int wholeDelta)
    {
        SnesFixedPosition x = rinka.Position.X;
        uint subSum = (uint)x.Fraction + IntroRinkaDefinitions.HalfPixelFraction;
        int carry = (int)(SnesCarry.OutOfWord(subSum));
        rinka.Position = rinka.Position.WithX(new(
            unchecked((ushort)(x.Whole + wholeDelta + carry)), unchecked((ushort)subSum)));
    }

    /// <summary>Advances the Rinka's vertical half-pixel fraction and carries into its whole position.</summary>
    private static void MoveHalfPixelY(IntroDiscoverySprite rinka)
    {
        SnesFixedPosition y = rinka.Position.Y;
        uint subSum = (uint)y.Fraction + IntroRinkaDefinitions.HalfPixelFraction;
        rinka.Position = rinka.Position.WithY(new(
            unchecked((ushort)(y.Whole + (SnesCarry.OutOfWord(subSum)))), unchecked((ushort)subSum)));
    }
}
