using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

/// <summary>
/// The Main namespace containing unit testing code for the ARFE Web project
/// </summary>
namespace ARFE.Tests
{
    /// <summary>
    /// Unit tests covering the search functionality of the SearchQueries.cs file in ARFE.csproj
    /// 
    /// </summary>
    [TestClass]
    class SearchQueriesTest
    {

        #region Text Search Tests

        /// <summary>
        /// Test that the search by text and order alphebetically returns the proper amount of FileUiInfo objects
        /// </summary>
        [TestMethod]
        public void TextSearch_Alphebetical_Count_Test()
        {
            List<Common.FileUIInfo> UnorderedList = new List<FileUIInfo>();
            UnorderedList.Add(new Common.FileUIInfo("atest", "sdsmt", "testFle", DateTime.Now));
            UnorderedList.Add(new Common.FileUIInfo("ztest", "sdsmt", "testFle", DateTime.Now));
            UnorderedList.Add(new Common.FileUIInfo("xtest", "sdsmt", "testFle", DateTime.Now));
            UnorderedList.Add(new Common.FileUIInfo("aexample", "sdsmt", "testFle", DateTime.Now));
            UnorderedList.Add(new Common.FileUIInfo("bexample", "sdsmt", "testFle", DateTime.Now));

            List<Common.FileUIInfo> orderedList = SearchQueries.FilterByFileNameOrderByNameAscending(UnorderedList, "test");

            Assert.AreEqual(orderedList.Count, 3);
        }

        /// <summary>
        /// Test that the search by text and order alphebetically returns the correct FileUIInfo Objects
        /// </summary>
        [TestMethod]
        public void TextSearch_Alphebetical_Filtered_Test()
        {
            List<Common.FileUIInfo> UnorderedList = new List<FileUIInfo>();
            UnorderedList.Add(new Common.FileUIInfo("atest", "sdsmt", "testFle", DateTime.Now));
            UnorderedList.Add(new Common.FileUIInfo("ztest", "sdsmt", "testFle", DateTime.Now));
            UnorderedList.Add(new Common.FileUIInfo("xtest", "sdsmt", "testFle", DateTime.Now));
            UnorderedList.Add(new Common.FileUIInfo("aexample", "sdsmt", "testFle", DateTime.Now));
            UnorderedList.Add(new Common.FileUIInfo("bexample", "sdsmt", "testFle", DateTime.Now));

            List<Common.FileUIInfo> orderedList = SearchQueries.FilterByFileNameOrderByNameAscending(UnorderedList, "test");

            bool filtered = true;

            foreach(Common.FileUIInfo x in orderedList)
            {
                if(!x.FileName.ToLower().Contains("test"))
                {
                    filtered = false;
                }
            }

            Assert.IsTrue(filtered);
        }

        /// <summary>
        /// Test that the search by text and order alphebetically returns the FileUIInfoObjects in proper order
        /// </summary>
        [TestMethod]
        public void TextSearch_Alphebetical_Ordered_Test()
        {
            List<Common.FileUIInfo> UnorderedList = new List<FileUIInfo>();
            UnorderedList.Add(new Common.FileUIInfo("atest", "sdsmt", "testFle", DateTime.Now));
            UnorderedList.Add(new Common.FileUIInfo("ztest", "sdsmt", "testFle", DateTime.Now));
            UnorderedList.Add(new Common.FileUIInfo("xtest", "sdsmt", "testFle", DateTime.Now));
            UnorderedList.Add(new Common.FileUIInfo("aexample", "sdsmt", "testFle", DateTime.Now));
            UnorderedList.Add(new Common.FileUIInfo("bexample", "sdsmt", "testFle", DateTime.Now));

            List<Common.FileUIInfo> orderedList = SearchQueries.FilterByFileNameOrderByNameAscending(UnorderedList, "test");

            bool ordered = true;

            for(int i = 1; i < orderedList.Count; i++)
            {
                if(String.Compare(orderedList[i-1].FileName.ToLower(),orderedList[i].FileName.ToLower()) > 0 )
                {
                    ordered = false;
                }
            }

            Assert.IsTrue(ordered);
        }

