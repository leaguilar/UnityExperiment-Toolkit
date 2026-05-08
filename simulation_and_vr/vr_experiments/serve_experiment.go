package main

import (
	"encoding/json"
	"log"
	"net/http"
	"os"
)

type Config struct {
	BuildFolder string `json:"buildFolder"`
}

func main() {
	data, err := os.ReadFile("experiment_1_config.json")
	if err != nil {
		log.Fatal("Could not read experiment_1_config.json: ", err)
	}
	var cfg Config
	if err := json.Unmarshal(data, &cfg); err != nil {
		log.Fatal("Could not parse experiment_1_config.json: ", err)
	}
	if cfg.BuildFolder == "" {
		log.Fatal("buildFolder not set in experiment_1_config.json")
	}

	fs := http.FileServer(http.Dir(cfg.BuildFolder))
	http.Handle("/", fs)

	log.Println("Listening on :8080...")
	log.Println("http://localhost:8080/index.html?ExpID=AABBCC&group=1")
	err = http.ListenAndServe(":8080", nil)
	if err != nil {
		log.Fatal(err)
	}
}
