using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adrift.Game
{
    public class AutomaticDoor : MonoBehaviour, IAutoInteractable
    {
        [SerializeField]
        private Transform m_MoveTransform, m_OpenTransform, m_ClosedTransform;

        [SerializeField]
        private AnimationCurve m_OpenAnimation;

        [SerializeField]
        private FMODUnity.StudioEventEmitter m_DoorAudio;

        private float m_CurrentTime = 0f;
        private bool m_Open = false;
        private bool Open
        {
            get => m_Open;
            set
            {
                m_Open = value;
                if (m_MoveDoorRoutine == null)
                    m_MoveDoorRoutine = StartCoroutine(MoveDoor());
            }
        }

        private readonly HashSet<GameObject> CurrentActors = new HashSet<GameObject>();
        public bool Enter(GameObject actor)
        {
            if (CurrentActors.Count == 0)
                Open = true;

            return CurrentActors.Add(actor);
        }
        
        public bool Exit(GameObject actor)
        {
            if (CurrentActors.Remove(actor))
            {
                if (CurrentActors.Count == 0)
                    Open = false;

                return true;
            }
            return false;
        }


        private Coroutine m_MoveDoorRoutine = null;
        private IEnumerator MoveDoor()
        {
            var startTime = 0f;
            var endTime = m_OpenAnimation.keys[m_OpenAnimation.keys.Length - 1].time;
            m_DoorAudio.Play();

            while (
                (Open && m_CurrentTime < endTime) ||
                (!Open && m_CurrentTime > startTime))
            {
                yield return null;
                
                m_CurrentTime += Open ? Time.deltaTime : -Time.deltaTime;
                Mathf.Clamp(m_CurrentTime, startTime, endTime);
                var openAmount = m_OpenAnimation.Evaluate(m_CurrentTime);

                m_MoveTransform.SetPositionAndRotation(
                    Vector3.Lerp(m_ClosedTransform.position, m_OpenTransform.position, openAmount),
                    Quaternion.Slerp(m_ClosedTransform.rotation, m_OpenTransform.rotation, openAmount)
                );
            }
            m_MoveDoorRoutine = null;
        }
    }
}



