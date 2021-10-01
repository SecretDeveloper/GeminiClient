# Gemini Client

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