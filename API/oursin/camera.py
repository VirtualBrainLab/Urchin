"""Camera"""

from . import client
from . import utils

import PIL
from PIL import Image
import io
import json
import numpy as np
		  
receive_fnames = {}
receive_totalBytes = {}
receive_bytes = {}

PIL.Image.MAX_IMAGE_PIXELS = 225000000

def receive_camera_img_meta(data_str):
	"""Handler for receiving metadata about incoming images

	Parameters
	----------
	data_str : string
		JSON with two fields {"name":"", "totalBytes":""}
	"""
	global receive_totalBytes

	data = json.loads(data_str)

	name = data["name"]
	totalBytes = data["totalBytes"]

	receive_totalBytes[name] = totalBytes
	receive_bytes[name] = bytearray()

	print(f'(Camera receive meta) {name} receiving {totalBytes} bytes')

def receive_camera_img(data_str):
	"""Handler for receiving data about incoming images

	Parameters
	----------
	data_str : string
		JSON with two fields {"name":"", "bytes":""}
	"""
	global receive_totalBytes, receive_bytes

	data = json.loads(data_str)

	name = data["name"]
	byte_data = bytes(data["data"])

	receive_bytes[name] = receive_bytes[name] + byte_data

	print(f'(Camera receive) {name} receiving {len(byte_data)} bytes')
	
	if len(receive_bytes[name]) == receive_totalBytes[name]:
		Image.open(io.BytesIO(receive_bytes[name])).save(receive_fnames[name])

		print(f'(Camera receive) {name} complete')
		del receive_fnames[name]
		del receive_totalBytes[name]
		del receive_bytes[name]

## Camera renderer
counter = 0

