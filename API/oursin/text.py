"""Text"""

from . import client

def create(text_id):
  """Create a text object

  Parameters
  ----------
  text_id : list of strings
      IDs for text objects
      
	Examples
	--------
	>>> urn.create(['t1','t2'])
  """
  client.sio.emit('CreateText',text_id)
  
def delete(text_id):
  """Delete a text object

  Parameters
  ----------
  text_id : list of strings
      IDs for text objects
      
	Examples
	--------
	>>> urn.delete(['t1'])
  """
  client.sio.emit('DeleteText',text_id)

def set_text(text_text):
  """Set the text in a set of text objects

  Parameters
  ----------
  text_text : dict {string : string}
		dictionary of IDs and text

	Examples
	--------
	>>> urn.set_text({'t1': 'test text'})
  """
  client.sio.emit('SetTextText',text_text)

def set_color(text_colors):
  """Set the color of a set of text objects

  Parameters
  ----------
  text_colors : dict {string : string hex color}
      dictionary of IDs and hex colors as strings
      
	Examples
	--------
	>>> urn.set_color({'t1': '#FF0000'})
  """
  client.sio.emit('SetTextColors',text_colors)

def set_size(text_sizes):
  """Set the font size of a set of text objects

  Parameters
  ----------
  text_sizes : dict {string : int}
      dictionary of IDs and font sizes
      
	Examples
	--------
	>>> urn.set_size({'t1': 12})
  """
  client.sio.emit('SetTextSizes',text_sizes)

def set_position(text_pos):
  """Set the positions of a set of text objects in UI canvas space
  Bottom left corner is [-1,-1], top right [1,1]

  Text is anchored at the top left corner of its text box.

  Parameters
  ----------
  text_pos : dict {string : list of two floats}
      dictionary of IDs and canvas positions relative to the center
      
	Examples
	--------
	>>> urn.set_position({'t1': [400, 300]})
  """
  client.sio.emit('SetTextPositions', text_pos)
