using UnityEngine;

public abstract class BaseController<TModel> : MonoBehaviour where TModel : BaseModel, new()
{
    protected TModel Model { get; private set; }

    protected virtual void Start()
    {
        Debug.Log("Controller Start");
        if (Model == null)
        {
            Debug.Log("Controller: Model is null creating and binding new model");
            BindModel();
        }
    }

    public void SetModel(TModel model)
    {
        Model = model;
        OnModelSet();
    }

    private void BindModel()
    {
        if (ModelLocator.TryGet(out TModel model))
        {
            SetModel(model);
        }
    }
    protected abstract void OnModelSet();
}
