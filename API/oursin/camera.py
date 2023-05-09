"""Camera"""

from . import client
import warnings
#import utils

## Camera renderer
counter = 0
class Camera:
	def __init__(self, target = [0,0,0], position = [0,0,0], preserve_target = True, rotation = [0,0,0], zoom = 1, pan_x = 3, pan_y = 4):
		self.create()

		#in theory, the target value can stand for either coordinate or area? (and taking coordinate as default)
		target_coordinate = utils.sanitize_vector3(target)
		self.target = target_coordinate
		client.sio.emit('SetCameraTarget', self.target)

		position = utils.sanitize_vector3(position)
		self.position = position
		packet = position.copy()
		packet.append(preserve_target)
		client.sio.emit('SetCameraPosition', packet)

		rotation = utils.sanitize_vector3(rotation)
		self.rotation = rotation
		client.sio.emit('SetCameraRotation', self.rotation)

		self.zoom = zoom
		client.sio.emit('SetCameraZoom', self.zoom)

		# target_area = utils.sanitize_string(target_area)
		# self.target_area = target_area
		# client.sio.emit('SetCameraTargetArea', self.target_area)

		self.pan = [pan_x, pan_y]
		client.sio.emit('SetCameraPan', self.pan)

	def create(self):
		"""Creates camera
		
		Parameters
		---------- 
		none

		Examples
		>>>c1 = urchin.camera.Camera()
		"""
		global counter
		counter += 1
		self.id = str(counter)
		#client.sio.emit('CreateCamera', [self.id])
		self.in_unity = True

	def delete(self):
		"""Deletes camera
		
		Parameters
		---------- 
		references object being deleted

		Examples
		>>>c1.delete()
		"""
		#client.sio.emit('DeleteCamera', [self.id])
		self.in_unity = False

	def set_target_coordinate(self,camera_target_coordinate):
		"""Set the camera target coordinate in CCF space in um relative to CCF (0,0,0), without moving the camera. Coordinates can be negative or larger than the CCF space (11400,13200,8000)

		Parameters
		----------
		camera_target_coordinate : float list
		  list of coordinates in ml/ap/dv in um

		Examples
		--------
		>>>c1.set_target([500,1500,1000])
		"""
		if self.in_unity == False:
			raise Exception("Camera is not created. Please create camera before calling method.")
		
		camera_target_coordinate = utils.sanitize_vector3(camera_target_coordinate)
		self.target = camera_target_coordinate
		client.sio.emit('SetCameraTarget', camera_target_coordinate)

	def set_position(self, position, preserve_target = True):
		"""Set the camera position in CCF space in um relative to CCF (0,0,0), coordinates can be outside CCF space. 

		Parameters
		----------
		position : float list
			list of coordinates in ml/ap/dv in um
		preserve_target : bool, optional
			when True keeps the camera aimed at the current target, when False preserves the camera rotation, by default True
		
		Examples
		--------
		>>> c1.set_position([500,1500,1000])
		"""
		if self.in_unity == False:
			raise Exception("Camera is not created. Please create camera before calling method.")
		
		position = utils.sanitize_vector3(position)
		self.position = position
		packet = position.copy()
		packet.append(preserve_target)
		client.sio.emit('SetCameraPosition', packet)

	def set_rotation(self, rotation):
		"""Set the camera rotation (pitch, yaw, spin). The camera is locked to a target, so this rotation rotates around the target.

		Rotations are applied in order: pitch, then yaw, then spin.

		Parameters
		----------
		rotation : float list
			list of euler angles to set the camera rotation in (pitch, yaw, spin)

		Examples
		--------
		>>> c1.set_rotation([0,0,0])
		"""
		if self.in_unity == False:
			raise Exception("Camera is not created. Please create camera before calling method.")
		
		rotation = utils.sanitize_vector3(rotation)
		self.rotation = rotation
		client.sio.emit('SetCameraRotation', rotation)

	def set_zoom(self,zoom):
		"""Set the camera zoom. 

		Parameters
		----------
		zoom : float	
			camera zoom parameter

		Examples
		--------
		>>> c1.set_zoom(1.0)
		"""
		if self.in_unity == False:
			raise Exception("Camera is not created. Please create camera before calling method.")
		
		self.zoom = zoom
		client.sio.emit('SetCameraZoom', zoom)

	def set_target_area(self, camera_target_area):
		"""Set the camera rotation to look towards a target area

		Note: some long/curved areas have mesh centers that aren't the 'logical' center. For these areas, calculate a center yourself and use set_camera_target.

		Parameters
		----------
		camera_target_area : string
			area ID or acronym, append "-lh" or "-rh" for one-sided meshes
		Examples
		--------
		>>> c1.set_target_area("grey-l") 
		"""
		if self.in_unity == False:
			raise Exception("Camera is not created. Please create camera before calling method.")
		
		camera_target_area
		self.target = camera_target_area
		client.sio.emit('SetCameraTargetArea', camera_target_area)

	def set_pan(self,pan_x, pan_y):
		"""Set camera pan coordinates

		Parameters
		----------
		pan_x : float
			x coordinate
		pan_y : float
			y coordinate
		
		Examples
		--------
		>>> c1.set_pan(3.0, 4.0)
		"""
		if self.in_unity == False:
			raise Exception("Camera is not created. Please create camera before calling method.")
		
		self.pan = [pan_x, pan_y]
		client.sio.emit('SetCameraPan',self.pan)