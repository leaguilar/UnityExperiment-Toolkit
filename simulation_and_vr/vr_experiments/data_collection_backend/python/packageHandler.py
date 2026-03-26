import os
import json

outputDir="results/"

# Barebone package management
# TODO add error checking, and other safeguards

def dumpPacket(fname,data):
    with open(fname, 'w+') as json_file:
        json.dump(data, json_file)

def loadPacket(fname):
    with open(fname) as json_file:
        data = json.load(json_file)
    return data

def HanglePackage(data):

    participantID=data["id"]
    sessionId = data["sid"]
    outputDirTrial=outputDir+participantID+"/"+sessionId+"/"
    
    if not os.path.exists(outputDirTrial):
        os.makedirs(outputDirTrial)
    
    print(outputDirTrial)
    # Handle trial header
    if "ge" in data.keys():
        print("Writing Experiment Header")
        fname=outputDirTrial+participantID+"_header.json"
        dumpPacket(fname,data)
    elif "tnum" in data.keys():
        trialNum=data["tnum"]
        outputDirTrial =outputDirTrial+trialNum+"/"
        if not os.path.exists(outputDirTrial):
            os.makedirs(outputDirTrial)
       
        # Packet Data
        if "pid" in data.keys():
            pid = data["pid"]
            print("Writing Packet")
            fname=outputDirTrial+participantID+"_trial_"+trialNum+"_"+pid+".json"
            dumpPacket(fname,data)
        elif "st" in data.keys():
            print("Writting Packet header")
            fname=outputDirTrial+participantID+"_trial_"+trialNum+"_PH.json"
            dumpPacket(fname,data)
        elif "checksum" in data.keys():
            print("Writting Packet tail")
            fname=outputDirTrial+participantID+"_trial_"+trialNum+"_PT.json"
            dumpPacket(fname,data)
        else:
            print("No proper packet head/data or tail")
    elif "cat" in data.keys(): 
        cat = data["cat"]
        # TODO Needs a better design
        outputDirTrial =outputDirTrial+"metadata/"
        if not os.path.exists(outputDirTrial):
            os.makedirs(outputDirTrial)            
        if "mid" in data.keys():
            mid = data["mid"]
            print("Writing Metadata Packet")
            fname=outputDirTrial+participantID+"_"+cat+"_"+mid+".json"
            dumpPacket(fname,data)
        else:
            print("No proper metadata package")
    else:
        print("No header, trial or meta data in the message")
            
        
            
        
        


