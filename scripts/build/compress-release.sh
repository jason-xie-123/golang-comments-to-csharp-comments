#!/bin/bash

OLD_PWD=$(pwd)
SHELL_FOLDER=$(
    cd "$(dirname "$0")" || exit
    pwd
)
PROJECT_FOLDER=$SHELL_FOLDER/../..

cd "$SHELL_FOLDER" || exit >/dev/null 2>&1

# shellcheck source=/dev/null
source "$PROJECT_FOLDER/scripts/base/env.sh"

RELEASE_DIR="$PROJECT_FOLDER/release"

cd "$RELEASE_DIR/windows-386" >/dev/null 2>&1 || exit
COMMAND="zip -r \"$RELEASE_DIR/golang-comments-sync-windows-386.zip\" ./*"
echo exec: "$COMMAND"
if ! eval "$COMMAND"; then
    echo ""
    echo ""
    echo "[ERROR]: failed to zip bins"
    echo ""
    echo ""

    exit 1
fi
cd - >/dev/null 2>&1 || exit
rm -rf "$RELEASE_DIR/windows-386"

cd "$RELEASE_DIR/windows-amd64" >/dev/null 2>&1 || exit
COMMAND="zip -r \"$RELEASE_DIR/golang-comments-sync-windows-amd64.zip\" ./*"
echo exec: "$COMMAND"
if ! eval "$COMMAND"; then
    echo ""
    echo ""
    echo "[ERROR]: failed to zip bins"
    echo ""
    echo ""

    exit 1
fi
cd - >/dev/null 2>&1 || exit
rm -rf "$RELEASE_DIR/windows-amd64"

cd "$RELEASE_DIR/windows-arm64" >/dev/null 2>&1 || exit
COMMAND="zip -r \"$RELEASE_DIR/golang-comments-sync-windows-arm64.zip\" ./*"
echo exec: "$COMMAND"
if ! eval "$COMMAND"; then
    echo ""
    echo ""
    echo "[ERROR]: failed to zip bins"
    echo ""
    echo ""

    exit 1
fi
cd - >/dev/null 2>&1 || exit
rm -rf "$RELEASE_DIR/windows-arm64"

COMMAND="tar -zcvf \"$RELEASE_DIR/golang-comments-sync-darwin-amd64.tar.gz\" -C \"$RELEASE_DIR/darwin-amd64\" \".\""
echo exec: "$COMMAND"
if ! eval "$COMMAND"; then
    echo ""
    echo ""
    echo "[ERROR]: failed to zip bins"
    echo ""
    echo ""

    exit 1
fi
rm -rf "$RELEASE_DIR/darwin-amd64"

COMMAND="tar -zcvf \"$RELEASE_DIR/golang-comments-sync-darwin-arm64.tar.gz\" -C \"$RELEASE_DIR/darwin-arm64\" \".\""
echo exec: "$COMMAND"
if ! eval "$COMMAND"; then
    echo ""
    echo ""
    echo "[ERROR]: failed to zip bins"
    echo ""
    echo ""

    exit 1
fi
rm -rf "$RELEASE_DIR/darwin-arm64"


cd "$OLD_PWD" || exit >/dev/null 2>&1
