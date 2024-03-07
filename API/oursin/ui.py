"""Interactive components within notebooks"""


def slider_widget(function_call, slider_parameters):
    """Creates a slider in the notebook, displays results of the input function
    
    
    Parameters
    ----------
    function_call: function established earlier in code
    slider_parameters: list of start, stop, and increment for slider to follow
    
    Examples
    ---------
    >>> urchin.ui.slider_widget(update_sizes_from_firing)
    """
    try:
        import ipywidgets as widgets
    except ImportError:
        raise ImportError("Widgets package is not installed. Please pip install ipywidgets to use this function.")
    # widgets.interact(function_call, t=(slider_parameters[0],slider_parameters[1],slider_parameters[2]))
