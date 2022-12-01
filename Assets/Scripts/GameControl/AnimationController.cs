using System.Collections;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    public static void InitializeState()
    {
        
    }

    public static IEnumerator AnimateState()
    {
        while (GameController.CurrentGameController.isPaused)
        {
            yield return null;
        }
        
        yield return null;
    }
}