        /// <summary>
        /// Test that the search by text and order reverse alphebetically returns the proper amount of FileUiInfo objects
        /// </summary>
        [TestMethod]
        public void TextSearch_ReverseAlphebetical_Count_Test()
        {
            List<Common.FileUIInfo> UnorderedList = new List<FileUIInfo>();
            UnorderedList.Add(new Common.FileUIInfo("atest", "sdsmt", "testFle", DateTime.Now));
            UnorderedList.Add(new Common.FileUIInfo("ztest", "sdsmt", "testFle", DateTime.Now));
            UnorderedList.Add(new Common.FileUIInfo("xtest", "sdsmt", "testFle", DateTime.Now));
            UnorderedList.Add(new Common.FileUIInfo("aexample", "sdsmt", "testFle", DateTime.Now));
            UnorderedList.Add(new Common.FileUIInfo("bexample", "sdsmt", "testFle", DateTime.Now));

            List<Common.FileUIInfo> orderedList = SearchQueries.FilterByFileNameOrderByNameDescending(UnorderedList, "test");

            Assert.AreEqual(orderedList.Count, 3);
        }

        /// <summary>
        /// Test that the search by text and order reverse alphebetically returns the correct FileUiInfo objects
        /// </summary>
        [TestMethod]
        public void TextSearch_ReverseAlphebetical_Filtered_Test()
        {
            List<Common.FileUIInfo> UnorderedList = new List<FileUIInfo>();
            UnorderedList.Add(new Common.FileUIInfo("atest", "sdsmt", "testFle", DateTime.Now));
            UnorderedList.Add(new Common.FileUIInfo("ztest", "sdsmt", "testFle", DateTime.Now));
            UnorderedList.Add(new Common.FileUIInfo("xtest", "sdsmt", "testFle", DateTime.Now));
            UnorderedList.Add(new Common.FileUIInfo("aexample", "sdsmt", "testFle", DateTime.Now));
            UnorderedList.Add(new Common.FileUIInfo("bexample", "sdsmt", "testFle", DateTime.Now));

            List<Common.FileUIInfo> orderedList = SearchQueries.FilterByFileNameOrderByNameDescending(UnorderedList, "test");

            bool filtered = true;

            foreach (Common.FileUIInfo x in orderedList)
            {
                if (!x.FileName.ToLower().Contains("test"))
                {
                    filtered = false;
                }
            }

            Assert.IsTrue(filtered);
        }

        /// <summary>
        /// Test that the search by text and order reverse alphebetically returns the objects in proper order
        /// </summary>
        [TestMethod]
        public void TextSearch_ReverseAlphebetical_Ordered_Test()
        {
            List<Common.FileUIInfo> UnorderedList = new List<FileUIInfo>();
            UnorderedList.Add(new Common.FileUIInfo("atest", "sdsmt", "testFle", DateTime.Now));
            UnorderedList.Add(new Common.FileUIInfo("ztest", "sdsmt", "testFle", DateTime.Now));
            UnorderedList.Add(new Common.FileUIInfo("xtest", "sdsmt", "testFle", DateTime.Now));
            UnorderedList.Add(new Common.FileUIInfo("aexample", "sdsmt", "testFle", DateTime.Now));
            UnorderedList.Add(new Common.FileUIInfo("bexample", "sdsmt", "testFle", DateTime.Now));

            List<Common.FileUIInfo> orderedList = SearchQueries.FilterByFileNameOrderByNameDescending(UnorderedList, "test");

            bool ordered = true;

            for (int i = 1; i < orderedList.Count; i++)
            {
                if (String.Compare(orderedList[i - 1].FileName.ToLower(), orderedList[i].FileName.ToLower()) < 0)
                {
                    ordered = false;
                }
            }

            Assert.IsTrue(ordered);
        }


