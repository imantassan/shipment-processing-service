using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ShipmentService.DependencyResolver
{
	/// <summary>
	/// Class represeting a simple dependency resolver supporting only an instance per container scope
	/// </summary>
	public class Container
	{
		private readonly Dictionary<Type, object> registry;

		/// <summary>
		/// Initializes a new instance of <see cref="Container"/> class.
		/// </summary>
		public Container()
		{
			registry = new Dictionary<Type, object>();
		}

		/// <summary>
		/// Binds the given <typeparamref name="TDefinition"/> type to its implementation
		/// </summary>
		/// <typeparam name="TDefinition">Type of the definition</typeparam>
		/// <returns>A <see cref="Binding{TDefinition}"/> allowing to bind it to a specific implementation type.</returns>
		public Binding<TDefinition> Bind<TDefinition>()
		{
			var binding = new Binding<TDefinition>();
			registry.Add(typeof(TDefinition), binding);

			return binding;
		}

		/// <summary>
		/// Resolves a definition type to an instance of its implementation type
		/// </summary>
		/// <typeparam name="TDefinition">Type of deinition to resolve to implementation instance</typeparam>
		/// <returns>An instance of implementation</returns>
		public TDefinition Resolve<TDefinition>()
		{
			var callSync = new HashSet<Type>();
			return Resolve<TDefinition>(callSync);
		}

		/// <summary>
		/// Returns a flag indicating whether the given <paramref name="bindingType"/> has a binding in registry
		/// </summary>
		internal bool IsBound(Type bindingType)
		{
			return registry.ContainsKey(bindingType);
		}
		
		/// <summary>
		/// Resolves definition to implementation using compile-time types
		/// </summary>
		internal TDefinition Resolve<TDefinition>(HashSet<Type> callSync)
		{
			return (TDefinition)((Binding<TDefinition>)registry[typeof(TDefinition)]).CreateInstance(this, callSync);
		}

		/// <summary>
		/// Resolves definition to implementation using reflection, used when compile-time types are not available
		/// </summary>
		internal object Resolve(Type definitionType, HashSet<Type> callSync)
		{
			var method = typeof(Container).GetMethod(nameof(Resolve), BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(HashSet<Type>) }, null);
			method = method.MakeGenericMethod(definitionType);
			try
			{
				return method.Invoke(this, BindingFlags.InvokeMethod, null, new object[] { callSync }, CultureInfo.InvariantCulture);
			}
			catch (TargetInvocationException exception)
			{
				throw exception.InnerException;
			}
		}
	}
}