namespace RestaurantManagement.Service.Implementation
{
    public class BaseService
    {
        protected AppSettings _appSettings { get; set; }
        protected IMapper _mapper { get; set; }
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BaseService(AppSettings appSettings, IMapper mapper, IHttpContextAccessor httpContextAccessor, RestaurantDBContext context)
        {
            _appSettings = appSettings;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public IEnumerable<T> AdvancedFilter<T>(IEnumerable<T> data, PagingBaseModel query, string defaultSortColumn)
        {
            data = ApplyAdvancedQuery(data, query, defaultSortColumn);
            return data;
        }

        public IEnumerable<T> ApplyAdvancedQuery<T>(IEnumerable<T> data, PagingBaseModel query,
            string defaultSortColumn)
        {
            // Apply search term filtering for each FilterColumn
            if (query.FilterColumns != null && query.FilterColumns.Any())
            {
                foreach (var filter in query.FilterColumns)
                {
                    data = ApplySearch(data, filter.SearchTerms, filter.SearchColumns, filter.Operator);
                }
            }

            // Apply range filtering for each FilterRangeColumn
            if (query.FilterRangeColumns != null && query.FilterRangeColumns.Any())
            {
                foreach (var rangeFilter in query.FilterRangeColumns)
                {
                    data = ApplyRangeFilter(data, rangeFilter.SearchTerms, rangeFilter.SearchColumns,
                        rangeFilter.Operator);
                }
            }

            // Apply sorting (if provided)
            if (query.SortColumnsDictionary != null && query.SortColumnsDictionary.Any())
            {
                data = ApplySort(data, query.SortColumnsDictionary);
            }
            else
            {
                data = DefaultSort(data, defaultSortColumn);
            }

            return data;
        }

        public static IEnumerable<T> ApplySearch<T>(IEnumerable<T> list, IList<string> searchTerms,
            IList<string> searchColumns, SearchOperatorEnum @operatorType)
        {
            var stringColumnList = typeof(T).GetProperties().Select(p => p.Name).ToList();

            searchColumns ??= new List<string>();

            foreach (var column in searchColumns)
            {
                if (!stringColumnList.Contains(column, StringComparer.OrdinalIgnoreCase))
                {
                    throw new ErrorException(StatusCodeEnum.BadRequest);
                }
            }

            if (searchTerms == null || !searchTerms.Any() || searchColumns == null || !searchColumns.Any())
                return list;

            return list.Where(item =>
            {
                foreach (var column in searchColumns)
                {
                    var property = typeof(T).GetProperties().FirstOrDefault(x =>
                        string.Equals(x.Name, column, StringComparison.OrdinalIgnoreCase));
                    if (property == null) continue;

                    var propertyValue = property.GetValue(item);


                    if (propertyValue == null)
                    {
                        if (searchTerms.Contains("null", StringComparer.OrdinalIgnoreCase))
                            return true;
                        continue;
                    }
                    var propertyString = propertyValue.ToString().ToLower();
                    switch (operatorType)
                    {
                        case SearchOperatorEnum.Equals:
                            if (searchTerms.Any(term => propertyString.Equals(term.ToLower())))
                                return true;
                            break;
                        case SearchOperatorEnum.Contains:
                            if (searchTerms.Any(term => propertyString.Contains(term.ToLower())))
                                return true;
                            break;
                        default:
                            continue;
                    }
                }

                return false;
            });
        }

        public static IEnumerable<T> ApplyRangeFilter<T>(IEnumerable<T> list, string searchTerms, string searchColumns,
            SearchOperatorEnum @operatorType)
        {
            var numericColumnList = typeof(T).GetProperties().Select(p => p.Name).ToList();

            if (!numericColumnList.Contains(searchColumns, StringComparer.OrdinalIgnoreCase))
            {
                throw new ErrorException(StatusCodeEnum.BadRequest);
            }

            if (searchTerms == null || !searchTerms.Any() || searchColumns == null || !searchColumns.Any())
                return list;

            return list.Where(item =>
            {
                foreach (var column in searchColumns)
                {
                    var property = typeof(T).GetProperties().FirstOrDefault(x =>
                        string.Equals(x.Name, searchColumns, StringComparison.OrdinalIgnoreCase));
                    if (property == null) continue;

                    var propertyValue = property.GetValue(item);
                    if (propertyValue != null)
                    {
                        var propertyType = property.PropertyType;
                        // Xử lý kiểu DateTime
                        if (propertyType == typeof(DateTime))
                        {
                            DateTime termValue = DateTime.Parse(searchTerms);
                            if (ApplyOperator((DateTime)propertyValue, termValue, operatorType))
                            {
                                return true;
                            }
                        }

                        // Xử lý kiểu int? (nullable int)
                        if (propertyType == typeof(int?) && int.TryParse(searchTerms, out var parsedValue))
                        {
                            int? termValue = parsedValue; // Chuyển đổi từ int sang int?
                            if (ApplyOperator((int?)propertyValue, termValue, operatorType))
                            {
                                return true;
                            }
                        }

                        // Xử lý kiểu int
                        if (propertyType == typeof(int) && int.TryParse(searchTerms, out var intTermValue))
                        {
                            if (ApplyOperator((int)propertyValue, intTermValue, operatorType))
                            {
                                return true;
                            }
                        }

                        // Thêm xử lý cho các kiểu dữ liệu khác nếu cần

                    }
                }

                return false;
            });
        }

        private static bool ApplyOperator<T>(T value, T termValue, SearchOperatorEnum @operatorType)
            where T : struct, IComparable
        {
            switch (operatorType)
            {
                case SearchOperatorEnum.GreaterThanOrEquals:
                    return value.CompareTo(termValue) >= 0;
                case SearchOperatorEnum.LessThanOrEquals:
                    return value.CompareTo(termValue) <= 0;
                case SearchOperatorEnum.GreaterThan:
                    return value.CompareTo(termValue) > 0;
                case SearchOperatorEnum.LessThan:
                    return value.CompareTo(termValue) < 0;
                case SearchOperatorEnum.Equals:
                    return value.Equals(termValue);
                // Thêm các toán tử khác nếu cần
                default:
                    return false;
            }
        }

        private static bool ApplyOperator(int? value, int? termValue, SearchOperatorEnum @operatorType)
        {
            switch (operatorType)
            {
                case SearchOperatorEnum.GreaterThanOrEquals:
                    return value >= termValue;
                case SearchOperatorEnum.LessThanOrEquals:
                    return value <= termValue;
                case SearchOperatorEnum.GreaterThan:
                    return value > termValue;
                case SearchOperatorEnum.LessThan:
                    return value < termValue;
                case SearchOperatorEnum.Equals:
                    return value == termValue;
                // Add more operators as needed
                default:
                    return false;
            }
        }

        public IEnumerable<T> DefaultSort<T>(IEnumerable<T> data, string defaultSortColumn)
        {
            if (string.IsNullOrEmpty(defaultSortColumn))
            {
                return data;
            }

            var propertyInfo = typeof(T).GetProperty(defaultSortColumn);
            if (propertyInfo == null)
            {
                return data;
            }

            return data.OrderByDescending(x => propertyInfo.GetValue(x, null));
            //OrderBy(x => propertyInfo.GetValue(x, null));

        }



        public static IEnumerable<T> ApplySort<T>(IEnumerable<T> data, IDictionary<string, string> columns)
        {
            if (columns is null || !columns.Any())
            {
                return data;
            }

            columns ??= new Dictionary<string, string>();

            var stringColumnList = new List<string>();

            foreach (var item in typeof(T).GetProperties())
            {
                stringColumnList.Add(item.Name);
            }

            foreach (var column in columns)
            {
                if (!stringColumnList.Contains(column.Key, StringComparer.OrdinalIgnoreCase))
                {
                    throw new Exception("Invalid columns name");
                }
            }

            var propertyDict = new Dictionary<PropertyInfo, string>();
            IOrderedEnumerable<T> dataFormatted = null;

            foreach (var column in columns)
            {
                if (!string.Equals(column.Value, SortDirectionEnum.Desc.ToString(),
                        StringComparison.OrdinalIgnoreCase) && !string.Equals(column.Value,
                        SortDirectionEnum.Asc.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("Invalid sort direction");
                }

                propertyDict.Add(
                    typeof(T).GetProperties().FirstOrDefault(x =>
                        string.Equals(x.Name, column.Key, StringComparison.OrdinalIgnoreCase)), column.Value);
            }

            if (typeof(IEnumerable<string>).IsAssignableFrom(propertyDict.First().Key.PropertyType))
            {
                if (string.Equals(propertyDict.First().Value, SortDirectionEnum.Desc.ToString(),
                        StringComparison.OrdinalIgnoreCase))
                {
                    dataFormatted = data.OrderByDescending(x =>
                        string.Join(", ", (IList<string>)propertyDict.First().Key.GetValue(x)));
                }
                else
                {
                    dataFormatted = data.OrderBy(x =>
                        string.Join(", ", (IList<string>)propertyDict.First().Key.GetValue(x)));
                }
            }
            else
            {
                if (string.Equals(propertyDict.First().Value, SortDirectionEnum.Desc.ToString(),
                        StringComparison.OrdinalIgnoreCase))
                {
                    if (propertyDict.First().Key.Name == "ClientNumber" ||
                        propertyDict.First().Key.Name == "AccountNumber")
                    {
                        dataFormatted =
                            data.OrderByDescending(x => Int64.Parse((string)propertyDict.First().Key.GetValue(x)));
                    }
                    else
                    {
                        dataFormatted = data.OrderByDescending(x => propertyDict.First().Key.GetValue(x));
                    }
                }
                else
                {
                    if (propertyDict.First().Key.Name == "ClientNumber" ||
                        propertyDict.First().Key.Name == "AccountNumber")
                    {
                        dataFormatted = data.OrderBy(x => Int64.Parse((string)propertyDict.First().Key.GetValue(x)));
                    }
                    else
                    {
                        dataFormatted = data.OrderBy(x => propertyDict.First().Key.GetValue(x));
                    }
                }
            }

            for (int i = 1; i < propertyDict.Count; i++)
            {
                var copy = i;
                if (typeof(IEnumerable<string>).IsAssignableFrom(propertyDict.ElementAt(copy).Key.PropertyType))
                {
                    if (string.Equals(propertyDict.ElementAt(copy).Value, SortDirectionEnum.Desc.ToString(),
                            StringComparison.OrdinalIgnoreCase))
                    {
                        dataFormatted = dataFormatted.ThenByDescending(x =>
                            string.Join(", ", (IList<string>)propertyDict.ElementAt(copy).Key.GetValue(x)));
                    }
                    else
                    {
                        dataFormatted = dataFormatted.ThenBy(x =>
                            string.Join(", ", (IList<string>)propertyDict.ElementAt(copy).Key.GetValue(x)));
                    }
                }
                else
                {
                    if (string.Equals(propertyDict.ElementAt(copy).Value, SortDirectionEnum.Desc.ToString(),
                            StringComparison.OrdinalIgnoreCase))
                    {
                        dataFormatted =
                            dataFormatted.ThenByDescending(x => propertyDict.ElementAt(copy).Key.GetValue(x));
                    }
                    else
                    {
                        dataFormatted = dataFormatted.ThenBy(x => propertyDict.ElementAt(copy).Key.GetValue(x));
                    }
                }
            }

            return dataFormatted.ToList();
        }

        // 1. Hàm lấy thông tin người tạo/sửa
        protected Guid GetCurrentUserId()
        {
            // Lấy từ claim
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Guid.Empty; // Hoặc ném ngoại lệ nếu không tìm thấy UserId
            }
            return userId;
        }

        // 2. Hàm chuyển thời gian về GMT+7
        protected DateTime ToGmt7(DateTime utcDateTime)
        {
            var gmt7 = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, gmt7);
        }

