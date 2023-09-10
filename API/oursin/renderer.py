import webbrowser
import os
from IPython.display import Javascript

from . import client

def is_running_in_colab():
	return True #'google.colab' in str(get_ipython())

def setup(verbose = True, localhost = False, standalone = False):
	"""Connect the Unity Renderer for Neuroscience Python API to the web-based (or standalone) viewer

	Parameters
	----------
	localhost : bool, optional
		connect to a local development server rather than the remote server, by default False
	standalone : bool, optional
		connect to a standalone Desktop build rather than the web-based Brain Viewer, by default False
	"""

	if client.connected():
		print("(urchin) Client is already connected")
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

def clear_neurons():
	"""Clear all neuron objects
	"""
	client.sio.emit('Clear', 'neurons')

def clear_probes():
	"""Clear all probe objects
	"""
	client.sio.emit('Clear', 'probes')

def clear_areas():
	"""Clear all CCF area models
	"""
	client.sio.emit('Clear', 'areas')

def clear_volumes():
	"""Clear all 3D volumes
	"""
	client.sio.emit('Clear', 'volumes')

def clear_texts():
	"""Clear all text
	"""
	client.sio.emit('Clear', 'texts')