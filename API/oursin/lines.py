from . import client

## Line Renderer
def create(line_names):
	"""Creates lines

  Parameters
  ----------
  line_names : list of strings
	IDs of lines being created
      
	Examples
	--------
	>>> urn.create(['l1', 'l2','l3'])
  """
	client.sio.emit('CreateLine', line_names)

def delete(line_names):
	"""Deletes lines

  Parameters
  ----------
  line_names : list of strings
	IDs of lines being deleted
      
	Examples
	--------
	>>> urn.delete(['l1', 'l2'])
  """
	client.sio.emit('DeleteLine', line_names)

def set_position(line_pos):
  """Set the position of line renderer

  Parameters
  ----------
  line_pos : dict {string : list of three floats}
      dictionary of IDs and vertex positions of the line
      
	Examples
	--------
	>>> urn.set_position({'l1': [[0, 0, 0], [1, 1, 1]]})
  """
  client.sio.emit('SetLinePosition', line_pos)

def set_color(line_color):
  """Set the color of line renderer

  Parameters
  ----------
  line_color : dict {string : string hex color}
      dictionary of IDs and new color of the line
      
	Examples
	--------
	>>> urn.set_color({'l1': '#FFFFFF'})
	
  """
  client.sio.emit('SetLineColor', line_color)