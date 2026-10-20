using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's room background: its fade, color transitions, top and bottom tilemap transfers and the ceiling rocks.</summary>
[RoomLifetime]
internal sealed class KraidBackground
{
    /// <summary>Room scratch memory, services, and tilemap state used throughout the Kraid fight.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns rocks, lint, and other fight projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Applies Kraid palettes, background tiles, and OAM presentation.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Kraid's state and part lookups, part programs, function timer, random numbers and the sound he last requested.</summary>
    private readonly KraidEncounterAccess _access;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal KraidBackground(EnemyRoomBinding room, EnemyProjectileEngine projectiles, EnemyPresentation presentation, KraidEncounterAccess access)
    {
        _room = room;
        _projectiles = projectiles;
        _presentation = presentation;
        _access = access;
    }

    /// <summary>Shows the upper working page exactly as <c>$A7:C874</c> uploads it, covering any head drawn over it.</summary>
    internal static void TransferKraidTopTilemap(KraidEnemyState state)
    {
        state.DisplayedBody = state.DisplayedBody with { Top = state.WorkingTop, Head = null };
        state.TopTilemapUploadCount++;
    }

    /// <summary>Shows the lower working page exactly as <c>$A7:C8B6</c> uploads it.</summary>
    internal static void TransferKraidBottomTilemap(KraidEnemyState state)
    {
        state.DisplayedBody = state.DisplayedBody with { Bottom = state.WorkingBottom };
        state.BottomTilemapUploadCount++;
    }

    /// <summary>Separates immutable RGB5 artwork from Kraid's native palette timing.</summary>
    internal Bgr555 ReadKraidColor(KraidPaletteSource source, int index)
    {
        if ((uint)index >= KraidPaletteRomData.ColorCount(source))
            throw new ArgumentOutOfRangeException(nameof(index));
        KraidColorCatalog colors = _room.Artwork?.KraidColors ?? throw new InvalidDataException(
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
                Bgr555 current = _presentation.BossPalettes.KraidRoomBackdrop[color];
                Bgr555 target = fade == KraidBackdropFade.ToBlack
                    ? Bgr555.Black
                    : ReadKraidColor(KraidPaletteSource.RoomBackdrop, color);
                _presentation.BossPalettes.KraidRoomBackdrop.Write(color, TransitionKraidColor(step, current, target));
            }
            _presentation.GradualColorChange.Advance();
            return false;
        }
        _presentation.GradualColorChange.Reset();
        return true;
    }

    /// <summary>Interpolates one RGB5 color toward its growth-phase target.</summary>
    private static Bgr555 TransitionKraidColor(int step, Bgr555 current, Bgr555 target) =>
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
        return SnesEightEight.WholeOf(signedDelta + SnesEightEight.FromWhole(current));
    }

    /// <summary>Emits a ceiling rock at the scripted horizontal position when a slot is free.</summary>
    internal bool SpawnKraidCeilingRock(ushort xPosition)
    {
        RoomEnemyProjectileSlot? rock = _projectiles.AllocateEnemyProjectile();
        if (rock is null)
            return false;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            rock,
            RoomEnemyProjectileKind.KraidCeilingRock,
            graphicsIndex: EnemyPaletteBits.Palette3);
        rock.StartAnimation(new KraidRockAnimation(KraidRockProgram.SharedPose));
        rock.Position = FixedPosition<RoomPixels>.FromWhole(new(xPosition, 312));
        rock.Velocity = rock.Velocity.WithX(new SnesSignedEightEight(0));
        rock.Velocity = rock.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)((_access.ReadKraidRandomNumber() & 63) + 64))));
        rock.GraphicsIndex = EnemyPaletteBits.Palette3;
        return true;
    }
}
