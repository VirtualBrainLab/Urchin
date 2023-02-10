"""Volumetric datasets (x*y*z matrix)"""

from . import client
import numpy as np

def create(volume_name):
	"""Create an empty volume data matrix in the renderer.

	Note: you must call create_volume and set_volume_colormap before setting data.

	Parameters
	----------
	volume_name : string
		volume name

	Examples
	--------
	>>> urn.create('histology')
	"""
	client.sio.emit('CreateVolume', volume_name)

def delete(volume_name):
	"""Delete a volume data matrix.

	Parameters
	----------
	volume_name : string
		volume name

	Examples
	--------
	>>> urn.delete('histology')
	"""
	client.sio.emit('DeleteVolume', volume_name)

	
def set_colormap(volume_name, colormap):
	"""Set the colormap for a volume, maximum of 254 values

	Note: index 255 is reserved by the renderer for transparency.

	Parameters
	----------
	volumeName : string
		volume name
	volumeData : list of string
		list of hex colors, values can be RGB or RGBA
		
	Examples
	--------
	>>> urn.set_colormap('histology',['#800000','#FF0000'])
	"""
	data_packet = colormap.copy()
	data_packet.insert(0,volume_name)
	client.sio.emit('SetVolumeColormap', data_packet)

def set_visibility(volume_name, volume_visibility):
	"""Change the visibility of a volume

	Parameters
	----------
	volumeName : string
		Volume name
	volumeVisibility : bool
		New visibility setting
		
	Examples
	--------
	>>> urn.set_visibility('histology', True)
	"""
	client.sio.emit('SetVolumeVisibility', [volume_name, volume_visibility])

def set_data(volume_name, volumeData):
	"""Set the data for a volume using uint8 values from 0->254 (255 is reserved for transparent).
	Sending your data as any type other than np.uint8 is potentially unsafe.
	Data will be remapped in the renderer according to the active colormap.
	nan values will be set to transparent.

	Note: this function slices the data by depth and sends data slice-by-slice, it may take a long time to run.

	Parameters
	----------
	volumeData : [string, numpy matrix]
		Name of the volume and the volume itself. Volume should be [528, 320, 456] with +x = posterior, +y = ventral, +z = right
	"""
	localData = volumeData.copy()
	localData[np.isnan(localData)] = 255
	localData = localData.astype(np.uint8)

	ndepth = localData.shape[2]

	for di in np.arange(0,ndepth):
		if di == (ndepth-1):
			set_slice_data(volume_name, int(ndepth-1-di), localData[:,:,di].flatten('F').tobytes(), True)
		else:
			set_slice_data(volume_name, int(ndepth-1-di), localData[:,:,di].flatten('F').tobytes())

def set_slice_data(volume_name, slice, volume_bytes, override_gpu_apply = False):
	"""Set a single slice of data in a volume

	Parameters
	----------
	volumeName : string
		name of volume
	slice : int
		depth slice in volume (on lr dimension)
	volume_bytes : bytes
		flattened bytes array from slice
	override_gpu_apply : bool, optional
		immediately set the GPU texture data after this slice is sent, by default False
	"""
	client.sio.emit('SetVolumeDataMeta', [volume_name, slice, override_gpu_apply])
	client.sio.emit('SetVolumeData', volume_bytes)

######################
# VOLUMES: Allen CCF #
######################

# special case of the regular volume visibility function
def set_visibility_allen(allenVisibility):
	"""Set the visibility of the Allen CCF annotation volume

	Parameters
	----------
	allenData : bool
		visibility of Allen CCF volume
	"""
	set_visibility('allen',allenVisibility)
