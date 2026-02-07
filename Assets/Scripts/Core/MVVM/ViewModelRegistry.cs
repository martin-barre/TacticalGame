using System;
using System.Collections.Generic;

public class ViewModelRegistry<TModel, TViewModel> where TViewModel : IViewModel<TModel>
{
    private readonly Dictionary<TModel, TViewModel> _registry;
    private readonly Func<TModel, TViewModel> _factory;
    
    public ViewModelRegistry(Func<TModel, TViewModel> factory)
    {
        _registry = new Dictionary<TModel, TViewModel>();
        _factory = factory;
    }
    
    public TViewModel GetOrCreate(TModel model)
    {
        if (_registry.TryGetValue(model, out TViewModel viewModel))
        {
            return viewModel;
        }
        
        viewModel = _factory(model);
        _registry[model] = viewModel;
        return viewModel;
    }

    public void Release(TModel model)
    {
        if (model == null) return;
        _registry.Remove(model);
    }
    
    public void NotifyUpdate(TModel model)
    {
        if (model == null) return;
        if (_registry.TryGetValue(model, out TViewModel vm))
        {
            vm.UpdateFromModel();
        }
    }
}