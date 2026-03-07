#!/bin/bash
set -e
cd "$(dirname "$0")/.."
dotnet run --project Backend/Altafraner.AfraApp/Altafraner.AfraApp.csproj
