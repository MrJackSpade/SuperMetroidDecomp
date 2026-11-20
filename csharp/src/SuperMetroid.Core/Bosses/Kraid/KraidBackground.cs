using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's room background: its fade, color transitions and the ceiling rocks.</summary>
[RoomLifetime]
internal sealed class KraidBackground
{
    /// <summary>Owns rocks, lint, and other fight projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Applies Kraid's colours, background pictures, and sprite presentation.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Kraid's state and part lookups, part animations, phase waits, random numbers and the sound he last requested.</summary>
    private readonly KraidEncounterAccess _access;
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal KraidBackground(EnemyProjectileEngine projectiles, EnemyPresentation presentation, KraidEncounterAccess access, BossArtworkBinding bossArtwork,
        KraidProjectiles.RockBehavior rocks)
    {
        Rocks = Ensure.NotNull(rocks);
        _bossArtwork = bossArtwork;
        _projectiles = projectiles;
        _presentation = presentation;
        _access = access;
    }

    /// <summary>Kraid's installed eye colours, which his eye glow shows and times his roar from.</summary>
    internal KraidEyeColours EyeColours => _bossArtwork.KraidEyes;

    /// <summary>The rocks his rise throws up and his roar shakes down from the ceiling, his encounter's own (#2571).</summary>
    internal KraidProjectiles.RockBehavior Rocks { get; }

    /// <summary>
    /// Takes one step of the room backdrop's fade toward <paramref name="fade"/>'s colours on the shared gradual colour
    /// change (#1512): while the shared counter stands at or below <see cref="KraidBackgroundDefinitions.BackdropFadeLastStep"/> the backdrop takes a step, and once it has
    /// passed them the fade completes and resets the counter. When the fade completes is what gameplay sees: his growth
    /// and death wait on it, and a door's fade sharing the counter brings it sooner. The backdrop shows a typed fade of its
    /// colours (#2215) as far as its own steps have taken it (#2302).
    /// </summary>
    /// <returns>Whether the fade completed on this update.</returns>
    internal bool AdvanceKraidRoomBackgroundFade(
        KraidBackdropSettledColours fade)
    {
        KraidBackdropColours backdrop = _access.Backdrop;
        if (_presentation.FadeStepCount.Step <= KraidBackgroundDefinitions.BackdropFadeLastStep)
        {
            backdrop.Step(fade);
            _presentation.FadeStepCount.Advance();
            return false;
        }
        backdrop.Complete(fade);
        _presentation.FadeStepCount.Reset();
        return true;
    }

    /// <summary>Emits a ceiling rock at the scripted horizontal position when a projectile place is free.</summary>
    internal void SpawnKraidCeilingRock(int xPosition)
    {
        EnemyProjectile? rock = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.KraidCeilingRock, Rocks, graphics: SpriteGraphics.SharedIn(KraidColours.RocksShading));
        if (rock is null)
            return;
        rock.StartAnimation(new KraidRockAnimation(Rocks, KraidRockSegment.SharedPose));
        rock.Position = FixedPosition<RoomPixels>.FromWhole(new(xPosition, KraidArenaDefinitions.CeilingRockY));
        rock.Velocity = rock.Velocity.WithX(SignedEightEight.Zero);
        rock.Velocity = rock.Velocity.WithY(KraidRandomDefinitions.CeilingRockFallSpeed.Of(_access.ReadKraidRandomNumber()));
    }
}
