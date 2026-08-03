using UnityEngine;

public abstract class BaseView<TModel> : MonoBehaviour where TModel : BaseModel
{
    protected TModel Model { get; private set; }

    public void SetModel(TModel p_model)
    {  
        Model = p_model;
        OnModelSet();
    }

    protected virtual void OnModelSet() { }

    public virtual void Show() => gameObject.SetActive(true);
    public virtual void Hide() => gameObject.SetActive(false);
}
