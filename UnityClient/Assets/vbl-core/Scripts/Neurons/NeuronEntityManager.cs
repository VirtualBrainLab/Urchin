using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms; // used to access Translation
using Unity.Rendering; // used to access RenderMesh/Bounds
using Unity.Collections;
using Unity.Mathematics;

/**
 * NeuronEntityManager is the MonoBehaviour class that handles all Neuron Entity interaction
 *
 * It interfaces with the NeuronEntitySystem which handles spiking and rendering
 * It also interfaces with the various Load() functions
 *
 * The main input call to this function is AddNeurons() which handles creating and adding new entities to
 * the ECS system
 */
public class NeuronEntityManager : MonoBehaviour
{
    // Manager access
    EntityManager eManager;
    [SerializeField] private ElectrodeManager elecmanager;
    [SerializeField] private CCFModelControl ccfmodelcontrol;

    // Expose mesh and materials
    [SerializeField] private GameObject neuronRoot;
    [SerializeField] private Mesh neuronMesh;
    [SerializeField] private Material neuronMaterial;
    [SerializeField] private float replayScale = 0.125f;
    [SerializeField] private float neuronScale = 0.075f;
    [SerializeField] private Utils util;
    private float _currentNeuronMaxSpikeRate;

    // Debug
    [SerializeField] private bool debug;

    // Local tracking
    private float _currentNeuronScale;
    private bool _useScaleForSpiking;

    List<Entity> neurons;
    private float _currentMaxFiringRate;

    // Rendering
    RenderMesh neuronRenderMesh;
    private float4 neuronDefaultColor = new float4(0.15f, 0.63f, 0f, 0.4f);

    // Tracking
    // set mVN to limit the number of neurons on-screen. Neurons will be only removed if they are not visible
    // i.e. their probeDistanceComponent alpha is set to 0
    //[SerializeField] private int maxVisibleNeurons = 1000;

    void Awake()
    {
        neurons = new List<Entity>();

        eManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        neuronRenderMesh = new RenderMesh
        {
            castShadows = UnityEngine.Rendering.ShadowCastingMode.Off,
            mesh = neuronMesh,
            material = neuronMaterial,
            layer = 11
        };
    }

    private void Start()
    {
        _currentMaxFiringRate = 100f;
    }

    private void Update()
    {
        if (debug)
        {
            foreach (Entity neuron in neurons)
            {
                if (UnityEngine.Random.value < (0.1f * Time.deltaTime))
                {
                    eManager.SetComponentData(neuron, new SpikingComponent { spiking = 1f });
                    eManager.SetComponentData(neuron, new MaterialColor { Value = new float4(1,1,1,1) });
                    eManager.SetComponentData(neuron, new Scale { Value = neuronScale * 2 });
                }
            }
        }
    }

    public bool UseScaleForSpiking()
    {
        return _useScaleForSpiking;
    }

    public float GetMaxFiringRate()
    {
        return _currentMaxFiringRate;
    }

    public float GetNeuronScale()
    {
        return _currentNeuronScale;
    }

    /**
     * Debug variant: add neurons with a CCF coordinate (just adds a sphere) with a particular color
     */
    public List<Entity> AddNeurons(List<float3> mlapdv)
    {
        EntityArchetype neuronArchetype = eManager.CreateArchetype(
            typeof(Translation),
            typeof(Scale),
            typeof(LocalToWorld),
            typeof(RenderMesh),
            typeof(RenderBounds),
            typeof(MaterialColor),
            typeof(SpikingComponent),
            typeof(SpikingColorComponent),
            typeof(SpikingRandomComponent)
            );

        NativeArray<Entity> newNeurons = eManager.CreateEntity(neuronArchetype, mlapdv.Count, Allocator.Temp);
        List<Entity> returnList = new List<Entity>();

        _currentNeuronScale = neuronScale;

        for (int i = 0; i < mlapdv.Count; i++)
        {
            Entity neuron = newNeurons[i];
            eManager.SetComponentData(neuron, new Translation { Value = CCF2Transform(mlapdv[i]) });
            eManager.SetComponentData(neuron, new Scale { Value = neuronScale });
            eManager.SetComponentData(neuron, new MaterialColor { Value = neuronDefaultColor });
            eManager.SetSharedComponentData(neuron, neuronRenderMesh);

            // Add the spiking component
            eManager.SetComponentData(neuron, new SpikingComponent { spiking = 0f });
            eManager.SetComponentData(neuron, new SpikingColorComponent { color = neuronDefaultColor });
            eManager.SetComponentData(neuron, NewSpikingRandomComponent());

            neurons.Add(neuron);
            returnList.Add(neuron);
        }

        newNeurons.Dispose();
        return returnList;
    }

