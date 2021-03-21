using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObjectGraphPath;
using ObjectGraphPathTest.TestGraphModels;

namespace ObjectGraphPathTest
{
    [TestClass]
    public class UnitTest1
    {

        private IServiceProvider ServiceProvider()
        {
            return new ServiceCollection()
                .AddSingleton<ISelectorVisitor, SelectorVisitor<Product>>()
                .AddSingleton<GraphFactory<Product>, GraphFactory<Product>>()
                .AddSingleton<IGraphNodeFactory,GraphNodeFactory>()
                .AddLogging()
                .BuildServiceProvider();

        }
        [TestMethod]
        public void TestNonSelectMethodInSelector()
        {
            var serviceProvider = ServiceProvider();
            var factory = serviceProvider.GetService<GraphFactory<Product>>();
            var duffIncludePaths = new Expression<Func<Product, object>>[]
            {
                (x)=>x.Dependencies.GroupBy(y=>y.RequiresChildPart)
            };
            Assert.ThrowsException<Exception>(() => factory?.SelectorsToGraph(duffIncludePaths), "Method is not select in selector \"x.Dependencies.GroupBy(y => y.RequiresChildPart)\"");
        }

        [TestMethod]
        public void TestSelectorToGraph()
        {
            var serviceProvider = ServiceProvider();
            var factory = serviceProvider.GetService<GraphFactory<Product>>();
            var graph = factory?.SelectorsToGraph(IncludePaths);

            Assert.AreEqual("{x.{.Manufacturer,.Dependencies.{.RequiresChildPart,.ParentPart}}}", graph?.ToString());
        }

        private Expression<Func<Product, object>>[] IncludePaths => new Expression<Func<Product, object>>[]
        {
            (x)=>x.Manufacturer,
            (x)=>x.Dependencies.Select(y=>y.RequiresChildPart),
            (x)=>x.Dependencies.Select(y=>y.ParentPart)
        };


        [TestMethod]
        public void TestDataGraphToJson()
        {
            var serviceProvider = ServiceProvider();
            var factory = serviceProvider.GetService<GraphFactory<Product>>();
            var graph = factory?.SelectorsToGraph(IncludePaths);
            var products = BuildTestObjectGraph();
        }

        private Product[] BuildTestObjectGraph()
        {
            var manufacturer = new Manufacturer {ManufacturerName = "Bolts Inc."};
            var bolt5Mm = new Product {PartNumber = "5mmBolt", Manufacturer = manufacturer};
            var nut5Mm = new Product {PartNumber = "5mmNut", Manufacturer = manufacturer };
            var nutDependency = new DependentPart {ParentPart = bolt5Mm, RequiresChildPart = nut5Mm, Optional = false};
            var washer5Mm = new Product { PartNumber = "5mmNut", Manufacturer = manufacturer };
            var washerDependency = new DependentPart { ParentPart = bolt5Mm, RequiresChildPart = washer5Mm, Optional = false };
            bolt5Mm.Dependencies = new List<DependentPart> {nutDependency, washerDependency};
            manufacturer.Products =
                new[]
                {
                    bolt5Mm,
                    nut5Mm,
                    washer5Mm
                };
            return manufacturer.Products.ToArray();
        }
    }
}
