using UnityEngine;

public abstract class BaseController<TModel> : MonoBehaviour where TModel : BaseModel
{
    protected TModel Model { get; private set; }

    public void SetModel(TModel model)
    {
        Model = model;
        OnModelSet();
    }

    protected abstract void OnModelSet();
}
