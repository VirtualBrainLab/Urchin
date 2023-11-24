"""Volumetric datasets (x*y*z matrix)"""

from . import client
from . import utils
import numpy as np
import zlib
import json

counter = 0

CHUNK_LIMIT = 1000000

class Volume:
	"""Volumetric dataset represented in a compressed format by using a colormap to translate
	uint8 x/y/z data into full RGB color.
	"""
	def __init__(self, volume_data, colormap = None):
		global counter
		self.id = f'volume{counter}'
		counter += 1

		volume_data[np.isnan(volume_data)] = 255

		flattened_data = volume_data.flatten().astype(np.uint8).tobytes()
		compressed_data = zlib.compress(flattened_data)

		self.n_compressed_bytes = len(compressed_data)
		self.visible = True
		if colormap is not None:
			self.colormap = colormap
		else:
			self.colormap = ['#000000'] * 255

		self.update()

		# send data packets
		# split data into chunks
		n_chunks = int(np.ceil(self.n_compressed_bytes / CHUNK_LIMIT))
		print(f'Data fits in {n_chunks} chunks of 1MB or less')
		offset = 0
		for chunk in range(n_chunks):
			# get the data
			chunk_size = np.min((self.n_compressed_bytes - offset, CHUNK_LIMIT))

			chunk_data = {}
			chunk_data['name'] = self.id
			chunk_data['offset'] = int(offset)
			chunk_data['compressedByteChunk'] = list(compressed_data[offset : offset + chunk_size])
			client.sio.emit('SetVolumeData', json.dumps(chunk_data))

			offset += chunk_size

	def update(self):
		data = {}
		data['name'] = self.id
		data['nCompressedBytes'] = self.n_compressed_bytes
		data['visible'] = self.visible
		data['colormap'] = self.colormap

		client.sio.emit('UpdateVolume', json.dumps(data))

	def delete(self):
		client.sio.emit('DeleteVolume', self.id)

def compress_volume(volume_data, n_colors=254):
	"""Compress a volume of float data into a uint8 volume by quantiles.

	NaN values are mapped to 255 (transparent) for Urchin.

	This is required for use with the urchin.volume.Volume object type.

	Parameters
	----------
	volume_data : float volume
		3D matrix of float data
	n_colors : int (optional)
		Default to 254, number of un-reserved colors. 255 must always be reserved for NaN / transparency

	Returns
	-------
	(uint8 volume, float[] map)
	"""
	quantiles = np.quantile(volume_data.flatten(), np.linspace(0,1,n_colors))

	out = np.digitize(volume_data, quantiles, right=True).astype(np.uint8)
	out[np.isnan(volume_data)] = 255

	return out.astype(np.uint8), quantiles

def colormap(colormap_name='greens', reserved_colors=[], datapoints=None):
	"""Build a colormap

	This function has two parts:
	1. It builds a standard colormap in indexes 0->n_colors
	2. It leaves "reserved" colors at the end, by default this is just 255 which becomes 
	transparent in Urchin. But you can add a list of additional colors which will be added
	to the end of the colormap. The order will match the list you pass in, so e.g.
	3. If you pass in datapoints, it will generate a non-uniform colormap going from the min
	to maximum value.

	indexes: 	[0->252, 	253->254, 				255]
	colors: 	[greens, 	your reserved colors, 	transparent]

	Colormap options
	----------
		reds: 0->255 R channel
		greens: 0->255 G channel
		blues: 0->255 B channel

	Parameters
	----------
	colormap_name : str, optional
		_description_, by default 'greens'
	reserved_colors : _type_, optional
		_description_, by default None

	Returns
	-------
	list of string
		List of colormap hex colors in Urchin-compatible format
	"""
	colormap = []

	n_unreserved = 254 - len(reserved_colors)

	datapoints = (datapoints - np.min(datapoints)) / (np.max(datapoints) - np.min(datapoints))
	datapoints = datapoints / np.max(datapoints)

	for i in range(0, n_unreserved):
		if datapoints is not None:
			v = int(np.round(datapoints[i] * 255))
		else:
			v = int(np.round(i / n_unreserved * 255))

		if colormap_name == 'reds':
			colormap.append(utils.rgba_to_hex((v,0,0,255)))
		elif colormap_name == 'greens':
			colormap.append(utils.rgba_to_hex((0,v,0,255)))
		elif colormap_name == 'blues':
			colormap.append(utils.rgba_to_hex((0,0,v,255)))
		else:
			raise Exception(f'{colormap_name} is not a valid colormap option')

	colormap.extend(reserved_colors)

	return colormap