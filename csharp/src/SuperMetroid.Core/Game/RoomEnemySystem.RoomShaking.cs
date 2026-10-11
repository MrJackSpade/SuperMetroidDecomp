namespace SuperMetroid.Core.Game;

/// <summary>
/// Shared bank-$A0 room-shaking consumer used by bosses, ordinary enemies, PLMs, and room
/// scripts. Producers only write <see cref="EarthquakeType"/> and <see cref="EarthquakeTimer"/>;
/// this routine turns those words into per-background displacement and advances their lifetime.
/// </summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>Ports $86:8427, evaluated at each projectile draw pass before room-shake timer consumption.</summary>
    private (short X, short Y) GetEnemyProjectileShake(bool timeIsFrozen)
    {
        if (EarthquakeTimer == 0 || timeIsFrozen || EarthquakeType >= RoomFxRomData.Earthquake.FirstNonRenderedType)
            return default;
        RoomShakeDefinition definition = RoomShakeDefinitions.ForType(EarthquakeType);
        Offset16<RoomPixels> shake = (EarthquakeTimer & RoomFxRomData.Earthquake.AlternatingDirectionTimerMask) != 0
            ? -definition.Projectile
            : definition.Projectile;
        return (shake.X, shake.Y);
    }

    /// <summary>
    /// Displacement produced by the most recent call to <see cref="HandleRoomShaking"/>.
    /// It is deliberately a delta rather than mutated camera state: the cartridge adjusts
    /// direct-page PPU scroll copies after camera logic, without moving the world viewport.
    /// </summary>
    public RoomShakeFrameResult LastRoomShake { get; private set; }

    /// <summary>Ports <c>Handle_Room_Shaking</c> at $A0:8687-$8711.</summary>
    public RoomShakeFrameResult HandleRoomShaking(bool timeIsFrozen)
    {
        EnsureLoaded();
        LastRoomShake = default;

        // Hardware leaves both the timer and scroll words untouched while time is frozen.
        // Types $24+ are also deliberately ignored; their values are outside the 36-entry
        // displacement table and are owned by other effects rather than room shaking.
        if (EarthquakeTimer == 0 ||
            timeIsFrozen ||
            EarthquakeType >= RoomFxRomData.Earthquake.FirstNonRenderedType)
            return LastRoomShake;

        RoomShakeDefinition definition = RoomShakeDefinitions.ForType(EarthquakeType);
        Offset16<RoomPixels> bg1 = definition.Bg1;
        Offset16<Bg2Pixels> bg2 = definition.Bg2;

        // Bit one alternates which side of the origin is shown. Native code forms the
        // negative half with EOR #$FFFF / INC before adding the same table word.
        if ((EarthquakeTimer & RoomFxRomData.Earthquake.AlternatingDirectionTimerMask) != 0)
        {
            bg1 = -bg1;
            bg2 = -bg2;
        }

        EarthquakeTimer = unchecked((ushort)(EarthquakeTimer - 1));

        // Types $12-$23 shake every actor selected by DetermineWhichEnemiesToProcess. This
        // runs after drawing on the cartridge, so the two-frame actor timer becomes visible
        // on the following frame rather than retroactively changing already-emitted OAM.
        bool shakesEnemies = EarthquakeType >= RoomFxRomData.Earthquake.FirstEnemyShakingType;
        if (shakesEnemies)
        {
            foreach (ushort nativeIndex in _activeEnemyIndexes)
                SlotFromNativeIndex(nativeIndex).ShakeTimer =
                    RoomFxRomData.Earthquake.EnemyShakeDuration;
        }

        LastRoomShake = new RoomShakeFrameResult(bg1, bg2);
        return LastRoomShake;
    }
}

/// <summary>Signed PPU-scroll deltas produced by one accepted gameplay frame.</summary>
/// <param name="Bg1">BG1 scroll displacement.</param>
/// <param name="Bg2">BG2 scroll displacement.</param>
public readonly record struct RoomShakeFrameResult(
    Offset16<RoomPixels> Bg1,
    Offset16<Bg2Pixels> Bg2);
