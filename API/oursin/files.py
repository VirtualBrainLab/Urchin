"""Files"""

from . import utils
import warnings
#for image
from IPython.display import Image
#for sheets
import requests
import gdown
import numpy as np
#for parquet
import pyarrow.parquet as pq

#Must have requests and gdown installed beforehand



def image(url):
	"""Displays an image from a url.

	
	Parameters
	----------
	url : string
		URL of the image to be displayed

	Examples
	--------
	>>> urchin.files.create('https://picsum.photos/200/300')
	"""
	url = utils.sanitize_string(url)
	Image(requests.get(url).content)
	

def sheets_to_npy(file_id, output_path):
	"""Downloads and returns a npy file from a Google Sheets file id.
	
	Parameters
	----------
	file_id : string
        id of the file to be downloaded from Google Sheets
	output_path : string
	    destination path for file in Colab/file name, must end in .npy
		
		Examples
		--------
		>>> urchin.files.sheets_to_npy('1A1l7OWbXFlE_4tUQ-WjhljzvN2r2_QrW', 'atlas.npy')
        """
	file_id = utils.sanitize_string(file_id)
	output_path = utils.sanitize_string(output_path)
	url = 'https://drive.google.com/uc?id=' + file_id
	gdown.download(url, output_path, quiet=False)
	return np.load(output_path)

def sheets_to_pq(file_id, output_path):
	"""Downloads and returns a parquet file from a Google Sheets file id.
    
    Parameters
    ----------
    file_id : string
        id of the file to be downloaded from Google Sheets
    output_path : string
	    destination path for file in Colab/file name, must end in .parquet
        
        Examples
        --------
        >>> urchin.files.sheets_to_pq(''1A1l7OWbXFlE_4tUQ-WjhljzvN2r2_QrW', 'atlas.parquet')
        """
	file_id = utils.sanitize_string(file_id)
	output_path = utils.sanitize_string(output_path)
	url = 'https://drive.google.com/uc?id=' + file_id
	gdown.download(url, output_path, quiet=False)
	return pq.read_table(output_path)
