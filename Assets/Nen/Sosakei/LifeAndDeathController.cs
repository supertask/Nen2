using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeAndDeathController : MonoBehaviour
{
    private Animator animator;
    private float WALK_SPEED;
    private List<GameObject> fallApartingBoneObjs;
    private List<GameObject> nenObjs;
    private List<Material> nenMaterials;

    void Start()
    {
        this.animator = GetComponent<Animator>();
        this.animator.enabled = false;
        this.WALK_SPEED = 0.085f;
        this.SetupTarget(this.gameObject);
        this.EnableKinematic(true); //Not to fall down objects
        //this.FallApartObject();

        //this.animator.Play("idle");
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

        if (Input.GetKeyDown(KeyCode.K)) {
            this.FallApartObject();
        }
    }

    private void SetupTarget(GameObject parent)
    {
        this.fallApartingBoneObjs = new List<GameObject>();
        this.nenMaterials = new List<Material>();
        this.nenObjs = new List<GameObject>();
        foreach (Transform t in parent.GetComponentsInChildren<Transform>())
        {
            if (t.gameObject.name == parent.name) { continue; } //Ignore a parent

            // Preparing to fall apart bone objects on the floor
            if (t.gameObject.GetComponent<Rigidbody>() != null) {
                this.fallApartingBoneObjs.Add(t.gameObject);
            }

            // Preparing to diable the Nen material (aura effect)
            SkinnedMeshRenderer renderer = t.gameObject.GetComponent<SkinnedMeshRenderer>();
            if (renderer != null && renderer.materials.Length >= 2) {
                Material effectMat = renderer.materials[1];
                this.nenMaterials.Add(effectMat); //Saving Nen material to use later
                this.nenObjs.Add(t.gameObject); //Saving Nen object to use later
            }
        }
    }
    

    /* private void AddForceAndRotation()
    {
        float min = 0.0f;
        float max = 0.01f;

        //なるべく同時刻に力を加える
        for(int i = 0; i < this.target_rigidbodies.Count; i++) {
            Vector3 direction = new Vector3(Random.Range(min, max), Random.Range(min, 3*max), Random.Range(min, max));
            this.target_rigidbodies[i].AddForce(direction, ForceMode.Impulse);
            this.target_rigidbodies[i].AddTorque(direction, ForceMode.Impulse);
        }
    } */

    private void EnableKinematic(bool isKinematic)
    {
        for(int i = 0; i < this.fallApartingBoneObjs.Count; i++) {
            this.fallApartingBoneObjs[i].GetComponent<Rigidbody>().isKinematic = isKinematic;
        }
    }


    private void EnableNenMaterials(bool enableNen)
    {
        if (enableNen) {
            for (int i = 0; i < this.nenObjs.Count; i++) {
                SkinnedMeshRenderer renderer = this.nenObjs[i].GetComponent<SkinnedMeshRenderer>();
                Material[] mats = new Material[2];
                mats[0] = renderer.materials[0];
                mats[1] = this.nenMaterials[i];
                renderer.materials = mats;
            }
        }
        else {
            for (int i = 0; i < this.nenObjs.Count; i++) {
                SkinnedMeshRenderer renderer = this.nenObjs[i].GetComponent<SkinnedMeshRenderer>();
                Material[] mats = new Material[1];
                mats[0] = renderer.materials[0];
                renderer.materials = mats;
            }
        }
    }

    public void FallApartObject()
    {
        this.EnableNenMaterials(false); //Disable Nen materials
        this.EnableKinematic(false); //Objects fall down on the floor
    }

}
