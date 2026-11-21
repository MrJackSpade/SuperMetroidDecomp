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
| `run.ps1` | the clone's tree | One run: restore, the tool builds, the fix and prune steps (code, dead code, stale reviews) and the inventories in one process, one build, commit, push. |
| `CodeFixRunner.cs` | the clone's tree | The code fixes, as a Roslyn workspace runner (below), and, with `--cleanup`, the whole fix and prune loop and the inventories on one load of the solution. |

Git has no `post-push` hook; a successful push updates the remote-tracking ref through a ref transaction, so
`reference-transaction` is where a push is seen after it has landed. A fetch that brings new commits triggers it too,
which also picks up pushes made from elsewhere. The hook costs a shell start per ref transaction and nothing more
until that ref moves.

`<dir>` is `G:\git\SuperMetroidDecomp-cleanup`. It holds `repo\` (a full clone of `origin`, never a worktree of the
developer's checkout, with its own `bin`/`obj` and its own `temp\`), `config.json`, `STATUS`, `state.json`,
`requests.log`, `service.log`, `trigger.log` and `logs\`.

### The run

1. **Restore** the solution, **build the repository's analyzer and code fixes in Debug**
   (`csharp/src/SuperMetroid.EnsureAnalyzer.CodeFixes`) and **the cleanup runner** (`tools/cleanup/CodeFixRunner.cs`, with
   the reachability and architecture audits it runs in its own process) through `tools/build.ps1`. The code fixes load
   the repository's analyzer and fixers from the projects' Debug references, so they must be built first.
2. **One process does the fix and prune loop and the inventories:**
   `dotnet run --no-build tools/cleanup/CodeFixRunner.cs -- --cleanup csharp/SuperMetroid.Full.slnx <run> <run>\cleanup-outcome.json <rules> <prune categories> csharp/src csharp/test-support/VerificationAccess.Core 6`.
   It loads the solution once (below, "One load") and runs every step on it:
   - **Fix and prune, taking turns, until one of each in a row changes nothing:**
     - **Code fixes** of the rules `run.ps1` reads from `csharp/.editorconfig`: the `none` lines directly under the
       comment that names this cleanup (CA1823, CS0162, CS0168, CS0169, CS0219, CS0414, IDE0004, IDE0005, IDE0032,
       IDE0044, IDE0051, IDE0052, IDE0059, IDE0076, IDE0077, IDE0079, IDE0330, SME6360, SME6391, SME6400, SME6401). A
       rule there that `run.ps1` has no commit-message words for, or words for a rule not there, fails the run. Each
       step's summary is kept as `<run>\code-fixes-<step>.json`.
     - **Dead code:** the reachability audit's `--prune-keeping-verification-used` of the categories
       `unreachable-member,unreachable-type,unreferenced-field,unreferenced-enum-member,unreferenced-constant` under
       `csharp/src`, into `csharp/test-support/VerificationAccess.Core`, retiring fields to
       `<run>\retired-fields-<step>.txt`. It moves the dead top-level types tests still use (check-only helper code)
       whole into test-support and deletes every other dead declaration, a dead member of a production type that only
       tests use among them: nothing in production exists for checks alone, so the checks reading such a member fail to
       build until they drive real game code (owner ruling, #2599). The next code-fix step removes the usings it copies
       into moved files. It prints how long each of its phases took. What is dead is read from code that compiles (a
       reference that does not bind reaches nothing, so live code would read as dead): when a project does not compile,
       as a fix or an earlier prune of the run leaves it where the developer has a choice to make, the prune prints
       `Pruned nothing: ...` and changes nothing, and the next run prunes once the tree builds. A prune that throws is a
       defect of the prune and fails the run with its exception.
     - **Stale reviews:** the architecture audit's `--prune-stale-reviews`. It deletes every `[ArchitectureReview]`
       whose finding its type no longer has (a prune of dead code can remove a finding), through the attribute's own
       syntax reference, and keeps every other byte of the file. It validates every edit before writing any and writes
       nothing if one fails. The next code-fix step removes a using the attribute alone needed.
     - A step that returns the tree to an earlier state (the steps undo each other), or a run that needs more than 6
       rounds of the three steps, stops the loop: the run keeps the tree it has and records a problem. A failed stale
       review prune (which writes nothing) is recorded as a problem and the loop goes on.
   - **Inventories:** the architecture audit's `--write`. A failure is recorded as a problem and the run goes on.

   The process prints each step to the log, with how long it took and how many files it changed, and writes what the
   steps did (the counts, paths, summaries, retired fields, removed reviews and problems the commit message is made of)
   to `cleanup-outcome.json`. A fix is always applied (#2599), so there are no unfixed sites to write. A failure to
   load the solution or the analyzers, a bad command line, or a fixer that offers no fix or throws (a defect of that
   fixer) fails it, with the full exception on standard error, as each step's own process failed the run before.
3. **Nothing changed:** the run ends quietly. A change outside `csharp/` and `docs/` fails the run.
4. **Build once:** `pwsh -File tools/build.ps1 csharp/SuperMetroid.Full.slnx`. When the loop changed the code, the runner starts
   this build as soon as the loop ends and writes the inventories while it runs, since the inventories write only
   `docs/`, which no build reads; when only the inventories changed, `run.ps1` builds after them. The owner's ruling (2026-11-15): the
   cleanup would rather commit broken code than fail. A failed build is recorded as a problem and the tree is
   committed and pushed anyway, as `Cleanup (does not build): ...`; its full output is kept as
   `logs\<run>\build.log`. Every problem is listed in the commit body and the result, and a pushed run with any of them
   raises a notification. A build failure is never fixed by judgment. The build changes no file;
   a tree it changed fails the run.
5. **Commit** as `Cleanup: <rule, what, count; ...>` (stale reviews as `stale architecture reviews removed <count>`),
   with the files, the prune's own summary and every removed review in the body, under
   the owner's identity from the main checkout and with no assistant attribution (the clone runs the repository's
   own `commit-msg` and `pre-push` guards), and **push** with a plain fast-forward push. A push a developer push beat
   is answered by a merge, built before it is pushed ("Conflicts with the developer", below).

### One load

The run used to start a process per step, and each loaded and compiled the whole solution again: the code fixes, both
halves of every dead-code prune, every stale-review prune and the inventories. A run of seven steps loaded it more than
ten times, which, with the analysis below, made a run take hours. The runner now loads the solution once, in Debug,
and each step is handed the solution as the steps before it left it. The load reads every project's design-time build in
one batch (`MSBuildProjectLoader.LoadSolutionInfoAsync`) rather than opening the projects one after another, and places
the projects in the order opening them one at a time gives (each listed project, then the projects its evaluated
`ProjectReference` items reach that are not placed yet, depth first), since the audits take the first project that
compiles a shared file:

- The code fixes return the solution they wrote.
- The prunes write their files as before, and the runner reads back what git reports changed since the solution was last
  brought up to date (`git diff --name-status --no-renames` between the working tree's trees, which the loop records
  anyway). A changed file's text is read where it differs. When files are added or deleted, or a project file changes,
  every project's source items are evaluated again (`dotnet msbuild -getItem:Compile`, all projects at once), and each
  project gets exactly those files, in that order, followed by the generated files it was loaded with. This is the shape
  a fresh load has, which the runner confirms when it loads: a project whose loaded documents are not its evaluated
  source items in order, followed by generated ones, fails the load.
- The audits take the solution through `SolutionLoader.CompileAsync`, which compiles every project on all processors and
  fails on a compile error, as their own loads did. A compilation no step and no reference changed is the same object
  from step to step, and its errors are found once. Their command lines open the solution from the files, as before,
  through the same code.
- The audits' walks bind every tree's code first, on all processors (`SemanticModels`), and then read the bound answers
  in their own, unchanged order, so what they record is what the serial walk recorded. A definition catalog's names are
  bound through several models at once, since one model binds each of them by binding its whole initializer again.
  The prune collects the verification projects' references while it analyzes reachability, since both only read the
  same compilations. The audits run with the server garbage collector, without which their parallel binding waits on
  collections.

The builds use a compiler server of the cleanup's own (`SharedCompilationId=supermetroid-cleanup`), named apart from the
developer's, so it never serves the developer's builds and runs at the cleanup's priority. `ShaderBuild.targets` in
`SuperMetroid.Rendering.Direct3D11` compiles the shaders only when a shader source changes, so neither a build nor the
load runs the shader compiler for an unchanged project.

The service gives every tool its own temporary folder, no shared compiler server (`UseSharedCompilation=false`), no
reused MSBuild nodes and no MSBuild server, so nothing the cleanup starts serves the developer's builds or runs at the
cleanup's Idle priority for them; `run.ps1` gives its builds the cleanup's own compiler server (above), which the
developer's builds cannot reach. The NuGet package cache is shared read-mostly, as the brief allows. Git credential
prompts are disabled (`GCM_INTERACTIVE=never`), so a missing credential fails the push instead of opening a dialog.

### The code fixes

`CodeFixRunner.cs` loads every project into one workspace (One load, above), runs each project's own analyzers for the requested
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
projects' Debug references, which `run.ps1` builds first. A fixer resolves every finding without changing behaviour.

**A fix is always applied, breaking the code if it must; nothing is left unfixed** (the owner's rule, 2026-11-15 and
#2599). When a finding's fix needs a choice the fixer cannot make, the fixer applies it all the same and the code it
leaves does not compile, or fails a build rule, at that site: the build fails there, the cleanup commits and pushes it
as `Cleanup (does not build)`, and the developer makes the choice. No fixer declines a finding, no site is skipped or
recorded as unfixed, and the runner catches nothing a fixer throws. A fixer that offers no fix, has no fix-all,
produces no usable change or throws is a defect of that fixer, not a site to leave: the run fails with the full
exception, and the fixer is fixed. What each fixer does where a choice was needed:

- SME6400 keeps in the file the type it is named after (its name compared without case) and moves the others, the
  file's first type when the reported one is the file's. A moved type takes the `#define`, `#nullable` and `#pragma
  warning` directives before it and the `#if` branches it lies in; lines inside a verbatim or raw string are not
  re-indented. When the file it belongs in exists, it goes into that file beside its code, deleting nothing. Two types
  that both belong in the file (`Pair` and `Pair<T>` in `Pair.cs`) get an `#error` naming both. A file-local type
  moves, and its references fail the build.
