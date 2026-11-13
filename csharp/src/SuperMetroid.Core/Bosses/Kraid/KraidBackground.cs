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
    /// <summary>Applies Kraid palettes, background tiles, and sprite presentation.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Kraid's state and part lookups, part programs, function timer, random numbers and the sound he last requested.</summary>
    private readonly KraidEncounterAccess _access;
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal KraidBackground(EnemyProjectileEngine projectiles, EnemyPresentation presentation, KraidEncounterAccess access, BossArtworkBinding bossArtwork)
    {
        _bossArtwork = bossArtwork;
        _projectiles = projectiles;
        _presentation = presentation;
        _access = access;
    }

    /// <summary>Kraid's installed eye colours, which his eye glow shows and times his roar from.</summary>
    /// <exception cref="InvalidDataException">Kraid's eye colours are not installed.</exception>
    internal KraidEyeColours EyeColours => _bossArtwork.KraidEyes ?? throw new InvalidDataException(
        "Kraid's eyes require the installed Kraid eye colours.");

    /// <summary>
    /// Takes one step of the room backdrop's fade toward <paramref name="fade"/>'s colours on the shared gradual colour
    /// change (#1512): steps zero through thirteen move the backdrop, the step after completes the fade and resets the
    /// shared counter. The backdrop's colours are the import's shade of each step.
    /// </summary>
    /// <returns>Whether the fade completed on this call.</returns>
    internal bool AdvanceKraidRoomBackgroundFade(
        KraidBackdropFade fade)
    {
        if (_access.Kraid is null)
            throw new InvalidOperationException("Kraid's backdrop fades once Kraid is loaded.");
        KraidBackdropColours backdrop = _access.Backdrop;
        int step = _presentation.FadeStepCount.Step;
        if (step <= KraidBackgroundDefinitions.BackdropFadeLastStep)
        {
            backdrop.Step(fade, step);
            _presentation.FadeStepCount.Advance();
            return false;
        }
        backdrop.Complete(fade);
        _presentation.FadeStepCount.Reset();
        return true;
    }

    /// <summary>Emits a ceiling rock at the scripted horizontal position when a slot is free.</summary>
    internal bool SpawnKraidCeilingRock(int xPosition)
    {
        EnemyProjectile? rock = _projectiles.AllocateEnemyProjectile();
        if (rock is null)
            return false;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            rock,
            RoomEnemyProjectileKind.KraidCeilingRock,
            graphics: SpriteGraphics.SharedIn(KraidColours.RocksShading));
        rock.StartAnimation(new KraidRockAnimation(KraidRockProgram.SharedPose));
        rock.Position = FixedPosition<RoomPixels>.FromWhole(new(xPosition, 312));
        rock.Velocity = rock.Velocity.WithX(SignedEightEight.Zero);
        rock.Velocity = rock.Velocity.WithY(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(
            KraidRandomDefinitions.CeilingRockFallSpeedSpread.Of(_access.ReadKraidRandomNumber()) +
            KraidRandomDefinitions.CeilingRockFallSpeedBase));
        rock.Graphics = SpriteGraphics.SharedIn(KraidColours.RocksShading);
        return true;
    }
}
