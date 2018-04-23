using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ARFE
{
    public class SearchQueries
    {

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


        public static List<Common.FileUIInfo> FilterDateOrderByNameAscending(List<Common.FileUIInfo> fileList, DateTime startDate, DateTime endDate)
        {
            return (from file in fileList
                    where file.UploadDate >= startDate && file.UploadDate <= endDate
                    orderby file.FileName.ToLower() ascending
                    select file).ToList();
        }

        public static List<Common.FileUIInfo> FilterDateOrderByNameDescending(List<Common.FileUIInfo> fileList, DateTime startDate, DateTime endDate)
        {

            return (from file in fileList
                    where file.UploadDate >= startDate && file.UploadDate <= endDate
                    orderby file.FileName.ToLower() descending
                    select file).ToList();
        }

        public static List<Common.FileUIInfo> FilterDateOrderByDateAscending(List<Common.FileUIInfo> fileList, DateTime startDate, DateTime endDate)
        {

            return (from file in fileList
                    where file.UploadDate >= startDate && file.UploadDate <= endDate
                    orderby file.UploadDate ascending
                    select file).ToList();
        }

        public static List<Common.FileUIInfo> FilterDateOrderByDateDescending(List<Common.FileUIInfo> fileList, DateTime startDate, DateTime endDate)
        {

            return (from file in fileList
                    where file.UploadDate >= startDate && file.UploadDate <= endDate
                    orderby file.UploadDate descending
                    select file).ToList();
        }

    }
}