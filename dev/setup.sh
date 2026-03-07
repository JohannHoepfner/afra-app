#!/bin/bash
set -e

WORKSPACE="${1:-/workspaces/afra-app}"

echo "==> Running dev environment setup..."
cd "$WORKSPACE/dev"
bash create_dev.sh

echo "==> Installing WebClient npm dependencies..."
cd "$WORKSPACE/WebClient"
npm install

echo "==> Building Rust bindings (warmup)..."
cd "$WORKSPACE/Backend/Altafraner.Typst/typst_with_bindings"
cargo build --release

echo "==> Building .NET backend (warmup)..."
dotnet build "$WORKSPACE/Backend/Altafraner.AfraApp/Altafraner.AfraApp.csproj"

echo "==> Setup complete."