class Camera:
	def __init__(self):		
		global counter
		counter += 1
		self.id = f'Camera{counter}'
		client.sio.emit('CreateCamera', [self.id])
		self.in_unity = True

	def create(self):
		"""Creates camera
		
		Parameters
		---------- 
		none

		Examples
		>>>c1 = urchin.camera.Camera()
		"""

	def delete(self):
		"""Deletes camera
		
		Parameters
		---------- 
		references object being deleted

		Examples
		>>>c1.delete()
		"""
		if self.in_unity == False:
			raise Exception("Camera is not created. Please create camera before calling method.")
		client.sio.emit('DeleteCamera', [self.id])
		self.in_unity = False

	def set_target_coordinate(self,camera_target_coordinate):
		"""Set the camera target coordinate in CCF space in um relative to CCF (0,0,0), without moving the camera. Coordinates can be negative or larger than the CCF space (11400,13200,8000)

		Parameters
		----------
		camera_target_coordinate : float list
		  list of coordinates in ap, ml, dv in um

		Examples
		--------
		>>>c1.set_target_coordinate([500,1500,1000])
		"""
		if self.in_unity == False:
			raise Exception("Camera is not created. Please create camera before calling method.")
		
		camera_target_coordinate = utils.sanitize_vector3(camera_target_coordinate)
		self.target = camera_target_coordinate
		client.sio.emit('SetCameraTarget', {self.id: camera_target_coordinate})

	# temporarily removed
	# def set_position(self, position, preserve_target = True):
	# 	"""Set the camera position in CCF space in um relative to CCF (0,0,0), coordinates can be outside CCF space. 

	# 	Parameters
	# 	----------
	# 	position : float list
	# 		list of coordinates in ml/ap/dv in um
	# 	preserve_target : bool, optional
	# 		when True keeps the camera aimed at the current target, when False preserves the camera rotation, by default True
		
	# 	Examples
	# 	--------
	# 	>>> c1.set_position([500,1500,1000])
	# 	"""
	# 	if self.in_unity == False:
	# 		raise Exception("Camera is not created. Please create camera before calling method.")
		
	# 	position = utils.sanitize_vector3(position)
	# 	self.position = position
	# 	packet = position.copy()
	# 	packet.append(preserve_target)
	# 	client.sio.emit('SetCameraPosition', {self.id: packet})

	def set_rotation(self, rotation):
		"""Set the camera rotation (pitch, yaw, roll). The camera is locked to a target, so this rotation rotates around the target.

		Rotations are applied in order: roll, yaw, pitch. This can cause gimbal lock.

		Parameters
		----------
		rotation : float list
			list of euler angles to set the camera rotation in (pitch, yaw, roll)

		Examples
		--------
		>>> c1.set_rotation([0,0,0])
		"""
		if self.in_unity == False:
			raise Exception("Camera is not created. Please create camera before calling method.")
		
		rotation = utils.sanitize_vector3(rotation)
		self.rotation = rotation
		client.sio.emit('SetCameraRotation', {self.id: rotation})

	def set_rotation_axial(self, above=True):
		"""Set the camera to the standard axial view. Does not change the window, only the rotation.
		Parameters
		----------
		above : boolean
			default true, the above view

		Examples
		--------
		>>> c1.set_rotation_axial(True)
		"""
		if self.in_unity == False:
			raise Exception("Camera is not created. Please create camera before calling method.")
		
		if above:
			rotation = [0,0,0]
		else:
			rotation = [0,-180,0]
		rotation = utils.sanitize_vector3(rotation)
		self.rotation = rotation
		client.sio.emit('SetCameraRotation', {self.id: rotation})

	
	def set_rotation_coronal(self, back=True):
		"""Set the camera to the standard coronal view. Does not change the window, only the rotation.
		Parameters
		----------
		bool : boolean
			default true, the back view

		Examples
		--------
		>>> c1.set_rotation_coronal(True)
		"""
		if self.in_unity == False:
			raise Exception("Camera is not created. Please create camera before calling method.")
		
		if back:
			rotation = [90,0,0]
		else:
			rotation = [90,-180,0]
		rotation = utils.sanitize_vector3(rotation)
		self.rotation = rotation
		client.sio.emit('SetCameraRotation', {self.id: rotation})

	
	def set_rotation_sagittal(self, left=True):
		"""Set the camera to the standard sagittal view. Does not change the window, only the rotation.
		Parameters
		----------
		bool : boolean
			default true, the left view

		Examples
		--------
		>>> c1.set_rotation_sagittal(True)
		"""
		if self.in_unity == False:
			raise Exception("Camera is not created. Please create camera before calling method.")
		
		if left:
			rotation = [90,-90,0]
		else:
			rotation = [90,90,0]
		rotation = utils.sanitize_vector3(rotation)
		self.rotation = rotation
		client.sio.emit('SetCameraRotation', {self.id: rotation})



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
		client.sio.emit('SetCameraZoom', {self.id: zoom})

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
		client.sio.emit('SetCameraTargetArea', {self.id: camera_target_area})

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
		client.sio.emit('SetCameraPan',{self.id: self.pan})

	def set_mode(self, mode):
		"""Set camera perspective mode

		Parameters
		----------
		mode : string
			"perspective" or "orthographic" (default)
		
		Examples
		--------
		>>> c1.set_mode('perspective')
		"""
		if self.in_unity == False:
			raise Exception("Camera is not created. Please create camera before calling method.")
		self.mode = mode
		client.sio.emit('SetCameraMode', {self.id: mode})

	def set_controllable(self):
		"""Sets camera to controllable
		
		Examples
		--------
		>>> c1.set_controllable()
		"""
		if self.in_unity == False:
			raise Exception("Camera is not created. Please create camera before calling method.")
		self.controllable = True
		client.sio.emit('SetCameraControl', self.id)
		
	def screenshot(self, filename, size=[1024,768]):
		"""Capture a screenshot

		Parameters
		----------
		filename: string
			Filename to save to, relative to local path
		size : list, optional
			Size of the screenshot, by default [1024,768]
		"""
		global receive_fnames
		print(self.id)
		print(filename)
		receive_fnames[self.id] = filename

		if size[0] > 15000 or size[1] > 15000:
			raise Exception('(urchin.camera) Screenshots can''t exceed 15000x15000')
		
		client.sio.emit('RequestCameraImg', json.dumps({"name":self.id, "size":size}))

def set_light_rotation(angles):
    angles = utils.sanitize_vector3(angles)
    print(angles)
    print(isinstance(angles,list))
    client.sio.emit('SetLightRotation', angles)

def set_light_camera(camera_name = None):
    if (camera_name is None):
        client.sio.emit('ResetLightLink')
    else:
        client.sio.emit('SetLightLink', camera_name)
	
main = Camera(main = True)