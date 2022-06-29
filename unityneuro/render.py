import socketio
import webbrowser
import os
import numpy as np

class bcolors:
    WARNING = '\033[93m'
    FAIL = '\033[91m'

sio = socketio.Client()
@sio.event
def connect():
	print("(URN) connected to server")
	ID = os.getlogin()
	change_id(ID)

@sio.event
def disconnect():
    print("(URN) disconnected from server")

log_messages = True
@sio.on('log')
def message(data):
	if log_messages:
		print('(Renderer) ' + data)

@sio.on('log-warning')
def message(data):
	if log_messages:
		print('(Renderer) ' + bcolors.WARNING + data)

@sio.on('log-error')
def message(data):
	print('(Renderer) ' + bcolors.FAIL + data)

def setup(verbose = True, localhost = False, standalone = False):
	"""Connect the Unity Renderer for Neuroscience Python API to the web-based (or standalone) viewer

	Parameters
	----------
	localhost : bool, optional
		connect to a local development server rather than the remote server, by default False
	standalone : bool, optional
		connect to a standalone Desktop build rather than the web-based Brain Viewer, by default False
	"""
	ID = os.getlogin()

	log_messages = verbose

	if localhost:
		sio.connect('http://localhost:5000')
	else:
		sio.connect('https://um-commserver.herokuapp.com/')

	if not standalone:
		url = "http://data.virtualbrainlab.org/UnityNeuro/?ID=" + ID
		webbrowser.open(url)

def close():
	"""Disconnect from the Node.js server
	"""
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

def set_area_visibility(area_visibilities):
	"""Set visibility of CCF area models

	**Note:** you can append a "-lh" or "-rh" suffix to any area acronym or ID to control the visibility of just one-half of the model. This can be used in all of the set_area_* functions.

	Parameters
	----------
	area_visibilities : dict {string : bool}
		dictionary of area IDs or acronyms and visibility values

	Examples
	--------
	>>> urn.set_area_visibility({'grey':True})
	>>> urn.set_area_visibility({8:True})
	>>> urn.set_area_visibility({'VISp-l':True})
	"""
	sio.emit('SetAreaVisibility', area_visibilities)

def set_area_color(area_colors):
	"""Set color of CCF area models.

	Parameters
	----------
	area_colors : dict {string: string}
		Keys are area IDs or acronyms, Values are hex colors

	Examples
	--------
	>>> urn.set_area_color( {'grey':"#FFFFFF"})
	>>> urn.set_area_color({8:"#FFFFFF"})
	>>> urn.set_area_color({"VISp-l":"#00000080"})
	"""
	sio.emit('SetAreaColors', area_colors)

def set_area_intensity(area_intensities):
	"""Set color of CCF area models using colormap.

	Parameters
	----------
	area_intensities : dict {string: float}
		keys are area IDs or acronyms, values are hex colors

	Examples
	--------
	>>> urn.set_area_intensity( {'grey':1.0})
	>>> urn.set_area_intensity({8:1.0})
	"""
	sio.emit('SetAreaIntensity', area_intensities)

def set_area_colormap(colormap_name):
	"""Set colormap used for CCF area intensity mapping


	Options are
	 - cool (default, teal 0 -> magenta 1)
	 - grey (black 0 -> white 1)
	 - grey-green (grey 0, light 1/255 -> dark 1)
	 - grey-purple (grey 0, light 1/255 -> dark 1)
	 - grey-red (grey 0, light 1/255 -> dark 1)

	Parameters
	----------
	colormap_name : string
		colormap name
	"""
	sio.emit('SetAreaColormap', colormap_name)

def set_area_alpha(area_alpha):
	"""Set transparency of CCF area models. 

	Parameters
	----------
	area_alpha : dict {string: float}
		keys are area IDs or acronyms, values are percent transparency

	Examples
	--------
	>>> urn.set_area_alpha( {'grey':0.5})
	>>> urn.set_area_alpha({8:0.5})
	"""
	sio.emit('SetAreaAlpha', area_alpha)

