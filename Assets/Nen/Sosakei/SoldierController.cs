using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierController : MonoBehaviour
{
    private Animator animator;
    private float WALK_SPEED;

    void Start()
    {
        this.animator = GetComponent<Animator>();
        this.animator.Play("idle");
        this.WALK_SPEED = 0.085f;
    }

    void Update()
    {
        //Attack
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            animator.SetTrigger("attackTrigger1");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            animator.SetTrigger("attackTrigger2");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3)) {
            animator.SetTrigger("attackTrigger3");
        }

        //Walk
        if (Input.GetKeyDown(KeyCode.Alpha9)) {
            animator.SetFloat("Speed", WALK_SPEED);
        }
        else if (Input.GetKeyUp(KeyCode.Alpha9)) {
            animator.SetFloat("Speed", 0);
        }

        //Change Direction
        if (Input.GetKey(KeyCode.Alpha7)) {
            this.transform.Rotate(new Vector3(0, 0.5f, 0), Space.Self);
        }

        if (animator.GetFloat("Speed") > 0) {
            Vector3 dv = this.transform.forward * animator.GetFloat("Speed");
            this.transform.position += dv * Time.deltaTime;
        }
    }
}
