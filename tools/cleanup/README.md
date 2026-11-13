# The asynchronous cleanup

Code cleanliness that a tool can fix must not cost the developer's time (owner decision, 2026-11-11). Anything a tool
fixes deterministically never blocks a build, a commit or a push: the cleanup fixes it after the push. The rules whose
every finding an existing code fix resolves, with no choice and no change of behaviour, sit at `none` in
`csharp/.editorconfig` under the comment that names this cleanup, so the build skips their analyzers and spends no
time on them. Dead code, stale inventories and stale architecture reviews never block a push either: CI runs no
dead-code check, and its architecture audit runs `--findings`, which fails only on what needs judgment. After every
push this cleanup takes the newest tip of `main`, applies those fixes, prunes dead code and stale reviews, regenerates
the inventories, confirms the result builds, and commits and pushes what changed. It runs on this machine, in its own
clone, at Idle priority, and the developer never waits for it.

Every other rule stays a full-severity build error, and every finding that needs judgment still blocks. Only the
owner moves a rule into the automatically fixed group (`docs/manifesto.md` sections 5, 6 and 8).

## What runs, and when

| Piece | Where | What it does |
| --- | --- | --- |
| `.githooks/reference-transaction` | the main checkout's hooks path | Git runs it for every ref update. When a push or fetch moves `refs/remotes/origin/<branch>` it runs the installed `trigger.ps1` and returns. It never fails a transaction or waits for the cleanup. |
| `trigger.ps1` | installed in `<dir>\bin` | Appends the request to `requests.log` and, if no service holds `service.lock`, starts `service.ps1` detached through WMI: Idle priority, a headless console, outside the caller's process tree. |
| `service.ps1` | installed in `<dir>\bin` | One at a time. Fetches, resets the clone hard to the remote tip, checks the installed scripts still match the tree, runs the tree's `tools/cleanup/run.ps1`, records the outcome, and repeats while requests are pending. |
| `run.ps1` | the clone's tree | One run: restore, the reachability audit's Release build, the fix and prune steps (code, dead code, stale reviews), the inventories, one build, commit, push. |
| `CodeFixRunner.cs` | the clone's tree | The code fixes, as a Roslyn workspace runner (below). |

Git has no `post-push` hook; a successful push updates the remote-tracking ref through a ref transaction, so
`reference-transaction` is where a push is seen after it has landed. A fetch that brings new commits triggers it too,
which also picks up pushes made from elsewhere. The hook costs a shell start per ref transaction and nothing more
until that ref moves.

