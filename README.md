# Gemini Client

## Description

The gemini protocol intends to provide a significantly simplified mechanism for transmitting web content.  You can read more about it here: [http://gemini.circumlunar.space/docs/](http://gemini.circumlunar.space/docs/)

This project is a command line application which will fetch gemini content for you.  Gemini requires a TLS certificate, see instructions below on how to create one.

## Help screen

``` console
Gemini
  A simple gemini protocol client

Usage:
  Gemini [options] <url> <certificatePath>

Arguments:
  <url>              The gemini url to fetch
  <certificatePath>  The path to the X509 certificate to use

Options:
  --password <password>  The password for the X509 certificate
  --verbose              Show debug information
  --version              Show version information
  -?, -h, --help         Show help and usage information
```

## Usage

Fetch gemini content using default certificate (assumes that GeminiClient.crt) exists in the current location

``` bash
gemini gemini://gemini.circumlunar.space/
```

Use verbose flag if you want to see timing and headers.

``` bash
gemini gemini://gemini.circumlunar.space/ --verbose
```

Or if a password is needed for the certificate

``` bash
gemini gemini://gemini.circumlunar.space/ --password abc123
```

Fetch gemini content with supplied certificate

``` bash
gemini gemini://gemini.circumlunar.space/ "C:\temp\GeminiClient.crt"
```

## Steps to generate X509 certificate

- Generate a Private Key
  - openssl genrsa -des3 -out GeminiClient.key 1024
- Generate a self-signed CSR
  - openssl req -new -key GeminiClient.key -out GeminiClient.csr
- Remove passphrase from Key
  - cp GeminiClient.key GeminiClient.key.org
  - openssl rsa -in GeminiClient.key.org -out GeminiClient.key
- Generate a self-signed certificate
  - openssl x509 -req -days 365 -in GeminiClient.csr -signkey GeminiClient.key -out GeminiClient.crt
- Optionally copy the new certificate to wherever you have placed the Gemini executable to be used as the default cert when a --cert argument is not provided.
