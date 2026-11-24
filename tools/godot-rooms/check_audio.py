"""Checks that every sound and piece of music the Godot project asks for by name is one the asset extractor writes.

The audio writer writes one sound file for each of the old game's sounds, named for the library it is in and its
name there ("samus_", "enemy_" or "room_" and the old name in lower case with underscores), one file of nothing for each of its requests that only stop sounds ("stop" after the library), and
one music file for each of the old game's tracks, named likewise. A set of music a room's version or the game's code
names must be one of the old game's sets, named likewise. This reads the sources as text: the Godot game's
code and scenes, the room generator's rules and the extractor's writers, and lists every name asked for that is
none of those.

    python tools/godot-rooms/check_audio.py        (from the root of the working tree; prints "0 problems")
"""
import glob
import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
GODOT = os.path.join(ROOT, 'godot')
ENGINE = os.path.join(ROOT, 'csharp', 'src', 'SuperMetroid.AssetExtraction', 'Engine')
AUDIO = os.path.join(ROOT, 'csharp', 'src', 'SuperMetroid.Core', 'Audio')


def read(path: str) -> str:
    with open(path, encoding='utf-8-sig') as source:
        return source.read()


def snake(name: str) -> str:
    return re.sub(r'(?<=[a-z0-9])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])', '_', name).lower()


def members(enum: str) -> list[str]:
    """The members of one of the old game's audio enums, in order."""
    body = re.search(r'enum ' + enum + r'\s*\{(.*?)\}', read(os.path.join(AUDIO, enum + '.cs')), re.S)
    if body is None:
        raise SystemExit(f'The old game has no enum {enum}.')
    code = '\n'.join(line for line in body.group(1).split('\n') if not line.strip().startswith('///'))
    return re.findall(r'^\s*(\w+)\s*(?:=[^,]*)?,?\s*$', code, re.M)


SOUNDS = set()
for library, prefix in (('SamusSound', 'samus_'), ('EnemySound', 'enemy_'), ('RoomSound', 'room_')):
    SOUNDS.update(prefix + snake(member) for member in members(library))
    # A request that only stops sounds: "stop" after its library, and what it stops unless it stops everything.
    SOUNDS.update(prefix + ('stop' if member == 'Any' else 'stop_' + snake(member)) for member in members(library + 'Stop'))
MUSIC = {snake(member) for member in members('MusicTrack')}
SETS = {snake(member) for member in members('MusicDataSet')}

asked_sounds: dict[str, str] = {}
asked_music: dict[str, str] = {}
asked_sets: dict[str, str] = {}
problems: list[str] = []

NAME = r'"([a-z0-9_]*)"'

# The Godot game's code: the names its constants hold, the names it asks for outright, and the names its
# properties of sounds and music start with.
for path in glob.glob(os.path.join(GODOT, '**', '*.cs'), recursive=True):
    if os.sep + 'tests' + os.sep in path:
        continue
    text = read(path)
    where = os.path.relpath(path, ROOT)
    file = os.path.basename(path)
    if file == 'Sounds.cs':
        for name in re.findall(r'const string \w+ = ' + NAME, text):
            asked_sounds[name] = where
    if file == 'Music.cs':
        for name in re.findall(r'const string \w+ = ' + NAME, text):
            asked_music[name] = where
    for name in re.findall(r'\bSounds\.Request\(\s*' + NAME, text):
        asked_sounds[name] = where
    for name in re.findall(r'\bRequestMusic\(\s*' + NAME, text):
        asked_music[name] = where
    for name in re.findall(r'\bAskMusicSet\(\s*' + NAME, text):
        asked_sets[name] = where
    for name in re.findall(r'public string \w*Sound(?!Folder)\w* \{ get; (?:set|init); \} = ' + NAME, text):
        asked_sounds[name] = where
    for name in re.findall(r'public string (?!MusicSet\b)\w*(?:Music(?!Folder)|Fanfare)\w* \{ get; (?:set|init); \} = ' + NAME, text):
        asked_music[name] = where

# Scenes, and the rules that generate the rooms' scenes: any property of a sound or of music.
for path in glob.glob(os.path.join(GODOT, 'game', '**', '*.tscn'), recursive=True):
    where = os.path.relpath(path, ROOT)
    text = read(path)
    for name in re.findall(r'^\w*Sound\w* = ' + NAME, text, re.M):
        asked_sounds[name] = where
    for name in re.findall(r'^(?!MusicSet\b)\w*(?:Music|Fanfare)\w* = ' + NAME, text, re.M):
        asked_music[name] = where
    for name in re.findall(r'^MusicSet = ' + NAME, text, re.M):
        asked_sets[name] = where
    for listed in re.findall(r'^\w*Sounds\w* = \w*\((.*)\)\s*$', text, re.M):
        for name in re.findall(NAME, listed):
            asked_sounds[name] = where
for path in glob.glob(os.path.join(ROOT, 'tools', 'godot-rooms', '*.py')):
    if os.path.basename(path).startswith('check_'):
        continue
    where = os.path.relpath(path, ROOT)
    text = read(path)
    for name in re.findall(r"'\w*Sound\w*': quoted\('([a-z0-9_]*)'\)", text):
        asked_sounds[name] = where
    for name in re.findall(r"'(?!MusicSet\b)\w*(?:Music|Fanfare)\w*': quoted\('([a-z0-9_]*)'\)", text):
        asked_music[name] = where

# The extractor's writers name sounds through EngineNames.Sound, which names them rightly by its making; a name
# written out by hand is checked.
for path in glob.glob(os.path.join(ENGINE, '*.cs')):
    where = os.path.relpath(path, ROOT)
    text = read(path)
    for name in re.findall(r'"sound:([a-z0-9_]+)"', text):
        asked_sounds[name] = where
    for name in re.findall(r'\["\w*sound\w*"\] = ' + NAME, text):
        asked_sounds[name] = where
    for name in re.findall(r'\["\w*(?:music|fanfare)\w*"\] = ' + NAME, text):
        asked_music[name] = where

for name, where in sorted(asked_sounds.items()):
    if name and name not in SOUNDS:
        problems.append(f'{where} asks for the sound "{name}", which is none of the old game\'s sounds')
for name, where in sorted(asked_music.items()):
    if name and name not in MUSIC:
        problems.append(f'{where} asks for the music "{name}", which is none of the old game\'s tracks')
for name, where in sorted(asked_sets.items()):
    if name and name not in SETS:
        problems.append(f'{where} names the set of music "{name}", which is none of the old game\'s sets')

for problem in problems:
    print(problem)
print(f'{len(problems)} problems; {len(asked_sounds)} sounds and {len(asked_music)} pieces of music asked for, of {len(SOUNDS)} sounds and {len(MUSIC)} pieces written')
sys.exit(1 if problems else 0)
