using FluentAssertions;
using Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace IocTests
{
    public class IocTesting
    {
        [Fact]
        public void CanCreateSimpleTypeWithoutConstructors()
        {
            var container = new IocContainer();
            container.Register<MySimpleThing>();
            var mySimpleThing = container.Resolve<MySimpleThing>();
            mySimpleThing.Should().NotBeNull();
        }

        [Fact]
        public void CanCreateSimpleTypeWithOneParameter()
        {
            var container = new IocContainer();
            container.Register<MySimpleThing>();
            container.Register<MySlightlySimpleThing>();
            var mySlightlySimpleThing = container.Resolve<MySlightlySimpleThing>();
            mySlightlySimpleThing.Should().NotBeNull();
            mySlightlySimpleThing.SimpleThing.Should().NotBeNull();
            mySlightlySimpleThing.SimpleThing.MyName.Should().Be("PropSnippetYourself");
        }

        [Fact]
        public void CanCreateSimpleTypeWithOneParameterUsingFactoryMethod()
        {
            var container = new IocContainer();
            container.Register(() => new MySimpleThing()
            {
                MyName = "MyNameIsBob"
            });
            container.Register<MySlightlySimpleThing>();
            var mySlightlySimpleThing = container.Resolve<MySlightlySimpleThing>();
            mySlightlySimpleThing.Should().NotBeNull();
            mySlightlySimpleThing.SimpleThing.Should().NotBeNull();
            mySlightlySimpleThing.SimpleThing.MyName.Should().Be("MyNameIsBob");
        }

        [Fact]
        public void CanCreateSimpleTypeWithOneParameterUsingActionMethod()
        {
            var container = new IocContainer();
            container.Register<MySimpleThing>(t => t.MyName = "MyNameIsT");
            container.Register<MySlightlySimpleThing>();
            var mySlightlySimpleThing = container.Resolve<MySlightlySimpleThing>();
            mySlightlySimpleThing.Should().NotBeNull();
            mySlightlySimpleThing.SimpleThing.Should().NotBeNull();
            mySlightlySimpleThing.SimpleThing.MyName.Should().Be("MyNameIsT");
        }

        [Fact]
        public void CanRegisterInterfaceAndResolve()
        {
            var container = new IocContainer();

            container.Register<IDatabaseRepository, SqlDatabaseRepository>();
            container.Register<ZooController>();

            var databaseRepository = container.Resolve<IDatabaseRepository>();
            databaseRepository.GetType().Should().Be(typeof(SqlDatabaseRepository));

            var controller = container.Resolve<ZooController>();

            var result = controller.AddMonkeys();
            result.Should().Be(1);
        }

        [Fact]
        public void CanRegisterInterfaceAndResolve_2()
        {
            var container = new IocContainer();

            container.Register<IDatabaseRepository, MemoryDatabaseRepository>();
            container.Register<ZooController>();

            var databaseRepository = container.Resolve<IDatabaseRepository>();
            databaseRepository.GetType().Should().Be(typeof(MemoryDatabaseRepository));

            var controller = container.Resolve<ZooController>();

            var result = controller.AddMonkeys();
            result.Should().Be(10);
        }

        [Fact]
        public void ResolvingATypeWithoutRegisteringAllDependenciesThrowsException()
        {
            var container = new IocContainer();

            container.Register<ZooController>();

            Action act = () =>
            {
                container.Resolve<ZooController>();
            };

            act.ShouldThrow<InvalidOperationException>().WithMessage("Unable to resolve constructor of type IocTests.IocTesting+ZooController");
        }

        [Fact]
        public void ResolvingATypeWithoutRegisteringAllDependenciesForDependentThrowsException()
        {
            var container = new IocContainer();

            container.Register<ZooController>();
            container.Register<IDatabaseRepository, DependentDatabaseRepository>();

            Action act = () =>
            {
                container.Resolve<ZooController>();
            };

            act.ShouldThrow<InvalidOperationException>().WithMessage("Unable to resolve constructor of type IocTests.IocTesting+DependentDatabaseRepository");
        }

        public class MySimpleThing
        {
            public string MyName { get; set; } = "PropSnippetYourself";
        }

        public class MySlightlySimpleThing
        {

            public MySlightlySimpleThing(MySimpleThing simpleThing)
            {
                SimpleThing = simpleThing;
            }

            public MySimpleThing SimpleThing { get; private set; }
        }

        public interface IDatabaseRepository
        {
            int AddMonkeys(string length);
        }

        public class SqlDatabaseRepository : IDatabaseRepository
        {
            public int AddMonkeys(string length)
            {
                return 1;
            }
        }

        public class MemoryDatabaseRepository : IDatabaseRepository
        {
            public int AddMonkeys(string length)
            {
                return 10;
            }
        }

        public class DependentDatabaseRepository : IDatabaseRepository
        {
            private MySimpleThing _mySimpleThing;

            public DependentDatabaseRepository(MySimpleThing mySimpleThing)
            {
                _mySimpleThing = mySimpleThing;
            }

            public int AddMonkeys(string length)
            {
                throw new NotImplementedException();
            }
        }

        public class ZooController
        {
            private IDatabaseRepository _databaseRepository;

            public ZooController(IDatabaseRepository databaseRepository)
            {
                _databaseRepository = databaseRepository;
            }

            public int AddMonkeys()
            {
                return _databaseRepository.AddMonkeys("ABC");
            }
        }
    }
}
