using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//https://docs.unity3d.com/ScriptReference/Animator.SetTrigger.html

public class ToolBarAnimation : MonoBehaviour
{

    Animator toolbarAnimator;
    public bool s;

    // Start is called before the first frame update
    void Start()
    {
        toolbarAnimator = gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
