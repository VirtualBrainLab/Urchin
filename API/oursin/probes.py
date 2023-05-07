"""Probers"""

from . import client
import warnings
import utils

##Probes Renderer
counter = 0
class Probe:
	def __init__(self, color = 'FFFFFF', position = [0,0,0], angle = [0,0,0], style = 'line', scale = [0.070, 3.840, 0.020]):
		self.create()

		color = utils.sanitize_color(color)
		self.color = color
		client.sio.emit('SetProbeColors', {self.id:color})

		position = utils.sanitize_vector3(position)
		self.position = position
		client.sio.emit('SetProbePos', {self.id:position})

		angle = utils.sanitize_vector3(angle)
		self.angle = angle
		client.sio.emit('SetProbeAngles', {self.id:angle})
		
		style = utils.sanitize_string(style)
		self.style = style
		#client.sio.emit('SetProbeStyle', {self.id:style})

		scale = utils.sanitize_vector3(scale)
		self.scale = scale
		client.sio.emit('SetProbeSize', {self.id:scale})

	def create(self):
		"""Create probe objects

		Parameters
		----------
		none

		Examples
		--------
		>>> p1 = urchin.probes.Probe()
		"""
		global counter
		counter +=1
		self.id = 'p' + str(counter)
		client.sio.emit('CreateProbes', [self.id])
		self.in_unity = True

	def delete(self):
		"""Delete probe objects

		Parameters
		----------
		references probe being deleted

		Examples
		--------
		>>> p1.delete()
		"""
		client.sio.emit('DeleteProbes', [self.id])
		self.in_unity = False

	def set_color(self,color):
		"""Set colors of probe objects

		Parameters
		----------
		color : string
			string is hex color

		Examples
		--------
		>>> p1.set_color('#FFFFFF')
		"""
		if self.in_unity == False:
			raise Exception("Object does not exist in Unity, call create method first.")
		
		color = utils.sanitize_color(color)
		self.color
		client.sio.emit('SetProbeColors', {self.id:color})

	def set_position(self, probe_positions):
		"""Set probe tip position in ml/ap/dv coordinates in um relative to the CCF (0,0,0) point

		Parameters
		----------
		probe_positions : float list
			value is list of floats in ml/ap/dv in um

		Examples
		--------
		>>> p1.set_position([500,1500,2500])
		"""
		if self.in_unity == False:
			raise Exception("Object does not exist in Unity, call create method first.")
		
		probe_positions = utils.sanitize_vector3(probe_positions)
		self.position = probe_positions
		client.sio.emit('SetProbePos', {self.id:probe_positions})

	def set_angle(self, probe_angles):
		"""Set probe azimuth/elevation/spin angles in degrees

		Azimuth 0 = has the probe facing the AP axis, positive values rotate clockwise
		Elevation 0 = probe is vertical, 90 = horizontal

		Parameters
		----------
		probe_angles : float list
			value is list of floats in az/elev/spin
			
		Examples
		--------
		>>> p1.set_angle([-90,0,0])
		"""
		if self.in_unity == False:
			raise Exception("Object does not exist in Unity, call create method first.")
		probe_angles = utils.sanitize_vector3(probe_angles)
		self.angle = probe_angles
		client.sio.emit('SetProbeAngles', {self.id:probe_angles})

	# def set_probe_style(self,probe_data):
	# 	"""Set probe rendering style

	# 	Style options are:
	# 		"line"
	# 		"probe-tip"
	# 		"probe-silicon"
	# 		"probe"

	# 	Inputs:
	# 	probe_data -- dictionary of probe names and string {'p1':'line'}
	# 	"""
	# 	if self.in_unity == False:
	# 		raise Exception("Object does not exist in Unity, call create method first.")
		
	# 	probe_data = utils.sanitize_string(probe_data)
	# 	self.style = probe_data
	# 	client.sio.emit('SetProbeStyle', {self.id:probe_data})

	def set_scale(self, probe_scale):
		"""Set probe scale in mm units, by default probes are scaled to 70 um wide x 20 um deep x 3840 um tall which is the size of a NP 1.0 probe.

		Parameters
		----------
		probe_scale: float list
			list of floats for width, height, depth
			
		Examples
		--------
		>>> p1.set_scale([0.070, 3.840, 0.020])
		"""
		if self.in_unity == False:
			raise Exception("Object does not exist in Unity, call create method first.")
		probe_scale = utils.sanitize_vector3(probe_scale)
		self.scale = probe_scale
		client.sio.emit('SetProbeSize', {self.id,probe_scale})

	
def create(num_objects):
	"""Create probe objects

	Parameters
	----------
	num_objects: int
		number of probe objects to be created

	Examples
	--------
	>>> probes = urchin.probes.create(2)
	"""
	probe_list = []
	for i in range(num_objects):
		probe_list.append(Probe())
	return probe_list

