using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Focused lifecycle proof for Shaktool's three fully-authored but unreachable attack-circle
/// definitions. Retail disables the bank-$AA producer with an early RTL; this audit invokes
/// the translated body directly, then keeps bank $86's definition, bytecode, collision, and
/// shared interaction dispatchers authoritative for everything after that call boundary.
/// </summary>
internal static partial class ShaktoolAudit
{
    private const ushort ShaktoolFrontCirclePreInstruction = 0xbe03;
    private const ushort ShaktoolLinkedCirclePreInstruction = 0xbe12;
    private const ushort EnemyProjectileNoOperationPreInstruction = 0x84fb;
    private const ushort BlankEnemyProjectileSpritemap = 0x8000;

    private static readonly CircleDefinition[] ShaktoolCircleDefinitions =
    [
        new(
            RoomEnemyProjectileKind.ShaktoolAttackFrontCircle,
            Initializer: 0xbda2,
            InitialPreInstruction: ShaktoolFrontCirclePreInstruction,
            InstructionList: 0xbd68,
            Damage: 10,
            CanDamageSamus: true),
        new(
            RoomEnemyProjectileKind.ShaktoolAttackMiddleCircle,
            Initializer: 0xbd9c,
            InitialPreInstruction: EnemyProjectileNoOperationPreInstruction,
            InstructionList: 0xbd78,
            Damage: 0,
            CanDamageSamus: false),
        new(
            RoomEnemyProjectileKind.ShaktoolAttackBackCircle,
            Initializer: 0xbd9c,
            InitialPreInstruction: EnemyProjectileNoOperationPreInstruction,
            InstructionList: 0xbd8c,
            Damage: 0,
            CanDamageSamus: false),
    ];

    private static void VerifyUnusedAttackCircleLifecycles(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        VerifyCircleInitializationAnimationAndTerrain(bus, room, assets);
        VerifyCircleInteractionsAndOwnerTeardown(bus, room, assets);
    }

    /// <summary>
    /// Proves the PHA/PLA owner link at $AA:D9A8-$D9B1, the staggered bank-$86 animation,
    /// exact signed 8.8 motion, visible rendering, front-circle terrain collision, and the
    /// linked circles' dependence on that physical front slot.
    /// </summary>
    private static void VerifyCircleInitializationAnimationAndTerrain(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        CircleEncounter encounter = SpawnFreshAttackCircles(bus, room, assets);
        VerifyCircleInitialization(bus, encounter);

        RoomEnemyProjectileSlot front = encounter.Front;
        RoomEnemyProjectileSlot middle = encounter.Middle;
        RoomEnemyProjectileSlot back = encounter.Back;
        var maps = ShaktoolCircleDefinitions.ToDictionary(
            definition => definition.Kind,
            _ => new HashSet<ushort>());

        ushort initialFrontX = front.XPosition;
        ushort initialFrontXSubposition = front.XSubposition;
        ushort initialFrontY = front.YPosition;
        ushort initialFrontYSubposition = front.YSubposition;
        ushort initialMiddleX = middle.XPosition;
        ushort initialMiddleY = middle.YPosition;
        ushort initialBackX = back.XPosition;
        ushort initialBackY = back.YPosition;

        // The front pre-instruction is active immediately. Middle and rear begin at $84FB;
        // their lists install $BE12 only after 6 and 10 stationary frames respectively.
        // Checking the boundary frame and the following frame prevents an implementation
        // from hiding an off-by-one delay behind a later, merely plausible formation.
        for (int step = 1; step <= 12; step++)
        {
            encounter.Enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus: null,
                cameraX: 0,
                cameraY: 0,
                nmiFrameCounter8: unchecked((byte)(step - 1)));

            foreach (RoomEnemyProjectileSlot projectile in
                     new[] { front, middle, back })
            {
                if (projectile.IsActive &&
                    projectile.SpritemapPointer is not 0 and not BlankEnemyProjectileSpritemap)
                {
                    maps[projectile.Kind].Add(projectile.SpritemapPointer);
                }
            }

            if (step == 1)
            {
                (ushort expectedX, ushort expectedXSubposition) = AdvanceCircleVelocity(
                    initialFrontX,
                    initialFrontXSubposition,
                    front.XVelocity);
                (ushort expectedY, ushort expectedYSubposition) = AdvanceCircleVelocity(
                    initialFrontY,
                    initialFrontYSubposition,
                    front.YVelocity);
                if (!front.IsActive || front.XPosition != expectedX ||
                    front.XSubposition != expectedXSubposition ||
                    front.YPosition != expectedY ||
                    front.YSubposition != expectedYSubposition ||
                    middle.XPosition != initialMiddleX || middle.YPosition != initialMiddleY ||
                    back.XPosition != initialBackX || back.YPosition != initialBackY)
                {
                    throw new InvalidDataException(
                        "Shaktool circle first frame did not move only the front actor by " +
                        "its definition-derived signed 8.8 velocity.");
                }
            }

            if (step == 7 &&
                (middle.PreInstruction != ShaktoolLinkedCirclePreInstruction ||
                 middle.XPosition != initialMiddleX || middle.YPosition != initialMiddleY))
            {
                throw new InvalidDataException(
                    $"Shaktool middle circle delay boundary failed: pre=" +
                    $"${middle.PreInstruction:X4}, position=(${middle.XPosition:X4}," +
                    $"${middle.YPosition:X4}).");
            }
            if (step == 8 &&
                (middle.XPosition == initialMiddleX && middle.YPosition == initialMiddleY))
            {
                throw new InvalidDataException(
                    "Shaktool middle circle did not begin moving one frame after installing $BE12.");
            }
            if (step == 11 &&
                (back.PreInstruction != ShaktoolLinkedCirclePreInstruction ||
                 back.XPosition != initialBackX || back.YPosition != initialBackY))
            {
                throw new InvalidDataException(
                    $"Shaktool rear circle delay boundary failed: pre=${back.PreInstruction:X4}, " +
                    $"position=(${back.XPosition:X4},${back.YPosition:X4}).");
            }
            if (step == 12 &&
                (back.XPosition == initialBackX && back.YPosition == initialBackY))
            {
                throw new InvalidDataException(
                    "Shaktool rear circle did not begin moving one frame after installing $BE12.");
            }
        }

