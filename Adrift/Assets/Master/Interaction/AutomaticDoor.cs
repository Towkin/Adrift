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

        [SerializeField]
        private AbstractMachine m_HookedMachine = null;

        private float m_CurrentTime = 0f;
        private bool m_Open = false;
        private bool Open
        {
            get => m_Open;
            set
            {
                if (m_Open == value)
                    return;

                m_Open = value;
                if (m_MoveDoorRoutine == null)
                    m_MoveDoorRoutine = StartCoroutine(MoveDoor());
            }
        }

        private void OnEnable()
        {
            if (m_HookedMachine != null)
                m_HookedMachine.OnStateChanged += HookedMachine_OnStateChanged;
        }

        private void OnDisable()
        {
            if (m_HookedMachine != null)
                m_HookedMachine.OnStateChanged -= HookedMachine_OnStateChanged;
        }

        private void UpdateOpen()
            => Open = !(m_HookedMachine != null && !m_HookedMachine.isWorking) && m_ActorCount > 0;

        private void HookedMachine_OnStateChanged(AbstractMachine obj)
        {
            UpdateOpen();
        }

        int m_ActorCount = 0;
        public bool Enter(GameObject actor)
        {
            m_ActorCount++;
            UpdateOpen();
            return true;
        }
        
        public bool Exit(GameObject actor)
        {
            m_ActorCount--;
            UpdateOpen();
            return true;
        }


        private Coroutine m_MoveDoorRoutine = null;
        private IEnumerator MoveDoor()
        {
            var startTime = 0f;
            var endTime = m_OpenAnimation.keys[m_OpenAnimation.keys.Length - 1].time;
            if (m_DoorAudio.EventInstance.hasHandle())
            {
                m_DoorAudio.Play();
            }

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



