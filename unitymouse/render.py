import socketio
import os

sio = socketio.Client()
@sio.event
def connect():
    print("UnityMouse Renderer connected to server")
@sio.event
def disconnect():
    print("UnityMouse Renderer disconnected from server")

def setup():
	"""Connect to the heroku server and provide a login ID"""
	sio.connect('https://um-commserver.herokuapp.com/')
	ID = os.getlogin()
	sio.emit('ID',ID)

def close():
	"""Disconnect from the heroku server"""
	sio.disconnect()

def set_volume_visibility(areaData):
	"""Set visibility of CCF volume regions

	NOTE: this must be called before setting color/intensity/etc
	in addition, this is an *asynchronous* call

	Inputs:
	areaData -- dictionary of area ID or acronym and bool values {'root':True} or {8:True}
	"""
	sio.emit('SetVolumeVisibility', areaData)

def set_volume_color(areaData):
	"""Set color of CCF volume regions

	Inputs:
	areaData -- dictionary of area ID or acronym and hex colors {'root':"#FFFFFF"} or {8:"#FFFFFF"}
	"""
	sio.emit('SetVolumeColors', areaData)

def set_volume_intensity(areaData):
	"""Set color of CCF volume regions according to intensity along a color map

	Inputs:
	areaData -- dictionary of area ID or acronym and hex colors {'root':"#FFFFFF"} or {8:"#FFFFFF"}
	"""
	sio.emit('SetVolumeIntensity', areaData)

def set_volume_colormap(colormap_name):
	"""Set colormap name

	Options are:
		cool (default, teal 0 -> magenta 1)

	Inputs:
	colormap_name -- string
	"""
	sio.emit('SetVolumeColormap', colormap_name)

def set_volume_style(areaData):
	"""Set the object style of the volumes

	Options are:
		whole (default)
		left
		right

	Inputs:
	areaData -- dictionary of area ID/acronym and string {'root':'left'}
	"""
	sio.emit('SetVolumeStyle', areaData)

def set_volume_alpha(areaData):
	"""Set alpha of CCF volume regions

	Inputs:
	areaData -- dictionary of area ID or acronym and float {'root':0.5} or {8:0.5}
	"""
	sio.emit('SetVolumeAlpha', areaData)

def set_volume_shader(areaData):
	"""Set shader of CCF volume regions

	Shader options are:
		"opaque-unlit"
		"opaque-unlit-outline"
		"toon"
		"toon-outline"

	Inputs:
	areaData -- dictionary of area ID or acronym and string {'root':0.5} or {8:0.5}
	"""
	sio.emit('SetVolumeShader', areaData)

def create_neurons(neuronList):
	"""Create neuron objects

	NOTE: This must be called before setting positions/size/color/shape

	Inputs:
	probeList -- list of neuron names as strings ['n1','n2','n3']
	"""
	sio.emit('CreateNeurons', neuronList)

def set_neuron_positions(neuronData):
	"""Set positions of neurons in ML/AP/DV space in um units relative to CCF [0,0,0] coordinate

	Inputs:
	neuronData -- dictionary of neuron names and list of coordinates as values {'n1':[500,1500,1800]}
	"""
	sio.emit('SetNeuronPos', neuronData)

def set_neuron_size(neuronData):
	"""Set size of neurons in um units

	Inputs:
	neuronData -- dictionary of neuron names and floats {'n1': 20}
	"""
	sio.emit('SetNeuronSize', neuronData)

def set_neuron_color(neuronData):
	"""Set size of neurons in um units

	Inputs:
	neuronData -- dictionary of neuron names and floats {'n1': 20}
	"""
	sio.emit('SetNeuronColor', neuronData)

def set_neuron_shape(neuronData):
	"""Change the neuron mesh

	Options:
		sphere (default)
		cube (lower resolution option with better performance)

	Inputs:
	neuronData -- dictionary of neuron names and strings {'n1':'sphere'}
	"""
	sio.emit('SetNeuronShape', neuronData)

def slice_volume(slicePosition):
	"""Sets the slice plane position and normal vector direction. The brain will be sliced in FRONT of the plane.

	Inputs:
	slicePosition -- float6 (x0,y0,z0,xn,yn,zn)
	"""
	sio.emit('SliceVolume', slicePosition)

def set_slice_annotation_color(annotationData):
	"""Set the color of the annotation dataset areas on the slice

	Inputs:
	annotationData -- dictionary of acronyms/IDs and hex color codes {'root':'#FFFFFF'}
	"""
	sio.emit('SetSliceColor', annotationData)

def create_probes(probeList):
	"""Create probe objects

	Inputs:
	probeList -- list of probe names ['p1','p2']
	"""
	sio.emit('CreateProbes', probeList)

def set_probe_colors(probeData):
	"""Set probe object colors

	Inputs:
	probeData -- dictionary of probe names and color hex codes {'p1':'#FFFFFF'}
	"""
	sio.emit('SetProbeColors', probeData)

def set_probe_positions(probeData):
	"""Set probe object ML/AP/DV positions in um relative to the CCF (0,0,0) point

	Inputs:
	probeData -- dictionary of probe names and float3 list {'p1':[500, 500, 500]}
	"""
	sio.emit('SetProbePos', probeData)

def set_probe_angles(probeData):
	"""Set probe azimuth/elevation/spin angles in degrees
	Azimuth 0 = AP axis, + rotates clockwise
	Elevation 0 = Vertical, 90 = Horizontal

	Inputs:
	probeData -- dictionary of probe names and float3 list {'p1':[-90,0,0]}
	"""
	sio.emit('SetProbeAngles', probeData)

def set_probe_style(probeData):
	"""Set probe rendering style

	Style options are:
		"line"
		"probe-tip"
		"probe-silicon"
		"probe"

	Inputs:
	probeData -- dictionary of probe names and string {'p1':'line'}
	"""
	sio.emit('SetProbeStyle', probeData)