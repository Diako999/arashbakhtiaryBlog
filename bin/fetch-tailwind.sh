#!/usr/bin/env bash
# Downloads the standalone Tailwind CLI binary (no Node.js/npm required).
# Re-run this on any new machine/host before running `bin/tailwindcss`.
set -euo pipefail
cd "$(dirname "$0")/.."

OS="$(uname -s)"
ARCH="$(uname -m)"

case "$OS-$ARCH" in
  Linux-x86_64)  ASSET="tailwindcss-linux-x64" ;;
  Linux-aarch64) ASSET="tailwindcss-linux-arm64" ;;
  Darwin-x86_64) ASSET="tailwindcss-macos-x64" ;;
  Darwin-arm64)  ASSET="tailwindcss-macos-arm64" ;;
  *) echo "Unsupported platform: $OS-$ARCH" >&2; exit 1 ;;
esac

curl -sL -o bin/tailwindcss \
  "https://github.com/tailwindlabs/tailwindcss/releases/latest/download/${ASSET}"
chmod +x bin/tailwindcss
./bin/tailwindcss --help >/dev/null && echo "tailwindcss CLI installed at bin/tailwindcss"