`<dir>` is `G:\git\SuperMetroidDecomp-cleanup`. It holds `repo\` (a full clone of `origin`, never a worktree of the
developer's checkout, with its own `bin`/`obj` and its own `temp\`), `config.json`, `STATUS`, `state.json`,
`requests.log`, `service.log`, `trigger.log` and `logs\`.

### The run

1. **Restore** the solution, **build the reachability audit in Release** and **the repository's analyzer and code fixes
   in Debug** (`csharp/src/SuperMetroid.EnsureAnalyzer.CodeFixes`) through `tools/build.ps1`. The code fixes load the
   repository's analyzer and fixers from the projects' Debug references, so they must be built first.
2. **Fix and prune, taking turns, until one of each in a row changes nothing:**
   - **Code fixes:** `dotnet run tools/cleanup/CodeFixRunner.cs -- csharp/SuperMetroid.Full.slnx <summary.json> CA1823,CS0162,CS0168,CS0169,CS0219,CS0414,IDE0004,IDE0005,IDE0032,IDE0044,IDE0051,IDE0052,IDE0059,IDE0076,IDE0077,IDE0079,IDE0330,SME6360,SME6391,SME6400,SME6401`.
     `run.ps1` reads the rules from `csharp/.editorconfig`: the `none` lines directly under the comment that names
     this cleanup. A rule there that `run.ps1` has no commit-message words for, or words for a
     rule not there, fails the run.
   - **Dead code:** `dotnet run --project csharp/src/SuperMetroid.ReachabilityAudit -c Release --no-build -- --solution csharp/SuperMetroid.Full.slnx --prune-keeping-verification-used unreachable-member,unreachable-type,unreferenced-field,unreferenced-enum-member,unreferenced-constant --path-prefix csharp/src --to csharp/test-support/VerificationAccess.Core --retired-fields <run>\retired-fields-<step>.txt`.
     It deletes dead code tests do not use and moves what they still use into test-support; the next code-fix step
     removes the usings it copies into moved files.
   - **Stale reviews:** `dotnet run --project csharp/src/SuperMetroid.ArchitectureAudit -c Release -- --solution csharp/SuperMetroid.Full.slnx --prune-stale-reviews`.
     It deletes every `[ArchitectureReview]` whose finding its type no longer has (a prune of dead code can remove a
     finding), through the attribute's own syntax reference, and keeps every other byte of the file. It validates
     every edit before writing any and writes nothing if one fails. The next code-fix step removes a using the
     attribute alone needed.
   - A step that returns the tree to an earlier state fails the run (the steps undo each other); so does a run that
     needs more than 6 rounds of the three steps.
3. **Inventories:** `dotnet run --project csharp/src/SuperMetroid.ArchitectureAudit -c Release -- --solution csharp/SuperMetroid.Full.slnx --write`.
4. **Nothing changed:** the run ends quietly. A change outside `csharp/` and `docs/` fails the run.
5. **Build once:** `pwsh -File tools/build.ps1 csharp/SuperMetroid.Full.slnx`. A failed build pushes nothing; its full
   output is kept as `logs\<run>\build.log`, STATUS says so and a notification is raised. A build failure is never
   fixed by judgment. The build changes no file; a tree it changed fails the run.
6. **Commit** as `Cleanup: <rule, what, count; ...>` (stale reviews as `stale architecture reviews removed <count>`),
   with the files, the prune's own summary and every removed review in the body, under
   the owner's identity from the main checkout and with no assistant attribution (the clone runs the repository's
   own `commit-msg` and `pre-push` guards), and **push** with a plain fast-forward push.

The service gives every tool its own temporary folder, no shared compiler server (`UseSharedCompilation=false`), no
reused MSBuild nodes and no MSBuild server, so nothing the cleanup starts serves the developer's builds or runs at the
cleanup's Idle priority for them. The NuGet package cache is shared read-mostly, as the brief allows. Git credential
prompts are disabled (`GCM_INTERACTIVE=never`), so a missing credential fails the push instead of opening a dialog.

### The code fixes

`CodeFixRunner.cs` loads every project into one MSBuildWorkspace, runs each project's own analyzers for the requested
rules with its own configuration, and applies each rule's fixer through its fix-all provider, given exactly the
findings chosen. `RuleOrder` says what reports each rule and where its one fixer comes from:

| Rule | Reported by | Fixer |
| --- | --- | --- |
| IDE0076, IDE0077 | code-style analyzer | Roslyn code-style |
| IDE0079 | the runner (below) | `UnnecessarySuppressionCodeFixProvider` |
| SME6360 | `PartialSplitAnalyzer` | `PartialMergeCodeFixProvider` |
| SME6400 | `OneTypePerFileAnalyzer` | `OneTypePerFileCodeFixProvider` |
| SME6401 | `TypeFileNameAnalyzer` | `TypeFileNameCodeFixProvider` |
| IDE0004 | code-style analyzer | Roslyn code-style |
| CS0162 | the compiler | `UnreachableCodeCodeFixProvider` |
| IDE0059 | code-style analyzer | Roslyn code-style (`csharp_style_unused_value_assignment_preference = discard_variable`) |
| CS0219 | the compiler | `UnreadLocalCodeFixProvider` |
| CS0168 | the compiler | `UnusedVariableCodeFixProvider` |
| CA1823, IDE0051 | NetAnalyzers, code-style analyzer | `UnusedMemberCodeFixProvider` |
| CS0169 | the compiler | `UnusedMemberCodeFixProvider` |
| IDE0052 | code-style analyzer | `WriteOnlyMemberCodeFixProvider` |
| CS0414 | the compiler | `WriteOnlyMemberCodeFixProvider` |
| IDE0330, IDE0032, IDE0044 | code-style analyzer | Roslyn code-style |
| SME6391 | `SpanAdapterAnalyzer` | `AdapterChainCodeFixProvider` |
| IDE0005 | code-style analyzer | Roslyn code-style |

The order is the order the rules are applied in within a file: suppressions, then the file's layout, then code, then
the usings the other fixes leave unnecessary. Roslyn's code-style fixers are the ones `dotnet format style` uses; on a
fixture the runner's output for IDE0004, IDE0005, IDE0032, IDE0044, IDE0076 and IDE0077 is byte-identical to
`dotnet format style --diagnostics ... --severity info`. The repository's fixers are in
`SuperMetroid.EnsureAnalyzer.CodeFixes`, which every project references as an analyzer; the runner loads them from the
projects' Debug references, which `run.ps1` builds first. A fixer resolves every finding without changing behaviour:
a finding it offers no fix for, or cannot resolve without a choice, fails the run, naming the site, and is never
skipped. A dead store or initializer whose value could have an effect keeps its evaluation as `_ = value;`.

Since the group is at `none`, the runner enables exactly the requested rules, as warnings, in its own analysis: in
the compilation's per-file diagnostic options (where the `.editorconfig` severity would otherwise win over any
compilation-wide setting) and in the analyzer configuration the code-style analyzers read their severity from. It
enables each rule only where the project's configuration would, were the rule's group line `warning`: it reads the
project's analyzer configuration files again with that one line changed, so the sections that turn the SME rules off
outside Core and AssetExtraction keep them off there. Every other rule keeps the project's configuration, and the
build itself never runs these analyzers.

A compiler warning in the group (CS0162, CS0168, CS0169, CS0219, CS0414) has no analyzer: the compiler reports it
while it binds. The runner reads it from the compilation's own diagnostics, with the rule enabled in the per-file
options like any other, and applies the repository's fixer for it. Such a rule is also absent from `WarningsAsErrors`
in `csharp/Directory.Build.props`; since the compiler computes it during binding anyway, `none` saves the build its
reporting, not its analysis. A project that compiled when the run started and has compiler errors after a pass fails
the run, naming them.

Roslyn skips an analyzer only when every rule it reports is off. A repository analyzer that reported a moved rule
beside one that stays an error would still run in every build, so a moved rule gets an analyzer of its own: SME6391 is
`SpanAdapterAnalyzer`, split from `AdapterChainAnalyzer` (SME6390), and SME6360 is `PartialSplitAnalyzer`, split from
`PartialTypeAnalyzer` (SME6361). Roslyn's own analyzers cannot be split: `CSharpRemoveUnusedValuesDiagnosticAnalyzer`
reports IDE0059 beside IDE0058 (`silent`), so it still runs in the build.

**IDE0079 (unnecessary suppression) has no command-line analyzer** (only the IDE's suppression analysis reports it),
so the runner finds its sites itself. When IDE0079 is requested it runs every analyzer of each project, with the whole
group enabled in its scope and suppressed diagnostics reported (`CompilationWithAnalyzers`,
`reportSuppressedDiagnostics: true`). A `[SuppressMessage]` or `[UnconditionalSuppressMessage]` (on a member, or at
assembly level with a target) is needed when it is the attribute that suppressed a diagnostic; a
`#pragma warning disable` code is needed when a diagnostic of that id, suppressed by no attribute, lies between it
and its `restore`. Every other suppression of a rule its file enables is unnecessary, and the repository's
`UnnecessarySuppressionCodeFixProvider` removes it: a pragma loses the unused codes (and its line, and the matching
restore's, when none is left), an attribute leaves its list (and the list's line when it was alone). A suppression
without codes, of an unknown rule, of a rule its file does not enable, or in `obj/` is left alone. The analysis sees
one configuration (Debug), so a suppression whose range or member holds code another configuration compiles (an
inactive `#if` branch) is kept.

