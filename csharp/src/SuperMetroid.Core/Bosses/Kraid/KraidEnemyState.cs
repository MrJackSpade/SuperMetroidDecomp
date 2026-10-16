namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Shared Kraid encounter state at WRAM <c>$7E:7800</c>, plus the room-level effects whose
/// native owners sit outside an individual enemy record. No host-only combat simplification
/// is represented here: thresholds and function pointers are copied from retail arithmetic.
/// </summary>
public sealed class KraidEnemyState
{
    /// <summary>Gets eight native-order part workspaces: body, arm, three lints, foot, and two fingernails; they remain owned by the room after the body is deleted.</summary>
    public KraidPartState[] Parts { get; } = Enumerable.Range(0, 8)
        .Select(_ => new KraidPartState())
        .ToArray();

    /// <summary>Gets the retained native word at $7E:7802; mouth-reopen continuations write two at $A7:AF24/$BC58 without an established broader meaning.</summary>
    public ushort Unknown2 { get; internal set; }
    /// <summary>Gets the body think countdown selected from the native random-delay table and consumed before the next mouth attack.</summary>
    public NativeCountdown ThinkingTimer { get; internal set; }
    /// <summary>Gets the minimum whole-pixel world Y allowed when Kraid's body ejects Samus; growth changes the floor-relative clamp from 324 to 164.</summary>
    public ushort MinimumYPositionForEjection { get; internal set; }
    /// <summary>Gets the native mouth-reopen word: low bits retain collision/reopen state and the high byte counts pending reopen continuations.</summary>
    public KraidMouthFlags MouthFlags { get; internal set; }
    /// <summary>Health captured by the living initializer before combat can change it.</summary>
    public ushort InitialHealth { get; internal set; }
    /// <summary>Returns the cartridge threshold for the selected eighth of the initializer's captured health.</summary>
    /// <param name="index">A zero-based table index from zero through seven, selecting one through eight eighths.</param>
    /// <returns>The initial health truncated to one eighth, multiplied by the selected number of eighths.</returns>
    public ushort HealthEighthThreshold(int index) => KraidHealthThresholdDefinitions.Eighth(InitialHealth, index);
    /// <summary>Gets the second-phase whole-pixel walking target; the native unused move-right instruction also reuses this word as a countdown.</summary>
    public ushort TargetX { get; internal set; }
    /// <summary>Gets the hurt-palette countdown phase, normally started at six by a vulnerable mouth hit and decremented by its frame timer.</summary>
    public ushort HurtFrame { get; internal set; }
    /// <summary>Gets the countdown pacing hurt-palette phases, normally reloaded to two and reused by background fades during death.</summary>
    public NativeCountdown HurtFrameTimer { get; internal set; }
    /// <summary>Gets the bank-$A7 tilemap offset selected by the current private eight-byte head instruction.</summary>
    public ushort CurrentHeadTilemap { get; internal set; }
    /// <summary>Gets the native bank-$A7 mouth-hitbox record offset accepting damaging shots for the current head frame.</summary>
    public ushort VulnerableMouthHitbox { get; internal set; }
    /// <summary>Gets the native bank-$A7 mouth-hitbox record offset used for nondamaging shot reactions in the current head frame.</summary>
    public ushort InvulnerableMouthHitbox { get; internal set; }

    /// <summary>
    /// Native $7E:2000-$2FFF combined BG2 tilemap. Keeping this surface separate from
    /// VRAM is essential because rise, growth, head animation, and sinking upload only
    /// selected portions of it on their authored frames.
    /// </summary>
    internal ushort[] BackgroundTilemapWords { get; } =
        new ushort[KraidBackgroundRomData.WorkingTilemapWords];

    /// <summary>True after `$A7:AAC6` prepared Kraid's two decompressed BG2 tilemaps.</summary>
    public bool BackgroundTilemapsPrepared { get; internal set; }

    /// <summary>
    /// True while native BG2SC=$43 makes Kraid's private 64x64 map the renderer's BG2.
    /// Death restores the ordinary room map at $4800 and relinquishes this ownership.
    /// </summary>
    public bool OwnsBg2Tilemap { get; internal set; }

    /// <summary>Live BG2HOFS/BG2VOFS calculated by Kraid main AI from the camera and body position.</summary>
    public Position16<Bg2Pixels> Bg2Scroll { get; internal set; }

    /// <summary>Native BG2 top/bottom upload requests issued during the rise sequence.</summary>
    public int TopTilemapUploadCount { get; internal set; }
    /// <summary>Gets the number of authored bottom-half BG2 tilemap uploads issued by rise, growth, and death logic.</summary>
    public int BottomTilemapUploadCount { get; internal set; }

    /// <summary>Number of `$A7:C995` rock/quake spawn requests made by retail cadence.</summary>
    public int RiseRockSpawnRequestCount { get; internal set; }

    /// <summary>Rise-rock requests that acquired a real bank-$86 projectile slot.</summary>
    public int SpawnedRiseRockCount { get; internal set; }

    /// <summary>BG2 head entries installed by the private eight-byte instruction stream.</summary>
    public int HeadTilemapUploadCount { get; internal set; }

    /// <summary>Retail `$BC0A` spat-rock requests issued while head tilemap three is active.</summary>
    public int SpitRockRequestCount { get; internal set; }

    /// <summary>Spat-rock requests that acquired a real bank-$86 projectile slot.</summary>
    public int SpawnedSpitRockCount { get; internal set; }

    /// <summary>Number of private `$AF94` roar opcodes consumed from the head stream.</summary>
    public int RoarRequestCount { get; internal set; }

    /// <summary>Last eight-frame-delayed music request made by the rise sequence.</summary>
    public MusicCommand? MusicRequest { get; internal set; }
    /// <summary>Gets the number of growth-sequence ceiling rocks that acquired a live bank-$86 projectile slot.</summary>
    public int CeilingRockSpawnCount { get; internal set; }

    /// <summary>Gets whether growth has installed the priority bits in Kraid's private BG2 tilemap.</summary>
    public bool Bg2PriorityBitsSet { get; internal set; }
    /// <summary>Gets whether growth has relinquished the first-screen camera lock for the enlarged encounter.</summary>
    public bool CameraReleasedForSecondPhase { get; internal set; }
    /// <summary>Gets the sinking sound countdown, initially forty-three and reloaded to thirty after each request.</summary>
    public NativeCountdown DeathSoundTimer { get; internal set; }
    /// <summary>Gets the number of authored sinking-table events consumed during the death descent.</summary>
    public int SinkTableEventCount { get; internal set; }
    /// <summary>Gets the number of pickup actors requested by Kraid's authored death scatter, independent of pool allocation success.</summary>
    public int DeathDropRequestCount { get; internal set; }
    /// <summary>Gets the number of standard BG3 graphics quarters queued during ordinary-background restoration.</summary>
    public int DeathBg3TransferCount { get; internal set; }
    /// <summary>Gets whether this encounter's death handoff newly wrote the persistent area-boss defeat bit.</summary>
    public bool BossDefeatPersisted { get; internal set; }
    /// <summary>Gets whether background restoration and the death music/persistence handoff have reached the terminal death entry.</summary>
    public bool DeathSequenceComplete { get; internal set; }
}
