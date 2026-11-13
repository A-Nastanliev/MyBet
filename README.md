# MyBet

A command-line tool that reads screenshots of settled sports bet slips, extracts the money numbers with a **local** vision model, works out whether each slip was won, lost or cashed out, and reports your betting performance: win rate, staked, returned, profit and ROI.

Everything runs on your own machine. No paid APIs, no cloud and your screenshots never leave your PC.

## Features

- Reads the slip summary (reference, stake, total odds, payout, bonus) from screenshots, using a vision model served by [Ollama](https://ollama.com).
- Derives the result from the numbers themselves, so there is no guessing from team names or scores.
- Skips screenshots it has already seen, and detects the same slip in two different screenshots.
- Sends anything doubtful to a review queue instead of saving a wrong number, and lets you fix it by hand.
- Stores everything in a local SQLite database that upgrades itself when you update the program.
- Reports totals with simple filters (won, lost, cashed out).

## How it works

The app reads only five values from each slip, all large and high-contrast text at the bottom of the screenshot:

| Value | Meaning |
|-------|---------|
| `slipId` | The slip reference (a number, a GUID or a base64 code) |
| `stake` | The amount staked |
| `totalOdds` | The odds of the whole slip |
| `payout` | The amount paid out |
| `bonusPercent` | The bonus percentage, if there is one |

The status is then a plain calculation:

| Payout | Status |
|--------|--------|
| 0 | Lost |
| Matches `stake × odds` plus the bonus | Won |
| Positive but clearly lower | Cashed out |
| Anything missing, or higher than possible | Goes to the review queue |

A slip that cannot be classified is never saved. It waits in the review queue until you correct it.

The prompt and the labels it looks for (`Залог`, `Общ коефициент`, `Изплащане`, `Печалба`, `Кешаут`) are written for **Bulgarian-language** bookmaker slips. Other languages need a prompt change.

## Requirements

- Windows, with the [.NET 10 SDK](https://dotnet.microsoft.com/download) to build it
- [Ollama](https://ollama.com) and the vision model:
  ```
  ollama pull qwen2.5vl:7b
  ```
- A GPU with enough memory for a 7B vision model (developed on a 16 GB card; Ollama can also run it on the CPU, more slowly)

You don't need to start Ollama yourself. The app starts it when it needs the model and stops it again on exit, unless it was already running.

## Install

Clone the repository and publish the CLI:

```
dotnet publish src/MyBet.Cli -c Release -o C:\Tools\MyBet
```

Add the output folder to your `PATH` (once), then open a new terminal:

```
[Environment]::SetEnvironmentVariable("Path", $env:Path + ";C:\Tools\MyBet", "User")
mybet --help
```

Don't enable trimming or single-file publishing: EF Core relies on reflection and SQLite's native library would be unpacked on every start.

## Usage

```
mybet -f "D:\Slips"       # set the home folder (your screenshots live here)
mybet -a "C:\Downloads"   # copy images from another folder into the home folder
mybet -u                  # scan the new screenshots
mybet -x                  # fix slips in the review queue
mybet                     # full summary
```

| Flag | Long | What it does |
|------|------|--------------|
| `-f [path]` | `--folder` | Set the home folder, or show it when no path is given. A different folder wipes the database, after a confirmation |
| `-u` | `--update` | Scan the home folder and import only screenshots not scanned before |
| `-a <path>` | `--add` | Copy an image, or all images in a folder, into the home folder |
| `-x` | `--review` | Go through the review queue |
| `-y` | `--yes` | Skip the confirmation when `-f` wipes the database |
| `-s` / `-r` / `-p` | `--stakes` / `--returns` / `--profit` | Show one number: total staked, total returned, or profit (one at a time) |
| `-w` / `-l` / `-c` | `--won` / `--lost` / `--cashed` | Narrow the stats to won, lost or cashed-out slips (combine freely, they mean "or") |

Examples:

```
mybet -l               summary of lost slips only
mybet -s -l            total staked on lost slips
mybet -p -c            profit on cashed-out slips
mybet -r -w -c         total returned by won or cashed-out slips
```

Supported image types are `.png`, `.jpg`, `.jpeg` and `.webp`. Only the top level of a folder is read, not its subfolders.

### Sample summary

```
Total slips: 111
  Won: 16 (14.4%)
  Cashed out: 40 (36.0%)
  Lost: 55 (49.5%)
Staked:   ...
Returned: ...
Profit:   ...
ROI:      ...
Needs review: 3
```

Slips in the review queue are counted only on the `Needs review` line. They are not part of any total.

### The review queue

For each slip that could not be classified, `-x` shows the five values the model read and lets you correct them:

- Enter keeps a value, `-` clears a number, and both `2.50` and `2,50` are accepted.
- The derived status is shown before anything is saved. You can accept (only when the slip is complete), edit again, skip or discard it.
- A slip reference that is already saved is flagged as a duplicate and cannot be accepted.

`-u` offers to start the review when it finishes.

## Your data

Settings and data live in `%AppData%\MyBet`:

| File | Holds |
|------|-------|
| `settings.json` | The home folder |
| `mybet.db` | The SQLite database (slips, scanned files, review queue) |
| `mybet.db.bak` | A copy made just before the database is upgraded |

Set the `MYBET_HOME` environment variable to use another location, which is handy for trying things out without touching your real data.

The database is created and upgraded automatically on every run through EF Core migrations, so updating the program never needs a manual step.

## Configuration

Ollama settings are in `appsettings.json` next to the executable: `BaseUrl`, `Model`, `NumCtx`, `TimeoutMinutes`, `ManageLifecycle`, `ExecutablePath`, `StartupTimeoutSeconds` and `KeepAlive`. Any of them can be overridden with an environment variable, for example:

```
$env:Ollama__Model = "qwen2.5vl:3b"
```

## Limitations

- Built for finished slips. A slip with no payout line goes to the review queue.
- The model can occasionally invent a value for a field that is covered or blurred. The payout math catches this for won and cashed-out slips, but a lost slip has nothing to cross-check the stake against.
- Slip references are compared exactly, including case. The same reference read in two different cases would be stored twice.
- The same slip in two screenshots costs two model calls, because the reference comes from the image. It is detected and counted once.
- Only the layouts the prompt has been tested on are known to work. A new bookmaker layout may need a prompt tweak.

### Structure

```
src/
  MyBet.Domain          Entities, the status rule (no dependencies)
  MyBet.Application     Services and interfaces: import, review, copy, stats, home folder
  MyBet.Infrastructure  Ollama client and lifecycle, prompt, schema, settings file
  MyBet.Persistence     EF Core + SQLite: context, migrations, repositories
  MyBet.Cli             Entry point and commands (System.CommandLine)
tests/
  MyBet.Tests           Unit tests, in-memory SQLite tests, model tests
```

Dependencies point inward: `Domain` knows nothing else, `Application` uses only `Domain`, and `Infrastructure` and `Persistence` implement `Application`'s interfaces. The services return result objects and never print or prompt, so another front end can reuse them.

### Model tests and samples

The model tests send real screenshots to Ollama and compare the five values with hand-written expected files. The real samples contain betting history, so they are not in the repository. See [tests/MyBet.Tests/Samples/README.md](tests/MyBet.Tests/Samples/README.md) for how to add your own, or how to share a synthetic one.