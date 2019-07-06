using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Klak.Motion;
using Leap;
using Leap.Unity;
using static Leap.Finger;

public class SoldierController : MonoBehaviour
{
    [SerializeField] public GameObject leapProviderObj;
    LeapServiceProvider m_Provider;

    private Animator animator;
    private float WALK_SPEED;
    private List<GameObject> fallApartingBoneObjs;
    private List<Transform> fallApartingBoneTransforms;
    private List<GameObject> nenObjs;
    private List<Material> nenMaterials;
    private HandUtil handUtil;
    private String[,] bindingSoldierParts = {
        //首(頭)，腕，手，膝，足
        {"Neck","LowerArm_L", "Hand_L", "Lower_Leg_L",  "Toes_L"},
        {"Head", "LowerArm_R", "Hand_R", "Lower_Leg_R", "Toes_R"}
    };
    private GameObject[,] bindingSoldierPartObjs ;
    private GameObject[,] operatingLines;

    void Start()
    {
        this.handUtil = new HandUtil();
        this.m_Provider = this.leapProviderObj.GetComponent<LeapServiceProvider>();
        this.bindingSoldierPartObjs = new GameObject[2, 5];

        this.animator = GetComponent<Animator>();
        this.animator.enabled = false;
        this.WALK_SPEED = 0.085f;
        this.InitSoldier(this.gameObject);

        this.EnableKinematic(true); //Not to fall down objects
        //this.FallApartObject();

        //this.animator.Play("idle");
    }

    void Update()
    {
        Frame frame = this.m_Provider.CurrentFrame;
        Hand[] hands = HandUtil.GetCorrectHands(frame); //0=LEFT, 1=RIGHT
        if (hands[HandUtil.LEFT] !=null & hands[HandUtil.RIGHT] !=null) {
            this.DrawLines(hands);
            this.ComeBackObjectToLife();
        }
        else {
            this.EraseLines();
            this.FallApartObject();
        }

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

        if (Input.GetKeyDown(KeyCode.K)) { }
        if (Input.GetKeyDown(KeyCode.L)) { }
    }

    private void InitSoldier(GameObject parent)
    {

        //
        // Preparing for Fall Aparting objects
        //
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
                GameObject tmpObj = new GameObject(); //作成されてしまってる
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

        //
        // Preparing for Keijiro's Smooth Follow script
        // Smooth Follow revives the soldier who died by fall aparting cause.
        // 啓次郎氏のSmooth Followスクリプトの準備
        // バラバラにしたソルジャーをSmooth Followの動きで生き返らせる 
        //
        for (int i = 0; i < this.fallApartingBoneObjs.Count; i++) {
            SmoothFollow sf = this.fallApartingBoneObjs[i].AddComponent<SmoothFollow>();
            sf.enabled = false;
            sf.interpolationType = SmoothFollow.Interpolator.Exponential;
            sf.positionSpeed = 5;
            sf.rotationSpeed = 5;
            sf.jumpAngle = 60;
            sf.target = this.fallApartingBoneTransforms[i];
        }

        //
        // Find bone parts which are operated from Root of the soldier.
        // ソルジャーのRoot直下から，操作するボーンパーツを探す
        //
        GameObject boneRootObj = GameObject.Find("Squelette_Lourd/Root");
        for (int hand_i = 0; hand_i < this.bindingSoldierParts.GetLength(0); hand_i++) {
            for(int finger_i = 0; finger_i < this.bindingSoldierParts.GetLength(1); finger_i++) {
                this.bindingSoldierPartObjs[hand_i, finger_i] = Util.FindRecursively(boneRootObj, this.bindingSoldierParts[hand_i, finger_i]);
            }
        }

        //
        // 線を複製する
        //
        GameObject lineRoot = GameObject.Find("Lines");
        GameObject origin = GameObject.Find("LineOrigin");
        this.operatingLines = new GameObject[2, 5];
        for (int hand_i = 0; hand_i < 2; hand_i++) {
            for (int finger_i = 0; finger_i < 5; finger_i++) {
                GameObject child = GameObject.Instantiate(origin) as GameObject;
                child.name = "Line" + hand_i + "_" + finger_i;
                child.transform.parent = lineRoot.transform;
                this.operatingLines[hand_i, finger_i] = child;
            }
        }
    }

    //
    // 線を描画する
    //
    private void DrawLines(Hand[] hands) {
        //bindingSoldierPartObjs
        //operatingLines
        //hands
        for (int hand_i = 0; hand_i < 2; hand_i++) {
            for (int finger_i = 0; finger_i < 5; finger_i++) {
                LineRenderer renderer = operatingLines[hand_i, finger_i].GetComponent<LineRenderer>();
                Vector3[] positions = new Vector3[2];
                Finger finger = hands[hand_i].Fingers[finger_i];
                positions[0] = HandUtil.GetVector3(finger.TipPosition); //指先の位置
                positions[1] = bindingSoldierPartObjs[hand_i, finger_i].transform.position;
                renderer.enabled = true;
                renderer.SetPositions(positions);
            }
        }
    }

    //
    // 線を消す
    //
    private void EraseLines() {
        for (int hand_i = 0; hand_i < 2; hand_i++) {
            for (int finger_i = 0; finger_i < 5; finger_i++) {
                LineRenderer renderer = operatingLines[hand_i, finger_i].GetComponent<LineRenderer>();
                renderer.enabled = false;
            }
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
