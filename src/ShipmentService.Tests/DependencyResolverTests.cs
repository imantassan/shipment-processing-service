using System;

using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using ShipmentService.DependencyResolver;

namespace ShipmentService.Tests
{
	public class ChildClass
	{
	}
	public class ParentClass
	{
		public ParentClass(ChildClass childClass)
		{
			ChildClass = childClass;
		}

		public ChildClass ChildClass { get; }	
	}

	public class UnregisteredClass
	{
	}

	public class UnregisteredParentClass
	{
		public UnregisteredParentClass(UnregisteredClass childClass)
		{
			ChildClass = childClass;
		}

		public UnregisteredClass ChildClass { get; }
	}

	public class CircularReferenceClass
	{
		public CircularReferenceClass(CircularReferenceClass circularReference)
		{
			CircularReference = circularReference;
		}

		public CircularReferenceClass CircularReference { get; }
	}

	[TestClass]
	public class DependencyResolverTests
	{
		[TestMethod]
		public void DependencyResolver_RegisteredDependencies_ResolvesSucessfully()
		{
			// arrange
			var container = new Container();
			container.Bind<ChildClass>().To<ChildClass>();
			container.Bind<ParentClass>().To<ParentClass>();
			
			// act + assert
			((Action)(() => container.Resolve<ParentClass>())).Should().NotThrow();
		}

		[TestMethod]
		public void DependencyResolver_UnregisteredClass_Fails()
		{
			// arrange
			var container = new Container();
			container.Bind<UnregisteredParentClass>().To<UnregisteredParentClass>();

			// act + assert
			((Action)(() => container.Resolve<UnregisteredParentClass>())).Should()
				.Throw<Exception>()
				.Which.Message.Should()
				.Contain(typeof(UnregisteredParentClass).Name)
				.And.Contain("constructor");
		}

		[TestMethod]
		public void DependencyResolver_CircularReference_ShouldDetectAndFail()
		{
			// arrange
			var container = new Container();
			container.Bind<CircularReferenceClass>().To<CircularReferenceClass>();

			// act + assert
			((Action)(() => container.Resolve<CircularReferenceClass>())).Should().Throw<Exception>().Which.Message.Should().Contain("Circular reference");
		}
	}
}