# Unity Mouse Brain Renderer

<p float="left">
 <img src="https://github.com/dbirman/UMRenderer/raw/main/Examples/gallery/flatmap_layout.png" width="300"> 
 <img src="https://github.com/dbirman/UMRenderer/raw/main/Examples/gallery/RS_fig1.png " width="425">
</p>

This project allows you to connect your Python scripts to a standalone "mouse brain renderer" program, to create graphics like the ones above. As a user, you only need to read the first set of instructions below (install + instructions).

# Install

The fastest way to get running is to open http://data.virtualbrainlab.org/UMRenderer in a browser window.

You can also download the latest standalone desktop app for Windows from the [releases page](https://github.com/dbirman/UMRenderer/releases). Linux and Mac executables are available by request.

# Instructions

Open either the web app page and allow it to load or run the desktop client. The desktop client will login automatically using your username (the python package will do the same). **Note:** If you are using the *web* app you will need to set your username manually. Press `I` and enter your account username. If you aren't sure what that is, try running:

```
python
import os
os.getlogin()
```

For now to import the python package you need clone this repository and write your code within the repository folder. See the Examples folder for some Jupyter notebook examples of how this works, as well as how to load data and render it. The minimal requirement is to run:

```
import unitymouse.render as umr
umr.setup()
```

If you want to be a good citizen you can also call `umr.close()` to close your connection.

### Interaction

Left click + drag along the Y axis to pitch the brain

Left click + drag along the X axis to yaw the brain

Hold shift while left clicking and dragging along the X axis to spin the brain

Scroll to zoom

Right click + drag to pan

## Render calls

Once you have imported `umr` you can use the following calls to display data. Refer to the `unitymouse/render.py` file for detailed instructions about what data format to use for each call.

### CCF 3D Volumes

`umr.set_volume_visibility` - pass a dictionary of area acronyms/IDs keys and boolean visibility values to load 3D meshes **note:** this must be called before you use any of the subsequent functions.

`umr.set_volume_color` - pass a dictionary of area acronyms/IDs and hex color strings to set the color of each 3D mesh.

`umr.set_volume_intensity` - pass a dictionary of area acronyms/IDs and floats to set the intensity according to the currently loaded colormap.

`umr.set_volume_colormap` - pass a string to set the current colormap (options: 'cool', 'gray')

`umr.set_volume_style` - pass a dictionary of area acronyms/IDs and strings to set the display style (options: 'whole', 'left', 'right')

`umr.set_volume_alpha` - pass a dictionary of area acronyms/IDs and floats to set transparency, only works with the transparent shaders

`umr.set_volume_shader` - pass a dictionary of area acronyms/IDs and strings to set shaders (options: 'default', 'toon', 'toon-outline', 'transparent-lit', 'transparent-unlit')

### Neurons

`umr.create_neurons` - pass a list of neuron names as strings to create new neurons

`umr.set_neuron_positions` - pass a dictionary of neuron names and lists of 3 floats to set neuron coordinates in ML/AP/DV space, units in um

`umr.set_neuron_size` - pass a dictionary of neuron names and floats to set neuron scale, units in um

`umr.set_neuron_color` - pass a dictionary of neuron names and hex colors

`umr.set_neuron_shape` - pass a dictionary of neuron names and strings (options: sphere, cube), use cube if you are displaying more than a few thousand objects

### Probes

`umr.create_probes` - pass a list of probe names as strings to create new probes

`umr.set_probe_positions` - pass a dictionary of probe names and a list of 3 floats to probe tip coordinates in ML/AP/DV space, units in um

`umr.set_probe_angles` - pass a dictionary of probe names and a list of 3 floats to set probe angles [Azimuth (phi), Elevation (theta), Spin (beta)]

`umr.set_probe_size` - pass a dictionary of probe names and a list of 3 floats to set probe size in width / height / depth (default: [0.07, 3.84, 0.02])

## Settings

In the application settings (which are open by default) you can "explode" the brain using the slider option. You can explode all areas, or just the cortex and hippocampus "vertically" sort of like a nested doll. You can also switch from exploding all areas to just the left side, as well as set the colors to the defaults on the right side of the brain. 

The index slider is not implemented.

# Developer Instructions

## Adding new functionality

[todo]

## Deploying the server

### For local testing

### On Heroku

The node.js server `Server/server.js` handles client/renderer connections and passing messages between them. It is deployed automatically on Heroku when the github app builds. 
