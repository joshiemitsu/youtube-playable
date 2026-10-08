using R3;
using UnityEngine;

public class ArrowTapController : MonoBehaviour
{
    [SerializeField] private ArrowInputHandler _input;

    private void Start()
    {
        _input.OnArrowTapped.SubscribeAwait(async (arrow, ct) => await arrow.OnPressed(), 
            AwaitOperation.Drop).AddTo(this);
    }
}
