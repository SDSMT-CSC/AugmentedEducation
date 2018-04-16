using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ARFE.Tests
{
    /// <summary>
    /// Summary description for UploadedFileCacheTests
    /// </summary>
    [TestClass]
    public class UploadedFileCacheTests
    {
        #region Members

        private TestContext _TestContextInstance;

        #endregion


        #region Constructor

        public UploadedFileCacheTests() { }

        #endregion


        #region Properties

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get { return _TestContextInstance; }
            set { _TestContextInstance = value; }
        }

        #endregion


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


        #region Tests

        /// <summary>
        /// Test that the constructor of the UploadedFileCache is in fact private.
        /// Calling Activator.CreateInstance will try to call the object constructor
        /// but will throw a MissingMethodException since one hasn't been made publicly 
        /// accessible due to the fact that it's a singleton cache. 
        /// </summary>
        [TestMethod]
        public void Test_PrivateConstructor()
        {
            UploadedFileCache cache = null;
            Type cacheType = typeof(UploadedFileCache);

            Assert.IsNull(cache);

            //assert couldn't create, still null
            Assert.ThrowsException<MissingMethodException>(() => {
                cache = (Activator.CreateInstance(cacheType) as UploadedFileCache);
            });

            Assert.IsNull(cache);
        }

        /// <summary>
        /// Test that the publicly available static init method, <see cref="UploadedFileCache.GetInstance"/>,
        /// creates an instance of the UploadedFileCache object.
        /// </summary>
        [TestMethod]
        public void Test_Init()
        {
            UploadedFileCache cache = null;
            Assert.IsNull(cache);

            cache = UploadedFileCache.GetInstance();

            Assert.IsNotNull(cache);
            Assert.IsInstanceOfType(cache, typeof(UploadedFileCache));
        }

        #endregion
    }
}
