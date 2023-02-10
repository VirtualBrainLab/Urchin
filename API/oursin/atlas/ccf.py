""" Allen CCF 3D Models"""

from .. import client

def load_beryl():
	"""Load all beryl areas and set visibility to True

	NOTE: One of the load functions OR set_area_visibility must be called
	before you set the color/alpha/etc of brain regions.
	"""
	client.sio.emit('LoadDefaultAreas', 'beryl')

def load_cosmos():
	"""Load all cosmos areas and set visibility to True

	NOTE: One of the load functions OR set_area_visibility must be called
	before you set the color/alpha/etc of brain regions.
	"""
	client.sio.emit('LoadDefaultAreas', 'cosmos')

def set_visibility(area_visibilities):
	"""Set visibility of CCF area models

	**Note:** you can append a "-lh" or "-rh" suffix to any area acronym or ID to control the visibility of just one-half of the model. This can be used in all of the set_area_* functions.

	Parameters
	----------
	area_visibilities : dict {string : bool}
		dictionary of area IDs or acronyms and visibility values

	Examples
	--------
	>>> urn.set_visibility({'grey':True})
	>>> urn.set_visibility({8:True})
	>>> urn.set_visibility({'VISp-l':True})
	"""
	client.sio.emit('SetAreaVisibility', area_visibilities)

def set_color(area_colors):
	"""Set color of CCF area models.

	Parameters
	----------
	area_colors : dict {string: string}
		Keys are area IDs or acronyms, Values are hex colors

	Examples
	--------
	>>> urn.set_color( {'grey':"#FFFFFF"})
	>>> urn.set_color({8:"#FFFFFF"})
	>>> urn.set_color({"VISp-l":"#00000080"})
	"""
	client.sio.emit('SetAreaColors', area_colors)

def set_intensity(area_intensities):
	"""Set color of CCF area models using colormap.

	Parameters
	----------
	area_intensities : dict {string: float}
		keys are area IDs or acronyms, values are hex colors

	Examples
	--------
	>>> urn.set_intensity( {'grey':1.0})
	>>> urn.set_intensity({8:1.0})
	"""
	client.sio.emit('SetAreaIntensity', area_intensities)

def set_colormap(colormap_name):
	"""Set colormap used for CCF area intensity mapping


	Options are
	 - cool (default, teal 0 -> magenta 1)
	 - grey (black 0 -> white 1)
	 - grey-green (grey 0, light 1/255 -> dark 1)
	 - grey-purple (grey 0, light 1/255 -> dark 1)
	 - grey-red (grey 0, light 1/255 -> dark 1)
	 - grey-rainbow (grey 0, rainbow colors from 1/255 -> 1)

	Parameters
	----------
	colormap_name : string
		colormap name
	"""
	client.sio.emit('SetAreaColormap', colormap_name)

def set_alpha(area_alpha):
	"""Set transparency of CCF area models. 

	Parameters
	----------
	area_alpha : dict {string: float}
		keys are area IDs or acronyms, values are percent transparency

	Examples
	--------
	>>> urn.set_alpha( {'grey':0.5})
	>>> urn.set_alpha({8:0.5})
	"""
	client.sio.emit('SetAreaAlpha', area_alpha)

def set_material(area_materials):
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
	client.sio.emit('SetAreaMaterial', area_materials)

def set_data(area_data):
	"""Set the data array for each CCF area model

	Data arrays work the same as the set_area_intensity() function but are controlled by the area_index value, which can be set in the renderer or through the API.

	Parameters
	----------
	area_data : dict {string: float list}
		keys area IDs or acronyms, values are a list of floats
	"""
	client.sio.emit('SetAreaData', area_data)

def set_data_index(area_index):
	"""Set the data index for the CCF area models

	Parameters
	----------
	area_index : int
		data index
	"""
	client.sio.emit('SetAreaIndex', area_index)