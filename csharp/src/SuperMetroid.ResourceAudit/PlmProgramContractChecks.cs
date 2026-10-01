using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Assets;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.ResourceAudit;

/// <summary>
/// Small confirmations of #1161's known record omissions and audit contracts.
/// No room setup, controller input or gameplay search is involved.
/// </summary>
internal static class PlmProgramContractChecks
{
    internal static void Run()
    {
        var absentDraw = Audit(new Dictionary<ushort, ushort>
        {
            [PlmAuditFixtureIds.Start] = 4,
            [PlmAuditFixtureIds.Start + 4] = RoomPlmInstructionCodes.Delete,
        });
        Require(absentDraw.Findings.Single().Address == PlmAuditFixtureIds.Start + 2,
            "A duration without its draw-pointer operand must fail, not fall through to WRAM.");

        var absentByte = Audit(new Dictionary<ushort, ushort>
        {
            [PlmAuditFixtureIds.Start] = RoomPlmInstructionCodes.QueueSoundLibrary2Maximum6,
            [PlmAuditFixtureIds.Start + 3] = RoomPlmInstructionCodes.Delete,
        });
        Require(absentByte.Findings.Single().Code == "missing-byte", "An odd-byte sound operand must be checked.");

        var branch = Audit(new Dictionary<ushort, ushort>
        {
            [PlmAuditFixtureIds.Start] = RoomPlmInstructionCodes.GotoIfSamusHasNoBombs,
            [PlmAuditFixtureIds.Start + 2] = PlmAuditFixtureIds.AbsentTarget,
            [PlmAuditFixtureIds.Start + 4] = RoomPlmInstructionCodes.Delete,
        });
        Require(branch.Findings.Single().Address == PlmAuditFixtureIds.AbsentTarget,
            "Both conditional outcomes must be visited even if one runtime branch would not execute.");

        var linked = Audit(new Dictionary<ushort, ushort>
        {
            [PlmAuditFixtureIds.Start] = RoomPlmInstructionCodes.LinkInstruction,
            [PlmAuditFixtureIds.Start + 2] = PlmAuditFixtureIds.AbsentTarget,
            [PlmAuditFixtureIds.Start + 4] = RoomPlmInstructionCodes.Sleep,
        });
        Require(linked.Findings.Single().Address == PlmAuditFixtureIds.AbsentTarget,
            "A link operand is a future program root, even when the current list sleeps.");

        var loop = Audit(new Dictionary<ushort, ushort>
        {
            [PlmAuditFixtureIds.Start] = RoomPlmInstructionCodes.Goto,
            [PlmAuditFixtureIds.Start + 2] = PlmAuditFixtureIds.Start,
        });
        Require(loop.Findings.Count == 0 && loop.Records == 1, "Control-flow cycles must terminate the audit deterministically.");

        var unknown = Audit(new Dictionary<ushort, ushort> { [PlmAuditFixtureIds.Start] = PlmAuditFixtureIds.UnknownOpcode });
        Require(unknown.Findings.Single().Code == "unknown-opcode", "An unknown opcode cannot receive an assumed width.");
        var unowned = Audit(new Dictionary<ushort, ushort>
        {
            [PlmAuditFixtureIds.Start] = 4,
            [PlmAuditFixtureIds.Start + 2] = RoomPlmShotBlockDrawDefinitions.SingleFrame0,
            [PlmAuditFixtureIds.Start + 4] = RoomPlmInstructionCodes.Delete,
        }, ownsDraw: false);
        Require(unowned.Findings.Single().Code == "missing-draw", "An existing pointer word does not prove a draw provider owns it.");

        // Recreate the precise prior bug against the real production resolver:
        // remove only CBBC while keeping the ordinary shot program complete.
        ushort omitted = RoomPlmInstructionLists.PermanentShotBlock2x1 + 5;
        var regression = new PlmProgramAuditReport();
        bool MissingReportedOperand(ushort address, out ushort value)
        {
            if (address != omitted) return RoomPlmProgramDefinitions.TryReadWord(address, out value);
            value = 0;
            return false;
        }
        new PlmProgramAudit.Walker(MissingReportedOperand, RoomPlmProgramDefinitions.TryReadByte, _ => true, regression)
            .Visit(RoomPlmInstructionLists.PermanentShotBlock2x1, "reported CBBC omission");
        Require(regression.Findings.Single().Address == omitted, "The reported CBBC hole must fail this audit.");

        // #1162: complete operands are insufficient when the source artwork was
        // never installed. Recreate that precise missing-provider handoff statically.
        var missingArtwork = new PlmProgramAuditReport();
        new PlmProgramAudit.Walker(RoomPlmProgramDefinitions.TryReadWord,
            RoomPlmProgramDefinitions.TryReadByte, _ => true, missingArtwork,
            (_, _) => false).Visit(BombTorizoHandPlmProgramDefinitions.DebrisUploadInstruction, "reported missing debris art");
        Require(missingArtwork.Findings.Single().Code == "missing-artwork-transfer" &&
            missingArtwork.ArtworkTransfers == 1,
            "The Bomb Torizo DMA must fail static closure when its artwork provider is absent.");
        var installedArtwork = new PlmProgramAuditReport();
        new PlmProgramAudit.Walker(RoomPlmProgramDefinitions.TryReadWord,
            RoomPlmProgramDefinitions.TryReadByte, _ => true, installedArtwork)
            .Visit(BombTorizoHandPlmProgramDefinitions.DebrisUploadInstruction, "installed debris art");
        Require(installedArtwork.Findings.Count == 0 && installedArtwork.ArtworkTransfers == 1,
            "The actual Torizo DMA must resolve the complete installed page.");
        TorizoInstructionTileSheetDefinition debris = TorizoInstructionVramArtworkDefinitions.ChozoDebris;
        Require(PlmVramArtworkAudit.OwnsInstalledTransfer(debris.SourceAddress, debris.ByteCount) &&
            !PlmVramArtworkAudit.OwnsInstalledTransfer(debris.SourceAddress, debris.ByteCount + 1),
            "PLM DMA artwork admission is bounded to the installed page, not the source bank.");

        ConfirmRestoredRecords();
        var original = (MethodDeclarationSyntax)SyntaxFactory.ParseMemberDeclaration("bool Read() { return true; }")!;
        var changed = (MethodDeclarationSyntax)SyntaxFactory.ParseMemberDeclaration("bool Read() { return false; }")!;
        var trivia = (MethodDeclarationSyntax)SyntaxFactory.ParseMemberDeclaration("bool Read() { /* comment */ return true; }")!;
        Require(PlmProgramSource.TokenHash(original) != PlmProgramSource.TokenHash(changed) &&
            PlmProgramSource.TokenHash(original) == PlmProgramSource.TokenHash(trivia),
            "Behavioral source changes must revoke the reviewed contract; whitespace/comments must not.");
        Console.WriteLine("PLM audit contracts passed: absent word/byte/draw, both branch paths, linked wake root, " +
            "unknown opcode, cycles, reported CBBC omission, missing DMA artwork and corrected native records.");
    }

