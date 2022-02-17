# UnityMouse

This repository lets you run a standalone graphics application that renders CCF allen atlas brain regions in user-controllable colors. It has two parts: a graphics application (UnityMouseRenderer.exe) and some simple Python code to connect to and control the graphics app. As a user you don't need to bother with the server, unless you run into issues.

## Unity renderer instructions

Download the latest Windows, Linux, or Mac release from the releases page (on the right side) and unzip the download.

### Windows

Run the executable (UnityMouseRenderer.exe)

### Linux

On Linux you need to tell the operating system that the x86_64 file is an executable. Set this by running: `chmod +x UnityMouseRender_Linux.x86_64` inside the unzipped folder. Then run the executable.

You may also run into permissions issues with the data folder. If the program won't run even after setting it as executable you need to do `chown -R yourusername .` in the same folder, to set the Data folder's ownership to your own user account.

### MacOS

Please email Dan for a build.

### Client ID

The renderer automatically connects to the server using your username as the client ID. If you have issues with this press `I` and enter a different client ID.

## Python client instructions

Install socketio: `pip install "python-socketio[client]"`

Basic test code, assuming you have already started a renderer.

```
import socketio
import time
import os

sio = socketio.Client()
sio.connect('https://um-commserver.herokuapp.com/')
sio.emit('ID',os.getlogin())
time.sleep(1)

vis = {'MDRN': True,
 'root': True,
 'ECU': True,
 'PYR': True,
 'LP': True,
 'CA1': True}
data = {'MDRN': 0.66,
 'root': 0.5,
 'ECU': 0.61,
 'PYR': 0.65,
 'LP': 0.55,
 'CA1': 0.59}

sio.emit('UpdateVisibility', vis)
time.sleep(1)
sio.emit('UpdateIntensity', data)
time.sleep(1)
sio.disconnect()
```

Some explanation: for the Python client, once you connect to the server at https://um-commserver.herokuapp.com/ you need to send an ID message to link the python session with the renderer session. Then you can send update commands, in this case we're updating the visibility of a group of brain areas and then their colors, according to the results of some analysis.

# Usage

## Messages

UpdateVisibility: Pass a dictionary with acronyms or area IDs as keys and bools as values

UpdateIntensity: Pass a dictionary with acronyms or area IDs as keys and floats as values

UpdateColors: Pass a dictionary with acronyms or area IDs as keys and hex code strings (i.e. `"#FFFFFF"`) as values

## Interaction

Left click + drag along the Y axis to pitch the brain

Left click + drag along the X axis to yaw the brain

Hold shift while left clicking and dragging along the X axis to spin the brain

Scroll to zoom

Right click + drag to pan

# Coming soon

 - Shader options
 - Brain exploding animations
 - More? Please put feature requests on the issues page or email me directly.

# Node.js server instructions

The node.js server handles client/renderer connections and passing messages between them. It is deployed automatically on Heroku when the github app builds. 
