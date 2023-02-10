"""Primitive meshes"""

from . import client

  ## Primitive Mesh Renderer
def create(mesh_names):
	"""Creates primitive mesh

  Parameters
  ----------
  mesh_names : list of strings
	IDs of meshes being created
      
	Examples
	--------
	>>> urn.create(['cube1','cube2'])
  """
	client.sio.emit('CreateMesh', mesh_names)

def delete(mesh_names):
	"""Deletes meshes

  Parameters
  ----------
  mesh_names : list of strings
	IDs of meshes being deleted
      
	Examples
	--------
	>>> urn.delete(['cube1'])
  """
	client.sio.emit('DeleteMesh', mesh_names)

def set_position(mesh_pos):
  """Set the position of mesh renderer

  Parameters
  ----------
  mesh_pos : dict {string : list of three floats}
      dictionary of IDs and vertex positions of the mesh
      
	Examples
	--------
	>>> urn.set_position({'cube1': [1, 2, 3]})
  """
  client.sio.emit('SetPosition', mesh_pos)

def set_scale(mesh_scale):
  """Set the scale of mesh renderer

  Parameters
  ----------
  mesh_scale : dict {string : list of three floats}
      dictionary of IDs and new scale of mesh
      
	Examples
	--------
	>>> urn.set_scale({'cube1': [3, 3, 3]})
  """
  client.sio.emit('SetScale', mesh_scale)

def set_color(mesh_color):
  """Set the color of mesh renderer

  Parameters
  ----------
  mesh_color : dict {string : string hex color}
      dictionary of IDs and new hex color of mesh
      
	Examples
	--------
	>>> urn.set_color({'cube1': '#FFFFFF'})
	
  """
  client.sio.emit('SetColor', mesh_color)