        /// <summary>
        /// Test that the search by text and order newest first returns the proper amount of FileUiInfo objects
        /// </summary>
        [TestMethod]
        public void TextSearch_Newest_Count_Test()
        {
            List<Common.FileUIInfo> UnorderedList = new List<FileUIInfo>();
            UnorderedList.Add(new Common.FileUIInfo("atest", "sdsmt", "testFle", new DateTime(2018,1,1)));
            UnorderedList.Add(new Common.FileUIInfo("ztest", "sdsmt", "testFle", new DateTime(2017,1,1)));
            UnorderedList.Add(new Common.FileUIInfo("xtest", "sdsmt", "testFle", new DateTime(2018,2,1)));
            UnorderedList.Add(new Common.FileUIInfo("aexample", "sdsmt", "testFle", new DateTime(2018,2,2)));
            UnorderedList.Add(new Common.FileUIInfo("bexample", "sdsmt", "testFle", new DateTime(2017,2,1)));

            List<Common.FileUIInfo> orderedList = SearchQueries.FilterByFileNameOrderByDateDescending(UnorderedList, "test");

            Assert.AreEqual(orderedList.Count, 3);
        }

        /// <summary>
        /// Test that the search by text and order newest first returns the correct FileUiInfo objects
        /// </summary>
        [TestMethod]
        public void TextSearch_Newest_Filtered_Test()
        {
            List<Common.FileUIInfo> UnorderedList = new List<FileUIInfo>();
            UnorderedList.Add(new Common.FileUIInfo("atest", "sdsmt", "testFle", new DateTime(2018, 1, 1)));
            UnorderedList.Add(new Common.FileUIInfo("ztest", "sdsmt", "testFle", new DateTime(2017, 1, 1)));
            UnorderedList.Add(new Common.FileUIInfo("xtest", "sdsmt", "testFle", new DateTime(2018, 2, 1)));
            UnorderedList.Add(new Common.FileUIInfo("aexample", "sdsmt", "testFle", new DateTime(2018, 2, 2)));
            UnorderedList.Add(new Common.FileUIInfo("bexample", "sdsmt", "testFle", new DateTime(2017, 2, 1)));

            List<Common.FileUIInfo> orderedList = SearchQueries.FilterByFileNameOrderByDateDescending(UnorderedList, "test");

            bool filtered = true;

            foreach (Common.FileUIInfo x in orderedList)
            {
                if (!x.FileName.ToLower().Contains("test"))
                {
                    filtered = false;
                }
            }

            Assert.IsTrue(filtered);
        }

        /// <summary>
        /// Test that the search by text and order newest first returns the objects in the proper order.
        /// </summary>
        [TestMethod]
        public void TextSearch_Newest_Ordered_Test()
        {
            List<Common.FileUIInfo> UnorderedList = new List<FileUIInfo>();
            UnorderedList.Add(new Common.FileUIInfo("atest", "sdsmt", "testFle", new DateTime(2018, 1, 1)));
            UnorderedList.Add(new Common.FileUIInfo("ztest", "sdsmt", "testFle", new DateTime(2017, 1, 1)));
            UnorderedList.Add(new Common.FileUIInfo("xtest", "sdsmt", "testFle", new DateTime(2018, 2, 1)));
            UnorderedList.Add(new Common.FileUIInfo("aexample", "sdsmt", "testFle", new DateTime(2018, 2, 2)));
            UnorderedList.Add(new Common.FileUIInfo("bexample", "sdsmt", "testFle", new DateTime(2017, 2, 1)));

            List<Common.FileUIInfo> orderedList = SearchQueries.FilterByFileNameOrderByDateDescending(UnorderedList, "test");

            bool ordered = true;

            for (int i = 1; i < orderedList.Count; i++)
            {
                if (orderedList[i-1].UploadDate < orderedList[i].UploadDate)
                {
                    ordered = false;
                }
            }

            Assert.IsTrue(ordered);
        }


        /// <summary>
        /// Test that the search by text and order oldest first returns the proper amount of FileUiInfo objects
        /// </summary>
        [TestMethod]
        public void TextSearch_Oldest_Count_Test()
        {
            List<Common.FileUIInfo> UnorderedList = new List<FileUIInfo>();
            UnorderedList.Add(new Common.FileUIInfo("atest", "sdsmt", "testFle", new DateTime(2018, 1, 1)));
            UnorderedList.Add(new Common.FileUIInfo("ztest", "sdsmt", "testFle", new DateTime(2017, 1, 1)));
            UnorderedList.Add(new Common.FileUIInfo("xtest", "sdsmt", "testFle", new DateTime(2018, 2, 1)));
            UnorderedList.Add(new Common.FileUIInfo("aexample", "sdsmt", "testFle", new DateTime(2018, 2, 2)));
            UnorderedList.Add(new Common.FileUIInfo("bexample", "sdsmt", "testFle", new DateTime(2017, 2, 1)));

            List<Common.FileUIInfo> orderedList = SearchQueries.FilterByFileNameOrderByDateAscending(UnorderedList, "test");

            Assert.AreEqual(orderedList.Count, 3);
        }

