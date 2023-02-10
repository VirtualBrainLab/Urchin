from . import client

###########
# NEURONS #
###########

def create(neuron_names):
	"""Create neuron objects

	Note: neurons must be created before setting other values

	Parameters
	----------
	neuron_names : string list
		names of the new neuron objects

	Examples
	--------
	>>> urn.create(["n1","n2","n3"])
	"""
	client.sio.emit('CreateNeurons', neuron_names)

def delete(neuron_names):
  """Delete neuron objects

  Parameters
  ----------
  neuron_names : string list
    names of the neuron objects

  Examples
  --------
  >>> urn.delete(["n1","n2","n3"])
  """
  client.sio.emit('DeleteNeurons', neuron_names)

def set_position(neuron_positions):
	"""Set neuron positions

	Parameters
	----------
	neuron_positions : dict {string: int list}
		keys are neuron names, values are ML/AP/DV coordinates in um units relative to CCF (0,0,0)

	Examples
	--------
	>>> urn.set_positions({'n1':[500,1500,1800]})
	"""
	client.sio.emit('SetNeuronPos', neuron_positions)

def set_size(neuron_sizes):
	"""Set size of neuron objects in mm units

	Parameters
	----------
	neuron_sizes : dict {string: float}
		keys are neuron names, values are float size in mm
		
	Examples
	--------
	>>> urn.set_size( {'n1':0.02})
	"""
	client.sio.emit('SetNeuronSize', neuron_sizes)

def set_color(neuron_colors):
	"""Set colors of neuron objects

	Parameters
	----------
	neuron_colors : dict {string: string}
		keys are neuron names, values are hex colors
		
	Examples
	--------
	>>> urn.set_color( {'n1':"#FFFFFF"})
	"""
	client.sio.emit('SetNeuronColor', neuron_colors)

def set_shape(neuron_shapes):
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
	>>> urn.set_shape( {'n1':'sphere'})
	"""
	client.sio.emit('SetNeuronShape', neuron_shapes)

def set_material(neuron_materials):
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
	client.sio.emit('SetNeuronMaterial', neuron_materials)