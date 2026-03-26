package storage

import (
    "bytes"
    "fmt"
    "github.com/aws/aws-sdk-go/aws"
    "github.com/aws/aws-sdk-go/aws/credentials"
    "github.com/aws/aws-sdk-go/aws/session"
    "github.com/aws/aws-sdk-go/service/s3"
    "log"
    "net/http"
    "os"
)

type aws_client struct{
    s *session.Session   
}


// TODO Better use environment variables instead of hardcoding values
var (
    S3_REGION = ""
    S3_BUCKET = ""
    S3_ACCESS_KEY_ID     = ""
    S3_SECRET_ACCESS_KEY = ""
)

func (cl *aws_client) initClient() {

    if S3_REGION == "" || S3_BUCKET == "" || S3_ACCESS_KEY_ID =="" || S3_SECRET_ACCESS_KEY ==""  {
        S3_REGION=os.Getenv("S3_REGION")
        S3_BUCKET=os.Getenv("S3_BUCKET")
        S3_ACCESS_KEY_ID=os.Getenv("S3_ACCESS_KEY_ID")
        S3_SECRET_ACCESS_KEY=os.Getenv("S3_SECRET_ACCESS_KEY")
    }

    
    // Creating AWS session
    s, err := session.NewSession(&aws.Config{
        Region: aws.String(S3_REGION),
        Credentials: credentials.NewStaticCredentials(S3_ACCESS_KEY_ID,S3_SECRET_ACCESS_KEY, ""),
    })
    
    if err != nil {
        log.Fatal(err)
    }
    cl.s=s
}

// AddFileToS3 will upload a single file to S3, it will require a pre-built aws session
// and will set file info like content type and encryption on the uploaded file.
func (cl *aws_client) AddFileToS3(fileDir string,targetLocation string) error {

    // Open the file for use
    file, err := os.Open(fileDir)
    if err != nil {
        fmt.Println("Couldn't find file")
        
        return err
    }
    defer file.Close()

    // Get file size and read the file content into a buffer
    fileInfo, _ := file.Stat()
    var size int64 = fileInfo.Size()
    buffer := make([]byte, size)
    file.Read(buffer)

    // Config settings: this is where you choose the bucket, filename, content-type etc.
    // of the file you're uploading.
    resp, err2 := s3.New(cl.s).PutObject(&s3.PutObjectInput{
        Bucket:               aws.String(S3_BUCKET),
        Key:                  aws.String(targetLocation),
        ACL:                  aws.String("private"),
        Body:                 bytes.NewReader(buffer),
        ContentLength:        aws.Int64(size),
        ContentType:          aws.String(http.DetectContentType(buffer)),
    })
    fmt.Println(resp)
    return err2
}

// AddFileToS3 will upload a single file to S3, it will require a pre-built aws session
// and will set file info like content type and encryption on the uploaded file.
func (cl *aws_client) AddFileBufferToS3(buffer []byte, size int64,targetLocation string) error {
    // Config settings: this is where you choose the bucket, filename, content-type etc.
    // of the file you're uploading.
    resp, err2 := s3.New(cl.s).PutObject(&s3.PutObjectInput{
        Bucket:               aws.String(S3_BUCKET),
        Key:                  aws.String(targetLocation),
        ACL:                  aws.String("private"),
        Body:                 bytes.NewReader(buffer),
        ContentLength:        aws.Int64(size),
        ContentType:          aws.String(http.DetectContentType(buffer)),
    })
    fmt.Println(resp)
    return err2
}

func NewAwsClient() (cl *aws_client) {
    cl = new(aws_client)
    cl.initClient()
    fmt.Println("SUCCESS")
    return cl
}




