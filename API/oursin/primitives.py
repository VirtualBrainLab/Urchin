"""Primitive meshes"""

from . import client
  ## Primitive Mesh Renderer
counter = 0
class Primitive:
  #Run everytime an object is created, sets the fields to defaults if one is not given, and sends the info to Unity
  #id = int index counter
  def __init__(self,position= [0.0,0.0,0.0], scale= [1,1,1], color= '#FFFFFF', material = 'default'):
    self.create()

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
    

    
  def create(self):
    global counter
    counter +=1
    self.id = str(counter)
    client.sio.emit('CreateMesh', [self.id])
    self.in_unity = True
  


  def delete(self):
    client.sio.emit('DeleteMesh', [self.id])
    self.in_unity = False
  
  def set_position(self, position):
    if self.in_unity == False:
      raise Exception("Object does not exist in Unity, call create method first.")
    
    self.position = position
   # print({self.id: position})
    client.sio.emit('SetPosition', {self.id: position})
  
  def set_scale(self, scale):
    if self.in_unity == False:
      raise Exception("Object does not exist in Unity, call create method first.")
    
    self.scale = scale
    client.sio.emit('SetScale', {self.id: scale})
  
  def set_color(self, color):
    if self.in_unity == False:
      raise Exception("Object does not exist in Unity, call create method first.")
    
    self.color = color
    client.sio.emit('SetColor',{self.id: color})

  def set_material(self, material):
    if self.in_unity == False:
      raise Exception("Object does not exist in Unity, call create method first.")
    
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
  mesh_pos = {}
  for i in range(len(meshes_list)):
    mesh = meshes_list.id
    if mesh[i].in_unity:
      mesh_pos[mesh] = positions_list[i]
    else:
      warnings.warn(f"Object with id {mesh} does not exist. Please create object{mesh}.")


  #Create for loop adding ids to dict IF exists in Unity
  #(does not need to throw exception)
  #use fstrings
  #warnings.warn("object id num does not exist")#add import to this file if it does not compile

  #mesh_ids = [x.id for x in meshes_list]
  #mesh_pos = {mesh_ids[i]: positions_list[i] for i in range(len(meshes_list))}
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