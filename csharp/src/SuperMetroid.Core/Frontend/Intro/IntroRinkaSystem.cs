using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The four Rinkas (the ring-shaped “Cheerios”) fired during the Mother Brain flashback.
/// </summary>
/// <remarks>
/// This models the Rinka and spawner definitions and their animations.
/// Only the first Rinka launched is scripted to strike Samus; the other three deliberately miss.
/// The hit step does not subtract health in the cinematic. It publishes eleven
/// frames of invincibility and knockback, which makes Samus flash and visibly react.
/// The Rinkas are a list in spawn order with no cap (#1625): only the first one launched touches Samus, so the order the
/// others step and draw in is not observable.
/// </remarks>
internal sealed class IntroRinkaSystem
{
    /// <summary>The live Rinkas, in spawn order; a deleted Rinka leaves the list at the end of the pass that deletes it.</summary>
    internal readonly List<IntroRinka> rinkas = [];
    /// <summary>Invisible actor whose animation schedules the two spawn pairs.</summary>
    private readonly CinematicActor<IntroRinkaAnimation, CinematicSpritePose, IntroRinkaSpawnerCue> spawner;
    /// <summary>Whether Mother Brain's explosions have started, after which the spawner's animation has no spawn left.</summary>
    private bool spawnsForbidden;

    /// <summary>Creates the invisible spawner before its timed animation runs.</summary>
    public IntroRinkaSystem()
    {
        // The spawner's animation times both launches (IntroRinkaAnimations.Spawner).
        spawner = new(new(0, 0), IntroShades.Drawn(IntroSpriteColouring.DiscoveryActors), IntroRinkaAnimations.Spawner,
            IntroRinkaDefinitions.SpawnerActor.Animation);
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
        // The spawner updates before the Rinkas, so a Rinka it spawns runs its first flight check and animation step in
        // this same update.
        if (spawner.IsActive)
            spawner.Step(HandleSpawnerCue);

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

    /// <summary>Handles the two spawner cues, launching the striking pair or the missing pair.</summary>
    private void HandleSpawnerCue(IntroRinkaSpawnerCue cue)
    {
        switch (cue)
        {
            case IntroRinkaSpawnerCue.SpawnStrikingPair:
                Spawn(IntroRinkaDefinitions.StrikingPair);
                return;

            case IntroRinkaSpawnerCue.SpawnMissingPair:
                Spawn(IntroRinkaDefinitions.MissingPair);
                return;

            default:
                throw new InvalidOperationException($"Undefined {nameof(IntroRinkaSpawnerCue)} {cue}.");
        }
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
                "The intro Rinka spawner spawned a Rinka after Mother Brain's explosions started.");
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