def set_area_material(area_materials):
	"""Set material of CCF area models.

	Material options are
	 - 'opaque-lit' or 'default'
	 - 'opaque-unlit'
	 - 'transparent-lit'
	 - 'transparent-unlit'

	Parameters
	----------
	area_materials : dict {string: string}
		keys are area IDs or acronyms, values are material options
	"""
	sio.emit('SetAreaMaterial', area_materials)

def set_area_data(area_data):
	"""Set the data array for each CCF area model

	Data arrays work the same as the set_area_intensity() function but are controlled by the area_index value, which can be set in the renderer or through the API.

	Parameters
	----------
	area_data : dict {string: float list}
		keys area IDs or acronyms, values are a list of floats
	"""
	sio.emit('SetAreaData', area_data)

def set_area_data_index(area_index):
	"""Set the data index for the CCF area models

	Parameters
	----------
	area_index : int
		data index
	"""
	sio.emit('SetAreaIndex', area_index)

###########
# NEURONS #
###########

def create_neurons(neuron_names):
	"""Create neuron objects

	Note: neurons must be created before setting other values

	Parameters
	----------
	neuron_names : string list
		names of the new neuron objects

	Examples
	--------
	>>> urn.create_neurons(["n1","n2","n3"])
	"""
	sio.emit('CreateNeurons', neuron_names)

def set_neuron_positions(neuron_positions):
	"""Set neuron positions

	Parameters
	----------
	neuron_positions : dict {string: int list}
		keys are neuron names, values are ML/AP/DV coordinates in um units relative to CCF (0,0,0)

	Examples
	--------
	>>> urn.set_neuron_positions({'n1':[500,1500,1800]})
	"""
	sio.emit('SetNeuronPos', neuron_positions)

def set_neuron_sizes(neuron_sizes):
	"""Set size of neuron objects in mm units

	Parameters
	----------
	neuron_sizes : dict {string: float}
		keys are neuron names, values are float size in mm
		
	Examples
	--------
	>>> urn.set_neuron_sizes( {'n1':0.02})
	"""
	sio.emit('SetNeuronSize', neuron_sizes)

def set_neuron_colors(neuron_colors):
	"""Set colors of neuron objects

	Parameters
	----------
	neuron_colors : dict {string: string}
		keys are neuron names, values are hex colors
		
	Examples
	--------
	>>> urn.set_neuron_sizes( {'n1':"#FFFFFF"})
	"""
	sio.emit('SetNeuronColor', neuron_colors)

def set_neuron_shapes(neuron_shapes):
	"""Change the mesh used to render neurons

	Options are
	 - 'sphere' (default)
	 - 'cube' better performance when rendering tens of thousands of neurons

	Parameters
	----------
	neuron_shapes : dict {string: string}
		keys are neuron names, values are shape strings

	Examples
	--------
	>>> urn.set_neuron_shapes( {'n1':'sphere'})
	"""
	sio.emit('SetNeuronShape', neuron_shapes)

def set_neuron_materials(neuron_materials):
	"""Change the material used to render neurons

	Options are
	- 'lit-transparent' (default)
	- 'lit'
	- 'unlit'

	Parameters
	----------
	neuron_materials : dict {string: string}
		keys are neuron names, values are material strings

	Examples
	--------
	>>> urn.set_neuron_materials( {'n1':'lit-transparent'})
	"""
	sio.emit('SetNeuronMaterial', neuron_materials)

##########
# PROBES #
##########

def create_probes(probe_names):
	"""Create probe objects

	Parameters
	----------
	probe_names : string list
		list of names of new probes to create

	Examples
	--------
	>>> urn.create_probes(['p1'])
	"""
	sio.emit('CreateProbes', probe_names)

