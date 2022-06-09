import socketio
import webbrowser
import os

sio = socketio.Client()
@sio.event
def connect():
	print("UnityMouse Renderer connected to server")
	ID = os.getlogin()
	change_id(ID)

@sio.event
def disconnect():
    print("UnityMouse Renderer disconnected from server")

def setup(localhost = False, standalone = False):
	"""Connect the unitymouse Python API to the web-based (or standalone) viewer

	Parameters
	----------
	localhost : bool, optional
		connect to a local development server rather than the remote server, by default False
	standalone : bool, optional
		connect to a standalone Desktop build rather than the web-based Brain Viewer, by default False
	"""
	ID = os.getlogin()

	if localhost:
		sio.connect('http://localhost:5000')
	else:
		sio.connect('https://um-commserver.herokuapp.com/')

	if not standalone:
		url = "http://data.virtualbrainlab.org/UMRenderer/?ID=" + ID
		webbrowser.open(url)

def close():
	"""Disconnect from the heroku server"""
	sio.disconnect()

def change_id(newID):
	sio.emit('ID',[newID,"send"])
	print("Login sent with ID: " + newID)


#######################
# ALLEN CCF 3D MODELS #
#######################

def load_beryl_areas():
	"""Load all beryl areas and set visibility to True

	NOTE: One of the load functions OR set_area_visibility must be called
	before you set the color/alpha/etc of brain regions.
	"""
	sio.emit('LoadDefaultAreas', 'beryl')

def load_cosmos_areas():
	"""Load all cosmos areas and set visibility to True

	NOTE: One of the load functions OR set_area_visibility must be called
	before you set the color/alpha/etc of brain regions.
	"""
	sio.emit('LoadDefaultAreas', 'cosmos')

def set_area_visibility(areaData):
	"""Set visibility of CCF volume regions

	Note: use "-l" or "-r" suffix to control visibility of one-sided models

	Parameters
	----------
	areaData : dict {string : bool}
		Dictionary of area IDs or acronyms and visibility values

	Examples
	--------
	>>> umr.set_area_visibility({"root":True})
	>>> umr.set_area_visibility({"8":True})
	>>> umr.set_area_visibility({"VISp-l":True})
	"""
	sio.emit('SetAreaVisibility', areaData)

def set_area_color(areaData):
	"""Set color of CCF volume regions

	Use "-l" and "-r" to set color of single-sided regions.

	Inputs:
	areaData -- dictionary of area ID or acronym and hex colors {'root':"#FFFFFF"} or {8:"#FFFFFF"}
	"""
	sio.emit('SetAreaColors', areaData)

def set_area_intensity(areaData):
	"""Set color of CCF volume regions according to intensity along a color map

	Use "-l" and "-r" to set color of single-sided regions.

	Inputs:
	areaData -- dictionary of area ID or acronym and hex colors {'root':"#FFFFFF"} or {8:"#FFFFFF"}
	"""
	sio.emit('SetAreaIntensity', areaData)

def set_area_colormap(colormap_name):
	"""Set colormap name

	Options are:
		cool (default, teal 0 -> magenta 1)
		gray (black 0 -> white 1)

	Inputs:
	colormap_name -- string
	"""
	sio.emit('SetAreaColormap', colormap_name)

# def set_volume_explode_style

def set_area_alpha(areaData):
	"""Set alpha of CCF volume regions

	Use "-l" and "-r" to set color of single-sided regions.

	Inputs:
	areaData -- dictionary of area ID or acronym and float {'root':0.5} or {8:0.5}
	"""
	sio.emit('SetAreaAlpha', areaData)

def set_area_shader(areaData):
	"""Set shader of CCF volume regions

	Use "-l" and "-r" to set color of single-sided regions.

	Shader options are:
		"default"
		"toon"
		"toon-outline"
		"transparent-lit"
		"transparent-unlit"

	Inputs:
	areaData -- dictionary of area ID or acronym and string {'root':0.5} or {8:0.5}
	"""
	sio.emit('SetAreaShader', areaData)

# def set_volume_data(areaData):
# 	"""Set data values for volumes (0->1). These are interpreted in the same way that the
# 	set_area_intensity function works, but there is a slider in the settings that
# 	lets you move through the index position of the values.

# 	Inputs:
# 	areaData -- dictionary of area ID or acronym and list of floats {'root':[0,0.5,1.0]}
# 	"""
# 	sio.emit('SetVolumeData', areaData)


###########
# NEURONS #
###########

def create_neurons(neuronList):
	"""Create neuron objects

	Note: neurons must be created before setting other values

	Parameters
	----------
	neuronList : List of strings
		Names of the new neuron objects

	Examples
	--------
	>>> umr.create_neurons(["n1","n2","n3"])
	"""
	sio.emit('CreateNeurons', neuronList)