- SME6401 renames a file whose name differs only in case (`objectcharacterplacement.cs`, which a file system that
  ignores case holds as the same file): the runner deletes it before it creates the new name, and the loop takes the
  old name out of git's index. When another file has the name, the type goes into it, as SME6400's does.
- SME6360 merges parts with directives (a part's body moves whole), a primary constructor on another part, type
  parameters with differing attributes and constraints only one part states, whatever the braces' layout.
- The dead-code fixes remove the member, variable or statement at every finding. A use they cannot rewrite (one that
  reads the variable, a `nameof` or cref, a write through a receiver with effects) is left as written and fails the
  build. An initializer with an effect is kept as `_ = value;` in a constructor the type is given when it has none,
  or, in a type with a primary constructor, at the start of the type's body, which cannot hold it. A parameter left
  unread goes with its arguments even where that needs a choice (an override's then fails the build), and a member
  left reading no instance data is reported by CA1822.

A dead store or initializer whose value could have an effect keeps its evaluation as `_ = value;`.

A dead-code fix (CS0414, IDE0052, IDE0051, CA1823, CS0169, CS0219) takes out with the code it removes every parameter
only that code read (`UnreadParameters`), since IDE0060, an unused parameter, is a build error: the cleanup of e4ce9514eb
(2026-11-15) removed the write-only field `_room` of eight constructors and left each one's `room` parameter unread. The
parameter goes with its `param` documentation and the argument every call passes it, a target-typed `new` among them;
an argument that read a parameter of its caller is removed code too, so that parameter goes when nothing else reads
it. Reading an automatic property is free of effects, like reading a field, so such an argument goes as well. A
parameter whose removal needs a choice goes all the same, with every argument passed to it (#2599): one passed an
argument that could have an effect, a `ref`, `out`, `in` or `params` parameter, one of a method something else decides
the signature of (virtual, abstract, an override, an interface's, which then no longer matches and fails the build),
one whose removal clashes with an overload (the build fails on the duplicate), a record's positional parameter, and one
`paramref` still names (the build fails on the reference). A use of the method that is no call (a method group, a cref
with parameters) stays as written and fails the build there. An instance member
that read its instance only in the removed code reads none afterwards, and CA1822 (a build error) asks for it to be
static. `IDE0052.unread-parameter` holds the constructor of the e4ce9514eb failure, a caller passing an automatic
property through a target-typed `new`, a factory whose own parameter only fed the removed argument and a caller of it
with a named argument, and a field whose removal leaves a caller reading no instance data, which the build then
reports (CA1822).

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
reporting, not its analysis. A project that compiled when the run started and has compiler errors after a pass is
recorded as a problem, naming them; its findings are fixed all the same and the run goes on.

Roslyn skips an analyzer only when every rule it reports is off. A repository analyzer that reported a moved rule
beside one that stays an error would still run in every build, so a moved rule gets an analyzer of its own: SME6391 is
`SpanAdapterAnalyzer`, split from `AdapterChainAnalyzer` (SME6390), and SME6360 is `PartialSplitAnalyzer`, split from
`PartialTypeAnalyzer` (SME6361). Roslyn's own analyzers cannot be split: `CSharpRemoveUnusedValuesDiagnosticAnalyzer`
reports IDE0059 beside IDE0058 (`silent`), so it still runs in the build.

**IDE0079 (unnecessary suppression) has no command-line analyzer** (only the IDE's suppression analysis reports it),
so the runner finds its sites itself. When IDE0079 is requested it runs, beside the requested rules' analyzers, every
analyzer that can show one of the project's suppressions to be needed, with the whole group enabled in its scope and
suppressed diagnostics reported (`CompilationWithAnalyzers`, `reportSuppressedDiagnostics: true`). A suppression is
needed only by a diagnostic of the id it names, so the analyzers that report an id one of the project's suppressions
names (a pragma code, or an attribute's check id) are the ones that can, with every diagnostic suppressor, which
decides what is suppressed; the compiler reports its own ids anyway. An attribute whose check id is not a string literal
makes every analyzer run. The others' diagnostics are neither findings nor evidence, so leaving them out changes nothing
the run decides; running every analyzer of every project, when the repository has almost no suppressions, made the
analysis cost a whole analyzer build done one analyzer at a time. A `[SuppressMessage]` or `[UnconditionalSuppressMessage]` (on a member, or at
assembly level with a target) is needed when it is the attribute that suppressed a diagnostic; a
`#pragma warning disable` code is needed when a diagnostic of that id, suppressed by no attribute, lies between it
and its `restore`. Every other suppression of a rule its file enables is unnecessary, and the repository's
`UnnecessarySuppressionCodeFixProvider` removes it: a pragma loses the unused codes (and its line, and the matching
restore's, when none is left), an attribute leaves its list (and the list's line when it was alone). A suppression
without codes, of an unknown rule, of a rule its file does not enable, or in `obj/` is left alone. The analysis sees
one configuration (Debug), so a suppression whose range or member holds code another configuration compiles (an
inactive `#if` branch) is kept.

It is a runner rather than `dotnet format` because of shared files. A file compiled into several projects
(`csharp/test-support` through `.projitems`, or a `<Compile Link>`) is decided from every project that compiles it and
reports the rule there (one without the rule's analyzer, or outside the rule's scope, has no say): a finding is code
none of them needs, so a rule's findings in the file are those every one of them reports at the same place. A finding
only some of them report is code another of them needs, and no finding. Where their unused usings differ, as where
DesktopVerification, IntegrationVerification and RenderVerification have global usings of `SuperMetroid.Core.Audio`,
`.Compatibility` and `.Spatial` that Verification lacks, the usings all of them report are removed and the others are
usings a project needs. `dotnet format` judges a shared file from one project and would remove usings another project
needs. A project with compiler errors decides like any other, and its fix is applied all the same (#2607): the build
then fails there, and the cleanup commits it as `Cleanup (does not build)`. No finding is held back and no commit says
that any was left: until #2607, findings in a shared file that not every project reported, and every finding in a file
of a project that did not compile, were left in place and counted in the commit body ("Left in files compiled by several
projects that do not all report them, or by a project that does not compile: IDE0005 240").

The runner repeats its analysis until a pass finds nothing (one rule per file per pass, since other spans go stale
once a file changes). A file that returns to an earlier text, or more than 20 passes, stops the loop: the run keeps
the tree it has and records a problem.

A pass analyzes its projects in parallel, each on its own compilation, and takes their results and log lines in name
order. The requested rules' analyzers run concurrently within a project: each of them declares concurrent execution and
keeps no state between callbacks. An analyzer that runs only as evidence for a suppression runs serially with the rest,
as every analyzer once had to: run concurrently with the whole analyzer set over Core, the repository analyzers'
suppressed findings were intermittently lost, and IDE0079 took the suppressions they needed for unused. An analyzer
that throws is logged with its AD0001. The pass then computes every file's fix at once, on all processors, from the
solution it analyzed, and applies them one by one in path order as before: since a pass fixes one rule per file and a
fix that would change a file another fix of the pass changed waits for the next pass, the file a fix was computed for
holds the same text when it is applied. Computing each fix from the solution with every earlier fix of the pass applied
made each fixer bind a new compilation of its project, file after file. The compiler-error check of a fix that changes
several files runs while the pass goes on, and its problems are recorded in their place when the pass ends.

A fix may change several files, such as IDE0032 deleting a field in one part of a partial type and rewriting its
property in another. The runner applies it unless another fix in the same pass touched one of those files, when it
waits for the next pass, since its spans would be stale; whether the projects compiling its files compile does not
matter. Every project compiling one of its files that does not compile afterwards is recorded as a problem naming the
fix, and the fix stays applied. The cycle and pass checks cover every changed file, and the summary lists them all.

A fix may also add, rename or delete files: SME6400 moves a type into a new file, SME6401 renames a file after its type,
and SME6360 merges a type's hand-written parts into one and deletes the files it empties. The runner adds a new file to
every project that compiles the file its finding is in, creates it with that file's byte-order mark (it must not exist
yet, unless the fix removes it under a name that differs only in case), deletes a removed file before it writes any,
and keeps the explicit source lists in step: in a project file or an imported
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
exist, and builds the fixed copy with `tools/build.ps1`. A fixture whose fix breaks the code, as a fix must where a choice
was needed, holds `expected-build-errors.txt` beside `input/` and `expected/`: one line per text (an error code, say)
the build of the fixed copy must report, and that build must fail. A failed fixture keeps its copy and prints where; the
script exits nonzero when any fixture fails.

The fixtures of #2599 hold the shapes the fixers used to decline, applied: `SME6400.file-named-type` the three sites of
cda71cca6b (a file named after its second type, `FrontendFloorSheetComposer.cs` and `GameOverBabyPaletteNames.cs`, and
after its third, `RoomPaletteFxPrograms.cs`, on which the fixer threw); `SME6401.case-only` the fourth
(`objectcharacterplacement.cs`); `SME6401.taken-name` a file whose name another file has, whose type joins that file;
`SME6400.directives` a type moved with its directives, a raw string in a namespace block, a type that joins an existing
file and a file of top-level statements; `SME6400.breaking` a file-local type and two types that both belong in one file;
`SME6360.refusals` the merges that needed a choice; `IDE0052.breaking` initializers kept in an added constructor, in a block body given to an expression-bodied one and in a primary
constructor's type body, a write through a receiver with effects left as written, an assignment used with the field's
type, and a virtual method's parameter taken out with its signature; `IDE0052.unread-parameter` (above) a member left reading no
instance data.

The fixtures of #2607 hold the two kinds of finding the runner used to leave and count: `IDE0005.shared` a file two
projects compile, one of which imports a namespace globally (`Fixture.csproj` and `Other/Other.csproj`, as
DesktopVerification and Verification share test-support): the using neither needs goes, and the using only the other
needs stays, as no finding; `IDE0005.does-not-compile` a project with a compiler error whose unnecessary using goes all
the same, its build failing on the error (CS0029). A fixture may hold several projects; every project's `bin/` and
`obj/` are left out of the comparison.

`tools/cleanup/prune-fixtures/<name>/` confirms the dead-code prune the same way: `input/` is a small repository whose
`csharp/Fixture.slnx` lists projects named like the shipped, host and verification projects (the reachability audit
knows them by name), with test-support shared through `.projitems`, and `expected/` is every file of it after the prune.
A fixture may also hold `expected-output.txt`, text the prune's output must hold, such as its summary of what it moved,
and `expected-build-errors.txt`, as a cleanup fixture does: the prune applies every removal and move it selects, and a
fixture whose pruned copy must not build names the errors its build must fail with.

```powershell
pwsh -File tools/cleanup/check-prune-fixtures.ps1                         # every prune fixture
pwsh -File tools/cleanup/check-prune-fixtures.ps1 -Name check-only-code   # only the named ones
```

It builds `SuperMetroid.ReachabilityAudit` once in Release (which builds the repository analyzer in Release too), then
for each fixture copies `input/` into a new temporary directory with a `csharp/Directory.Build.props` that imports
`csharp/Directory.Build.props`, names the repository's `csharp/test-support` (`$(RepositoryTestSupport)`, for the host's
console error policy) and adds `SuperMetroid.EnsureAnalyzer` to every project, and with the repository's
`csharp/.editorconfig` beside it, so every rule has the severity and scope the repository gives it (the SME rules in
`src/SuperMetroid.Core` and `src/SuperMetroid.AssetExtraction` only). The module map is not added: its rules
(SME6402-SME6405) place the repository's own modules, which a fixture does not have, and a prune changes no namespace or
path they read. The check restores the copy, runs the prune on it with the arguments `run.ps1` uses and
`csharp/test-support/VerificationAccess.Core` as the destination, compares every file with `expected/` as above, and
builds the pruned copy, so an analyzer finding fails the fixture as it fails the cleanup's build; with
`expected-output.txt` it also requires the prune's output to hold that text, and with `expected-build-errors.txt` it
requires the build to fail with each error that file names.

What the prune does with dead code tests still use follows the owner's rulings of #2599: nothing in production exists
for checks alone, and nothing is reached through reflection. A dead top-level type that verification uses, directly or
through other such types (check-only helper code), moves whole into test-support (`SymbolRelocator` in
`csharp/src/SuperMetroid.ReachabilityAudit/Removal`, chosen by `VerificationReferences.SelectRelocatedTypes` in
`Analysis`): each declaration, every part of a partial type, goes to the same path under the destination, less the
project directory, with its namespace, its file's using directives and the source project's global usings it names,
merging into a file an earlier run wrote there, and a source file left without a type is deleted. Its name and namespace
do not change, so every check naming it binds to it as before. Every other dead declaration is removed, a dead member of a
production type that only checks use among them, and the checks and moved code reading it fail to build until they are
rewritten to drive real game code. No production member is widened, renamed or reached through a generated
private-access member for moved code, and nothing is refused, kept in place or reported as `MANUAL`: moved code that
reaches what test-support cannot see fails to build there. Documentation staying in a shipped assembly that cites a moved
type becomes text, a labelled cref its label and an unlabelled one the name as code, which SME6454 rejects in Core where
the sentence must be reworded; moved documentation citing a member test-support cannot see (private, or private
protected) becomes text the same way. A staying type compiled by a project that cannot see the destination no longer
implements a moved interface; a type every project compiling it sees the destination from keeps it. A key the relocation
is given that names no top-level type of repository source fails the run.

Until #2599 the prune also moved check-only members of production types, into per-owner `<Owner>Constants` and
`<Owner>Access` classes in test-support; moved code reached the staying private members it used through generated
`<Owner>PrivateAccess` members (#2570) or, before that, through members made internal under promotion rules, and every
move the prune could not make whole stayed in place with a `MANUAL` line. The fixtures of those shapes went with them.

- `check-only-code`: `ListWindows.WideWidth`, a member of a production type only a check uses, is removed, and the check
  fails to build (CS0117); `ListSamples`, a top-level type only checks use, moves whole into test-support, its emptied
  file is deleted, and the shipped documentation citing it with a labelled cref keeps the label.
- `moved-interface-implementers`: an interface only checks use moves into test-support, and the test-support types
  implementing or extending it keep it in their base lists. Only a type some project that cannot see the destination
  compiles loses it. The 2026-11-15 cleanup of e4ce9514eb took `ISnesMutableMemory` off test-support's
  `ISnesMemoryBus` and the verification doubles, and every check reading work memory through them failed (CS1503,
  CS1061).
- `shipped-unlabelled-cref`: documentation staying in a shipped assembly cites a removed declaration with an unlabelled
  cref. The cref becomes the name as code text, and SME6454 fails the build there, where the sentence must be reworded
  (`<c>LastMark</c>` in `QuadraticSpeedCurvePlace`, the 2026-11-15 cleanup of e4ce9514eb). The declaration was kept in
  place with a `MANUAL` line before.

`paired-operators` holds the shape of the failure on every run from e40e73afe5 (2026-11-13) to 86b2e34794: only checks
used `PowerBombRadius.operator >=` and nothing used `operator <=`, so the prune deleted `<=` alone and its reload failed
on CS0216. The compiler requires some declarations beside others, and the reachability audit records that on the
declarations themselves (`RequiredDeclarations` in `csharp/src/SuperMetroid.ReachabilityAudit/Analysis`), not on the
code that uses them, which for code only checks use is code the audit does not walk: each operator of a pair (`==`/`!=`,
`<`/`>`, `<=`/`>=`, `true`/`false`) reaches its partner, a checked operator its unchecked form, and an override the
member it overrides. A pair is kept when either is used and removed only together. The fixture removes `<=` together with
the `>=` only checks use, and the check fails to build (CS0019); it keeps `>` beside the `<` production uses, removes an
unused `true`/`false` pair together, keeps a `GetHashCode` nothing calls beside the `Equals` production calls, and keeps
an abstract property production reads only through its override, which the prune deleted before (CS0115). An `Equals`
override and `GetHashCode` stay with their type, as every override of a framework member does, and an interface member
needs no rule: an explicit implementation is reached only through it, and an implicit one does not need it.

The record-equality fixtures hold the shape of the 2026-11-18 failure at a0a6e3b3a5 (#2434), where the prune removed
`Row` from the three `GameOptionsRow` cases as unread: the cases are keys of a dictionary typed by their abstract base,
nothing names `Row`, and with it gone every row of a page compared equal and reading the options layout threw on a
duplicate key, while the pruned tree compiled. A record's synthesized equality, hashing and text read every value it
declares and have no source to walk, so the audit counts those values as read wherever a value is compared, hashed or
formatted (`ValueEqualityReads` in `csharp/src/SuperMetroid.ReachabilityAudit/Analysis`), through every type that can
stand behind the compared one. Each fixture keeps every file as written, and each failed on the tool before:

- `derived-record-key`: a closed hierarchy's base record keys a dictionary, and its nested cases differ only in the row
  each names (the a0a6e3b3a5 shape; the prune emptied the cases' parameter lists and the copy still built).
- `deep-record-key`: the value telling two keys apart is held five records deep (a case, its row's hierarchy, a case
  there, its button's hierarchy, a numbered button); the audit followed records four deep.
- `derived-record-elsewhere`: the derived record is declared in another project than the base record keying the
  dictionary; the audit looked only in the base record's own assembly.
- `interface-record-key`: the key is an interface the records implement.
- `held-record-key`: records held in the framework's value holders, whose equality compares what they hold: a tuple
  key, tuple equality (`==` on tuples, which has no symbol of its own), a nullable record struct in a set and a
  key-value pair `Distinct` compares.
- `object-record-equality`: `object.Equals(a, b)` on two records, and `Equals` called on a record held by an
  interface, both of which dispatch to the record's synthesized equality.
- `record-concatenation`: a record concatenated with text, which formats it.

`positional-base-argument` holds the shape of the failure at 0ea41f339b (#2599): the prune retired the dead property of
`KraidColours.BandShade(PictureShade Shade) : BodyShown(Shade)` by deleting its positional parameter, which the base call
reads as a constructor parameter, and the tree no longer built (CS0103). When every positional property of the record is
dead and only the base call reads parameters, the record loses its parameter list and gets a constructor taking the
parameters read, under constructor-parameter names, with their documentation, and passing them to its base: `BandShade`
becomes `record BandShade : BodyShown` with `BandShade(Tint shade) : base(shade)`. Otherwise the parameter goes anyway
and the build fails at each read, where moving the code that reads it into a constructor is the developer's choice: the
fixture's `Glow(Tint Level)`, whose property initializer reads `Level`, loses it, and the build fails there (CS0103). An
argument passed to a removed parameter whose evaluation calls code or writes state stays as written, so the call fails
to compile there instead of the effect silently going with it.

A value typed `object` names no type, so what is compared through it is not followed. A `with` expression copies a
record without comparing it, and a pattern names each value it tests (a property pattern by name, a positional one
through the synthesized `Deconstruct`), so neither needs a rule of its own.

## Seeing status

```powershell
pwsh -File tools/cleanup/status.ps1
```

prints `STATUS`: `idle`, `running`, `failed` or `stopped`, the tip, what happened, and the run's log. When the state is
`failed`, it also prints how many runs in a row have failed (from `service.log`, since the last run that pushed or found
nothing to clean), when the first of them failed, and the last clean tip: a failure that persists over several tips is a
defect in the cleanup's own tools on `main`, which every new tip runs into again (#1882), and is fixed there. A failed run, a
stop and a trigger that cannot start the service also raise a Windows toast (`notify.ps1`, through Windows
PowerShell), which never takes focus. Every run's full output is `logs\<utc>-<tip>.log`, with its code-fix summaries,
retired fields, build logs and commit message in `logs\<utc>-<tip>\`. `service.log` and `trigger.log` record every
start, request and outcome.

A failed tip is not retried: the next push (a new tip) runs again, and a later successful run clears the failure.

A network step is retried before it fails the run (owner ruling, 2026-11-12). That covers every fetch and the push,
when a transient network or system failure stops them: a dropped connection, or a DNS thread that failed to start.
Each such step is tried up to 4 times in all, the first attempt and three retries, with a 30-second pause before each
retry (`Invoke-NetworkGit` in `common.ps1`). Each failed attempt is logged with git's output. A rejected push is an
outcome, not a failure; it is answered by a merge, not a retry (see below).

## Conflicts with the developer

The developer runs, before pushing:

```powershell
pwsh -File tools/cleanup/pull.ps1
```

It fetches the cleaned branch and, when it holds commits the branch lacks, merges them (`git merge --no-ff -X ours`),
so main's first-parent history stays the developer's and every conflicting line keeps the developer's side. Cleanup
output is regenerated, never hand-merged: a line the developer also changed is cleaned again by the next run. It
refuses to run over uncommitted changes, then prints the status.

If the developer pushes while a run is in progress, the run's push is rejected. The run never discards its work (owner,
2026-11-15): a run takes long enough that the developer usually pushes during it, and discarding meant no cleanup ever
landed. `run.ps1` fetches the new tip and merges its commit onto it (`git merge --no-ff -X ours`, the new tip as first
parent, so main's first-parent history stays the developer's and every conflicting line keeps the developer's side; a
file one side deleted and the other changed keeps the developer's version, or stays deleted), restores and builds the
merged tree, and when it builds commits the merge as "Cleanup: ... (merged onto <tip>)" and pushes it, repeating while
the developer keeps pushing.

The cleanup of an older tip can break a newer one even where git sees no conflict: the merge of e4171830fc, the cleanup
of e4ce9514eb, onto 4813017a1d (2026-11-15) kept the developer's new calls of members the prune had moved into
test-support (`RotationAngle.FromRaw`, `BoyonCurvePlace.AtEntry`) and new reads of fields the code fixes had removed as
unread (`EnemyCollision._room`), and did not build. So when the merged tree does not build, the run aborts that merge,
starts an `-s ours` merge of its commit onto the new tip (the tree stays the developer's, the earlier commit stays in the
history as the second parent), builds the new tip's own tools and runs the whole fix and prune loop, the inventories and
the build again on it, as a run of that tip would, and commits the result as the merge, "Cleanup: ... (merged onto <tip>,
cleaned again)", with the body naming the merge that did not build. When the new tip needs nothing, the run ends with
nothing to push. When even that result does not build, it is committed and pushed anyway as `Cleanup (does not build)`,
as the first pass's would be: the owner would rather the cleanup commit broken code, which the next pull shows and the
developer fixes, than lose its work. Each merge's build logs, outcome and message are kept in `logs\<run>\merge-<n>\`. A
rejection with the branch unmoved fails the run.

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
`cleanup.branch` in the main checkout's git configuration. A change to any of those four scripts on `main` installs
itself: before each run the service copies the tree's version of every changed one into `<dir>in` and logs it, and
the copies take effect from the next service start. To uninstall, unset
`cleanup.directory` and delete `<dir>`.

Inside the worker every process carries `SUPERMETROID_CLEANUP_WORKER=1`: the clone's own `reference-transaction` does
nothing, `post-commit` skips the agent time report and `pre-push` keeps its attribution guard but skips the Discord
reminder.
