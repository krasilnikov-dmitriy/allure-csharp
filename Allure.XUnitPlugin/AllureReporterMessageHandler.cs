using System;
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
            Execution.TestMethodFinishedEvent += HandleMethodFinishedEvent;*/

            Execution.TestStartingEvent += HandleStarting;

            Execution.TestPassedEvent += HandlePassed;
            Execution.TestSkippedEvent += HandleSkipped;
            Execution.TestFailedEvent += HandleFailed;

            Execution.TestFinishedEvent += HandleFinishedEvent;
        }

        private void HandleFinishedEvent(MessageHandlerArgs<ITestFinished> args)
        {
            var testResult = args.Message;
            allure.UpdateTestCase(testResult.TestCase.UniqueID, t => t.stage = Stage.finished);
        }

        private void HandleFailed(MessageHandlerArgs<ITestFailed> args)
        {
            var testResult = args.Message;
            allure.UpdateTestCase(testResult.TestCase.UniqueID, t => t.status = Status.failed);
        }

        private void HandleSkipped(MessageHandlerArgs<ITestSkipped> args)
        {
            var testResult = args.Message;
            allure.UpdateTestCase(testResult.TestCase.UniqueID, t => t.status = Status.skipped);
        }

        private void HandlePassed(MessageHandlerArgs<ITestPassed> args)
        {
            var testResult = args.Message;
            allure.UpdateTestCase(testResult.TestCase.UniqueID, t => t.status = Status.passed);
        }

        private void HandleStarting(MessageHandlerArgs<ITestStarting> args)
        {
            var testStarting = args.Message;
            
            var testContainer = new TestResult
            {
                uuid = testStarting.TestCase.UniqueID,
                name = testStarting.TestCase.DisplayName
            };

            allure.StartTestCase(testContainer);
        }
        

        private void HandleTestCollectionStarting(MessageHandlerArgs<ITestCollectionStarting> args)
        {
            Console.WriteLine("###########################################################");
            Console.WriteLine("###########################################################");
            Console.WriteLine("###########################################################");
            Console.WriteLine("###########################################################");
            Console.WriteLine("###########################################################");
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