using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Debugger-visible projection of one of bank $86's eighteen fixed enemy-projectile slots.
/// Positions retain separate 16-bit subpositions because a fireball's signed 8.8 velocity
/// is added to the high byte of that fraction by the original movement helpers.
/// </summary>
public sealed class RoomEnemyProjectileSlot
{
    internal RoomEnemyProjectileSlot(int slotIndex) => SlotIndex = slotIndex;

    /// <summary>Gets the physical pool index from zero through seventeen; twice this value is the native byte index $00..$22.</summary>
    public int SlotIndex { get; }
    /// <summary>Gets the native bank-$86 definition identity occupying the slot; zero releases the slot without clearing its other words.</summary>
    public RoomEnemyProjectileKind Kind { get; internal set; }
    /// <summary>Gets whether the slot has a nonzero native definition identity and participates in projectile passes.</summary>
    public bool IsActive => Kind != RoomEnemyProjectileKind.None;
    /// <summary>
    /// Gets the native 16.16 position: wrapped sixteen-bit whole words plus fractional words into
    /// whose high byte signed 8.8 motion adds. Room pixels for every family except the few
    /// pre-instructions that deliberately keep screen-relative words here.
    /// </summary>
    public FixedPosition<RoomPixels> Position { get; internal set; }
    /// <summary>Gets the unchanged native velocity words, interpreted as signed 8.8 by the shared movement helpers or reused by family-specific logic.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }
    /// <summary>Gets the bank-$86 offset of the next instruction-list word to execute; zero while a typed animation runs.</summary>
    public ushort InstructionPointer { get; internal set; }
    /// <summary>
    /// The typed animation replacing this actor's native instruction list (#641), or null when the
    /// bank-$86 interpreter runs <see cref="InstructionPointer"/>.
    /// </summary>
    internal EnemyProjectileAnimation? Animation { get; private set; }
    /// <summary>Gets the native draw-instruction countdown; allocation starts it at one so the initial list runs on its first reached pass.</summary>
    public ushort InstructionTimer { get; internal set; }
    /// <summary>Gets the native spritemap identity selected by the current draw instruction, initially the blank map.</summary>
    public ushort SpritemapPointer { get; internal set; }
    /// <summary>
    /// Host-only visual identity for an installed enemy-projectile program frame.
    /// Zero selects a named direct composition, including the native empty frame.
    /// </summary>
    public ushort PresentationOperandAddress { get; internal set; }
    /// <summary>Gets the bank-$86 routine offset dispatched before this slot's instruction-list processing.</summary>
    public ushort PreInstruction { get; internal set; }
    /// <summary>Gets the native combined sprite tile-base and palette word used when composing projectile OAM.</summary>
    public ushort GraphicsIndex { get; internal set; }
    /// <summary>Gets the collision half-extents in pixels, decoded from the definition's native packed radii.</summary>
    public Extent16<RoomPixels> Radius { get; internal set; }
    /// <summary>Gets the contact-damage value decoded from the native properties word, before Samus's suit reduction.</summary>
    public ushort Damage { get; internal set; }
    /// <summary>Gets the invincibility timer written to Samus on a damaging collision; definition initialization uses ninety-six frames.</summary>
    public ushort InvincibilityFrames { get; internal set; }
    /// <summary>
    /// Draw pass selected by native enemy-projectile property bit <c>$1000</c>.
    /// This is independent of the two-bit priority stored in each spritemap's OAM word.
    /// </summary>
    public EnemyProjectileDrawPriority DrawPriority { get; internal set; }
    /// <summary>
    /// Bank $86's independent <c>eproj_timers</c> word. This is not the frame-list
    /// instruction timer: projectile bytecode explicitly initializes and decrements this
    /// value for counted loops and randomized impact animations.
    /// </summary>
    public ushort GeneralTimer { get; internal set; }
    /// <summary>Gets Ridley's afterburn-chain counter; continuation decrements only its low byte and stops on signed underflow.</summary>
    public ushort RemainingAfterburns { get; internal set; }
    /// <summary>Gets the native definition offset used to spawn the next directional afterburn in the chain.</summary>
    public ushort NextAfterburnKind { get; internal set; }
    /// <summary>Gets the family-specific direction or initializer parameter, retaining its native word representation rather than imposing a shared angle format.</summary>
    public ushort DirectionParameter { get; internal set; }
    /// <summary>Native generic enemy-projectile variable E at WRAM <c>$1AFF,x</c>.</summary>
    public ushort Variable0 { get; internal set; }
    /// <summary>Native generic enemy-projectile variable F at WRAM <c>$1B23,x</c>.</summary>
    public ushort Variable1 { get; internal set; }
    /// <summary>Gets whether this actor participates in damaging Samus-contact checks, independently of persistence and shot-blocking properties.</summary>
    public bool CanDamageSamus { get; internal set; }
    /// <summary>Native projectile property $4000: contact does not delete this actor.</summary>
    public bool PersistsOnSamusContact { get; internal set; }
    /// <summary>Native projectile property $8000: Samus's shots test this actor for collision.</summary>
    public bool BlocksSamusProjectiles { get; internal set; }
    /// <summary>
    /// Native enemy-projectile initializer flag. Zero runs the actor's shot list, one
    /// creates an indestructible dud, and two suppresses the collision scan altogether.
    /// This is deliberately separate from property $8000: Kago enables that property only
    /// after its bug has moved far enough from the source shell, while retaining flag zero.
    /// </summary>
    public ushort CollisionOption { get; internal set; }
    /// <summary>
    /// Native <c>eproj_G</c> collision word after a destructible projectile is shot. Kago
    /// reuses the same word as its idle timer before collision, so the typed bug wrapper is
    /// the authority on which meaning is currently active.
    /// </summary>
    public ushort CollidedProjectileType { get; internal set; }
    /// <summary>
    /// Native <c>eproj_killed_enemy_index</c>. Enemy-death explosions set bit $8000 when
    /// their terminal instruction must rebuild this physical enemy slot.
    /// </summary>
    public ushort KilledEnemyNativeIndex { get; internal set; }
    /// <summary>
    /// Native bank-$A0 enemy-header pointer retained in the parallel bank-$7E projectile
    /// metadata. Death explosions use offset $3A of this record to find their six-byte
    /// bank-$B4 drop table.
    /// </summary>
    public EnemyDefinitionId EnemyHeaderPointer { get; internal set; }
    /// <summary>
    /// Direct bank-$B4 drop-table pointer used by the few boss/projectile instructions
    /// which already resolved a special drop table before allocating pickup $F337. Zero
    /// means that <see cref="EnemyHeaderPointer"/> remains authoritative.
    /// </summary>
    public ushort ItemDropChancesPointerOverride { get; internal set; }

