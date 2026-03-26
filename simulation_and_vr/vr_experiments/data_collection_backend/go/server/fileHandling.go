package server

import (
	"crypto/md5"
	"encoding/json"
	"fmt"
	"github.com/leaguilar/SimpleExaCT/4_data_assembly/backend/go/storage"
	"io/ioutil"
	"log"
	"net/http"
	"os"
	"path/filepath"
	"strconv"
	"strings"
	"time"
)

func fileExists(filename string) bool {
	info, err := os.Stat(filename)
	if os.IsNotExist(err) {
		return false
	}
	return !info.IsDir()
}

func dirSizeMB(path string) float64 {
	var dirSize int64 = 0
	readSize := func(path string, file os.FileInfo, err error) error {
		if !file.IsDir() {
			dirSize += file.Size()
		}
		return nil
	}
	filepath.Walk(path, readSize)
	sizeMB := float64(dirSize) / 1024.0 / 1024.0
	return sizeMB
}

func (s *server) MonitorDirectories() {
 	_, err := os.Stat(s.outputDir)
	if os.IsNotExist(err) {
		os.MkdirAll(s.outputDir, os.ModePerm)
	}
	go func() {
		for {
			log.Println(Succ(fmt.Sprintf("CHECKING DISK USAGE")))
			// Iterate through experiments entries
			files, err := ioutil.ReadDir(s.outputDir)
			if err != nil {
				log.Println(Fata(fmt.Sprintf("OFFENDING OUTPUT DIR %+v",err)))
				//log.Fatal(err)
			}
			//log.Println(files)
			for _, f := range files {
				if f.IsDir() {
					var dirSize float64 = dirSizeMB(s.outputDir + f.Name())
					var allowed bool
					if dirSize < s.threshold {
						allowed = true
					} else {
						allowed = false
					}
					// Iterate through sessions
					if !s.modExperimentEntry(f.Name(), dirSize, allowed){
						s.initExperimentEntry(f.Name())
						log.Println(Succ(fmt.Sprintf("InitExperimentEntry: %v",f.Name())))
					}
				}
			}
			time.Sleep(time.Second * 60 * 5)
		}
	}()
}

func writeMetadataIfMissing(req *http.Request,fname string){
	if !fileExists(fname) {
		metadata := make(map[string]string)

		if userAgent, ok := req.Header["User-Agent"]; ok {
			metadata["User-Agent"] = strings.Join(userAgent, " ")
		} else {
			metadata["User-Agent"] = ""
		}
		metadata["IP"] = getRealAddr(req)

		dataBytes, err := json.Marshal(metadata)
		if err != nil {
			log.Println(Fata(fmt.Sprintf("OFFENDING METADATA %+v",err)))
			//panic(err)
		}

		err = ioutil.WriteFile(fname, dataBytes, os.ModePerm)
		if err != nil {
			log.Println(fmt.Sprintf("Couldn't write metadata: %+v", err))
		}
	}
}

func extractFile(trialFile string) (map[string]interface{},[]byte,os.FileInfo){
	jsonFile, err := os.Open(trialFile)
	if err != nil {
		fmt.Println(err)
	}
	defer jsonFile.Close()
	byteValue, _ := ioutil.ReadAll(jsonFile)

	var data map[string]interface{}
	err = json.Unmarshal(byteValue, &data)
	if err != nil {
		log.Println(Fata(fmt.Sprintf("OFFENDING FILE %+v",trialFile)))
		log.Println(byteValue)
		//log.Fatal(err)
	}
	fileInfo, _ := jsonFile.Stat()
	// If dealing with too many files it might not be a good idea to keep
	// map and byte array in memory
	return data,byteValue,fileInfo
}

func (s *server) uploadHeader(file string, location string){
	// THIS IS CODE DEALS WITH AT MOST ~2GB FILES (TOTAL)
	go func(){
		cl:=storage.NewAwsClient()
		cl.AddFileToS3(file,location)
	}()
}