It is a runner rather than `dotnet format` because of shared files. A file compiled into several projects
(`csharp/test-support` through `.projitems`, or a `<Compile Link>`) is edited only where every project compiling it
reports the same finding at the same place, and not at all while one of them has a compiler error. Where the projects
disagree about a file's unused usings, only the usings all of them report are removed. `dotnet format` judges a shared
file from one project and would remove usings another project needs. Disagreeing findings are left and counted in the
commit body.

The runner repeats its analysis until a pass finds nothing (one rule per file per pass, since other spans go stale
once a file changes). A file that returns to an earlier text fails the run, and so do more than 20 passes.

A fix may change several files, such as IDE0032 deleting a field in one part of a partial type and rewriting its
property in another. The runner accepts it when every file it changes is compiled by analyzed projects that have no
compiler errors (otherwise its findings are kept and counted), and when no other fix in the same pass touched one of
those files (otherwise it waits for the next pass, since its spans would be stale). Every project compiling one of
its files must still compile afterwards, or the run fails naming the fix. The cycle and pass checks cover every
changed file, and the summary lists them all.

A fix may also add, rename or delete files: SME6400 moves a type into a new file, SME6401 renames a file after its type,
and SME6360 merges a type's hand-written parts into one and deletes the files it empties. The runner adds a new file to
every project that compiles the file its finding is in, creates it with that file's byte-order mark (it must not exist
yet), deletes a removed file, and keeps the explicit source lists in step: in a project file or an imported
`.projitems`, a single-line `<Compile Include="..."/>` item that names a deleted file goes, one that names a renamed
file is retargeted (its `Link` too), and a file created beside a listed one gets a copy of that item right after it. A
wildcard item already covers a new file in its folder. Git records a renamed file's deletion and creation as a rename.
The summary lists every file written, created or deleted.