    /// <summary>Runs <paramref name="animation"/> from its first frame on the next pass, in place of any native list.</summary>
    internal void StartAnimation(EnemyProjectileAnimation animation)
    {
        Animation = Ensure.NotNull(animation);
        InstructionPointer = 0;
        InstructionTimer = 1;
    }

    /// <summary>Runs the native instruction list at <paramref name="instructionList"/> from the next pass, ending any typed animation.</summary>
    internal void StartNativeProgram(ushort instructionList)
    {
        Animation = null;
        InstructionPointer = instructionList;
        InstructionTimer = 1;
    }

    /// <summary>
    /// A bare <c>STZ EnemyProjectile_ID,X</c>: frees the slot but leaves every other word,
    /// so a routine that keeps running after the store still moves the released slot.
    /// </summary>
    internal void ReleaseIdentityOnly() => Kind = RoomEnemyProjectileKind.None;

    internal void Clear()
    {
        Kind = RoomEnemyProjectileKind.None;
        // $86:8016/8154 release identity only. SpawnEprojInner clears fractions,
        // but leaves whole coordinates and velocities for the family initializer.
        Position = Position.WithFractionX(0).WithFractionY(0);
        InstructionPointer = InstructionTimer = SpritemapPointer = PreInstruction = 0;
        Animation = null;
        PresentationOperandAddress = 0;
        Radius = Extent16<RoomPixels>.Zero;
        GraphicsIndex = Damage = InvincibilityFrames = GeneralTimer = 0;
        DrawPriority = EnemyProjectileDrawPriority.Low;
        RemainingAfterburns = NextAfterburnKind = 0;
        DirectionParameter = Variable0 = Variable1 = 0;
        CollisionOption = CollidedProjectileType = KilledEnemyNativeIndex = 0;
        EnemyHeaderPointer = EnemyDefinitionId.None;
        ItemDropChancesPointerOverride = 0;
        CanDamageSamus = PersistsOnSamusContact = BlocksSamusProjectiles = false;
    }
}