        //public ExportExcelResponseModel ExportByEpPlus<T>(IEnumerable<T> datas, Dictionary<string, string?> chosenColumns, string pageName) where T : class
        //{
        //    var verifiedColumns = TypeHelper.VerifyProperties<T>(chosenColumns);

        //    var columnNames = verifiedColumns.Select(x => x.Key).ToList();
        //    var memoryStream = new MemoryStream();
        //    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        //    using (ExcelPackage package = new ExcelPackage())
        //    {
        //        var workSheet = package.Workbook.Worksheets.Add("Sheet1");

        //        // Set header
        //        for (int i = 0; i < columnNames.Count; i++)
        //        {
        //            workSheet.Cells[1, i + 1].Value = verifiedColumns[columnNames[i]];
        //            workSheet.Cells[1, i + 1].Style.Font.Bold = true;
        //        }

        //        // Insert data
        //        int rowIndex = 2; // Start from the second row
        //        foreach (var data in datas)
        //        {
        //            for (int colIndex = 0; colIndex < columnNames.Count; colIndex++)
        //            {
        //                var columnName = columnNames[colIndex];
        //                var property = typeof(T).GetProperty(columnName);
        //                var value = property?.GetValue(data, null); // Access the value dynamically
        //                if (value != null && property.PropertyType == typeof(DateTime))
        //                {
        //                    DateTime date = (DateTime)value;
        //                    workSheet.Cells[rowIndex, colIndex + 1].Value = date.ToString("d");
        //                }
        //                else
        //                {
        //                    workSheet.Cells[rowIndex, colIndex + 1].Value = value ?? ""; // Handle null values
        //                }
        //            }

