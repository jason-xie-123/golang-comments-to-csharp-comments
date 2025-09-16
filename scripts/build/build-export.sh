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

TARGET_NAME="comments-sync-golang-export"

RELEASE_DIR="$PROJECT_FOLDER/release"

echo "Compiling binaries for multiple platforms..."

cd "$PROJECT_FOLDER/export" || exit >/dev/null 2>&1

BUILD_PARAMETER="-ldflags '-w -s' -trimpath"
# BUILD_PARAMETER=""

# Windows amd64
COMMAND="GOOS=windows GOARCH=amd64 go build $BUILD_PARAMETER -o $RELEASE_DIR/windows-amd64/${TARGET_NAME}.exe"
echo exec: "$COMMAND"
if eval "$COMMAND"; then
    echo "Windows amd64 binary compiled successfully."
else
    echo "Failed to compile Windows amd64 binary."
    exit 1
fi
# Windows 32-bit
COMMAND="GOOS=windows GOARCH=386 go build $BUILD_PARAMETER -o $RELEASE_DIR/windows-386/${TARGET_NAME}.exe"
echo exec: "$COMMAND"
if eval "$COMMAND"; then
    echo "Windows 32-bit binary compiled successfully."
else
    echo "Failed to compile Windows 32-bit binary."
    exit 1
fi
# Windows ARM
COMMAND="GOOS=windows GOARCH=arm64 go build $BUILD_PARAMETER -o $RELEASE_DIR/windows-arm64/${TARGET_NAME}.exe"
echo exec: "$COMMAND"
if eval "$COMMAND"; then
    echo "Windows ARM binary compiled successfully."
else
    echo "Failed to compile Windows ARM binary."
    exit 1
fi

# darwin amd64
COMMAND="GOOS=darwin GOARCH=amd64 go build $BUILD_PARAMETER -o $RELEASE_DIR/darwin-amd64/${TARGET_NAME}"
echo exec: "$COMMAND"
if eval "$COMMAND"; then
    echo "darwin amd64 binary compiled successfully."
else
    echo "Failed to compile darwin amd64 binary."
    exit 1
fi
# darwin ARM
COMMAND="GOOS=darwin GOARCH=arm64 go build $BUILD_PARAMETER -o $RELEASE_DIR/darwin-arm64/${TARGET_NAME}"
echo exec: "$COMMAND"
if eval "$COMMAND"; then
    echo "darwin ARM binary compiled successfully."
else
    echo "Failed to compile darwin ARM binary."
    exit 1
fi

echo "Compilation completed."

echo "Generated binaries:"
ls -lh "$RELEASE_DIR"

cd "$OLD_PWD" || exit >/dev/null 2>&1
