using System.IO;
using System.Linq;
using NUnit.Framework;
using NUnit.Tests.Assemblies;
using NUnit.VisualStudio.TestAdapter.Tests.Fakes;

namespace NUnit.VisualStudio.TestAdapter.Tests
{
    [Category("TestExecution")]
    public class TestFilteringTests
    {
        private string mockAssemblyPath;
        [OneTimeSetUp]
        public void LoadMockassembly()
        {
            mockAssemblyPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "mock-assembly.dll");

            // Sanity check to be sure we have the correct version of mock-assembly.dll
            Assert.That(MockAssembly.TestsAtRuntime, Is.EqualTo(MockAssembly.Tests),
                "The reference to mock-assembly.dll appears to be the wrong version");
        }

        [TestCase("", 35)]
        [TestCase(null, 35)]
        [TestCase("cat == Special", 1)]
        [TestCase("cat == MockCategory", 2)]
        [TestCase("method =~ MockTest?", 5)]
        [TestCase("method =~ MockTest? and cat != MockCategory", 3)]
        [TestCase("namespace == ThisNamespaceDoesNotExist", 0)]
        [TestCase("test==NUnit.Tests.Assemblies.MockTestFixture", MockTestFixture.Tests - MockTestFixture.Explicit, TestName = "{m}_MockTestFixture")]
        [TestCase("test==NUnit.Tests.IgnoredFixture and method == Test2", 1, TestName = "{m}_IgnoredFixture")]
        [TestCase("class==NUnit.Tests.Assemblies.MockTestFixture", MockTestFixture.Tests - MockTestFixture.Explicit)]
        [TestCase("name==MockTestFixture", MockTestFixture.Tests + NUnit.Tests.TestAssembly.MockTestFixture.Tests - MockTestFixture.Explicit)]
        [TestCase("cat==FixtureCategory", MockTestFixture.Tests - MockTestFixture.Explicit)]
        public void TestsWhereShouldFilter(string filter, int expectedCount)
        {
            // Create a fake environment.
            var context = new FakeRunContext(new FakeRunSettingsForWhere(filter));
            var fakeFramework = new FakeFrameworkHandle();

            var executor = TestAdapterUtils.CreateExecutor();
            executor.RunTests(new[] { mockAssemblyPath }, context, fakeFramework);

            var completedRuns = fakeFramework.Events.Where(e => e.EventType == FakeFrameworkHandle.EventType.RecordEnd);

            Assert.That(completedRuns, Has.Exactly(expectedCount).Items);
        }
    }
}