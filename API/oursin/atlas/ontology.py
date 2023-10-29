from .. import client
from .. import utils

import json
from pkg_resources import resource_filename

class Atlas:
    def __init__(self, atlas_name):
        # load the ontology structure file
        self.atlas_name = atlas_name
        self.loaded = False

        data_file_path = resource_filename(__name__, f'data/{atlas_name}.structures.json')
        with open(data_file_path,'r') as f:
            temp = json.load(f)

        self.areas = []
        for structure_data in temp:
            self.areas.append(structure_data['acronym'])
            structure_data['color'] = utils.sanitize_color(structure_data['rgb_triplet'])
            setattr(self, structure_data['acronym'], Structure(structure_data))

    def load(self):
        """Load this atlas
        """
        if self.loaded:
            raise Exception("Atlas is already loaded")
        
        self.loaded = True
        client.sio.emit('LoadAtlas', self.atlas_name)

    def clear(self):
        """Clear all visible areas
        """
        client.sio.emit('Clear', 'area')

    def load_defaults(self):
        """Load the left and right areas
        """
        client.sio.emit('LoadDefaultAreas', "")

    def get_areas(self, area_list):
        """Get the area objects given a list of area acronyms

        Parameters
        ----------
        area_list : list of string
            List of acronyms to get objects for

        Returns
        -------
        list of areas

        Examples
        --------
        >>> area_list = urchin.get_areas(["root", "VISp"])
        """
        return [getattr(self, name) for name in area_list]

    def set_visibilities(self, area_list, area_visibility, sided = "full"):
        """Set visibility of multiple areas at once

        Parameters
        ----------
        area_visibilities : dict {string : bool}
            dictionary of area IDs or acronyms and visibility values

        Examples
        --------
        >>> urchin.ccf25.set_visibilities(urchin.ccf25.get_areas(["root", "VISp"]), [True, False])
        >>> urchin.ccf25.set_visibilities(urchin.ccf25.get_areas(["root", "VISp"]), True)
        """
        area_visibility = utils.sanitize_list(area_visibility, len(area_list))

        data_dict = {}
        for i, area in enumerate(area_list):
            area_name = utils.sanitize_side(area.acronym, sided)
            data_dict[area_name] = area_visibility[i]

        client.sio.emit('SetAreaVisibility', data_dict)

    def set_colors(self, area_list, area_colors, sided="full"):
        """Set color of multiple areas at once.

        Parameters
        ----------
        area_colors : list of colors (hex string or RGB triplet)

        Examples
        --------
        >>> urchin.ccf25.set_visibilities(urchin.ccf25.get_areas(["root", "VISp"]), ['#ff0000', '#00ff00'])
        >>> urchin.ccf25.set_visibilities(urchin.ccf25.get_areas(["root", "VISp"]), [255, 255, 255])
        """
        for i in range(0,len(area_colors)):
            area_colors[i] = utils.sanitize_color(area_colors[i])
        area_colors = utils.sanitize_list(area_colors, len(area_list))

        data_dict = {}
        for i, area in enumerate(area_list):
            area_name = utils.sanitize_side(area.acronym, sided)
            data_dict[area_name] = area_colors[i]

        client.sio.emit('SetAreaColors', data_dict)

    def set_alphas(self, area_list, area_alphas, sided="full"):
        """Set transparency of multiple areas at once. Requires a transparent material. 
        
        Parameters
        ----------
        area_alphas : list of float
            Alpha ranges from 0->1

        Examples
        --------
        >>> urchin.ccf25.set_alphas(urchin.ccf25.get_areas(["root", "VISp"]), [0.15, 0.15])
        >>> urchin.ccf25.set_alphas(urchin.ccf25.get_areas(["root", "VISp"]), [0.5])
        """
        for i in range(0,len(area_alphas)):
            area_alphas[i] = utils.sanitize_float(area_alphas[i])
        area_alphas = utils.sanitize_list(area_alphas, len(area_list))

        data_dict = {}
        for i, area in enumerate(area_list):
            area_name = utils.sanitize_side(area.acronym, sided)
            data_dict[area_name] = area_alphas[i]
        
        client.sio.emit('SetAreaAlpha', data_dict)

    def set_materials(self, area_list, area_materials, sided="full"):
        """Set material of multiple areas at once.

        Material options are
        - 'opaque-lit' or 'default'
        - 'opaque-unlit'
        - 'transparent-lit'
        - 'transparent-unlit'
        
        Parameters
        ----------
        area_colors : dict {string: string}
            Keys are area IDs or acronyms, Values are hex colors

        Examples
        --------
        >>> urchin.ccf25.set_materials(urchin.ccf25.get_areas(["root", "VISp"]), ['transparent-lit', 'opaque-lit'])
        >>> urchin.ccf25.set_materials(urchin.ccf25.get_areas(["root", "VISp"]), ['transparent-lit])
        """
        for i in range(0,len(area_materials)):
            area_materials[i] = utils.sanitize_string(area_materials[i])
        area_materials = utils.sanitize_list(area_materials, len(area_list))

        data_dict = {}
        for i, area in enumerate(area_list):
            area_name = utils.sanitize_side(area.acronym, sided)
            data_dict[area_name] = area_materials[i]

        client.sio.emit('SetAreaMaterial', data_dict)

