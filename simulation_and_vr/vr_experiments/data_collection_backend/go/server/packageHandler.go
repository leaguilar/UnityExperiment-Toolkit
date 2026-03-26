package server

import (
	"encoding/json"
	"fmt"
	"io/ioutil"
	"log"
	"net"
	"net/http"
	"os"
	"strings"
)

func getRealAddr(r *http.Request)  string {

	remoteIP := ""
	// the default is the originating ip. but we try to find better options because this is almost
	// never the right IP
	if parts := strings.Split(r.RemoteAddr, ":"); len(parts) == 2 {
		remoteIP = parts[0]
	}
	// If we have a forwarded-for header, take the address from there
	if xff := strings.Trim(r.Header.Get("X-Forwarded-For"), ","); len(xff) > 0 {
		addrs := strings.Split(xff, ",")
		lastFwd := addrs[len(addrs)-1]
		if ip := net.ParseIP(lastFwd); ip != nil {
			remoteIP = ip.String()
		}
		// parse X-Real-Ip header
	} else if xri := r.Header.Get("X-Real-Ip"); len(xri) > 0 {
		if ip := net.ParseIP(xri); ip != nil {
			remoteIP = ip.String()
		}
	}

	return remoteIP

}

func setupResponse(w *http.ResponseWriter, req *http.Request) {
	(*w).Header().Set("Access-Control-Allow-Origin", "*")
	(*w).Header().Set("Access-Control-Allow-Methods", "POST, GET, OPTIONS, PUT, DELETE")
	(*w).Header().Set("Access-Control-Allow-Headers", "Accept, Content-Type, Content-Length, Accept-Encoding, X-CSRF-Token, Authorization")
}

func (s *server) GetJsonHTTPS(rw http.ResponseWriter, req *http.Request){
	log.Println("## USING SECURE CHANNEL")
	s.GetJson(rw,req)
}

func (s *server) GetJson(rw http.ResponseWriter, req *http.Request) {

	setupResponse(&rw, req)
	if (*req).Method == "OPTIONS" {
		return
	}

	body, err := ioutil.ReadAll(req.Body)
	if err != nil {
		log.Println(Fata(fmt.Sprintf("Bad Request: %+v",err)))
		//panic(err)
	}

	var data map[string]interface{}
	err = json.Unmarshal([]byte(body), &data)
	if err != nil {
		log.Println((*req).Method)
		log.Println(Fata(fmt.Sprintf("From IP: %+v",getRealAddr(req))))
		log.Println(req.Header)
		log.Println(Fata(fmt.Sprintf("Empty package: %+v", data)))
		return
	}



	if participantID, ok := data["id"].(string); ok { //Every data should contain an ID
		participantID = s.reg.ReplaceAllString(participantID, "BAD")
		//Check if participant hasn't reached the limit
		if !s.isAllowed(participantID){
			log.Println(Fata(fmt.Sprintf("Not accepting packages from: %+v", participantID)))
			log.Println(req.Header)
			return
		}

		var sessionId string;
		if sessionId, ok = data["sid"].(string); !ok {
			sessionId ="BSESSION"
		}else{
			sessionId = s.reg.ReplaceAllString(sessionId, "BAD")
		}

		outputDirTrial :=s.outputDir+participantID+"/"+sessionId+"/"
		_, err = os.Stat(outputDirTrial)
		if os.IsNotExist(err) {
			os.MkdirAll(outputDirTrial, os.ModePerm)
		}
		// Experiment Header
		if _, ok := data["ge"].(string); ok && !fileExists(outputDirTrial+participantID+"_header.json") { //Gender is only in the header
			log.Println("Writing Header and Metadata")
			// Preparing the data to be marshalled and written.

			err = ioutil.WriteFile(outputDirTrial+participantID+"_header.json", body, os.ModePerm)
			if err != nil {
				log.Println(fmt.Sprintf("Couldn't write header: %+v", err))
			}
			s.uploadHeader(outputDirTrial+participantID+"_header.json",outputDirTrial+participantID+"_header.json")

			writeMetadataIfMissing(req,outputDirTrial+participantID+"_metadata.json")

		} else if trialNum, ok := data["tnum"].(string); ok {//trial number is required for every packet
			trialNum = s.reg.ReplaceAllString(trialNum, "BAD")

			outputDirTrial =outputDirTrial+trialNum+"/"
			_, err = os.Stat(outputDirTrial)
			if os.IsNotExist(err) {
				os.MkdirAll(outputDirTrial, os.ModePerm)
			}

			// Packet DATA
			if pid, ok := data["pid"].(string); ok && !fileExists(outputDirTrial+participantID+"_"+trialNum+"_"+pid+".json"){
				log.Println("Writing PACKET")
				err = ioutil.WriteFile(outputDirTrial+participantID+"_trial_"+trialNum+"_"+pid+".json", body, os.ModePerm) //Write trial header
				if err != nil {
					log.Println(fmt.Sprintf("Couldn't write data: %+v", err))
				}
				// Packet Header
			}else if _, ok := data["st"].(string); ok && !fileExists(outputDirTrial+participantID+"_"+trialNum+"_PH.json"){ //
				log.Println("Writing PACKET Header")
				err = ioutil.WriteFile(outputDirTrial+participantID+"_trial_"+trialNum+"_PH.json", body, os.ModePerm) //Write trial header
				if err != nil {
					log.Println(fmt.Sprintf("Couldn't write data: %+v", err))
				}
				// Packet TAIL
			} else if _, ok := data["checksum"].(string); ok && !fileExists(outputDirTrial+participantID+"_trial_"+trialNum+"_PT.json"){
				log.Println("Writing PACKET tail")
				err = ioutil.WriteFile(outputDirTrial+participantID+"_trial_"+trialNum+"_PT.json", body, os.ModePerm) //Write trial header
				if err != nil {
					log.Println(fmt.Sprintf("Couldn't write data: %+v", err))
				}
				// Maybe add code to assemble packets here
				s.checkAndAssembleAndSend(outputDirTrial, participantID+"_trial_"+trialNum)

				// METADATA Packet
			} else{
				log.Println(fmt.Sprintf("No proper packet head/data/ or tail"))
			}
		} else if cat, ok := data["cat"].(string); ok {

			// TODO Bad Design
			writeMetadataIfMissing(req,outputDirTrial+participantID+"_metadata.json")

			trialNum = s.reg.ReplaceAllString(trialNum, "BAD")

			outputDirTrial =outputDirTrial+"metadata/"
			_, err = os.Stat(outputDirTrial)
			if os.IsNotExist(err) {
				os.MkdirAll(outputDirTrial, os.ModePerm)
			}
			// TODO Sanitize inputs (not only here)
			if mid, ok := data["mid"].(string); ok && !fileExists(outputDirTrial+participantID+"_"+cat+"_"+mid+".json") {
				log.Println("Writing Metadata Packet")
				err = ioutil.WriteFile(outputDirTrial+participantID+"_"+cat+"_"+mid+".json", body, os.ModePerm) //Write trial header
				if err != nil {
					log.Println(fmt.Sprintf("Couldn't write data: %+v", err))
				}
			}
		} else {
			log.Println(Fata(fmt.Sprintf("No header, trial or meta data in the message")))

		}
	}else{
		log.Println(Warn(fmt.Sprintf("No id in the message: %+v",data)))
	}
	log.Println(Succ(fmt.Sprintf("SUCCESS")))
}