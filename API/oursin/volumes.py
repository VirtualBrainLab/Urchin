"""Volumetric datasets (x*y*z matrix)"""

from . import client
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
		compressed_data = zlib.compress(flattened_data, level=zlib.Z_BEST_COMPRESSION)

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