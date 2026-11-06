namespace SuperMetroid.FrameDefinitionsGenerator;

/// <summary>Evaluates a <see cref="DefinitionTableConfiguration"/> over Core's source.</summary>
/// <param name="model">The Core facts.</param>
/// <param name="configuration">The behaviour's configuration.</param>
internal sealed class DefinitionTables(CoreModel model, DefinitionTableConfiguration configuration)
{
    /// <summary>
    /// <paramref name="split"/>'s frames with every configured table's frames kept only on the definitions that own the
    /// table; every other frame is left as the walk gave it.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// A definition is unconfigured or not the behaviour's, a table names no frame, or a definition's walk does not reach
    /// its own table's frames.
    /// </exception>
    public IReadOnlyDictionary<string, IReadOnlySet<string>> Apply(Behaviour behaviour, IReadOnlyDictionary<string, IReadOnlySet<string>> split)
    {
        var problems = new List<string>();
        foreach (string definition in configuration.Tables.Keys.Where(definition => !behaviour.Definitions.Contains(definition)))
            problems.Add($"{definition} is not a definition {behaviour.Type.Name} serves");
        foreach (string definition in behaviour.Definitions.Where(definition => !configuration.Tables.ContainsKey(definition)))
            problems.Add($"{definition} has no configured frame tables");
        var own = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        foreach ((string definition, string[] tables) in configuration.Tables)
        {
            var frames = new HashSet<string>(StringComparer.Ordinal);
            foreach (string table in tables)
            {
                if (table.Split('.') is not [var type, var member])
                {
                    problems.Add($"{definition}'s table {table} is not Type.Member");
                    continue;
                }
                HashSet<string> named = model.EnemyFramesNamed(model.MemberInitializer(type, member));
                if (named.Count == 0)
                    problems.Add($"{table} names no frame");
                frames.UnionWith(named);
            }
            if (split.TryGetValue(definition, out IReadOnlySet<string>? reached))
            {
                foreach (string frame in frames.Where(frame => !reached.Contains(frame)))
                    problems.Add($"{definition}'s own table frame {frame} is not a frame its code reaches");
            }
            own.Add(definition, frames);
        }
        if (problems.Count > 0)
            throw new InvalidDataException($"{behaviour.Type.Name}'s definition tables cannot be split:\n" + string.Join('\n', problems));

        var tableFrames = new HashSet<string>(own.Values.SelectMany(frames => frames), StringComparer.Ordinal);
        var result = new Dictionary<string, IReadOnlySet<string>>(StringComparer.Ordinal);
        foreach (string definition in behaviour.Definitions)
        {
            var frames = new HashSet<string>(split[definition].Where(frame => !tableFrames.Contains(frame) || own[definition].Contains(frame)),
                StringComparer.Ordinal);
            if (frames.Count > 0)
                result.Add(definition, frames);
        }
        return result;
    }
}
