# Helper script to start the Go Data Collection Server
$env:Path += ";C:\Program Files\Go\bin"
cd simulation_and_vr\vr_experiments\data_collection_backend\go
go run main.go