## Fixtures

`tools/cleanup/fixtures/<rule>/` confirms the runner on one rule: `input/` is a small project (`Fixture.csproj`) with
the violating code of every shape the rule's ticket lists, and `expected/` is every file of that project after the
fix, byte for byte.

```powershell
pwsh -File tools/cleanup/check-fixtures.ps1                  # every fixture
pwsh -File tools/cleanup/check-fixtures.ps1 -Rule IDE0032    # only the named ones
```

It builds `SuperMetroid.EnsureAnalyzer.CodeFixes` (and so the analyzer) once in Debug, then for each fixture copies
`input/` into a new temporary directory beside the repository's `csharp/.editorconfig` and a `Directory.Build.props`
that imports `csharp/Directory.Build.props` and names the analyzer and code-fix assemblies (`$(CleanupAnalyzer)`,
`$(CleanupCodeFixes)`) for the fixture's own `<Analyzer>` items. It restores the copy, runs the runner on it for that
rule, compares every file (less `bin/`, `obj/` and the two added files) with `expected/`, including which files
exist, and builds the fixed copy with `tools/build.ps1`. A failed fixture keeps its copy and prints where; the script
exits nonzero when any fixture fails.

`tools/cleanup/prune-fixtures/<name>/` confirms the dead-code prune the same way: `input/` is a small repository whose
`csharp/Fixture.slnx` lists projects named like the shipped, host and verification projects (the reachability audit
knows them by name), with test-support shared through `.projitems`, and `expected/` is every file of it after the prune.

```powershell
pwsh -File tools/cleanup/check-prune-fixtures.ps1                         # every prune fixture
pwsh -File tools/cleanup/check-prune-fixtures.ps1 -Name holder-collision  # only the named ones
```

