using UnityEngine;

namespace Adrift.Game
{
    public interface IInteractable
    {
        bool Interact(GameObject interactor);

        ComponentBase PickupComponent { get; }

        Vector3 InteractionPosition { get; }
    }
}