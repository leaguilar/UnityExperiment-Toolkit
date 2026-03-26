package server

import (
"fmt"
"log"
"regexp"
)

var (
	Info = Teal
	Warn = Yellow
	Fata = Red
	Succ = Green
)

var (
	Black   = color("\033[1;30m%s\033[0m")
	Red     = color("\033[1;31m%s\033[0m")
	Green   = color("\033[1;32m%s\033[0m")
	Yellow  = color("\033[1;33m%s\033[0m")
	Purple  = color("\033[1;34m%s\033[0m")
	Magenta = color("\033[1;35m%s\033[0m")
	Teal    = color("\033[1;36m%s\033[0m")
	White   = color("\033[1;37m%s\033[0m")
)

type trialSummary struct{
	sessionName string
	trialNumber int
	trialTime   float64
	state       string
}

type experimentEntry struct {
	name string
	size  float64
	allowed bool
	completedTrials map[int]trialSummary
}

type CommandType int


const (
	getCommand = iota
	setCommand
	modCommand
)

type entryLevelCommand struct {
	ty        CommandType
	name      string
	entry     experimentEntry
	replyChan chan experimentEntry
}

type trialLevelCommand struct {
	ty        CommandType
	name      string
	sessionName string
	entry     trialSummary
	replyChan chan trialSummary
}

///*
// Server is the shared data structure for HTTP handlers. It wraps a channel of
// commands that are used to interact with a manager running concurrently.
type server struct {
	outputDir string
	threshold float64
	allEntries map[string]experimentEntry
	reg *regexp.Regexp //can be used conccurently
	cmds chan<- interface{}
}

func color(colorString string) func(...interface{}) string {
	sprint := func(args ...interface{}) string {
		return fmt.Sprintf(colorString,
			fmt.Sprint(args...))
	}
	return sprint
}

func NewServer(outputDir string,threshold float64) server {

	var allEntries = make(map[string]experimentEntry)
	var reg *regexp.Regexp

	var err error
	reg, err = regexp.Compile("[^a-zA-Z0-9]+")
	if err != nil {
		log.Fatal(err)
	}

	cmds := make(chan interface{})

	//Listen to the commands
	go func () {
		for cmd := range cmds {
			//fmt.Printf("STARTING chanel TYPE: %T\n",cmd)
			switch cmd.(type){
			case entryLevelCommand:
				pcmd:=cmd.(entryLevelCommand)
				//log.Println("pcmd %+v\n",pcmd.ty)
				switch pcmd.ty {
				case getCommand:
					if entry, ok := allEntries[pcmd.name]; ok {
						pcmd.replyChan <- entry
					} else {
						pcmd.replyChan <- experimentEntry{}
					}
				case setCommand:
					allEntries[pcmd.name] = pcmd.entry
					pcmd.replyChan <- pcmd.entry
				case modCommand:
					// Modifies only the size and allowed
					if _, ok := allEntries[pcmd.name]; ok {
						tempEntry:=allEntries[pcmd.name]
						tempEntry.size=pcmd.entry.size
						tempEntry.allowed=pcmd.entry.allowed
						allEntries[pcmd.name] = tempEntry
						pcmd.replyChan <- allEntries[pcmd.name]
					} else {
						pcmd.replyChan <- experimentEntry{}
					}
				default:
					log.Fatal("unknown command type", pcmd.ty)
				}
			case trialLevelCommand:
				pcmd:=cmd.(trialLevelCommand)
				//log.Println("pcmd %+v\n",pcmd.ty)
				switch pcmd.ty {
				case getCommand:
					if entry, ok := allEntries[pcmd.name].completedTrials[pcmd.entry.trialNumber]; ok {
						pcmd.replyChan <- entry
					} else {
						pcmd.replyChan <- trialSummary{}
					}
				case setCommand:
					if _, ok := allEntries[pcmd.name]; ok {
						allEntries[pcmd.name].completedTrials[pcmd.entry.trialNumber] = pcmd.entry
						pcmd.replyChan <- allEntries[pcmd.name].completedTrials[pcmd.entry.trialNumber]
					} else {
						pcmd.replyChan <- trialSummary{}
					}
				case modCommand:
					if _, okEntry := allEntries[pcmd.name]; okEntry {
						if _, ok := allEntries[pcmd.name]; ok {
							allEntries[pcmd.name].completedTrials[pcmd.entry.trialNumber] = pcmd.entry
							pcmd.replyChan <- allEntries[pcmd.name].completedTrials[pcmd.entry.trialNumber]
						}else{
							pcmd.replyChan <- trialSummary{}
						}
					} else {
						pcmd.replyChan <- trialSummary{}
					}
				default:
					log.Fatal("unknown command type", pcmd.ty)
				}
			default:
				fmt.Printf("unknown chanel TYPE: %T\n",cmd)
			}
			//log.Println(Succ(fmt.Sprintf("COMMAND SUCCESS")))
		}
	}()
	server := server{outputDir,threshold,allEntries,reg,cmds}
	return server
}

func (s *server) getExperimentEntry(name string) experimentEntry {
	replyChan := make(chan experimentEntry)
	s.cmds <- entryLevelCommand{ty: getCommand, name: name, replyChan: replyChan}
	reply := <-replyChan
	return reply
}

func (s *server) setExperimentEntry(name string, val experimentEntry) {
	replyChan := make(chan experimentEntry)
	s.cmds <- entryLevelCommand{ty: setCommand, name: name, entry: val, replyChan: replyChan}
	ok := <-replyChan
	if ok.name == "" {
		log.Println(Fata(fmt.Sprintf("Unable to set entry: %+v ", name)))
	}
}

func (s *server) initExperimentEntry(name string) bool {
	replyChan := make(chan experimentEntry)
	s.cmds <- entryLevelCommand{ty: setCommand, name: name, entry: experimentEntry{name,0,true,make(map[int]trialSummary)}, replyChan: replyChan}
	ok := <-replyChan
	if ok.name == "" {
		log.Println(Fata(fmt.Sprintf("Unable to set entry: %+v ", name)))
		return false
	}
	return true
}

func (s *server) modExperimentEntry(name string,size float64, allowed bool) bool {
	replyChan := make(chan experimentEntry)
	s.cmds <- entryLevelCommand{ty: modCommand, name: name, entry: experimentEntry{name,size,allowed,nil}, replyChan: replyChan}
	ok := <-replyChan
	if ok.name == "" {
		log.Println(Fata(fmt.Sprintf("Unable to modify entry: %+v ", name)))
		return false
	}
	return true
}

func (s *server) setTrialSummary(name string, val trialSummary) bool {
	replyChan := make(chan trialSummary)
	s.cmds <- trialLevelCommand{ty: setCommand, name: name, entry: val, replyChan: replyChan}
	ok := <-replyChan
	if ok.sessionName == "" {
		log.Println(Fata(fmt.Sprintf("Unable to set entry: %+v ", name)))
		return false
	}
	return true
}


func (s *server) isAllowed(name string) bool{
	entry:=s.getExperimentEntry(name)
	if entry.name == "" {
		log.Println(Fata(fmt.Sprintf("Unable to get entry: %+v ", name)))
		return true
	}else{
		if entry.allowed {
			return true
		}
	}
	return false
}


