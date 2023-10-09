"""Files"""

from . import utils
import pandas as pd
import numpy as np
from google.colab import files

def upload_file(file_name, url):
	"""Uploads a file from Google drive to colab

	
	Parameters
	----------
	file_name: string
		name of the file to be uploaded, INCLUDING the file extension

	Examples
	--------
	>>> urchin.io.upload_file("test.txt", "https://drive.google.com/file/d/exampleid123456789/view?usp=sharing")
	"""
	!wget -O {file_name} {url}



#CHANGE NAMES TO LOAD

def image(image_url):
	"""Displays an image from a url.

	
	Parameters
	----------
	url : string
		URL of the image to be displayed

	Examples
	--------
	>>> urchin.io.image('https://picsum.photos/200/300')
	"""
	from IPython.display import Image, display

	image_url = utils.sanitize_string(image_url)
	display(Image(url=image_url))
	

def load_df(file_id):
	"""Loads a pandas dataframe from a csv file on drive

	
	Parameters
	----------
	file_id : string
		Id of the file to be loaded
		The string of numbers and letters after the "file/d/" and before the "/view?usp=sharing" in the url of the file
		Ensure that the file is viewable by anyone with the link

	Examples
	--------
	>>> data = urchin.io.load_df('1Vn5OpFRkEu_GYSmi9kZYXH8WlwmJ6Qs6')
	"""
	file_id = utils.sanitize_string(file_id)
	file_url = f'https://drive.google.com/uc?id={file_id}'
	return pd.read_csv(file_url)


def load_npy(file_id, file_name):
	"""Loads a numpy array from a npy file on drive

	
	Parameters
	----------
	file_id : string
		Id of the file to be loaded
		The string of numbers and letters after the "file/d/" and before the "/view?usp=sharing" in the url of the file
		Ensure that the file is viewable by anyone with the link

	file_name : string
		Name of the file to be loaded, will be saved locally on colab

	Examples
	--------
	>>> test = urchin.io.load_npy('1zE3Vobs5HBH_ne4KOpaPNKZFjhdxl6yQ', 'tester')
	"""
	file_id = utils.sanitize_string(file_id)
	!wget -O {file_name}.npy https://drive.google.com/uc?id={file_id}
	return np.load(f'/content/{file_name}.npy')

def load_parquet(file_id, file_name):
	"""Loads a parquet file from a parquet file on drive

	
	Parameters
	----------
	file_id : string
		Id of the file to be loaded
		The string of numbers and letters after the "file/d/" and before the "/view?usp=sharing" in the url of the file
		Ensure that the file is viewable by anyone with the link

	file_name : string
		Name of the file to be loaded, will be saved locally on colab

	Examples
	--------
		--------
	>>> test = urchin.io.load_parquet("13_YNx5ATSGb5LlV4X24yq6PH-E22mvQX","pq_test")
	"""
	import pandas as pd
	file_id = utils.sanitize_string(file_id)
	!wget -O {file_name}.npy https://drive.google.com/uc?id={file_id}
	return pd.read_parquet(f'/content/{file_name}.npy')

def download_to_csv(df_name):
	"""Downloads a pandas dataframe to a csv file on local machine

	
	Parameters
	----------
	df : pandas dataframe
		Dataframe to be downloaded
		Ensure that the file is viewable by anyone with the link

	Examples
	--------
	>>> urchin.io.download_to_csv(data)
	"""
	csv_file_path = f'/content/{df_name}.csv'
	df_name.to_csv(csv_file_path, index=False)
	files.download(csv_file_path)

def download_file(file_name):
	"""Downloads a file to local machine

	
	Parameters
	----------
	file_name : string
		Name of the file to be downloaded, INCLUDING the file extension

	Examples
	--------
	>>> urchin.io.download_file('test.txt')
	"""
	file_path = f'/content/{file_name}'
	files.download(file_path)