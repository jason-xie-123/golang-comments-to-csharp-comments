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

check_dotnet_exist

TARGET_NAME="comments-sync-csharp-import"

RELEASE_DIR="$PROJECT_FOLDER/release"

COMMAND="dotnet restore \"$PROJECT_FOLDER/import/SyncGoComments.csproj\""
echo exec: "$COMMAND"
if ! eval "$COMMAND"; then
    echo ""
    echo ""
    echo "[ERROR]: failed to restore NuGet packages"
    echo ""
    echo ""
    exit 1
fi

echo "Compiling binaries for multiple platforms..."

cd "$PROJECT_FOLDER/import" || exit >/dev/null 2>&1


# Windows amd64
COMMAND="dotnet publish -c Release -r win-x64 --self-contained false /p:PublishSingleFile=true /p:AssemblyName=$TARGET_NAME -o $RELEASE_DIR/windows-amd64/"
echo exec: "$COMMAND"
if eval "$COMMAND"; then
    echo "Windows amd64 binary compiled successfully."
    rm -rf "$RELEASE_DIR/windows-amd64/$TARGET_NAME.pdb"
else
    echo "Failed to compile Windows amd64 binary."
    exit 1
fi
# Windows 32-bit
COMMAND="dotnet publish -c Release -r win-x86 --self-contained false /p:PublishSingleFile=true /p:AssemblyName=$TARGET_NAME -o $RELEASE_DIR/windows-386/"
echo exec: "$COMMAND"
if eval "$COMMAND"; then
    echo "Windows 32-bit binary compiled successfully."
    rm -rf "$RELEASE_DIR/windows-386/$TARGET_NAME.pdb"
else
    echo "Failed to compile Windows 32-bit binary."
    exit 1
fi
# Windows ARM
COMMAND="dotnet publish -c Release -r win-arm64 --self-contained false /p:PublishSingleFile=true /p:AssemblyName=$TARGET_NAME -o $RELEASE_DIR/windows-arm64/"
echo exec: "$COMMAND"
if eval "$COMMAND"; then
    echo "Windows ARM binary compiled successfully."
    rm -rf "$RELEASE_DIR/windows-arm64/$TARGET_NAME.pdb"
else
    echo "Failed to compile Windows ARM binary."
    exit 1
fi

# darwin amd64
COMMAND="dotnet publish -c Release -r osx-x64 --self-contained true /p:PublishSingleFile=true /p:AssemblyName=$TARGET_NAME -o $RELEASE_DIR/darwin-amd64/"
echo exec: "$COMMAND"
if eval "$COMMAND"; then
    echo "darwin amd64 binary compiled successfully."
    rm -rf "$RELEASE_DIR/darwin-amd64/$TARGET_NAME.pdb"
else
    echo "Failed to compile darwin amd64 binary."
    exit 1
fi
# darwin ARM
COMMAND="dotnet publish -c Release -r osx-arm64 --self-contained true /p:PublishSingleFile=true /p:AssemblyName=$TARGET_NAME -o $RELEASE_DIR/darwin-arm64/"
echo exec: "$COMMAND"
if eval "$COMMAND"; then
    echo "darwin ARM binary compiled successfully."
    rm -rf "$RELEASE_DIR/darwin-arm64/$TARGET_NAME.pdb"
else
    echo "Failed to compile darwin ARM binary."
    exit 1
fi

echo "Compilation completed."

echo "Generated binaries:"
ls -lh "$RELEASE_DIR"

cd "$OLD_PWD" || exit >/dev/null 2>&1
