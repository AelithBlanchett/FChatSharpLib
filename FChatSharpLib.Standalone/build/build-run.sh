#!/bin/bash
environment=$1
lower_environment=$(echo "$environment" | awk '{print tolower($0)}')
cwd="$( cd -- "$(dirname "$0")" >/dev/null 2>&1 ; pwd -P )"

#Updating
git pull

#Compiling .NET Host
cd $cwd/..
dotnet publish -c Release -r linux-x64

# Creating required directories
mkdir -p /var/www/vc/$lower_environment/fchatbot/

# Stop running service
service fchatbot.$lower_environment stop

# Removing files from destination
rm -rf /var/www/vc/$lower_environment/fchatbot/*

# Copying files to destination
cp -rf $cwd/../bin/Release/netcoreapp2.1/linux-x64/publish/* /var/www/vc/$lower_environment/fchatbot/

# Creating log dir and file
mkdir -p /var/www/vc/$lower_environment/fchatbot/Logs/
chown -R cityseventeen:cityseventeen /var/www/vc/$lower_environment/fchatbot/Logs/
chmod -R 775 /var/www/vc/$lower_environment/fchatbot/Logs/
touch /var/www/vc/$lower_environment/fchatbot/Logs/logs.txt
chown -R cityseventeen:cityseventeen /var/www/vc/$lower_environment/fchatbot/Logs/

# ONCE - Copy service files
#cp -rf $cwd/../config/systemd/*$lower_environment*.service /etc/systemd/system/

#Return to initial directory
cd $cwd

# ONCE - Enable new Service files and Apache files
#systemctl daemon-reload

# Start service
service fchatbot.$lower_environment start