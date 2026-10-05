namespace SuperMetroid.ResourceAudit;

/// <summary>Individually reviewed complete enemy color sources; no AI, battle or palette cadence is exercised.</summary>
internal static class RemainingEnemyColorClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.BotwoonColorCatalog", "botwoon-complete-health-colors", ["HealthColor"],
            [new("csharp/src/SuperMetroid.Core/Assets/BotwoonColorCatalog.cs", "849F9F333F01D3349173EC4B239B49C00590A501A67616B2488AEE4A3664D31A"),
             new("csharp/src/SuperMetroid.Core/Game/BotwoonHealthPaletteDefinitions.cs", "DD4DC9B57ABB2B15C9C1CFF83AFDC87B420BEACFD70417B21F2E24B8FE829C2B")],
            "The sole private-constructor loader requires eight sixteen-color health bands and compiles independent arrays. HealthColor guards band/color bounds. Health thresholds and their signed comparison are not executed or certified."),
        new("SuperMetroid.Core.Assets.BabyMetroidCutsceneColorCatalog", "cutscene-baby-complete-color-images", ["InitialColor", "FadeColor"],
            [new("csharp/src/SuperMetroid.Core/Assets/BabyMetroidCutsceneColorCatalog.cs", "46F3505AB98C768E6A1E4595E0F6B06D5278AB68AD19AA532C0175EA3784EBC7"),
             new("csharp/src/SuperMetroid.Core/Game/BabyMetroidCutsceneColorRomData.cs", "6204D0F2EF5780EC535FBAC57AD3618FA8E59BE5A283398EC40C45C77633D869")],
            "Private construction requires fifteen initial colors and six fourteen-color fade rows, compiled independently. InitialColor and FadeColor guard their separate widths; fade palette indices are one-based one through six. Cutscene state/timing is outside this proof."),
        new("SuperMetroid.Core.Assets.NorfairRidleyColorCatalog", "norfair-ridley-complete-color-images", ["ApplyInitial", "ApplyReveal", "ResolveInitial", "ResolveReveal"],
            [new("csharp/src/SuperMetroid.Core/Assets/NorfairRidleyColorCatalog.cs", "7FDE7CBA28EA43F4CA1E6A672F94BD769273DDE9C90F725EECC5FD3C8BC4DB2D"),
             new("csharp/src/SuperMetroid.Core/Game/NorfairRidleyPaletteRomData.cs", "74B727AF1CDBFA0956092B2FD84ED0FF91529B13F7DF528C347D2D6A32F2C163")],
            "Private construction requires thirty-two initial colors and fifteen fourteen-color reveal rows. All arrays are compiled independently. Reviewed methods use fixed fields or CLR-checked array indices; complete domains do not certify arbitrary caller indices. Reveal cadence and boss phases are unchanged."),
        new("SuperMetroid.Core.Assets.EnemyAuxiliaryColorCatalog", "enemy-auxiliary-complete-color-images", ["Resolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/EnemyAuxiliaryColorCatalog.cs", "385C18FB6E31BAFF217579A3B482B6CE376999A8D5E7C80ACA2E189C1C3A85CB"),
             new("csharp/src/SuperMetroid.Core/Assets/EnemyAuxiliaryColorDefinitions.cs", "BC86BF385470ADA3AB9E8C98DB8520E402EA457F3357082F1636A42EF41FF6B5")],
            "The private-constructor loader requires all four named auxiliary palettes and each definition's exact frame/color counts, compiled into independent arrays. Resolve guards membership and selected frame/color bounds. Known short palettes cannot borrow another palette's dimensions; enemy cadence is not certified."),
        new("SuperMetroid.Core.Assets.KraidColorCatalog", "kraid-complete-color-sources", ["Resolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/KraidColorCatalog.cs", "6F95D8AF47229B3D4478BE064A8EAF6932AA2370E3C61C43FF607B087449D729"),
             new("csharp/src/SuperMetroid.Core/Game/KraidPaletteRomData.cs", "2B20227015C8C601187D4F81BD60476B03547CE6295E2556F1B71FA48E825813")],
            "Private construction requires all five sources with their exact sixteen- or one-hundred-forty-four-color lengths and compiles independent words. Resolve guards source membership and selected array bounds. Known short sources cannot borrow health/secondary capacity. Fade arithmetic and battle phases are not executed."),
        new("SuperMetroid.Core.Assets.ZebetiteColorCatalog", "zebetite-complete-pulse-colors", ["Apply"],
            [new("csharp/src/SuperMetroid.Core/Assets/ZebetiteColorCatalog.cs", "3A4EE38D8359F81AFC08FD54538E40031B1239994E1C922945BD7820A1A42025")],
            "The sole private-constructor loader requires eight two-color pulse frames and compiles independent arrays. Apply guards the frame and uses a fixed loaded row; destinationColor is CGRAM placement. Pulse timing and enemy mechanics are unchanged."),
        new("SuperMetroid.Core.Assets.ShitroidColorCatalog", "shitroid-complete-live-color-images", ["NormalColor", "TargetColor"],
            [new("csharp/src/SuperMetroid.Core/Assets/ShitroidColorCatalog.cs", "12F7860798BE02F0057DE1DCBACC84F9E02F4BCC833E0646C6BCB6855057406F"),
             new("csharp/src/SuperMetroid.Core/Game/ShitroidColorRomData.cs", "69D8E2EB4828CF59C5E5715FEC479A9BA175510F8269091A8C0FBD44A525DE93")],
            "Private construction requires eight four-color normal frames and three sixteen-color targets, compiled independently. NormalColor/TargetColor guard their distinct widths and the exact target enum. Drain/fade timing and actor state are outside this proof."),
        new("SuperMetroid.Core.Assets.CeresRidleyMode7ColorCatalog", "ceres-ridley-mode7-complete-shades", ["Apply", "Resolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/CeresRidleyMode7ColorCatalog.cs", "8D96C4093DA59B8B236FCD531D77235E35ABA2DE8C336FD2B0BAEFF0782CC99E"),
             new("csharp/src/SuperMetroid.Core/Game/CeresRidleyPaletteRomData.cs", "20CB1E292CC94F8BB30D9F237F148789E3E165F58BD116D5259990370C9E17CE")],
            "Private construction requires nine fifteen-color zoom shades and compiles independent rows. Apply/Resolve guard zoom-high-byte/color bounds. Movement, rotation, scaling and the palette clock are not certified."),
        new("SuperMetroid.Core.Assets.DachoraColorCatalog", "dachora-complete-phase-colors", ["Resolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/DachoraColorCatalog.cs", "09E5557FCCE22A99D5D012C5E7B0BDA9E276978D5BD97C506954A1F3D37FC912"),
             new("csharp/src/SuperMetroid.Core/Game/DachoraColorRomData.cs", "AEB49D309BF39A60BF98D611B68B5288B4829EA7263421DBEC34C0AF88201CCE")],
            "Private construction requires the normal row and four speed/four shine rows, all sixteen colors and compiled independently. Resolve guards each phase's exact frame/color domain; Default owns only frame zero. Enemy speed/shine clocks are not executed."),
        new("SuperMetroid.Core.Assets.MotherBrainHealthPalettePresentation", "mother-brain-complete-health-colors", ["Apply"],
            [new("csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPalettePresentation.cs", "05E148A6572B5F7FC629794FC4F0853D9E73DC2D861C721B4843CBB7FF3169E4"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPaintDefinitions.cs", "CED903B0B3BD50BFD557B00AF84843408D5E3476F04BAA309BE6C043B101C37D"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainRainbowPaletteRomData.cs", "8FF000D50D77216696DF8B2BB3D75D90BBC90B697FEEEFAAFEAA07325CE6135E")],
            "The sole private-constructor loader requires all four fifteen-color body/back-leg state pairs and compiles independent arrays. Apply guards damageState and selects only those loaded pairs. Damage thresholds, rainbow transitions and battle timing are not certified."),
    ];
}
