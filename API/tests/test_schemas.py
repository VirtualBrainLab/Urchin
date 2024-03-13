import oursin as urchin
import pytest
import jsonschema
import json
import requests

from unittest.mock import patch

classes = ['AreaGroupData','CameraModel','CustomAtlasModel',
           'CustomMeshData','CustomMeshModel','MeshModel','ParticleGroupModel']
raw_url = 'https://github.com/VirtualBrainLab/vbl-json-schema/raw/main/src/vbl_json_schema/schemas/urchin/'

def load_schema(idx):
    url = f'{raw_url}{classes[idx]}.json'
    response = requests.get(url)
    return json.loads(response.text)

def test_mesh():
    schema = load_schema(5)
    with patch.object(urchin.client.sio, 'emit') as mock_emit:
        mesh = urchin.meshes.Mesh()

        mesh.set_scale([1,1,1])

        call = mock_emit.call_args

        msg = call.args[0]
        data = call.args[1]
        print(data)

        try:
            jsonschema.validate(data, schema)
        except jsonschema.exceptions.ValidationError as e:
            pytest.fail()