//using System;
using BrainAtlas;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    [SerializeField] ParticleSystem _particleSystem;
    Dictionary<string, int> _particleMapping;

    #region Unity
    private void Awake()
    {
        _particleMapping = new();
    }
    #endregion

    public void CreateParticles(List<string> particleNames) //instantiates cube as default
    {
        foreach (string particleName in particleNames)
        {
            if (_particleMapping.ContainsKey(particleName))
                Debug.Log($"MParticleesh id {particleName} already exists.");

#if UNITY_EDITOR
            Debug.Log($"Creating particle {particleName}");
#endif
            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
            emitParams.position = Vector3.zero;
            emitParams.startColor = Color.red;
            emitParams.startSize = 0.02f;
            _particleSystem.Emit(emitParams, 1);

            _particleMapping.Add(particleName, _particleSystem.particleCount - 1);
        }
    }

    public void Clear()
    {
        _particleSystem.Clear();
        _particleMapping.Clear();
    }

    public void DeleteParticles(List<string> particleNames)
    {
        // Deleting particles is a little complicated, since the particle system is data-oriented
        // and only holds references to particle by index
        // This means that each time we delete a particle we need to update the indexes of all the other particles
        // which makes this a very expensive operation to run. Users should be warned not to send individual delete
        // messages!

        // TODO
        //throw new NotImplementedException();
        //ParticleSystem.Particle[] particles = new ParticleSystem.Particle[_particleSystem.particleCount];
        //int nParticles = _particleSystem.GetParticles(particles);

        //for (int i = 0; i < nParticles; i++)
        //{
        //    // For each particle, 
        //}
    }

    public void SetPosition(Dictionary<string, float[]> particlePositions)
    {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[_particleSystem.particleCount];
        int nParticles = _particleSystem.GetParticles(particles);

        foreach (var kvp in particlePositions)
        {
            Vector3 coordU = new Vector3(kvp.Value[0], kvp.Value[1], kvp.Value[2]);
            particles[_particleMapping[kvp.Key]].position = BrainAtlasManager.ActiveReferenceAtlas.Atlas2World(coordU);
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
