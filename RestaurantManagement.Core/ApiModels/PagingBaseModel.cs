using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestaurantManagement.Core.Enums;

namespace RestaurantManagement.Core.ApiModels
{
    public class PagingBaseModel
    {
        [DefaultValue(1)]
        public int PageIndex { get; set; } = 1;
        [DefaultValue(10)]
        public int PageSize { get; set; } = 10;
        public IList<FilterColumn> FilterColumns { get; set; }
        public IDictionary<string, string> SortColumnsDictionary { get; set; }
        public IList<FilterRangeColumn> FilterRangeColumns { get; set; }

        //[Required]
        public FilterOptionEnum FilterOption { get; set; }
        public ExportRequestBaseModel Export { get; set; }
    }

    public class FilterColumn
    {
        public IList<string> SearchColumns { get; set; }
        public IList<string> SearchTerms { get; set; }
        [DefaultValue("5")]
        public SearchOperatorEnum Operator { get; set; }
    }

    public class FilterRangeColumn
    {
        public string SearchColumns { get; set; }
        public string SearchTerms { get; set; }
        [DefaultValue("4")]
        public SearchOperatorEnum Operator { get; set; }
    }

    public class ExportRequestBaseModel
    {
        //[Required]
        public required Dictionary<string, string> ChosenColumnNameList { get; set; }

        //[Required]
        public string PageName { get; set; }
    }
}