        VerifyAuthoredCircleMaps(bus, maps);

        // Rendering after all three delays proves that the cartridge map pointers are not
        // only changing in state: the common enemy-projectile writer can consume them into
        // real OBJ entries with the Shaktool segment's graphics index.
        var oam = new OamBuffer();
        oam.BeginFrame();
        encounter.Enemies.DrawEnemyProjectiles(oam, cameraX: 0, cameraY: 0);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("Shaktool attack circles emitted no OBJ pieces.");

        // `$86:810D-$8122` scans physical slots `$22->$00`. Front occupies `$22`, so its
        // `$BE03` terrain collision clears the owner before middle `$20` and rear `$1E`
        // execute later in the SAME bank-$86 pass. Their shared `$BE12` pre-instruction
        // must observe that dead physical link immediately; retaining them for one host
        // frame would prove that the scheduler had reversed the cartridge's slot order.
        bool frontHitTerrain = false;
        for (int frame = 12; frame < 4096 && front.IsActive; frame++)
        {
            encounter.Enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus: null,
                cameraX: 0,
                cameraY: 0,
                nmiFrameCounter8: unchecked((byte)frame));
            frontHitTerrain = !front.IsActive;
        }
        if (!frontHitTerrain || middle.IsActive || back.IsActive)
        {
            throw new InvalidDataException(
                $"Shaktool front terrain boundary failed: front/middle/rear live=" +
                $"{front.IsActive}/{middle.IsActive}/{back.IsActive}.");
        }
    }

    /// <summary>
    /// Uses a separate untouched encounter for destructive probes. Definition $BE25 lacks
    /// property $8000, so beams pass through; its literal property word $000A then drives
    /// the shared Samus-contact path for 10 damage, invincibility, knockback, and deletion.
    /// The delayed circles must eventually notice that contact-deleted owner as well.
    /// </summary>
    private static void VerifyCircleInteractionsAndOwnerTeardown(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        CircleEncounter encounter = SpawnFreshAttackCircles(bus, room, assets);
        VerifyCircleInitialization(bus, encounter);

        var shots = new SamusProjectileSystem();
        SamusProjectileSlot beam = shots.Slots[0];
        beam.Type = 0x0001;
        beam.Damage = 20;
        beam.Direction = (ushort)SamusProjectileDirection.Right;
        beam.XPosition = encounter.Front.XPosition;
        beam.YPosition = encounter.Front.YPosition;
        beam.XRadius = 4;
        beam.YRadius = 4;
        beam.InstructionPointer = 0x9000;
        beam.InstructionTimer = 1;
        int shotHits = encounter.Enemies.ResolveEnemyProjectileSamusProjectileHits(
            bus,
            shots,
            new SamusBombProjectileSystem());
        if (shotHits != 0 || beam.InstructionPointer != 0x9000 ||
            !encounter.Front.IsActive)
        {
            throw new InvalidDataException(
                $"Shaktool front circle incorrectly blocked a beam: hits={shotHits}, " +
                $"beam=${beam.InstructionPointer:X4}, live={encounter.Front.IsActive}.");
        }

        var bombs = new SamusBombProjectileSystem();
        EnemyProjectileAuditAssertions.VerifyNaturalSamusContact(
            bus,
            encounter.Enemies,
            encounter.Samus,
            bombs,
            assets.LevelData,
            encounter.Front,
            cameraX: 0,
            cameraY: 0);

        // Front contact happens before either delayed actor has installed its link-checking
        // pre-instruction. Let their real bytecode reach $BE12; each must then delete itself
        // from the saved physical owner index, without a bespoke contact cleanup path.
        for (int frame = 0;
             frame < 16 && (encounter.Middle.IsActive || encounter.Back.IsActive);
             frame++)
        {
            encounter.Enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus: null,
                cameraX: 0,
                cameraY: 0,
                nmiFrameCounter8: unchecked((byte)frame));
        }
        if (encounter.Middle.IsActive || encounter.Back.IsActive)
        {
            throw new InvalidDataException(
                $"Contact-deleted Shaktool front left linked actors alive: " +
                $"middle/rear={encounter.Middle.IsActive}/{encounter.Back.IsActive}.");
        }
    }

    private static CircleEncounter SpawnFreshAttackCircles(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        RoomEnemySystem enemies = CreateEncounter(bus, room, assets, out SamusState samus);
        RoomEnemySlot source = GetGroup(enemies)[3];
        enemies.SpawnUnusedShaktoolAttackCircles(source);

        RoomEnemyProjectileSlot front = RequireOneCircle(
            enemies,
            RoomEnemyProjectileKind.ShaktoolAttackFrontCircle);
        RoomEnemyProjectileSlot middle = RequireOneCircle(
            enemies,
            RoomEnemyProjectileKind.ShaktoolAttackMiddleCircle);
        RoomEnemyProjectileSlot back = RequireOneCircle(
            enemies,
            RoomEnemyProjectileKind.ShaktoolAttackBackCircle);
        return new CircleEncounter(enemies, samus, source, front, middle, back);
    }

    private static RoomEnemyProjectileSlot RequireOneCircle(
        RoomEnemySystem enemies,
        RoomEnemyProjectileKind kind)
    {
        RoomEnemyProjectileSlot[] matches = enemies.EnemyProjectiles
            .Where(projectile => projectile.Kind == kind)
            .ToArray();
        if (matches.Length != 1)
        {
            throw new InvalidDataException(
                $"Shaktool dead-code producer created {matches.Length} ${kind:X4} circles.");
        }
        return matches[0];
    }

    private static void VerifyCircleInitialization(
        ISnesAddressSpace bus,
        CircleEncounter encounter)
    {
        RoomEnemyProjectileSlot[] actors =
            [encounter.Front, encounter.Middle, encounter.Back];
        ushort frontNativeIndex = unchecked((ushort)(encounter.Front.SlotIndex * 2));
        ushort expectedGraphics = unchecked((ushort)(
            encounter.Source.VramTilesIndex | encounter.Source.PaletteIndex));
        byte angle = unchecked((byte)encounter.Source.VariableD);
        int offsetIndex = angle >> 5;
        ushort expectedX = unchecked((ushort)(encounter.Source.XPosition +
            unchecked((short)ReadWord(bus, 0x86bde3 + offsetIndex * 2))));
        ushort expectedY = unchecked((ushort)(encounter.Source.YPosition +
            unchecked((short)ReadWord(bus, 0x86bdf3 + offsetIndex * 2))));
        ushort expectedXVelocity = ReadWord(bus, 0xa0b443 + angle * 2);
        ushort expectedYVelocity = ReadWord(
            bus,
            0xa0b443 + (((angle - 64) & 0xff) * 2));

        for (int index = 0; index < actors.Length; index++)
        {
            CircleDefinition expected = ShaktoolCircleDefinitions[index];
            RoomEnemyProjectileSlot actor = actors[index];
            int definitionAddress = 0x860000 | (ushort)expected.Kind;
            ushort packedRadii = ReadWord(bus, definitionAddress + 6);
            ushort properties = ReadWord(bus, definitionAddress + 8);
            ushort expectedOwner = index == 0 ? (ushort)0 : frontNativeIndex;
            if (ReadWord(bus, definitionAddress) != expected.Initializer ||
                actor.PreInstruction != ReadWord(bus, definitionAddress + 2) ||
                actor.PreInstruction != expected.InitialPreInstruction ||
                actor.InstructionPointer != ReadWord(bus, definitionAddress + 4) ||
                actor.InstructionPointer != expected.InstructionList ||
                actor.InstructionTimer != 1 ||
                actor.SpritemapPointer != BlankEnemyProjectileSpritemap ||
                actor.XRadius != unchecked((byte)packedRadii) ||
                actor.YRadius != unchecked((byte)(packedRadii >> 8)) ||
                actor.Damage != (properties & 0x0fff) || actor.Damage != expected.Damage ||
                actor.CanDamageSamus != expected.CanDamageSamus ||
                actor.PersistsOnSamusContact || actor.BlocksSamusProjectiles ||
                actor.CollisionOption != 0 || actor.InvincibilityFrames != 96 ||
                actor.Variable0 != expectedOwner ||
                actor.XPosition != expectedX || actor.YPosition != expectedY ||
                actor.XSubposition != 0 || actor.YSubposition != 0 ||
                actor.XVelocity != expectedXVelocity || actor.YVelocity != expectedYVelocity ||
                actor.GraphicsIndex != expectedGraphics)
            {
                throw new InvalidDataException(
                    $"Shaktool circle {expected.Kind} initialization mismatch: slot/native=" +
                    $"{actor.SlotIndex}/${actor.SlotIndex * 2:X2}, owner=${actor.Variable0:X4}, " +
                    $"position=(${actor.XPosition:X4},${actor.YPosition:X4}), velocity=" +
                    $"(${actor.XVelocity:X4},${actor.YVelocity:X4}), list/pre/map=" +
                    $"${actor.InstructionPointer:X4}/${actor.PreInstruction:X4}/" +
                    $"${actor.SpritemapPointer:X4}, radii={actor.XRadius}/{actor.YRadius}, " +
                    $"damage={actor.Damage}, flags={actor.CanDamageSamus}/" +
                    $"{actor.PersistsOnSamusContact}/{actor.BlocksSamusProjectiles}.");
            }
        }

        // A fresh eighteen-slot pool allocates downward exactly like native: front $22,
        // middle $20, rear $1E. More importantly, both delayed records must point to $22,
        // never to their own slot, the immediately preceding slot, or literal zero.
        if (encounter.Front.SlotIndex != 17 || encounter.Middle.SlotIndex != 16 ||
            encounter.Back.SlotIndex != 15 || frontNativeIndex != 0x22)
        {
            throw new InvalidDataException(
                $"Shaktool circle allocation order was front/middle/rear " +
                $"${encounter.Front.SlotIndex * 2:X2}/" +
                $"${encounter.Middle.SlotIndex * 2:X2}/" +
                $"${encounter.Back.SlotIndex * 2:X2}, expected $22/$20/$1E.");
        }
    }

    private static void VerifyAuthoredCircleMaps(
        ISnesAddressSpace bus,
        Dictionary<RoomEnemyProjectileKind, HashSet<ushort>> actual)
    {
        var expected = new Dictionary<RoomEnemyProjectileKind, HashSet<ushort>>
        {
            [RoomEnemyProjectileKind.ShaktoolAttackFrontCircle] =
                [ReadWord(bus, 0x86bd6a), ReadWord(bus, 0x86bd6e), ReadWord(bus, 0x86bd72)],
            [RoomEnemyProjectileKind.ShaktoolAttackMiddleCircle] =
                [ReadWord(bus, 0x86bd7a), ReadWord(bus, 0x86bd82), ReadWord(bus, 0x86bd86)],
            [RoomEnemyProjectileKind.ShaktoolAttackBackCircle] =
                [ReadWord(bus, 0x86bd8e), ReadWord(bus, 0x86bd96)],
        };

        foreach (CircleDefinition definition in ShaktoolCircleDefinitions)
        {
            if (!actual[definition.Kind].SetEquals(expected[definition.Kind]))
            {
                throw new InvalidDataException(
                    $"Shaktool {definition.Kind} maps [" +
                    $"{string.Join(',', actual[definition.Kind].Select(map => $"${map:X4}"))}] " +
                    $"did not equal cartridge maps [" +
                    $"{string.Join(',', expected[definition.Kind].Select(map => $"${map:X4}"))}].");
            }
        }
    }

    private static (ushort Position, ushort Subposition) AdvanceCircleVelocity(
        ushort position,
        ushort subposition,
        ushort velocity)
    {
        int fixedPosition = unchecked((position << 16) | subposition);
        fixedPosition = unchecked(fixedPosition + (unchecked((short)velocity) << 8));
        return (unchecked((ushort)(fixedPosition >> 16)), unchecked((ushort)fixedPosition));
    }

    private readonly record struct CircleDefinition(
        RoomEnemyProjectileKind Kind,
        ushort Initializer,
        ushort InitialPreInstruction,
        ushort InstructionList,
        ushort Damage,
        bool CanDamageSamus);

    private readonly record struct CircleEncounter(
        RoomEnemySystem Enemies,
        SamusState Samus,
        RoomEnemySlot Source,
        RoomEnemyProjectileSlot Front,
        RoomEnemyProjectileSlot Middle,
        RoomEnemyProjectileSlot Back);
}
