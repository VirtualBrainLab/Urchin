#!/usr/bin/env python
import unitymouse.render as umr
import time
import numpy as np
from ibllib.atlas.regions import BrainRegions
from one.api import ONE
import ibllib.atlas as atlas

"""
Display all trajectories in one single brain volume
Author: Mayo, Gaelle, Dan
"""

import numpy as np

one = ONE()
ba = atlas.AllenAtlas(25)
traj_rep = one.alyx.rest('trajectories', 'list', provenance='Planned',
	x=-2243, y=-2000,  project='ibl_neuropixel_brainwide_01', use_cache=False)

umr.setup()

# TODO removing PID manually for sake of figure, but need to iron this out in Alyx
except_pid = ['8b735d77-b77b-4243-8821-37802bf402fe',
'94af9073-0914-4323-a90a-5eea1ef5f92c']

# temp_traj_rep = [traj_rep[0]]
count = 0

for traj in traj_rep:
    print('Trajectory: ' + traj['probe_insertion'])

    if traj['probe_insertion'] not in except_pid:
        temp_traj = one.alyx.rest('trajectories', 'list',
            provenance='Ephys aligned histology track',
            probe_insertion=traj['probe_insertion'], use_cache=False)
        if len(temp_traj) == 0:
            temp_traj = one.alyx.rest('trajectories', 'list', provenance='Histology track',
                probe_insertion=traj['probe_insertion'], use_cache=False)

            if len(temp_traj) == 0:
                continue

        if not temp_traj[0]['x']:
            continue

        ins = atlas.Insertion.from_dict(temp_traj[0])

        entry_coords = ba.xyz2ccf(ins.entry)
        tip_coords = ba.xyz2ccf(ins.tip)
        recording_size = np.sqrt(np.sum(np.power(entry_coords-tip_coords,2)))
        print(recording_size)
        tip_angles = [ins.phi, ins.theta, ins.beta]

        probename = 'p'+str(count)
        count+=1
        umr.create_probes([probename])
        umr.set_probe_positions({probename:tip_coords.tolist()})
        umr.set_probe_angles({probename:tip_angles})
        umr.set_probe_size({probename:[0.07,recording_size/1000,0.02]})

br = BrainRegions()

data = {'VISa': True, 'CA1': True, 'DG': True, 'LP': True, 'PO': True}
umr.set_volume_visibility(data)
data = {'VISa': 'left', 'CA1': 'left', 'DG': 'left', 'LP': 'left', 'PO': 'left'}
umr.set_volume_style(data)

umr.close()