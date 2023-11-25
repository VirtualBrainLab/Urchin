from .. import client
from .. import utils
from pathlib import Path

import json

class CustomAtlas:
    def __init__(self, atlas_name, atlas_dimensions, atlas_resolution):
        self.atlas_name = atlas_name

        data = {}
        data['name'] = atlas_name
        data['dimensions'] = utils.sanitize_vector3(atlas_dimensions)
        data['resolution'] = utils.sanitize_vector3(atlas_resolution)

        print(data)

        client.sio.emit('CustomAtlas', json.dumps(data))

class Atlas:
    def __init__(self, atlas_name):
        # load the ontology structure file
        self.atlas_name = atlas_name
        self.loaded = False

        current_script_directory = Path(__file__).resolve().parent
        
        data_file_path = f'{current_script_directory}/data/{atlas_name}.structures.json'
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
            print("(Warning) Atlas was already loaded, the renderer can have issues if you try to load an atlas twice.")
        
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
        areas = []
        for name in area_list:
            try:
                area = getattr(self, name)
                areas.append(area)
            except:
                print(f'(Warning): Area {name} couldn''t be found in this atlas!')
        return areas

    def set_visibilities(self, area_list, area_visibility, side = utils.Side.FULL):
        """Set visibility of multiple areas at once

        Parameters
        ----------
        area_list : list of string
            List of acronyms
        area_visibility : list of bool
        sided : string, optional
            Brain area side to control "full"/"left"/"right", default = "full"

        Examples
        --------
        >>> urchin.ccf25.set_visibilities(urchin.ccf25.get_areas(["root", "VISp"]), [True, False])
        >>> urchin.ccf25.set_visibilities(urchin.ccf25.get_areas(["root", "VISp"]), True)
        """
        area_visibility = utils.sanitize_list(area_visibility, len(area_list))

        # output dictionary should match JSON schema AreaData:
        #{"acronym": ["a", "b", "c"], "side": [-1, 0, 1], "visible": [true, true, false]}
        data_dict = {}
        data_dict['acronym'] = []
        data_dict['side'] = []
        data_dict['visible'] = []
        for i, area in enumerate(area_list):
            data_dict['acronym'].append(area.acronym)
            data_dict['visible'].append(area_visibility[i])
            data_dict['side'].append(side.value)

        client.sio.emit('SetAreaVisibility', json.dumps(data_dict))

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
        area_colors = utils.sanitize_list(area_colors, len(area_list))
        for i in range(0,len(area_colors)):
            area_colors[i] = utils.sanitize_color(area_colors[i])

        data_dict = {}
        for i, area in enumerate(area_list):
            area_name = utils.sanitize_side(area.acronym, sided)
            data_dict[area_name] = area_colors[i]

        client.sio.emit('SetAreaColors', data_dict)
        
    def set_colormap(self, colormap_name):
        """Set colormap used for mapping area *intensity* values to colors


        Options are
        - cool (default, teal 0 -> magenta 1)
        - grey (black 0 -> white 1)
        - grey-green (grey 0, light 1/255 -> dark 1)
        - grey-purple (grey 0, light 1/255 -> dark 1)
        - grey-red (grey 0, light 1/255 -> dark 1)
        - grey-rainbow (grey 0, rainbow colors from 1/255 -> 1)

        Parameters
        ----------
        colormap_name : string
            colormap name
        """
        client.sio.emit('SetAreaColormap', colormap_name)

    def set_color_intensity(self, area_list, area_intensities, sided="full"):
        """Set intensity values, colors will be set according to the active colormap

        Parameters
        ----------
        area_intensities : list of float
            0->1

        Examples
        --------
        >>> urchin.ccf25.set_color_intensity(urchin.ccf25.get_areas(["root", "VISp"]), [0, 1])
        """
        area_intensities = utils.sanitize_list(area_intensities, len(area_list))
        for i in range(0,len(area_intensities)):
            area_intensities[i] = utils.sanitize_float(area_intensities[i])

        data_dict = {}
        for i, area in enumerate(area_list):
            area_name = utils.sanitize_side(area.acronym, sided)
            data_dict[area_name] = area_intensities[i]

        client.sio.emit('SetAreaIntensity', data_dict)

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
        area_alphas = utils.sanitize_list(area_alphas, len(area_list))
        for i in range(0,len(area_alphas)):
            area_alphas[i] = utils.sanitize_float(area_alphas[i])

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
        area_materials = utils.sanitize_list(area_materials, len(area_list))
        for i in range(0,len(area_materials)):
            area_materials[i] = utils.sanitize_string(area_materials[i])

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

    def set_visibility(self, visibility, sided = utils.Side.FULL):
        """Set area visibility

        Parameters
        ----------
        visibility : bool
        sided : str, optional
            "full" "left" or "right, by default "full"

        Examples
        --------
        >>> urchin.ccf25.root.set_visibility(True)
        >>> urchin.ccf25.root.set_visibility(True, urchin.utils.Side.LEFT)
        """
        data_dict = {
            'acronym':[self.acronym],
            'side':[sided.value],
            'visible':[visibility]
        }

        client.sio.emit('SetAreaVisibility', json.dumps(data_dict))

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

    def set_intensity(self, intensity, sided = "full"):
        """Set color based on the intensity value through the active colormap.

        Parameters
        ----------
        intensity : float
            0->1

        Examples
        --------
        >>> urn.set_intensity(0.5)
        """
        intensity = utils.sanitize_float(intensity)
        area_name = utils.sanitize_side(self.acronym, sided)

        client.sio.emit('SetAreaIntensity', {area_name:intensity})

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