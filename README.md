# How to Use

```
./comments-sync-golang-export -h
NAME:
   comments-sync-golang-export - CLI Tool to sync golang comments into c# code

USAGE:
   comments-sync-golang-export [global options] command [command options]

VERSION:
   0.1.0

COMMANDS:
   help, h  Shows a list of commands or help for one command

GLOBAL OPTIONS:
   --go-folder value         golang folder
   --output-json-file value  output json file
   --help, -h                show help
   --version, -v             print the version
```

```
./comments-sync-csharp-import -h
comments-sync-csharp-import 0.1.0
Copyright (C) 2025 comments-sync-csharp-import

ERROR(S):
  Option 'h' is unknown.
  Required option 'c, cs' is missing.
  Required option 'j, json' is missing.

  -c, --cs      Required. C# source file path

  -j, --json    Required. Go comments JSON file path

  --help        Display this help screen.

  --version     Display version information.
```