#!/bin/bash
die () {
    echo "$*" >&2
    exit 1
}
make_absolute () {
    case "$1" in
    /*)
        echo "$1"
        ;;
    *)
        echo "$PWD/$1"
        ;;
    esac
}

# Directories
THISDIR="$( cd "$(dirname "$0")" ; pwd -P )"
ROOT="$( cd "$THISDIR"/../../.. ; pwd -P )"
SRC="$ROOT/src"
OUT="$ROOT/out"
INSTALLER_SRC="$SRC/osx/Installer.Mac"
MSAUTH_OUT="$OUT/osx/Microsoft.Authentication.Helper.Mac"
GCM_SRC="$SRC/shared/Git-Credential-Manager"

# Build parameters
FRAMEWORK=netcoreapp2.1
RUNTIME=osx-x64

# Parse script arguments
for i in "$@"
do
case "$i" in
    --configuration=*)
    CONFIGURATION="${i#*=}"
    shift # past argument=value
    ;;
    --output=*)
    PAYLOAD="${i#*=}"
    shift # past argument=value
    ;;
    --symbol-output=*)
    SYMBOLOUT="${i#*=}"
    ;;
    *)
          # unknown option
    ;;
esac
done

# Perform pre-execution checks
CONFIGURATION="${CONFIGURATION:=Debug}"
if [ -z "$PAYLOAD" ]; then
	die "--output was not set"
fi
if [ -z "$SYMBOLOUT" ]; then
    SYMBOLOUT="$PAYLOAD.sym"
fi

MSAUTH_BIN="$MSAUTH_OUT/bin/$CONFIGURATION/native"
MSAUTH_SYM="$MSAUTH_OUT/bin/$CONFIGURATION/native.sym"
if [ ! -d "$MSAUTH_BIN" ]; then
	die "No native helper binaries found. Did you build?"
fi

# Cleanup any old payload directory
if [ -d "$PAYLOAD" ]; then
    echo "Cleaning old payload directory '$PAYLOAD'..."
    rm -rf "$PAYLOAD"
fi

# Ensure payload and symbol directories exists
mkdir -p "$PAYLOAD" "$SYMBOLOUT"

# Copy uninstaller script
echo "Copying uninstall script..."
cp "$INSTALLER_SRC/uninstall.sh" "$PAYLOAD" || exit 1

# Copy native authentication helper executables
echo "Copying native helpers..."
cp -R "$MSAUTH_BIN/" "$PAYLOAD" || exit 1

# Publish core application executables
echo "Publishing core application..."
dotnet publish "$GCM_SRC" \
	--configuration="$CONFIGURATION" \
	--framework="$FRAMEWORK" \
	--runtime="$RUNTIME" \
	--output="$(make_absolute "$PAYLOAD")" || exit 1

# Collect symbols
echo "Collecting managed symbols..."
mv "$PAYLOAD"/*.pdb "$SYMBOLOUT" || exit 1
echo "Collecting native symbols..."
cp -R "$MSAUTH_SYM/" "$SYMBOLOUT" || exit 1

# Remove any unwanted .DS_Store files
echo "Removing unnecessary files..."
find "$PAYLOAD" -name '*.DS_Store' -type f -delete || exit 1

echo "Layout complete."
