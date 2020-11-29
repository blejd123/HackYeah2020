using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHitDetector : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private float _range;

    public void Update()
    {
        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }

        RaycastHit hit;
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            HitDetectorUtilities.DetectHit(hit.point, _range, null);
        }
    }
}
