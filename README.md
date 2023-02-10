
urchin
========================================

[![PyPI Version](https://img.shields.io/pypi/v/oursin.svg)](https://pypi.org/project/oursin/)

Urchin (Unity Renderer for Neuroscience) is a python package that links your analysis scripts to a standalone brain renderer program, to create graphics like the ones below.

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

To install the package run `pip install oursin`. To get started open a Python terminal and run:

```
import oursin as urchin
urchin.setup()
```

A few quick examples to get you started.

### Render all Allen CCF areas
```
urchin.ccf.load_beryl()
```

### Load the basic 3D model of the brain and make it transparent
```
urchin.ccf.set_visibility({"grey":True})
urchin.ccf.set_material({"grey":"transparent-lit"})
urchin.ccf.set_alpha({"grey":0.25})
```

# Documentation

For detailed instructions please see the [documentation](https://virtualbrainlab.org/03_unity_neuro/01_urn_intro.html).

# Citing

If you use this to make figures for a publication you should cite this repo, email me (dbirman@uw.edu) and I can generate a DOI for the version you are using.
