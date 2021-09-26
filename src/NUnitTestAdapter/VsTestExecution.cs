using NUnit.Engine;
using NUnit.VisualStudio.TestAdapter.NUnitEngine;

namespace NUnit.VisualStudio.TestAdapter
{
    public class VsTestExecution : Execution
    {
        public VsTestExecution(IExecutionContext ctx) : base(ctx)
        {
        }

        public override bool Run(TestFilter filter, DiscoveryConverter discovery, NUnit3TestExecutor nUnit3TestExecutor)
        {
            filter = CheckVsTestFilter(filter, discovery, VsTestFilter);

            if (filter == NUnitTestFilterBuilder.NoTestsFound)
            {
                TestLog.Info("   Skipping assembly - no matching test cases found");
                return false;
            }
            return base.Run(filter, discovery, nUnit3TestExecutor);
        }

        public TestFilter CheckVsTestFilter(TestFilter filter, IDiscoveryConverter discovery, IVsTestFilter vsTestFilter)
        {
            // If we have a VSTest TestFilter, convert it to an nunit filter
            if (vsTestFilter == null || vsTestFilter.IsEmpty)
                return filter;
            TestLog.Debug(
                $"TfsFilter used, length: {vsTestFilter.TfsTestCaseFilterExpression?.TestCaseFilterValue.Length}");
            // NOTE This overwrites filter used in call
            var filterBuilder = CreateTestFilterBuilder();
            if (Settings.DiscoveryMethod == DiscoveryMethod.Current)
            {
                filter = Settings.UseNUnitFilter
                    ? filterBuilder.ConvertVsTestFilterToNUnitFilter(vsTestFilter)
                    : filterBuilder.ConvertTfsFilterToNUnitFilter(vsTestFilter, discovery);
            }
            else
            {
                filter = filterBuilder
                    .ConvertTfsFilterToNUnitFilter(vsTestFilter, discovery.LoadedTestCases);
            }

            Dump?.AddString($"\n\nTFSFilter: {vsTestFilter.TfsTestCaseFilterExpression.TestCaseFilterValue}\n");
            Dump?.DumpVSInputFilter(filter, "(At Execution (TfsFilter)");

            return filter;
        }
        public override TestFilter CheckFilterInCurrentMode(TestFilter filter, IDiscoveryConverter discovery)
        {
            if (!discovery.IsDiscoveryMethodCurrent)
                return filter;
            if ((VsTestFilter == null || VsTestFilter.IsEmpty) && filter != TestFilter.Empty)
            {
                filter = CheckFilter(discovery);
            }
            else if (VsTestFilter is { IsEmpty: false } && !Settings.UseNUnitFilter)
            {
                var s = VsTestFilter.TfsTestCaseFilterExpression.TestCaseFilterValue;
                var scount = s.Split('|', '&').Length;
                if (scount > Settings.AssemblySelectLimit)
                {
                    TestLog.Debug("Setting filter to empty due to TfsFilter size");
                    filter = TestFilter.Empty;
                }
            }

            return filter;
        }
    }
}