    /**
     * Add neurons while keeping track of their mlapdv coordinates, option to set neuron color based on their mlapdv coordinate
     */
    public List<Entity> AddNeurons(List<float3> mlapdvCoords, List<int3> apdvlr)
    {
        EntityArchetype neuronArchetype = eManager.CreateArchetype(
            typeof(Translation),
            typeof(Scale),
            typeof(LocalToWorld),
            typeof(RenderMesh),
            typeof(RenderBounds),
            typeof(MaterialColor),
            typeof(SpikingComponent),
            typeof(SpikingColorComponent),
            typeof(SpikingRandomComponent),
            typeof(SimulatedNeuronComponent)
            );

        NativeArray<Entity> newNeurons = eManager.CreateEntity(neuronArchetype, mlapdvCoords.Count, Allocator.Temp);
        List<Entity> returnList = new List<Entity>();

        _currentNeuronScale = neuronScale;

        for (int i = 0; i < mlapdvCoords.Count; i++)
        {
            // Calculate what area we are in and get the color
            float annotation = elecmanager.GetAnnotation(apdvlr[i].x, apdvlr[i].y, apdvlr[i].z);
            Color neuronColor = ccfmodelcontrol.GetCCFAreaColorMinDepth((int)annotation);
            //Debug.Log("Neuron at " + apdvlr[i] + "in: " + annotation + " with color " + neuronColor);
            float4 color = new float4(neuronColor.r, neuronColor.g, neuronColor.b, 0.4f);

            Entity neuron = newNeurons[i];
            eManager.SetComponentData(neuron, new Translation { Value = CCF2Transform(mlapdvCoords[i]) });
            eManager.SetComponentData(neuron, new Scale { Value = neuronScale });
            eManager.SetComponentData(neuron, new MaterialColor { Value = color });
            eManager.SetSharedComponentData(neuron, neuronRenderMesh);

            // Add the spiking component
            eManager.SetComponentData(neuron, new SpikingComponent { spiking = 0f });
            eManager.SetComponentData(neuron, new SpikingColorComponent { color = color });
            eManager.SetComponentData(neuron, NewSpikingRandomComponent());

            // Track the coordinates
            eManager.SetComponentData(neuron, new SimulatedNeuronComponent { apdvlr = mlapdvCoords[i] });

            neurons.Add(neuron);
            returnList.Add(neuron);
        }

        newNeurons.Dispose();
        return returnList;
    }

    /**
     * Add neurons with receptive field data
     */
    public List<Entity> AddNeurons(List<float3> mlapdvCoords, List<int3> apdvlr, List<float3> rfData)
    {
        EntityArchetype neuronArchetype = eManager.CreateArchetype(
            typeof(Translation),
            typeof(Scale),
            typeof(LocalToWorld),
            typeof(RenderMesh),
            typeof(RenderBounds),
            typeof(MaterialColor),
            typeof(SpikingComponent),
            typeof(SpikingColorComponent),
            typeof(SpikingRandomComponent),
            typeof(SimulatedNeuronComponent),
            typeof(SimRFComponent)
            );

        NativeArray<Entity> newNeurons = eManager.CreateEntity(neuronArchetype, mlapdvCoords.Count, Allocator.Temp);
        List<Entity> returnList = new List<Entity>();

        _currentNeuronScale = neuronScale;

        for (int i = 0; i < mlapdvCoords.Count; i++)
        {
            // Calculate what area we are in and get the color
            float annotation = elecmanager.GetAnnotation(apdvlr[i].x, apdvlr[i].y, apdvlr[i].z);
            Color neuronColor = ccfmodelcontrol.GetCCFAreaColorMinDepth((int)annotation);
            //Debug.Log("Neuron at " + apdvlr[i] + "in: " + annotation + " with color " + neuronColor);
            float4 color = new float4(neuronColor.r, neuronColor.g, neuronColor.b, 0.4f);

            Entity neuron = newNeurons[i];
            eManager.SetComponentData(neuron, new Translation { Value = CCF2Transform(mlapdvCoords[i]) });
            eManager.SetComponentData(neuron, new Scale { Value = neuronScale });
            eManager.SetComponentData(neuron, new MaterialColor { Value = color });
            eManager.SetSharedComponentData(neuron, neuronRenderMesh);

            // Add the RF data
            eManager.SetComponentData(neuron, new SimRFComponent { rf_x = rfData[i].x, rf_y = rfData[i].y, rf_sigma = rfData[i].z });

            // Add the spiking component
            eManager.SetComponentData(neuron, new SpikingComponent { spiking = 0f });
            eManager.SetComponentData(neuron, new SpikingColorComponent { color = color });
            eManager.SetComponentData(neuron, NewSpikingRandomComponent());

            // Track the coordinates
            eManager.SetComponentData(neuron, new SimulatedNeuronComponent { apdvlr = mlapdvCoords[i] });

            neurons.Add(neuron);
            returnList.Add(neuron);
        }

        newNeurons.Dispose();
        return returnList;
    }

