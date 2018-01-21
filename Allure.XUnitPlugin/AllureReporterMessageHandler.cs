using Allure.Commons;
using Xunit;
using Xunit.Abstractions;

namespace Allure.XUnitPlugin
{
    public class AllureReporterMessageHandler : TestMessageSink
    {
        readonly IRunnerLogger logger;
        static AllureLifecycle allure = AllureLifecycle.Instance;

        public AllureReporterMessageHandler(IRunnerLogger logger)
        {
            this.logger = logger;

            Execution.TestCollectionStartingEvent += HandleTestCollectionStarting;
            Execution.TestCollectionFinishedEvent += HandleTestCollectionFinished;

            /*Execution.TestMethodStartingEvent += HandleMethodStartingEvent;
            Execution.TestMethodFinishedEvent += HandleMethodFinishedEvent;

            Execution.TestStartingEvent += HandleStarting;

            Execution.TestPassedEvent += HandlePassed;
            Execution.TestSkippedEvent += HandleSkipped;
            Execution.TestFailedEvent += HandleFailed;

            Execution.TestFinishedEvent += HandleFinishedEvent;*/
        }
        
        private void HandleTestCollectionStarting(MessageHandlerArgs<ITestCollectionStarting> args)
        {
            var testCollectionStarting = args.Message;
            string key = $"{testCollectionStarting.TestCollection.UniqueID}";
            
            var collectionContainer = new TestResultContainer
            {
                uuid = key,
                name = testCollectionStarting.TestCollection.DisplayName
            };
            
            allure.StartTestContainer(collectionContainer);
        }

        private void HandleTestCollectionFinished(MessageHandlerArgs<ITestCollectionFinished> args)
        {
            string key = $"{args.Message.TestCollection.UniqueID}";
            allure.StopTestContainer(key);
        }
    }
}