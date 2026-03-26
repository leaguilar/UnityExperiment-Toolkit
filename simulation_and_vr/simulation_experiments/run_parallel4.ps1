# Enable strict mode
Set-StrictMode -Version Latest

# Define your working directory
$workingDirectory = ""

cd $workingDirectory

# Create necessary directories
New-Item -ItemType Directory -Path logs, sim_logs -Force | Out-Null

# Get the list of JSON files
$fileList = Get-ChildItem -Path "output_json\*\*_simId_*_sampleNum_*.json" -File
$NUM_FILES = $fileList.Count

Write-Output "Total simulations: $NUM_FILES"

# Define maximum parallel jobs
$MaxParallelJobs = 8
$jobs = @{}
# Function to check and remove completed jobs
function Remove-CompletedJobs {
    foreach ($id in @($jobs.Keys)) {
        $job = Get-Job -Id $jobs[$id]
        # Write-Output "Checking job $($job.Id) - State: $($job.State)"
        
        # Instead of checking for completion on a job state, wait for the job to complete
        if ($job.State -eq 'Running') {
            # Write-Output "Job $($job.Id) is still running."
        } elseif ($job.State -eq 'Completed') {
            Write-Output "Job $($job.Id) completed. Removing it."
            $jobOutput = Receive-Job -Id $jobs[$id] -Wait  # Capture the output
            Write-Output "Job $($job.Id) output: $jobOutput"
            Remove-Job -Id $jobs[$id]
            $jobs.Remove($id)
        } elseif ($job.State -eq 'Failed') {
            Write-Error "Job $($job.Id) failed. Removing it."
            $jobOutput = Receive-Job -Id $jobs[$id] -Wait  # Capture the output
            Write-Error "Job $($job.Id) output: $jobOutput"
            Remove-Job -Id $jobs[$id]
            $jobs.Remove($id)
        }
    }
}

# Modify the loop where you start jobs, add a clearer running condition
for ($i = 0; $i -lt $NUM_FILES; $i++) {
    $file = $fileList[$i].FullName
    $folderName = Split-Path (Split-Path $file -Parent) -Leaf
    $filename = $fileList[$i].Name

    if ($filename -match "simId_(\d+)_sampleNum_(\d+)\.json") {
        $row = $matches[1]
        $sim = $matches[2]
        $logFile = "sim_logs\${folderName}_loop_$($i+1)_simId_${row}_sampleNum_${sim}.txt"

        # Wait for available job slots
        while ($jobs.Count -ge $MaxParallelJobs) {
            Start-Sleep -Seconds 10  # Reduce sleep interval for quicker job checks
            Remove-CompletedJobs
        }

        $job = Start-Job -ScriptBlock {
            param ($file, $logFile, $workingDirectory)
    
            try {
                # Change the working directory to the specified one
                Set-Location -Path $workingDirectory
        
                Start-Process -FilePath ".\EBD-Toolkit-HS4U-20241108.exe" -ArgumentList "-config=`"$file`"", "-logFile `"$logFile`"" -Wait

            } catch {
                # Ignore any errors here as well
                $null = $_
            }
        } -ArgumentList $file, $logFile, $workingDirectory

        # Store job ID with a unique identifier (row-sim) instead of $i
        $jobs["$row-$sim"] = $job.Id
        Write-Output "Started job $($job.Id) for file: $file"
    } else {
        Write-Output "Skipping invalid task ID: $($i+1) (file does not match expected pattern)"
    }
}

# Wait for remaining jobs to finish
while ($jobs.Count -gt 0) {
    Start-Sleep -Seconds 1
    Remove-CompletedJobs  # This will keep checking jobs and clean up once done
}

Write-Output "All simulations completed."

