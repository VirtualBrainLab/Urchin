using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrainRegionSelector : MonoBehaviour
{
    private Renderer rend;
    private Shader jellyShader;
    private Shader opaqueShader;
    private Shader transparentShader;

    private int mode = 0;

    private bool trajectoryPlanner;

    private void Awake() {
        rend = GetComponent<Renderer>();
        jellyShader = Shader.Find("Shader Graphs/BrainRegionJelly");
        opaqueShader = Shader.Find("Shader Graphs/BrainRegionOpaque");
        transparentShader = Shader.Find("Shader Graphs/BrainRegionTransparent");

        TrajectoryPlannerManager tpmanager;
        trajectoryPlanner = GameObject.Find("main").TryGetComponent<TrajectoryPlannerManager>(out tpmanager);
    }

    private void OnMouseDown() {
        if (!trajectoryPlanner)
            CycleModes();
    }

    private void JellyMode() {
        rend.material.shader = jellyShader;
        mode = 1;
    }

    private void OpaqueMode() {
        rend.material.shader = opaqueShader;
        mode = 2;
    }

    private void TransparentMode() {
        rend.material.shader = transparentShader;
        mode = 0;
    }

    public void CycleModes()
    {
        Shader curShader = rend.material.shader;
        if (curShader == jellyShader)
        {
            OpaqueMode();
        }
        else if (curShader == opaqueShader)
        {
            TransparentMode();
        }
        else
        {
            JellyMode();
        }
    }

    public int GetMode() {
        return mode;
    }
}
