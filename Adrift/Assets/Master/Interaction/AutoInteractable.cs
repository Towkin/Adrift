using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adrift.Game
{
    public interface IAutoInteractable
    {
        bool Enter(GameObject actor);
        bool Exit(GameObject actor);
    }
}