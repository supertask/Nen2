using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util
{

    public static GameObject FindRecursively(GameObject target, string name)
    {
        foreach (Transform child in target.GetComponentsInChildren<Transform>()) {
            if (child.gameObject.name == name) { return child.gameObject; }
        }
        return null;
    }

    /// <summary>
    /// 指定された GameObject を複製して返します
    /// </summary>
    public static GameObject Clone(GameObject go)
    {
        var clone = GameObject.Instantiate(go) as GameObject;
        clone.transform.parent = go.transform.parent;
        clone.transform.localPosition = go.transform.localPosition;
        clone.transform.localScale = go.transform.localScale;
        return clone;
    }


    public class Timer
    {
        public float currentTime;
        public float waitingSec;
        public bool isStart;
        public Timer(float waitingSec) {
            this.currentTime = 0.0f;
            this.waitingSec = waitingSec;
            this.isStart = false;
        }
        public bool OnTime() {
            if (this.isStart) {
                if (this.currentTime > this.waitingSec) {
                    this.Reset();
                    return true; //設定した時刻が来れば，true
                }
                else { return false; }
            }
            else { return false; }
        }
        public void Start() {
            this.currentTime = 0.0f;
            this.isStart = true;
        }
        public void Reset() {
            this.currentTime = 0.0f;
            this.isStart = false;
        }
        public void Clock() {
            this.currentTime += Time.deltaTime;
        }
    }

    //指定の時間が来たら，OnTimeで知らせる
    //ボタンの連打を防ぐ
    public class ButtonMashingStopper 
    {
        public float currentTime;
        public float waitingSec;
        public ButtonMashingStopper(float waitingSec) {
            this.currentTime = 0.0f;
            this.waitingSec = waitingSec;
        }
        public bool isOkNextButton() {
            if (currentTime > this.waitingSec) {
                this.Reset();
                this.Clock();
                return true;
            }
            else {
                this.Clock();
                return false;
            }
        }
        public void Clock() { this.currentTime += Time.deltaTime; }
        public void Reset() { this.currentTime = 0.0f; }
    }

    public class Cron {
        public float timeleft;
        public float waitingSec;

        public Cron(float waitingSec)
        {
            this.timeleft = 0.0f;
            this.waitingSec = waitingSec;
        }

        public bool OnTime()
        {
            if (timeleft <= 0.0f) {
                timeleft = this.waitingSec;
                return true;
            }
            else {
                return false;
            }
        }
        public void Clock() {
            timeleft -= Time.deltaTime;
        }
    }
}
