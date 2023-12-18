using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raycast : MonoBehaviour
{
    public event Action<GameObject> OnClick;
    public LayerMask spiderMask;
    void Update()
    {
        // On clicking, cast a ray from the camera to the mouse position
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // If the ray hits something, print the name of the object
            if (Physics.Raycast(ray, out hit, 100, spiderMask))
            {
                Debug.Log(hit.transform.name);
                OnClick?.Invoke(hit.transform.gameObject);
            }
        }
    }
}
