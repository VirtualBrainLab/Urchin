"""Neurons"""

from . import client
import warnings
import utils

## Neurons renderer
counter = 0
class Neuron:
	def __init__(self, position= [0.0,0.0,0.0], color= '#FFFFFF', size= 0.02, shape= 'sphere', material= 'lit-transparent'):
		self.create()

		position = utils.sanitize_vector3(position)
		self.position = position
		client.sio.emit('SetNeuronPos', {self.id: position})

		color = utils.sanitize_color(color)
		self.color = color
		client.sio.emit('SetNeuronColor',{self.id: color})

		size = utils.sanitize_float(size)
		self.size = size
		client.sio.emit('SetNeuronSize',{self.id: size})

		shape = utils.sanitize_string(shape)
		self.shape = shape
		client.sio.emit('SetNeuronShape',{self.id: shape})

		material = utils.sanitize_material(material)
		self.material = material
		client.sio.emit('SetNeuronMaterial',{self.id: material})

	def create(self):
		"""Creates neurons
		
		Parameters
		---------- 
		none

		Examples
		>>>n1 = urchin.neurons.Neuron()
		"""
		global counter
		counter += 1
		self.id = 'n' + str(counter)
		client.sio.emit('CreateNeurons', [self.id])
		self.in_unity = True

	def delete(self):
		"""Deletes neurons
		
		Parameters
		---------- 
		references object being deleted

		Examples
		>>>n1.delete()
		"""
		client.sio.emit('DeleteNeurons', [self.id])
		self.in_unity = False

	def set_position(self, position):
		"""Set the position of neuron position in ml/ap/dv coordinates relative to the CCF (0,0,0) point
		
		Parameters
		---------- 
		position : list of three floats
			vertex positions of the neuron relative to the CCF point

		Examples
		--------
		>>>n1.set_position([2,2,2])
		"""
		if self.in_unity == False:
			raise Exception("Neuron does not exist in Unity, call create method first.")
		
		position = utils.sanitize_vector3(position)
		self.position = position
		client.sio.emit('SetNeuronPos', {self.id: position})

	def set_size(self, size):
		"""Set the size of neuron renderer
		
		Parameters
		---------- 
		size : float
			size of the neuron

		Examples
		--------
		>>>n1.set_size(0.02)
		"""
		if self.in_unity == False:
			raise Exception("Neuron does not exist in Unity, call create method first.")
		
		size = utils.sanitize_float(size)
		self.size = size
		client.sio.emit('SetNeuronSize', {self.id: size})
	
	def set_color(self, color):
		"""Set the color of neuron renderer
		
		Parameters
		---------- 
		color : string hex color
			new hex color of the neuron

		Examples
		--------
		>>>n1.set_color('#FFFFFF')
		"""
		if self.in_unity == False:
			raise Exception("Neuron does not exist in Unity, call create method first.")
		
		color = utils.sanitize_color(color)
		self.color = color
		client.sio.emit('SetNeuronColor', {self.id: color})

	def set_shape(self, shape):
		"""Set the shape of neuron renderer
		Options are
	 	 - 'sphere' (default)
	 	 - 'cube' better performance when rendering tens of thousands of neurons

		Parameters
		---------- 
		shape : string
			new shape of the neuron

		Examples
		--------
		>>>n1.set_shape('sphere')
		"""
		if self.in_unity == False:
			raise Exception("Neuron does not exist in Unity, call create method first.")
		
		self.shape = shape
		client.sio.emit('SetNeuronShape', {self.id: shape})

	def set_material(self, material):
		"""Set the material of neuron renderer
		Options are
	 	- 'lit-transparent' (default)
	 	- 'lit'
	 	- 'unlit'

		Parameters
		---------- 
		material : string
			new material of the neuron

		Examples
		--------
		>>>n1.set_material('lit-transparent')
		"""
		if self.in_unity == False:
			raise Exception("Neuron does not exist in Unity, call create method first.")
		
		self.material = material
		client.sio.emit('SetNeuronMaterial', {self.id: material})

def create(num_neurons):
	"""Create neuron objects

	Note: neurons must be created before setting other values

	Parameters
	----------
	num_neurons : int
		number of new neuron objects

	Examples
	--------
	>>> neurons = urchin.neurons.create(3)
	"""
	neuron_names = []
	for i in range(num_neurons):
		neuron_names.append(Neuron())
	return neuron_names

def delete(neurons_list):
  """Delete neuron objects

  Parameters
  ----------
  neuron_names : list of neuron objects
	list of neurons being deleted

  Examples
  --------
  >>> urchin.neurons.delete()
  """
  neurons_list = utils.sanitize_list(neurons_list)

  neurons_ids = [x.id for x in neurons_list]
  client.sio.emit('DeleteNeurons', neurons_ids)

