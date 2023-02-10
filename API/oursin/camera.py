"""Camera"""

from . import client

def set_target(camera_target_coordinate):
	"""Set the camera target coordinate in CCF space in um relative to CCF (0,0,0), without moving the camera. Coordinates can be negative or larger than the CCF space (11400,13200,8000)

	Parameters
	----------
	camera_target_coordinate : float list
		list of coordinates in ml/ap/dv in um

	Examples
	--------
	>>> urn.set_target([500,1500,1000])
	"""
	client.sio.emit('SetCameraTarget', camera_target_coordinate)

def set_position(camera_pos, preserve_target = True):
	"""Set the camera position in CCF space in um relative to CCF (0,0,0), coordinates can be outside CCF space. 

	Parameters
	----------
	camera_pos : float list
		list of coordinates in ml/ap/dv in um
	preserve_target : bool, optional
		when True keeps the camera aimed at the current target, when False preserves the camera rotation, by default True
	
	Examples
	--------
	>>> urn.set_position([500,1500,1000])
	"""
	packet = camera_pos.copy()
	packet.append(preserve_target)
	client.sio.emit('SetCameraPosition', packet)

def set_rotation(pitch, yaw, spin):
	"""Set the camera rotation (pitch, yaw, spin). The camera is locked to a target, so this rotation rotates around the target.

	Rotations are applied in order: pitch, then yaw, then spin.

	Parameters
	----------
	camera_rot : float list
		list of euler angles to set the camera rotation in (pitch, yaw, spin)
	"""
	client.sio.emit('SetCameraRotation', [pitch, yaw, spin])

def set_zoom(zoom):
	"""Set the camera zoom. 

	Parameters
	----------
	zoom : float	
		camera zoom parameter
	"""
	client.sio.emit('SetCameraZoom', zoom)

def set_target_area(camera_target_area):
	"""Set the camera rotation to look towards a target area

	Note: some long/curved areas have mesh centers that aren't the 'logical' center. For these areas, calculate a center yourself and use set_camera_target.

	Parameters
	----------
	camera_target_area : string
		area ID or acronym, append "-lh" or "-rh" for one-sided meshes
	"""
	client.sio.emit('SetCameraTargetArea', camera_target_area)

def set_pan(pan_x, pan_y):
	"""Set camera pan coordinates

	Parameters
	----------
	pan_x : float
		x coordinate
	pan_y : float
		y coordinate
	
	Examples
	--------
	>>> urn.set_pan(3.0, 4.0)
	"""
	client.sio.emit('SetCameraPan', [pan_x, pan_y])