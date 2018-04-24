//System .dll's
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// This namespaces is the ARFE project.
/// </summary>
namespace ARFE
{
    /// <summary>
    /// This class contains all of the linq queries for the search and order functionality.
    /// </summary>
    public class SearchQueries
    {

        #region Name Filtering

        /// <summary>
        /// This function takes in a list of fileUIInfo and search string. It uses linq to find the files
        /// with the name containing that string, order them in alphebetical order and return it in a list.
        /// </summary>
        /// <param name="fileList">
        /// A list of FileUIInfo objects that need to be filtered and ordered
        /// </param>
        /// <param name="SearchCriteria">
        /// A string holding the search criteria
        /// </param>
        /// <returns>
        /// A filtered and ordered list of FIleUIInfo objects
        /// </returns>
        public static List<Common.FileUIInfo> FilterByFileNameOrderByNameAscending(List<Common.FileUIInfo> fileList, string SearchCriteria)
        {
            if (String.IsNullOrWhiteSpace(SearchCriteria))
            {
                return (from file in fileList
                        orderby file.FileName.ToLower() ascending
                        select file).ToList();
            }
            else
            {
                return (from file in fileList
                        where file.FileName.ToLower().Contains(SearchCriteria.ToLower())
                        orderby file.FileName.ToLower() ascending
                        select file).ToList();
            }
        }

        /// <summary>
        /// This function takes in a list of fileUIInfo and search string. It uses linq to find the files
        /// with the name containing that string, order them in reverse alphebetical order and return it in a list.
        /// </summary>
        /// <param name="fileList">
        /// A list of FileUIInfo objects that need to be filtered and ordered
        /// </param>
        /// <param name="SearchCriteria">
        /// A string holding the search criteria
        /// </param>
        /// <returns>
        /// A filtered and ordered list of FIleUIInfo objects
        /// </returns>
        public static List<Common.FileUIInfo> FilterByFileNameOrderByNameDescending(List<Common.FileUIInfo> fileList, string SearchCriteria)
        {
            if (String.IsNullOrWhiteSpace(SearchCriteria))
            {
                return (from file in fileList
                        orderby file.FileName.ToLower() descending
                        select file).ToList();
            }
            else
            {
                return (from file in fileList
                        where file.FileName.ToLower().Contains(SearchCriteria.ToLower())
                        orderby file.FileName.ToLower() descending
                        select file).ToList();
            }
        }

        /// <summary>
        /// This function takes in a list of fileUIInfo and search string. It uses linq to find the files
        /// with the name containing that string, order them by date ascending and return it in a list.
        /// </summary>
        /// <param name="fileList">
        /// A list of FileUIInfo objects that need to be filtered and ordered
        /// </param>
        /// <param name="SearchCriteria">
        /// A string holding the search criteria
        /// </param>
        /// <returns>
        /// A filtered and ordered list of FIleUIInfo objects
        /// </returns>
        public static List<Common.FileUIInfo> FilterByFileNameOrderByDateAscending(List<Common.FileUIInfo> fileList, string SearchCriteria)
        {
            if (String.IsNullOrWhiteSpace(SearchCriteria))
            {
                return (from file in fileList
                        orderby file.UploadDate ascending
                        select file).ToList();
            }
            else
            {
                return (from file in fileList
                        where file.FileName.ToLower().Contains(SearchCriteria.ToLower())
                        orderby file.UploadDate ascending
                        select file).ToList();
            }
        }

        /// <summary>
        /// This function takes in a list of fileUIInfo and search string. It uses linq to find the files
        /// with the name containing that string, order them by date in desending order and return it in a list.
        /// </summary>
        /// <param name="fileList">
        /// A list of FileUIInfo objects that need to be filtered and ordered
        /// </param>
        /// <param name="SearchCriteria">
        /// A string holding the search criteria
        /// </param>
        /// <returns>
        /// A filtered and ordered list of FIleUIInfo objects
        /// </returns>
        /// 
        public static List<Common.FileUIInfo> FilterByFileNameOrderByDateDescending(List<Common.FileUIInfo> fileList, string SearchCriteria)
        {
            if (String.IsNullOrWhiteSpace(SearchCriteria))
            {
                return (from file in fileList
                        orderby file.UploadDate descending
                        select file).ToList();
            }
            else
            {
                return (from file in fileList
                        where file.FileName.ToLower().Contains(SearchCriteria.ToLower())
                        orderby file.UploadDate descending
                        select file).ToList();
            }
        }
        #endregion

