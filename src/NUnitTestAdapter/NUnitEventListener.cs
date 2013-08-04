﻿// ****************************************************************
// Copyright (c) 2011 NUnit Software. All rights reserved.
// ****************************************************************

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NUnit.Core;
using TestResult = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace NUnit.VisualStudio.TestAdapter
{
    /// <summary>
    /// NUnitEventListener implements the EventListener interface and
    /// translates each event into a message for the VS test platform.
    /// </summary>
    public class NUnitEventListener : MarshalByRefObject, EventListener // Public for testing
    {
        private readonly ITestExecutionRecorder testLog;
        //private string assemblyName;
        //private readonly Dictionary<string, NUnit.Core.TestNode> nunitTestCases;
        //private readonly TestConverter testConverter;
        private AssemblyFilter filter;

        //public NUnitEventListener(ITestExecutionRecorder testLog, Dictionary<string, NUnit.Core.TestNode> nunitTestCases, string assemblyName, bool isBuildFromTfs)
        //{
        //    this.testLog = testLog;
        //    this.assemblyName = assemblyName;
        //    this.nunitTestCases = nunitTestCases;
        //    this.testConverter = new TestConverter(assemblyName, nunitTestCases, isBuildFromTfs);
        //}

        public NUnitEventListener(ITestExecutionRecorder testLog, AssemblyFilter filter)
        {
            this.testLog = testLog;
            this.filter = filter;
            //this.assemblyName = assemblyName;
            //this.nunitTestCases = nunitTestCases;
            //this.testConverter = new TestConverter(assemblyName, nunitTestCases, isBuildFromTfs);
        }

        public void RunStarted(string name, int testCount)
        {
            testLog.SendMessage(TestMessageLevel.Informational, "Run started: " + name);
            //if (EqtTrace.IsVerboseEnabled)
            //{
            //EqtTrace.Verbose("Run started: " + name + " : testcount :" + testCount);
            //}
        }

        public void RunFinished(Exception exception)
        {
        }

        public void RunFinished(NUnit.Core.TestResult result)
        {
        }

        public string Output { get; private set; }

        public void SuiteStarted(TestName testName)
        {

        }

        public void SuiteFinished(NUnit.Core.TestResult result)
        {
            if ((result.IsError || result.IsFailure) &&
                (result.FailureSite == FailureSite.SetUp || result.FailureSite == FailureSite.TearDown))
            {
                testLog.SendMessage(
                    TestMessageLevel.Error,
                    string.Format("{0} failed for test fixture {1}", result.FailureSite, result.FullName));
                if (result.Message != null)
                    testLog.SendMessage(TestMessageLevel.Error, result.Message);
                if (result.StackTrace != null)
                    testLog.SendMessage(TestMessageLevel.Error, result.StackTrace);
            }
        }

        public void TestStarted(TestName testName)
        {
            string key = testName.UniqueName;

            // Simply ignore any TestName not found
            if (filter.NUnitTestCaseMap.ContainsKey(key))
            {
                var nunitTest = filter.NUnitTestCaseMap[key];
                var ourCase = filter.TestConverter.ConvertTestCase(nunitTest);
                this.testLog.RecordStart(ourCase);
               // Output = testName.FullName + "\r";
            }

        }

        public void TestFinished(NUnit.Core.TestResult result)
        {
            TestResult ourResult = filter.TestConverter.ConvertTestResult(result);
            ourResult.Messages.Add(new TestResultMessage(TestResultMessage.StandardOutCategory, Output));
            this.testLog.RecordEnd(ourResult.TestCase, ourResult.Outcome);
            this.testLog.RecordResult(ourResult);
            Output = "";
        }

        public void TestOutput(TestOutput testOutput)
        {
            string message = testOutput.Text;
            int length = message.Length;
            int drop = message.EndsWith(Environment.NewLine)
                ? Environment.NewLine.Length
                : message[length - 1] == '\n' || message[length - 1] == '\r'
                    ? 1
                    : 0;
            if (drop > 0)
                message = message.Substring(0, length - drop);
            this.testLog.SendMessage(TestMessageLevel.Informational, message);
            string type="";
            // Consider adding this later, as an option.
            //switch (testOutput.Type)
            //{
            //    case TestOutputType.Trace:
            //        type ="Debug: ";
            //        break;
            //    case TestOutputType.Out:
            //        type ="Console: ";
            //        break;
            //    case TestOutputType.Log:
            //        type="Log: ";
            //        break;
            //    case TestOutputType.Error:
            //        type="Error: ";
            //        break;
            //}
            this.Output += (type+message+'\r');
        }

        public void UnhandledException(Exception exception)
        {
        }

        //public void Dispose()
        //{
        //    if (this.testConverter != null)
        //        this.testConverter.Dispose();
        //}
    }
}
