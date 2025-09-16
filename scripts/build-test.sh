#!/bin/bash

OLD_PWD=$(pwd)
SHELL_FOLDER=$(
    cd "$(dirname "$0")" || exit
    pwd
)
PROJECT_FOLDER=$SHELL_FOLDER/..

cd "$SHELL_FOLDER" || exit >/dev/null 2>&1


COMMAND="\"$PROJECT_FOLDER/scripts/build/clean-build-cache.sh\""
echo exec: "$COMMAND"
if ! eval "$COMMAND"; then
    echo ""
    echo ""
    echo "[ERROR]: failed to clean build cache"
    echo ""
    echo "" 

    exit 1
fi


COMMAND="\"$PROJECT_FOLDER/scripts/build/build-export.sh\""
echo exec: "$COMMAND"
if ! eval "$COMMAND"; then
    echo ""
    echo ""
    echo "[ERROR]: failed to build export"
    echo ""
    echo "" 

    exit 1
fi

COMMAND="\"$PROJECT_FOLDER/scripts/build/build-import.sh\""
echo exec: "$COMMAND"
if ! eval "$COMMAND"; then
    echo ""
    echo ""
    echo "[ERROR]: failed to build import"
    echo ""
    echo "" 

    exit 1
fi

COMMAND="\"$PROJECT_FOLDER/scripts/build/compress-release.sh\""
echo exec: "$COMMAND"
if ! eval "$COMMAND"; then
    echo ""
    echo ""
    echo "[ERROR]: failed to compress release"
    echo ""
    echo "" 

    exit 1
fi

COMMAND="\"$PROJECT_FOLDER/scripts/upload/upload-to-github.sh\""
echo exec: "$COMMAND"
if ! eval "$COMMAND"; then
    echo ""
    echo ""
    echo "[ERROR]: failed to upload to GitHub"
    echo ""
    echo "" 

    exit 1
fi



cd "$OLD_PWD" || exit >/dev/null 2>&1