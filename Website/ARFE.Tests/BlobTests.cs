using System;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace ARFE.Tests
{
    /// <summary>
    /// Unit tests covering the non-Azure-facing functionality of the BlobManager.cs file in ARFE.csproj
    /// 
    /// Calls to Azure storage are unable to be made from this test project.  The BlobManager class is 
    /// unable to appropriately access the configuration string required from the Web.config file, located
    /// at ARFE/Web.config, without the application startup sequence. Azure-facing functionality of the 
    /// BlobManager.cs file must be manually tested.
    /// </summary>
    [TestClass]
    public class BlobTests
    {
        /// <summary>
        /// Test the validity of the using alias at the top of this file
        /// (using BlobMan = ARFE.BlobManager;) and the correctness of the 
        /// constructor of the BlobManager class.
        /// </summary>
        [TestMethod]
        public void Test_Constructor()
        {
            BlobManager blobManager = null;

            Assert.IsNull(blobManager);
            Assert.IsInstanceOfType(blobManager, typeof(ARFE.BlobManager), "Bad using alias");

            blobManager = new BlobManager();

            Assert.IsNotNull(blobManager);
        }


        /// <summary>
        /// Test that the method for formatting blob container names follows Microsoft naming procedures
        /// available at https://blogs.msdn.microsoft.com/jmstall/2014/06/12/azure-storage-naming-rules/
        /// </summary>
        [TestMethod]
        public void Test_FormatBlobContainerName()
        {
            Random random = new Random();
            BlobManager blobManager = new BlobManager();
            StringBuilder longString = new StringBuilder();
            StringBuilder randString = new StringBuilder();
            string falseNegative = "Is a valid container name.";
            string falsePositive = "Is not a valid container name.";

            while (longString.Length < 75)
                longString.Append('a');

            Assert.IsFalse(VerifyFormattedContainerName(blobManager.FormatBlobContainerName("")), falseNegative);
            Assert.IsFalse(VerifyFormattedContainerName(blobManager.FormatBlobContainerName("1")), falseNegative);
            Assert.IsFalse(VerifyFormattedContainerName(blobManager.FormatBlobContainerName("12")), falseNegative);

            Assert.IsTrue(VerifyFormattedContainerName(blobManager.FormatBlobContainerName("123")), falsePositive);
            //capitalization doesn't matter
            Assert.IsTrue(VerifyFormattedContainerName(blobManager.FormatBlobContainerName("brady")), falsePositive);
            Assert.IsTrue(VerifyFormattedContainerName(blobManager.FormatBlobContainerName("Brady")), falsePositive);
            Assert.IsTrue(VerifyFormattedContainerName(blobManager.FormatBlobContainerName("BRADY")), falsePositive);
            //symbols get replaced with '-'
            Assert.IsTrue(VerifyFormattedContainerName(blobManager.FormatBlobContainerName("brady@mail.com")), falsePositive);
            //long strings should just get truncated
            Assert.IsTrue(VerifyFormattedContainerName(blobManager.FormatBlobContainerName(longString.ToString())), falsePositive);

            //test random byte strings to make sure rules hold when the input isn't known
            for (int randomAttempt = 0; randomAttempt < 10; randomAttempt++)
            {
                int randLength = random.Next(100);
                byte[] buffer = new byte[randLength];

                random.NextBytes(buffer);
                foreach (byte b in buffer)
                {
                    randString.Append((char)b);
                }
                string formatted = blobManager.FormatBlobContainerName(randString.ToString());

                if (randLength > 2 && formatted.Length > 2)
                    Assert.IsTrue(VerifyFormattedContainerName(formatted), $"{randString.ToString()} {falsePositive}");
                else
                    Assert.IsFalse(VerifyFormattedContainerName(formatted), $"{randString.ToString()} {falseNegative}");
                randString.Clear();
            }
        }

        /// <summary>
        /// Verify that a blob container name provided by the call to 
        /// <see cref="BlobManager.FormatBlobContainerName(string)"/>
        /// meets all of the blob container name rule standards.
        /// </summary>
        /// <param name="containerName">The formatted container name produced.</param>
        /// <returns>
        ///     <ul>
        ///         <li>True: The name is valid.</li>
        ///         <li>False: The name is not valid.</li>
        ///     </ul>
        /// </returns>
        private bool VerifyFormattedContainerName(string containerName)
        {
            bool valid = (containerName.Length >= 3 && containerName.Length < 63);

            if (valid)
            {
                //doesn't start with -, doesn't end with -, no --
                valid = valid && !(containerName.StartsWith("-"));
                valid = valid && !(containerName.Contains("--"));
                valid = valid && !(containerName.EndsWith("-"));
                if (valid)
                {
                    foreach (char c in containerName)
                    {   //every character is lowercase letter, digit, or -
                        if (char.IsLetter(c))
                            valid = valid && char.IsLower(c);
                        else if (!char.IsDigit(c))
                            valid = valid && c == '-';
                    }
                }
            }
            return valid;
        }
    }
}
