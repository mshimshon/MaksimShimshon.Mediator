#!/bin/bash
set -euo pipefail

VERSION_NEXT="${VERSION_NEXT:-}"

echo "Releasing version: $VERSION_NEXT"
OUT="/tmp/artifacts/output";
mkdir -p "$OUT"

dotnet build ./src/MaksimShimshon.Mediator.slnx  --configuration Release
dotnet pack "./src/MaksimShimshon.Mediator/MaksimShimshon.Mediator.csproj" -c Release -o "$OUT" /p:Version=$VERSION_NEXT
