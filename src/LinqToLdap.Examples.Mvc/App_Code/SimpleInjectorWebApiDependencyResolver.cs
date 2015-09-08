using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web.Http.Dependencies;
using SimpleInjector;

namespace LinqToLdap.Examples.Mvc
{
    //public sealed class SimpleInjectorWebApiDependencyResolver : IDependencyResolver
    //{
    //    private readonly Container _container;

    //    public SimpleInjectorWebApiDependencyResolver(Container container)
    //    {
    //        this._container = container;
    //    }

    //    [DebuggerStepThrough]
    //    public IDependencyScope BeginScope()
    //    {
    //        return this;
    //    }

    //    [DebuggerStepThrough]
    //    public object GetService(Type serviceType)
    //    {
    //        return ((IServiceProvider)this._container)
    //            .GetService(serviceType);
    //    }

    //    [DebuggerStepThrough]
    //    public IEnumerable<object> GetServices(Type serviceType)
    //    {
    //        return this._container.GetAllInstances(serviceType);
    //    }

    //    [DebuggerStepThrough]
    //    public void Dispose()
    //    {
    //    }
    //}
}