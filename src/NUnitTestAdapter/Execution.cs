using System;
using NUnit.Engine;
using NUnit.VisualStudio.TestAdapter.Dump;
using NUnit.VisualStudio.TestAdapter.NUnitEngine;

namespace NUnit.VisualStudio.TestAdapter
{
    public interface IExecutionContext
    {
        ITestLogger Log { get; }
        INUnitEngineAdapter EngineAdapter { get; }
        string TestOutputXmlFolder { get; }
        IAdapterSettings Settings { get; }
        IDumpXml Dump { get; }

        IVsTestFilter VsTestFilter { get; }
    }

    public static class ExecutionFactory
    {
        public static Execution Create(IExecutionContext ctx)
        {
            if (ctx.Settings.DesignMode) // We come from IDE
                return new IdeExecution(ctx);
            return new VsTestExecution(ctx);
        }
    }

    public abstract class Execution
    {
        protected string TestOutputXmlFolder => ctx.TestOutputXmlFolder;
        private readonly IExecutionContext ctx;
        protected ITestLogger TestLog => ctx.Log;
        protected IAdapterSettings Settings => ctx.Settings;

        protected IDumpXml Dump => ctx.Dump;
        protected IVsTestFilter VsTestFilter => ctx.VsTestFilter;

        protected INUnitEngineAdapter NUnitEngineAdapter => ctx.EngineAdapter;
        protected Execution(IExecutionContext ctx)
        {
            this.ctx = ctx;
        }



        public virtual bool Run(TestFilter filter, DiscoveryConverter discovery, NUnit3TestExecutor nUnit3TestExecutor)
        {
            filter = CheckFilterInCurrentMode(filter, discovery);
            nUnit3TestExecutor.Dump?.StartExecution(filter, "(At Execution)");
            var converter = CreateConverter(discovery);
            using var listener = new NUnitEventListener(converter, nUnit3TestExecutor);
            try
            {
                var results = NUnitEngineAdapter.Run(listener, filter);
                NUnitEngineAdapter.GenerateTestOutput(results, discovery.AssemblyPath, TestOutputXmlFolder);
            }
            catch (NullReferenceException)
            {
                // this happens during the run when CancelRun is called.
                TestLog.Debug("   Null ref caught");
            }

            return true;
        }

        public abstract TestFilter CheckFilterInCurrentMode(TestFilter filter, IDiscoveryConverter discovery);

        protected NUnitTestFilterBuilder CreateTestFilterBuilder()
            => new (NUnitEngineAdapter.GetService<ITestFilterService>(), Settings);
        protected ITestConverterCommon CreateConverter(DiscoveryConverter discovery) => Settings.DiscoveryMethod == DiscoveryMethod.Current ? discovery.TestConverter : discovery.TestConverterForXml;

        protected TestFilter CheckFilter(IDiscoveryConverter discovery)
        {
            TestFilter filter;
            if (discovery.NoOfLoadedTestCasesAboveLimit)
            {
                TestLog.Debug("Setting filter to empty due to number of testcases");
                filter = TestFilter.Empty;
            }
            else
            {
                var filterBuilder = CreateTestFilterBuilder();
                filter = filterBuilder.FilterByList(discovery.LoadedTestCases);
            }
            return filter;
        }
    }
}
