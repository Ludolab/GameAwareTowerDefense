using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToyControls : MonoBehaviour
{

    public string ToyName = string.Empty;

    [TextArea]
    public string Description = string.Empty;

    public virtual void ToyControlGUI() {
        GUILayout.Label("No additional controls available for this toy.");
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
