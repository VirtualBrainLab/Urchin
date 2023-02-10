using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    #region Serialized
    [SerializeField] BrainCameraController _cameraControl;
    [SerializeField] AreaManager _areaManager;
    [SerializeField] CCFModelControl _modelControl;
    #endregion

    #region Public functions
    public void SetCameraRotation(List<float> obj)
    {
        _cameraControl.SetBrainAxisAngles(new Vector3(obj[1], obj[0], obj[2]));
    }

    public void SetCameraZoom(float obj)
    {
        _cameraControl.SetZoom(obj);
    }

    public void SetCameraPosition(List<float> obj)
    {
        // position in ml/ap/dv relative to ccf 0,0,0
        Client.LogError("Setting camera position not implemented yet. Use set_camera_target and set_camera_rotation instead.");
        //Vector3 ccfPosition25 = new Vector3(obj[0]/25, obj[1]/25, obj[2]/25);
        //cameraControl.SetOffsetPosition(Utils.apdvlr2World(ccfPosition25));
    }

    public void SetCameraYAngle(float obj)
    {
        _cameraControl.SetSpin(obj);
    }

    public void SetCameraTargetArea(string obj)
    {
        (int ID, bool full, bool leftSide, bool rightSide) = _areaManager.GetID(obj);
        CCFTreeNode node = _modelControl.tree.findNode(ID);
        if (node != null)
        {
            Vector3 center;
            if (full)
                center = node.GetMeshCenterFull();
            else
                center = node.GetMeshCenterSided(leftSide);
            _cameraControl.SetCameraTarget(center);
        }
        else
            UM_Launch.Log("Failed to find node to set camera target: " + obj);
    }

    public void SetCameraTarget(List<float> mlapdv)
    {
        // data comes in in um units in ml/ap/dv
        // note that (0,0,0) in world is the center of the brain
        // so offset by (-6.6 ap, -4 dv, -5.7 lr) to get to the corner
        // in world space, x = ML, y = DV, z = AP

        Vector3 worldCoords = new Vector3(5.7f - mlapdv[0] / 1000f, 4f - mlapdv[2] / 1000f, mlapdv[1] / 1000f - 6.6f);
        _cameraControl.SetCameraTarget(worldCoords);
    }

    public void SetCameraPan(List<float> panXY)
    {
        _cameraControl.SetCameraPan(new Vector2(panXY[0], panXY[1]));
    }
    #endregion
}
