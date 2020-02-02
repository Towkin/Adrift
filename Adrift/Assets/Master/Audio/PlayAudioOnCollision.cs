using UnityEngine;

public class PlayAudioOnCollision : MonoBehaviour
{
    [SerializeField]
    private float m_MinRelativeVelocity = 1.5f;

    [SerializeField]
    private FMODUnity.StudioEventEmitter m_Audio = null;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.sqrMagnitude > m_MinRelativeVelocity * m_MinRelativeVelocity)
        {
            if (m_Audio.EventInstance.hasHandle())
            {
                m_Audio.Play();
            }
        }
    }
}
