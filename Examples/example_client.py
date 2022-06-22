import sys
import unityneuro.render as urn
import time
import numpy as np

# This is an old test script, the example notebooks are much more complete! Check those out first.

urn.setup(standalone=True)

urn.clear()

time.sleep(0.1)

data = {'MDRN': True,
 'ECU': True,
 'PYR-l': True,
 'LP-r': True,
 'CA1': True}
urn.set_area_visibility(data)
urn.set_area_color({'ECU':'#FF0000'})
time.sleep(0.1)

data = {'MDRN': 0.66,
 'ECU': 0.61,
 'PYR': 0.65,
 'LP': 0.55,
 'CA1': 0.59}
urn.set_area_intensity(data)
time.sleep(0.1)

urn.set_area_color({'ECU':'#FF0000'})
time.sleep(0.1)

urn.set_area_material({'CA1':'toon'})
time.sleep(0.1)

neurons = ['n1','n2','n3']
urn.create_neurons(neurons)
time.sleep(0.1)

data = {'n1': [5700,4000,0],
 'n2': [5700,4500,0],
 'n3': [5700,4000,500]}
urn.set_neuron_positions(data)
time.sleep(0.1)

data = {'n1': 1.0,
 'n2': 0.5,
 'n3': 0.1}
urn.set_neuron_sizes(data)
time.sleep(0.1)

data = {'n1': '#FFFFFF',
 'n2': '#FF0000',
 'n3': '#00FF00'}
urn.set_neuron_colors(data)
time.sleep(0.1)

data = ['p1']
urn.create_probes(data)
time.sleep(0.1)

for az in np.arange(0,180):
    urn.set_probe_angles({'p1':[az*1.0,45.0,0.0]})
    time.sleep(0.1)

for el in np.arange(0,90):
    urn.set_probe_angles({'p1':[0,el*1.0,0.0]})
    time.sleep(0.1)

urn.set_probe_positions({'p1':[5700,6600,4000]})
time.sleep(0.1)
urn.set_probe_angles({'p1':[45,45,0]})
time.sleep(0.1)

urn.close()