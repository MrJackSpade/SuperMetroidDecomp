using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Assets;

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
    /// <param name="samus">Samus, whose X position a hitting Rinka crosses before applying its knockback.</param>
    /// <param name="motherBrainExploding">True once Mother Brain's exploding routine deletes the Rinkas.</param>
    /// <param name="explosionsAllocated">
    /// True once Mother Brain's explosions occupy cinematic slots. Their slots then share
    /// this range, so a Rinka spawn would need the whole table to allocate faithfully.
    /// </param>
    [IndependentFlag("Mother Brain exploding and explosion slots being allocated are separate intro states read each frame.")]
    public void Step(SamusState samus, bool motherBrainExploding,
        bool explosionsAllocated)
    {
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
    public void Draw(ScreenSprites objects,
        IIntroCinematicSpritePresentation? installedArt = null)
    {
        for (int slot = SpawnerSlot; slot >= LowestSlot; slot--)
        {
            if (slots[slot] is { } actor && !ReferenceEquals(actor, spawner))
                actor.Draw(objects, installedArt: installedArt);
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

            case CinematicSpriteCueKind.SetPreInstruction:
            case CinematicSpriteCueKind.EndZebesExplosion:
            case CinematicSpriteCueKind.ExplosionFinale:
            case CinematicSpriteCueKind.FadeExplosionPalette:
            case CinematicSpriteCueKind.FadeInZebesText:
            case CinematicSpriteCueKind.FadeOutZebesText:
            case CinematicSpriteCueKind.GreyOut:
            case CinematicSpriteCueKind.Launch:
            case CinematicSpriteCueKind.LaunchHead:
            case CinematicSpriteCueKind.PrepareHead:
            case CinematicSpriteCueKind.Shoot:
            case CinematicSpriteCueKind.SpawnClearTime:
            case CinematicSpriteCueKind.SpawnColon:
            case CinematicSpriteCueKind.SpawnCompletedText:
            case CinematicSpriteCueKind.SpawnExplosionSilhouette:
            case CinematicSpriteCueKind.SpawnFinalExplosionWave:
            case CinematicSpriteCueKind.SpawnHoursTens:
            case CinematicSpriteCueKind.SpawnHoursUnits:
            case CinematicSpriteCueKind.SpawnInitialExplosionWave:
            case CinematicSpriteCueKind.SpawnJapaneseZebesTextIfNeeded:
            case CinematicSpriteCueKind.SpawnMinutesTens:
            case CinematicSpriteCueKind.SpawnMinutesUnits:
            case CinematicSpriteCueKind.SpawnParticles:
            case CinematicSpriteCueKind.SpawnSuitedJump:
            case CinematicSpriteCueKind.SpawnSuitlessJump:
            case CinematicSpriteCueKind.StartFlyingToZebes:
            case CinematicSpriteCueKind.StartIntroPage2:
            case CinematicSpriteCueKind.StartIntroPage3:
            case CinematicSpriteCueKind.StartIntroPage4:
            case CinematicSpriteCueKind.StartIntroPage5:
            case CinematicSpriteCueKind.StartRinkaMoving:
            case CinematicSpriteCueKind.StartZebesExplosion:
            case CinematicSpriteCueKind.TransitionToCredits:
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
        rinka.InstallPreInstruction(
            rinka.GeneralTimer == 0
                ? CinematicSpritePreInstruction.IntroRinkaMovingHitsSamus
                : CinematicSpritePreInstruction.IntroRinkaMovingMissesSamus);
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
            paletteBits: IntroCinematicDefinitions.Objects.DiscoveryPalette.Raw,
            program: IntroRinkaDefinitions.RinkaActor.Program)
        {
            GeneralTimer = (ushort)parameter,
        };
        rinka.InstallPreInstruction(
            IntroRinkaDefinitions.RinkaActor.PreInstruction);
        slots[slot] = rinka;
    }

    /// <summary>Moves a Rinka, publishes Samus knockback on the hit route, or deletes it at its exit bounds.</summary>
    private static void RunPreInstruction(
        IntroDiscoverySprite rinka,
        SamusState samus,
        bool motherBrainExploding)
    {
        switch (rinka.PreInstruction)
        {
            case null:
            case CinematicSpritePreInstruction.NoOp:
                return;

            case CinematicSpritePreInstruction.IntroRinkaMovingHitsSamus:
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

                samus.InvincibilityTimer = WrappingCountdown.Start(11);
                samus.KnockbackTimer = WrappingCountdown.Start(11);
                samus.KnockbackXDirection = Samus.Movement.KnockbackHorizontalDirection.Right;
                rinka.Delete();
                return;

            case CinematicSpritePreInstruction.IntroRinkaMovingMissesSamus:
                MoveHalfPixelX(rinka,
                    IntroRinkaDefinitions.Rinka(rinka.GeneralTimer).XWholeVelocity);
                MoveHalfPixelY(rinka);
                int y = unchecked((short)rinka.WholePosition.Y);
                if (y < 16 || y >= 208 || motherBrainExploding)
                    rinka.Delete();
                return;

            case CinematicSpritePreInstruction.MetroidEgg:
            case CinematicSpritePreInstruction.MetroidEggDeleteAfterCrossFade:
            case CinematicSpritePreInstruction.MetroidEggParticle:
            case CinematicSpritePreInstruction.MetroidEggSlimeDrops:
            case CinematicSpritePreInstruction.BabyMetroidBeingDelivered:
            case CinematicSpritePreInstruction.BabyMetroidBeingExamined:
            case CinematicSpritePreInstruction.IntroMotherBrainCrossFading:
            case CinematicSpritePreInstruction.ConfusedBabyWaitingForHatch:
            case CinematicSpritePreInstruction.ConfusedBabyHatched:
            case CinematicSpritePreInstruction.ConfusedBabyIdling:
            case CinematicSpritePreInstruction.ConfusedBabyDancing:
            case CinematicSpritePreInstruction.CeresStars:
            case CinematicSpritePreInstruction.CeresLargeAsteroids:
            case CinematicSpritePreInstruction.CeresUnderAttack:
            case CinematicSpritePreInstruction.CeresSmallAsteroid:
            case CinematicSpritePreInstruction.CeresPurpleSpaceVortex:
            case CinematicSpritePreInstruction.CeresStationaryVortexNoOp:
            case CinematicSpritePreInstruction.CeresExplosionSpawnerRepeating:
            case CinematicSpritePreInstruction.CeresExplosion:
            case CinematicSpritePreInstruction.Zebes:
            case CinematicSpritePreInstruction.ZebesSlideSceneAway:
            case CinematicSpritePreInstruction.ZebesStarsCompletionWait:
            case CinematicSpritePreInstruction.ZebesStarsCompletionSlideSceneAway:
            case CinematicSpritePreInstruction.ZebesStarsWait:
            case CinematicSpritePreInstruction.ZebesStarsSlideSceneAway:
            case CinematicSpritePreInstruction.SuperMetroidIconTopHalf:
            case CinematicSpritePreInstruction.SuperMetroidIconBottomHalf:
            case CinematicSpritePreInstruction.ZebesExplosionStarsWaitForFlyaway:
            case CinematicSpritePreInstruction.ZebesExplosionStarsMoving:
            case CinematicSpritePreInstruction.RewardSuitlessUpperNoOp:
            case CinematicSpritePreInstruction.RewardBodyFlight:
            case CinematicSpritePreInstruction.RewardHeadFlight:
            case CinematicSpritePreInstruction.RewardLanding:
            default:
                throw new InvalidOperationException($"A Rinka does not run {rinka.PreInstruction}.");
        }
    }

    /// <summary>Advances horizontal whole motion plus a half-pixel fractional carry with native word wrapping.</summary>
    private static void MoveHalfPixelX(IntroDiscoverySprite rinka, int wholeDelta)
    {
        FixedCoordinate x = rinka.Position.X;
        uint subSum = (uint)x.Fraction + IntroRinkaDefinitions.HalfPixel.Fraction;
        int carry = (int)(SnesCarry.OutOfWord(subSum));
        rinka.Position = rinka.Position.WithX(new(
            unchecked((ushort)(x.Whole + wholeDelta + carry)), unchecked((ushort)subSum)));
    }

    /// <summary>Advances the Rinka's vertical half-pixel fraction and carries into its whole position.</summary>
    private static void MoveHalfPixelY(IntroDiscoverySprite rinka)
    {
        FixedCoordinate y = rinka.Position.Y;
        uint subSum = (uint)y.Fraction + IntroRinkaDefinitions.HalfPixel.Fraction;
        rinka.Position = rinka.Position.WithY(new(
            unchecked((ushort)(y.Whole + (SnesCarry.OutOfWord(subSum)))), unchecked((ushort)subSum)));
    }
}
