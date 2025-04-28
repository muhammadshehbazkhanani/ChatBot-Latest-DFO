#!/bin/sh
# Start .NET server in background
dotnet AngularApp1.Server.dll --urls=http://+:8080 &
# Start Nginx for Angular in foreground
nginx -g 'daemon off;'