"""Sanitizing inputs to send through API"""
import numpy as np

def sanitize_vector3(input):
#if statement to check type and then convert to vector3
    if(len(input)!=3):
        raise Exception("Did not recieve proper number of inputs, please pass in 3 arguments.")
    if isinstance(input, tuple) or isinstance(input, np.array):
        return(list(input))#vector3



def sanitize_color(color):
    if isinstance(color, str): #if color is string
        if(color.substring(0) == '#'): #if color has # prefix
            return(color)
        else: #adding # prefix if not present
            color = '#' + color
            return(color)
    if isinstance(color, list):#if list
        if isinstance(color[0], float):
            for i in range(len(color)):
                color[i] = int(255*color[i]) 
        hex_color = ('{:02X}' * 3).format(color[0], color[1], color[2])
        hex_color = '#' + hex_color
        return(hex_color)


def sanitize_material(material):
    if isinstance(material, str):
        return(material)
    else:
        raise Exception("Material is not properly passed in as a string. Please pass in material as a string.")

def sanitize_list(input):
    if isinstance(input,list):
        return input
    else:
        return(list(input))
    
def sanitize_string(string):
    if isinstance(string, str):
        return(string)
    else:
        raise Exception("Input is not properly passed in as a string. Please pass in input as a string.")