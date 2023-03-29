"""Primitive meshes"""

from . import client
  ## Primitive Mesh Renderer
counter = 0
class Primitive:
  #Run everytime an object is created, sets the fields to defaults if one is not given, and sends the info to Unity
  #id = int index counter
  def __init__(self,position= [0.0,0.0,0.0], scale= [1,1,1], color= '#FFFFFF', material = 'default'):
    
    global counter
    counter +=1
    self.id = str(counter)
    client.sio.emit('CreateMesh', [self.id])
    print(self.id)

    #if(position == None):
    #   position = [0,0,0]
    self.position = position
    client.sio.emit('SetPosition', {self.id: position})
      
    #if(scale == None):
    #  scale c
    self.scale = scale
    client.sio.emit('SetScale', {self.id: scale})
      
    #if(color == None):
    #  color = '#FFFFFF'
    self.color = color
    client.sio.emit('SetColor',{self.id: color})

    self.material = material

    

  


  def delete(self):
    client.sio.emit('DeleteMesh', [self.id])
  
  def set_position(self, position):
    """Sets position of mesh object


    Parameters
    ----------
    position : float list
    intended position of mesh object

    Examples
    --------
    >>> cube_obj.set_position([2,2,2])
    """
    self.position = position
   # print({self.id: position})
    client.sio.emit('SetPosition', {self.id: position})
  
  def set_scale(self, scale):
    self.scale = scale
    client.sio.emit('SetScale', {self.id: scale})
  
  def set_color(self, color):
    self.color = color
    client.sio.emit('SetColor',{self.id: color})

  def set_material(self, material):
    self.material = material
    client.sio.emit('SetMaterial',{self.id: material})




#actually initializes each object(s), doesn't use any parameters other than how many to initialize (uses all defaults)
def create(numObjects):
  prim_names = []
  for i in range(numObjects):
    prim_names.append(Primitive())
  return(prim_names)

def delete(meshes_list):
  mesh_ids = [x.id for x in meshes_list]
  client.sio.emit('DeleteMesh', mesh_ids)

def set_positions(meshes_list, positions_list):
  mesh_ids = [x.id for x in meshes_list]
  mesh_pos = {mesh_ids[i]: positions_list[i] for i in range(len(meshes_list))}
  client.sio.emit('SetPosition', mesh_pos)

def set_scales(meshes_list, scales_list):
  mesh_ids = [x.id for x in meshes_list]
  mesh_scale = {mesh_ids[i]: scales_list[i] for i in range(len(meshes_list))}
  client.sio.emit('SetScale', mesh_scale)

def set_colors(meshes_list, colors_list):
  mesh_ids = [x.id for x in meshes_list]
  mesh_colors = {mesh_ids[i]: colors_list[i] for i in range(len(meshes_list))}
  client.sio.emit('SetColor', mesh_colors)

def set_material(meshes_list, materials_list):
  mesh_ids = [x.id for x in meshes_list]
  mesh_materials = {mesh_ids[i]: materials_list[i] for i in range(len(meshes_list))}
  client.sio.emit('SetMaterial', mesh_materials) 

##OLD CODE BELOW
    
    

# def create(mesh_names):
# 	"""Creates primitive mesh

#   Parameters
#   ----------
#   mesh_names : list of strings
# 	IDs of meshes being created
      
# 	Examples
# 	--------
# 	>>> urn.create(['cube1','cube2'])
#   """
# 	client.sio.emit('CreateMesh', mesh_names)

# def delete(mesh_names):
# 	"""Deletes meshes

#   Parameters
#   ----------
#   mesh_names : list of strings
# 	IDs of meshes being deleted
      
# 	Examples
# 	--------
# 	>>> urn.delete(['cube1'])
#   """
# 	client.sio.emit('DeleteMesh', mesh_names)

# def set_position(mesh_pos):
#   """Set the position of mesh renderer

#   Parameters
#   ----------
#   mesh_pos : dict {string : list of three floats}
#       dictionary of IDs and vertex positions of the mesh
      
# 	Examples
# 	--------
# 	>>> urn.set_position({'cube1': [1, 2, 3]})
#   """
#   client.sio.emit('SetPosition', mesh_pos)

# def set_scale(mesh_scale):
#   """Set the scale of mesh renderer

#   Parameters
#   ----------
#   mesh_scale : dict {string : list of three floats}
#       dictionary of IDs and new scale of mesh
      
# 	Examples
# 	--------
# 	>>> urn.set_scale({'cube1': [3, 3, 3]})
#   """
#   client.sio.emit('SetScale', mesh_scale)

# def set_color(mesh_color):
#   """Set the color of mesh renderer

#   Parameters
#   ----------
#   mesh_color : dict {string : string hex color}
#       dictionary of IDs and new hex color of mesh
      
# 	Examples
# 	--------
# 	>>> urn.set_color({'cube1': '#FFFFFF'})
	
#   """
#   client.sio.emit('SetColor', mesh_color)

# def set_material(mesh_material):
#   """Set the material of mesh renderer

#   Parameters
#   ----------
#   mesh_material : dict {string : string}
#       dictionary of object IDs and name of new material
      
# 	Examples
# 	--------
# 	>>> urn.set_material({'cube1': 'unlit'})
	
#   """
#   client.sio.emit('SetMaterial', mesh_material)  