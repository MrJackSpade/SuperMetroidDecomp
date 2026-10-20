using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Fireflea.Artwork;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>The Fireflea species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class FirefleaBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Fireflea];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new FirefleaEntity(this, slot, common);

    /// <summary>Runs common touch handling before Fireflea's unconditional native death call.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Starts explosion and drop processing, including the retail second-death path.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Holds the shared darkness level that advances when Firefleas die.</summary>
    private readonly EnemyRoomStatus _roomStatus;

    /// <summary>Connects Fireflea motion and collision callbacks to room death and darkness state.</summary>
    internal FirefleaBehavior(EnemyCollision collision, EnemyDeathsAndDrops deathsAndDrops, EnemyRoomStatus roomStatus)
    {
        _collision = collision;
        _deathsAndDrops = deathsAndDrops;
        _roomStatus = roomStatus;
    }

    /// <summary>The words each physical slot keeps across Fireflea occupants and rooms.</summary>
    private readonly FirefleaExtensionWords[] _extensions = NewExtensions();

    /// <summary>Fresh, zeroed extension words for every physical slot.</summary>
    private static FirefleaExtensionWords[] NewExtensions() =>
        [.. Enumerable.Range(0, RoomEnemySystem.MaximumEnemyCount).Select(_ => new FirefleaExtensionWords())];

    /// <summary>Ports <c>InitAI_Fireflea</c> at <c>$A3:8D2D</c>, decoding motion parameters and the retail initial-circle angle bug.</summary>
    internal void InitializeFireflea(RoomEnemySlot slot)
    {
        var state = new FirefleaEnemyState(slot, _extensions[slot.SlotIndex]);
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.Program = AnimationProgramCursor<FirefleaProgram>.Start(FirefleaProgram.Loop);

        // Parameter one is native `init0`: bit one selects circle versus vertical motion,
        // bit zero selects the sign half of the speed table, and its high byte is the initial
        // circle angle. Parameter two (`init1`) packs speed index low and radius index high.
        int speedTableIndex = SnesLittleEndian.LowByte(slot.Spawn.Population.Parameter2) * 8;
        if ((slot.Spawn.Population.Parameter1 & 1) == 0)
            speedTableIndex += 4;
        state.SpeedTableIndex = speedTableIndex;
        state.AngularSpeed = EnemyMath.ReadLinearEnemySpeed(checked((ushort)speedTableIndex));

        byte radiusIndex = SnesLittleEndian.HighByte(slot.Spawn.Population.Parameter2);
        state.Radius = FirefleaMovementDefinitions.Radius(radiusIndex);

        if (state.UsesCircularMovement)
        {
            state.Center = slot.Position.Whole;
            state.Angle = SnesAngle.FromRaw(unchecked((ushort)(SnesLittleEndian.HighByteInPlace(slot.Spawn.Population.Parameter1))));

            // The initializer passes the complete 8.8 angle word to an eight-bit routine,
            // which reads its low byte. That is the retail missing-division bug; main AI
            // begins using the high byte on frame one and corrects the position immediately.
            slot.Position = slot.Position.WithWhole(state.Center + EnemyMath.CircleOffset(new SnesByteAngle(SnesLittleEndian.LowByte(state.Angle.RawValue)), checked((ushort)state.Radius)));
            return;
        }

        var spawnY = new Position16<RoomPixels>(0, slot.Position.Y.Whole);
        var reach = new Offset16<RoomPixels>(0, checked((short)state.Radius));
        state.MinimumYExtent = spawnY - reach;
        state.MaximumYExtent = spawnY + reach;
    }

    /// <summary>Ports <c>MainAI_Fireflea</c> at <c>$A3:8DEE</c>, advancing circular angle or wrapped vertical travel.</summary>
    private static void RunFirefleaMain(RoomEnemySlot slot, FirefleaEnemyState state)
    {
        if (state.UsesCircularMovement)
        {
            slot.Position = slot.Position.WithWhole(state.Center + EnemyMath.CircleOffset(state.Angle.ToByteAngle(), checked((ushort)state.Radius)));

            // `$A3:8E2D` performs an unaligned word read at var0+1: the speed's middle sixteen
            // bits, the high byte of its fraction under the low byte of its whole part. That
            // reproduces the exact signed 8.8 delta, including negative half-speeds.
            SnesSignedSixteenSixteen speed = state.AngularSpeed;
            state.Angle = state.Angle.AddRaw(SnesEightEight.FromSixteenSixteen(speed.WholePixels, speed.Fraction));
            return;
        }

        slot.Position = slot.Position.AdvanceY(EnemyMath.ReadLinearEnemySpeed(checked((ushort)state.SpeedTableIndex)));

        // Native CMP/BMI and CMP/BPL consume wrapped signed subtraction, not ordinary host
        // bounds comparisons. The actor is intentionally allowed to overshoot before XOR
        // bit two switches between the adjacent positive/negative table entries.
        bool crossedMinimum = (slot.Position.Whole - state.MinimumYExtent).Y < 0;
        bool reachedMaximum = (slot.Position.Whole - state.MaximumYExtent).Y >= 0;
        if (crossedMinimum || reachedMaximum)
            state.SpeedTableIndex ^= 4;
    }

    /// <summary>
    /// Ports Fireflea touch AI at $A3:8E6B, including its retail double-death bug. The
    /// private handler calls common touch AI and then calls <c>EnemyDeath</c> unconditionally:
    /// normal contact therefore kills an otherwise healthy Fireflea, while Screw Attack and
    /// similar contact attacks can increment the room kill count twice.
    /// </summary>
    internal void ResolveFirefleaTouch(
        RoomEnemySlot slot,
        SamusState samus)
    {
        _collision.ResolveNormalEnemyTouch(slot, samus);

        // With ordinary contact, native A still contains the surviving health ($A0:A480).
        // EnemyDeath clamps values above four to the small explosion. Queue that real
        // death program before clearing the actor so its artwork and sound can run.
        if (slot.EnemyDefinitionPointer != 0)
            _deathsAndDrops.StartGenericEnemyDeath(slot, deathAnimation: checked((ushort)slot.Health));
        else
        {
            // Common attacking contact already ran EnemyDeath ($A0:A48F), which returns A = 0
            // (or $00A3 after installing the respawn placeholder); $A3:8E6F then runs EnemyDeath
            // again on the cleared slot. Both values select the small explosion. It spawns at
            // (0,0) with the slot's current header (zero, or the placeholder), clears any
            // placeholder, and counts a second kill.
            _deathsAndDrops.StartGenericEnemyDeath(slot, deathAnimation: 0);
        }
        AdvanceFirefleaDarknessLevel();
    }

    /// <summary>
    /// Advances global <c>$177E</c> after one Fireflea death. Retail rooms contain at most
    /// five, so valid progression is 0,2,4,6,8,10; the original comparison nevertheless
    /// permits 12 and rejects 14, and that exact boundary remains visible here.
    /// </summary>
    private void AdvanceFirefleaDarknessLevel()
    {
        ushort next = unchecked((ushort)(_roomStatus.FirefleaDarknessLevel + 2));
        if (unchecked((short)(next - 14)) < 0)
            _roomStatus.FirefleaDarknessLevel = next;
    }

    /// <summary>Advances darkness after common bomb damage kills a Fireflea.</summary>
    internal void RegisterFirefleaBombReactions(EnemySharedReactions table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.Fireflea &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.FirefleaShot)
        {
            AfterCommon = c =>
            {
                if (c.Enemy.Health == 0)
                    AdvanceFirefleaDarknessLevel();
            },
        });
    }

    /// <summary>Advances Fireflea's typed animation.</summary>
    internal static void AdvanceFirefleaTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, FirefleaAnimations.Programs, FirefleaFrameDefinitions.ShownFrames.Of,
            static (FirefleaEnemyState state) => state.Program, static (state, cursor) => state.Program = cursor);

    /// <summary>Ports Fireflea's initialization AI.</summary>
    internal void RunFirefleaInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeFireflea(slot);
    }

    /// <summary>Ports Fireflea's main AI.</summary>
    internal static void RunFirefleaMainAi(RoomEnemySlot slot, IEnemyAiFrame _)
    {
        RunFirefleaMain(slot, slot.RequireSpeciesState<FirefleaEnemyState>());
    }

    /// <summary>Advances darkness before a lethal common power-bomb death is finalized.</summary>
    internal void RegisterFirefleaPowerBombReactions(EnemySharedReactions table)
    {
        // Reactions around common power-bomb damage.
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Fireflea &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.FirefleaPowerBomb)
        {
            BeforeGenericDeath = (_, _) => AdvanceFirefleaDarknessLevel(),
        });
    }

    /// <summary>Advances darkness before a lethal common projectile death is finalized.</summary>
    internal void RegisterFirefleaShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Fireflea &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.FirefleaShot)
        {
            BeforeGenericDeath = _ => AdvanceFirefleaDarknessLevel(),
        });
    }

    /// <summary>Registers Fireflea's touch callback, which runs the retail unconditional-death sequence.</summary>
    internal void RegisterFirefleaTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Fireflea &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.FirefleaTouch,
            context => ResolveFirefleaTouch(context.Slot, context.Samus));
    }
}