It builds `SuperMetroid.ReachabilityAudit` once in Release, then for each fixture copies `input/` into a new temporary
directory with a `csharp/Directory.Build.props` that imports `csharp/Directory.Build.props` and names the repository's
`csharp/test-support` (`$(RepositoryTestSupport)`, for the host's console error policy), restores it, runs the prune on
it with the arguments `run.ps1` uses and `csharp/test-support/VerificationAccess.Core` as the destination, compares
every file with `expected/` as above, and builds the pruned copy. `holder-collision` holds the shapes of the
2026-11-13 failure at a7f8ec16e6: a moved instance member whose `<Owner>Access` class another test-support folder
already declares (it is merged into), a moved static member whose `<Owner>Constants` name an instance class holds (the
first free name, `<Owner>Constants2`, is used), and `inheritdoc` and `see` crefs to both, which follow them.
`extension-block` holds the shape of the 2026-11-13 failure at 2390b2f3af: a whole extension block moves into a new
`<Owner>Constants` class with no modifier on its header, the member it declared without one becomes internal, and the
staying private members it reaches through its owner's name or through a receiver of the owner's other block become
internal. `extension-block-merge` moves a block into an existing `<Owner>Constants` class that already declares a block
for the same receiver. `holder-modifiers` holds the other modifiers the prune writes: a moved `protected internal` member
becomes `internal`, a moved `new` member drops `new`, a struct's `readonly` instance members drop `readonly` as extension
members, an internal property's private setter that moved code writes loses its modifier, and both parts of a partial
method that moved code calls become internal. `moved-crefs` holds crefs to members inside a moved extension block (a
generic one among them) and a moved nested type: `see`, `seealso` and `inheritdoc`, written with the owner, with its
namespace too, or alone inside the moved code; a moved block's cref to a member of the owner's staying block; and a
labelled `see` in shipped code, which becomes its label. `partial-members` moves a partial method without an
implementation as written and an implemented one, both into a partial `<Owner>Constants`, and joins the two parts of an
instance partial method and property into one extension member each. `same-receiver-blocks` holds two extension blocks
of one class taking the same receiver, one production uses and one only checks use: the second moves whole. Such blocks
share a documentation-comment ID, so the audit keys each block by where it is declared as well; before, the two counted
as one reachable declaration, the second block's members moved one by one and an empty block stayed in shipped code. A
staying block every member of which would move out one by one fails the run.

The prune puts an owner's moved members in the class its style names, in the owner's namespace, unless a project
compiling the destination already knows that name. It then merges into that class when it is one static class (abstract,
for an adapter of moved interfaces) that exactly the destination's projects compile, so the moved code sees what it would
see in the destination and reaches no other project, and otherwise passes the name over for the next free one. A merge
into a class that already declares a moved member fails the run; an extension block is compared member by member, under
its receiver.

Every modifier the prune writes is one its declaration takes (`RelocationModifiers` in
`csharp/src/SuperMetroid.ReachabilityAudit/Removal`): a moved declaration, and a staying one moved code uses, become
internal; an extension block, an explicit interface implementation and an operator keep their declarations as written. A
declaration a holder class cannot take, a staying member whose accessibility is bound to what it overrides or what
overrides it, and an extension block whose owner's adapter is abstract fail the run instead of being written wrongly.
A partial member moves into a partial class (an existing class is merged into only when it is partial). A partial
method without an implementation moves as written, private to its new class, and the run fails when code outside that
class reaches it or when a staying one must become internal. An instance partial member that becomes an extension
member is written once from its two parts, and the run fails when they are not both repository source.

## Seeing status

```powershell
pwsh -File tools/cleanup/status.ps1
```

prints `STATUS`: `idle`, `running`, `failed` or `stopped`, the tip, what happened, and the run's log. A failed run, a
stop and a trigger that cannot start the service also raise a Windows toast (`notify.ps1`, through Windows
PowerShell), which never takes focus. Every run's full output is `logs\<utc>-<tip>.log`, with its code-fix summaries,
retired fields, build logs and commit message in `logs\<utc>-<tip>\`. `service.log` and `trigger.log` record every
start, request and outcome.

A failed tip is not retried: the next push (a new tip) runs again, and a later successful run clears the failure.

A network step is retried before it fails the run (owner ruling, 2026-11-12). That covers every fetch and the push,
when a transient network or system failure stops them: a dropped connection, or a DNS thread that failed to start.
Each such step is tried up to 4 times in all, the first attempt and three retries, with a 30-second pause before each
retry (`Invoke-NetworkGit` in `common.ps1`). Each failed attempt is logged with git's output. A rejected push is an
outcome, not a failure, and is never retried (see below).

## Conflicts with the developer

The developer runs, before pushing:

```powershell
pwsh -File tools/cleanup/pull.ps1
```

It fetches the cleaned branch and, when it holds commits the branch lacks, merges them (`git merge --no-ff -X ours`),
so main's first-parent history stays the developer's and every conflicting line keeps the developer's side. Cleanup
output is regenerated, never hand-merged: a line the developer also changed is cleaned again by the next run. It
refuses to run over uncommitted changes, then prints the status.

If the developer pushes while a run is in progress, the run's push is rejected. The service then discards its commit,
resets to the new tip and runs again from the top: the cleanup is regenerated, never merged. It does this only when
the branch really moved; a rejection with the branch unmoved fails the run. After 10 consecutive rejections the
service stops and notifies (a count, not a time limit); the next push starts it again.

## The Discord tracker

`tools/discord-updates.ps1` skips commits whose subject starts with `Cleanup:` explicitly, by one switch at the top of
the script (`$CleanupCommits = 'skip'`). Setting it to `'post'` announces them like any other commit, as a short
"Cleanup" note. `tools/discord-updates.md` records the choice.

## Install, update, disable

```powershell
pwsh -File tools/cleanup/install.ps1            # install, or update after tools/cleanup changes
git config --unset cleanup.directory            # disable: the hook does nothing while this is unset
```

`install.ps1` verifies that the main checkout's `core.hooksPath` is its own absolute `.githooks`, clones `origin` into
`<dir>\repo` (or reuses the clone), points the clone's hooks at its own `.githooks`, writes `config.json`, copies
`common.ps1`, `notify.ps1`, `trigger.ps1` and `service.ps1` into `<dir>\bin`, and sets `cleanup.directory` and
`cleanup.branch` in the main checkout's git configuration. A change to any of those four scripts on `main` makes the
next run fail until `install.ps1` is run again, so a stale service never runs silently. To uninstall, unset
`cleanup.directory` and delete `<dir>`.

Inside the worker every process carries `SUPERMETROID_CLEANUP_WORKER=1`: the clone's own `reference-transaction` does
nothing, `post-commit` skips the agent time report and `pre-push` keeps its attribution guard but skips the Discord
reminder.