def set_neuron_positions(neuronData):
	"""Set positions of neurons in ML/AP/DV space in um units relative to CCF [0,0,0] coordinate

	Inputs:
	neuronData -- dictionary of neuron names and list of coordinates as values {'n1':[500,1500,1800]}
	"""
	sio.emit('SetNeuronPos', neuronData)

def set_neuron_size(neuronData):
	"""Set size of neurons in mm units

	Inputs:
	neuronData -- dictionary of neuron names and floats {'n1': 20}
	"""
	sio.emit('SetNeuronSize', neuronData)

def set_neuron_color(neuronData):
	"""Set color of neurons

	Inputs:
	neuronData -- dictionary of neuron names and hex colors {'n1': '#FFFFFF'}
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

# def set_probe_style(probeData):
# 	"""Set probe rendering style

# 	Style options are:
# 		"line"
# 		"probe-tip"
# 		"probe-silicon"
# 		"probe"

# 	Inputs:
# 	probeData -- dictionary of probe names and string {'p1':'line'}
# 	"""
# 	sio.emit('SetProbeStyle', probeData)

def set_probe_size(probeData):
	"""Set probe rendering style

	By default probes are scaled to their real size (70um wide x 20um deep x 3.84 mm tall)

	Inputs:
	probeData -- dictionary of probe names and float3 {'p1':[0.07, 3.84, 0.02]}
	"""
	sio.emit('SetProbeSize', probeData)

##########
# CAMERA #
##########

def set_camera_target(cameraData):
	"""Set camera angle around the Y (vertical) axis

	By default the camera is centered at the center point of the CCF space

	Inputs:
	cameraData -- list containing the CCF coordinate in mm to target ap/dv/lr, default [5.7, 4, 6.6]
	"""
	sio.emit('SetCameraTarget', cameraData)

# def set_camera_target_area(cameraData):
# 	"""Set camera angle around the Y (vertical) axis

# 	By default the camera is centered at the center point of the CCF space. Append "-l" or "-r"
# 	to target only the left hemisphere or right hemisphere areas. Note that area centers come from
# 	the area mesh models, which for some areas (especially long/curved areas) will not be where
# 	you think the center "should" be, so to speak. For those areas, calculate a center yourself
# 	and use set_camera_target([ap,dv,lr])

# 	Inputs:
# 	cameraData -- string area name or acronym e.g. 'HIP' or 'HIP-l'
# 	"""
# 	sio.emit('SetCameraTargetArea', cameraData)

def set_camera_y_angle(cameraData):
	"""Set camera angle around the Y (vertical) axis

	Inputs:
	cameraData -- degree value to set the camera angle to, e.g. 0->360
	"""
	sio.emit('SetCameraYAngle', cameraData)

###########
# VOLUMES #
###########

def create_new_volume(volumeData):

	# volumeData is [name, width, height, depth]
	sio.emit('CreateVolume', volumeData)

def set_volume_visibility(volumeVisibility):

	# volumeVisibility is [string name, bool visibility]
	sio.emit('SetVolumeVisibility', volumeVisibility)

def set_volume_data(volumeData):
	"""Set the data for a volume

	Note: this function slices the data by depth and sends data slice-by-slice

	Parameters
	----------
	volumeData : [string, numpy matrix]
		Name of the volume and the volume itself
	"""
	name = volumeData[0]
	for di in range(volumeData[1].size[2]):
		set_volume_slice_data([name, di, volumeData[:,:,di]])

def set_volume_slice_data(volumeData):
	"""Set the data for a specific slice in a volume

	Parameters
	----------
	volumeData : [string, int, numpy matrix]
		Name of the volume, depth index in volume, slice data
	"""
	# not sure yet what the data size limit is, but lets add one slice at a time for now
	# volumeData is float array [name, depth, n floats where n = width * height]
	sio.emit('SetVolumeData', volumeData)

######################
# VOLUMES: Allen CCF #
######################

# special case of the regular volume visibility function
def set_allen_volume_visibility(allenVisibility):
	"""_summary_

	Parameters
	----------
	allenData : bool
		new visibility setting for Allen CCF volume
	"""
	set_volume_visibility(['allen', allenVisibility])

# def set_allen_annotation_color(annotationData):
# 	"""Set the color of the annotation dataset areas on the slice

# 	Inputs:
# 	annotationData -- dictionary of acronyms/IDs and hex color codes {'root':'#FFFFFF'}
# 	"""
# 	sio.emit('SetSliceColor', annotationData)

def clear():
	sio.emit('Clear', 'all')

def clear_neurons():
	sio.emit('Clear', 'neurons')

def clear_probes():
	sio.emit('Clear', 'probes')

def clear_areas():
	sio.emit('Clear', 'areas')

def clear_volumes():
	sio.emit('Clear', 'volumes')