def delete(probes_list):
	"""Delete probe objects

	Parameters
	----------
	probes_list: list
		list of probe objects to be deleted

	Examples
	--------
	>>> probes.delete([p1,p2])
	"""
	probes_list = utils.sanitize_list(probes_list)
	probe_ids = [x.id for x in probes_list]
	client.sio.emit('DeleteProbes', probe_ids)

def set_colors(probes_list, colors_list):
	"""Set colors of probe objects

	Parameters
	----------
	probes_list: list of probe objects
		list of probe objects to be colored
	colors_list : list of string hex colors
		new hex colors for each probe

	Examples
	--------
	>>> urchin.probes.set_colors(probes,['#FFFFFF','#000000'])
	"""
	probes_list = utils.sanitize_list(probes_list)
	colors_list = utils.sanitize_list(colors_list)

	probe_colors = {}
	for i in range(len(probes_list)):
		probe = probes_list[i]
		if probe.in_unity:
			probe_colors[probe.id] = utils.sanitize_color(colors_list[i])
		else:
			warnings.warn(f"Object with id {probe.id} does not exist in Unity, Please create object {probe.id}.")
	client.sio.emit('SetProbeColors', probe_colors)

def set_positions(probes_list, positions_list):
	"""Set probe tip position in ml/ap/dv coordinates in um relative to the CCF (0,0,0) point
	Parameters
	----------
	probes_list : list of probe objects
	    list of probes being set
	positions_list : list of list of three floats
		vertex positions of each probe in ml/ap/dv in um
      
	Examples
	--------
	>>> urchin.probes.set_positions(probes,[[3,3,3],[2,2,2]])
	"""
	probes_list = utils.sanitize_list(probes_list)
	positions_list = utils.sanitize_list(positions_list)

	probe_pos = {}
	for i in range(len(probes_list)):
		probe = probes_list[i]
		if probe.in_unity:
			probe_pos[probe.id] = utils.sanitize_vector3(positions_list[i])
		else:
			warnings.warn(f"Object with id {probe.id} does not exist. Please create object {probe.id}.")

	client.sio.emit('SetPosition', probe_pos)

def set_angles(probes_list, angles_list):
	"""Set probe azimuth/elevation/spin angles in degrees

	Azimuth 0 = has the probe facing the AP axis, positive values rotate clockwise
	Elevation 0 = probe is vertical, 90 = horizontal

	Parameters
	----------
	probes_list : list of probe objects
		list of probes being set
	probe_angles : list of list of three floats
		value is list of floats in az/elev/spin	
		
	Examples
	--------
	>>> urchin.probes.set_angles(probes,[[-90,0,0],[0,30,0]])
	"""
	probes_list = utils.sanitize_list(probes_list)
	angles_list = utils.sanitize_list(angles_list)

	probe_angle = {}
	for i in range(len(probes_list)):
		probe = probes_list[i]
		if probe.in_unity:
			probe_angle[probe.id] = utils.sanitize_vector3(angles_list[i])
		else:
			warnings.warn(f"Object with id {probe.id} does not exist. Please create object {probe.id}.")

	client.sio.emit('SetProbeAngles', probe_angle)

# def set_probe_styles(probes_list,styles_list):
# 	"""Set probe rendering style

# 	Style options are:
# 		"line"
# 		"probe-tip"
# 		"probe-silicon"
# 		"probe"

# 	Parameters
# 	----------
# 	probes_list : list of probe objects
# 		list of probes being set
# 	styles_list :list of strings
# 		list of strings for probe style
	
# 	Examples
# 	--------
# 	>>> urchin.probes.set_probe_styles(probes,['line','probe-tip'])
# 	"""
# 	probes_list = utils.sanitize_list(probes_list)
# 	styles_list = utils.sanitize_list(styles_list)

# 	probe_styles = {}
# 	for i in range(len(probes_list)):
# 		probe = probes_list[i]
# 		if probe.in_unity:
# 			probe_styles[probe.id] = utils.sanitize_string(styles_list[i])
# 		else:
# 			warnings.warn(f"Object with id {probe.id} does not exist in Unity, Please create object {probe.id}.")
# 	client.sio.emit('SetProbeStyle', probe_styles)

def set_scales(probes_list, scales_list):
	"""Set probe scale in mm units, by default probes are scaled to 70 um wide x 20 um deep x 3840 um tall which is the size of a NP 1.0 probe.

	Parameters
	----------
	probes_list: list of probe objects
		list of probe sizes being set
	scales_list: list of list of three floats
		list of floats for width, height, depth for each probe
		
	Examples
	--------
	>>> urchin.probes.set_scales(probes,[0.070, 3.840, 0.020])
	"""
	probes_list = utils.sanitize_list(probes_list)
	scales_list = utils.sanitize_list(scales_list)

	probe_scale = {}
	for i in range(len(probes_list)):
		probe = probes_list[i]
		if probe.in_unity:
			probe_scale[probe.id] = utils.sanitize_vector3(scales_list[i])
		else:
			warnings.warn(f"Object with id {probe.id} does not exist. Please create object {probe.id}.")

	client.sio.emit('SetProbeSize', probe_scale)