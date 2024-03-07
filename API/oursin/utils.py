"""Sanitizing inputs to send through API"""
import numpy as np
from enum import Enum

### ENUMS
class Side(Enum):
    LEFT = -1
    FULL = 0
    RIGHT = 1

### SANITIZING FUNCTIONS

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
    try:
        vector_list = list(map(float, vector))
    except (TypeError, ValueError):
        raise ValueError("Input vector must be convertible to a list of three floats.")

    # Check if the length is exactly three
    if len(vector_list) != 3:
        raise ValueError("Input vector must have exactly three elements.")

    return vector_list


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
        try:
            return float(value)
        except:
            raise Exception("Value could not be coerced to a float.")

def sanitize_material(material):
    if isinstance(material, str):
        return(material)
    else:
        raise Exception("Material is not properly passed in as a string. Please pass in material as a string.")

def sanitize_list(input, length=0):
    """Guarantee that a list is of at least size length, or try to broadcast to that size

    Parameters
    ----------
    input : list
    length : int, optional
        length to broadcast to, by default 0

    Returns
    -------
    list
    """
    if length > 0 and not isinstance(input, list):
        input = [input] * length

    if not isinstance(input, list):
        raise Exception("List parameter needs to be a list.")

    return input
    
def sanitize_string(string):
    if isinstance(string, str):
        return(string)
    else:
        raise Exception("Input is not properly passed in as a string. Please pass in input as a string.")
    
def sanitize_side(acronym, sided):
    if sided == "full":
        return acronym
    elif sided == "left":
        return f'{acronym}-lh'
    elif sided == "right":
        return f'{acronym}-rh'
    else:
        raise Exception(f'Sided enum {sided} not properly defined, should be full/left/right')
    
def rgb_to_hex(rgb):
    return '#%02x%02x%02x' % rgb
    
def rgba_to_hex(rgba):
    return '#%02x%02x%02x%02x' % rgba

def list_of_list2vector3(list_of_list):
    """Convert a list of lists to a list of vector3 {"x","y","z"} objects

    Parameters
    ----------
    list_of_list : list of length 3 lists
        _description_
    """
    return [{"x":str(data[0]), "y":str(data[1]), "z":str(data[2])} for data in list_of_list]

def formatted_vector3(list_of_float):
    """Convert a list of floats to a formatted vector3 dict

    Parameters
    ----------
    list_of_float : list
    """
    return {"x":str(list_of_float[0]), "y":str(list_of_float[1]), "z":str(list_of_float[2])}
