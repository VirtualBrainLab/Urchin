"""Probers"""

from . import client

def create(probe_names):
	"""Create probe objects

	Parameters
	----------
	probe_names : string list
		list of names of new probes to create

	Examples
	--------
	>>> urn.create(['p1'])
	"""
	client.sio.emit('CreateProbes', probe_names)

def delete(probe_names):
	"""Delete probe objects

	Parameters
	----------
	probe_names : string list
		list of names of new probes to create

	Examples
	--------
	>>> urn.delete(['p1'])
	"""
	client.sio.emit('DeleteProbes', probe_names)

def set_color(probe_colors):
	"""Set colors of probe objects

	Parameters
	----------
	probe_colors : dict {string: string}
		key is probe name, value is hex color

	Examples
	--------
	>>> urn.set_color({'p1':'#FFFFFF'})
	"""
	client.sio.emit('SetProbeColors', probe_colors)

def set_position(probe_positions):
	"""Set probe tip position in ml/ap/dv coordinates in um relative to the CCF (0,0,0) point

	Parameters
	----------
	probe_positions : dict {string: float list}
		key is probe name, value is list of floats in ml/ap/dv in um

	Examples
	--------
	>>> urn.set_position({'p1':[500,1500,2500]})
	"""
	client.sio.emit('SetProbePos', probe_positions)

def set_angle(probe_angles):
	"""Set probe azimuth/elevation/spin angles in degrees

	Azimuth 0 = has the probe facing the AP axis, positive values rotate clockwise
	Elevation 0 = probe is vertical, 90 = horizontal

	Parameters
	----------
	probe_angles : dict {string: float list}
		key is probe name, value is list of floats in az/elev/spin
		
	Examples
	--------
	>>> urn.set_angle({'p1':[-90,0,0]})
	"""
	client.sio.emit('SetProbeAngles', probe_angles)

# def set_probe_style(probeData):
# 	"""Set probe rendering style

# 	Style options are:
# 		"line"
# 		"probe-tip"
# 		"probe-silicon"
# 		"probe"

# 	Inputs:
# 	probeData -- dictionary of probe names and string {'p1':'line'}
# 	"""
# 	client.sio.emit('SetProbeStyle', probeData)

def set_size(probe_size):
	"""Set probe scale in mm units, by default probes are scaled to 70 um wide x 20 um deep x 3840 um tall which is the size of a NP 1.0 probe.

	Parameters
	----------
	probe_size : dict {string: float list}
		key is probe name, value is list of floats for width, height, depth
		
	Examples
	--------
	>>> urn.set_size({'p1':[0.070, 3.840, 0.020]})
	"""
	client.sio.emit('SetProbeSize', probe_size)