using UnityEngine;

namespace Adrift.Game
{
    public class AudioZone : MonoBehaviour, IAutoInteractable
    {
        [SerializeField]
        private FMODUnity.StudioEventEmitter m_Audio = null;

        [SerializeField]
        private string m_Parameter = "";

        public bool Enter(GameObject actor)
        {
            if (m_Audio != null && m_Audio.EventInstance.hasHandle())
                m_Audio.SetParameter(m_Parameter, 1f);
            return true;
        }

        public bool Exit(GameObject actor)
        {
            if (m_Audio != null && m_Audio.EventInstance.hasHandle())
                m_Audio.SetParameter(m_Parameter, 0f);
            return true;
        }
    }
}