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
    counter += 1
    self.id = str(counter)
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
