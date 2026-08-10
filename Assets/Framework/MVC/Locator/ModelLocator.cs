using System;
using System.Collections.Generic;
using UnityEngine;

public static class ModelLocator
{
    private static Dictionary<Type, BaseModel> _models = new();

    public static void Register<TModel> (TModel model) where TModel : BaseModel
        => _models [typeof (TModel)] = model;

    public static bool TryGet<TModel>(out TModel model) where TModel : BaseModel, new()
    {
        if(_models.TryGetValue(typeof(TModel), out var stored))
        {
            model = (TModel) stored;
            return true;
        }

        var newModel = new TModel();
        newModel.Init();
        _models.Add(typeof(TModel), newModel);

        model = newModel;
        return false;
    }

    public static void Unregister<TModel>() where TModel : BaseModel
        => _models.Remove(typeof (TModel));

    public static void Clear()
        => _models.Clear();
}