    public List<Entity> AddNeurons(List<float3> mlapdv, List<IBLGLMComponent> data)
    {
        EntityArchetype neuronArchetype = eManager.CreateArchetype(
            typeof(IBLGLMComponent),
            typeof(Translation),
            typeof(Scale),
            typeof(LocalToWorld),
            typeof(RenderMesh),
            typeof(RenderBounds),
            typeof(MaterialColor),
            typeof(SpikingComponent),
            typeof(SpikingColorComponent),
            typeof(SpikingRandomComponent)
            );

        NativeArray<Entity> newNeurons = eManager.CreateEntity(neuronArchetype, mlapdv.Count, Allocator.Temp);
        List<Entity> returnList = new List<Entity>();

        for (int i = 0; i < mlapdv.Count; i++)
        {
            Entity neuron = newNeurons[i];
            eManager.SetComponentData(neuron, new Translation { Value = CCF2Transform(mlapdv[i]) });
            eManager.SetComponentData(neuron, new Scale { Value = neuronScale });
            eManager.SetSharedComponentData(neuron, neuronRenderMesh);

            // Add the spiking component
            eManager.SetComponentData(neuron, new SpikingComponent { spiking = 0f });
            eManager.SetComponentData(neuron, new SpikingColorComponent { color = neuronDefaultColor });
            eManager.SetComponentData(neuron, NewSpikingRandomComponent());

            // Add the NeuronDataComponent
            eManager.SetComponentData(neuron, data[i]);

            neurons.Add(neuron);
            returnList.Add(neuron);
        }

        newNeurons.Dispose();
        return returnList;
    }

    public List<Entity> AddNeurons(List<float3> mlapdv, List<Color> data)
    {
        EntityArchetype neuronArchetype = eManager.CreateArchetype(
            typeof(Translation),
            typeof(Scale),
            typeof(LocalToWorld),
            typeof(RenderMesh),
            typeof(RenderBounds),
            typeof(MaterialColor)
            );

        NativeArray<Entity> newNeurons = eManager.CreateEntity(neuronArchetype, mlapdv.Count, Allocator.Temp);
        List<Entity> returnList = new List<Entity>();

        for (int i = 0; i < mlapdv.Count; i++)
        {
            Entity neuron = newNeurons[i];
            // Add the required position and render components
            eManager.SetComponentData(neuron, new Translation { Value = CCF2Transform(mlapdv[i]) });
            eManager.SetComponentData(neuron, new Scale { Value = replayScale });
            eManager.SetComponentData(neuron, new MaterialColor { Value = util.Color2float4(data[i]) });
            eManager.SetSharedComponentData(neuron, neuronRenderMesh);

            // Add the simulated neuron component

            neurons.Add(neuron);
            returnList.Add(neuron);
        }

        newNeurons.Dispose();
        return returnList;
    }