        /// <summary>
        /// Test that the search by text and order oldest first returns the correct FileUiInfo objects
        /// </summary>
        [TestMethod]
        public void TextSearch_Oldest_Filtered_Test()
        {
            List<Common.FileUIInfo> UnorderedList = new List<FileUIInfo>();
            UnorderedList.Add(new Common.FileUIInfo("atest", "sdsmt", "testFle", new DateTime(2018, 1, 1)));
            UnorderedList.Add(new Common.FileUIInfo("ztest", "sdsmt", "testFle", new DateTime(2017, 1, 1)));
            UnorderedList.Add(new Common.FileUIInfo("xtest", "sdsmt", "testFle", new DateTime(2018, 2, 1)));
            UnorderedList.Add(new Common.FileUIInfo("aexample", "sdsmt", "testFle", new DateTime(2018, 2, 2)));
            UnorderedList.Add(new Common.FileUIInfo("bexample", "sdsmt", "testFle", new DateTime(2017, 2, 1)));

            List<Common.FileUIInfo> orderedList = SearchQueries.FilterByFileNameOrderByDateAscending(UnorderedList, "test");

            bool filtered = true;

            foreach (Common.FileUIInfo x in orderedList)
            {
                if (!x.FileName.ToLower().Contains("test"))
                {
                    filtered = false;
                }
            }

            Assert.IsTrue(filtered);
        }

        /// <summary>
        /// Test that the search by text and order oldest first returns the objects in the proper order.
        /// </summary>
        [TestMethod]
        public void TextSearch_Oldest_Ordered_Test()
        {
            List<Common.FileUIInfo> UnorderedList = new List<FileUIInfo>();
            UnorderedList.Add(new Common.FileUIInfo("atest", "sdsmt", "testFle", new DateTime(2018, 1, 1)));
            UnorderedList.Add(new Common.FileUIInfo("ztest", "sdsmt", "testFle", new DateTime(2017, 1, 1)));
            UnorderedList.Add(new Common.FileUIInfo("xtest", "sdsmt", "testFle", new DateTime(2018, 2, 1)));
            UnorderedList.Add(new Common.FileUIInfo("aexample", "sdsmt", "testFle", new DateTime(2018, 2, 2)));
            UnorderedList.Add(new Common.FileUIInfo("bexample", "sdsmt", "testFle", new DateTime(2017, 2, 1)));

            List<Common.FileUIInfo> orderedList = SearchQueries.FilterByFileNameOrderByDateAscending(UnorderedList, "test");

            bool ordered = true;

            for (int i = 1; i < orderedList.Count; i++)
            {
                if (orderedList[i - 1].UploadDate > orderedList[i].UploadDate)
                {
                    ordered = false;
                }
            }

            Assert.IsTrue(ordered);
        }

        #endregion

        #region Date Search Tests

        /// <summary>
        /// Test that the search by Date and order alphebetically returns the proper amount of FileUiInfo objects
        /// </summary>
        [TestMethod]
        public void DateSearch_Alphebetical_Count_Test()
        {
            DateTime LowerDate = new DateTime(2018, 1, 1);
            DateTime UpperDate = new DateTime(2018, 2, 1);

            List<Common.FileUIInfo> UnorderedList = new List<FileUIInfo>();
            UnorderedList.Add(new Common.FileUIInfo("atest", "sdsmt", "testFle", new DateTime(2018, 1, 1)));
            UnorderedList.Add(new Common.FileUIInfo("ztest", "sdsmt", "testFle", new DateTime(2017, 1, 1)));
            UnorderedList.Add(new Common.FileUIInfo("xtest", "sdsmt", "testFle", new DateTime(2018, 2, 1)));
            UnorderedList.Add(new Common.FileUIInfo("aexample", "sdsmt", "testFle", new DateTime(2018, 2, 2)));
            UnorderedList.Add(new Common.FileUIInfo("bexample", "sdsmt", "testFle", new DateTime(2017, 2, 1)));
            
            List<Common.FileUIInfo> orderedList = SearchQueries.FilterDateOrderByNameAscending(UnorderedList, LowerDate, UpperDate);

            Assert.AreEqual(orderedList.Count, 2);
        }

