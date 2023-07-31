"""Calcium"""
from . import client
import warnings
from . import utils
from PIL import Image
from typing import List
import io

receive_fname = ''
receive_count = 0
receive_data = []

CHUNK_SIZE = 1000000

counter = 0
class FOV:
    def __init__(self, position=[0.0, 0.0, 0.0], offset=0.0, texture_file=None):
        self.create()

        position = utils.sanitize_vector3(position)
        self.set_position(position)
        self.set_offset(offset)
        if texture_file: self.set_texture(texture_file)

    def create(self):
        """Creates FOVs

        Parameters
        ---------- 
        none

        Examples
        >>>f1 = urchin.fov.FOV()
        """
        global counter
        counter += 1
        self.id = 'fov' + str(counter)
        client.sio.emit('CreateFOV',[self.id])
        self.in_unity = True

    def delete(self):
        """Deletes fovs

        Parameters
        ---------- 
        references object being deleted

        Examples
        >>>f1.delete()
        """
        client.sio.emit('DeleteFOV',[self.id])
        self.in_unity = False

    def set_position(self,position):
        """Set the position of fov in ml/ap/dv coordinates relative to the CCF (0,0,0) point

        Parameters
        ---------- 
        position : list of three floats
        	vertex positions of the fov relative to the CCF point

        Examples
        --------
        >>>f1.set_position([2,2,2])
        """
        if self.in_unity == False:
            raise Exception("fov does not exist in Unity, call create method first.")

        position = utils.sanitize_vector3(position)
        self.position = position
        client.sio.emit('SetFOVPos',{self.id: position})
    
    def set_texture(self,texture_file):
        """Set the texture of fov from an image

        Parameters
        ----------
        texture_file : str, file name of FOV image

        Examples
        --------
        >>>f1.set_texture('fov1.png')
        """
        if self.in_unity == False:
            raise Exception("fov does not exist in Unity, call create method first.")

        texture_file = utils.sanitize_string(texture_file)
        self.texture_file=texture_file

        img = Image.open(texture_file)
        # Convert img to bytes
        img_bytes = io.BytesIO()
        img.save(img_bytes,format=img.format)
        img_bytes.seek(0)
        # Split bytes into chunks
        chunks = []
        chunk = []
        while chunk:
            chunk = img_bytes.read(CHUNK_SIZE)
            chunks.append(chunk)
        # Send img by chunk
        for i,chunk in enumerate(chunks):
            immediate_apply = True if i==len(chunks)-1 else False
            client.sio.emit('SetFOVTextureDataMeta', [self.id,i,immediate_apply])
            client.sio.emit('SetFOVTextureData',chunk)

    def set_offset(self,fov_offsets):
        client.sio.emit('SetFOVOffset',fov_offsets)


def create(num_fovs):
    """Create fov objects

    Note: fovs must be created before setting other values

    Parameters
    ----------
    num_fovs : int
        number of new fov objects

    Examples
    --------
    >>> fovs = urchin.fovs.create(3)
    """
    fov_ids = []
    for i in range(num_fovs):
        fov = FOV()
        fov_ids.append(fov.id)
    return fov_ids

def delete(fovs_list):
    """Delete fov objects

    Parameters
    ----------
    fov_names : list of fov objects
    list of fovs being deleted

    Examples
    --------
    >>> urchin.fovs.delete()
    """
    fovs_list = utils.sanitize_list(fovs_list)
    for fov in fovs_list:
        if fov.in_unity:
            fov.delete()
        else:
            warnings.warn(f"fov with id {fov.id} does not exist in Unity, call create method first.")

    fovs_ids = [x.id for x in fovs_list]
    client.sio.emit('DeleteFOVs', fovs_ids)

def set_positions(fovs_list, positions_list):
    """Set the position of fov in ml/ap/dv coordinates relative to the CCF (0,0,0) point

    Parameters
    ----------
    fovs_list : list of fov objects
        list of fovs being moved
    positions : list of list of three floats
        list of positions of fovs

    Examples
    --------
    >>> urchin.fovs.set_positions([f1,f2,f3], [[1,1,1],[2,2,2],[3,3,3]])
    """
    fovs_list = utils.sanitize_list(fovs_list)
    positions_list = utils.sanitize_list(positions_list)

    for fov,position in zip(fovs_list, positions_list):
        if fov.in_unity:
            fov.set_position(position)
        else:
            warnings.warn(f"fov with id {fov.id} does not exist in Unity, call create method first.")

def set_textures(fovs_list, texture_files_list):
    """Set the position of fov in ml/ap/dv coordinates relative to the CCF (0,0,0) point

    Parameters
    ----------
    fovs_list : list of fov objects
        list of fovs being moved
    positions : list of list of three floats
        list of positions of fovs

    Examples
    --------
    >>> urchin.fovs.set_textures([f1,f2,f3], ['fov1.png','fov'])
    """
    fovs_list = utils.sanitize_list(fovs_list)
    texture_files_list = utils.sanitize_list(texture_files_list)

    for fov,texture in zip(fovs_list,texture_files_list):
        if fov.in_unity:
            fov.set_texture(texture)
        else:
            warnings.warn(f"fov with id {fov.id} does not exist in Unity, call create method first.")