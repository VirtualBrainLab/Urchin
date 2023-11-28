"""Primitive meshes"""

from . import client
import warnings
from . import utils

callback = None

def _neuron_callback(callback_data):
  if callback is not None:
    callback(callback_data)

  ## Primitive Mesh Renderer
counter = 0
class Mesh:
  """Mesh Object in Unity
  """

  def __init__(self,position= [0.0,0.0,0.0], scale= [1,1,1], color= '#FFFFFF', material = 'default'):
    """Create a mesh object

    Note: this function should be avoided, use `urchin.meshes.create()`

    Parameters
    ----------
    position : list, optional
        position in ap/ml/dv Atlas coordinates, by default [0.0,0.0,0.0]
    scale : list, optional
        neuron scale, by default [1,1,1]
    color : str, optional
        by default '#FFFFFF'
    material : str, optional
        by default 'default'
    """
    self.create()

    
    position = utils.sanitize_vector3(position)
    self.position = position
    client.sio.emit('SetPosition', {self.id: position})
      
    
    scale = utils.sanitize_vector3(scale)
    self.scale = scale
    client.sio.emit('SetScale', {self.id: scale})
      
   
    color = utils.sanitize_color(color)
    self.color = color
    client.sio.emit('SetColor',{self.id: color})

    material = utils.sanitize_material(material)
    self.material = material
    

    
  def create(self):
    """Creates primitive mesh

    Parameters
    ----------
    none
      
	  Examples
	  --------
	  >>> cube_obj = urchin.primitives.Primitive()
    """
    global counter
    self.id = str(counter)
    counter +=1
    client.sio.emit('CreateMesh', [self.id])
    self.in_unity = True
  


  def delete(self):
    """Deletes meshes

    Parameters
    ----------
    references object being deleted
      
	  Examples
	  --------
	  >>> cube_obj.delete() 
    """
    client.sio.emit('DeleteMesh', [self.id])
    self.in_unity = False
  
  def set_position(self, position):
    """Set the position of mesh renderer

    Parameters
    ----------
    position : list of three floats
      vertex positions of the mesh
        
    Examples
    --------
    >>> cube_obj.set_position([2,2,2])
    """
    if self.in_unity == False:
      raise Exception("Object does not exist in Unity, call create method first.")
    
    self.position = utils.sanitize_vector3([position[0]/1000, position[1]/1000, position[2]/1000])
    client.sio.emit('SetPosition', {self.id: self.position})
  
  def set_scale(self, scale):
    """Set the scale of mesh renderer

    Parameters
    ----------
    scale : list of three floats
      new scale of mesh
        
    Examples
    --------
    >>> cube_obj.set_scale([3,2,3])
    """
    if self.in_unity == False:
      raise Exception("Object does not exist in Unity, call create method first.")
    
    scale = utils.sanitize_vector3(scale)
    self.scale = scale
    client.sio.emit('SetScale', {self.id: scale})
  
  def set_color(self, color):
    """Set the color of mesh renderer

    Parameters
    ----------
    mesh_color : string hex color
      new hex color of mesh
        
    Examples
    --------
    >>> cube_obj.set_color("#000000")
    
    """ 
    if self.in_unity == False:
      raise Exception("Object does not exist in Unity, call create method first.")
    
    color = utils.sanitize_color(color)
    self.color = color
    client.sio.emit('SetColor',{self.id: color})

  def set_material(self, material):
    """Set the material of mesh renderer

    Parameters
    ----------
    mesh_material : string
      name of new material
        
    Examples
    --------
    >>> cube_obj.set_material('opaque-lit')
    
    """
    if self.in_unity == False:
      raise Exception("Object does not exist in Unity, call create method first.")
    
    material = utils.sanitize_material(material)
    self.material = material
    client.sio.emit('SetMaterial',{self.id: material})




#actually initializes each object(s), doesn't use any parameters other than how many to initialize (uses all defaults)
def create(num_objects):
  """Creates primitive meshes

  Parameters
  ----------
  num_objects : int
    number of primitive objects to be created      
	Examples
	--------
	>>> cubes = urchin.primitives.create(2)
  """
  prim_names = []
  for i in range(num_objects):
    prim_names.append(Mesh())
  return(prim_names)