        /// <summary>
        /// Test that the search by Date and order alphebetically returns the correct FileUiInfo objects
        /// </summary>
        [TestMethod]
        public void DateSearch_Alphebetical_Filtered_Test()
        {
            DateTime LowerDate = new DateTime(2018, 1, 1);
            DateTime UpperDate = new DateTime(2018, 2, 1);
            List<Common.FileUIInfo> UnorderedList = new List<FileUIInfo>();
            UnorderedList.Add(new Common.FileUIInfo("atest", "sdsmt", "testFle", new DateTime(2018, 1, 1)));
            UnorderedList.Add(new Common.FileUIInfo("ztest", "sdsmt", "testFle", new DateTime(2017, 1, 1)));
            UnorderedList.Add(new Common.FileUIInfo("xtest", "sdsmt", "testFle", new DateTime(2018, 2, 1)));
            UnorderedList.Add(new Common.FileUIInfo("aexample", "sdsmt", "testFle", new DateTime(2018, 2, 2)));
            UnorderedList.Add(new Common.FileUIInfo("bexample", "sdsmt", "testFle", new DateTime(2017, 2, 1)));

            List<Common.FileUIInfo> orderedList = SearchQueries.FilterDateOrderByNameAscending(UnorderedList, LowerDate, UpperDate);

            bool filtered = true;

            foreach (Common.FileUIInfo x in orderedList)
            {
                if (x.UploadDate < LowerDate || x.UploadDate > UpperDate )
                {
                    filtered = false;
                }
            }

            Assert.IsTrue(filtered);
        }

        /// <summary>
        /// Test that the search by Date and order alphebetically returns the objects in the correct order.
        /// </summary>
        [TestMethod]
        public void DateSearch_Alphebetical_Ordered_Test()
        {
            DateTime LowerDate = new DateTime(2018, 1, 1);
            DateTime UpperDate = new DateTime(2018, 2, 1);
            List<Common.FileUIInfo> UnorderedList = new List<FileUIInfo>();
            UnorderedList.Add(new Common.FileUIInfo("atest", "sdsmt", "testFle", new DateTime(2018, 1, 1)));
            UnorderedList.Add(new Common.FileUIInfo("ztest", "sdsmt", "testFle", new DateTime(2017, 1, 1)));
            UnorderedList.Add(new Common.FileUIInfo("xtest", "sdsmt", "testFle", new DateTime(2018, 2, 1)));
            UnorderedList.Add(new Common.FileUIInfo("aexample", "sdsmt", "testFle", new DateTime(2018, 2, 2)));
            UnorderedList.Add(new Common.FileUIInfo("bexample", "sdsmt", "testFle", new DateTime(2017, 2, 1)));

            List<Common.FileUIInfo> orderedList = SearchQueries.FilterDateOrderByNameAscending(UnorderedList, LowerDate, UpperDate);

            bool ordered = true;

            for (int i = 1; i < orderedList.Count; i++)
            {
                if (String.Compare(orderedList[i - 1].FileName.ToLower(), orderedList[i].FileName.ToLower()) > 0)
                {
                    ordered = false;
                }
            }

            Assert.IsTrue(ordered);
        }

        /// <summary>
        /// Test that the search by Date and order reverse alphebetically returns the proper amount of FileUiInfo objects
        /// </summary>
        [TestMethod]
        public void DateSearch_ReverseAlphebetical_Count_Test()
        {
            DateTime LowerDate = new DateTime(2018, 1, 1);
            DateTime UpperDate = new DateTime(2018, 2, 1);
            List<Common.FileUIInfo> UnorderedList = new List<FileUIInfo>();
            UnorderedList.Add(new Common.FileUIInfo("atest", "sdsmt", "testFle", new DateTime(2018, 1, 1)));
            UnorderedList.Add(new Common.FileUIInfo("ztest", "sdsmt", "testFle", new DateTime(2017, 1, 1)));
            UnorderedList.Add(new Common.FileUIInfo("xtest", "sdsmt", "testFle", new DateTime(2018, 2, 1)));
            UnorderedList.Add(new Common.FileUIInfo("aexample", "sdsmt", "testFle", new DateTime(2018, 2, 2)));
            UnorderedList.Add(new Common.FileUIInfo("bexample", "sdsmt", "testFle", new DateTime(2017, 2, 1)));

            List<Common.FileUIInfo> orderedList = SearchQueries.FilterDateOrderByNameDescending(UnorderedList, LowerDate, UpperDate);

            Assert.AreEqual(orderedList.Count, 2);
        }

