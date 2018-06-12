using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;

namespace ShipmentService.DependencyResolver
{
	public class Binding<TDefinition>
	{
		internal Binding()
		{
			DefinitionType = typeof(TDefinition);
		}

		internal Type DefinitionType { get; set; }

		internal Type ImplementationType { get; set; }

		internal object Instance { get; set; }

		public void To<TImplementation>()
			where TImplementation : class, TDefinition
		{
			ImplementationType = typeof(TImplementation);
		}

		internal object CreateInstance(Container container, HashSet<Type> callSync)
		{
			if (callSync.Contains(typeof(TDefinition)))
			{
				throw new Exception("Circular reference detected");
			}

			callSync.Add(typeof(TDefinition));

			if (Instance != null)
			{
				return Instance;
			}

			var constructors = ImplementationType.GetConstructors().ToList();
			constructors.Sort((x, y) => x.GetParameters().Length - y.GetParameters().Length);

			foreach (var constructor in constructors)
			{
				var result = CreateFromConstructor(constructor, container, callSync);
				if (result == null)
				{
					continue;
				}

				return result;
			}

			throw new Exception($"No matching constructor could be found for {ImplementationType.Name} bound as {DefinitionType.Name}");
		}

		private static object CreateFromConstructor(ConstructorInfo constructor, Container container, HashSet<Type> callSync)
		{

			var parameters = constructor.GetParameters();
			var instanceParameters = new List<object>(parameters.Length);

			foreach (var parameter in parameters)
			{
				if (!container.IsBound(parameter.ParameterType))
				{
					return default(TDefinition);
				}

				instanceParameters.Add(container.Resolve(parameter.ParameterType, callSync));
			}

			return (TDefinition)constructor.Invoke(BindingFlags.CreateInstance, null, instanceParameters.ToArray(), CultureInfo.InvariantCulture);
		}
	}
}