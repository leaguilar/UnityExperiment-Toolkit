package main
 
import (
    "flag"
    "fmt"
    "github.com/leaguilar/SimpleExaCT/4_data_assembly/backend/go/storage"
    "github.com/leaguilar/SimpleExaCT/4_data_assembly/backend/go/server"
    "log"
    "net/http"
)

func main() {
    var secure bool
    var testUpload bool

    flag.BoolVar(&secure, "secure", false,"if this flag is set communication happens over an encrypted channel")
    flag.BoolVar(&testUpload, "testUpload", false,"tests if it is able to upload to the S3 bucket")

    flag.Parse()

    // Tests if S3 bucket is accessible
    if testUpload {
        cl:=storage.NewAwsClient()
        testData:="This is a test"
        byteData:=[]byte(testData)
        err := cl.AddFileBufferToS3(byteData,int64(len(testData)),"test.txt")
        if err != nil {
            fmt.Println(err)
            panic(err)
        }
    }


    var outputDir string ="results/"
    var threshold float64 = 100
    server := server.NewServer(outputDir,threshold)
    server.MonitorDirectories()
    // Make a Regex to say we only want letters and numbers

    if !secure {
        http.HandleFunc("/", server.GetJson)
     
        fmt.Printf("PORT: 8080 Starting server for receiving JSON POSTs... \n")
        if err := http.ListenAndServe(":8080", nil); err != nil {
            log.Fatal(err)
        }
    }else{
        http.HandleFunc("/secure", server.GetJsonHTTPS)
        fmt.Printf("PORT: 8443 PATH:/secure Starting server for receiving JSON POSTs... \n")
        err := http.ListenAndServeTLS(":8443", "server.crt", "server.key", nil)
        if err != nil {
            log.Fatal("ListenAndServe: ", err)
        }
    }
    //
}


