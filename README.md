# golang-comments-to-csharp-comments

[![CI](https://github.com/jason-xie-123/golang-comments-to-csharp-comments/actions/workflows/ci.yml/badge.svg)](https://github.com/jason-xie-123/golang-comments-to-csharp-comments/actions/workflows/ci.yml)
[![Release](https://img.shields.io/github/v/release/jason-xie-123/golang-comments-to-csharp-comments)](https://github.com/jason-xie-123/golang-comments-to-csharp-comments/releases/latest)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE)

Two small CLI tools that together sync Go doc comments onto generated C# bindings:

1. **`comments-sync-golang-export`** (Go) — parses a Go source folder and extracts the doc comments of exported functions, exported methods, and exported interface methods into a JSON file.
2. **`comments-sync-csharp-import`** (.NET) — reads that JSON file and injects the doc comments as XML `<summary>`/`<param>`/`<returns>` tags onto matching methods and delegates in a C# source file, using Roslyn to parse and rewrite the syntax tree.

## Install

Download the archive for your platform from the [latest release](https://github.com/jason-xie-123/golang-comments-to-csharp-comments/releases/latest) — each archive (Windows `.zip` / macOS `.tar.gz`) contains **both** tools.

## How to Use

```
comments-sync-golang-export -h
NAME:
   comments-sync-golang-export - CLI Tool to sync golang comments into c# code

USAGE:
   comments-sync-golang-export [global options] command [command options]

GLOBAL OPTIONS:
   --go-folder value         golang folder
   --output-json-file value  output json file
   --help, -h                show help
   --version, -v             print the version
```

```
comments-sync-csharp-import -h
  -c, --cs      Required. C# source file path
  -j, --json    Required. Go comments JSON file path
  --overwrite   Overwrite existing comments (default: keep existing comments if no matching Go doc is found)
  --help        Display this help screen.
  --version     Display version information.
```

Typical usage — export comments from a Go package, then inject them into a generated C# file:

```sh
comments-sync-golang-export --go-folder ./mypackage --output-json-file ./comments.json
comments-sync-csharp-import --cs ./Generated.cs --json ./comments.json
```

## Development

This repo has two independent toolchains that don't share a build system:

```sh
# Go side (export/)
cd export
go build ./...
go test ./...
gofmt -l .
golangci-lint run ./...

# .NET side (import/)
cd import
dotnet build SyncGoComments.csproj
dotnet test SyncGoComments.Tests/SyncGoComments.Tests.csproj
```

Releases are cut by pushing a `vX.Y.Z` tag — see `.github/workflows/release.yml`. Release notes live in `release_notes.md` and are drafted locally before tagging (see `AGENTS.md`, which also explains why there are two separate `Version` fields to keep in sync).

## License

[MIT](./LICENSE)
