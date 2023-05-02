"""Lines"""

from . import client
import warnings
import utils


counter = 0

class Line:
  def __init__(self, position= [0.0,0.0,0.0], color= '#FFFFFF'):
    self.create()

    position = utils.sanitize_vector3(position)
    self.position = position
    client.sio.emit('SetLinePosition', {self.id: position})


    color = utils.sanitize_color(color)
    self.color = color
    client.sio.emit('SetLineColor',{self.id: color})

  def create(self):
    """Creates lines
    
    Parameters
    ---------- 
    none

    Examples
    >>>l1 = urchin.lines.Line()
    """
    global counter
    self.id = str(counter)
    client.sio.emit('CreateLine', [self.id])
    counter += 1
    self.in_unity = True

  def delete(self):
    """Deletes lines
    
    Parameters
    ---------- 
    references object being deleted

    Examples
    >>>l1.delete()
    """
    client.sio.emit('DeleteLine', [self.id])
    self.in_unity = False

  def set_position(self, position):
    """Set the position of line renderer
    
    Parameters
    ---------- 
    position : list of three floats
        vertex positions of the line

    Examples
    --------
    >>>l1.set_position([0, 0, 0])
    """
    if self.in_unity == False:
      raise Exception("Line does not exist in Unity, call create method first.")

    position = utils.sanitize_vector3(position)
    self.position = position
    client.sio.emit('SetLinePosition', {self.id: position})

  def set_color(self, color):
    """Set the color of line renderer
    
    Parameters
    ---------- 
    color : string hex color
        new color of the line

    Examples
    --------
    >>>l1.set_color('#000000')
    """
    if self.in_unity == False:
      raise Exception("Line does not exist in Unity, call create method first.")

    color = utils.sanitize_color(color)
    self.color = color
    client.sio.emit('SetLineColor',{self.id: color})

def create (numLines):
  """Creates lines
  
  Parameters
  ---------- 
  numLines : int
      number of lines to be created

  Examples
  --------
  >>>lines = urchin.lines.create(3)
  """
  line_names = []
  for i in range(numLines):
    line_names.append(Line())
  return(line_names)

def delete (lines_list):
  """Deletes lines
  
  Parameters
  ---------- 
  lines_list : list of Line objects
      list of lines to be deleted

  Examples
  --------
  >>> lines.delete()
  """
  lines_list = utils.sanitize_list(lines_list)
  lines_ids = [x.id for x in lines_list]
  client.sio.emit("DeleteLine", lines_ids)

# def set_positions (lines_list, positions_list):
#   """Set the positions of line renderers
  
#   Parameters
#   ---------- 
#   lines_list : list of Line objects
#       list of lines to be modified
#   positions_list : list of list of lists of three floats
#       list of new vertex positions of the lines for each line

#   Examples
#   --------
#   >>> urchin.lines.set_positions(lines, [[[0,0,0],[1,1,1],[2,2,2]], [[0,0,0],[-1,-1,-1],[-2,-2,-2]]])
#   """
#   lines_list = utils.sanitize_list(lines_list)
#   positions_list = utils.sanitize_list(positions_list)

#   lines_pos = {}
#   for i in range(len(lines_list)):
#     line = lines_list[i]
#     if line.in_unity:
#       lines_pos[line.id] = utils.sanitize_vector3(positions_list[i])
#     else:
#       warnings.warn(f"Object with id {line.id} does not exist. Please create object {line.id}.")
#   client.sio.emit("SetLinePosition", lines_pos)

def set_colors(lines_list, colors_list):
  """Set the colors of line renderers
  
  Parameters
  ---------- 
  lines_list : list of Line objects
      list of lines to be modified
  colors_list : list of string hex colors
      list of new colors of the lines for each line

  Examples
  --------
  >>> urchin.lines.set_colors(lines, ['#000000', '#FFFFFF'])
  """
  lines_list = utils.sanitize_list(lines_list)
  colors_list = utils.sanitize_list(colors_list)

  lines_color = {}
  for i in range(len(lines_list)):
    line = lines_list[i]
    if line.in_unity:
      lines_color[line.id] = utils.sanitize_color(colors_list[i])
    else:
      warnings.warn(f"Line with id {line.id} does not exist. Please create Line {line.id}.")
  client.sio.emit("SetLineColor", lines_color)


##OLD CODE BELOW

# ## Line Renderer
# def create(line_names):
# 	"""Creates lines

#   Parameters
#   ----------
#   line_names : list of strings
# 	IDs of lines being created
      
# 	Examples
# 	--------
# 	>>> urn.create(['l1', 'l2','l3'])
#   """
# 	client.sio.emit('CreateLine', line_names)

# def delete(line_names):
# 	"""Deletes lines

#   Parameters
#   ----------
#   line_names : list of strings
# 	IDs of lines being deleted
      
# 	Examples
# 	--------
# 	>>> urn.delete(['l1', 'l2'])
#   """
# 	client.sio.emit('DeleteLine', line_names)

# def set_position(line_pos):
#   """Set the position of line renderer

#   Parameters
#   ----------
#   line_pos : dict {string : list of three floats}
#       dictionary of IDs and vertex positions of the line
      
# 	Examples
# 	--------
# 	>>> urn.set_position({'l1': [[0, 0, 0], [1, 1, 1]]})
#   """
#   client.sio.emit('SetLinePosition', line_pos)

# def set_color(line_color):
#   """Set the color of line renderer

#   Parameters
#   ----------
#   line_color : dict {string : string hex color}
#       dictionary of IDs and new color of the line
      
# 	Examples
# 	--------
# 	>>> urn.set_color({'l1': '#FFFFFF'})
	
#   """
#   client.sio.emit('SetLineColor', line_color)