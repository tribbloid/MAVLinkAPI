#!/bin/bash

# This script downloads the ArduPilot SITL binary for Plane and makes it executable.

echo "Downloading ArduPilot SITL..."
wget https://firmware.ardupilot.org/Plane/stable/SITL_x86_64_linux_gnu/arduplane -O arduplane

if [ $? -ne 0 ]; then
    echo "Download failed. Please check your internet connection or the URL."
    exit 1
fi

echo "Download complete. Making the file executable..."
chmod +x arduplane

echo "Setup complete. 'arduplane' is ready to be used."
