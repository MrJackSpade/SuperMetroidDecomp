using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The Rinkas (the ring-shaped “Cheerios”) fired during the Mother Brain flashback.
/// </summary>
/// <remarks>
/// This models the Rinka definitions, their animations and the scene's two waves of launches.
/// Only the first Rinka launched is scripted to strike Samus; the others deliberately miss.
/// The hit step does not subtract health in the cinematic. It publishes its invincibility and knockback
/// (<see cref="IntroRinkaDefinitions.StrikeKnockbackUpdates"/>), which makes Samus flash and visibly react.
/// The Rinkas are a list in spawn order with no cap (#1625): only the first one launched touches Samus, so the order the
/// others step and draw in is not observable.
/// </remarks>
internal sealed class IntroRinkaSystem
{
    /// <summary>The live Rinkas, in spawn order; a deleted Rinka leaves the list at the end of the pass that deletes it.</summary>
    private readonly List<IntroRinka> rinkas = [];
    /// <summary>The waves of launches still to come, the next first (<see cref="IntroRinkaDefinitions.Launches"/>).</summary>
    private readonly Queue<IntroRinkaLaunchWave> waves = new(IntroRinkaDefinitions.Launches);
    /// <summary>The next wave's wait, counted once each update; the update it ends on launches the wave.</summary>
    private UpdateCountdown launchWait = UpdateCountdown.Lasting(IntroRinkaDefinitions.Launches[0].Wait);
    /// <summary>Whether Mother Brain's explosions have started, after which no wave is left to launch.</summary>
    private bool spawnsForbidden;

    /// <summary>One update of the launch schedule, then of every Rinka.</summary>
    /// <param name="samus">Samus, whose X position a hitting Rinka crosses before applying its knockback.</param>
    /// <param name="motherBrainExploding">True once Mother Brain's exploding step deletes the Rinkas.</param>
    /// <param name="explosionsAllocated">
    /// True once Mother Brain's explosions have started. The schedule launches every Rinka before then, so a launch after it
    /// is a fault in the cinematic's script.
    /// </param>
    [IndependentFlag("Mother Brain exploding and her explosions having started are separate intro states read each frame.")]
    public void Step(SamusState samus, bool motherBrainExploding,
        bool explosionsAllocated)
    {
        Ensure.NotNull(samus);

        spawnsForbidden = explosionsAllocated;
        // The schedule runs before the Rinkas, so a Rinka it launches runs its first flight check and animation step in this
        // same update.
        if (waves.Count != 0 && launchWait.Count())
        {
            Spawn(waves.Dequeue().Pair);
            if (waves.TryPeek(out IntroRinkaLaunchWave? next))
                launchWait = UpdateCountdown.Lasting(next.Wait);
        }

        foreach (IntroRinka rinka in rinkas)
        {
            Fly(rinka, samus, motherBrainExploding);
            if (rinka.Sprite.IsActive)
            {
                rinka.Sprite.Step(cue => HandleRinkaCue(rinka, cue));
            }
        }
        _ = rinkas.RemoveAll(static rinka => !rinka.Sprite.IsActive);
    }

    /// <summary>Adds each visible Rinka in spawn order.</summary>
    public void Draw(ScreenSprites screenSprites, IIntroCinematicSpritePresentation installedArt)
    {
        foreach (IntroRinka rinka in rinkas)
            rinka.Sprite.Draw(screenSprites, installedArt: installedArt);
    }

    /// <summary>Starts a Rinka's movement, selecting the hit route for the Rinka that strikes Samus and the miss route for the others.</summary>
    private static void HandleRinkaCue(
        IntroRinka rinka,
        IntroRinkaCue cue)
    {
        switch (cue)
        {
            case IntroRinkaCue.StartMoving:
                rinka.StartMoving();
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(IntroRinkaCue)} {cue}.");
        }
    }

    /// <summary>Spawns the Rinkas of <paramref name="pair"/>, in launch order, after the Rinkas already live.</summary>
    private void Spawn(IReadOnlyList<IntroRinkaLaunch> pair)
    {
        if (spawnsForbidden)
        {
            throw new InvalidOperationException(
                "The intro Rinka schedule launched a Rinka after Mother Brain's explosions started.");
        }

        foreach (IntroRinkaLaunch launch in pair)
            rinkas.Add(new IntroRinka(launch));
    }

    /// <summary>Moves a moving Rinka, publishes Samus knockback on the striking flight, or deletes it at its exit bounds.</summary>
    private static void Fly(
        IntroRinka rinka,
        SamusState samus,
        bool motherBrainExploding)
    {
        CinematicActor<IntroRinkaAnimation, CinematicSpritePose, IntroRinkaCue> sprite = rinka.Sprite;
        if (!rinka.IsMoving)
            return;
        switch (rinka.Flight)
        {
            case IntroRinkaFlight.StrikesSamus:
                sprite.Position = Drifted(sprite.Position, rinka.Drift);

                // The Rinka compares its striking edge against a point just left of Samus. Until that crossing,
                // the actor survives unless Mother Brain has entered her exploding step.
                if (sprite.ScreenPlace.X + IntroRinkaDefinitions.StrikingEdge < samus.Position.X.Whole - IntroRinkaDefinitions.SamusStruckReach)
                {
                    if (motherBrainExploding)
                        sprite.Delete();
                    return;
                }

                samus.RequestKnockback(IntroRinkaDefinitions.StrikeKnockbackUpdates, IntroRinkaDefinitions.StrikeKnockbackUpdates,
                    Samus.Movement.KnockbackHorizontalDirection.Right);
                sprite.Delete();
                return;

            case IntroRinkaFlight.MissesSamus:
                sprite.Position = Drifted(sprite.Position, rinka.Drift);
                int y = sprite.ScreenPlace.Y;
                if (y < IntroRinkaDefinitions.MissTopY || y >= IntroRinkaDefinitions.MissBottomY || motherBrainExploding)
                    sprite.Delete();
                return;

            default:
                throw new InvalidOperationException($"Undefined {nameof(IntroRinkaFlight)} {rinka.Flight}.");
        }
    }

    /// <summary>
    /// Where a Rinka at <paramref name="position"/> drifts to: sideways by <paramref name="sideways"/> and down by half a
    /// pixel, so every second update the half pixels add up to one more whole pixel.
    /// </summary>
    private static FixedVector<ScreenPixels> Drifted(FixedVector<ScreenPixels> position, SignedSixteenSixteen sideways) =>
        new(position.X + sideways, position.Y + IntroRinkaDefinitions.DownwardDrift);
}