def set_probe_colors(probe_colors):
	"""Set colors of probe objects

	Parameters
	----------
	probe_colors : dict {string: string}
		key is probe name, value is hex color

	Examples
	--------
	>>> urn.set_probe_colors({'p1':'#FFFFFF'})
	"""
	sio.emit('SetProbeColors', probe_colors)

def set_probe_positions(probe_positions):
	"""Set probe tip position in ml/ap/dv coordinates in um relative to the CCF (0,0,0) point

	Parameters
	----------
	probe_positions : dict {string: float list}
		key is probe name, value is list of floats in ml/ap/dv in um

	Examples
	--------
	>>> urn.set_probe_positions({'p1':[500,1500,2500]})
	"""
	sio.emit('SetProbePos', probe_positions)

def set_probe_angles(probe_angles):
	"""Set probe azimuth/elevation/spin angles in degrees

	Azimuth 0 = has the probe facing the AP axis, positive values rotate clockwise
	Elevation 0 = probe is vertical, 90 = horizontal

	Parameters
	----------
	probe_angles : dict {string: float list}
		key is probe name, value is list of floats in az/elev/spin
		
	Examples
	--------
	>>> urn.set_probe_angles({'p1':[-90,0,0]})
	"""
	sio.emit('SetProbeAngles', probe_angles)

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

def set_probe_size(probe_size):
	"""Set probe scale in mm units, by default probes are scaled to 70 um wide x 20 um deep x 3840 um tall which is the size of a NP 1.0 probe.

	Parameters
	----------
	probe_size : dict {string: float list}
		key is probe name, value is list of floats for width, height, depth
		
	Examples
	--------
	>>> urn.set_probe_size({'p1':[0.070, 3.840, 0.020]})
	"""
	sio.emit('SetProbeSize', probe_size)

##########
# CAMERA #
##########

def set_camera_target(camera_target_coordinate):
	"""Set the camera target coordinate in CCF space in um relative to CCF (0,0,0), without moving the camera. Coordinates can be negative or larger than the CCF space (11400,13200,8000)

	Parameters
	----------
	camera_target_coordinate : float list
		list of coordinates in ml/ap/dv in um

	Examples
	--------
	>>> urn.set_camera_target([500,1500,1000])
	"""
	sio.emit('SetCameraTarget', camera_target_coordinate)

def set_camera_position(camera_pos, preserve_target = True):
	"""Set the camera position in CCF space in um relative to CCF (0,0,0), coordinates can be outside CCF space. 

	Parameters
	----------
	camera_pos : float list
		list of coordinates in ml/ap/dv in um
	preserve_target : bool, optional
		when True keeps the camera aimed at the current target, when False preserves the camera rotation, by default True
	
	Examples
	--------
	>>> urn.set_camera_position([500,1500,1000])
	"""
	packet = camera_pos.copy()
	packet.append(preserve_target)
	sio.emit('SetCameraPosition', packet)

def set_camera_rotation(pitch, yaw, spin):
	"""Set the camera rotation (pitch, yaw, spin). The camera is locked to a target, so this rotation rotates around the target.

	Rotations are applied in order: pitch, then yaw, then spin.

	Parameters
	----------
	camera_rot : float list
		list of euler angles to set the camera rotation in (pitch, yaw, spin)
	"""
	sio.emit('SetCameraRotation', [pitch, yaw, spin])

def set_camera_zoom(zoom):
	"""Set the camera zoom. 

	Parameters
	----------
	zoom : float	
		camera zoom parameter
	"""
	sio.emit('SetCameraZoom', zoom)

def set_camera_target_area(camera_target_area):
	"""Set the camera rotation to look towards a target area

	Note: some long/curved areas have mesh centers that aren't the 'logical' center. For these areas, calculate a center yourself and use set_camera_target.

	Parameters
	----------
	camera_target_area : string
		area ID or acronym, append "-lh" or "-rh" for one-sided meshes
	"""
	sio.emit('SetCameraTargetArea', camera_target_area)

