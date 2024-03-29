import webbrowser
import os

notebook = False
try:
	from IPython import get_ipython
	if get_ipython() is not None:
		from IPython.display import Javascript
		notebook = True
except ImportError:
	# You are not running in a Jupyter Notebook
	pass

from . import client

def is_running_in_colab():
	return notebook and 'google.colab' in str(get_ipython())

def setup(localhost = False, standalone = False, verbose = True):
	"""Connect the Unity Renderer for Neuroscience Python API to the web-based (or standalone) viewer

	Parameters
	----------
	localhost : bool, optional
		connect to a local development server rather than the remote server, by default False
	standalone : bool, optional
		connect to a standalone Desktop build rather than the web-based Brain Viewer, by default False
	"""

	if client.connected():
		print(f'(urchin) Client is already connected. Use ID: {client.ID}')
		return
		
	log_messages = verbose

	if localhost:
		client.sio.connect('http://localhost:5000')
	else:
		client.sio.connect('https://urchin-commserver.herokuapp.com/')

	if not standalone:
		#To open browser window:
		url = f'https://data.virtualbrainlab.org/Urchin/?ID={client.ID}'
		if not is_running_in_colab():
			webbrowser.open(url)
		else:
			# Specify window features
			window_features = "width=1200,height=800,toolbar=no,location=no,directories=no,status=no,menubar=no,scrollbars=yes,resizable=yes"
			# Use the window.open function with window features
			javascript_code = f'window.open("{url}", "_blank", "{window_features}");'
			# Display the JavaScript code to open the new window
			display(Javascript(javascript_code))

######################
# CLEAR #
######################

def clear():
	"""Clear the renderer scene of all objects
	"""
	client.sio.emit('Clear', 'all')

def clear_probes():
	"""Clear all probe objects
	"""
	client.sio.emit('Clear', 'probe')

def clear_volumes():
	"""Clear all 3D volumes
	"""
	client.sio.emit('Clear', 'volume')

def clear_texts():
	"""Clear all text
	"""
	client.sio.emit('Clear', 'text')

def clear_meshes():
	"""Clear all primitives
	"""
	client.sio.emit('Clear','mesh')