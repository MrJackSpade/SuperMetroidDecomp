using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Drawing;

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
    /// <exception cref="InvalidDataException">No boss artwork with Kraid's eye colours is installed.</exception>
    internal KraidEyeColours EyeColours => _bossArtwork.Artwork?.KraidEyes ?? throw new InvalidDataException(
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
        KraidBackdropColours backdrop = (_access.Kraid ?? throw new InvalidOperationException(
            "Kraid's backdrop fades once Kraid is loaded.")).Backdrop;
        int step = _presentation.GradualColorChange.Numerator;
        if (step <= KraidBackgroundDefinitions.BackdropFadeLastStep)
        {
            backdrop.Step(fade, step);
            _presentation.GradualColorChange.Advance();
            return false;
        }
        backdrop.Complete();
        _presentation.GradualColorChange.Reset();
        return true;
    }

    /// <summary>Emits a ceiling rock at the scripted horizontal position when a slot is free.</summary>
    internal bool SpawnKraidCeilingRock(int xPosition)
    {
        RoomEnemyProjectileSlot? rock = _projectiles.AllocateEnemyProjectile();
        if (rock is null)
            return false;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            rock,
            RoomEnemyProjectileKind.KraidCeilingRock,
            graphics: SpriteGraphics.CharacterZeroIn(KraidColours.RocksShading));
        rock.StartAnimation(new KraidRockAnimation(KraidRockProgram.SharedPose));
        rock.Position = FixedPosition<RoomPixels>.FromWhole(new(xPosition, 312));
        rock.Velocity = rock.Velocity.WithX(new SignedEightEight(0));
        rock.Velocity = rock.Velocity.WithY(SignedEightEight.Wrapped((_access.ReadKraidRandomNumber() & 63) + 64));
        rock.Graphics = SpriteGraphics.CharacterZeroIn(KraidColours.RocksShading);
        return true;
    }
}
