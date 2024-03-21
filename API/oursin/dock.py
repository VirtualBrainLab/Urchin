
from . import client

def save():
    """Save the current scene
    """
    client.sio.emit('urchin-save')

def load(string url):
    client.sio.emit('urchin-load', url)