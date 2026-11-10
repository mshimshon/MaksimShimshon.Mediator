set -euo pipefail
VERSION_NEXT="${VERSION_NEXT:-}"

echo "Releasing version: $VERSION_NEXT"
OUTPUT="/tmp/artifacts/output"

echo "Checking directory: $OUTPUT"

# Define array of expected filenames
ALLOWED_FILES=(
  "MaksimShimshon.Mediator.$VERSION_NEXT.nupkg"
  "_.$VERSION_NEXT.md"
  "build.env"
)

# Fail immediately if a required file is missing
for file in "${ALLOWED_FILES[@]}"; do
  if [ ! -f "$OUTPUT/$file" ]; then
    echo "❌ Error: Required file missing -> $file"
    echo "🚨 Validation failed!"
    exit 1
  fi
done

# Optional: List contents only if everything up to this point passed
ls "$OUTPUT"

echo "✅ Validation passed successfully! Match complete."
