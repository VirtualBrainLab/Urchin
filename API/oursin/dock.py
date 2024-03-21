
from . import client
import requests
import hashlib

from vbl_aquarium.models.dock import *

# Define the API endpoint URL
api_url = "http://localhost:5000"

def create_bucket(bucket_name, password, api_url = api_url):
    headers = {
        "Content-Type": "application/json"
    }

    create_url = f'{api_url}/create/{bucket_name}'

    data = BucketModel(
        token = "c503675a-506c-48c0-9b5e-5265e8260a06",
        password = hash256(password)
    )

    response = requests.post(api_url, data=data.model_dump_json(), headers=headers)

    # Check the response
    if response.status_code == 201:
        print(response.text)
    else:
        print("Error:", response.status_code, response.text)



def save():
    """Save the current scene
    """
    client.sio.emit('urchin-save')

def load(url):
    client.sio.emit('urchin-load', url)



def hash256(password):
    return hashlib.sha256(password.encode()).hexdigest()
