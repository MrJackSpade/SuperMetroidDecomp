using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's room background: its fade, color transitions, top and bottom tilemap transfers and the ceiling rocks.</summary>
[RoomLifetime]
internal sealed class KraidBackground
{
    /// <summary>Owns rocks, lint, and other fight projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Applies Kraid palettes, background tiles, and OAM presentation.</summary>
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

    /// <summary>Shows the upper working page exactly as <c>$A7:C874</c> uploads it, covering any head drawn over it.</summary>
    internal static void TransferKraidTopTilemap(KraidEnemyState state)
    {
        state.DisplayedBody = state.DisplayedBody with { Top = state.WorkingTop, Head = null };
    }

    /// <summary>Shows the lower working page exactly as <c>$A7:C8B6</c> uploads it.</summary>
    internal static void TransferKraidBottomTilemap(KraidEnemyState state)
    {
        state.DisplayedBody = state.DisplayedBody with { Bottom = state.WorkingBottom };
    }

    /// <summary>Separates immutable RGB5 artwork from Kraid's native palette timing.</summary>
    internal RgbColor ReadKraidColor(KraidPaletteSource source, int index)
    {
        if ((uint)index >= KraidPalettes.ColorCount(source))
            throw new ArgumentOutOfRangeException(nameof(index));
        KraidColorCatalog colors = _bossArtwork.Artwork?.KraidColors ?? throw new InvalidDataException(
            "Installed enemy artwork has no Kraid RGB5 palette catalog.");
        return colors.Resolve(source, index);
    }

    /// <summary>Steps room background colors toward their grown-form target.</summary>
    internal bool AdvanceKraidRoomBackgroundFade(
        KraidBackdropFade fade)
    {
        int step = _presentation.GradualColorChange.Numerator;
        if (step <= 13)
        {
            for (int color = 0; color < 16; color++)
            {
                RgbColor current = _presentation.BossColours().KraidRoomBackdrop[color];
                RgbColor target = fade == KraidBackdropFade.ToBlack
                    ? RgbColor.Black
                    : ReadKraidColor(KraidPaletteSource.RoomBackdrop, color);
                _presentation.BossColours().KraidRoomBackdrop.Write(color, TransitionKraidColor(step, current, target));
            }
            _presentation.GradualColorChange.Advance();
            return false;
        }
        _presentation.GradualColorChange.Reset();
        return true;
    }

    /// <summary>Interpolates one RGB5 color toward its growth-phase target.</summary>
    private static RgbColor TransitionKraidColor(int step, RgbColor current, RgbColor target) =>
        current.Zip(target, (_, from, to) => TransitionKraidComponent(step, from, to));

    /// <summary>Moves one color channel toward its target under the native step rule.</summary>
    private static int TransitionKraidComponent(int step, int current, int target)
    {
        if (step == 0)
            return current;
        if (step == 13)
            return target;
        int denominator = 13 - step;
        int fixedDelta = Math.Abs(target - current) * 256 / denominator;
        int signedDelta = target < current ? -fixedDelta : fixedDelta;
        return EightEight.WholeOf(signedDelta + EightEight.FromWhole(current));
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
            graphics: SpriteGraphics.InPalette(3));
        rock.StartAnimation(new KraidRockAnimation(KraidRockProgram.SharedPose));
        rock.Position = FixedPosition<RoomPixels>.FromWhole(new(checked((ushort)xPosition), 312));
        rock.Velocity = rock.Velocity.WithX(new SignedEightEight(0));
        rock.Velocity = rock.Velocity.WithY(new SignedEightEight(unchecked((ushort)((_access.ReadKraidRandomNumber() & 63) + 64))));
        rock.Graphics = SpriteGraphics.InPalette(3);
        return true;
    }
}