def delete(meshes_list):
  """Deletes meshes

  Parameters
  ----------
  meshes_list : list of mesh objects
	  list of meshes being deleted
      
	Examples
	--------
	>>> cubes.delete()
  """
  meshes_list = utils.sanitize_list(meshes_list)

  mesh_ids = [x.id for x in meshes_list]
  client.sio.emit('DeleteMesh', mesh_ids)

def set_positions(meshes_list, positions_list):
  """Set the positions of mesh renderers

  Parameters
  ----------
  meshes_list : list of mesh objects
	  list of meshes being set
  positions_list : list of list of three floats
    vertex positions of each mesh
      
	Examples
	--------
	>>> urchin.primitives.set_positions(cubes,[[3,3,3],[2,2,2]])
  """
  meshes_list = utils.sanitize_list(meshes_list)
  positions_list = utils.sanitize_list(positions_list)

  mesh_pos = {}
  for i in range(len(meshes_list)):
    mesh = meshes_list[i]
    if mesh.in_unity:
      pos = [positions_list[i][0]/1000, positions_list[i][1]/1000, positions_list[i][2]/1000]
      mesh_pos[mesh.id] = utils.sanitize_vector3(pos)
    else:
      warnings.warn(f"Object with id {mesh.id} does not exist. Please create object {mesh.id}.")

  client.sio.emit('SetPosition', mesh_pos)

def set_scales(meshes_list, scales_list):
  """Set scale of mesh renderers

  Parameters
  ----------
  meshes_list : list of mesh objects
	  list of meshes being scaled
  scales_list : list of list of three floats
    new scales of each mesh
      
	Examples
	--------
	>>> urchin.primitives.set_scales(cubes,[[3,3,3],[2,2,2]])
  """
  meshes_list = utils.sanitize_list(meshes_list)
  scales_list = utils.sanitize_list(scales_list)

  mesh_scale = {}
  for i in range(len(meshes_list)):
    mesh = meshes_list[i]
    if mesh.in_unity:
      mesh_scale[mesh.id] = utils.sanitize_vector3(scales_list[i])
    else:
      warnings.warn(f"Object with id {mesh.id} does not exist. Please create object {mesh.id}.")

  client.sio.emit('SetScale', mesh_scale)

def set_colors(meshes_list, colors_list):
  """Sets colors of mesh renderers

  Parameters
  ----------
  meshes_list : list of mesh objects
	  list of meshes undergoing color change
  colors_list : list of string hex colors
      new hex colors for each mesh
      
	Examples
	--------
	>>> urchin.primitives.set_colors(cubes,["#000000","#000000"])
	
  """
  meshes_list = utils.sanitize_list(meshes_list)
  colors_list = utils.sanitize_list(colors_list)

  mesh_colors = {}
  for i in range(len(meshes_list)):
    mesh = meshes_list[i]
    if mesh.in_unity:
      mesh_colors[mesh.id] = utils.sanitize_color(colors_list[i])
    else:
      warnings.warn(f"Object with id {mesh.id} does not exist. Please create object {mesh.id}.")

  client.sio.emit('SetColor', mesh_colors)

def set_materials(meshes_list, materials_list):
  """Sets materials of mesh renderers

  Parameters
  ----------
  meshes_list : list of mesh objects
	  list of meshes undergoing material change
  materials_list : list of strings
      name of new materials
      
	Examples
	--------
	>>> urchin.primitives.set_material(cubes,['opaque-lit','opaque-lit'])
	
  """
  meshes_list = utils.sanitize_list(meshes_list)
  materials_list = utils.sanitize_list(materials_list)

  mesh_materials = {}
  for i in range(len(meshes_list)):
    mesh = meshes_list[i]
    if mesh.in_unity:
      mesh_materials[mesh.id] = utils.sanitize_material(materials_list[i])
    else:
      warnings.warn(f"Object with id {mesh.id} does not exist. Please create object {mesh.id}.")  
      
  client.sio.emit('SetMaterial', mesh_materials) 