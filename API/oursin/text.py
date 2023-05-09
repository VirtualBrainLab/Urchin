"""Text"""

from . import client
import warnings
import utils

## Text renderer

counter = 0
class Text:
  def __init__(self, text = "", color = "#FFFFFF", size = 12, positon = [0,0]):
    self.create()

    text = utils.sanitize_string(text)
    self.text = text
    client.sio.emit('SetTextText',{self.id: text})

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

  def set_size(self,text_size):
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

  def set_position(self,text_pos):
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
    position = utils.sanitize_list(position)
    self.position = position
    client.sio.emit('SetTextPositions',{self.id: position})
