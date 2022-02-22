#!/usr/bin/env python
import unitymouse.render as umr
import time

umr.setup()

time.sleep(0.1)
data = {'MDRN': 0.66,
 'ECU': 0.61,
 'PYR': 0.65,
 'LP': 0.55,
 'CA1': 0.59}
umr.set_volume_intensity(data)
time.sleep(0.1)

neurons = ['n1','n2','n3']
umr.create_neurons(neurons)
time.sleep(0.1)

data = {'n1': [5700,4000,0],
 'n2': [5700,4500,0],
 'n3': [5700,4000,500]}
umr.set_neuron_positions(data)
time.sleep(0.1)

data = {'n1': '#FFFFFF',
 'n2': '#FF0000',
 'n3': '#00FF00'}
umr.set_neuron_color(data)
time.sleep(0.1)

umr.close()