        /// <summary>
        /// Test that the search by Date and order reverse alphebetically returns the correct FileUiInfo objects
        /// </summary>
        [TestMethod]
        public void DateSearch_ReverseAlphebetical_Filtered_Test()
        {
            DateTime LowerDate = new DateTime(2018, 1, 1);
            DateTime UpperDate = new DateTime(2018, 2, 1);
            List<Common.FileUIInfo> UnorderedList = new List<FileUIInfo>();
            UnorderedList.Add(new Common.FileUIInfo("atest", "sdsmt", "testFle", new DateTime(2018, 1, 1)));
            UnorderedList.Add(new Common.FileUIInfo("ztest", "sdsmt", "testFle", new DateTime(2017, 1, 1)));
            UnorderedList.Add(new Common.FileUIInfo("xtest", "sdsmt", "testFle", new DateTime(2018, 2, 1)));
            UnorderedList.Add(new Common.FileUIInfo("aexample", "sdsmt", "testFle", new DateTime(2018, 2, 2)));
            UnorderedList.Add(new Common.FileUIInfo("bexample", "sdsmt", "testFle", new DateTime(2017, 2, 1)));

            
            List<Common.FileUIInfo> orderedList = SearchQueries.FilterDateOrderByNameDescending(UnorderedList,LowerDate, UpperDate);

            bool filtered = true;

            foreach (Common.FileUIInfo x in orderedList)
            {
                if (x.UploadDate < LowerDate || x.UploadDate > UpperDate)
                {
                    filtered = false;
                }
            }

            Assert.IsTrue(filtered);
        }

        /// <summary>
        /// Test that the search by Date and order reverse alphebetically returns the objects in the correct order.
        /// </summary>
        [TestMethod]
        public void DateSearch_ReverseAlphebetical_Ordered_Test()
        {
            DateTime LowerDate = new DateTime(2018, 1, 1);
            DateTime UpperDate = new DateTime(2018, 2, 1);
            List<Common.FileUIInfo> UnorderedList = new List<FileUIInfo>();
            UnorderedList.Add(new Common.FileUIInfo("atest", "sdsmt", "testFle", new DateTime(2018, 1, 1)));
            UnorderedList.Add(new Common.FileUIInfo("ztest", "sdsmt", "testFle", new DateTime(2017, 1, 1)));
            UnorderedList.Add(new Common.FileUIInfo("xtest", "sdsmt", "testFle", new DateTime(2018, 2, 1)));
            UnorderedList.Add(new Common.FileUIInfo("aexample", "sdsmt", "testFle", new DateTime(2018, 2, 2)));
            UnorderedList.Add(new Common.FileUIInfo("bexample", "sdsmt", "testFle", new DateTime(2017, 2, 1)));

            List<Common.FileUIInfo> orderedList = SearchQueries.FilterDateOrderByNameDescending(UnorderedList, LowerDate, UpperDate);

            bool ordered = true;

            for (int i = 1; i < orderedList.Count; i++)
            {
                if (String.Compare(orderedList[i - 1].FileName.ToLower(), orderedList[i].FileName.ToLower()) < 0)
                {
                    ordered = false;
                }
            }

            Assert.IsTrue(ordered);
        }

