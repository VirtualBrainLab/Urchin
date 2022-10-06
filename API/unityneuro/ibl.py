import unityneuro.render as urn
from one.api import ONE
one = ONE(base_url='https://alyx.internationalbrainlab.org')
import ibllib.atlas as atlas
NeedlesAtlas = atlas.NeedlesAtlas(25)


def getCoords(ins, coord_transform):
    entry_coords = coord_transform.xyz2ccf(ins.entry, mode='wrap')
    tip_coords = coord_transform.xyz2ccf(ins.tip, mode='wrap')
    angles = [ins.phi, ins.theta, ins.beta]
    depth = vec_dist(entry_coords, tip_coords)
    return (entry_coords, tip_coords, angles, depth)

def traj2coords(traj, coord_transform):
    insertion = atlas.Insertion.from_dict(traj)
    return getCoords(insertion, coord_transform)

def plot_session_histology(mouse):
    bwm_mm = one.alyx.rest('trajectories', 'list', provenance='Ephys aligned histology track', subject=mouse,
                          project='ibl_neuropixel_brainwide_01', use_cache=False)

    urn.setup()

    urn.clear()

    # get all the insertions for this mouse
    urn.set_area_visibility({'8':True})
    urn.set_area_material({'8':'transparent-unlit'})
    urn.set_area_alpha({'8':0.15})

    for i, ins in enumerate(bwm_mm):
        # get the coords
        (entry, tip, angles) = traj2coords(ins, NeedlesAtlas)
        
        pname = 'mm'+str(i)
        urn.create_probes([pname])
        urn.set_probe_positions({pname:list(tip)})
        urn.set_probe_angles({pname:list(angles)})
        urn.set_probe_colors({pname:'#000000'})
        
    urn.create_text(['mouse_name'])
    urn.set_text({'mouse_name':mouse})
    urn.set_text_colors({'mouse_name':'#FFA500'})

    urn.set_camera_rotation(45,0,0)