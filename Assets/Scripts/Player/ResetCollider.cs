using System.Collections;
using System.Collections.Generic;
using HackYeah;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetCollider : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponentInParent<PlayerController>();
        if (player != null)
        {
            SceneManager.LoadScene("Gameplay");
        }
    }
}
