"""Text"""

from . import client
import warnings
from . import utils

## Text renderer

counter = 0
class Text:
  def __init__(self, text = "", color = "#FFFFFF", font_size = 12, position = [0,0]):
    self.create()

    self.set_text(text)

    self.set_font_size(font_size)

    color = utils.sanitize_color(color)
    self.color = color
    client.sio.emit('SetTextColors',{self.id: color})

    position = utils.sanitize_list(position)
    self.position = position
    client.sio.emit('SetTextPositions',{self.id: position})


  def create(self):
    """Create a text object

    Parameters
    ----------
    none
        
    Examples
    --------
    >>> t1 = urchin.text.Text()
    """
    global counter
    counter +=1
    self.id = 't' + str(counter)
    client.sio.emit('CreateText',[self.id])
    self.in_unity = True
  
  def delete(self):
    """Delete a text object

    Parameters
    ----------
    references object being deleted
        
    Examples
    --------
    >>> t1.delete()
    """
    client.sio.emit('DeleteText',[self.id])
    self.in_unity = False

  def set_text(self, text_text):
    """Set the text in a set of text objects

    Parameters
    ----------
    text : string
      text to be displayed

    Examples
    --------
    >>> t1.set_text('test text')
    """
    if self.in_unity == False:
      raise Exception("Object does not exist in Unity, call create method first.")
    text_text = utils.sanitize_string(text_text)
    self.text = text_text
    client.sio.emit('SetTextText',{self.id: text_text})

  def set_color(self,text_color):
    """Set the color of a set of text objects

    Parameters
    ----------
    text_colors : string hex color
        hex colors as strings
        
    Examples
    --------
    >>> t1.set_color('#FF0000')
    """
    if self.in_unity == False:
      raise Exception("Object does not exist in Unity, call create method first.")
    
    text_color = utils.sanitize_color(text_color)
    self.color = text_color
    client.sio.emit('SetTextColors',{self.id: text_color})

  def set_font_size(self,text_size):
    """Set the font size of a set of text objects

    Parameters
    ----------
    text_sizes : int
        font sizes
        
    Examples
    --------
    >>> t1.set_size(12)
    """
    if self.in_unity == False:
      raise Exception("Object does not exist in Unity, call create method first.")
    self.size = text_size
    client.sio.emit('SetTextSizes',{self.id: text_size})

  def set_position(self,position):
    """Set the positions of a set of text objects in UI canvas space
    Bottom left corner is [-1,-1], top right [1,1]

    Text is anchored at the top left corner of its text box.

    Parameters
    ----------
    text_pos : list of two floats
        canvas positions relative to the center
        
    Examples
    --------
    >>> t1.set_position([400, 300])
    """
    if self.in_unity == False:
      raise Exception("Object does not exist in Unity, call create method first.")
    self.position = utils.sanitize_list(position)
    client.sio.emit('SetTextPositions',{self.id: self.position})


def create(n):
  """Create n text objects with default parameters

  Parameters
  ----------
  n : int
      number of text objects
  """
  text_list = []
  for i in range(n):
    text_list.append(Text())
  return text_list

def set_texts(text_list, str_list):
  """Set the string value of multiple text objects

  Parameters
  ----------
  text_list : list of Text
      Text objects
  str_list : _type_
      _description_
  """
  text_list = utils.sanitize_list(text_list)
  str_list = utils.sanitize_list(str_list, len(text_list))

  text_strs = {}
  for i, text in enumerate(text_list):
    text_strs[text.id] = str_list[i]
  
  client.sio.emit('SetTextText',text_strs)

def set_positions(text_list, pos_list):
  """Set the positions of multiple text objects

  Positions are [0,1] relative to the edges of the screen

  Parameters
  ----------
  text_list : list of Text
      Text objects
  pos_list : list of float
      [0,0] top left [1,1] bottom right
  """
  text_list = utils.sanitize_list(text_list)
  pos_list = utils.sanitize_list(pos_list, len(text_list))

  text_poss = {}
  for i, text in enumerate(text_list):
    text_poss[text.id] = pos_list[i]
  
  client.sio.emit('SetTextPositions',text_poss)

def set_font_sizes(text_list, font_size_list):
  """_summary_

  Parameters
  ----------
  text_list : list of Text
      Text objects
  font_size_list : _type_
      _description_
  """
  text_list = utils.sanitize_list(text_list)
  font_size_list = utils.sanitize_list(font_size_list, len(text_list))

  text_font_sizes = {}
  for i, text in enumerate(text_list):
    text_font_sizes[text.id] = font_size_list[i]
  
  client.sio.emit('SetTextSizes',text_font_sizes)

def set_colors(text_list, color_list):
  """_summary_

  Parameters
  ----------
  text_list : list of Text
      Text objects
  color_list : _type_
      _description_
  """
  text_list = utils.sanitize_list(text_list)
  color_list = utils.sanitize_list(color_list, len(text_list))

  text_colors = {}
  for i, text in enumerate(text_list):
    text_colors[text.id] = color_list[i]
  
  client.sio.emit('SetTextColors',text_colors)