###########
# VOLUMES #
###########

def create_volume(volume_name):
	"""Create an empty volume data matrix in the renderer.

	Note: you must call create_volume and set_volume_colormap before setting data.

	Parameters
	----------
	volume_name : string
		volume name

	Examples
	--------
	>>> urn.create_volume('histology')
	"""
	sio.emit('CreateVolume', volume_name)

	
def set_volume_colormap(volume_name, colormap):
	"""Set the colormap for a volume, maximum of 254 values

	Note: index 255 is reserved by the renderer for transparency.

	Parameters
	----------
	volumeName : string
		volume name
	volumeData : list of string
		list of hex colors, values can be RGB or RGBA
		
	Examples
	--------
	>>> urn.set_volume_colormap('histology',['#800000','#FF0000'])
	"""
	data_packet = colormap.copy()
	data_packet.insert(0,volume_name)
	sio.emit('SetVolumeColormap', data_packet)

def set_volume_visibility(volume_name, volume_visibility):
	"""Change the visibility of a volume

	Parameters
	----------
	volumeName : string
		Volume name
	volumeVisibility : bool
		New visibility setting
		
	Examples
	--------
	>>> urn.set_volume_visibility('histology', True)
	"""
	sio.emit('SetVolumeVisibility', [volume_name, volume_visibility])

def set_volume_data(volume_name, volumeData):
	"""Set the data for a volume using uint8 values from 0->254 (255 is reserved for transparent).
	Sending your data as any type other than np.uint8 is potentially unsafe.
	Data will be remapped in the renderer according to the active colormap.
	nan values will be set to transparent.

	Note: this function slices the data by depth and sends data slice-by-slice, it may take a long time to run.

	Parameters
	----------
	volumeData : [string, numpy matrix]
		Name of the volume and the volume itself. Volume should be [528, 320, 456] with +x = posterior, +y = ventral, +z = right
	"""
	localData = volumeData.copy()
	localData[np.isnan(localData)] = 255
	localData = localData.astype(np.uint8)

	ndepth = localData.shape[2]

	for di in np.arange(0,ndepth):
		if di == (ndepth-1):
			set_volume_slice_data(volume_name, int(ndepth-1-di), localData[:,:,di].flatten('F').tobytes(), True)
		else:
			set_volume_slice_data(volume_name, int(ndepth-1-di), localData[:,:,di].flatten('F').tobytes())

def set_volume_slice_data(volume_name, slice, volume_bytes, override_gpu_apply = False):
	"""Set a single slice of data in a volume

	Parameters
	----------
	volumeName : string
		name of volume
	slice : int
		depth slice in volume (on lr dimension)
	volume_bytes : bytes
		flattened bytes array from slice
	override_gpu_apply : bool, optional
		immediately set the GPU texture data after this slice is sent, by default False
	"""
	sio.emit('SetVolumeDataMeta', [volume_name, slice, override_gpu_apply])
	sio.emit('SetVolumeData', volume_bytes)

######################
# VOLUMES: Allen CCF #
######################

# special case of the regular volume visibility function
def set_allen_volume_visibility(allenVisibility):
	"""Set the visibility of the Allen CCF annotation volume

	Parameters
	----------
	allenData : bool
		visibility of Allen CCF volume
	"""
	set_volume_visibility('allen',allenVisibility)

def clear():
	"""Clear the renderer scene of all objects
	"""
	sio.emit('Clear', 'all')

def clear_neurons():
	"""Clear all neuron objects
	"""
	sio.emit('Clear', 'neurons')

def clear_probes():
	"""Clear all probe objects
	"""
	sio.emit('Clear', 'probes')

def clear_areas():
	"""Clear all CCF area models
	"""
	sio.emit('Clear', 'areas')

def clear_volumes():
	"""Clear all 3D volumes
	"""
	sio.emit('Clear', 'volumes')