        /// <summary>
        /// Test that the search by Date and order by newest returns the proper amount of FileUiInfo objects
        /// </summary>
        [TestMethod]
        public void DateSearch_Newest_Count_Test()
        {
            List<Common.FileUIInfo> UnorderedList = new List<FileUIInfo>();
            UnorderedList.Add(new Common.FileUIInfo("atest", "sdsmt", "testFle", new DateTime(2018, 1, 1)));
            UnorderedList.Add(new Common.FileUIInfo("ztest", "sdsmt", "testFle", new DateTime(2017, 1, 1)));
            UnorderedList.Add(new Common.FileUIInfo("xtest", "sdsmt", "testFle", new DateTime(2018, 2, 1)));
            UnorderedList.Add(new Common.FileUIInfo("aexample", "sdsmt", "testFle", new DateTime(2018, 2, 2)));
            UnorderedList.Add(new Common.FileUIInfo("bexample", "sdsmt", "testFle", new DateTime(2017, 2, 1)));

            List<Common.FileUIInfo> orderedList = SearchQueries.FilterDateOrderByDateDescending(UnorderedList, new DateTime(2018, 1, 1), new DateTime(2018, 2, 1));

            Assert.AreEqual(orderedList.Count, 2);
        }

        /// <summary>
        /// Test that the search by Date and order by newest returns the correct proper amount of FileUiInfo objects
        /// </summary>
        [TestMethod]
        public void DateSearch_Newest_Filtered_Test()
        {
            DateTime LowerDate = new DateTime(2018, 1, 1);
            DateTime UpperDate = new DateTime(2018, 2, 1);
            List<Common.FileUIInfo> UnorderedList = new List<FileUIInfo>();
            UnorderedList.Add(new Common.FileUIInfo("atest", "sdsmt", "testFle", new DateTime(2018, 1, 1)));
            UnorderedList.Add(new Common.FileUIInfo("ztest", "sdsmt", "testFle", new DateTime(2017, 1, 1)));
            UnorderedList.Add(new Common.FileUIInfo("xtest", "sdsmt", "testFle", new DateTime(2018, 2, 1)));
            UnorderedList.Add(new Common.FileUIInfo("aexample", "sdsmt", "testFle", new DateTime(2018, 2, 2)));
            UnorderedList.Add(new Common.FileUIInfo("bexample", "sdsmt", "testFle", new DateTime(2017, 2, 1)));

            List<Common.FileUIInfo> orderedList = SearchQueries.FilterDateOrderByDateDescending(UnorderedList, LowerDate, UpperDate);

            bool filtered = true;

            foreach (Common.FileUIInfo x in orderedList)
            {
                if (x.UploadDate < LowerDate || x.UploadDate > UpperDate)
                {
                    filtered = false;
                }
            }

            Assert.IsTrue(filtered);
        }

        /// <summary>
        /// Test that the search by Date and order by newest returns the objects in the correct order
        /// </summary>
        [TestMethod]
        public void DateSearch_Newest_Ordered_Test()
        {
            DateTime LowerDate = new DateTime(2018, 1, 1);
            DateTime UpperDate = new DateTime(2018, 2, 1);
            List<Common.FileUIInfo> UnorderedList = new List<FileUIInfo>();
            UnorderedList.Add(new Common.FileUIInfo("atest", "sdsmt", "testFle", new DateTime(2018, 1, 1)));
            UnorderedList.Add(new Common.FileUIInfo("ztest", "sdsmt", "testFle", new DateTime(2017, 1, 1)));
            UnorderedList.Add(new Common.FileUIInfo("xtest", "sdsmt", "testFle", new DateTime(2018, 2, 1)));
            UnorderedList.Add(new Common.FileUIInfo("aexample", "sdsmt", "testFle", new DateTime(2018, 2, 2)));
            UnorderedList.Add(new Common.FileUIInfo("bexample", "sdsmt", "testFle", new DateTime(2017, 2, 1)));

            List<Common.FileUIInfo> orderedList = SearchQueries.FilterDateOrderByDateDescending(UnorderedList, LowerDate, UpperDate);

            bool ordered = true;

            for (int i = 1; i < orderedList.Count; i++)
            {
                if (orderedList[i - 1].UploadDate < orderedList[i].UploadDate)
                {
                    ordered = false;
                }
            }

            Assert.IsTrue(ordered);
        }

