
urchin
========================================

[![PyPI Version](https://img.shields.io/pypi/v/unityneuro.svg)](https://pypi.org/project/unityneuro/)

Unity Rendering Creating Helpful Images for Neuroscience (Urchin) is a python package that links your analysis scripts to a standalone brain renderer program, to create graphics like the ones below.

# Gallery
<p float="left">
 <img src="https://github.com/dbirman/UnityNeuroscience/raw/main/Examples/gallery/flatmap_layout.png" width="25%"> 
 <img src="https://github.com/dbirman/UnityNeuroscience/raw/main/Examples/gallery/data_onesided.png" width="25%"> 
 <img src="https://github.com/dbirman/UnityNeuroscience/raw/main/Examples/gallery/RS_fig1.png " width="35%">
</p>

<p float="center">
 <img src="https://github.com/dbirman/UnityNeuroscience/raw/main/Examples/gallery/brain_rotate_cropped.gif" width="45%"> 
</p>

# Quickstart

To install the package run `pip install unityneuro`. To get started open a Python terminal and run:

```
import unityneuro.render as urn
urn.setup()
```

## Examples

Head over to [Examples/basics](https://github.com/VirtualBrainLab/Urchin/tree/main/Examples/basics) to find many tutorials that introduce the functionality in Urchin.

To get you started, try running the code below to load the root brain model. 

```
urn.set_area_visibility({"grey":True})
urn.set_area_material({"grey":"transparent-lit"})
urn.set_area_alpha({"grey":0.25})
```

# Documentation

For detailed instructions please see the [documentation](https://virtualbrainlab.org/03_unity_neuro/01_urn_intro.html).

# Citing

[![DOI](https://zenodo.org/badge/460577328.svg)](https://zenodo.org/badge/latestdoi/460577328)

If Urchin is used as part of a research project you should cite this repository. We've created a DOI for this purpose on Zenodo. Your citations will help us get grant support for this project in the future!

These materials are not sponsored by or affiliated with Unity Technologies or its affiliates. “Unity” is a trademark or registered trademark of Unity Technologies or its affiliates in the U.S. and elsewhere.
