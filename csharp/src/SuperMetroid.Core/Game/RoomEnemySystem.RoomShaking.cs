namespace SuperMetroid.Core.Game;

/// <summary>
/// Shared bank-$A0 room-shaking consumer used by bosses, ordinary enemies, PLMs, and room
/// scripts. Producers only write <see cref="EarthquakeType"/> and <see cref="EarthquakeTimer"/>;
/// this routine turns those words into per-background displacement and advances their lifetime.
/// </summary>
public sealed partial class RoomEnemySystem
{
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

        int tableAddress = RoomFxRomData.Earthquake.BgDisplacementTableAddress +
            EarthquakeType * RoomFxRomData.Earthquake.BytesPerType;
        short bg1X = unchecked((short)ReadWord(_bus!, tableAddress));
        short bg1Y = unchecked((short)ReadWord(_bus!, tableAddress + 2));
        short bg2X = unchecked((short)ReadWord(_bus!, tableAddress + 4));
        short bg2Y = unchecked((short)ReadWord(_bus!, tableAddress + 6));

        // Bit one alternates which side of the origin is shown. Native code forms the
        // negative half with EOR #$FFFF / INC before adding the same table word.
        if ((EarthquakeTimer & RoomFxRomData.Earthquake.AlternatingDirectionTimerMask) != 0)
        {
            bg1X = unchecked((short)-bg1X);
            bg1Y = unchecked((short)-bg1Y);
            bg2X = unchecked((short)-bg2X);
            bg2Y = unchecked((short)-bg2Y);
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

        LastRoomShake = new RoomShakeFrameResult(
            Applied: true,
            Bg1X: bg1X,
            Bg1Y: bg1Y,
            Bg2X: bg2X,
            Bg2Y: bg2Y,
            ShakesEnemies: shakesEnemies);
        return LastRoomShake;
    }
}

/// <summary>Signed PPU-scroll deltas produced by one accepted gameplay frame.</summary>
public readonly record struct RoomShakeFrameResult(
    bool Applied,
    short Bg1X,
    short Bg1Y,
    short Bg2X,
    short Bg2Y,
    bool ShakesEnemies);
