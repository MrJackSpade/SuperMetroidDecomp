using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The four Rinkas (the ring-shaped “Cheerios”) fired during the Mother Brain flashback.
/// </summary>
/// <remarks>
/// This translates the Rinka and spawner definitions and their animation
/// lists. Only Rinka zero is scripted to strike Samus; the other three deliberately miss.
/// The hit routine does not subtract health in the retail cinematic. It publishes eleven
/// frames of invincibility and knockback, which makes Samus flash and visibly react.
/// </remarks>
internal sealed class IntroRinkaSystem
{
    /// <summary>Highest freeable cinematic slot; the invisible spawner starts here while Mother Brain owns slot zero.</summary>
    private const int SpawnerSlot = 14;
    /// <summary>Lowest slot available to Rinkas; the pass visits slots 14 down through one.</summary>
    private const int LowestSlot = 1;

    /// <summary>Cinematic slots shared by the spawner and its four Rinkas.</summary>
    internal readonly IntroDiscoverySprite?[] slots = new IntroDiscoverySprite?[SpawnerSlot + 1];
    /// <summary>Invisible actor whose instruction list schedules the two spawn pairs.</summary>
    internal readonly IntroDiscoverySprite spawner;
    /// <summary>Prevents allocating Rinkas after Mother Brain's explosions occupy the shared slot range.</summary>
    private bool spawnsForbidden;

    /// <summary>Creates the invisible spawner in the highest free slot before its timed animation runs.</summary>
    public IntroRinkaSystem()
    {
        // The flashback's setup spawns the invisible spawner while slots 1..14 are all free. Reusing the
        // focused animation interpreter preserves its exact 74 then 128 frame waits.
        spawner = new(new(0, 0), IntroShades.Drawn(IntroSpriteColouring.DiscoveryActors), IntroRinkaDefinitions.SpawnerActor.Program);
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
                RunFrameBehavior(actor, samus, motherBrainExploding);
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

    /// <summary>Adds each visible Rinka in descending slot order.</summary>
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

            case CinematicSpriteCueKind.SetFrameBehavior:
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
        rinka.InstallFrameBehavior(
            rinka.GeneralTimer == 0
                ? CinematicSpriteFrameBehavior.IntroRinkaMovingHitsSamus
                : CinematicSpriteFrameBehavior.IntroRinkaMovingMissesSamus);
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

        IntroRinkaPhysicalDefinition definition = IntroRinkaLaunches.Rinka(parameter);
        var rinka = new IntroDiscoverySprite(
            definition.Position,
            colouring: IntroShades.Drawn(IntroSpriteColouring.DiscoveryActors),
            program: IntroRinkaDefinitions.RinkaActor.Program)
        {
            GeneralTimer = parameter,
        };
        rinka.InstallFrameBehavior(
            IntroRinkaDefinitions.RinkaActor.FrameBehavior);
        slots[slot] = rinka;
    }