        /// <summary>
        /// Test that the search by Date and order by oldest returns the proper amount of FileUiInfo objects
        /// </summary>
        [TestMethod]
        public void DateSearch_Oldest_Count_Test()
        {
            DateTime LowerDate = new DateTime(2018, 1, 1);
            DateTime UpperDate = new DateTime(2018, 2, 1);
            List<Common.FileUIInfo> UnorderedList = new List<FileUIInfo>();
            UnorderedList.Add(new Common.FileUIInfo("atest", "sdsmt", "testFle", new DateTime(2018, 1, 1)));
            UnorderedList.Add(new Common.FileUIInfo("ztest", "sdsmt", "testFle", new DateTime(2017, 1, 1)));
            UnorderedList.Add(new Common.FileUIInfo("xtest", "sdsmt", "testFle", new DateTime(2018, 2, 1)));
            UnorderedList.Add(new Common.FileUIInfo("aexample", "sdsmt", "testFle", new DateTime(2018, 2, 2)));
            UnorderedList.Add(new Common.FileUIInfo("bexample", "sdsmt", "testFle", new DateTime(2017, 2, 1)));

            List<Common.FileUIInfo> orderedList = SearchQueries.FilterDateOrderByDateAscending(UnorderedList, LowerDate, UpperDate);

            Assert.AreEqual(orderedList.Count, 3);
        }

        /// <summary>
        /// Test that the search by Date and order by oldest returns the correct objects
        /// </summary>
        [TestMethod]
        public void DateSearch_Oldest_Filtered_Test()
        {
            DateTime LowerDate = new DateTime(2018, 1, 1);
            DateTime UpperDate = new DateTime(2018, 2, 1);
            List<Common.FileUIInfo> UnorderedList = new List<FileUIInfo>();
            UnorderedList.Add(new Common.FileUIInfo("atest", "sdsmt", "testFle", new DateTime(2018, 1, 1)));
            UnorderedList.Add(new Common.FileUIInfo("ztest", "sdsmt", "testFle", new DateTime(2017, 1, 1)));
            UnorderedList.Add(new Common.FileUIInfo("xtest", "sdsmt", "testFle", new DateTime(2018, 2, 1)));
            UnorderedList.Add(new Common.FileUIInfo("aexample", "sdsmt", "testFle", new DateTime(2018, 2, 2)));
            UnorderedList.Add(new Common.FileUIInfo("bexample", "sdsmt", "testFle", new DateTime(2017, 2, 1)));

            List<Common.FileUIInfo> orderedList = SearchQueries.FilterDateOrderByDateAscending(UnorderedList, LowerDate, UpperDate);

            bool filtered = true;

            foreach (Common.FileUIInfo x in orderedList)
            {
                if (x.UploadDate < LowerDate || x.UploadDate > UpperDate)
                {
                    filtered = false;
                }
            }

            Assert.IsTrue(filtered);
        }

        /// <summary>
        /// Test that the search by Date and order by oldest returns the objects in the correct order
        /// </summary>
        [TestMethod]
        public void DateSearch_Oldest_Ordered_Test()
        {
            DateTime LowerDate = new DateTime(2018, 1, 1);
            DateTime UpperDate = new DateTime(2018, 2, 1);
            List<Common.FileUIInfo> UnorderedList = new List<FileUIInfo>();
            UnorderedList.Add(new Common.FileUIInfo("atest", "sdsmt", "testFle", new DateTime(2018, 1, 1)));
            UnorderedList.Add(new Common.FileUIInfo("ztest", "sdsmt", "testFle", new DateTime(2017, 1, 1)));
            UnorderedList.Add(new Common.FileUIInfo("xtest", "sdsmt", "testFle", new DateTime(2018, 2, 1)));
            UnorderedList.Add(new Common.FileUIInfo("aexample", "sdsmt", "testFle", new DateTime(2018, 2, 2)));
            UnorderedList.Add(new Common.FileUIInfo("bexample", "sdsmt", "testFle", new DateTime(2017, 2, 1)));

            List<Common.FileUIInfo> orderedList = SearchQueries.FilterDateOrderByDateAscending(UnorderedList, LowerDate, UpperDate);

            bool ordered = true;

            for (int i = 1; i < orderedList.Count; i++)
            {
                if (orderedList[i - 1].UploadDate > orderedList[i].UploadDate)
                {
                    ordered = false;
                }
            }

            Assert.IsTrue(ordered);
        }
        #endregion
    }
}
