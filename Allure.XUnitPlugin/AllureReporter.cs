using Xunit;
using Xunit.Abstractions;

namespace Allure.XUnitPlugin
{
    public class AllureReporter : IRunnerReporter
    {
        public string Description => "Reporting tests results to Allure";

        public bool IsEnvironmentallyEnabled => false;

        public string RunnerSwitch => "allure";

        public IMessageSink CreateMessageHandler(IRunnerLogger logger) => new AllureReporterMessageHandler(logger);
    }
}