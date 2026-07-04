## Changelog for v0.2.1

Housekeeping release, no CLI behavior changes.

- **Fixed `scripts/build-test.sh`**: removed the last step's call to `scripts/upload/upload-to-github.sh`, which was deleted during the Azure DevOps → GitHub Actions migration. Uploading is now handled entirely by the GitHub Actions release workflow, so running this script locally no longer fails at that step.
- **Docs**: corrected `AGENTS.md`'s claim that the two toolchains share no `go.work` at all — the root `go.work` does exist, it just only covers `export/` (there's no .NET equivalent unifying the two). Also fixed a `README.md` example that showed `comments-sync-csharp-import -h`, which `CommandLineParser` doesn't recognize as a `--help` alias (unlike the Go tool's `urfave/cli`, which does support `-h`); the example now uses `--help`.
