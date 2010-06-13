using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LaunchLaterOM;

namespace LaunchLaterUnitTests
{
    /// <summary>
    /// Summary description for LLApplicationManagerTests
    /// </summary>
    [TestClass]
    public class LLApplicationManagerTests
    {
        public LLApplicationManagerTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestAppDetectsItselfTryingToRun_Success()
        {
            Assert.IsTrue(LLUtilities.LLIsTryingToRunItself("lslkd\\jksdfj\\LL_LaunchPad.exe"));

        }

        [TestMethod]
        public void TestAppDetectsItselfTryingToRun_Fail()
        {
            Assert.IsFalse(LLUtilities.LLIsTryingToRunItself("lksjdfldkjf"));
        }

        [TestMethod]
        public void TestConfigurationLoaded_Success()
        {
            LLConfiguration config = new LLConfiguration("LaunchLaterApps.config");

            Assert.IsTrue(config.Profiles.First().Applications.Count > 0);

        }

        [TestMethod]
        [ExpectedException(typeof(LLConfigurationException))]
        public void TestConfigurationLoaded_Fail()
        {
            LLConfiguration config = new LLConfiguration("blah.config");
        }

        [TestMethod]
        public void TestApplicationXMLLoaded_Success()
        {
            LLConfiguration config = new LLConfiguration("LaunchLaterApps.config");
            var apps = config.Profiles.First().Applications;
            var NotePadXML = apps.First().ToString();
            Assert.IsTrue(NotePadXML == @"<LLApplication Name=""Notepad"" FullPath=""notepad.exe"" DelaySeconds=""3"" Arguments="""" />");
        }

       

    }
}
