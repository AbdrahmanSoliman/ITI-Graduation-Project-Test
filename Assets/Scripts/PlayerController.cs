using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static Action onNextButtonClicked;
    public static Action onPrevButtonClicked;

    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Material highlightedMaterial;
    private Camera cam;
    private Ray ray;
    private MeshRenderer prevRenderer;
    private Material defaultMat;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if(prevRenderer != null)
        {
            prevRenderer.material = defaultMat;
        }
        if(Input.GetMouseButton(0))
        {
            ray = cam.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(Input.mousePosition, Vector3.forward, Color.red);

            if(Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
            {
                Debug.Log(hit.collider.gameObject.name);

                prevRenderer = hit.collider.GetComponent<MeshRenderer>();
                defaultMat = prevRenderer.material;

                hit.collider.GetComponent<MeshRenderer>().material = highlightedMaterial;

                if(Input.GetKey(KeyCode.N))
                {
                    onNextButtonClicked?.Invoke();
                }

                if(Input.GetKey(KeyCode.P))
                {
                    onPrevButtonClicked?.Invoke();
                }
            }
        }
    }
}