        //            rowIndex++;
        //        }

        //        // Adjust column widths
        //        for (int i = 1; i <= columnNames.Count; i++)
        //        {
        //            workSheet.Column(i).AutoFit();
        //        }

        //        // Save to stream
        //        package.SaveAs(memoryStream);
        //    }

        //    memoryStream.Position = 0;
        //    var downloadedDay = DateTime.UtcNow.ToLocalTime().ToString("MM.dd.yyyy");
        //    var fileName = $"{pageName}_{downloadedDay}.xlsx";
        //    var fileUrl = "";
        //    var mimeType = Constants.MimeTypeForExcel;
        //    var fileType = Constants.TypeForExcelFile;
        //    var response = new ExportExcelResponseModel(fileName, memoryStream.ToArray(), fileUrl, mimeType, fileType);
        //    return response;
        //}

        //public async Task<ExportPdfResponseModel> ExportPdf(string header, string htmlContent, string pageName)
        //{
        //    var filePath =
        //        $"{DateTime.UtcNow.Year}/{DateTime.UtcNow.Month}/{DateTime.UtcNow.Day}/{pageName}-{Guid.NewGuid()}.pdf";

        //    var downloadedDay = DateTime.UtcNow.ToLocalTime().ToString("MM.dd.yyyy");
        //    var fileName = $"{pageName}_{downloadedDay}.pdf";

