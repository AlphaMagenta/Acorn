using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Acorn {

    public class HudFPS : MonoBehaviour  {

        private float updateInterval = 0.5f;
        private float accum = 0;
        private int frames = 0;
        private float timeleft;
        private float fps = 0;

        private Text label;

        void Start() {
            label = GetComponent<Text>();
            timeleft = updateInterval;
        }

        void Update() {
            timeleft -= Time.deltaTime;
            accum += Time.timeScale/Time.deltaTime;
            frames += 1;

            // Interval ended - update GUI text and start new interval
            if (timeleft <= 0) {
                fps = accum/frames;
                timeleft = updateInterval;
                accum = 0f;
                frames = 0;
                label.text = string.Format("{0:0.}", fps);
            }
        }

    }

}
