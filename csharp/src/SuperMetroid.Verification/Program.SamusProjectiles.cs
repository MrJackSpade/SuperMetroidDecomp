using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{

/// <summary>Samus beam and missile projectile verification.</summary>
static void VerifySamusPowerBeamProjectiles()
{
    VerifyPlasmaEnemyPenetration();
    var bus = new TestAddressSpace();

    // Construct the literal ROM records consumed by `$90:B887`, `$93:8000`, and
    // `$93:81E9`. Every direction points at one deliberately shared animation record; the
    // direction-table lookup itself is still exercised because all ten pointer cells must
    // be populated for the loop below to succeed.
    WriteTestWord(bus, 0x9383c1, 0x8431);
    WriteTestWord(bus, 0x938431, 0x0014);
    for (int direction = 0; direction < 10; direction++)
        WriteTestWord(bus, 0x938433 + direction * 2, 0x9000);
    WriteTestWord(bus, 0x939000, 0x000f);
    WriteTestWord(bus, 0x939002, 0xa000);
    bus.WriteByte(0x939004, 8);
    bus.WriteByte(0x939005, 4);
    WriteTestWord(bus, 0x939006, 0);
    WriteTestWord(bus, 0x939008, 0x8239);
    WriteTestWord(bus, 0x93900a, 0x9000);

    // Missile family one selects `$93:8641`; the non-beam table is indexed by type high
    // nibble before its ten direction records are selected.
    WriteTestWord(bus, 0x9383f3, 0x8641);
    WriteTestWord(bus, 0x938641, 0x0064);
    for (int direction = 0; direction < 10; direction++)
        WriteTestWord(bus, 0x938643 + direction * 2, 0x9200);
    bus.WriteBytes(0x939200, [
        0x0f, 0x00, 0x20, 0xa0, 0x04, 0x04, 0x00, 0x00,
        0x39, 0x82, 0x00, 0x92,
    ]);

    // Super Missiles use the adjacent non-beam family plus an invisible `$93:866D` link.
    // The link's empty spritemap is intentional: it exists for collision continuity and
    // shared-slot accounting, not as a second visible rocket.
    WriteTestWord(bus, 0x9383f5, 0x8657);
    WriteTestWord(bus, 0x938657, 0x012c);
    for (int direction = 0; direction < 10; direction++)
        WriteTestWord(bus, 0x938659 + direction * 2, 0x9240);
    bus.WriteBytes(0x939240, [
        0x0f, 0x00, 0x30, 0xa0, 0x08, 0x08, 0x00, 0x00,
        0x39, 0x82, 0x40, 0x92,
    ]);
    WriteTestWord(bus, 0x93842f, 0x866d);
    WriteTestWord(bus, 0x93866d, 0x012c);
    WriteTestWord(bus, 0x93866f, 0x9280);
    bus.WriteBytes(0x939280, [
        0x0f, 0x00, 0x40, 0xa0, 0x08, 0x08, 0x00, 0x00,
        0x39, 0x82, 0x80, 0x92,
    ]);

    // The animation record's `$A000` pointer is a bank-$93 spritemap, not merely an opaque
    // animation token. One literal entry makes the draw path observable independently of
    // the separate flare spritemap family seeded below.
    WriteTestWord(bus, 0x93a000, 1);
    WriteTestWord(bus, 0x93a002, 0);
    bus.WriteByte(0x93a004, 0);
    WriteTestWord(bus, 0x93a005, 0x2c20);
    // Both compact explosion programs below intentionally share `$A010`; give that pointer
    // its own visible one-OBJ map so the early explosion draw pass is tested, not inferred
    // from a nonzero animation pointer.
    WriteTestWord(bus, 0x93a010, 1);
    WriteTestWord(bus, 0x93a012, 0);
    bus.WriteByte(0x93a014, 0);
    WriteTestWord(bus, 0x93a015, 0x2c30);
    WriteTestWord(bus, 0x93a020, 1);
    WriteTestWord(bus, 0x93a022, 0);
    bus.WriteByte(0x93a024, 0);
    WriteTestWord(bus, 0x93a025, 0x2a44);
    WriteTestWord(bus, 0x93a030, 1);
    WriteTestWord(bus, 0x93a032, 0);
    bus.WriteByte(0x93a034, 0);
    WriteTestWord(bus, 0x93a035, 0x2a45);
    WriteTestWord(bus, 0x93a040, 0);

    // Charge-only power beam uses the parallel `$93:83D9` pointer family. Keep its
    // direction records shared but give it unmistakable damage so release cannot pass by
    // accidentally reusing the uncharged table.
    WriteTestWord(bus, 0x9383d9, 0x8460);
    WriteTestWord(bus, 0x938460, 0x0064);
    for (int direction = 0; direction < 10; direction++)
        WriteTestWord(bus, 0x938462 + direction * 2, 0x9000);

    // Fill the remaining eleven entries of both bank-$93 beam-data pointer tables with
    // distinct damage words. All directions deliberately share the already valid `$9000`
    // animation stream: these fixtures isolate the low-nibble table index without replacing
    // the production instruction interpreter or inventing host-side projectile art.
    for (int beamType = 1; beamType < 12; beamType++)
    {
        ushort unchargedData = unchecked((ushort)(0x8800 + beamType * 0x20));
        ushort chargedData = unchecked((ushort)(0x8a00 + beamType * 0x20));
        WriteTestWord(bus, 0x9383c1 + beamType * 2, unchargedData);
        WriteTestWord(bus, 0x9383d9 + beamType * 2, chargedData);
        WriteTestWord(bus, 0x930000 | unchargedData, unchecked((ushort)(0x0020 + beamType)));
        WriteTestWord(bus, 0x930000 | chargedData, unchecked((ushort)(0x0100 + beamType)));
        for (int direction = 0; direction < 10; direction++)
        {
            WriteTestWord(bus, 0x930000 | unchecked((ushort)(unchargedData + 2 + direction * 2)), 0x9000);
            WriteTestWord(bus, 0x930000 | unchecked((ushort)(chargedData + 2 + direction * 2)), 0x9000);
        }
    }

    // `$90:B5BB/$B609` select two independent trail instruction streams from the beam's
    // low six type bits. Ordinary power selects two empty lists; charged power selects the
    // long ice-style left list and an empty right list. These are genuine table relationships,
    // not a verifier-specific particle definition.
    WriteTestWord(bus, 0x90b5bb, 0xb4c9);
    WriteTestWord(bus, 0x90b609, 0xb4c9);
    WriteTestWord(bus, 0x90b5db, 0xb4cb);
    WriteTestWord(bus, 0x90b629, 0xb4c9);
    WriteTestWord(bus, 0x90b5fb, 0xb5a1);
    WriteTestWord(bus, 0x90b649, 0xb4c9);
    WriteTestWord(bus, 0x90b5fd, 0xb5a1);
    WriteTestWord(bus, 0x90b64b, 0xb4c9);
    WriteTestWord(bus, 0x90b4c9, 0x0000);
    bus.WriteBytes(0x90b5a1, [
        0x04, 0x00, 0x48, 0x2a,
        0x04, 0x00, 0x49, 0x2a,
        0x04, 0x00, 0x4a, 0x2a,
        0x04, 0x00, 0x4b, 0x2a,
        0x00, 0x00,
    ]);

    // Retain enough of retail `$90:B4CB` to prove repeated one-frame tiles and the embedded
    // `$B525` position command. The production interpreter remains ROM-driven and continues
    // through the full list when the real cartridge data is mounted.
    ushort chargedTrailInstruction = 0xb4cb;
    void WriteChargedTrailWord(ushort value)
    {
        WriteTestWord(bus, 0x900000 | chargedTrailInstruction, value);
        chargedTrailInstruction = unchecked((ushort)(chargedTrailInstruction + 2));
    }
    for (int record = 0; record < 4; record++)
    {
        WriteChargedTrailWord(1);
        WriteChargedTrailWord(0x2c38);
    }
    for (int record = 0; record < 2; record++)
    {
        WriteChargedTrailWord(1);
        WriteChargedTrailWord(0x2c39);
    }
    WriteChargedTrailWord(0xb525);
    WriteChargedTrailWord(1);
    WriteChargedTrailWord(0x2c39);
    WriteChargedTrailWord(0);

    // `$9B:A4B3/$A4CB` first choose a beam-combination family, then a direction-specific
    // offset list. Plain power's offsets are all zero, so `$9B:A3CC` positions the 8x8 trail
    // exactly four pixels above and left of the projectile's pre-movement center.
    WriteTestWord(bus, 0x9ba4b3, 0xa50b);
    WriteTestWord(bus, 0x9ba4cb, 0xa98f);
    for (int direction = 0; direction < 10; direction++)
    {
        WriteTestWord(bus, 0x9ba50b + direction * 2, 0xa56f);
        WriteTestWord(bus, 0x9ba98f + direction * 2, 0xaa07);
    }
    bus.WriteBytes(0x9ba56f, new byte[32]);
    bus.WriteBytes(0x9baa07, new byte[32]);

    // `$93:83FF` does NOT point directly at animation bytecode. It selects the two-word
    // non-beam data record at `$93:8679`: ignored damage eight followed by the actual
    // instruction-list pointer at `$93:867B`. Mirroring both indirections is important.
    // A regression that reads `$83FF` as the list will interpret `$8679`'s data tables as
    // animation records, publish garbage spritemaps, and never reach the delete opcode.
    WriteTestWord(bus, 0x9383ff, 0x8679);
    WriteTestWord(bus, 0x938679, 0x0008);
    WriteTestWord(bus, 0x93867b, 0x9100);

    // Collision swaps to this two-frame explosion record. Its following delete opcode
    // proves that damage remains occupied during the explosion and decrements the separate
    // projectile counter only when `$93:822F` finally clears the slot.
    WriteTestWord(bus, 0x939100, 0x0002);
    WriteTestWord(bus, 0x939102, 0xa010);
    bus.WriteByte(0x939104, 8);
    bus.WriteByte(0x939105, 8);
    WriteTestWord(bus, 0x939106, 0);
    WriteTestWord(bus, 0x939108, 0x822f);

    // `$93:867F` is the missile-explosion instruction pointer consumed by `$93:80CF`.
    WriteTestWord(bus, 0x93867f, 0x9300);
    bus.WriteBytes(0x939300, [
        0x02, 0x00, 0x10, 0xa0, 0x08, 0x08, 0x00, 0x00,
        0x2f, 0x82,
    ]);
    WriteTestWord(bus, 0x938693, 0x9340);
    bus.WriteBytes(0x939340, [
        0x02, 0x00, 0x10, 0xa0, 0x08, 0x08, 0x00, 0x00,
        0x2f, 0x82,
    ]);

    bus.WriteByte(0x90c254, 0x0f);
    bus.WriteByte(0x90c264, 0x1e);
    WriteTestWord(bus, 0x90c28f, 0x000b);
    WriteTestWord(bus, 0x90c2a7, 0x0017);
    for (int beamType = 1; beamType < 12; beamType++)
    {
        // Distinct fixture bytes/words make an accidental entry-zero read immediately
        // observable. Charged cooldown indices begin at `$10`; auto-fire has its own twelve
        // byte table even though retail happens to store `$19` in every entry.
        bus.WriteByte(0x90c254 + beamType, unchecked((byte)(10 + beamType)));
        bus.WriteByte(0x90c264 + beamType, unchecked((byte)(30 + beamType)));
        bus.WriteByte(0x90c283 + beamType, unchecked((byte)(40 + beamType)));
        WriteTestWord(bus, 0x90c28f + beamType * 2, unchecked((ushort)(0x0030 + beamType)));
        WriteTestWord(bus, 0x90c2a7 + beamType * 2, unchecked((ushort)(0x0050 + beamType)));
    }
    for (int beamType = 0; beamType < 12; beamType++)
    {
        WriteTestWord(bus, SamusBeamPreInstructionCodes.UnchargedTable + beamType * 2,
            (beamType & 1) == 0 ? SamusBeamPreInstructionCodes.NoWave : beamType < 4
                ? SamusBeamPreInstructionCodes.WaveThreeFrameTrail : SamusBeamPreInstructionCodes.WaveFourFrameTrail);
        WriteTestWord(bus, SamusBeamPreInstructionCodes.ChargedTable + beamType * 2,
            (beamType & 1) == 0 ? SamusBeamPreInstructionCodes.NoWave : SamusBeamPreInstructionCodes.WaveFourFrameTrail);
    }
    WriteTestWord(bus, 0x90c3b1, 0x8000);
    WriteTestWord(bus, 0x90c3c9, 0xc3e1);
    for (int index = 0; index < 0x100; index++)
        bus.WriteByte(0x9a8000 + index, unchecked((byte)(index ^ 0x5a)));
    for (int index = 0; index < 16; index++)
        WriteTestWord(bus, 0x90c3e1 + index * 2, unchecked((ushort)(0x0100 + index)));

    // Samus body palette fixtures for `$91:D743`. The normal table uses all three native
    // byte offsets so a Gravity restore can prove selection priority independently of the
    // ten Hyper-shot pointers. Every palette/color word is unique and remains valid BGR555.
    ushort[] normalSuitPalettePointers = [0xe300, 0xe320, 0xe340];
    for (int suit = 0; suit < normalSuitPalettePointers.Length; suit++)
    {
        WriteTestWord(bus, 0x91d727 + suit * 2, normalSuitPalettePointers[suit]);
        for (int color = 0; color < 16; color++)
        {
            WriteTestWord(
                bus,
                0x9b0000 | unchecked((ushort)(normalSuitPalettePointers[suit] + color * 2)),
                unchecked((ushort)(0x0100 + suit * 0x20 + color)));
        }
    }
    ushort[] hyperShotPalettePointers = new ushort[10];
    for (int palette = 0; palette < hyperShotPalettePointers.Length; palette++)
    {
        ushort pointer = unchecked((ushort)(0xe400 + palette * 0x20));
        hyperShotPalettePointers[palette] = pointer;
        // `$91:D829` has one padding word; timer `$8014` indexes byte offset 20 and thus
        // palette zero, descending by one palette on each later even timer value.
        WriteTestWord(bus, 0x91d829 + 0x14 - palette * 2, pointer);
        for (int color = 0; color < 16; color++)
        {
            WriteTestWord(
                bus,
                0x9b0000 | unchecked((ushort)(pointer + color * 2)),
                unchecked((ushort)(0x2000 + palette * 0x20 + color)));
        }
    }

    // `$91:D7D5-$D827` is two levels of pointers: each family first selects a
    // suit-specific list in bank $91, then `$0B62` selects one of six bank-$9B palettes.
    // Keep every family/suit/frame distinct so a wrong table, offset unit, or wrap cannot
    // accidentally agree with the expected pixels.
    ushort[,] beamChargePalettePointers = new ushort[3, 6];
    ushort[,] pseudoScrewPalettePointers = new ushort[3, 6];
    for (int suit = 0; suit < 3; suit++)
    {
        ushort chargeListPointer = unchecked((ushort)(0xe600 + suit * 12));
        ushort pseudoListPointer = unchecked((ushort)(0xe624 + suit * 12));
        WriteTestWord(bus, 0x91d7d5 + suit * 2, chargeListPointer);
        WriteTestWord(bus, 0x91d7ff + suit * 2, pseudoListPointer);
        for (int palette = 0; palette < 6; palette++)
        {
            ushort chargePointer = unchecked((ushort)(0xe800 + (suit * 6 + palette) * 0x20));
            ushort pseudoPointer = unchecked((ushort)(0xeb00 + (suit * 6 + palette) * 0x20));
            beamChargePalettePointers[suit, palette] = chargePointer;
            pseudoScrewPalettePointers[suit, palette] = pseudoPointer;
            WriteTestWord(bus, 0x910000 | unchecked((ushort)(chargeListPointer + palette * 2)), chargePointer);
            WriteTestWord(bus, 0x910000 | unchecked((ushort)(pseudoListPointer + palette * 2)), pseudoPointer);
            for (int color = 0; color < 16; color++)
            {
                WriteTestWord(
                    bus,
                    0x9b0000 | unchecked((ushort)(chargePointer + color * 2)),
                    unchecked((ushort)(0x3000 + suit * 0x100 + palette * 0x20 + color)));
                WriteTestWord(
                    bus,
                    0x9b0000 | unchecked((ushort)(pseudoPointer + color * 2)),
                    unchecked((ushort)(0x4000 + suit * 0x100 + palette * 0x20 + color)));
            }
        }
    }

    // Minimal but structurally authentic flare tables let the verifier exercise bank
    // `$90:BAFC` -> `$81:8A37` without copying production animation logic. Every possible
    // early table index selects the same one-entry spritemap; timing still comes from the
    // three independently addressed delay lists.
    for (int index = 0; index < 0x36; index++)
        WriteTestWord(bus, 0x93a1a1 + index * 2, 0xa500);
    WriteTestWord(bus, 0x93a500, 1);
    WriteTestWord(bus, 0x93a502, 0);
    bus.WriteByte(0x93a504, 0);
    WriteTestWord(bus, 0x93a505, 0x2c30);
    WriteTestWord(bus, 0x90c481, 0xc487);
    WriteTestWord(bus, 0x90c483, 0xc4a7);
    WriteTestWord(bus, 0x90c485, 0xc4ae);
    for (int index = 0; index < 30; index++)
        bus.WriteByte(0x90c487 + index, 3);
    bus.WriteByte(0x90c4a7, 5);
    bus.WriteByte(0x90c4a8, 0xff);
    bus.WriteByte(0x90c4ae, 4);
    bus.WriteByte(0x90c4af, 0xff);
    // Motion definitions are compiled independently of the synthetic presentation bus.

    const int width = 32;
    const int height = 16;
    RoomLevelData air = new(
        width,
        height,
        new ushort[width * height],
        new byte[width * height],
        new ushort[width * height],
        new byte[8]);

    // Update_Beam_Tiles_and_Palette queues exactly $100 bytes to VRAM word $6300 and
    // writes sprite palette six. Drain the ordinary queue so this also validates the same
    // hardware path used by runtime room setup, not a verifier-only direct memory copy.
    var beamVram = new SnesVram();
    var beamCgram = new SnesCgram();
    var beamWrites = new VramWriteQueue();
    var beamGraphics = new SamusProjectileSystem();
        SamusProjectileSystem.QueueBeamTilesAndLoadPalette(bus, beamWrites, beamCgram, equippedBeams: 0);
    AssertEqual(1, beamWrites.Entries.Count, "power beam queues one tile DMA");
    beamWrites.DrainTo(beamVram, bus);
    AssertEqual(0x5a, beamVram.ReadByte(0x6300 * 2),
        "power beam tiles begin at VRAM word $6300");
    AssertEqual(0xa5, beamVram.ReadByte(0x6300 * 2 + 0xff),
        "power beam tile DMA copies exactly $100 source bytes");
    AssertEqual(0x0100, beamCgram.Colors[0xe0],
        "power beam palette begins at OBJ palette six");
    AssertEqual(0x010f, beamCgram.Colors[0xef],
        "power beam palette copies sixteen colors");

    // `$90:BA56` accepts exactly ten low-nibble direction values. Exercise every pointer,
    // horizontal/vertical/diagonal base-speed choice, acceleration sign, immediate movement,
    // animation selection, cooldown, sound, and live-slot counter in isolation.
    for (byte direction = 0; direction < 10; direction++)
    {
        byte pose = unchecked((byte)(0x20 + direction));
        WritePoseDefinition(
            bus,
            pose,
            [0x08, 0x00, 0x00, direction, 0x00, 0x00, 0x00, 0x00]);
        var samus = new SamusState
        {
            Pose = pose,
            XPosition = 128,
            YPosition = 96,
            EquippedBeams = 0,
            SelectedHudItem = 0,
        };
        var bombs = new SamusBombProjectileSystem();
        var projectiles = new SamusProjectileSystem();
        bombs.StepFrame(bus, air, samus, 0, 0);
        SamusProjectileFrameResult result = projectiles.StepFrame(
            bus,
            air,
            samus,
            (ushort)SnesButton.X,
            (ushort)SnesButton.X,
            0,
            0,
            bombs);

        AssertEqual((int?)0, result.FiredSlot, $"power beam direction {direction} allocates slot zero");
        AssertEqual(
            (SoundEffectId?)SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 0x000b),
            result.QueuedSoundEffect,
            $"power beam direction {direction} queues ROM sound");
        AssertEqual(1, projectiles.ProjectileCounter,
            $"power beam direction {direction} increments counter");
        AssertEqual(0x000f, bombs.CooldownTimer,
            $"power beam direction {direction} installs cooldown");
        AssertEqual(direction, projectiles.Slots[0].Direction,
            $"power beam direction {direction} survives initialization");
        AssertEqual(0x0014, projectiles.Slots[0].Damage,
            $"power beam direction {direction} loads damage");
        AssertEqual(0xa000, projectiles.Slots[0].SpritemapPointer,
            $"power beam direction {direction} selects first art record");
        AssertEqual(8, projectiles.Slots[0].XRadius,
            $"power beam direction {direction} loads X radius");
        AssertEqual(4, projectiles.Slots[0].YRadius,
            $"power beam direction {direction} loads Y radius");
    }

    // Charge Beam fires an ordinary shot on the initial held frame, counts to sixty while
    // the muzzle flare becomes visible at fifteen, and emits the charged data family only
    // when Shoot is released. This sequence mirrors `$90:B80D` frame by frame.
    const byte rightPose = 1;
    WritePoseDefinition(
        bus,
        rightPose,
        [0x08, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00]);

    // Every valid low-nibble combination must index its own projectile data, cooldown, and
    // sound cells. The dispatch split is equally data-significant: even entries stop on
    // terrain, low wave entries 1/3 reload trail timer three, and wave entries 5/7/9/11
    // reload four. Spazer/plasma remain one slot because their width lives in spritemap art.
    for (ushort beamType = 1; beamType < 12; beamType++)
    {
        var combinedSamus = new SamusState
        {
            Pose = rightPose,
            XPosition = 128,
            YPosition = 96,
            EquippedBeams = beamType,
        };
        var combinedBombs = new SamusBombProjectileSystem();
        var combinedProjectiles = new SamusProjectileSystem();
        combinedBombs.StepFrame(bus, air, combinedSamus, 0, 0);
        SamusProjectileFrameResult combinedResult = combinedProjectiles.StepFrame(
            bus,
            air,
            combinedSamus,
            (ushort)SnesButton.X,
            (ushort)SnesButton.X,
            0,
            0,
            combinedBombs);

        SamusProjectileSlot combinedSlot = combinedProjectiles.Slots[0];
        AssertEqual((int?)0, combinedResult.FiredSlot,
            $"beam combination {beamType} allocates one ordinary slot");
        AssertEqual(unchecked((ushort)(0x0020 + beamType)), combinedSlot.Damage,
            $"beam combination {beamType} indexes uncharged data pointer");
        AssertEqual(unchecked((ushort)(10 + beamType)), combinedBombs.CooldownTimer,
            $"beam combination {beamType} indexes uncharged cooldown");
        AssertEqual(
            (SoundEffectId?)SoundEffectId.FromCartridge(
                SoundEffectLibrary.Library1,
                unchecked((ushort)(0x0030 + beamType))),
            combinedResult.QueuedSoundEffect,
            $"beam combination {beamType} indexes uncharged sound");
        AssertEqual(10, combinedProjectiles.ProjectileInvincibilityTimer,
            $"beam combination {beamType} publishes native invincibility timer");

        SamusProjectilePreInstruction expectedPreInstruction = (beamType & 1) == 0
            ? SamusProjectilePreInstruction.NoWaveBeam
            : beamType < 4
                ? SamusProjectilePreInstruction.WaveBeamThreeFrameTrail
                : SamusProjectilePreInstruction.WaveBeamFourFrameTrail;
        AssertEqual(expectedPreInstruction, combinedSlot.PreInstruction,
            $"beam combination {beamType} selects native pre-instruction family");
    }

    // Spazer/plasma art takes `$93:8275` rather than the ordinary power/ice/wave flicker
    // branch. Host slot zero is visible while NMI bit one is clear and suppressed while it
    // is set—the opposite comparison and a different counter bit from entry-zero power.
    var spazerFlickerSamus = new SamusState
    {
        Pose = rightPose,
        XPosition = 128,
        YPosition = 96,
        EquippedBeams = 4,
    };
    var spazerFlickerBombs = new SamusBombProjectileSystem();
    var spazerFlickerProjectiles = new SamusProjectileSystem();
    spazerFlickerBombs.StepFrame(bus, air, spazerFlickerSamus, 0, 0);
    spazerFlickerProjectiles.StepFrame(
        bus,
        air,
        spazerFlickerSamus,
        (ushort)SnesButton.X,
        (ushort)SnesButton.X,
        0,
        0,
        spazerFlickerBombs);
    var spazerFlickerOam = new OamBuffer();
    spazerFlickerOam.BeginFrame();
    spazerFlickerProjectiles.DrawLiveProjectiles(
        bus, spazerFlickerOam, 0, 0, nmiFrameCounter: 0);
    AssertTrue(spazerFlickerOam.NextByteOffset != 0,
        "even-slot Spazer draws while NMI bit one is clear");
    spazerFlickerOam.BeginFrame();
    spazerFlickerProjectiles.DrawLiveProjectiles(
        bus, spazerFlickerOam, 0, 0, nmiFrameCounter: 2);
    AssertEqual(0, spazerFlickerOam.NextByteOffset,
        "even-slot Spazer suppresses while NMI bit one is set");

    // The initial firing pass changes timer four to three. Three more wave passes cause the
    // first trail allocation and expose the only cadence distinction in `$B0C3/$B0E4`.
    foreach ((ushort beamType, ushort expectedReload) in new (ushort, ushort)[]
    {
        (1, 3), // Uncharged power+wave uses `$90:B0E4`.
        (3, 3), // Uncharged ice+wave uses the same low-family routine.
        (5, 4), // Spazer+wave uses the common `$90:B0C3` routine.
        (9, 4), // Plasma+wave also uses the common routine.
    })
    {
        var cadenceSamus = new SamusState
        {
            Pose = rightPose,
            XPosition = 128,
            YPosition = 96,
            EquippedBeams = beamType,
        };
        var cadenceBombs = new SamusBombProjectileSystem();
        var cadenceProjectiles = new SamusProjectileSystem();
        cadenceBombs.StepFrame(bus, air, cadenceSamus, 0, 0);
        cadenceProjectiles.StepFrame(
            bus, air, cadenceSamus, (ushort)SnesButton.X, (ushort)SnesButton.X, 0, 0, cadenceBombs);
        for (int frame = 0; frame < 3; frame++)
        {
            cadenceBombs.StepFrame(bus, air, cadenceSamus, 0, 0);
            cadenceProjectiles.StepFrame(bus, air, cadenceSamus, 0, 0, 0, 0, cadenceBombs);
        }
        AssertEqual(expectedReload, cadenceProjectiles.Slots[0].TrailTimer,
            $"wave combination {beamType} reloads native trail cadence");
    }

    var chargeSamus = new SamusState
    {
        Pose = rightPose,
        XPosition = 128,
        YPosition = 96,
        EquippedBeams = 0x1000,
    };
    var chargeBombs = new SamusBombProjectileSystem();
    var chargeProjectiles = new SamusProjectileSystem();
    var flareOam = new OamBuffer();
    bool flareBecameVisible = false;
    for (int frame = 0; frame < 60; frame++)
    {
        chargeBombs.StepFrame(bus, air, chargeSamus, 0, 0);
        SamusProjectileFrameResult chargeFrame = chargeProjectiles.StepFrame(
            bus,
            air,
            chargeSamus,
            (ushort)SnesButton.X,
            frame == 0 ? (ushort)SnesButton.X : (ushort)0,
            0,
            0,
            chargeBombs);
        if (frame == 0)
            AssertEqual((int?)0, chargeFrame.FiredSlot, "charge press fires initial ordinary shot");
        else if (frame == 15)
        {
            AssertEqual(
                (SoundEffectId?)SoundEffectLibrary1Sounds.ChargeBeamStart,
                chargeFrame.QueuedSoundEffect,
                "charge counter sixteen queues the cartridge startup sound");
            AssertEqual((byte)9, chargeFrame.QueuedSoundMaximum,
                "charge startup uses QueueSfx1_Max9");
        }
        else
        {
            AssertEqual((SoundEffectId?)null, chargeFrame.QueuedSoundEffect,
                $"charge held frame {frame + 1} does not restart its sustained sound");
        }

        flareOam.BeginFrame();
        chargeProjectiles.HandleChargeFlareAndDraw(bus, flareOam, chargeSamus, 0, 0);
        // Runtime's later `$93:82F7` draw phase must run on every simulated gameplay frame.
        // The ordinary power beam periodically allocates two empty streams; `$90:B6A9`
        // consumes their zero terminators immediately instead of leaving timer-one slots.
        chargeProjectiles.HandleTrailsAndDraw(bus, flareOam, 0, 0, timeIsFrozen: false);
        flareBecameVisible |= flareOam.NextByteOffset != 0;
    }
    AssertEqual(60, chargeProjectiles.FlareCounter,
        "charge held frames reach armed threshold");
    AssertTrue(flareBecameVisible, "charge flare becomes visible from ROM spritemap table");
    VerifySpinChargePreservation(bus, air);

    // Make the release allocation fail through the real shared-cooldown gate. Native
    // FireUnchargedBeam still stops a charge that reached sound-start counter sixteen,
    // even though no replacement firing sequence can be queued.
    var rejectedReleaseSamus = new SamusState
    {
        Pose = rightPose,
        XPosition = 128,
        YPosition = 96,
        EquippedBeams = (ushort)SamusBeamFlags.Charge,
    };
    var rejectedReleaseBombs = new SamusBombProjectileSystem();
    var rejectedReleaseProjectiles = new SamusProjectileSystem();
    for (int frame = 0;
        frame < SamusProjectileRomData.Beams.ChargeSoundStartCounter;
        frame++)
    {
        rejectedReleaseBombs.StepFrame(bus, air, rejectedReleaseSamus, 0, 0);
        rejectedReleaseProjectiles.StepFrame(
            bus,
            air,
            rejectedReleaseSamus,
            (ushort)SnesButton.X,
            frame == 0 ? (ushort)SnesButton.X : (ushort)0,
            0,
            0,
            rejectedReleaseBombs);
    }
    rejectedReleaseBombs.SetSharedCooldown(1);
    SamusProjectileFrameResult rejectedRelease = rejectedReleaseProjectiles.StepFrame(
        bus, air, rejectedReleaseSamus, 0, 0, 0, 0, rejectedReleaseBombs);
    AssertEqual((int?)null, rejectedRelease.FiredSlot,
        "cooldown gate rejects charged-audio release fixture");
    AssertEqual((SoundEffectId?)SoundEffectLibrary1Sounds.CancelAll,
        rejectedRelease.QueuedSoundEffect,
        "rejected post-sound charge release queues cartridge cancellation");
    AssertEqual((byte)15, rejectedRelease.QueuedSoundMaximum,
        "rejected charge release uses QueueSfx1_Max15 cancellation");

    // Build two identical fifteen-call charge states so their central flare frame and
    // spritemap are identical. Rotating Samus's (+8,0) displacement around (120,96) by
    // 90 degrees moves the temporary render center from (128,96) to (120,104): every OBJ
    // entry must therefore shift exactly (-8,+8), while both physical Samus points stay put.
    var baselineMode7Samus = new SamusState
    {
        Pose = rightPose,
        XPosition = 128,
        YPosition = 96,
        EquippedBeams = 0x1000,
    };
    var rotatedMode7Samus = new SamusState
    {
        Pose = rightPose,
        XPosition = 128,
        YPosition = 96,
        EquippedBeams = 0x1000,
    };
    var baselineMode7Bombs = new SamusBombProjectileSystem();
    var rotatedMode7Bombs = new SamusBombProjectileSystem();
    var baselineMode7Projectiles = new SamusProjectileSystem();
    var rotatedMode7Projectiles = new SamusProjectileSystem();
    for (int frame = 0; frame < 15; frame++)
    {
        baselineMode7Bombs.StepFrame(bus, air, baselineMode7Samus, 0, 0);
        rotatedMode7Bombs.StepFrame(bus, air, rotatedMode7Samus, 0, 0);
        ushort newInput = frame == 0 ? (ushort)SnesButton.X : (ushort)0;
        baselineMode7Projectiles.StepFrame(
            bus, air, baselineMode7Samus, (ushort)SnesButton.X, newInput,
            0, 0, baselineMode7Bombs);
        rotatedMode7Projectiles.StepFrame(
            bus, air, rotatedMode7Samus, (ushort)SnesButton.X, newInput,
            0, 0, rotatedMode7Bombs);
    }
    var baselineMode7FlareOam = new OamBuffer();
    var rotatedMode7FlareOam = new OamBuffer();
    baselineMode7FlareOam.BeginFrame();
    rotatedMode7FlareOam.BeginFrame();
    baselineMode7Projectiles.HandleChargeFlareAndDraw(
        bus, baselineMode7FlareOam, baselineMode7Samus, 0, 0);
    rotatedMode7Projectiles.HandleChargeFlareAndDraw(
        bus,
        rotatedMode7FlareOam,
        rotatedMode7Samus,
        0,
        0,
        new SamusMode7Transform(0, 0x0100, 0xff00, 120, 96));
    AssertTrue(baselineMode7FlareOam.NextByteOffset != 0,
        "Mode 7 flare fixture reaches visible central component");
    AssertEqual(baselineMode7FlareOam.NextByteOffset, rotatedMode7FlareOam.NextByteOffset,
        "Mode 7 transform preserves charge-flare OBJ count");
    int flareEntryCount = baselineMode7FlareOam.NextByteOffset / 4;
    for (int entry = 0; entry < flareEntryCount; entry++)
    {
        OamEntry baselineEntry = baselineMode7FlareOam.GetEntry(entry);
        OamEntry rotatedEntry = rotatedMode7FlareOam.GetEntry(entry);
        AssertEqual((baselineEntry.X - 8) & 0x01ff, rotatedEntry.X,
            $"Mode 7 charge-flare OBJ {entry} receives transformed center X");
        AssertEqual(unchecked((byte)(baselineEntry.Y + 8)), rotatedEntry.Y,
            $"Mode 7 charge-flare OBJ {entry} receives transformed center Y");
    }
    AssertSamusPosition(
        128,
        96,
        rotatedMode7Samus,
        "Mode 7 flare calculation preserves physical Samus position");

    // While `$0B18` is zero and grapple is inactive, a fully charged beam cycles all six
    // entries once per palette call. Verify Power/Varia/Gravity independently for both the
    // normal charge and contact-damage-index-four pseudo-screw families. Six calls must
    // wrap the native byte offset 0,2,4,6,8,10 back to zero.
    var liveChargeCgram = new SnesCgram();
    ushort[] suitEquipment = [0x0000, 0x0001, 0x0020];
    for (int family = 0; family < 2; family++)
    {
        bool pseudoScrew = family == 1;
        chargeSamus.HorizontalSpeed.ContactDamageIndex = pseudoScrew ? (ushort)4 : (ushort)0;
        for (int suit = 0; suit < suitEquipment.Length; suit++)
        {
            chargeSamus.EquippedItems = suitEquipment[suit];
            for (int palette = 0; palette < 6; palette++)
            {
                SamusBeamChargePaletteStepResult liveChargeStep =
                    chargeProjectiles.UpdateBeamChargePalette(bus, liveChargeCgram, chargeSamus);
                ushort expectedPointer = pseudoScrew
                    ? pseudoScrewPalettePointers[suit, palette]
                    : beamChargePalettePointers[suit, palette];
                AssertEqual(
                    pseudoScrew
                        ? SamusBeamChargePaletteAction.PseudoScrewCycle
                        : SamusBeamChargePaletteAction.ChargeCycle,
                    liveChargeStep.Action,
                    $"{(pseudoScrew ? "pseudo-screw" : "beam-charge")} suit {suit} palette {palette} branch");
                AssertEqual(palette, liveChargeStep.ChargePaletteIndex,
                    $"{(pseudoScrew ? "pseudo-screw" : "beam-charge")} exposes palette ordinal");
                AssertEqual(expectedPointer, liveChargeStep.PalettePointer,
                    $"{(pseudoScrew ? "pseudo-screw" : "beam-charge")} uses exact nested ROM pointer");
                for (int color = 0; color < 16; color++)
                {
                    ushort expectedColor = unchecked((ushort)(
                        (pseudoScrew ? 0x4000 : 0x3000) +
                        suit * 0x100 + palette * 0x20 + color));
                    AssertEqual(expectedColor, liveChargeCgram.Colors[192 + color],
                        $"{(pseudoScrew ? "pseudo-screw" : "beam-charge")} suit {suit} " +
                        $"palette {palette} color {color}");
                }
            }
            AssertEqual(0, chargeProjectiles.SamusChargePaletteIndex,
                $"{(pseudoScrew ? "pseudo-screw" : "beam-charge")} six-entry list wraps");
        }
    }

    // Any grapple function other than inactive takes `$D7B0` immediately. Seed index two
    // with one eligible call, then prove a firing grapple resets it instead of merely
    // pausing and resuming on the second palette.
    chargeSamus.HorizontalSpeed.ContactDamageIndex = 0;
    chargeSamus.EquippedItems = 0;
    chargeProjectiles.UpdateBeamChargePalette(bus, liveChargeCgram, chargeSamus);
    AssertEqual(2, chargeProjectiles.SamusChargePaletteIndex,
        "one live charge-palette call advances byte offset to two");
    chargeSamus.Grapple.Phase = GrapplePhase.Firing;
    SamusBeamChargePaletteStepResult grappleSuppressesCharge =
        chargeProjectiles.UpdateBeamChargePalette(bus, liveChargeCgram, chargeSamus);
    AssertEqual(SamusBeamChargePaletteAction.Inactive, grappleSuppressesCharge.Action,
        "active grapple suppresses charge-body palette");
    AssertEqual(0, chargeProjectiles.SamusChargePaletteIndex,
        "active grapple resets charge-palette byte offset");
    chargeSamus.Grapple.Phase = GrapplePhase.Inactive;

    chargeBombs.StepFrame(bus, air, chargeSamus, 0, 0);
    SamusProjectileFrameResult chargedRelease = chargeProjectiles.StepFrame(
        bus, air, chargeSamus, 0, 0, 0, 0, chargeBombs);
    AssertTrue(chargedRelease.FiredSlot is not null, "charged release allocates projectile");
    SamusProjectileSlot chargedSlot = chargeProjectiles.Slots[chargedRelease.FiredSlot!.Value];
    AssertEqual(0x0064, chargedSlot.Damage, "charged release uses charged data pointer");
    AssertEqual(0x0010, unchecked((ushort)(chargedSlot.Type & 0x0010)),
        "charged release sets charged type bit");
    AssertEqual(
        (SoundEffectId?)SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 0x0017),
        chargedRelease.QueuedSoundEffect,
        "charged power beam queues ROM sound");
    AssertEqual(0x001e, chargeBombs.CooldownTimer,
        "charged power beam installs charged cooldown");
    AssertEqual(4, chargeProjectiles.ChargedShotGlowTimer,
        "charged release installs glow timer");
    AssertEqual(0, chargeProjectiles.FlareCounter,
        "charged release clears flare counter");

    // `$91:D799-$D7AE` decrements before testing zero. Calls one through three paint only
    // colors 1-15 white; call four takes carry-set back to `$91:D717` and restores the
    // complete suit palette. Seed transparent color zero with a sentinel to catch a broad
    // sixteen-color white fill, and equip both suit bits to prove Gravity wins over Varia.
    chargeSamus.EquippedItems = 0x0021;
    var chargedGlowCgram = new SnesCgram();
    chargedGlowCgram.SetColor(192, 0x4321);
    for (int call = 0; call < 4; call++)
    {
        SamusBeamChargePaletteStepResult paletteStep =
            chargeProjectiles.UpdateBeamChargePalette(bus, chargedGlowCgram, chargeSamus);
        if (call < 3)
        {
            AssertEqual(SamusBeamChargePaletteAction.OrdinaryWhite, paletteStep.Action,
                $"ordinary charged glow call {call + 1} selects white branch");
            AssertEqual(unchecked((ushort)(3 - call)), paletteStep.TimerAfter,
                $"ordinary charged glow call {call + 1} decrements before branch");
            AssertEqual(0x4321, chargedGlowCgram.Colors[192],
                $"ordinary charged glow call {call + 1} preserves transparent color zero");
            for (int color = 1; color < 16; color++)
            {
                AssertEqual(0x03ff, chargedGlowCgram.Colors[192 + color],
                    $"ordinary charged glow call {call + 1} paints visible color {color}");
            }
        }
        else
        {
            AssertEqual(SamusBeamChargePaletteAction.RestoredNormalSuit, paletteStep.Action,
                "ordinary charged glow fourth call restores suit");
            AssertEqual(normalSuitPalettePointers[2], paletteStep.PalettePointer,
                "ordinary charged glow restore selects Gravity pointer");
            for (int color = 0; color < 16; color++)
            {
                AssertEqual(unchecked((ushort)(0x0140 + color)), chargedGlowCgram.Colors[192 + color],
                    $"ordinary charged glow restore copies Gravity color {color}");
            }
        }
    }
    AssertEqual(0, chargeProjectiles.ChargedShotGlowTimer,
        "ordinary charged glow ends exactly on fourth palette call");

    // Recreate `$91:F5CF-$F5E6 -> $90:B82D/$BA5F -> $90:EB20`: a new Shoot edge while
    // normal-jump pose initialization occurs publishes `$8000 | direction` after this
    // frame's projectile phase. On the next phase it forces release even though Shoot is
    // still held, supplies the stored direction to the new projectile, and is then cleared.
    WritePoseDefinition(
        bus,
        SamusPoseIds.NeutralJumpTransitionRightPose,
        [0x08, 0x02, 0xff, 0x02, 0x00, 0x00, 0x13, 0x00]);
    var bridgeSamus = new SamusState
    {
        Pose = rightPose,
        XPosition = 128,
        YPosition = 96,
        EquippedBeams = 0x1000,
    };
    var bridgeBombs = new SamusBombProjectileSystem();
    var bridgeProjectiles = new SamusProjectileSystem();
    for (int frame = 0; frame < 60; frame++)
    {
        bridgeBombs.StepFrame(bus, air, bridgeSamus, 0, 0);
        bridgeProjectiles.StepFrame(
            bus,
            air,
            bridgeSamus,
            (ushort)SnesButton.X,
            frame == 0 ? (ushort)SnesButton.X : (ushort)0,
            0,
            0,
            bridgeBombs);
    }
    bridgeSamus.ApplyOrdinaryJumpTransition(
        bus,
        SamusPoseIds.NeutralJumpTransitionRightPose,
        controllerNewInput: (ushort)SnesButton.X);
    AssertEqual(0x8002, bridgeSamus.PoseTransitionShotDirection,
        "normal-jump initializer publishes tagged shot direction");
    bridgeBombs.StepFrame(bus, air, bridgeSamus, 0, 0);
    SamusProjectileFrameResult bridgeRelease = bridgeProjectiles.StepFrame(
        bus,
        air,
        bridgeSamus,
        (ushort)SnesButton.X,
        controllerNewInput: 0,
        layer1X: 0,
        layer1Y: 0,
        sharedProjectiles: bridgeBombs);
    AssertTrue(bridgeRelease.FiredSlot is not null,
        "pose-direction bridge forces charged release while Shoot remains held");
    AssertEqual(2,
        bridgeProjectiles.Slots[bridgeRelease.FiredSlot!.Value].Direction,
        "pose-direction bridge supplies stored low-byte direction");
    AssertEqual(0, bridgeProjectiles.FlareCounter,
        "pose-direction bridge consumes charge counter");
    bridgeSamus.ClearPoseTransitionShotDirection();
    AssertEqual(0, bridgeSamus.PoseTransitionShotDirection,
        "current-state epilogue clears pose-direction bridge");

    // Charged combinations index the parallel pointer/sound range and cooldown bytes
    // `$10-$1B`. Even a low-family wave now uses the common four-frame wave routine; the
    // special three-frame reload belongs only to uncharged types one and three.
    foreach (ushort beamType in new ushort[] { 1, 4, 5, 9, 11 })
    {
        var combinedChargeSamus = new SamusState
        {
            Pose = rightPose,
            XPosition = 128,
            YPosition = 96,
            EquippedBeams = unchecked((ushort)(0x1000 | beamType)),
        };
        var combinedChargeBombs = new SamusBombProjectileSystem();
        var combinedChargeProjectiles = new SamusProjectileSystem();
        for (int frame = 0; frame < 60; frame++)
        {
            combinedChargeBombs.StepFrame(bus, air, combinedChargeSamus, 0, 0);
            combinedChargeProjectiles.StepFrame(
                bus,
                air,
                combinedChargeSamus,
                (ushort)SnesButton.X,
                frame == 0 ? (ushort)SnesButton.X : (ushort)0,
                0,
                0,
                combinedChargeBombs);
        }

        combinedChargeBombs.StepFrame(bus, air, combinedChargeSamus, 0, 0);
        SamusProjectileFrameResult combinedChargedRelease = combinedChargeProjectiles.StepFrame(
            bus, air, combinedChargeSamus, 0, 0, 0, 0, combinedChargeBombs);
        AssertTrue(combinedChargedRelease.FiredSlot is not null,
            $"charged beam combination {beamType} allocates on release");
        SamusProjectileSlot combinedChargedSlot =
            combinedChargeProjectiles.Slots[combinedChargedRelease.FiredSlot!.Value];
        AssertEqual(unchecked((ushort)(0x0100 + beamType)), combinedChargedSlot.Damage,
            $"charged beam combination {beamType} indexes charged data pointer");
        AssertEqual(unchecked((ushort)(30 + beamType)), combinedChargeBombs.CooldownTimer,
            $"charged beam combination {beamType} indexes charged cooldown");
        AssertEqual(
            (SoundEffectId?)SoundEffectId.FromCartridge(
                SoundEffectLibrary.Library1,
                unchecked((ushort)(0x0050 + beamType))),
            combinedChargedRelease.QueuedSoundEffect,
            $"charged beam combination {beamType} indexes charged sound");
        AssertEqual(
            (beamType & 1) == 0
                ? SamusProjectilePreInstruction.NoWaveBeam
                : SamusProjectilePreInstruction.WaveBeamFourFrameTrail,
            combinedChargedSlot.PreInstruction,
            $"charged beam combination {beamType} selects native wave dispatch");
    }

    // Hyper Beam ignores the equipped combination for projectile identity and forces
    // charged-Plasma type `$9018`. Bank `$93` supplies its direction art/radii, after which
    // the producer overwrites damage with literal 1000 and arms the descending flare state.
    var hyperSamus = new SamusState
    {
        Pose = rightPose,
        XPosition = 128,
        YPosition = 96,
        EquippedBeams = 0x1009,
        HyperBeam = 0x8000,
    };
    var hyperBombs = new SamusBombProjectileSystem();
    var hyperProjectiles = new SamusProjectileSystem();
    hyperBombs.StepFrame(bus, air, hyperSamus, 0, 0);
    SamusProjectileFrameResult hyperResult = hyperProjectiles.StepFrame(
        bus,
        air,
        hyperSamus,
        (ushort)SnesButton.X,
        (ushort)SnesButton.X,
        0,
        0,
        hyperBombs);
    SamusProjectileSlot hyperSlot = hyperProjectiles.Slots[0];
    AssertEqual((int?)0, hyperResult.FiredSlot, "Hyper Beam allocates ordinary slot zero");
    AssertEqual(0x9018, hyperSlot.Type, "Hyper Beam forces literal type `$9018`");
    AssertEqual(1000, hyperSlot.Damage, "Hyper Beam overwrites ROM-table damage with 1000");
    AssertEqual(SamusProjectilePreInstruction.HyperBeam, hyperSlot.PreInstruction,
        "Hyper Beam selects trail-free Wave movement");
    AssertEqual(0, hyperSlot.TrailTimer, "Hyper Beam does not arm a projectile trail");
    AssertEqual(21, hyperBombs.CooldownTimer, "Hyper Beam installs literal cooldown 21");
    AssertEqual(
        (SoundEffectId?)SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 0x0058),
        hyperResult.QueuedSoundEffect,
        "Hyper Beam indexes charged sound entry eight");
    AssertEqual(0x8014, hyperProjectiles.ChargedShotGlowTimer,
        "Hyper Beam installs signed palette/glow phase `$8014`");
    AssertEqual(0x8000, hyperProjectiles.FlareCounter,
        "Hyper Beam arms descending flare sentinel");

    // `$8014` supplies ten descending even table offsets with an odd no-write hold after
    // each one. The twenty-first call sees `$8000`, skips the padding pointer, and restores
    // Power Suit. Verify every ROM word so reversed palette order cannot look plausible.
    var hyperGlowCgram = new SnesCgram();
    for (int call = 0; call < 21; call++)
    {
        ushort[] beforeColors = hyperGlowCgram.Colors.ToArray();
        SamusBeamChargePaletteStepResult paletteStep =
            hyperProjectiles.UpdateBeamChargePalette(bus, hyperGlowCgram, hyperSamus);
        if (call < 20 && (call & 1) == 0)
        {
            int palette = call / 2;
            AssertEqual(SamusBeamChargePaletteAction.HyperPalette, paletteStep.Action,
                $"Hyper body glow call {call + 1} loads palette");
            AssertEqual(palette, paletteStep.HyperPaletteIndex,
                $"Hyper body glow call {call + 1} reports descending table index");
            AssertEqual(hyperShotPalettePointers[palette], paletteStep.PalettePointer,
                $"Hyper body glow call {call + 1} reads exact pointer");
            ushort[] expectedPalette = Enumerable.Range(0, 16)
                .Select(color => unchecked((ushort)(0x2000 + palette * 0x20 + color)))
                .ToArray();
            AssertPaletteSlice(hyperGlowCgram, 192, expectedPalette,
                $"Hyper body palette {palette}");
        }
        else if (call < 20)
        {
            AssertEqual(SamusBeamChargePaletteAction.HyperHold, paletteStep.Action,
                $"Hyper body glow call {call + 1} is odd hold");
            AssertSequenceEqual(
                beforeColors,
                hyperGlowCgram.Colors.ToArray(),
                $"Hyper body glow hold {call + 1} leaves CGRAM unchanged");
        }
        else
        {
            AssertEqual(SamusBeamChargePaletteAction.RestoredNormalSuit, paletteStep.Action,
                "Hyper body glow call 21 restores suit");
            AssertEqual(normalSuitPalettePointers[0], paletteStep.PalettePointer,
                "Hyper body glow restore selects Power Suit pointer");
        }
    }
    AssertEqual(0, hyperProjectiles.ChargedShotGlowTimer,
        "Hyper body glow clears signed timer after 21 calls");

    // Its three components begin at frames 29/5/5 with timer three. Fast sparks (component
    // two) own completion, so exactly fifteen draw calls count five records down to zero.
    var hyperFlareOam = new OamBuffer();
    for (int call = 0; call < 15; call++)
    {
        hyperFlareOam.BeginFrame();
        hyperProjectiles.HandleChargeFlareAndDraw(bus, hyperFlareOam, hyperSamus, 0, 0);
        AssertTrue(hyperFlareOam.NextByteOffset != 0,
            $"Hyper Beam descending flare call {call + 1} draws ROM spritemaps");
    }
    AssertEqual(0, hyperProjectiles.FlareCounter,
        "Hyper Beam fast-spark frame one clears flare sentinel");

    // `$93:8268` exempts charged-family bit `$0010` from ordinary beam flicker. Slot zero
    // would be suppressed on even NMI under the uncharged rule, making this a direct guard
    // against accidentally applying that branch to the charged projectile.
    var chargedOam = new OamBuffer();
    chargedOam.BeginFrame();
    chargeProjectiles.DrawLiveProjectiles(bus, chargedOam, 0, 0, nmiFrameCounter: 0);
    AssertTrue(chargedOam.NextByteOffset != 0,
        "charged power beam bypasses ordinary alternating-frame flicker");

    // The release frame has already changed trail timer 4 to 3. Three more alpha passes
    // reach zero and allocate native trail byte index `$22` before the third pass moves the
    // projectile. Handling the draw immediately must consume only the populated left stream.
    chargeBombs.StepFrame(bus, air, chargeSamus, 0, 0);
    chargeProjectiles.StepFrame(bus, air, chargeSamus, 0, 0, 0, 0, chargeBombs);
    chargeBombs.StepFrame(bus, air, chargeSamus, 0, 0);
    chargeProjectiles.StepFrame(bus, air, chargeSamus, 0, 0, 0, 0, chargeBombs);
    ushort trailSourceX = chargedSlot.XPosition;
    ushort trailSourceY = chargedSlot.YPosition;
    chargeBombs.StepFrame(bus, air, chargeSamus, 0, 0);
    chargeProjectiles.StepFrame(bus, air, chargeSamus, 0, 0, 0, 0, chargeBombs);

    AssertEqual(1, chargeProjectiles.ActiveTrailCount,
        "charged power allocates one of eighteen trail slots every fourth alpha pass");
    SamusProjectileTrailSlot chargedTrail =
        chargeProjectiles.TrailSlots[SamusProjectileSystem.TrailSlotCount - 1];
    AssertTrue(chargedTrail.IsActive, "trail allocation scans downward from native index $22");
    AssertEqual(unchecked((ushort)(trailSourceX - 4)), chargedTrail.Left.XPosition,
        "trail samples projectile X before movement and subtracts four");
    AssertEqual(unchecked((ushort)(trailSourceY - 4)), chargedTrail.Left.YPosition,
        "trail samples projectile Y before movement and subtracts four");

    var trailOam = new OamBuffer();
    trailOam.BeginFrame();
    chargeProjectiles.HandleTrailsAndDraw(bus, trailOam, 0, 0, timeIsFrozen: false);
    AssertEqual(4, trailOam.NextByteOffset, "charged power's left stream emits one raw OBJ");
    AssertEqual(0, chargedTrail.Right.InstructionTimer,
        "charged power's empty right stream terminates without drawing");
    OamEntry firstTrailObj = trailOam.GetEntry(0);
    AssertEqual(0x038, firstTrailObj.TileNumber, "charged trail reads first `$2C38` tile");
    AssertEqual(6, firstTrailObj.Palette, "charged trail retains packed OBJ palette six");
    AssertTrue(!firstTrailObj.IsLarge, "projectile trail is an explicit small OBJ");

    // Time freeze bypasses DEC and command parsing but not OAM emission. The same record and
    // instruction pointer must remain visible and unchanged for an arbitrary frozen frame.
    ushort frozenTrailPointer = chargedTrail.Left.InstructionPointer;
    ushort frozenTrailTimer = chargedTrail.Left.InstructionTimer;
    trailOam.BeginFrame();
    chargeProjectiles.HandleTrailsAndDraw(bus, trailOam, 0, 0, timeIsFrozen: true);
    AssertEqual(frozenTrailPointer, chargedTrail.Left.InstructionPointer,
        "frozen trail retains instruction pointer");
    AssertEqual(frozenTrailTimer, chargedTrail.Left.InstructionTimer,
        "frozen trail retains instruction timer");
    AssertEqual(4, trailOam.NextByteOffset, "frozen active trail still draws");

    // Five more records reach the inline `$B525` opcode on the sixth call after the first
    // draw. It mutates world Y, then falls through to the following timed tile in one pass.
    ushort beforeTrailCommandY = chargedTrail.Left.YPosition;
    for (int call = 0; call < 6; call++)
    {
        trailOam.BeginFrame();
        chargeProjectiles.HandleTrailsAndDraw(bus, trailOam, 0, 0, timeIsFrozen: false);
    }
    AssertEqual(unchecked((ushort)(beforeTrailCommandY + 1)), chargedTrail.Left.YPosition,
        "inline `$B525` command moves the left trail down one pixel");
    AssertEqual(0x039, trailOam.GetEntry(0).TileNumber,
        "position command falls through to the following `$2C39` timed record");

    // Commands identify the destination side, not the stream executing them. Charged
    // Wave's right stream uses MoveLeftDown; rejecting that cross-side write crashes
    // the first fully integrated special attack even though particle motion is correct.
    foreach (bool executeOnLeft in new[] { false, true })
    foreach (ushort command in new[] { SamusProjectileRomData.Trails.MoveLeftDown,
        SamusProjectileRomData.Trails.MoveRightDown, SamusProjectileRomData.Trails.MoveLeftUp })
    {
        var commandProjectiles = new SamusProjectileSystem();
        var pair = commandProjectiles.TrailSlots[0];
        pair.Left.YPosition = 100;
        pair.Right.YPosition = 120;
        var executingSide = executeOnLeft ? pair.Left : pair.Right;
        executingSide.InstructionTimer = 1;
        executingSide.InstructionPointer = 0x8000;
        WriteTestWord(bus, 0x908000, command);
        WriteTestWord(bus, 0x908002, 1);
        WriteTestWord(bus, 0x908004, 0x2c38);
        trailOam.BeginFrame();
        commandProjectiles.HandleTrailsAndDraw(bus, trailOam, 0, 0, timeIsFrozen: false);
        AssertEqual(command == SamusProjectileRomData.Trails.MoveLeftDown ? 101 :
            command == SamusProjectileRomData.Trails.MoveLeftUp ? 99 : 100,
            pair.Left.YPosition, "trail command targets named left side regardless of executing stream");
        AssertEqual(command == SamusProjectileRomData.Trails.MoveRightDown ? 121 : 120,
            pair.Right.YPosition, "trail command targets named right side regardless of executing stream");
        AssertEqual(0x8006, executingSide.InstructionPointer,
            "cross-side command continues the executing stream through its timed record");
    }

    // Isolate horizontal fixed-point motion and collision against an authentic type-eight
    // solid column. The first rightward frame uses velocity `$0400+$0010`, producing four
    // whole pixels and subposition `$1000`; repeated alpha passes eventually install the
    // ROM explosion without freeing the slot early.
    var wallWords = new ushort[width * height];
    for (int y = 0; y < height; y++)
        wallWords[y * width + 6] = 0x8000;
    RoomLevelData wall = new(
        width,
        height,
        wallWords,
        new byte[wallWords.Length],
        new ushort[wallWords.Length],
        new byte[8]);

    // Wave collision routines still scan the projectile's full radius but return carry
    // clear after every block reaction. Drive power+wave through the same strict type-eight
    // wall that kills the no-wave fixture below; its center must emerge beyond the column
    // without ever entering the explosion family.
    var waveWallSamus = new SamusState
    {
        Pose = rightPose,
        XPosition = 64,
        YPosition = 96,
        EquippedBeams = 1,
    };
    var waveWallBombs = new SamusBombProjectileSystem();
    var waveWallProjectiles = new SamusProjectileSystem();
    bool waveReportedExplosion = false;
    for (int frame = 0; frame < 12; frame++)
    {
        waveWallBombs.StepFrame(bus, wall, waveWallSamus, 0, 0);
        SamusProjectileFrameResult waveFrame = waveWallProjectiles.StepFrame(
            bus,
            wall,
            waveWallSamus,
            frame == 0 ? (ushort)SnesButton.X : (ushort)0,
            frame == 0 ? (ushort)SnesButton.X : (ushort)0,
            0,
            0,
            waveWallBombs);
        waveReportedExplosion |= waveFrame.CollisionStartedExplosion;
    }
    AssertTrue(!waveReportedExplosion, "wave beam never converts on a type-eight wall");
    AssertTrue(waveWallProjectiles.Slots[0].XPosition > 112,
        "wave beam advances completely through the solid column");
    AssertEqual(SamusProjectilePreInstruction.WaveBeamThreeFrameTrail,
        waveWallProjectiles.Slots[0].PreInstruction,
        "power+wave remains in its native pass-through pre-instruction");

    WritePoseDefinition(
        bus,
        rightPose,
        [0x08, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00]);
    var wallSamus = new SamusState
    {
        Pose = rightPose,
        XPosition = 64,
        YPosition = 96,
    };
    var wallBombs = new SamusBombProjectileSystem();
    var wallProjectiles = new SamusProjectileSystem();
    wallBombs.StepFrame(bus, wall, wallSamus, 0, 0);
    wallProjectiles.StepFrame(
        bus,
        wall,
        wallSamus,
        (ushort)SnesButton.X,
        (ushort)SnesButton.X,
        0,
        0,
        wallBombs);
    AssertEqual(79, wallProjectiles.Slots[0].XPosition,
        "right beam starts eleven pixels beyond Samus then moves four whole pixels");
    AssertEqual(0x1000, wallProjectiles.Slots[0].XSubposition,
        "right beam first frame retains one-sixteenth pixel");

    SamusProjectileFrameResult wallResult = default;
    for (int frame = 0; frame < 16 && !wallResult.CollisionStartedExplosion; frame++)
    {
        wallBombs.StepFrame(bus, wall, wallSamus, 0, 0);
        wallResult = wallProjectiles.StepFrame(
            bus, wall, wallSamus, 0, 0, 0, 0, wallBombs);
    }
    AssertTrue(wallResult.CollisionStartedExplosion, "power beam reaches type-eight wall");
    AssertEqual(SamusProjectileFamily.BeamExplosion,
        wallProjectiles.Slots[0].PackedType.Family,
        "wall collision installs beam-explosion family");
    AssertEqual(1, wallProjectiles.ProjectileCounter,
        "beam explosion retains ordinary slot count");
    AssertEqual(0xa010, wallProjectiles.Slots[0].SpritemapPointer,
        "collision frame selects first explosion art");

    for (int frame = 0; frame < 2; frame++)
    {
        wallBombs.StepFrame(bus, wall, wallSamus, 0, 0);
        wallProjectiles.StepFrame(bus, wall, wallSamus, 0, 0, 0, 0, wallBombs);
    }
    AssertEqual(0, wallProjectiles.ProjectileCounter,
        "explosion delete decrements ordinary counter");
    AssertTrue(!wallProjectiles.Slots[0].IsActive,
        "explosion delete clears ordinary slot");

    // Replace the inert type-eight column with the two native shootable collision nibbles.
    // This is an end-to-end producer test: a fired projectile must reach bank-$94's radius
    // scanner, publish the correct bank-$84 PLM, run CE6B's synchronous terrain mutation,
    // and retain the collision nibble's carry result. Filling the entire column makes the
    // fixture independent of the pose-authored cannon Y offset while still requiring the
    // projectile to travel from Samus to block column six.
    var solidShotWords = new ushort[width * height];
    var solidShotBehaviors = new byte[solidShotWords.Length];
    for (int y = 0; y < height; y++)
        solidShotWords[y * width + 6] = 0xc000;
    RoomLevelData solidShotWall = new(
        width,
        height,
        solidShotWords,
        solidShotBehaviors,
        new ushort[solidShotWords.Length],
        new byte[8]);
    var solidShotSamus = new SamusState
    {
        Pose = rightPose,
        XPosition = 64,
        YPosition = 96,
    };
    var solidShotBombs = new SamusBombProjectileSystem();
    var solidShotProjectiles = new SamusProjectileSystem();
    var solidShotPlms = new RoomPlmSystem();
    SamusProjectileFrameResult solidShotResult = default;
    for (int frame = 0; frame < 16 && !solidShotResult.CollisionStartedExplosion; frame++)
    {
        solidShotBombs.StepFrame(bus, solidShotWall, solidShotSamus, 0, 0);
        solidShotResult = solidShotProjectiles.StepFrame(
            bus,
            solidShotWall,
            solidShotSamus,
            frame == 0 ? (ushort)SnesButton.X : (ushort)0,
            frame == 0 ? (ushort)SnesButton.X : (ushort)0,
            0,
            0,
            solidShotBombs,
            roomPlms: solidShotPlms);
    }
    AssertTrue(solidShotResult.CollisionStartedExplosion,
        "type-C shootable block retains solid shot collision");
    AssertTrue(solidShotPlms.ActiveCount > 0,
        "ordinary beam collision allocates bank-$84 shot-block PLM");
    int synthesizedSolidShotBlocks = 0;
    for (int y = 0; y < height; y++)
    {
        if (solidShotWall.GetCollisionBlock(6, y).LevelWord == 0x8052)
            synthesizedSolidShotBlocks++;
    }
    AssertTrue(synthesizedSolidShotBlocks > 0,
        "CE6B synchronously converts contacted type-C block to synthesized $8052");

    // Type four calls the very same setup but returns carry clear. Wave compounds that rule:
    // `$94:A352` must run every block side effect and then discard even a carry-set reaction.
    // Crossing the whole column without an explosion proves neither the new PLM publication
    // nor the temporary `$0052` word accidentally turned Wave into a clipping projectile.
    var waveShotWords = new ushort[width * height];
    var waveShotBehaviors = new byte[waveShotWords.Length];
    for (int y = 0; y < height; y++)
        waveShotWords[y * width + 6] = 0x4000;
    RoomLevelData waveShotWall = new(
        width,
        height,
        waveShotWords,
        waveShotBehaviors,
        new ushort[waveShotWords.Length],
        new byte[8]);
    var waveShotSamus = new SamusState
    {
        Pose = rightPose,
        XPosition = 64,
        YPosition = 96,
        EquippedBeams = 1,
    };
    var waveShotBombs = new SamusBombProjectileSystem();
    var waveShotProjectiles = new SamusProjectileSystem();
    var waveShotPlms = new RoomPlmSystem();
    bool waveShotReportedExplosion = false;
    for (int frame = 0; frame < 12; frame++)
    {
        waveShotBombs.StepFrame(bus, waveShotWall, waveShotSamus, 0, 0);
        SamusProjectileFrameResult waveShotFrame = waveShotProjectiles.StepFrame(
            bus,
            waveShotWall,
            waveShotSamus,
            frame == 0 ? (ushort)SnesButton.X : (ushort)0,
            frame == 0 ? (ushort)SnesButton.X : (ushort)0,
            0,
            0,
            waveShotBombs,
            roomPlms: waveShotPlms);
        waveShotReportedExplosion |= waveShotFrame.CollisionStartedExplosion;
    }
    AssertTrue(!waveShotReportedExplosion,
        "Wave remains alive after publishing type-four shot-block reaction");
    AssertTrue(waveShotProjectiles.Slots[0].XPosition > 112,
        "Wave crosses the complete shootable-air column");
    AssertTrue(waveShotPlms.ActiveCount > 0,
        "Wave scan allocates bank-$84 shot-block PLM");
    int synthesizedAirShotBlocks = 0;
    for (int y = 0; y < height; y++)
    {
        if (waveShotWall.GetCollisionBlock(6, y).LevelWord == 0x0052)
            synthesizedAirShotBlocks++;
    }
    AssertTrue(synthesizedAirShotBlocks > 0,
        "CE6B synchronously converts contacted type-four block to synthesized $0052");

    // `$90:BE62` shares the ordinary five-slot array with beams but selects a completely
    // different bank-$93 data family and bank-$90 pre-instruction. Fire a rightward missile
    // into the same type-eight column so producer state, first-frame ignition, persistent
    // trail, point collision, and missile-specific explosion are all observed in one route.
    var missileSamus = new SamusState
    {
        Pose = rightPose,
        XPosition = 64,
        YPosition = 96,
        SelectedHudItem = 1,
        Missiles = 3,
    };
    var missileBombs = new SamusBombProjectileSystem();
    var missileProjectiles = new SamusProjectileSystem();
    missileBombs.StepFrame(bus, wall, missileSamus, 0, 0);
    SamusProjectileFrameResult missileFired = missileProjectiles.StepFrame(
        bus,
        wall,
        missileSamus,
        (ushort)SnesButton.X,
        (ushort)SnesButton.X,
        0,
        0,
        missileBombs);
    SamusProjectileSlot missile = missileProjectiles.Slots[0];
    AssertEqual((int?)0, missileFired.FiredSlot, "missile fresh press allocates slot zero");
    AssertEqual(
        (SoundEffectId?)SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 3),
        missileFired.QueuedSoundEffect,
        "missile producer queues library-one effect three");
    AssertEqual(2, missileSamus.Missiles, "missile producer consumes exactly one ammo");
    AssertEqual(1, missileSamus.SelectedHudItem,
        "nonempty missile reserve remains HUD-selected");
    AssertEqual(1, missileProjectiles.ProjectileCounter,
        "missile increments the shared ordinary-projectile counter");
    AssertEqual(10, missileBombs.CooldownTimer,
        "missile producer installs literal ten-frame shared cooldown");
    AssertEqual(20, missileProjectiles.ProjectileInvincibilityTimer,
        "missile producer installs literal projectile invincibility timer twenty");
    AssertEqual(0x8100, missile.Type, "missile uses active type word `$8100`");
    AssertEqual(0x0064, missile.Damage, "missile reads damage from `$93:8641`");
    AssertEqual(SamusProjectilePreInstruction.Missile, missile.PreInstruction,
        "missile selects `$90:AF68` pre-instruction family");
    AssertEqual(0x0100, missile.Variable,
        "first alpha pass crosses `$0100` ignition threshold");
    AssertEqual(0x0100, missile.XVelocity,
        "right missile begins at one pixel per frame after ignition");
    AssertEqual(76, missile.XPosition,
        "right missile moves one whole pixel on its ignition frame");
    AssertEqual(0xa020, missile.SpritemapPointer,
        "missile instruction handler selects first bank-$93 art record");

    var missileOam = new OamBuffer();
    missileOam.BeginFrame();
    missileProjectiles.DrawLiveProjectiles(bus, missileOam, 0, 0, nmiFrameCounter: 0);
    AssertEqual(4, missileOam.NextByteOffset,
        "missile family bypasses ordinary beam alternating-frame flicker");
    AssertEqual(0x044, missileOam.GetEntry(0).TileNumber,
        "missile draw consumes its `$2A44` fixture OBJ");

    // The firing alpha pass changed trail timer four to three. Exactly three further alpha
    // passes allocate native trail entry `$20`; the subsequent draw parses `$90:B5A1` and
    // publishes its first four-frame `$2A48` record while the empty right stream terminates.
    for (int frame = 0; frame < 3; frame++)
    {
        missileBombs.StepFrame(bus, wall, missileSamus, 0, 0);
        missileProjectiles.StepFrame(bus, wall, missileSamus, 0, 0, 0, 0, missileBombs);
    }
    AssertEqual(1, missileProjectiles.ActiveTrailCount,
        "missile allocates one persistent trail every fourth alpha pass");
    SamusProjectileTrailSlot missileTrail =
        missileProjectiles.TrailSlots[SamusProjectileSystem.TrailSlotCount - 1];
    missileOam.BeginFrame();
    missileProjectiles.HandleTrailsAndDraw(bus, missileOam, 0, 0, timeIsFrozen: false);
    AssertEqual(4, missileOam.NextByteOffset, "missile left trail emits one small OBJ");
    AssertEqual(0x048, missileOam.GetEntry(0).TileNumber,
        "missile trail starts at retail tile `$2A48`");
    AssertEqual(4, missileTrail.Left.InstructionTimer,
        "missile trail retains its four-frame record duration");
    AssertEqual(0, missileTrail.Right.InstructionTimer,
        "missile's empty right trail stream terminates immediately");

    SamusProjectileFrameResult missileImpact = default;
    for (int frame = 0; frame < 32 && !missileImpact.CollisionStartedExplosion; frame++)
    {
        missileBombs.StepFrame(bus, wall, missileSamus, 0, 0);
        missileImpact = missileProjectiles.StepFrame(
            bus, wall, missileSamus, 0, 0, 0, 0, missileBombs);
    }
    AssertTrue(missileImpact.CollisionStartedExplosion,
        "accelerating missile reaches the type-eight wall");
    AssertEqual(SamusProjectileFamily.MissileExplosion, missile.PackedType.Family,
        "missile collision installs missile-explosion family `$0800`");
    AssertEqual(1, missileProjectiles.ProjectileCounter,
        "missile explosion retains its shared ordinary slot count");
    AssertEqual(0xa010, missile.SpritemapPointer,
        "missile collision frame selects first explosion art");
    missileOam.BeginFrame();
    missileProjectiles.DrawExplosions(bus, missileOam, 0, 0);
    AssertEqual(4, missileOam.NextByteOffset,
        "missile explosion participates in the early explosion draw pass");

    for (int frame = 0; frame < 2; frame++)
    {
        missileBombs.StepFrame(bus, wall, missileSamus, 0, 0);
        missileProjectiles.StepFrame(bus, wall, missileSamus, 0, 0, 0, 0, missileBombs);
    }
    AssertEqual(0, missileProjectiles.ProjectileCounter,
        "missile explosion delete decrements ordinary counter");
    AssertTrue(!missile.IsActive, "missile explosion delete clears its slot");

    // Reuse the exact producer instance after the missile has completed, cycle the HUD with
    // Select exactly as a player does, and fire again. With no later selectable item owned,
    // `$90:C4E7` wraps item one back to zero. This guards the playthrough report that missile
    // damage might survive deselection: a new beam must rebuild both packed family and damage
    // from `$93:83C1`, never inherit slot history from the missile which occupied slot zero.
    AssertTrue(missileSamus.HandleHudSelection(
            (ushort)SnesButton.Select,
            (ushort)SnesButton.Select),
        "Select cycles the live HUD selection from missiles to beams");
    AssertEqual(0, missileSamus.SelectedHudItem,
        "Select wrap chooses the ordinary beam producer");
    missileBombs.Reset();
    SamusProjectileFrameResult postMissileBeamResult = missileProjectiles.StepFrame(
        bus,
        wall,
        missileSamus,
        (ushort)SnesButton.X,
        (ushort)SnesButton.X,
        0,
        0,
        missileBombs);
    AssertTrue(postMissileBeamResult.FiredSlot.HasValue,
        "Shoot after missile deselection allocates a new beam");
    SamusProjectileSlot postMissileBeam =
        missileProjectiles.Slots[postMissileBeamResult.FiredSlot!.Value];
    AssertEqual(0x8000, postMissileBeam.Type,
        "post-missile shot rebuilds active packed type as uncharged Power Beam");
    AssertEqual(0x0014, postMissileBeam.Damage,
        "post-missile shot reloads Power Beam damage instead of retaining missile damage");

    // Point missiles use a deliberately different slope route from radius-spanning beams.
    // Put the muzzle directly inside one synthetic type-one block and compare points above
    // and inside the exact cartridge height. This locks `$94:A58F`'s shape-row indexing and
    // `height <= y` comparison independently of the Landing Site cartridge smoke test.

    RoomLevelData BuildPointSlopeRoom(byte behavior)
    {
        var words = new ushort[width * height];
        var behaviors = new byte[words.Length];
        int muzzleBlock = 6 * width + 4;
        words[muzzleBlock] = 0x1000;
        behaviors[muzzleBlock] = behavior;
        return CreateRoom(width, height, words, behaviors);
    }

    SamusProjectileFrameResult FirePointMissile(RoomLevelData terrain, ushort yPosition)
    {
        var pointSamus = new SamusState
        {
            Pose = rightPose,
            // Request muzzle (64,yPosition): default Right adds (11,1), then
            // standing-pose mechanics subtract six from Y, regardless of artwork.
            XPosition = 53,
            YPosition = unchecked((ushort)(yPosition + 5)),
            SelectedHudItem = 1,
            Missiles = 1,
        };
        var pointBombs = new SamusBombProjectileSystem();
        var pointProjectiles = new SamusProjectileSystem();
        pointBombs.StepFrame(bus, terrain, pointSamus, 0, 0);
        return pointProjectiles.StepFrame(
            bus,
            terrain,
            pointSamus,
            (ushort)SnesButton.X,
            (ushort)SnesButton.X,
            0,
            0,
            pointBombs);
    }

    RoomLevelData nonSquareSlope = BuildPointSlopeRoom(0x12);
    AssertTrue(!FirePointMissile(nonSquareSlope, 96).CollisionStartedExplosion,
        "non-square point above ROM height remains air");
    AssertTrue(FirePointMissile(nonSquareSlope, 111).CollisionStartedExplosion,
        "non-square point at ROM height collides");

    // Shape zero is the retail half-height square: top-left/top-right are air and both
    // bottom quadrants are solid. These two centers differ only in bit three of Y, proving
    // `$94:A66A`'s perpendicular quadrant XOR used during horizontal missile movement.
    RoomLevelData squareSlope = BuildPointSlopeRoom(0x00);
    AssertTrue(!FirePointMissile(squareSlope, 96).CollisionStartedExplosion,
        "square-slope top half remains air");
    AssertTrue(FirePointMissile(squareSlope, 104).CollisionStartedExplosion,
        "square-slope bottom half collides");

    // `$94:A1B5` gives bombable air and bombable solid distinct carry results even when
    // `$84:CEDA` immediately deletes the missile-created PLM. This is easy to miss because
    // neither terrain nor the PLM pool retains a visible mutation after an ordinary missile.
    RoomLevelData BuildPointBlockRoom(ushort levelWord, byte behavior = 0)
    {
        var words = new ushort[width * height];
        var behaviors = new byte[words.Length];
        int muzzleBlock = 6 * width + 4;
        words[muzzleBlock] = levelWord;
        behaviors[muzzleBlock] = behavior;
        return CreateRoom(width, height, words, behaviors);
    }

    AssertTrue(!FirePointMissile(BuildPointBlockRoom(0x7000), 96).CollisionStartedExplosion,
        "bombable-air point reaction spawns-and-deletes PLM but returns carry clear");
    AssertTrue(FirePointMissile(BuildPointBlockRoom(0xf000), 96).CollisionStartedExplosion,
        "bombable-solid point reaction spawns-and-deletes PLM and returns carry set");

    // Horizontal/vertical extensions return N set and redispatch the signed parent rather
    // than using the extension's own apparent carry. Resolve both forms into bombable solid.
    RoomLevelData horizontalExtension = BuildPointBlockRoom(0x5000, behavior: 1);
    horizontalExtension.SetForegroundEntry(6 * width + 5, 0xf000);
    AssertTrue(FirePointMissile(horizontalExtension, 96).CollisionStartedExplosion,
        "horizontal extension redispatches missile against signed parent");

    RoomLevelData verticalExtension = BuildPointBlockRoom(0xd000, behavior: 1);
    verticalExtension.SetForegroundEntry(7 * width + 4, 0xf000);
    AssertTrue(FirePointMissile(verticalExtension, 96).CollisionStartedExplosion,
        "vertical extension redispatches missile against row-relative parent");

    // Landing Site's first door is the more important inverse case: its power beam hits
    // one of three type-$D cells whose negative BTS points upward to a type-$C/BTS-$41
    // origin. Seed the exact retail right-facing blue-door program and draw lists, then
    // prove the live radius scanner creates the PLM and its timed bank-$84 program clears
    // the complete four-block cap without a test-owned terrain mutation.
    WriteTestWord(bus, 0x84c4ba, 0x8c19);
    bus.WriteByte(0x84c4bc, 0x07);
    WriteTestWord(bus, 0x84c4bd, 0x0006);
    WriteTestWord(bus, 0x84c4bf, 0xa9fb);
    WriteTestWord(bus, 0x84c4c1, 0x0006);
    WriteTestWord(bus, 0x84c4c3, 0xaa07);
    WriteTestWord(bus, 0x84c4c5, 0x0006);
    WriteTestWord(bus, 0x84c4c7, 0xaa13);
    WriteTestWord(bus, 0x84c4c9, 0x005e);
    WriteTestWord(bus, 0x84c4cb, 0xa683);
    WriteTestWord(bus, 0x84c4cd, 0x86bc);

    static void WriteDoorDrawList(TestAddressSpace targetBus, int address, ushort[] words)
    {
        foreach (ushort word in words)
        {
            WriteTestWord(targetBus, address, word);
            address += 2;
        }
    }
    WriteDoorDrawList(bus, 0x84a9fb, [0x8004, 0x840d, 0x842d, 0x8c2d, 0x8c0d, 0x0000]);
    WriteDoorDrawList(bus, 0x84aa07, [0x8004, 0x840e, 0x842e, 0x8c2e, 0x8c0e, 0x0000]);
    WriteDoorDrawList(bus, 0x84aa13, [0x8004, 0x840f, 0x042f, 0x0c2f, 0x8c0f, 0x0000]);
    WriteDoorDrawList(bus, 0x84a683, [0x8004, 0x0482, 0x04a2, 0x0ca2, 0x0c82, 0x0000]);

    // Room loading runs the shared `$84:C7B1` setup for every colored-door header before
    // gameplay projectiles can inspect the decompressed collision map. Exercise all three
    // colors and four orientations so no uncertain header is silently treated as a blue cap.
    ushort[] coloredDoorHeaders =
    [
        0xc85a, 0xc860, 0xc866, 0xc86c,
        0xc872, 0xc878, 0xc87e, 0xc884,
        0xc88a, 0xc890, 0xc896, 0xc89c,
    ];
    const ushort coloredDoorPopulation = 0x9000;
    var coloredDoorWords = new ushort[16 * 16];
    var coloredDoorBehaviors = new byte[coloredDoorWords.Length];
    for (int index = 0; index < coloredDoorHeaders.Length; index++)
    {
        int blockX = index + 1;
        int blockY = 3;
        int blockIndex = blockY * 16 + blockX;
        coloredDoorWords[blockIndex] = unchecked((ushort)(0x8000 | 0x0120 + index));
        coloredDoorBehaviors[blockIndex] = unchecked((byte)(0x40 + (index & 3)));

        int recordAddress = 0x8f0000 | unchecked((ushort)(
            coloredDoorPopulation + index * 6));
        WriteTestWord(bus, recordAddress, coloredDoorHeaders[index]);
        bus.WriteByte(recordAddress + 2, unchecked((byte)blockX));
        bus.WriteByte(recordAddress + 3, unchecked((byte)blockY));
        WriteTestWord(bus, recordAddress + 4, unchecked((ushort)index));
    }
    WriteTestWord(
        bus,
        0x8f0000 | unchecked((ushort)(
            coloredDoorPopulation + coloredDoorHeaders.Length * 6)),
        0);
    RoomLevelData coloredDoors = new(
        16,
        16,
        coloredDoorWords,
        coloredDoorBehaviors,
        new ushort[coloredDoorWords.Length],
        new byte[8]);
    var coloredDoorPlms = new RoomPlmSystem();
    AssertEqual(
        coloredDoorHeaders.Length,
        coloredDoorPlms.LoadRoomPopulation(
            bus,
            coloredDoors,
            coloredDoors.CreateBackgroundStreamer(),
            new SnesVram(),
            coloredDoorPopulation,
            new Bank80SystemState(),
            0,
            () => new SamusState(),
            () => false),
        "colored-door setup scans all yellow, green, and red orientations");
    for (int index = 0; index < coloredDoorHeaders.Length; index++)
    {
        RoomCollisionBlock block = coloredDoors.GetCollisionBlock(index + 1, 3);
        AssertEqual(RoomCollisionType.ShootableBlock, block.CollisionType,
            $"colored-door setup installs shootable collision for header {index}");
        AssertEqual(0x44, block.Behavior,
            $"colored-door setup installs shared BTS for header {index}");
        AssertEqual(0x0120 + index, block.LevelWord & 0x0fff,
            $"colored-door setup preserves tile payload for header {index}");
    }

    // Exercise the resident actor rather than setup alone. These pointers reproduce the
    // header/list graph consumed by the translation: header +2 selects the initial list,
    // that list links the closed-blue, hit, and colored-closed draw records, and the hit
    // list embeds its threshold byte plus opening-list pointer at the native odd offsets.
    const ushort residentDoorPopulation = 0x9100;
    const ushort residentDoorArgument = 37;
    const int residentDoorBlock = 3 * 16 + 5;
    WriteTestWord(bus, 0x84c88c, 0xe000);
    WriteTestWord(bus, 0x84e002, 0xe100);
    WriteTestWord(bus, 0x84e006, 0xe200);
    WriteTestWord(bus, 0x84e00e, 0xe300);
    bus.WriteByte(0x84e202, 5);
    WriteTestWord(bus, 0x84e203, 0xe400);
    WriteTestWord(bus, 0x84e205, 1);
    WriteTestWord(bus, 0x84e207, 0xe320);
    WriteTestWord(bus, 0x84e400, 12);
    WriteTestWord(bus, 0x84e402, 0xe340);
    WriteTestWord(bus, 0x84e105, 0xe360);
    WriteDoorDrawList(bus, 0x84e300, [0x0001, 0xc123, 0x0000]);
    WriteDoorDrawList(bus, 0x84e320, [0x0001, 0xc124, 0x0000]);
    WriteDoorDrawList(bus, 0x84e340, [0x0001, 0xc125, 0x0000]);
    WriteDoorDrawList(bus, 0x84e360, [0x0001, 0x8126, 0x0000]);
    WriteTestWord(bus, 0x8f0000 | residentDoorPopulation, 0xc88a);
    bus.WriteByte(0x8f0000 | (residentDoorPopulation + 2), 5);
    bus.WriteByte(0x8f0000 | (residentDoorPopulation + 3), 3);
    WriteTestWord(bus, 0x8f0000 | (residentDoorPopulation + 4), residentDoorArgument);
    WriteTestWord(bus, 0x8f0000 | (residentDoorPopulation + 6), 0);

    var residentWords = new ushort[16 * 16];
    residentWords[residentDoorBlock] = 0x8120;
    RoomLevelData residentDoorLevel = new(
        16,
        16,
        residentWords,
        new byte[residentWords.Length],
        new ushort[residentWords.Length],
        new byte[8]);
    var residentDoorSystem = new Bank80SystemState();
    var residentDoorPlms = new RoomPlmSystem();
    AssertEqual(1, residentDoorPlms.LoadRoomPopulation(
            bus,
            residentDoorLevel,
            residentDoorLevel.CreateBackgroundStreamer(),
            new SnesVram(),
            residentDoorPopulation,
            residentDoorSystem,
            0,
            () => new SamusState(),
            () => false),
        "resident colored-door loader allocates red door actor");
    BackgroundTilemapStreamer residentDoorStreamer =
        residentDoorLevel.CreateBackgroundStreamer();
    residentDoorPlms.Step(
        bus, residentDoorLevel, residentDoorStreamer, 0x1000, 0x1000, 0);

    AssertTrue(residentDoorPlms.TryNotifyColoredDoorHit(residentDoorBlock, 0x0000),
        "resident red door receives power-beam family for native rejection");
    residentDoorPlms.Step(
        bus, residentDoorLevel, residentDoorStreamer, 0x1000, 0x1000, 0);
    AssertTrue(residentDoorPlms.SoundRequests.Contains(new PlmSoundRequest(SoundEffectLibrary2Sounds.DoorOpening, 6)),
        "wrong colored-door weapon queues native dud sound");
    AssertEqual(0, residentDoorPlms.ColoredDoors.Single().HitCounter,
        "wrong colored-door weapon does not advance threshold counter");

    for (int hit = 1; hit <= 5; hit++)
    {
        AssertTrue(residentDoorPlms.TryNotifyColoredDoorHit(residentDoorBlock, 0x0100),
            $"red door accepts missile collision {hit}");
        residentDoorPlms.Step(
            bus, residentDoorLevel, residentDoorStreamer, 0x1000, 0x1000, 0);
    }
    ColoredDoorPlmSnapshot openingDoor = residentDoorPlms.ColoredDoors.Single();
    AssertEqual(ColoredDoorPhase.Opening, openingDoor.Phase,
        "fifth missile selects cartridge opening list");
    AssertEqual(5, openingDoor.HitCounter,
        "red door retains exact five-missile threshold");
    AssertTrue(residentDoorSystem.HasOpenedDoorBit(residentDoorArgument),
        "opening instruction persists room-argument door bit");
    AssertTrue(!residentDoorPlms.TryNotifyColoredDoorHit(residentDoorBlock, 0x0100),
        "opening door no longer runs projectile pre-instruction");

    residentDoorPlms.Reset();
    AssertEqual(0, residentDoorPlms.ColoredDoors.Count,
        "room reset discards resident colored-door actors");
    AssertTrue(!residentDoorPlms.TryNotifyColoredDoorHit(residentDoorBlock, 0x0100),
        "room reset cannot leak a hit into the discarded population");

    var reopenedWords = new ushort[16 * 16];
    reopenedWords[residentDoorBlock] = 0x8120;
    RoomLevelData reopenedDoorLevel = new(
        16,
        16,
        reopenedWords,
        new byte[reopenedWords.Length],
        new ushort[reopenedWords.Length],
        new byte[8]);
    var reopenedDoorPlms = new RoomPlmSystem();
    reopenedDoorPlms.LoadRoomPopulation(
        bus,
        reopenedDoorLevel,
        reopenedDoorLevel.CreateBackgroundStreamer(),
        new SnesVram(),
        residentDoorPopulation,
        residentDoorSystem,
        0,
        () => new SamusState(),
        () => false);
    reopenedDoorPlms.Step(
        bus,
        reopenedDoorLevel,
        reopenedDoorLevel.CreateBackgroundStreamer(),
        0x1000,
        0x1000,
        0);
    AssertEqual(0x40, reopenedDoorLevel.GetCollisionBlockByIndex(residentDoorBlock).Behavior,
        "persisted left-facing red door reloads as ordinary left blue cap");
    AssertEqual(0, reopenedDoorPlms.ColoredDoors.Count,
        "persisted colored-door conversion deletes resident actor after draw");

    // Reproduce the generic grey-door graph with relocatable fixture pointers. This is
    // intentionally not a direct call to an event setter: the regression starts at a
    // bank-$8F population record, follows the header/list operands, publishes hits through
    // BTS $44's shared collision seam, and lets the ordinary PLM interpreter execute the
    // flashing/opening streams. It therefore guards the real cartridge ownership boundary
    // that wakes Zebes after Old Mother Brain room's enemy quota is satisfied.
    const ushort greyDoorPopulation = 0x9180;
    const ushort greyDoorInitialList = 0xe500;
    const ushort greyDoorClosedBlueList = 0xe600;
    const ushort greyDoorActivationList = 0xe700;
    const ushort greyDoorFlashList = greyDoorActivationList + 8;
    const ushort greyDoorOpenTriggerList = 0xe900;
    const ushort greyDoorOpeningList = 0xea00;
    const ushort greyDoorRawArgument = 0x0c2a;
    const ushort greyDoorPersistedArgument = 0x002a;
    const int greyDoorBlock = 6 * 16 + 2;

    // Header $C842 selects the synthetic initial list. The graph offsets below mirror
    // $BE70: opened-door target at +2, activation link at +6, and closed draw at +12.
    WriteTestWord(bus, 0x84c844, greyDoorInitialList);
    WriteTestWord(bus, 0x840000 | (greyDoorInitialList + 2), greyDoorClosedBlueList);
    WriteTestWord(bus, 0x840000 | (greyDoorInitialList + 6), greyDoorActivationList);
    WriteTestWord(bus, 0x840000 | (greyDoorInitialList + 12), 0xeb00);

    // The activation list links a later shot to the one-hit trigger. Its eight-byte setup
    // falls through to a compact flash loop composed only of a timed draw and shared Goto.
    WriteTestWord(bus, 0x840000 | (greyDoorActivationList + 2), greyDoorOpenTriggerList);
    WriteTestWord(bus, 0x840000 | greyDoorFlashList, 1);
    WriteTestWord(bus, 0x840000 | (greyDoorFlashList + 2), 0xeb20);
    WriteTestWord(bus, 0x840000 | (greyDoorFlashList + 4), 0x8724);
    WriteTestWord(bus, 0x840000 | (greyDoorFlashList + 6), greyDoorFlashList);

    // Instruction $8A91 has a one-byte threshold followed by the successful target at
    // offset three. The target uses the real shared sound-seven and Delete opcodes so the
    // semantic door pre-pass and generic instruction interpreter are tested together.
    bus.WriteByte(0x840000 | (greyDoorOpenTriggerList + 2), 1);
    WriteTestWord(bus, 0x840000 | (greyDoorOpenTriggerList + 3), greyDoorOpeningList);
    WriteTestWord(bus, 0x840000 | greyDoorOpeningList, 0x8c19);
    bus.WriteByte(0x840000 | (greyDoorOpeningList + 2), 7);
    WriteTestWord(bus, 0x840000 | (greyDoorOpeningList + 3), 1);
    WriteTestWord(bus, 0x840000 | (greyDoorOpeningList + 5), 0xeb40);
    WriteTestWord(bus, 0x840000 | (greyDoorOpeningList + 7), 0x86bc);

    // Persisted grey doors execute the same closed-blue list layout already exercised by
    // colored doors: PLM_BTS_Y precedes a one-byte timer and draw pointer at offset five.
    WriteTestWord(bus, 0x840000 | (greyDoorClosedBlueList + 5), 0xeb60);
    WriteDoorDrawList(bus, 0x84eb00, [0x0001, 0xc220, 0x0000]);
    WriteDoorDrawList(bus, 0x84eb20, [0x0001, 0xc221, 0x0000]);
    WriteDoorDrawList(bus, 0x84eb40, [0x0001, 0x0222, 0x0000]);
    WriteDoorDrawList(bus, 0x84eb60, [0x0001, 0x8223, 0x0000]);
    WriteTestWord(bus, 0x8f0000 | greyDoorPopulation, 0xc842);
    bus.WriteByte(0x8f0000 | (greyDoorPopulation + 2), 2);
    bus.WriteByte(0x8f0000 | (greyDoorPopulation + 3), 6);
    WriteTestWord(bus, 0x8f0000 | (greyDoorPopulation + 4), greyDoorRawArgument);
    WriteTestWord(bus, 0x8f0000 | (greyDoorPopulation + 6), 0);

    var greyDoorWords = new ushort[16 * 16];
    greyDoorWords[greyDoorBlock] = 0x8220;
    RoomLevelData greyDoorLevel = new(
        16,
        16,
        greyDoorWords,
        new byte[greyDoorWords.Length],
        new ushort[greyDoorWords.Length],
        new byte[8]);
    var greyDoorSystem = new Bank80SystemState();
    var greyDoorPlms = new RoomPlmSystem();
    AssertEqual(1, greyDoorPlms.LoadRoomPopulation(
            bus,
            greyDoorLevel,
            greyDoorLevel.CreateBackgroundStreamer(),
            new SnesVram(),
            greyDoorPopulation,
            greyDoorSystem,
            areaIndex: AreaId.Crateria,
            getSamus: () => new SamusState(),
            isAreaTorizoDefeated: () => false),
        "grey-door loader allocates enemy-quota actor from population");
    AssertEqual(RoomCollisionType.ShootableBlock,
        greyDoorLevel.GetCollisionBlockByIndex(greyDoorBlock).CollisionType,
        "grey-door setup installs shootable-solid collision");
    AssertEqual(0x44, greyDoorLevel.GetCollisionBlockByIndex(greyDoorBlock).Behavior,
        "grey-door setup installs generic resident-PLM BTS");
    GreyDoorPlmSnapshot lockedGreyDoor = greyDoorPlms.GreyDoors.Single();
    AssertEqual(GreyDoorCondition.EnemyDeathQuota, lockedGreyDoor.Condition,
        "room argument high bits select cartridge enemy-quota condition");
    AssertEqual(greyDoorPersistedArgument, lockedGreyDoor.RoomArgument,
        "grey-door setup removes condition selector before persistence checks");

    BackgroundTilemapStreamer greyDoorStreamer = greyDoorLevel.CreateBackgroundStreamer();
    greyDoorPlms.Step(
        bus, greyDoorLevel, greyDoorStreamer, 0x1000, 0x1000, 0,
        enemyDeaths: 1, enemyDeathQuota: 2);
    AssertTrue(greyDoorPlms.TryNotifyColoredDoorHit(greyDoorBlock, 0x0000),
        "shared BTS $44 collision publishes power-beam hit to grey door");
    greyDoorPlms.Step(
        bus, greyDoorLevel, greyDoorStreamer, 0x1000, 0x1000, 0,
        enemyDeaths: 1, enemyDeathQuota: 2);
    AssertTrue(greyDoorPlms.SoundRequests.Contains(new PlmSoundRequest(SoundEffectLibrary2Sounds.DoorOpening, 6)),
        "locked enemy-quota grey door consumes shot with native dud sound");
    AssertTrue(!greyDoorSystem.HasEvent(EventNumber.ZebesAwake),
        "below-quota grey door does not publish Zebes-awake event");

    // Publish a second hit on the exact quota-completion frame. BE01 clears the PLM shot
    // timer while selecting its activation link, so this impact must not persist/open it.
    AssertTrue(greyDoorPlms.TryNotifyColoredDoorHit(greyDoorBlock, 0x0000),
        "quota-completion frame can observe a colliding shot");
    greyDoorPlms.Step(
        bus, greyDoorLevel, greyDoorStreamer, 0x1000, 0x1000, 0,
        enemyDeaths: 2, enemyDeathQuota: 2);
    AssertTrue(greyDoorSystem.HasEvent(EventNumber.ZebesAwake),
        "enemy-quota grey door is native producer of Zebes-awake event");
    AssertEqual(GreyDoorPhase.Flashing, greyDoorPlms.GreyDoors.Single().Phase,
        "quota completion enters cartridge flash loop");
    AssertTrue(!greyDoorSystem.HasOpenedDoorBit(greyDoorPersistedArgument),
        "quota-completion hit is cleared rather than opening grey door");

    AssertTrue(greyDoorPlms.TryNotifyColoredDoorHit(greyDoorBlock, 0x0000),
        "later shot reaches flashing grey door's installed shot pre-instruction");
    greyDoorPlms.Step(
        bus, greyDoorLevel, greyDoorStreamer, 0x1000, 0x1000, 0,
        enemyDeaths: 2, enemyDeathQuota: 2);
    AssertEqual(GreyDoorPhase.Opening, greyDoorPlms.GreyDoors.Single().Phase,
        "post-unlock shot selects cartridge opening list");
    AssertTrue(greyDoorSystem.HasOpenedDoorBit(greyDoorPersistedArgument),
        "grey-door one-hit instruction persists sanitized room argument");
    AssertTrue(greyDoorPlms.SoundRequests.Contains(new PlmSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 0x07), 6)),
        "grey-door opening stream queues native library-three sound seven");

    var reloadedGreyWords = new ushort[16 * 16];
    reloadedGreyWords[greyDoorBlock] = 0x8220;
    RoomLevelData reloadedGreyLevel = new(
        16,
        16,
        reloadedGreyWords,
        new byte[reloadedGreyWords.Length],
        new ushort[reloadedGreyWords.Length],
        new byte[8]);
    var reloadedGreyPlms = new RoomPlmSystem();
    reloadedGreyPlms.LoadRoomPopulation(
        bus,
        reloadedGreyLevel,
        reloadedGreyLevel.CreateBackgroundStreamer(),
        new SnesVram(),
        greyDoorPopulation,
        greyDoorSystem,
        areaIndex: AreaId.Crateria,
        getSamus: () => new SamusState(),
        isAreaTorizoDefeated: () => false);
    reloadedGreyPlms.Step(
        bus,
        reloadedGreyLevel,
        reloadedGreyLevel.CreateBackgroundStreamer(),
        0x1000,
        0x1000,
        0);
    AssertEqual(0x40, reloadedGreyLevel.GetCollisionBlockByIndex(greyDoorBlock).Behavior,
        "persisted left grey door reloads as ordinary left blue cap");
    AssertEqual(0, reloadedGreyPlms.GreyDoors.Count,
        "persisted grey-door conversion releases resident actor");

    var blueDoorWords = new ushort[width * height];
    var blueDoorBehaviors = new byte[blueDoorWords.Length];
    int blueDoorOrigin = 4 * width + 6;
    blueDoorWords[blueDoorOrigin] = 0xc000;
    blueDoorBehaviors[blueDoorOrigin] = 0x41;
    for (int rowOffset = 1; rowOffset < 4; rowOffset++)
    {
        blueDoorWords[blueDoorOrigin + rowOffset * width] = 0xd000;
        blueDoorBehaviors[blueDoorOrigin + rowOffset * width] =
            unchecked((byte)-rowOffset);
    }
    RoomLevelData blueDoor = new(
        width,
        height,
        blueDoorWords,
        blueDoorBehaviors,
        new ushort[blueDoorWords.Length],
        new byte[8]);

    // Reproduce the in-game contact case rather than merely waiting for a distant beam to
    // reach the cap. Right-facing power fire uses ROM muzzle offset +11: Samus center $5A
    // therefore creates the beam at $65, already inside the $60-$6F door block. Native
    // FireUnchargedBeam runs a zero-speed collision probe before its first four-pixel move;
    // omitting that probe advances the leading edge to block $70 and skips the cap entirely.
    var contactDoorWords = blueDoorWords.ToArray();
    var contactDoorBehaviors = blueDoorBehaviors.ToArray();
    RoomLevelData contactDoor = new(
        width,
        height,
        contactDoorWords,
        contactDoorBehaviors,
        new ushort[contactDoorWords.Length],
        new byte[8]);
    var contactDoorSamus = new SamusState
    {
        Pose = rightPose,
        XPosition = 0x005a,
        YPosition = 0x0060,
    };
    var contactDoorBombs = new SamusBombProjectileSystem();
    var contactDoorProjectiles = new SamusProjectileSystem();
    var contactDoorPlms = new RoomPlmSystem();
    contactDoorBombs.StepFrame(bus, contactDoor, contactDoorSamus, 0, 0);
    SamusProjectileFrameResult contactDoorImpact = contactDoorProjectiles.StepFrame(
        bus,
        contactDoor,
        contactDoorSamus,
        (ushort)SnesButton.X,
        (ushort)SnesButton.X,
        0,
        0,
        contactDoorBombs,
        roomPlms: contactDoorPlms);
    AssertTrue(contactDoorImpact.CollisionStartedExplosion,
        "beam born inside a blue cap collides before first-frame movement");
    AssertEqual(1, contactDoorPlms.ActiveCount,
        "contact-distance shot allocates the blue-door opening PLM");
    AssertEqual(RoomCollisionType.SolidBlock,
        contactDoor.GetCollisionBlockByIndex(blueDoorOrigin).CollisionType,
        "contact-distance shot synchronously opens the cap origin");
    AssertEqual(0x006d, contactDoorProjectiles.Slots[0].XPosition,
        "fire-time impact anchors its explosion at the native leading edge");

    var blueDoorSamus = new SamusState
    {
        Pose = rightPose,
        XPosition = 64,
        YPosition = 96,
    };
    var blueDoorBombs = new SamusBombProjectileSystem();
    var blueDoorProjectiles = new SamusProjectileSystem();
    var blueDoorPlms = new RoomPlmSystem();
    SamusProjectileFrameResult blueDoorImpact = default;
    for (int frame = 0; frame < 16 && !blueDoorImpact.CollisionStartedExplosion; frame++)
    {
        blueDoorBombs.StepFrame(bus, blueDoor, blueDoorSamus, 0, 0);
        blueDoorImpact = blueDoorProjectiles.StepFrame(
            bus,
            blueDoor,
            blueDoorSamus,
            frame == 0 ? (ushort)SnesButton.X : (ushort)0,
            frame == 0 ? (ushort)SnesButton.X : (ushort)0,
            0,
            0,
            blueDoorBombs,
            roomPlms: blueDoorPlms);
    }
    AssertTrue(blueDoorImpact.CollisionStartedExplosion,
        "power beam collides through negative vertical door extension");
    AssertEqual(1, blueDoorPlms.ActiveCount,
        "BTS $41 collision allocates one right-facing blue-door PLM");
    AssertEqual(RoomCollisionType.SolidBlock,
        blueDoor.GetCollisionBlockByIndex(blueDoorOrigin).CollisionType,
        "Setup_BlueDoor synchronously changes cap origin to type eight");

    BackgroundTilemapStreamer blueDoorStreamer = blueDoor.CreateBackgroundStreamer();
    blueDoorPlms.Step(bus, blueDoor, blueDoorStreamer, 0x1000, 0x1000, 0);
    AssertTrue(blueDoorPlms.SoundRequests.Any(request =>
            request == new PlmSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 0x07), 6)),
        "blue-door list queues library-three opening sound seven");
    for (int frame = 1; frame < 19; frame++)
        blueDoorPlms.Step(bus, blueDoor, blueDoorStreamer, 0x1000, 0x1000, 0);
    for (int rowOffset = 0; rowOffset < 4; rowOffset++)
    {
        AssertEqual(RoomCollisionType.Air,
            blueDoor.GetCollisionBlockByIndex(
                blueDoorOrigin + rowOffset * width).CollisionType,
            $"blue-door final draw clears cap row {rowOffset}");
    }

    // Super Missiles share `$BE62` but differ in every animation-adjacent constant: HUD item
    // two, type `$8200`, sound four, cooldown twenty, `$012C` damage, acceleration `$0100`,
    // two-frame exhaust after the initial delay, an invisible linked slot, and a larger quake-
    // producing explosion. Keep those distinctions together so a speed-only implementation
    // cannot satisfy the regression.
    var superSamus = new SamusState
    {
        Pose = rightPose,
        XPosition = 64,
        YPosition = 96,
        SelectedHudItem = 2,
        SuperMissiles = 3,
    };
    var superBombs = new SamusBombProjectileSystem();
    var superProjectiles = new SamusProjectileSystem();
    superBombs.StepFrame(bus, wall, superSamus, 0, 0);
    SamusProjectileFrameResult superFired = superProjectiles.StepFrame(
        bus,
        wall,
        superSamus,
        (ushort)SnesButton.X,
        (ushort)SnesButton.X,
        0,
        0,
        superBombs);
    SamusProjectileSlot super = superProjectiles.Slots[0];
    SamusProjectileSlot superLink = superProjectiles.Slots[1];
    AssertEqual((int?)0, superFired.FiredSlot, "super fresh press allocates owner slot zero");
    AssertEqual(
        (SoundEffectId?)SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 4),
        superFired.QueuedSoundEffect,
        "super producer queues library-one effect four");
    AssertEqual(2, superSamus.SuperMissiles,
        "super producer consumes exactly one ammo");
    AssertEqual(20, superBombs.CooldownTimer,
        "super producer installs literal twenty-frame cooldown");
    AssertEqual(2, superProjectiles.ProjectileCounter,
        "super ignition counts visible owner plus invisible link");
    AssertEqual(0x8200, super.Type, "super owner uses active type `$8200`");
    AssertEqual(0x012c, super.Damage, "super owner reads retail 300 damage");
    AssertEqual(0x0100, super.XVelocity,
        "super ignition begins at one pixel per frame");
    AssertEqual(0x0102, super.Variable,
        "super variable combines initialized high byte and link byte index two");
    AssertEqual(SamusProjectilePreInstruction.SuperMissile, super.PreInstruction,
        "super owner selects `$90:AFE5`");
    AssertEqual(0x8200, superLink.Type, "super link retains family `$0200`");
    AssertEqual(0x012c, superLink.Damage, "super link carries native damage sentinel");
    AssertEqual(SamusProjectilePreInstruction.SuperMissileLink, superLink.PreInstruction,
        "super link selects stationary `$90:B075`");
    AssertEqual(super.XPosition, superLink.XPosition,
        "slow horizontal link follows owner center after ignition movement");

    // The link instruction list is intentionally invisible; only the owner contributes an
    // OBJ after its own bank-$93 program selects `$A030`.
    var superOam = new OamBuffer();
    superOam.BeginFrame();
    superProjectiles.DrawLiveProjectiles(bus, superOam, 0, 0, nmiFrameCounter: 0);
    AssertEqual(4, superOam.NextByteOffset, "super owner draws while link spritemap is empty");
    AssertEqual(0x045, superOam.GetEntry(0).TileNumber,
        "super owner consumes its distinct `$2A45` OBJ");

    for (int frame = 0; frame < 3; frame++)
    {
        superBombs.StepFrame(bus, wall, superSamus, 0, 0);
        superProjectiles.StepFrame(bus, wall, superSamus, 0, 0, 0, 0, superBombs);
    }
    AssertEqual(1, superProjectiles.ActiveTrailCount,
        "super's initial four-count allocates first exhaust trail");
    AssertEqual(2, super.TrailTimer,
        "super exhaust reloads two instead of missile four");

    SamusProjectileFrameResult superImpact = default;
    for (int frame = 0; frame < 24 && !superImpact.CollisionStartedExplosion; frame++)
    {
        superBombs.StepFrame(bus, wall, superSamus, 0, 0);
        superImpact = superProjectiles.StepFrame(
            bus, wall, superSamus, 0, 0, 0, 0, superBombs);
    }
    AssertTrue(superImpact.CollisionStartedExplosion,
        "accelerating super reaches the type-eight wall");
    AssertEqual(0x8800, super.Type,
        "super impact preserves active bit and selects family `$0800`");
    AssertEqual(0x9348, super.InstructionPointer,
        "super collision consumes first record of `$93:9340` explosion fixture");
    AssertEqual(20, superProjectiles.EarthquakeType,
        "super impact publishes quake type `$14`");
    AssertEqual(30, superProjectiles.EarthquakeTimer,
        "super impact publishes thirty-frame quake timer");
    AssertTrue(!superLink.IsActive, "super owner impact clears invisible linked slot");
    AssertEqual(1, superProjectiles.ProjectileCounter,
        "super explosion retains only its visible owner count");

    // Issue #283: reproduce the narrow-pillar helper impact from the player's recording.
    // Original-ROM execution in native/ProjectileLinkAudit proves that the exploded helper
    // continues following over air. Preserve that quirk, but require the cartridge's exact
    // impact position and deletion on a second collision (not an explosion restart).
    var pillarWords = new ushort[64 * 16];
    pillarWords[851] = 0x8119;
    var pillarRoom = CreateRoom(64, 16, pillarWords, new byte[pillarWords.Length]);
    var pillarSamus = new SamusState
    {
        Pose = rightPose, XPosition = 64, YPosition = 96,
        SelectedHudItem = 2, SuperMissiles = 3,
    };
    var pillarBombs = new SamusBombProjectileSystem();
    var pillarProjectiles = new SamusProjectileSystem();
    pillarProjectiles.StepFrame(bus, pillarRoom, pillarSamus,
        (ushort)SnesButton.X, (ushort)SnesButton.X, 0, 0, pillarBombs);
    var pillarOwner = pillarProjectiles.Slots[0];
    var pillarLink = pillarProjectiles.Slots[1];
    pillarOwner.XPosition = 0x12f;
    pillarOwner.YPosition = pillarLink.YPosition = 0xd1;
    pillarOwner.XVelocity = 0x1100;
    pillarOwner.XSubposition = 0;
    pillarLink.XPosition = 0x128;
    pillarLink.XRadius = 8;
    // Keep the constructed animation alive long enough to observe the second collision.
    ushort oldExplosionDuration = (ushort)(bus.ReadByte(0x939340) |
        bus.ReadByte(0x939341) << 8);
    WriteTestWord(bus, 0x939340, 20);
    pillarProjectiles.StepFrame(bus, pillarRoom, pillarSamus, 0, 0, 0x100, 0, pillarBombs);
    AssertEqual(0x139, pillarLink.XPosition,
        "native missile helper impact does not apply the beam leading-edge radius");
    AssertEqual(SamusProjectileFamily.MissileExplosion, pillarLink.PackedType.Family,
        "supplemental pillar collision explodes the helper, not its owner");
    pillarProjectiles.StepFrame(bus, pillarRoom, pillarSamus, 0, 0, 0x100, 0, pillarBombs);
    AssertEqual(0x14b, pillarLink.XPosition,
        "original ROM repositions an exploded helper over air");
    pillarWords[853] = 0x8119;
    var secondPillarRoom = CreateRoom(64, 16, pillarWords, new byte[pillarWords.Length]);
    pillarProjectiles.StepFrame(bus, secondPillarRoom, pillarSamus, 0, 0, 0x100, 0, pillarBombs);
    AssertTrue(!pillarLink.IsActive,
        "native second collision deletes the explosion instead of restarting it");
    AssertEqual(1, pillarProjectiles.ProjectileCounter,
        "second helper collision decrements its counted slot exactly once");
    WriteTestWord(bus, 0x939340, oldExplosionDuration);

    // Drive a second real Super Missile into type-$C/BTS-A rather than calling the PLM
    // owner directly. `$90:B00E`'s invisible linked point probe and the visible owner both
    // carry family `$0200`; whichever reaches the column first must publish `$84:D08C`, run
    // CF67 synchronously, and leave `$809F` behind for the normal same-frame PLM pass.
    var integratedSuperWords = new ushort[width * height];
    var integratedSuperBehaviors = new byte[integratedSuperWords.Length];
    for (int y = 0; y < height; y++)
    {
        int index = y * width + 6;
        integratedSuperWords[index] = 0xc000;
        integratedSuperBehaviors[index] = 10;
    }
    RoomLevelData integratedSuperWall = new(
        width,
        height,
        integratedSuperWords,
        integratedSuperBehaviors,
        new ushort[integratedSuperWords.Length],
        new byte[8]);
    var integratedSuperSamus = new SamusState
    {
        Pose = rightPose,
        XPosition = 64,
        YPosition = 96,
        SelectedHudItem = 2,
        SuperMissiles = 1,
    };
    var integratedSuperBombs = new SamusBombProjectileSystem();
    var integratedSuperProjectiles = new SamusProjectileSystem();
    var integratedSuperPlms = new RoomPlmSystem();
    SamusProjectileFrameResult integratedSuperResult = default;
    for (int frame = 0; frame < 32 && !integratedSuperResult.CollisionStartedExplosion; frame++)
    {
        integratedSuperBombs.StepFrame(
            bus, integratedSuperWall, integratedSuperSamus, 0, 0);
        integratedSuperResult = integratedSuperProjectiles.StepFrame(
            bus,
            integratedSuperWall,
            integratedSuperSamus,
            frame == 0 ? (ushort)SnesButton.X : (ushort)0,
            frame == 0 ? (ushort)SnesButton.X : (ushort)0,
            0,
            0,
            integratedSuperBombs,
            roomPlms: integratedSuperPlms);
    }
    AssertTrue(integratedSuperPlms.ActiveCount > 0,
        "live Super Missile publishes weapon-gated block PLM");
    int integratedSuperMutations = 0;
    for (int y = 0; y < height; y++)
    {
        if (integratedSuperWall.GetCollisionBlock(6, y).LevelWord == 0x809f)
            integratedSuperMutations++;
    }
    AssertTrue(integratedSuperMutations > 0,
        "live Super Missile collision runs CF67 and synthesizes $809F");

    Console.WriteLine(
        "  Samus beams/missiles: producers, charge flare, linked supers, trails, all point-block families, motion, collision, and explosions agree.");
}

/// <summary>
/// Exercises ordinary Morph-Ball entry, `$F9` endpoint selection, rolling momentum,
/// walk-off, both automatic rebounds, grounded recovery, and blocked unmorph expansion.
/// Every table byte below is copied from the corresponding retail-ROM structure rather
/// than replaced with a host animation or physics constant.
/// </summary>
}
