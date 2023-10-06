
urchin
========================================

[![PyPI Version](https://img.shields.io/pypi/v/oursin.svg)](https://pypi.org/project/oursin/) [![Downloads](https://static.pepy.tech/badge/oursin)](https://pepy.tech/project/oursin)

Universal Renderer Creating Helpful Images for Neuroscience (Urchin) is a python package that links your analysis scripts to a standalone brain renderer program, to create graphics like the ones below.

# Gallery
<p float="left">
 <img src="https://github.com/VirtualBrainLab/Urchin/raw/develop/gallery/flatmap_layout.png" width="25%"> 
 <img src="https://github.com/VirtualBrainLab/Urchin/raw/develop/gallery/data_onesided.png" width="25%"> 
 <img src="https://github.com/VirtualBrainLab/Urchin/raw/develop/gallery/RS_fig1.png " width="35%">
</p>

<p float="center">
 <img src="https://github.com/VirtualBrainLab/Urchin/raw/develop/gallery/brain_rotate_cropped.gif" width="45%"> 
</p>

# Quickstart

To install the package run `pip install oursin`. To get started open a Python terminal and run:

```
import oursin as urchin
urchin.setup()
```

## Examples

Head over to [urchin-examples/basics](https://github.com/VirtualBrainLab/urchin-examples/tree/colab/basics) to find tutorials that introduce the functionality in Urchin.

To get you started, try running the code below to load the root brain model. 

### Render all Allen CCF areas
```
urchin.ccf.load_beryl()
```

### Render the root area

```
urchin.ccf.set_visibility({"grey":True})
urchin.ccf.set_material({"grey":"transparent-lit"})
urchin.ccf.set_alpha({"grey":0.25})
```

# Documentation

For detailed instructions please see the [documentation](https://virtualbrainlab.org/urchin/installation_and_use.html).

# Urchin for Unity

The Urchin package can be embedded into a Unity application by adding the package: https://github.com/VirtualBrainLab/Urchin.git?path=/UnityClient/Packages/vbl.urchin

# Citing

[![DOI](https://zenodo.org/badge/460577328.svg)](https://zenodo.org/badge/latestdoi/460577328)

If Urchin is used as part of a research project you should cite this repository. We've created a DOI for this purpose on Zenodo. Your citations will help us get grant support for this project in the future!

These materials are not sponsored by or affiliated with Unity Technologies or its affiliates. “Unity” is a trademark or registered trademark of Unity Technologies or its affiliates in the U.S. and elsewhere.
