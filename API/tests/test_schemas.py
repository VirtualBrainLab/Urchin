import oursin as urchin
import pytest
import jsonschema
import json
import requests

from unittest.mock import patch

vector3data_url = "https://raw.githubusercontent.com/VirtualBrainLab/vbl-json-schema/pydantic/src/vbl_json_schema/schemas/vector3.json"
response = requests.get(vector3data_url)
vector3data_schema = json.loads(response.text)

def test_mesh():
    with patch.object(urchin.client.sio, 'emit') as mock_emit:
        mesh = urchin.meshes.Mesh()

        mesh.set_scale([1,1,1])

        call = mock_emit.call_args

        msg = call.args[0]
        data = call.args[1]
        print(data)

        try:
            jsonschema.validate(data, vector3data_schema)
        except jsonschema.exceptions.ValidationError as e:
            pytest.fail()