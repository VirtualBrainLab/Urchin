"""
urchin.

Python library for connecting to and sending data to a Universal Renderer for Neuroscience renderer.
"""
__author__ = 'Daniel Birman'
__version__ = "0.4.4"

# load the client
from . import client
from .renderer import *

# load the scene controls
from . import camera

# load the object controls
from . import lines
from . import neurons
from . import primitives
from . import probes
from . import text
from . import volumes

# load the colors
from . import colors

# load the atlases
from .atlas import *