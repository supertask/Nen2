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
    public Transform player;
    public GameObject soldier;

    private LeapServiceProvider m_Provider;
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
    private bool isRevivedSoldier;
    private Util.ButtonMashingStopper handMashingStopper;
    private Util.Timer reviveTimer;

    void Start()
    {
        this.handUtil = new HandUtil(player);
        this.m_Provider = this.leapProviderObj.GetComponent<LeapServiceProvider>();
        this.bindingSoldierPartObjs = new GameObject[2, 5];

        this.animator = GetComponent<Animator>();
        //this.animator.enabled = false;
        this.WALK_SPEED = 0.085f;
        this.isRevivedSoldier = true;
        this.handMashingStopper = new Util.ButtonMashingStopper(0.12f);
        this.reviveTimer = new Util.Timer(2.5f);
        this.InitSoldier();

        //this.EnableKinematic(true); //Not to fall down objects
        this.FallApartSoldier();

        //this.animator.Play("idle");
    }

    void Update()
    {
        Frame frame = this.m_Provider.CurrentFrame;
        Hand[] hands = HandUtil.GetCorrectHands(frame); //0=LEFT, 1=RIGHT
        if (hands[HandUtil.LEFT] != null && hands[HandUtil.RIGHT] != null) {
            /*
            this.DrawLines(hands);
            this.ReviveSoldier();

            //Hand operations
            //連打を防ぐタイマー
            if (this.handMashingStopper.isOkNextButton())
            {
                if (this.handUtil.IsMoveRight(hands[HandUtil.LEFT])) {
                    this.animator.enabled = true;
                    Debug.Log("left hand: moves to RIGHT");
                    animator.SetTrigger("attackTrigger1");
                }
                else if (this.handUtil.IsMoveDown(hands[HandUtil.LEFT])) {
                    this.animator.enabled = true;
                    Debug.Log("left hand: moves to DOWN");
                    animator.SetTrigger("attackTrigger3");
                }
                else if (this.handUtil.IsMoveLeft(hands[HandUtil.RIGHT])) {
                    this.animator.enabled = true;
                    Debug.Log("rigth hand: moves to LEFT");
                    animator.SetTrigger("attackTrigger2");
                }
            }
            */
        }
        else {
            /*
            this.EraseLines();
            this.FallApartSoldier();
            */
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

        if (Input.GetKeyDown(KeyCode.R))
        {
            //this.DrawLines(hands);
            this.ReviveSoldier();
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            //this.EraseLines();
            this.FallApartSoldier();
        }

        /*
        //Walk
        if (Input.GetKeyDown(KeyCode.Alpha9)) {
            animator.SetFloat("Speed", WALK_SPEED);
        }
        else if (Input.GetKeyUp(KeyCode.Alpha9)) {
            animator.SetFloat("Speed", 0);
        }

        //Change Direction
        if (Input.GetKey(KeyCode.Alpha7)) {
            this.soldier.transform.Rotate(new Vector3(0, 0.5f, 0), Space.Self);
        }

        if (animator.GetFloat("Speed") > 0) {
            Vector3 dv = this.soldier.transform.forward * animator.GetFloat("Speed");
            this.soldier.transform.position += dv * Time.deltaTime;
        }
        */

        if (this.reviveTimer.OnTime()) {
            Debug.Log("Finished smooth Follow");

            //HERE: SmoothFollowが完了するまでanimatorスタートさせない
            //this.SaveBoneTransforms(); //Save Bone Transforms for reiviving

            //FallApartする前の状態に戻す
            /*
            for(int i = 0; i < this.fallApartingBoneObjs.Count;  i++) {
                 this.fallApartingBoneObjs[i].transform.position = this.fallApartingBoneTransforms[i].position;
                 this.fallApartingBoneObjs[i].transform.rotation = this.fallApartingBoneTransforms[i].rotation;
            }
            */
            this.animator.enabled = true;
            this.isRevivedSoldier = true;
        }
        this.reviveTimer.Clock();
    }

    private void InitSoldier()
    {
        //
        // Preparing for Fall Aparting objects
        //
        GameObject root = GameObject.Find("Squelette_Lourd/Root"); //HERE: ルートに置き換えてやる
        GameObject tmpObj = GameObject.Find("tmp");
        this.fallApartingBoneObjs = new List<GameObject>();
        this.fallApartingBoneTransforms = new List<Transform> ();
        this.nenMaterials = new List<Material>();
        this.nenObjs = new List<GameObject>();

        foreach (Transform t in root.GetComponentsInChildren<Transform>())
        {
            // Preparing to fall apart bone objects on the floor
            if (t.gameObject.GetComponent<Rigidbody>() != null) {
                this.fallApartingBoneObjs.Add(t.gameObject);
                GameObject anObj = new GameObject("tmp_" + t.gameObject.name);
                anObj.transform.position = t.gameObject.transform.position;
                anObj.transform.rotation = t.gameObject.transform.rotation;
                anObj.transform.parent = tmpObj.transform;
                this.fallApartingBoneTransforms.Add(anObj.transform);
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
    private void DrawLines(Hand[] hands)
    {
        for (int hand_i = 0; hand_i < 2; hand_i++) {
            for (int finger_i = 0; finger_i < 5; finger_i++) {
                LineRenderer renderer = operatingLines[hand_i, finger_i].GetComponent<LineRenderer>();
                Vector3[] positions = new Vector3[2];
                Finger finger = hands[hand_i].Fingers[finger_i];
                positions[0] = HandUtil.ToVector3(finger.TipPosition); //指先の位置
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
        for (int i = 0; i < this.fallApartingBoneObjs.Count; i++) {
            this.fallApartingBoneObjs[i].GetComponent<SmoothFollow>().enabled = enable;
        }
    }
    

    /*
    public void SaveBoneTransforms()
    {
    }

    public void SaveBoneTransforms()
    {
    }
    */

    public void FallApartSoldier()
    {
        if (! this.isRevivedSoldier) { return; } //if the soldier is already fall aparted

        this.animator.enabled = false; //Stop animation
        /*
        for(int i = 0; i < this.fallApartingBoneObjs.Count;  i++) {
             this.fallApartingBoneTransforms[i].position = this.fallApartingBoneObjs[i].transform.position;
             this.fallApartingBoneTransforms[i].rotation = this.fallApartingBoneObjs[i].transform.rotation;
        }
        */
        this.EnableNenMaterials(false); //Disable Nen materials
        this.EnableKinematic(false); //Objects fall down on the floor
        this.EnableSmoothFollow(false);

        this.isRevivedSoldier = false;
    }

    public void ReviveSoldier()
    {
        if (this.isRevivedSoldier) { return; } //if the soldier is already revived
        this.EnableNenMaterials(true); //Enable Nen materials
        this.EnableKinematic(true); //Disable rigidbody behaviour(gravity and force)
        this.EnableSmoothFollow(true);
        this.reviveTimer.Start();
    }
}
