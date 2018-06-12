using System;

using ShipmentService.DependencyResolver;
using ShipmentService.RequestProcessor.FileParser;
using ShipmentService.RequestProcessor.Rules;

namespace ShipmentService.App
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new Container();
			container.Bind<IFileParser>().To<FileParser>();
			container.Bind<IRouter>().To<Router>();
			container.Bind<IProvider>().To<Provider>();

	        var router = container.Resolve<IRouter>();
        }
    }
}