    /// <summary>Moves a Rinka, publishes Samus knockback on the hit route, or deletes it at its exit bounds.</summary>
    private static void RunFrameBehavior(
        IntroDiscoverySprite rinka,
        SamusState samus,
        bool motherBrainExploding)
    {
        switch (rinka.FrameBehavior)
        {
            case null:
            case CinematicSpriteFrameBehavior.NoOp:
                return;

            case CinematicSpriteFrameBehavior.IntroRinkaMovingHitsSamus:
                MoveHalfPixelX(rinka, IntroRinkaLaunches.Rinka(rinka.GeneralTimer).XWholeVelocity);
                MoveHalfPixelY(rinka);

                // The Rinka compares (Rinka X + 8) against (Samus X - 5). Until that crossing,
                // the actor survives unless Mother Brain has entered her exploding routine.
                if (CoordinateAxis.AsSigned(rinka.WholePosition.X + 8) <
                    CoordinateAxis.Difference(samus.Position.X.Whole, 5))
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

            case CinematicSpriteFrameBehavior.IntroRinkaMovingMissesSamus:
                MoveHalfPixelX(rinka,
                    IntroRinkaLaunches.Rinka(rinka.GeneralTimer).XWholeVelocity);
                MoveHalfPixelY(rinka);
                int y = CoordinateAxis.AsSigned(rinka.WholePosition.Y);
                if (y < 16 || y >= 208 || motherBrainExploding)
                    rinka.Delete();
                return;

            case CinematicSpriteFrameBehavior.MetroidEgg:
            case CinematicSpriteFrameBehavior.MetroidEggDeleteAfterCrossFade:
            case CinematicSpriteFrameBehavior.MetroidEggParticle:
            case CinematicSpriteFrameBehavior.MetroidEggSlimeDrops:
            case CinematicSpriteFrameBehavior.BabyMetroidBeingDelivered:
            case CinematicSpriteFrameBehavior.BabyMetroidBeingExamined:
            case CinematicSpriteFrameBehavior.IntroMotherBrainCrossFading:
            case CinematicSpriteFrameBehavior.ConfusedBabyWaitingForHatch:
            case CinematicSpriteFrameBehavior.ConfusedBabyHatched:
            case CinematicSpriteFrameBehavior.ConfusedBabyIdling:
            case CinematicSpriteFrameBehavior.ConfusedBabyDancing:
            case CinematicSpriteFrameBehavior.CeresStars:
            case CinematicSpriteFrameBehavior.CeresLargeAsteroids:
            case CinematicSpriteFrameBehavior.CeresUnderAttack:
            case CinematicSpriteFrameBehavior.CeresSmallAsteroid:
            case CinematicSpriteFrameBehavior.CeresPurpleSpaceVortex:
            case CinematicSpriteFrameBehavior.CeresStationaryVortexNoOp:
            case CinematicSpriteFrameBehavior.CeresExplosionSpawnerRepeating:
            case CinematicSpriteFrameBehavior.CeresExplosion:
            case CinematicSpriteFrameBehavior.Zebes:
            case CinematicSpriteFrameBehavior.ZebesSlideSceneAway:
            case CinematicSpriteFrameBehavior.ZebesStarsCompletionWait:
            case CinematicSpriteFrameBehavior.ZebesStarsCompletionSlideSceneAway:
            case CinematicSpriteFrameBehavior.ZebesStarsWait:
            case CinematicSpriteFrameBehavior.ZebesStarsSlideSceneAway:
            case CinematicSpriteFrameBehavior.SuperMetroidIconTopHalf:
            case CinematicSpriteFrameBehavior.SuperMetroidIconBottomHalf:
            case CinematicSpriteFrameBehavior.ZebesExplosionStarsWaitForFlyaway:
            case CinematicSpriteFrameBehavior.ZebesExplosionStarsMoving:
            case CinematicSpriteFrameBehavior.RewardSuitlessUpperNoOp:
            case CinematicSpriteFrameBehavior.RewardBodyFlight:
            case CinematicSpriteFrameBehavior.RewardHeadFlight:
            case CinematicSpriteFrameBehavior.RewardLanding:
            default:
                throw new InvalidOperationException($"A Rinka does not run {rinka.FrameBehavior}.");
        }
    }

    /// <summary>
    /// Moves the Rinka horizontally by its whole velocity plus half a pixel, as one 16.16 displacement: every second
    /// update the half pixels add up to one more whole pixel, and the position wraps at its width.
    /// </summary>
    private static void MoveHalfPixelX(IntroDiscoverySprite rinka, int wholeDelta)
    {
        FixedCoordinate x = rinka.Position.X;
        SignedSixteenSixteen displacement = SignedSixteenSixteen.FromParts(wholeDelta, 0) + IntroRinkaDefinitions.HalfPixel;
        rinka.Position = rinka.Position.WithX(displacement.AddTo(x.Whole, x.Fraction));
    }

    /// <summary>Moves the Rinka down half a pixel: every second update the half pixels add up to one whole pixel.</summary>
    private static void MoveHalfPixelY(IntroDiscoverySprite rinka)
    {
        FixedCoordinate y = rinka.Position.Y;
        rinka.Position = rinka.Position.WithY(IntroRinkaDefinitions.HalfPixel.AddTo(y.Whole, y.Fraction));
    }
}
