using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Klak.Motion;

public class LifeAndDeathController : MonoBehaviour
{
    private Animator animator;
    private float WALK_SPEED;
    private List<GameObject> fallApartingBoneObjs;
    private List<Transform> fallApartingBoneTransforms;
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
        if (Input.GetKeyDown(KeyCode.L)) {
            ComeBackObjectToLife();
        }
    }

    private void SetupTarget(GameObject parent)
    {
        this.fallApartingBoneObjs = new List<GameObject>();
        this.fallApartingBoneTransforms = new List<Transform> ();
        this.nenMaterials = new List<Material>();
        this.nenObjs = new List<GameObject>();
        foreach (Transform t in parent.GetComponentsInChildren<Transform>())
        {
            if (t.gameObject.name == parent.name) { continue; } //Ignore a parent

            // Preparing to fall apart bone objects on the floor
            if (t.gameObject.GetComponent<Rigidbody>() != null) {
                this.fallApartingBoneObjs.Add(t.gameObject);
                GameObject tmpObj = new GameObject();
                tmpObj.transform.position = t.gameObject.transform.position;
                tmpObj.transform.rotation = t.gameObject.transform.rotation;
                this.fallApartingBoneTransforms.Add(tmpObj.transform);
            }

            // Preparing to diable the Nen material (aura effect)
            SkinnedMeshRenderer renderer = t.gameObject.GetComponent<SkinnedMeshRenderer>();
            if (renderer != null && renderer.materials.Length >= 2) {
                Material effectMat = renderer.materials[1];
                this.nenMaterials.Add(effectMat); //Saving Nen material to use later
                this.nenObjs.Add(t.gameObject); //Saving Nen object to use later
            }
        }

        //Preparing for smooth follow
        for (int i = 0; i < this.fallApartingBoneObjs.Count; i++) {
            SmoothFollow sf = this.fallApartingBoneObjs[i].AddComponent<SmoothFollow>();
            sf.enabled = false;
            sf.interpolationType = SmoothFollow.Interpolator.Exponential;
            //sf.interpolationType = SmoothFollow.Interpolator.DampedSpring;
            sf.positionSpeed = 5;
            sf.rotationSpeed = 5;
            sf.jumpAngle = 60;
            sf.target = this.fallApartingBoneTransforms[i];
        }
    }

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

    // Move back to base position if it is enabled
    private void EnableSmoothFollow(bool enable)
    {
        for (int i = 0; i < this.fallApartingBoneTransforms.Count; i++) {
            this.fallApartingBoneObjs[i].GetComponent<SmoothFollow>().enabled = enable;
        }
    }

    public void FallApartObject()
    {
        this.EnableNenMaterials(false); //Disable Nen materials
        this.EnableKinematic(false); //Objects fall down on the floor
        this.EnableSmoothFollow(false);
    }

    public void ComeBackObjectToLife()
    {
        this.EnableNenMaterials(true); //Enable Nen materials
        this.EnableKinematic(true); //Disable rigidbody behaviour(gravity and force)
        this.EnableSmoothFollow(true);
    }
}