    private static void ConfirmRestoredRecords()
    {
        // Representative native operands from each corrected family, including
        // reverse animation and linked restoration, not just a no-throw endpoint.
        Word(RoomPlmInstructionLists.CollisionBombBlock1x1Respawning + 5,
            RoomPlmInstructionLists.ReactionBombBlock1x1Respawning + 3);
        Word(RoomPlmInstructionLists.ReactionBombBlock2x2Respawning + 5,
            RoomPlmShotBlockDrawDefinitions.SquareFrame0);
        Word(RoomPlmInstructionLists.ReactionBombBlock2x2Respawning + 29,
            RoomPlmShotBlockDrawDefinitions.SquareFrame0);
        Word(RoomPlmInstructionLists.ReactionBombBlock2x2Respawning + 33,
            RoomPlmBombBlockRestoreDrawDefinitions.Square);
        Word(RoomPlmInstructionLists.ContactCrumble2x1Respawning + 33,
            RoomPlmContactCrumbleRestoreDrawDefinitions.Horizontal);
        Word(RoomPlmInstructionLists.ContactCrumble1x2Permanent + 17,
            RoomPlmShotBlockDrawDefinitions.VerticalFrame0 + 24);
        Word(RoomPlmInstructionLists.RespawningBreakableGrappleBlock + 2,
            RoomPlmGrappleBlockDrawDefinitions.Grapple);
        Word(RoomPlmInstructionLists.RespawningBreakableGrappleBlock + 21,
            RoomPlmGrappleBlockDrawDefinitions.Blank);
        Word(RoomPlmInstructionLists.RespawningBreakableGrappleBlock + 33,
            RoomPlmGrappleBlockDrawDefinitions.BreakFrame0);
        Word(RoomPlmInstructionLists.RespawningSuperMissileBlock, RoomPlmInstructionCodes.QueueSoundLibrary2Maximum6);
        Word(RoomPlmInstructionLists.PermanentPowerBombBlock + 3, 3);
        Word(RoomPlmInstructionLists.PermanentPowerBombBlock + 7, 2);
        Word(RoomPlmInstructionLists.PermanentPowerBombBlock + 11, 1);
        Word(EnemyBreakableTerrainDefinitions.InstructionList, RoomPlmInstructionCodes.QueueSoundLibrary2Maximum3);
        Word(RoomPlmInstructionLists.CrumbleReveal1x1 + 2, RoomPlmBombedRevealDrawDefinitions.CrumbleSingle);
        Word(RoomPlmInstructionLists.BombedPowerBombBlockUnused + 2, RoomPlmBombedRevealDrawDefinitions.PowerBomb);
        Word(EscapeAnimalPlmRomData.ReactionList + 17, EscapeAnimalPlmDrawDefinitions.Blank);
        Word(EscapeAnimalPlmRomData.ReactionList + 19, EscapeAnimalPlmRomData.SetEscapedEventInstruction);
        Require(EscapeAnimalPlmDrawDefinitions.TryGet(EscapeAnimalPlmDrawDefinitions.Blank, out var wall) &&
            wall.Runs.Span[0].DirectionAndCount == 0x8003 &&
            wall.Runs.Span[0].LevelWords.Span.SequenceEqual(new ushort[] { 0x80ff, 0x80ff, 0x80ff }),
            "Rescue-wall blank frame must mutate all three vertical terrain words, not just render one block.");
    }

    private static void Word(int address, int expected) => Require(
        RoomPlmProgramDefinitions.TryReadWord(checked((ushort)address), out ushort actual) && actual == expected,
        $"Native record $84:{address:X4} must contain ${expected:X4}.");

    private static PlmProgramAuditReport Audit(Dictionary<ushort, ushort> words, bool ownsDraw = true)
    {
        var report = new PlmProgramAuditReport();
        new PlmProgramAudit.Walker(words.TryGetValue,
            (ushort address, out byte value) => { value = 0; return false; }, _ => ownsDraw, report)
            .Visit(PlmAuditFixtureIds.Start, "constructed record");
        return report;
    }

    private static void Require(bool condition, string description)
    {
        if (!condition) throw new InvalidDataException("PLM audit contract failed: " + description);
    }
}

internal static class PlmAuditFixtureIds
{
    internal const ushort Start = 0xf000;
    internal const ushort AbsentTarget = 0xf100;
    internal const ushort UnknownOpcode = 0xffff;
}
