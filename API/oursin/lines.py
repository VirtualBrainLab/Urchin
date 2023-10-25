"""Lines"""

from . import client
import warnings
from . import utils


counter = 0

class Line:
  def __init__(self, position= [[0.0,0.0,0.0]], color= '#FFFFFF'):
    self.create()

    self.set_position(position)
    self.set_color(color)

  def create(self):
    """Creates lines
    
    Parameters
    ---------- 
    none

    Examples
    >>>l1 = urchin.lines.Line()
    """
    global counter
    counter += 1
    self.id = 'l' + str(counter)
    client.sio.emit('CreateLine', [self.id])
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
    position : list of vector3 [[ap,ml,dv],[ap,ml,dv]]
        vertex positions of the line in the ReferenceAtlas space (um)

    Examples
    --------
    >>>l1.set_position([[0, 0, 0],[13200,11400,8000]])
    """
    if self.in_unity == False:
      raise Exception("Line does not exist in Unity, call create method first.")

    for i, vec3 in enumerate(position):
      position[i] = utils.sanitize_vector3(vec3)
    self.position = position

    client.sio.emit('SetLinePosition', {self.id: self.position})

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

def create (n):
  """Create Line objects

  Parameters
  ----------
  n : int
      Number of objects to create
  """
  lines_list = []
  
  for i in range(n):
    line = Line()
    lines_list.append(line)

  return lines_list

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
