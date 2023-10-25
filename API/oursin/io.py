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
	url: string
		the sharing link of the file to be uploaded

	Examples
	--------
	>>> urchin.io.upload_file("test.txt", "https://drive.google.com/file/d/exampleid123456789/view?usp=sharing")
	"""
	file_name = utils.sanitize_extension(file_name)
	url = utils.sanitize_drive_url(url)
	!wget -O {file_name} {url}



#CHANGE NAMES TO LOAD

def image(image_url):
	"""Displays an image from a url.

	
	Parameters
	----------
	url : string
		URL of the image to be displayed
		if the image is on google drive, use the sharing link + sanitization gdrive function

	Examples
	--------
	>>> urchin.io.image('https://picsum.photos/200/300')
	"""
	from IPython.display import Image, display

	#image_url = utils.sanitize_drive_url(image_url)

	display(Image(url=image_url))
	

def load_df(url):
	"""Loads a pandas dataframe from a csv file on drive

	
	Parameters
	----------
	url : string
		the sharing link of the file to be uploaded
		Ensure that the file is viewable by anyone with the link

	Examples
	--------
	>>> probes_data = urchin.io.load_df('https://drive.google.com/file/d/1Vn5OpFRkEu_GYSmi9kZYXH8WlwmJ6Qs6/view?usp=drive_link')
	"""
	url = utils.sanitize_drive_url(url)
	return pd.read_csv(url)


def load_npy(url, file_title):
	"""Loads a numpy array from a npy file on drive

	
	Parameters
	----------
	url : string
		the sharing link of the file to be uploaded
		Ensure that the file is viewable by anyone with the link

	file_title : string
		Name of the file to be loaded, will be saved locally on colab

	Examples
	--------
	>>> test = urchin.io.load_npy('https://drive.google.com/file/d/1zE3Vobs5HBH_ne4KOpaPNKZFjhdxl6yQ/view?usp=drive_link', 'tester')
	"""
	file_title = utils.sanitize_string(file_title)
	url = utils.sanitize_drive_url(url)
	!wget -O {file_title}.npy {url}
	return np.load(f'/content/{file_title}.npy')

def load_parquet(url, file_title):
	"""Loads a parquet file from a parquet file on drive

	
	Parameters
	----------
	url : string
		the sharing link of the file to be uploaded
		Ensure that the file is viewable by anyone with the link

	file_title : string
		Name of the file to be loaded, will be saved locally on colab

	Examples
	--------
	>>> test = urchin.io.load_parquet("https://drive.google.com/file/d/13_YNx5ATSGb5LlV4X24yq6PH-E22mvQX/view?usp=drive_link","pq_test")
	"""
	file_title = utils.sanitize_string(file_title)
	url = utils.sanitize_drive_url(url)
	!wget -O {file_title}.parquet {url}
	return pd.read_parquet(f'/content/{file_title}.parquet')

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
	file_name = utils.sanitize_extension(file_name)
	file_path = f'/content/{file_name}'
	files.download(file_path)