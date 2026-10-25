namespace SuperMetroid.Core.Fx;

/// <summary>
/// The room's earthquake: the shared wrapping shake timer and type that actors and effects write, the projectile
/// displacement they imply, and the per-frame background displacement <c>Handle_Room_Shaking</c> consumes.
/// </summary>
internal sealed class RoomEarthquake
{
    /// <summary>Shared wrapping room-shake lifetime word, written by actor/effect producers and decremented by <see cref="Advance"/> for supported types while time is not frozen.</summary>
    public WrappingCountdown EarthquakeTimer { get; set; }

    /// <summary>
    /// ORs <paramref name="bits"/> into the shake timer word, as the producers' <c>ORA</c>/<c>STA</c> into
    /// <c>$1840</c> do. It never shortens a running shake; setting bit 15 keeps the room shaking for tens of
    /// thousands of frames, which is how continuous effects avoid re-arming the timer every frame.
    /// </summary>
    internal void AddTimerBits(int bits) =>
        EarthquakeTimer = WrappingCountdown.FromRaw(unchecked((ushort)(EarthquakeTimer.RawValue | Ensure.BetweenInclusive(0, ushort.MaxValue, bits))));

    /// <summary>The shake timer's top bit, which keeps the room shaking for tens of thousands of frames once set.</summary>
    private const int ContinuousShakeBit = 1 << 15;

    /// <summary>
    /// Keeps the room shaking without re-arming the timer every frame, by setting its top bit (#1415): the Zebes escape's
    /// outdoor rooms do this on every update.
    /// </summary>
    internal void KeepShaking() => AddTimerBits(ContinuousShakeBit);

    /// <summary>
    /// Room-shake type selecting background/projectile offsets and whether actors shake; types 0 through 35 are
    /// consumed by <see cref="Advance"/>, while 36 and above are ignored there. Producers write a type from 0 through
    /// 65535 (the cartridge's word); every test is an ordinary comparison of that non-negative number, so values
    /// outside the range are rejected rather than wrapped.
    /// </summary>
    public int EarthquakeType { get; set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value); }

    /// <summary>
    /// Displacement produced by the most recent <see cref="Advance"/>. It is deliberately a delta rather than
    /// mutated camera state: the cartridge adjusts direct-page PPU scroll copies after camera logic, without
    /// moving the world viewport.
    /// </summary>
    public RoomShakeFrameResult LastRoomShake { get; internal set; }

    /// <summary>Ports $86:8427, evaluated at each projectile draw pass before room-shake timer consumption.</summary>
    internal Offset16<RoomPixels> GetEnemyProjectileShake(bool timeIsFrozen)
    {
        if (EarthquakeTimer.IsZero || timeIsFrozen || EarthquakeType >= RoomEarthquakeDefinitions.FirstNonRenderedType)
            return default;
        RoomShakeDefinition definition = RoomShakeDefinitions.ForType(EarthquakeType);
        return (EarthquakeTimer.RawValue & RoomEarthquakeDefinitions.AlternatingDirectionTimerMask) != 0
            ? -definition.Projectile
            : definition.Projectile;
    }

    /// <summary>
    /// The earthquake part of <c>Handle_Room_Shaking</c> ($A0:8687-$8711): this frame's background displacement and
    /// timer step. Returns whether the type also shakes the actors selected for processing.
    /// </summary>
    internal bool Advance(bool timeIsFrozen)
    {
        LastRoomShake = default;

        // Hardware leaves both the timer and scroll words untouched while time is frozen.
        // Types $24+ are also deliberately ignored; their values are outside the 36-entry
        // displacement table and are owned by other effects rather than room shaking.
        if (EarthquakeTimer.IsZero ||
            timeIsFrozen ||
            EarthquakeType >= RoomEarthquakeDefinitions.FirstNonRenderedType)
            return false;

        RoomShakeDefinition definition = RoomShakeDefinitions.ForType(EarthquakeType);
        Offset16<RoomPixels> bg1 = definition.Bg1;
        Offset16<Bg2Pixels> bg2 = definition.Bg2;

        // Bit one alternates which side of the origin is shown. Native code forms the
        // negative half with EOR #$FFFF / INC before adding the same table word.
        if ((EarthquakeTimer.RawValue & RoomEarthquakeDefinitions.AlternatingDirectionTimerMask) != 0)
        {
            bg1 = -bg1;
            bg2 = -bg2;
        }

        EarthquakeTimer = EarthquakeTimer.Decrement().Remaining;
        LastRoomShake = new RoomShakeFrameResult(bg1, bg2);
        // Types $12-$23 shake every actor selected by DetermineWhichEnemiesToProcess.
        return EarthquakeType >= RoomEarthquakeDefinitions.FirstEnemyShakingType;
    }
}
