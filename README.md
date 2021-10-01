# Gemini Client

## Description
The gemini protocol intends to provide a significantly simplified mechanism for transmitting web content.  You can read more about it here: http://gemini.circumlunar.space/docs/

This project is a command line application which will fetch gemini content for you.  Gemini requires a TLS certificate, see instructions below on how to create one.

## Usage

```
Gemini
  A simple gemini protocol client

Usage:
  Gemini [options] <url>

Arguments:
  <url>  The gemini url to fetch

Options:
  --cert <cert>          The X509 certificate to use
  --password <password>  The password for the X509 certificate
  --verbose              Show debug information
  --version              Show version information
  -?, -h, --help         Show help and usage information
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