        //    var doc = new PdfDocument();
        //    var converter = new HtmlToPdf();
        //    converter.Options.PdfPageSize = PdfPageSize.A4;
        //    converter.Options.PdfPageOrientation = PdfPageOrientation.Landscape;
        //    converter.Options.AutoFitWidth = HtmlToPdfPageFitMode.AutoFit;
        //    converter.Options.MarginLeft = 5;
        //    converter.Options.MarginRight = 5;

        //    if (!string.IsNullOrEmpty(header))
        //    {
        //        converter.Options.DisplayHeader = true;
        //        converter.Header.DisplayOnEvenPages = true;
        //        converter.Header.DisplayOnOddPages = true;

        //        PdfHtmlSection headerHtml = new PdfHtmlSection(header, "");
        //        headerHtml.AutoFitHeight = HtmlToPdfPageFitMode.AutoFit;
        //        converter.Header.Add(headerHtml);
        //    }

        //    var pdfDoc = converter.ConvertHtmlString(htmlContent);
        //    doc.Append(pdfDoc);

        //    byte[] pdfBytes;
        //    using (var stream = new MemoryStream())
        //    {
        //        doc.Save(stream);
        //        doc.Close();
        //        stream.Position = 0;
        //        pdfBytes = stream.ToArray();

        //        using (var azureFileStream = new MemoryStream(stream.ToArray()))
        //        {
        //            var fileUrl = "";
        //            var mimeType = Constants.MimeTypeForPdf;
        //            var fileType = Constants.TypeForPdfFile;
        //            var response = new ExportPdfResponseModel(fileName, pdfBytes, fileUrl, mimeType, fileType);
        //            return response;
        //        }
        //    }
        //}