    public List<Entity> AddNeurons(List<float3> mlapdv, List<Color> data, List<int> labData)
    {
        EntityArchetype neuronArchetype = eManager.CreateArchetype(
            typeof(Translation),
            typeof(Scale),
            typeof(LocalToWorld),
            typeof(RenderMesh),
            typeof(RenderBounds),
            typeof(MaterialColor),
            typeof(PAComponent)
            );

        NativeArray<Entity> newNeurons = eManager.CreateEntity(neuronArchetype, mlapdv.Count, Allocator.Temp);
        List<Entity> returnList = new List<Entity>();

        for (int i = 0; i < mlapdv.Count; i++)
        {
            Entity neuron = newNeurons[i];
            // Add the required position and render components
            eManager.SetComponentData(neuron, new Translation { Value = CCF2Transform(mlapdv[i]) });
            eManager.SetComponentData(neuron, new Scale { Value = replayScale });
            eManager.SetComponentData(neuron, new MaterialColor { Value = util.Color2float4(data[i]) });
            eManager.SetSharedComponentData(neuron, neuronRenderMesh);

            // Add the ProbeAnimation component (keeps track of the lab)
            eManager.SetComponentData(neuron, new PAComponent { lab = labData[i] });

            neurons.Add(neuron);
            returnList.Add(neuron);
        }

        newNeurons.Dispose();
        return returnList;
    }

    public List<Entity> AddNeurons(List<float3> mlapdv, List<IBLEventAverageComponent> eventAverage)
        {
            EntityArchetype neuronArchetype = eManager.CreateArchetype(
                typeof(Translation),
                typeof(Scale),
                typeof(LocalToWorld),
                typeof(RenderMesh),
                typeof(RenderBounds),
                typeof(MaterialColor),
                typeof(SpikingComponent),
                typeof(SpikingColorComponent),
                typeof(SpikingRandomComponent),
                typeof(IBLEventAverageComponent)
                );

            int n = mlapdv.Count;

            NativeArray<Entity> newNeurons = eManager.CreateEntity(neuronArchetype, n, Allocator.Temp);
            List<Entity> returnList = new List<Entity>();

            for (int i = 0; i < n; i++)
            {
                Entity neuron = newNeurons[i];
                eManager.SetComponentData(neuron, new Translation { Value = CCF2Transform(mlapdv[i]) });
                eManager.SetComponentData(neuron, new Scale { Value = neuronScale });
                eManager.SetSharedComponentData(neuron, neuronRenderMesh);

                eManager.SetComponentData(neuron, eventAverage[i]);
                eManager.SetComponentData(neuron, NewSpikingRandomComponent());

                neurons.Add(neuron);
                returnList.Add(neuron);
            }

            newNeurons.Dispose();
            return returnList;
        }

    private SpikingRandomComponent NewSpikingRandomComponent()
    {
        return new SpikingRandomComponent { rand = Unity.Mathematics.Random.CreateFromIndex((uint)neurons.Count) };
    }

    void RemoveNeuron(Entity neuron)
    {
        eManager.DestroyEntity(neuron);
        neurons.Remove(neuron);
    }
    void RemoveNeurons(List<Entity> removeNeurons)
    {
        foreach (Entity neuron in removeNeurons)
        {
            RemoveNeuron(neuron);
        }
    }
    void RemoveNeurons(EntityQuery neuronQuery)
    {
        eManager.DestroyEntity(neuronQuery);
        foreach(Entity entity in neuronQuery.ToEntityArray(Allocator.Temp))
        {
            neurons.Remove(entity);
        }
    }

    public void RemoveAllNeurons()
    {
        eManager.DestroyAndResetAllEntities();
        neurons = new List<Entity>();
    }

    public void SetComponentData(Entity entity, SpikingComponent spikeComp)
    {
        eManager.SetComponentData(entity, spikeComp);
    }

    // ** HELPER FUNCTIONS ** //

    // Note that
    // X axis = -ML
    // Y axis = -DV
    // Z axis = AP
    public float3 CCF2Transform(float3 mlapdv)
    {
        return new float3(-mlapdv.x, -mlapdv.z, mlapdv.y) + new float3(neuronRoot.transform.position);
    }
    public float3 CCF2Transform(Vector3 mlapdv)
    {
        return new float3(-mlapdv.x, -mlapdv.z, mlapdv.y) + new float3(neuronRoot.transform.position);
    }
}