class Structure:
    """Structure attributes can be accessed as

    >>> structure.name
    >>> structure.acronym
    >>> structure.id
    >>> structure.rgb_triplet
    >>> structure.path
    """
    def __init__(self, structure_data):
        for key, value in structure_data.items():
            setattr(self, key, value)

    def set_test(self):
        print(self)

    def set_visibility(self, visibility, sided = "full"):
        """Set area visibility

        Parameters
        ----------
        visibility : bool
        sided : str, optional
            "full" "left" or "right, by default "full"

        Examples
        --------
        >>> urchin.ccf25.root.set_visibility(True)
        >>> urchin.ccf25.root.set_visibility(True, "left")
        """
        area_name = self.acronym
        if sided == "right":
            area_name += '-rh'
        elif sided == "left":
            area_name += '-lh'

        client.sio.emit('SetAreaVisibility', {area_name:visibility})

    def set_color(self, color, sided = "full"):
        """Set area color.

        Parameters
        ----------
        color : hex string

        Examples
        --------
        >>> urchin.ccf25.root.set_color('#ff0000')
        >>> urchin.ccf25.root.set_color([255, 0, 0], "left")
        """
        color = utils.sanitize_color(color)

        area_name = utils.sanitize_side(self.acronym, sided)

        client.sio.emit('SetAreaColors', {area_name:color})

    # def set_intensity(area_intensities):
    #     """Set color of CCF area models using colormap.

    #     Parameters
    #     ----------
    #     area_intensities : dict {string: float}
    #         keys are area IDs or acronyms, values are hex colors

    #     Examples
    #     --------
    #     >>> urn.set_intensity( {'grey':1.0})
    #     >>> urn.set_intensity({8:1.0})
    #     """
    #     client.sio.emit('SetAreaIntensity', area_intensities)

    # def set_colormap(colormap_name):
    #     """Set colormap used for CCF area intensity mapping


    #     Options are
    #     - cool (default, teal 0 -> magenta 1)
    #     - grey (black 0 -> white 1)
    #     - grey-green (grey 0, light 1/255 -> dark 1)
    #     - grey-purple (grey 0, light 1/255 -> dark 1)
    #     - grey-red (grey 0, light 1/255 -> dark 1)
    #     - grey-rainbow (grey 0, rainbow colors from 1/255 -> 1)

    #     Parameters
    #     ----------
    #     colormap_name : string
    #         colormap name
    #     """
    #     client.sio.emit('SetAreaColormap', colormap_name)

    def set_alpha(self, alpha, sided = "full"):
        """Set transparency. 

        Parameters
        ----------
        alpha : float

        Examples
        --------
        >>> urchin.ccf25.root.set_alpha(0.15)
        >>> urchin.ccf25.root.set_alpha(0.5, "left")
        """
        alpha = utils.sanitize_float(alpha)

        area_name = utils.sanitize_side(self.acronym, sided)

        client.sio.emit('SetAreaAlpha', {area_name:alpha})

    def set_material(self, material, sided = "full"):
        """Set material.

        Material options are
        - 'opaque-lit' or 'default'
        - 'opaque-unlit'
        - 'transparent-lit'
        - 'transparent-unlit'

        Parameters
        ----------
        material: string

        Examples
        ----------
        >>> urchin.ccf25.root.set_material('transparent-lit')
        """
        material = utils.sanitize_string(material)

        area_name = utils.sanitize_side(self.acronym, sided)

        client.sio.emit('SetAreaMaterial', {area_name:material})

    # def set_data(area_data):
    #     """Set the data array for each CCF area model

    #     Data arrays work the same as the set_area_intensity() function but are controlled by the area_index value, which can be set in the renderer or through the API.

    #     Parameters
    #     ----------
    #     area_data : dict {string: float list}
    #         keys area IDs or acronyms, values are a list of floats
    #     """
    #     client.sio.emit('SetAreaData', area_data)

    # def set_data_index(area_index):
    #     """Set the data index for the CCF area models

    #     Parameters
    #     ----------
    #     area_index : int
    #         data index
    #     """
    #     client.sio.emit('SetAreaIndex', area_index)