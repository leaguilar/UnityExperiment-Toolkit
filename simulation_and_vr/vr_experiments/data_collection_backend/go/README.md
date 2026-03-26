# Run 
go run main.go

# S3 Credentials

# Option A (recommended)
export as environmental variables

    export S3_REGION="us-east-2"
    export S3_BUCKET="myBucket"
    export S3_ACCESS_KEY_ID="AAAAAAAAAAAAAAAAA"
    export S3_SECRET_ACCESS_KEY="BBBBBBBBBBBBBBBBB"

# Option B (not recommended)
Hardcode your credentials in storage.go

## Test if you have the correct credentials
go run main.go -testUpload


# OPTIONAL

# Run
go run main.go -secure

## To use over an encrypted channel (https)

Generate private key (.key)
### Key considerations for algorithm "RSA" ≥ 2048-bit
openssl genrsa -out server.key 2048

#### Key considerations for algorithm "ECDSA" (X25519 || ≥ secp384r1), https://safecurves.cr.yp.to/ List ECDSA the supported curves (openssl ecparam -list_curves)

## Generate Key
openssl ecparam -genkey -name secp384r1 -out server.key

## Generate Cert
Generation of self-signed(x509) public key (PEM-encodings .pem|.crt) based on the private (.key)
openssl req -new -x509 -sha256 -key server.key -out server.crt -days 3650