func (s *server) checkAndAssembleAndSend(path string, filePrefix string){
	// THIS IS CODE DEALS WITH AT MOST ~2GB FILES (TOTAL)
	go func(){

		// Wait to ensure all packages have been received
		time.Sleep(time.Second * 30)

		cl:=storage.NewAwsClient()
		pattern := path+filePrefix+"_*.json"
		fmt.Println(pattern)
		matches, err := filepath.Glob(pattern)

		if err != nil {
			log.Println(Fata(fmt.Sprintf("## ERROR No files with pattern %+v",pattern)))
		}
		csvData:=""
		checksum:=""
		experimentName:=""
		sessionName:=""
		trialNumber:=-1
		var trialTime float64=-1.0
		//totalTime:=-1
		var packetData = make(map[int]string)
		maxPacket:=0

		count:=0
		for _, trialFile := range matches {

			data,byteValue,fileInfo := extractFile(trialFile)

			if pid, ok := data["pid"].(string); !ok {
				log.Println("Deal with Head and Tail: "+trialFile)
				if strings.Contains(trialFile, "PT"){
					if checksum, ok = data["checksum"].(string); !ok {
						log.Println(Fata(fmt.Sprintf("Couldn't find checksum")))
					}

					if experimentName, ok = data["id"].(string); !ok {
						log.Println(Fata(fmt.Sprintf("Couldn't find id")))
					}

					if sessionName, ok = data["sid"].(string); !ok {
						log.Println(Fata(fmt.Sprintf("Couldn't find SID")))
					}

					tempStr:=""
					if tempStr, ok = data["pnum"].(string); !ok {
						log.Println(Fata(fmt.Sprintf("Couldn't find MaxPackets")))
						log.Println(tempStr)
					}else{
						maxPacket,_ = strconv.Atoi(data["pnum"].(string))
					}

					if tempStr, ok = data["tnum"].(string); !ok {
						log.Println(Fata(fmt.Sprintf("Couldn't find trial number")))
					}else{
						trialNumber, _ =strconv.Atoi(data["tnum"].(string))
					}

					if tempStr, ok = data["tspan"].(string); !ok {
						log.Println(Fata(fmt.Sprintf("Couldn't find trial time")))
					}else{
						trialTime, _ = strconv.ParseFloat( data["tspan"].(string), 64)
					}
				}

				err := cl.AddFileBufferToS3(byteValue,fileInfo.Size(),trialFile)
				if err != nil {
					log.Println(Fata(fmt.Sprintf("Couldn upload: %+v", trialFile)))
				}

			}else{
				intPID, err := strconv.Atoi(pid)
				if err != nil {
					log.Println(Fata(fmt.Sprintf("Incorrect PID: %+v", intPID)))
				}

				count+=1
				packetData[intPID]=data["DATA"].(string)
			}
		}

		// THIS IS CODE DEALS WITH AT MOST ~2GB FILES
		for i := 0; i < maxPacket; i++ {
			if val, ok := packetData[i]; ok {
				csvData+=val
			}else{
				csvData+="\nMISSING DATA\n"
			}
		}

		fileState:=""
		byteData:=[]byte(csvData)
		fileName:=""
		calcChecksum:=fmt.Sprintf("%x", md5.Sum(byteData))

		if maxPacket!=count{
			log.Println(Fata(fmt.Sprintf("Missing packages: %+v/%+v", count,maxPacket)))
			fileState="MissingPackages"
		}else if calcChecksum == checksum{
			//Send data to S3
			fileState="CORRECT"
			log.Println("Hurray Correct Trial")
		}else{
			log.Println(Fata(fmt.Sprintf("Wrong Checksum: %+v should be %+v", calcChecksum,checksum)))
			fileState="FAIL"
		}

		fileName=path+filePrefix+"_"+fileState+".csv"
		if s.setTrialSummary(experimentName,trialSummary{sessionName,trialNumber,trialTime,fileState}){
			s.initExperimentEntry(experimentName)
			s.setTrialSummary(experimentName,trialSummary{sessionName,trialNumber,trialTime,fileState})
		}

		log.Println(Fata(fmt.Sprintf("DATA: %+v", fileName)))


		// THIS IS CODE DEALS WITH AT MOST ~2GB FILES
		err = cl.AddFileBufferToS3(byteData,int64(len(csvData)),fileName)
		if err != nil {
			log.Println(Fata(fmt.Sprintf("Couldn't upload CSV: %+v", fileName)))
		}

	}()
}
