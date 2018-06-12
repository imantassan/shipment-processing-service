using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace ShipmentService.DependencyResolver
{
	/// <summary>
	/// Represents a binding link between a definition and implementation of a type
	/// </summary>
	/// <typeparam name="TDefinition">Type of the definition</typeparam>
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

		/// <summary>
		/// Creates an instance of <typeparamref name="TDefinition"/> within the given <paramref name="container"/> scope and <paramref name="callSync"/> call chain
		/// </summary>
		/// <param name="container"><see cref="Container"/> used to resolve dependencies of the implementation of <typeparamref name="TDefinition"/></param>
		/// <param name="callSync">Virtual call stack used to prevent circular references</param>
		/// <returns>An instance of <typeparamref name="TDefinition"/></returns>
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

			var constructors = ImplementationType.GetConstructors();
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

		/// <summary>
		/// Tries to create an instance of <typeparamref name="TDefinition"/> using the given <paramref name="constructor"/>
		/// </summary>
		/// <param name="constructor">Specific constructor to be used for creating an instance</param>
		/// <param name="container"><see cref="Container"/> used to resolve child dependencies</param>
		/// <param name="callSync">Virtual call stack used to prevent circular references</param>
		/// <returns>An instance of <typeparamref name="TDefinition"/> if all constructor parameters can be resolved, <c>null</c> otherwise</returns>
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