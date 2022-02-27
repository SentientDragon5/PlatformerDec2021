using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace L.VFX
{
    public class EndShadows : MonoBehaviour
    {
        public float sendtime = 3f;
        public string message = "OnEndShadows";
        private VisualEffect vfx;

        void Awake()
        {
            vfx = GetComponent<VisualEffect>();
            StartCoroutine(Send());
        }

        IEnumerator Send()
        {
            yield return new WaitForSeconds(sendtime);
            vfx.SendEvent(message);
        }
    }
}