        #region Date Filtering

        /// <summary>
        /// This function takes in a list of fileUIInfo, a start and end date. It uses linq to find the files 
        /// between the start and end date, then order them in alphebetical order and return it in a list.
        /// </summary>
        /// <param name="fileList">
        /// A list of FileUIInfo objects that need to be filtered and ordered.
        /// </param>
        /// <param name="startDate">
        /// A datetime object holding the lower bound date.
        /// </param>
        /// <param name="endDate">
        /// A datetime object holding the upper bound date.
        /// </param>
        /// <returns>
        /// A filtered and ordered list of FIleUIInfo objects
        /// </returns>
        ///
        public static List<Common.FileUIInfo> FilterDateOrderByNameAscending(List<Common.FileUIInfo> fileList, DateTime startDate, DateTime endDate)
        {
            return (from file in fileList
                    where file.UploadDate >= startDate && file.UploadDate <= endDate
                    orderby file.FileName.ToLower() ascending
                    select file).ToList();
        }

        /// <summary>
        /// This function takes in a list of fileUIInfo, a start and end date. It uses linq to find the files 
        /// between the start and end date, then order them in reverse alphebetical order and return it in a list.
        /// </summary>
        /// <param name="fileList">
        /// A list of FileUIInfo objects that need to be filtered and ordered.
        /// </param>
        /// <param name="startDate">
        /// A datetime object holding the lower bound date.
        /// </param>
        /// <param name="endDate">
        /// A datetime object holding the upper bound date.
        /// </param>
        /// <returns>
        /// A filtered and ordered list of FIleUIInfo objects
        /// </returns>
        ///
        public static List<Common.FileUIInfo> FilterDateOrderByNameDescending(List<Common.FileUIInfo> fileList, DateTime startDate, DateTime endDate)
        {

            return (from file in fileList
                    where file.UploadDate >= startDate && file.UploadDate <= endDate
                    orderby file.FileName.ToLower() descending
                    select file).ToList();
        }

        /// <summary>
        /// This function takes in a list of fileUIInfo, a start and end date. It uses linq to find the files 
        /// between the start and end date, then order them oldest first and return it in a list.
        /// </summary>
        /// <param name="fileList">
        /// A list of FileUIInfo objects that need to be filtered and ordered.
        /// </param>
        /// <param name="startDate">
        /// A datetime object holding the lower bound date.
        /// </param>
        /// <param name="endDate">
        /// A datetime object holding the upper bound date.
        /// </param>
        /// <returns>
        /// A filtered and ordered list of FIleUIInfo objects
        /// </returns>
        ///
        public static List<Common.FileUIInfo> FilterDateOrderByDateAscending(List<Common.FileUIInfo> fileList, DateTime startDate, DateTime endDate)
        {

            return (from file in fileList
                    where file.UploadDate >= startDate && file.UploadDate <= endDate
                    orderby file.UploadDate ascending
                    select file).ToList();
        }

        /// <summary>
        /// This function takes in a list of fileUIInfo, a start and end date. It uses linq to find the files 
        /// between the start and end date, then order them by newest first and return it in a list.
        /// </summary>
        /// <param name="fileList">
        /// A list of FileUIInfo objects that need to be filtered and ordered.
        /// </param>
        /// <param name="startDate">
        /// A datetime object holding the lower bound date.
        /// </param>
        /// <param name="endDate">
        /// A datetime object holding the upper bound date.
        /// </param>
        /// <returns>
        /// A filtered and ordered list of FIleUIInfo objects
        /// </returns>
        ///
        public static List<Common.FileUIInfo> FilterDateOrderByDateDescending(List<Common.FileUIInfo> fileList, DateTime startDate, DateTime endDate)
        {

            return (from file in fileList
                    where file.UploadDate >= startDate && file.UploadDate <= endDate
                    orderby file.UploadDate descending
                    select file).ToList();
        }
        #endregion
    }
}