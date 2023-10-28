//using System;
using BrainAtlas;
using System.Collections.Generic;
using UnityEngine;
using Urchin.API;

public class ParticleManager : MonoBehaviour
{
    [SerializeField] ParticleSystem _particleSystem;
    Dictionary<string, int> _particleMapping;

    #region Unity
    private void Awake()
    {
        _particleMapping = new();
    }

    private void Start()
    {
        Client_SocketIO.CreateParticles += CreateParticles;
        Client_SocketIO.SetParticlePosition += SetPosition;
        Client_SocketIO.SetParticleSize += SetSize;
        //Client_SocketIO.SetParticleShape += SetShape;
        Client_SocketIO.SetParticleColor += SetColor;
        //Client_SocketIO.SetParticleMaterial += SetMaterial;
    }
    #endregion

    public void CreateParticles(List<string> particleNames) //instantiates cube as default
    {
        foreach (string particleName in particleNames)
        {
            if (_particleMapping.ContainsKey(particleName))
                Debug.Log($"Particle id {particleName} already exists.");

#if UNITY_EDITOR
            Debug.Log($"Creating particle {particleName}");
#endif
            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
            emitParams.position = Vector3.zero;
            emitParams.startColor = Color.red;
            emitParams.startSize = 0.1f;
            _particleSystem.Emit(emitParams, 1);

            _particleMapping.Add(particleName, _particleSystem.particleCount - 1);
        }
    }

    public void Clear()
    {
        _particleSystem.Clear();
        _particleMapping.Clear();
    }

    public void SetPosition(Dictionary<string, float[]> particlePositions)
    {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[_particleSystem.particleCount];
        int nParticles = _particleSystem.GetParticles(particles);

        foreach (var kvp in particlePositions)
        {
            Vector3 coordU = new Vector3(kvp.Value[0], kvp.Value[1], kvp.Value[2]);
            particles[_particleMapping[kvp.Key]].position = BrainAtlasManager.ActiveReferenceAtlas.Atlas2World(coordU, false);
        }

        _particleSystem.SetParticles(particles);
    }

    public void SetSize(Dictionary<string, float> particleSizes)
    {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[_particleSystem.particleCount];
        int nParticles = _particleSystem.GetParticles(particles);

        foreach (var kvp in particleSizes)
        {
            // TODO: Replace with vertex stream size property in Shader Graph
            particles[_particleMapping[kvp.Key]].startSize = kvp.Value;
        }

        _particleSystem.SetParticles(particles);
    }

    public void SetColor(Dictionary<string, string> particleColors)
    {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[_particleSystem.particleCount];
        int nParticles = _particleSystem.GetParticles(particles);

        foreach (var kvp in particleColors)
        {
            Color newColor;
            if (ColorUtility.TryParseHtmlString(kvp.Value, out newColor))
            {
                particles[_particleMapping[kvp.Key]].startColor = newColor;
            }
        }

        _particleSystem.SetParticles(particles);
    }
}
