using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.Verification.Enemies.Corpses;

/// <summary>The species, enemy definition, variant count, instruction table and configuration table of one dead tourian corpse definitions family.</summary>
/// <param name="Species">The species.</param>
/// <param name="EnemyDefinition">The enemy definition.</param>
/// <param name="VariantCount">The variant count.</param>
/// <param name="InstructionTable">The instruction table.</param>
/// <param name="ConfigurationTable">The configuration table.</param>
internal readonly record struct DeadTourianCorpseDefinitionsFamily(DeadTourianCorpseSpecies Species, EnemyDefinitionId EnemyDefinition, int VariantCount, int InstructionTable, int ConfigurationTable);
