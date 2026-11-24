"""Checks that every word and message the Godot project asks for by name is one the asset extractor writes.

The game's words are asked for from GameText by name, in code (`Text.Get("name")`) and by the scenes of things that
type a warning (`Message = "name"`). The extractor's writers give words with `Words("name", ...)`. A message shown
in a box is a picture named for one of the old game's messages. This reads the sources as text and lists every
name asked for that nothing writes.

    python tools/godot-rooms/check_words.py        (from the root of the working tree; prints "0 problems")
"""
import glob
import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
GODOT = os.path.join(ROOT, 'godot')
ENGINE = os.path.join(ROOT, 'csharp', 'src', 'SuperMetroid.AssetExtraction', 'Engine')
CORE = os.path.join(ROOT, 'csharp', 'src', 'SuperMetroid.Core')


def read(path: str) -> str:
    with open(path, encoding='utf-8-sig') as source:
        return source.read()


def snake(name: str) -> str:
    return re.sub(r'(?<=[a-z0-9])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])', '_', name).lower()


def members(path: str, enum: str) -> list[str]:
    """The members of an enum of the old game, in order."""
    body = re.search(r'enum ' + enum + r'\s*\{(.*?)\}', read(path), re.S)
    if body is None:
        raise SystemExit(f'{path} has no enum {enum}.')
    code = '\n'.join(line for line in body.group(1).split('\n') if not line.strip().startswith('///'))
    return re.findall(r'^\s*(\w+)\s*(?:=[^,]*)?,?\s*$', code, re.M)


def constants(path: str) -> list[str]:
    """The text of every string constant a Godot class of names declares."""
    return re.findall(r'(?:const string \w+|GameAction \w+ \{ get; \}) = (?:new\()?"([a-z0-9_]+)"', read(path))


def call_argument(text: str, start: int) -> str:
    """The text of a call's arguments, from just after its opening bracket to its matching closing one."""
    depth = 1
    at = start
    while depth > 0:
        if text[at] == '(':
            depth += 1
        elif text[at] == ')':
            depth -= 1
        at += 1
    return text[start:at - 1]


# What a name built in code from a list of the game's own names stands for, by what it begins with.
BUILT = {
    'equipment_': [name for name in constants(os.path.join(GODOT, 'game', 'data', 'Equipment.cs')) if name != 'hyper_beam'],
    'options_toggle_': constants(os.path.join(GODOT, 'game', 'data', 'Toggles.cs')),
    'options_action_': constants(os.path.join(GODOT, 'engine', 'input', 'GameAction.cs')) + constants(os.path.join(GODOT, 'game', 'data', 'Actions.cs')),
}

problems: list[str] = []
asked_words: dict[str, str] = {}
asked_messages: dict[str, str] = {}

for path in glob.glob(os.path.join(GODOT, '**', '*.cs'), recursive=True):
    if os.sep + 'tests' + os.sep in path:
        continue
    text = read(path)
    where = os.path.relpath(path, ROOT)
    for found in re.finditer(r'\bText\.Get\(', text):
        # What is named inside a call within the arguments is that call's business: a name read from a data file.
        argument = call_argument(text, found.end())
        while re.search(r'\w\([^()]*\)', argument):
            argument = re.sub(r'\w\([^()]*\)', '', argument)
        for built in re.findall(r'\$"([a-z0-9_]+)\{', argument):
            if built not in BUILT:
                problems.append(f'{where} builds the name of words from "{built}", which this check does not know how to list')
                continue
            for name in BUILT[built]:
                asked_words[built + name] = where
        for name in re.findall(r'(?<![$\w])"([a-z0-9_]+)"', argument):
            asked_words[name] = where

for path in glob.glob(os.path.join(GODOT, 'game', '**', '*.tscn'), recursive=True):
    where = os.path.relpath(path, ROOT)
    text = read(path)
    for name in re.findall(r'^Message = "([a-z0-9_]+)"', text, re.M):
        asked_words[name] = where
    for name in re.findall(r'^(?:DoneMessage|AsksFirst|SaveQuestion|SavedMessage) = "([a-z0-9_]+)"', text, re.M):
        asked_messages[name] = where

written_words: set[str] = set()
for path in glob.glob(os.path.join(ENGINE, '*.cs')):
    text = read(path)
    written_words.update(re.findall(r'\bWords\("([a-z0-9_{}]+)"', text))
    # Names the writers build from the old game's own lists.
    for prefix, kind in re.findall(r'Words\("([a-z0-9_]+)" \+ Named\((\w+)(?:\.\w+)?\)', text):
        declared = re.search(r'\b(\w+) ' + kind + r'\b', text)
        enum = {'id': 'IntroNarrationPageId', 'text': 'EndingTypedText'}.get(kind)
        if enum is None:
            problems.append(f'{os.path.relpath(path, ROOT)} names words "{prefix}" + Named({kind}), which this check does not know how to list')
            continue
        found = glob.glob(os.path.join(CORE, '**', enum + '.cs'), recursive=True)
        if len(found) != 1:
            raise SystemExit(f'The old game has {len(found)} files named {enum}.cs.')
        written_words.update(prefix + snake(member) for member in members(found[0], enum))
    for prefix, listed in re.findall(r'Words\("([a-z0-9_]+)" \+ (\w+)', text):
        if listed == 'Named':
            continue
        # The names come from a list of pairs written just above, each a name and what it shows.
        block = text[:text.index(f'Words("{prefix}" + {listed}')]
        block = block[block.rindex('[') + 1:]
        written_words.update(prefix + name for name in re.findall(r'\("([a-z0-9_]+)", "', block))
    for name in re.findall(r'\(EscapeTypewriterMessageId\.\w+, "([a-z0-9_]+)"\)', text):
        written_words.add(name)
    for name in re.findall(r'\(EndingTextSequence\.\w+, "([a-z0-9_]+)"\)', text):
        written_words.add(name)

message_ids = glob.glob(os.path.join(CORE, '**', 'GameplayMessageId.cs'), recursive=True)
if len(message_ids) != 1:
    raise SystemExit(f'The old game has {len(message_ids)} files named GameplayMessageId.cs.')
written_messages = {snake(member) for member in members(message_ids[0], 'GameplayMessageId')}

for name, where in sorted(asked_words.items()):
    if name not in written_words:
        problems.append(f'{where} asks for the words "{name}", which no writer gives')
for name, where in sorted(asked_messages.items()):
    if name not in written_messages:
        problems.append(f'{where} asks for the message "{name}", which is none of the old game\'s messages')

for problem in problems:
    print(problem)
print(f'{len(problems)} problems; {len(asked_words)} names of words and {len(asked_messages)} messages asked for, {len(written_words)} names of words written')
sys.exit(1 if problems else 0)