        //public async Task<ExportPdfResponseModel> Export<T>(IList<T> data, Dictionary<string, string> chosenColumnNames, string header, string title, string pageName) where T : class
        //{
        //    var htmlContent = ConvertDataToHtmlContent(data, chosenColumnNames, title);

        //    return await ExportPdf(header, htmlContent, pageName);
        //}

        //public string ConvertDataToHtmlContent<T>(IList<T> data, Dictionary<string, string> chosenColumns, string title) where T : class
        //{
        //    var verifiedProperties = TypeHelper.VerifyProperties<T>(chosenColumns);
        //    var response = new StringBuilder();

        //    response.Append("<style>thead { display: table-header-group; } tr { page-break-inside: avoid; } @page { size: landscape; } thead {overflow-wrap: break-word} th {overflow-wrap: break-word} td {overflow-wrap: break-word} th {overflow-wrap: break-word}</style>");
        //    response.Append("<div style = 'text-align: center; margin-bottom: 20px;'>");
        //    response.Append($"<h1> {title} </h1>");
        //    response.Append("</div>");

        //    GetPdfBodyHtmlResponse(data, response, verifiedProperties);

        //    return response.ToString();
        //}

        //public static void GetPdfBodyHtmlResponse<T>(IList<T> data, StringBuilder response, Dictionary<string, string> verifiedColumns)
        //{
        //    var properties = typeof(T).GetProperties();
        //    if (data != null)
        //    {
        //        response.Append("<table style='width: 100%; border-collapse: collapse; border:1px solid black'>");
        //        response.Append("<thead>");
        //        response.Append("<tr>");
        //        response.Append("<th style='border:1px solid black; background-color: gray'> </th>");

        //        foreach (var column in verifiedColumns)
        //        {
        //            response.Append($"<th style='border:1px solid black; background-color: gray'>{column.Value}</th>");
        //        }

        //        response.Append("</tr>");
        //        response.Append("</thead>");

        //        for (int i = 0; i < data.Count; i++)
        //        {
        //            response.Append("<tr>");
        //            response.Append($"<td style='border:1px solid black; background-color: gray'>{i + 1}</td>");

        //            foreach (var column in verifiedColumns)
        //            {
        //                var property = properties.FirstOrDefault(x => string.Equals(x.Name, column.Key, StringComparison.OrdinalIgnoreCase));

        //                if (property == null)
        //                {
        //                    continue;
        //                }

        //                if (typeof(IEnumerable<string>).IsAssignableFrom(property.PropertyType))
        //                {
        //                    IList<string> list = (IList<string>)property.GetValue(data[i]);
        //                    var result = "";
        //                    if (list.Count != 0)
        //                    {
        //                        result = string.Join(", ", list);
        //                    }
        //                    response.Append($"<td style='border:1px solid black'>{result}</td>");
        //                }
        //                else
        //                {
        //                    var textDisplay = property.GetValue(data[i]);
        //                    if (property.PropertyType == typeof(DateTime))
        //                    {
        //                        DateTime date = (DateTime)textDisplay;
        //                        response.Append($"<td style='border:1px solid black'>{date.ToString("d")}</td>");
        //                    }
        //                    else
        //                    {

        //                        response.Append($"<td style='border:1px solid black'>{textDisplay}</td>");
        //                    }

        //                }
        //            }

        //            response.Append("</tr>");
        //        }

        //        response.Append("</table>");
        //    }
        //}
    }
}