def set_positions(neurons_list, positions_list):
	"""Set the position of neuron position in ml/ap/dv coordinates relative to the CCF (0,0,0) point

	Parameters
	----------
	neurons_list : list of neuron objects
		list of neurons being moved
	positions : list of list of three floats
		list of positions of neurons

	Examples
	--------
	>>> urchin.neurons.set_position([n1,n2,n3], [[1,1,1],[2,2,2],[3,3,3]])
	"""
	neurons_list = utils.sanitize_list(neurons_list)
	positions_list = utils.sanitize_list(positions_list)

	neurons_pos = {}
	for i in range(len(neurons_list)):
		neuron = neurons_list[i]
		if neuron.in_unity:
			neurons_pos[neuron.id] = utils.sanitize_vector3[positions_list[i]]
		else:
			warnings.warn(f"Neuron with id {neuron.id} does not exist in Unity, call create method first.")
	client.sio.emit('SetNeuronPos', neurons_pos)

def set_sizes(neurons_list, sizes_list):
	"""Set neuron sizes

	Parameters
	----------
	neurons_list : list of neuron objects
		list of neurons being resized
	sizes : list of floats
		list of sizes of neurons

	Examples
	--------
	>>> urchin.neurons.set_size([n1,n2,n3], [0.01,0.02,0.03])
	"""
	neurons_list = utils.sanitize_list(neurons_list)
	sizes_list = utils.sanitize_list(sizes_list)

	neurons_sizes = {}
	for i in range(len(neurons_list)):
		neuron = neurons_list[i]
		if neuron.in_unity:
			neurons_sizes[neuron.id] = utils.sanitize_float(sizes_list[i])
		else:
			warnings.warn(f"Neuron with id {neuron.id} does not exist in Unity, call create method first.")
	client.sio.emit('SetNeuronSize', neurons_sizes)

def set_colors(neurons_list, colors_list):
	"""Set neuron colors

	Parameters
	----------
	neurons_list : list of neuron objects
		list of neurons being recolored
	colors : list of string hex colors
		list of colors of neurons

	Examples
	--------
	>>> urchin.neurons.set_color([n1,n2,n3], ['#FFFFFF','#000000','#FF0000'])
	"""
	neurons_list = utils.sanitize_list(neurons_list)
	colors_list = utils.sanitize_list(colors_list)

	neurons_colors = {}
	for i in range(len(neurons_list)):
		neuron = neurons_list[i]
		if neuron.in_unity:
			neurons_colors[neuron.id] = utils.sanitize_color(colors_list[i])
		else:
			warnings.warn(f"Neuron with id {neuron.id} does not exist in Unity, call create method first.")
	client.sio.emit('SetNeuronColor', neurons_colors)

def set_shapes(neurons_list, shapes_list):
	"""Set neuron shapes

	Options are
	 - 'sphere' (default)
	 - 'cube' better performance when rendering tens of thousands of neurons

	Parameters
	----------
	neurons_list : list of neuron objects
		list of neurons being reshaped
	shapes : list of string
		list of shapes of neurons

	Examples
	--------
	>>> urchin.neurons.set_shape([n1,n2,n3], ['sphere','cube','sphere'])
	"""
	neurons_list = utils.sanitize_list(neurons_list)
	shapes_list = utils.sanitize_list(shapes_list)

	neurons_shapes = {}
	for i in range(len(neurons_list)):
		neuron = neurons_list[i]
		if neuron.in_unity:
			neurons_shapes[neuron.id] = utils.sanitize_shape(shapes_list[i])
		else:
			warnings.warn(f"Neuron with id {neuron.id} does not exist in Unity, call create method first.")
	client.sio.emit('SetNeuronShape', neurons_shapes)

def set_materials(neurons_list, materials_list):
	"""Change the material used to render neurons

 	Options are
 	- 'lit-transparent' (default)
 	- 'lit'
 	- 'unlit'

	Parameters
	----------
	neurons_list : list of neuron objects
		list of neurons being rematerialized
	materials : list of string
		list of materials of neurons

	Examples
	--------
	>>> urchin.neurons.set_material([n1,n2,n3], ['lit','unlit','lit'])
	"""
	neurons_list = utils.sanitize_list(neurons_list)
	materials_list = utils.sanitize_list(materials_list)

	neurons_materials = {}
	for i in range(len(neurons_list)):
		neuron = neurons_list[i]
		if neuron.in_unity:
			neurons_materials[neuron.id] = utils.sanitize_material(materials_list[i])
		else:
			warnings.warn(f"Neuron with id {neuron.id} does not exist in Unity, call create method first.")
	client.sio.emit('SetNeuronMaterial', neurons_materials)

