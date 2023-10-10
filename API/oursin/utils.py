"""Sanitizing inputs to send through API"""
import numpy as np

def sanitize_vector3(vector):
  """Guarantee that a vector is a vector 3, or raise an exception

  Parameters
  ----------
  input : any
      arbitrary input parameter

  Returns
  -------
  list
      vector3 as a list [x,y,z]

  Raises
  ------
  Exception
      Failed to coerce input to a length 3 list
  """
    #if statement to check type and then convert to vector3
  if(len(vector)!=3):
    raise Exception(f'Expected a vector3 but received length {len(vector)}.')
  
  if isinstance(vector, list):
      return vector
  
  if isinstance(vector, tuple) or isinstance(vector, np.ndarray):
      return list(vector)
  else:
      raise Exception('Vector3 failed to be coerced properly')



def sanitize_color(color):
    if isinstance(color, str): #if color is string
        if(color[0] == '#'): #if color has # prefix
            return(color)
        else: #adding # prefix if not present
            color = '#' + color
            return(color)    
    
    if isinstance(color, list) or isinstance(color, tuple):#if list
        # check first if you're dealing with floats between 0 and 1
        if isinstance(color[0], float):
            # we have floats, check which range we have
            if all([(x <= 1 and x >= 0) for x in color]):
                # range 0->1, re-scale to 0->255 as ints
                color = [int(255*x) for x in color]
            elif all([(x <= 255 and x >= 0) for x in color]):
                # range 0->255, round to ints
                color = [int(np.rint(x)) for x in color]
            else:
                raise Exception('Color value ranges need to conform to [R,G,B] formatted either as 0->1 or 0->255')

        # convert list of ints to a hex string
        hex_color = '#' + ('{:02X}' * 3).format(color[0], color[1], color[2])
        return(hex_color)
    
    else:
        raise Exception('Failed to re-format color input as a hex code. Please enter either an RGB list or tuple, or a hex code')

def sanitize_float(value):
    if isinstance(value, float):
        return value
    else:
        raise Exception("Parameter needs to be passed as a float.")

def sanitize_material(material):
    if isinstance(material, str):
        return(material)
    else:
        raise Exception("Material is not properly passed in as a string. Please pass in material as a string.")

def sanitize_list(input, length=0):
    # resize to match list size
    if length > 0 and len(input) != length:
        input = input * length

    if isinstance(input,list):
        return input
    else:
        return(list(input))
    
def sanitize_string(string):
    if isinstance(string, str):
        return(string)
    else:
        raise Exception("Input is not properly passed in as a string. Please pass in input as a string.")
    
def sanitize_drive_url(url):
    url = sanitize_string(url)
    if "drive.google.com" in url and "file/d/" in url and "/view?" in url:
        parts = url.split('/')
        index = parts.index('d')
        file_id = parts[index + 1] #extracting the file id
        download_link = f"https://drive.google.com/uc?id={file_id}"
        return download_link
    else:
        raise Exception("The url is not a valid google drive url. Please make sure the url is a valid google drive url.")
    
def sanitize_extension(file_name): #NOT VERIFIED
    file_name = sanitize_string(file_name)
    if file_name.endswith(".parquet") or file_name[-4]== ".":
        return(file_name)
    else:
        raise Exception("The file name is not valid. Please make sure the file name is valid.")
