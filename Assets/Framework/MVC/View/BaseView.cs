using UnityEngine;

public abstract class BaseView<TModel> : MonoBehaviour where TModel : BaseModel, new()
{
    protected TModel Model { get; private set; }

    protected virtual void Start()
    {
        Debug.Log("View Start");
        if (Model == null)
        {
            Debug.Log("View: Model is null creating and binding new model");
            BindModel();
        }
    }

    public void SetModel(TModel p_model)
    {  
        Model = p_model;
        OnModelSet();
    }

    protected virtual void OnModelSet() { }

    private void BindModel()
    {
        if (ModelLocator.TryGet(out TModel model))
        {
            SetModel(model);
        }
        else
        {

        }
    }

    public virtual void Show() => gameObject.SetActive(true);
    public virtual void Hide() => gameObject.SetActive(false);
}
