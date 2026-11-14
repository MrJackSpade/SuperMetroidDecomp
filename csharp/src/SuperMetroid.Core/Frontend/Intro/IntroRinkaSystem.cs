using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The four Rinkas (the ring-shaped “Cheerios”) fired during the Mother Brain flashback.
/// </summary>
/// <remarks>
/// This models the Rinka and spawner definitions and their animations.
/// Only Rinka zero is scripted to strike Samus; the other three deliberately miss.
/// The hit step does not subtract health in the retail cinematic. It publishes eleven
/// frames of invincibility and knockback, which makes Samus flash and visibly react.
/// The Rinkas are a list in spawn order with no cap (#1625): only the first one launched touches Samus, so the order the
/// others step and draw in is not observable.
/// </remarks>
internal sealed class IntroRinkaSystem
{
    /// <summary>The live Rinkas, in spawn order; a deleted Rinka leaves the list at the end of the pass that deletes it.</summary>
    internal readonly List<IntroRinka> rinkas = [];
    /// <summary>Invisible actor whose animation schedules the two spawn pairs.</summary>
    private readonly IntroDiscoverySprite spawner;
    /// <summary>Whether Mother Brain's explosions have started, after which the spawner's animation has no spawn left.</summary>
    private bool spawnsForbidden;

    /// <summary>Creates the invisible spawner before its timed animation runs.</summary>
    public IntroRinkaSystem()
    {
        // Reusing the focused animation playback preserves the spawner's exact 74 then 128 frame waits.
        spawner = new(new(0, 0), IntroShades.Drawn(IntroSpriteColouring.DiscoveryActors), IntroRinkaDefinitions.SpawnerActor.Animation);
    }

    /// <summary>One update of the spawner, then of every Rinka.</summary>
    /// <param name="samus">Samus, whose X position a hitting Rinka crosses before applying its knockback.</param>
    /// <param name="motherBrainExploding">True once Mother Brain's exploding step deletes the Rinkas.</param>
    /// <param name="explosionsAllocated">
    /// True once Mother Brain's explosions have started. The spawner's animation spawns every Rinka before then, so a
    /// spawn after it is a fault in the cinematic's script.
    /// </param>
    [IndependentFlag("Mother Brain exploding and her explosions having started are separate intro states read each frame.")]
    public void Step(SamusState samus, bool motherBrainExploding,
        bool explosionsAllocated)
    {
        Ensure.NotNull(samus);

        spawnsForbidden = explosionsAllocated;
        // The spawner updates before the Rinkas, so a Rinka it spawns runs its first frame behaviour and animation step in
        // this same update.
        if (spawner.IsActive)
            spawner.Step(HandleSpawnerCue);

        foreach (IntroRinka rinka in rinkas)
        {
            RunFrameBehavior(rinka, samus, motherBrainExploding);
            if (rinka.Sprite.IsActive)
            {
                rinka.Sprite.Step(cue => HandleRinkaCue(rinka, cue));
            }
        }
        _ = rinkas.RemoveAll(static rinka => !rinka.Sprite.IsActive);
    }

    /// <summary>Adds each visible Rinka in spawn order.</summary>
    public void Draw(ScreenSprites objects,
        IIntroCinematicSpritePresentation? installedArt = null)
    {
        foreach (IntroRinka rinka in rinkas)
            rinka.Sprite.Draw(objects, installedArt: installedArt);
    }

    /// <summary>Handles the two spawner cues, launching the striking pair or the missing pair.</summary>
    private void HandleSpawnerCue(CinematicSpriteCueKind cue)
    {
        switch (cue)
        {
            case CinematicSpriteCueKind.SpawnStrikingRinkaPair:
                Spawn(IntroRinkaDefinitions.StrikingPair);
                return;

            case CinematicSpriteCueKind.SpawnMissingRinkaPair:
                Spawn(IntroRinkaDefinitions.MissingPair);
                return;

            case CinematicSpriteCueKind.SetFrameBehavior:
            case CinematicSpriteCueKind.EndZebesExplosion:
            case CinematicSpriteCueKind.ExplosionFinale:
            case CinematicSpriteCueKind.FadeExplosionColours:
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
                throw new InvalidOperationException($"The Rinka spawner's animation publishes no {cue} cue.");
        }
    }

    /// <summary>Starts a Rinka's movement, selecting the hit route for the Rinka that strikes Samus and the miss route for the others.</summary>
    private static void HandleRinkaCue(
        IntroRinka rinka,
        CinematicSpriteCueKind cue)
    {
        if (cue != CinematicSpriteCueKind.StartRinkaMoving)
            throw new InvalidOperationException($"The Rinka's animation publishes no {cue} cue.");

        rinka.Sprite.InstallFrameBehavior(rinka.Flight);
    }

    /// <summary>Spawns the Rinkas of <paramref name="pair"/>, in launch order, after the Rinkas already live.</summary>
    private void Spawn(IReadOnlyList<IntroRinkaLaunch> pair)
    {
        if (spawnsForbidden)
        {
            throw new InvalidOperationException(
                "The intro Rinka spawner spawned a Rinka after Mother Brain's explosions started.");
        }

        foreach (IntroRinkaLaunch launch in pair)
            rinkas.Add(new IntroRinka(launch));
    }

    /// <summary>Moves a Rinka, publishes Samus knockback on the hit route, or deletes it at its exit bounds.</summary>
    private static void RunFrameBehavior(
        IntroRinka rinka,
        SamusState samus,
        bool motherBrainExploding)
    {
        IntroDiscoverySprite sprite = rinka.Sprite;
        switch (sprite.FrameBehavior)
        {
            case null:
            case CinematicSpriteFrameBehavior.NoOp:
                return;

            case CinematicSpriteFrameBehavior.IntroRinkaMovingHitsSamus:
                Drift(sprite, rinka.Drift);

                // The Rinka compares (Rinka X + 8) against (Samus X - 5). Until that crossing,
                // the actor survives unless Mother Brain has entered her exploding step.
                if (CoordinateAxis.AsSigned(sprite.WholePosition.X + 8) <
                    CoordinateAxis.Difference(samus.Position.X.Whole, 5))
                {
                    if (motherBrainExploding)
                        sprite.Delete();
                    return;
                }

                samus.InvincibilityTimer = WrappingCountdown.Start(11);
                samus.KnockbackTimer = 11;
                samus.KnockbackXDirection = Samus.Movement.KnockbackHorizontalDirection.Right;
                sprite.Delete();
                return;

            case CinematicSpriteFrameBehavior.IntroRinkaMovingMissesSamus:
                Drift(sprite, rinka.Drift);
                int y = CoordinateAxis.AsSigned(sprite.WholePosition.Y);
                if (y < 16 || y >= 208 || motherBrainExploding)
                    sprite.Delete();
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
                throw new InvalidOperationException($"A Rinka does not run {sprite.FrameBehavior}.");
        }
    }

    /// <summary>
    /// Moves the Rinka sideways by <paramref name="sideways"/> and down by half a pixel: every second update the half
    /// pixels add up to one more whole pixel, and the position wraps at its width.
    /// </summary>
    private static void Drift(IntroDiscoverySprite rinka, SignedSixteenSixteen sideways)
    {
        FixedCoordinate x = rinka.Position.X;
        rinka.Position = rinka.Position.WithX(sideways.AddTo(x.Whole, x.Fraction));
        FixedCoordinate y = rinka.Position.Y;
        rinka.Position = rinka.Position.WithY(IntroRinkaDefinitions.DownwardDrift.AddTo(y.Whole, y.Fraction));
    }
}
