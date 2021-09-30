# Gemini Client


## Steps to generate X509 certificate
 - Generate a Private Key
   - openssl genrsa -des3 -out server.key 1024
 - Generate a self-signed CSR
   - openssl req -new -key server.key -out server.csr
 - Remove passphrase from Key
   - cp server.key server.key.org
   - openssl rsa -in server.key.org -out server.key
 - Generate a self-signed certificate
   - openssl x509 -req -days 365 -in server.csr -signkey server.key -out server.crt