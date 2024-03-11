"""Interactive components within notebooks"""

# def slider_widget(function_call, slider_parameters):
#     """Creates a slider in the notebook, displays results of the input function
    
    
#     Parameters
#     ----------
#     function_call: function established earlier in code
#     slider_parameters: list of start, stop, and increment for slider to follow
    
#     Examples
#     ---------
#     >>> urchin.ui.slider_widget(update_sizes_from_firing)
#     """
#     try:
#         import ipywidgets as widgets
#     except ImportError:
#         raise ImportError("Widgets package is not installed. Please pip install ipywidgets to use this function.")
#     widgets.interact(function_call, t=(slider_parameters[0],slider_parameters[1],slider_parameters[2]))

def spikes_bin_data(spike_times_raw_data, spike_clusters_data, bin_size=0.02):
    """Bins spike clusters into an array
    
    Parameters
    ----------
    spike_times_raw_data: np array
        raw data of spiking times, in samples
    spike_clusters_data: np array
        list of spike clusters data
    bin_size: float
        bin size in seconds, default value is 20ms
    
    Returns
    -------
    array
        spiking data binned in given bin size
    
    Examples
    --------
    >>> urchin.ui.spikes_bin_data(spike_times_samp, spike_clusters)
    """
    try:
        import numpy as np
    except ImportError:
        raise ImportError("Numpy package is not installed. Please pip install numpy to use this function.")
    # bin the spike times and clusters
    spike_times_raw_data = np.squeeze(spike_times_raw_data)
    spike_clusters_data = np.squeeze(spike_clusters_data)
    spike_times_sec = spike_times_raw_data / 3e4 # convert from 30khz samples to seconds
    # set up bin edges - 20 ms here
    bins_seconds = np.arange(np.min(spike_times_sec), np.max(spike_times_sec), bin_size)
    # make list of lists for spike times specific to each cluster
    spikes = [spike_times_sec[spike_clusters_data == cluster] for cluster in np.unique(spike_clusters_data)]
    # bin
    binned_spikes = []
    for cluster in spikes:
        counts, _ = np.histogram(cluster, bins_seconds)  
        binned_spikes.append(counts)
    binned_spikes = np.array(binned_spikes) # should be [#neurons, #bins]
    return binned_spikes

def spikes_binned_event_average(binned_spikes, event_start, event_ids, bin_size_sec=0.02, window_start_sec = 0.1, window_end_sec = 0.5):
    """Prepares intermediate data table and averages binned spikes over a given time window
    
    Parameters
    ----------
    binned_spikes: array
        binned spiking data
    event_start: array
        start times of events in seconds
    event_ids: array
        ids of events
    bin_size_sec: float
        bin size in seconds, default value is 20ms
    window_start_sec: float
        start of window in seconds, default value is 0.1
    window_end_sec: float
        end of window in seconds, default value is 0.5
    
    Returns
    -------
    array
        binned spikes averaged over given time window
    
    Examples
    --------
    >>> urchin.ui.spikes_binned_event_average(binned_spikes, event_start, event_ids)
    """
    try:
        import numpy as np
    except ImportError:
        raise ImportError("Numpy package is not installed. Please pip install numpy to use this function.")
    
    bintime_prev = int(window_start_sec * 50)
    bintime_post = int(window_end_sec * 50 + 1)
    windowsize = bintime_prev + bintime_post
    bin_size = bin_size_sec * 1000

    # To bin: divide by 20, floor
    stim_binned = np.floor(event_start * 1000 / bin_size).astype(int)
    stim_binned = np.transpose(stim_binned)


    u_stim_ids = np.unique(event_ids)

    # Initialize final_avg matrix
    final_avg = np.empty((binned_spikes.shape[0], len(u_stim_ids), windowsize))

    for neuron_id in range(binned_spikes.shape[0]):

        for stim_id in u_stim_ids:
            stim_indices = np.where(event_ids[0] == stim_id)[0]

            neuron_stim_data = np.empty((len(stim_indices), windowsize))
            
            for i, stim_idx in enumerate(stim_indices):
                bin_id = int(stim_binned[0][stim_idx])
                selected_columns = binned_spikes[neuron_id, bin_id - bintime_prev: bin_id + bintime_post]
                neuron_stim_data[i,:] = selected_columns

            bin_average = np.mean(neuron_stim_data, axis=0)/bin_size_sec
            final_avg[neuron_id, int(stim_id) - 1, :] = bin_average
    return final_avg

