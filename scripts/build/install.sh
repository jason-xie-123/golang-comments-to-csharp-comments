#!/bin/bash

OLD_PWD=$(pwd)
SHELL_FOLDER=$(
    cd "$(dirname "$0")" || exit
    pwd
)
PROJECT_FOLDER=$SHELL_FOLDER/../..

cd "$PROJECT_FOLDER/export" || exit >/dev/null 2>&1

go install ./cmd/comments-sync-golang-export

cd "$OLD_PWD" || exit >/dev/null 2>&1
