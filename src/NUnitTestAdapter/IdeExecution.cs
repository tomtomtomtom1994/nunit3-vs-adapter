using NUnit.Engine;
using NUnit.VisualStudio.TestAdapter.NUnitEngine;

namespace NUnit.VisualStudio.TestAdapter
{
    public class IdeExecution : Execution
    {
        public IdeExecution(IExecutionContext ctx) : base(ctx)
        {
        }
        public override bool Run(TestFilter filter, DiscoveryConverter discovery, NUnit3TestExecutor nUnit3TestExecutor)
        {
            return base.Run(filter, discovery, nUnit3TestExecutor);
        }

        public override TestFilter CheckFilterInCurrentMode(TestFilter filter, IDiscoveryConverter discovery)
        {
            if (!discovery.IsDiscoveryMethodCurrent)
                return filter;
            if (filter.IsEmpty())
                return filter;
            filter = CheckFilter(discovery);
            return filter;
        }
    }
}