using SuperMetroid.SourceAudit;

internal static partial class Program
{
    /// <summary>
    /// Proves every required domain classifier and then enforces the repository's exact
    /// reviewed baseline. Constructed snippets make failures deterministic without needing
    /// to add deliberate bad literals to production code.
    /// </summary>
    static void VerifyProductionMagicNumberAudit()
    {
        const string UnsafeSource = """
            _ = ReadByte(0x8f8000);
            instructionPointer = 0x9438;
            switch (value) { case 0x2: break; }
            _ = block.CollisionType == (RoomCollisionType)0x3;
            _ = block.Bts == new RoomBlockBehavior(0x46);
            samus.Pose = 0x1a;
            projectileKind = 0x0500;
            QueueSound(0x23);
            _ = controllerInput & 0x8000;
            _ = tilemapWord & 0x1c00;
            _ = saveRam[0x10];
            """;
        IReadOnlyList<MagicNumberFinding> findings =
            ProductionMagicNumberAudit.AuditText("FunctionalState.cs", UnsafeSource);
        MagicNumberCategory[] expectedCategories = Enum.GetValues<MagicNumberCategory>();
        foreach (MagicNumberCategory category in expectedCategories)
        {
            AssertTrue(findings.Any(finding => finding.Category == category),
                $"magic-number audit detects {category}");
        }

        AssertEqual(0,
            ProductionMagicNumberAudit.AuditText(
                "ExampleRomData.cs",
                "public const int Address = 0x8f8000;").Count,
            "magic-number audit allows dedicated data catalogs");
        AssertEqual(0,
            ProductionMagicNumberAudit.AuditText(
                "FunctionalState.cs",
                "_ = tilemapWord & 0x1c00; // magic-number-audit: allow(PackedPpuWord) - intrinsic test vector").Count,
            "magic-number audit accepts reasoned category-specific exemption");
        AssertEqual(0,
            ProductionMagicNumberAudit.AuditText(
                "FunctionalState.cs",
                "// instructionPointer = 0x9438;\nstring text = \"0x8f8000\";").Count,
            "magic-number audit ignores comments and strings");

        MagicNumberAuditResult repositoryAudit = ProductionMagicNumberAudit.Run(
            ProductionMagicNumberAudit.FindRepositoryRoot());
        if (!repositoryAudit.Passed)
        {
            throw new InvalidOperationException(
                "Production magic-number audit found unreviewed literals:\n" +
                string.Join('\n', repositoryAudit.NewFindings.Select(finding => finding.Diagnostic)));
        }

        Console.WriteLine(
            $"  Magic numbers: 11 domain classifiers enforced; baseline " +
            $"{repositoryAudit.BaselineCount}, current {repositoryAudit.CurrentFindingCount}, " +
            $"retired {repositoryAudit.RetiredBaselineEntries}.");
    }
}