def slope_viz_stimuli_per_neuron(prepped_data, t=-100, neuron_id = 0):
    """Visualizes and creates interactive plot for the average of each stimulus per neuron
    
    Parameters
    ----------
    prepped_data: 3D array
        prepped data of averages of binned spikes and events in the format of [neuron_id, stimulus_id, time]
    t: int
        time in milliseconds of where to initially place the vertical line
    neuron_id: int
        id of neuron
    
    Examples
    --------
    >>> urchin.ui.slope_viz_stimuli_per_neuron(t=-100, neuron_id = 0)
    """
    try:
        import numpy as np
    except ImportError:
        raise ImportError("Numpy package is not installed. Please pip install numpy to use this function.")
    try:
        import matplotlib.pyplot as plt
    except ImportError:
        raise ImportError("Matplotlib package is not installed. Please pip install matplotlib to use this function.")
    
    # Plotting data:
    for i in range(0,prepped_data.shape[1]):
        y = prepped_data[neuron_id][i]
        x = np.arange(-100, 520, step=20)
        plt.plot(x,y)

    # Labels:
    plt.xlabel('Time from stimulus onset')
    plt.ylabel('Number of Spikes Per Second')
    plt.title(f'Neuron {neuron_id} Spiking Activity with Respect to Each Stimulus')

    #Accessories:
    plt.axvspan(0, 300, color='gray', alpha=0.3)
    plt.axvline(t, color='red', linestyle='--',)
    # Set y-axis limits
     # Calculate y-axis limits
    max_y = max([max(prepped_data[neuron_id][i]) for i in range(10)])  # Maximum y-value across all lines
    if max_y < 10:
        max_y = 10  # Set ymax to 10 if the default max is lower than 10
    plt.ylim(0, max_y)
   
    # plt.legend()
    plt.show()

def slope_viz_neurons_per_stimuli(prepped_data, t=-100, stim_id = 0):
    """Visualizes and creates interactive plot for the average of every neuron per stimulus
    
    Parameters
    ----------
    prepped_data: 3D array
        prepped data of averages of binned spikes and events in the format of [neuron_id, stimulus_id, time]
    t: int
        time in milliseconds of where to initially place the vertical line
    stim_id: int
        id of neuron
    
    Examples
    --------
    >>> urchin.ui.slope_viz_stimuli_per_neuron(t=-100, stim_id = 0)
    """
    try:
        import numpy as np
    except ImportError:
        raise ImportError("Numpy package is not installed. Please pip install numpy to use this function.")
    try:
        import matplotlib.pyplot as plt
    except ImportError:
        raise ImportError("Matplotlib package is not installed. Please pip install matplotlib to use this function.")
     # Plotting data:
    for i in range(0,prepped_data.shape[0]):
        y = prepped_data[i][stim_id]
        x = np.arange(-100, 520, step=20)
        plt.plot(x,y)
    
    # Labels:
    plt.xlabel(f'Time from Stimulus {stim_id} display (20 ms bins)')
    plt.ylabel('Number of Spikes Per Second')
    plt.title(f'Neuron Spiking Activity with Respect to Stimulus ID {stim_id}')

    # Accessories:
    plt.axvspan(0, 300, color='gray', alpha=0.3)
    plt.axvline(t, color='red', linestyle='--',)

    plt.show()

def plot_appropriate_interactie_graph(view = "stim", prepped_data, window_start_sec = 0.1, window_end_sec = 0.5):
    """Plots appropriate interactive graph based on view
    
    Parameters
    ----------
    prepped_data: 3D array
        prepped data of averages of binned spikes and events in the format of [neuron_id, stimulus_id, time]
    view: str
        view type, either "stim" or "neuron"
    window_start_sec: float
        start of window in seconds, default value is 0.1
    window_end_sec: float
        end of window in seconds, default value is 0.5
    
    Examples
    --------
    >>> urchin.ui.plot_appropriate_interactie_graph(view = "stim")
    """
    try:
        import ipywidgets as widgets
    except ImportError:
        raise ImportError("Widgets package is not installed. Please pip install ipywidgets to use this function.")
        
    time_slider = widgets.IntSlider(value=-1e3 * window_start_sec, min=--1e3 * window_start_sec, max=5e3 * window_start_sec, step=5, description='Time')
    time_slider.layout.width = '6.53in'
    time_slider.layout.margin = '0 -4px'
    
    if view == "stim":
        stimuli_dropdown = widgets.Dropdown(
            options= range(0,prepped_data.shape[1]),
            value=0,
            description='Stimulus ID:',
        )
        stimuli_dropdown.layout.margin = "20px 20px"
        output = widgets.interactive_output(slope_viz_neurons_per_stimuli, {'t': time_slider, 'stim_id': stimuli_dropdown})
        # Display the widgets and the output
        display(widgets.VBox([stimuli_dropdown,time_slider]))
        display(output)
    
    elif view == "neuron":
        neuron_dropdown = widgets.Dropdown(
            options= range(0,prepped_data.shape[0]),
            value=355,
            description='Neuron ID:',
        )
        neuron_dropdown.layout.margin = "20px 20px"

        # Link the function with the interact function
        output = widgets.interactive_output(slope_viz_stimuli_per_neuron, {'t': time_slider, 'neuron_id': neuron_dropdown})

        # Display the widgets and the output
        display(widgets.VBox([neuron_dropdown,time_slider]))
        display(output)