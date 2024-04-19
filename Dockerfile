from mcr.microsoft.com/dotnet/sdk:6.0.420-alpine3.18-amd64

run yes | apk add bash chromium
run yes | apk add coreutils 

RUN addgroup -S appgroup --gid 1000 && adduser -S appuser --uid 1000 -G appgroup
run chown -R appuser:appgroup /root
run cd /root
workdir /root

user appuser

cmd /bin/bash
