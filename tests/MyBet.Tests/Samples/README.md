# Samples

This folder is intentionally empty in the repository. The real screenshots and
expected files are git-ignored because they contain real bet slip numbers and
betting history.

Model tests (`Category=Ollama`) are skipped automatically while this folder has
no samples, so a fresh clone builds and passes the fast tests as-is.

## Adding a sample (local only)

For each slip, add two files with the same base name:

- `<name>.png` (or `.jpg`, `.jpeg`, `.webp`): the screenshot
- `<name>.expected.json`: what is really on the screenshot, written by hand,
  never copied from the model's output

Both files stay untracked. To keep samples somewhere else entirely, set
`MYBET_SAMPLES` to that folder.

## Sharing a sample publicly

Make a synthetic one: invent a slip number, change the stake and teams, and
edit the image. Name it `synthetic-*` and add `!` exceptions for it in
`.gitignore`.
