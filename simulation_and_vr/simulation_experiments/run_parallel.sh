#!/usr/bin/env bash

set -euo pipefail

# -----------------------------

# Defaults

# -----------------------------

WORKDIR="$(pwd)"
EXECUTABLE="EBD-Toolkit-HS4U-20241108.exe"
JSON_DIR="output_json"
LOG_DIR="sim_logs"
MAX_PARALLEL="$(nproc)"

# -----------------------------

# Help

# -----------------------------

show_help() {
echo "Usage: ./run_parallel.sh [options]"
echo ""
echo "Options:"
echo "  -e, --exe PATH        Executable name or path"
echo "  -w, --workdir PATH    Working directory"
echo "  -j, --jsondir PATH    JSON input directory"
echo "  -l, --logdir PATH     Log directory"
echo "  -p, --parallel N      Max parallel jobs"
echo "  -h, --help            Show help"
echo ""
echo "Example:"
echo "  ./run_parallel.sh -e mytool.exe -w /data/run -j output_json -p 8"
}

# -----------------------------

# Argument parsing

# -----------------------------

while [[ $# -gt 0 ]]; do
case "$1" in
-e|--exe)
EXECUTABLE="$2"
shift 2
;;
-w|--workdir)
WORKDIR="$2"
shift 2
;;
-j|--jsondir)
JSON_DIR="$2"
shift 2
;;
-l|--logdir)
LOG_DIR="$2"
shift 2
;;
-p|--parallel)
MAX_PARALLEL="$2"
shift 2
;;
-h|--help)
show_help
exit 0
;;
*)
echo "Unknown option: $1"
show_help
exit 1
;;
esac
done

# -----------------------------

# Setup

# -----------------------------

cd "$WORKDIR"

mkdir -p logs "$LOG_DIR"

echo "Working directory: $WORKDIR"
echo "Executable: $EXECUTABLE"
echo "JSON directory: $JSON_DIR"
echo "Log directory: $LOG_DIR"
echo "Max parallel jobs: $MAX_PARALLEL"
echo ""

if [[ ! -f "$EXECUTABLE" ]]; then
echo "Error: executable not found: $EXECUTABLE"
exit 1
fi

# -----------------------------

# Load JSON files

# -----------------------------

mapfile -t fileList < <(find "$JSON_DIR" -type f -name "**simId***sampleNum**.json")

NUM_FILES=${#fileList[@]}

echo "Total simulations: $NUM_FILES"
echo ""

running_jobs() {
jobs -rp | wc -l
}

# -----------------------------

# Run simulations

# -----------------------------

for ((i=0; i<NUM_FILES; i++)); do

```
file="${fileList[$i]}"
filename=$(basename "$file")
folderName=$(basename "$(dirname "$file")")

if [[ "$filename" =~ simId_([0-9]+)_sampleNum_([0-9]+)\.json ]]; then

    row="${BASH_REMATCH[1]}"
    sim="${BASH_REMATCH[2]}"

    logFile="${LOG_DIR}/${folderName}_loop_$((i+1))_simId_${row}_sampleNum_${sim}.txt"

    while [ "$(running_jobs)" -ge "$MAX_PARALLEL" ]; do
        sleep 1
    done

    (
        echo "Starting: $filename"

        "./$EXECUTABLE" \
            -config "$file" \
            -logFile "$logFile"

        echo "Finished: $filename"

    ) &

else
    echo "Skipping invalid file: $filename"
fi
```

done

# -----------------------------

# Wait for all

# -----------------------------

wait

echo ""
echo "All simulations completed."
