using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitDetectorUtilities
{
    public static void DetectHit(Vector3 hitPoint, float range)
    {
        var hits = Physics.OverlapSphere(hitPoint, range);
        foreach (var hit in hits)
        {
            var spawner = hit.GetComponentInParent<ParticleSpawner>();
            if (spawner != null)
            {
                spawner.Wave(hitPoint, range);
            }
        }
    }
}
