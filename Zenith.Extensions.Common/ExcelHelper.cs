using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Data;
using static Zenith.Extensions.Common.ExcelAttribute;
namespace Zenith.Extensions.Common

{
    public class ExcelHelper
    {
        public static IWorkbook ExportExcelForCompare<T>(IEnumerable<T> list, List<string> exportColumn = null, DateTime? start = null, DateTime? end = null, DateTime? startCompared = null, DateTime? endCompared = null, IEnumerable<ExcelColumn> extraColumns = null)
        {
            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet1 = workbook.CreateSheet("sheet1");

            ICellStyle headStyle = workbook.CreateCellStyle();
            headStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.DarkTeal.Index;
            headStyle.FillPattern = FillPattern.SolidForeground;
            IFont font = workbook.CreateFont();
            font.Color = NPOI.HSSF.Util.HSSFColor.White.Index;
            headStyle.SetFont(font);

            ICellStyle percentStyle = workbook.CreateCellStyle();
            percentStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat("0.00%");

            ICellStyle dollarStyle = workbook.CreateCellStyle();
            IDataFormat format = workbook.CreateDataFormat();
            dollarStyle.DataFormat = format.GetFormat("$#,##0.00");

            ICellStyle numStyle = workbook.CreateCellStyle();
            numStyle.DataFormat = format.GetFormat("#,##0");

            int i = 0, j = 0;
            if (start != null && end != null && startCompared != null && endCompared != null)
            {
                sheet1.SetColumnWidth(0, 60 * 256);
                IRow titleRow = sheet1.CreateRow(0);
                ICell titlecell = titleRow.CreateCell(0);

                titlecell.CellStyle = headStyle;
                titlecell.SetCellValue("Compare Date: " + start?.ToShortDateString() + "~" + end?.ToShortDateString() + " vs " + Convert.ToDateTime(startCompared).ToShortDateString() + "~" + Convert.ToDateTime(endCompared).ToShortDateString());

                i = 1;
            }

            IRow row = sheet1.CreateRow(i++);
            var properties = typeof(T).GetProperties()
                .Where(x => x.GetCustomAttributes(typeof(ExcelAttribute), true).Any());
            var columns = properties.Select(x =>
            {
                var attribute = x.GetCustomAttributes(typeof(ExcelAttribute), true).FirstOrDefault();
                var columnType = (ExcelAttribute.ColumnType)typeof(ExcelAttribute).GetProperty("ContentType").GetValue(attribute);
                string title = (string)typeof(ExcelAttribute).GetProperty("Title").GetValue(attribute);
                return new ExcelColumn
                {
                    PropertyName = x.Name,
                    Title = string.IsNullOrEmpty(title) ? x.Name : title,
                    ColumnType = columnType
                };
            }).ToList();

            //筛选和排序
            if (exportColumn != null && exportColumn.Count > 0)
            {
                List<ExcelColumn> sortList = new List<ExcelColumn>();
                exportColumn.ForEach(t =>
                {

                    var column = columns.Where(c => c.PropertyName.ToLower() == t.ToLower()).FirstOrDefault();
                    if (column != null)
                    {
                        sortList.Add(column);
                    }
                });
                columns = sortList;
            }
            if (extraColumns != null)
            {
                columns.InsertRange(0, extraColumns);
            }
            //ICell titlecell = row.CreateCell(1);
            //titlecell.CellStyle = headStyle;
            //titlecell.SetCellValue("Compare Date: " + start?.ToShortDateString() + "~" + end?.ToShortDateString() + " vs " + Convert.ToDateTime(startCompared).ToShortDateString() + "~" + Convert.ToDateTime(endCompared).ToShortDateString());

            //j = 2;
            foreach (var column in columns)
            {
                ICell cell = row.CreateCell(j++);
                cell.CellStyle = headStyle;
                cell.SetCellValue(column.Title);
            }

            foreach (T entity in list)
            {
                IRow r = sheet1.CreateRow(i++);
                j = 0;
                foreach (var column in columns)
                {
                    var property = typeof(T).GetProperty(column.PropertyName);
                    switch (column.ColumnType)
                    {
                        case (ExcelAttribute.ColumnType)ColumnType.usdollar:
                            var dollarCell = r.CreateCell(j++);
                            dollarCell.SetCellValue(Convert.ToDouble(property.GetValue(entity)));
                            dollarCell.CellStyle = dollarStyle;
                            break;
                        case (ExcelAttribute.ColumnType)ColumnType.num:
                            var numCell = r.CreateCell(j++);
                            numCell.SetCellValue(Convert.ToDouble(property.GetValue(entity)));
                            numCell.CellStyle = numStyle;
                            break;
                        case (ExcelAttribute.ColumnType)ColumnType.percent:
                            var percentCell = r.CreateCell(j++);
                            percentCell.SetCellValue(Convert.ToDouble(property.GetValue(entity)) / 100);
                            percentCell.CellStyle = percentStyle;
                            break;
                        default:
                            r.CreateCell(j++).SetCellValue(property.GetValue(entity)?.ToString());
                            break;
                    }
                }
            }
            return workbook;
        }
        public static IWorkbook ExportExcel<T>(IEnumerable<T> list, IEnumerable<ExcelColumn> extraColumns = null)
        {
            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet1 = workbook.CreateSheet("sheet1");
            int i = 0, j = 0;
            IRow row = sheet1.CreateRow(i++);
            var properties = typeof(T).GetProperties()
                .Where(x => x.GetCustomAttributes(typeof(ExcelAttribute), true).Any());
            var columns = properties.Select(x =>
            {
                var attribute = x.GetCustomAttributes(typeof(ExcelAttribute), true).FirstOrDefault();
                var columnType = (ExcelAttribute.ColumnType)typeof(ExcelAttribute).GetProperty("ContentType").GetValue(attribute);
                string title = (string)typeof(ExcelAttribute).GetProperty("Title").GetValue(attribute);
                return new ExcelColumn
                {
                    PropertyName = x.Name,
                    Title = string.IsNullOrEmpty(title) ? x.Name : title,
                    ColumnType = columnType
                };
            }).ToList();
            if (extraColumns != null)
            {
                columns.InsertRange(0, extraColumns);
            }
            ICellStyle headStyle = workbook.CreateCellStyle();
            headStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.DarkTeal.Index;
            headStyle.FillPattern = FillPattern.SolidForeground;
            IFont font = workbook.CreateFont();
            font.Color = NPOI.HSSF.Util.HSSFColor.White.Index;
            headStyle.SetFont(font);

            ICellStyle percentStyle = workbook.CreateCellStyle();
            percentStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat("0.00%");

            ICellStyle dollarStyle = workbook.CreateCellStyle();
            IDataFormat format = workbook.CreateDataFormat();
            dollarStyle.DataFormat = format.GetFormat("$#,##0.00");

            ICellStyle numStyle = workbook.CreateCellStyle();
            numStyle.DataFormat = format.GetFormat("#,##0");

            foreach (var column in columns)
            {
                ICell cell = row.CreateCell(j++);
                cell.CellStyle = headStyle;
                cell.SetCellValue(column.Title);
            }

            foreach (T entity in list)
            {
                IRow r = sheet1.CreateRow(i++);
                j = 0;
                foreach (var column in columns)
                {
                    var property = typeof(T).GetProperty(column.PropertyName);
                    switch (column.ColumnType)
                    {
                        case ColumnType.usdollar:
                            var dollarCell = r.CreateCell(j++);
                            dollarCell.SetCellValue(Convert.ToDouble(property.GetValue(entity)));
                            dollarCell.CellStyle = dollarStyle;
                            break;
                        case ColumnType.num:
                            var numCell = r.CreateCell(j++);
                            numCell.SetCellValue(Convert.ToDouble(property.GetValue(entity)));
                            numCell.CellStyle = numStyle;
                            break;
                        case ColumnType.percent:
                            var percentCell = r.CreateCell(j++);
                            percentCell.SetCellValue(Convert.ToDouble(property.GetValue(entity)) / 100);
                            percentCell.CellStyle = percentStyle;
                            break;
                        default:
                            r.CreateCell(j++).SetCellValue(property.GetValue(entity)?.ToString());
                            break;
                    }
                }
            }
            return workbook;
        }
        public static IWorkbook ExportExcel(DataTable dt, Dictionary<string, ColumnType> columnTypes = null)
        {
            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet1 = workbook.CreateSheet("sheet1");

            ICellStyle headStyle = workbook.CreateCellStyle();
            headStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.DarkTeal.Index;
            headStyle.FillPattern = FillPattern.SolidForeground;
            IFont font = workbook.CreateFont();
            font.Color = NPOI.HSSF.Util.HSSFColor.White.Index;
            headStyle.SetFont(font);

            ICellStyle percentStyle = workbook.CreateCellStyle();
            percentStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat("0.00%");

            ICellStyle dollarStyle = workbook.CreateCellStyle();
            IDataFormat format = workbook.CreateDataFormat();
            dollarStyle.DataFormat = format.GetFormat("$#,##0.00");

            ICellStyle caDollarStyle = workbook.CreateCellStyle();
            caDollarStyle.DataFormat = format.GetFormat("C$#,##0.00");

            ICellStyle numStyle = workbook.CreateCellStyle();
            numStyle.DataFormat = format.GetFormat("#,##0");

            int i = 0, j = 0;
            IRow row = sheet1.CreateRow(i++);
            foreach (DataColumn dc in dt.Columns)
            {
                if (dc.ColumnName == "Currency") continue;
                ICell cell = row.CreateCell(j++);
                cell.CellStyle = headStyle;
                cell.SetCellValue(dc.ColumnName);
            }

            string currency = "USD";
            foreach (DataRow dr in dt.Rows)
            {
                IRow r = sheet1.CreateRow(i++);
                currency = dr.Table.Columns.Contains("Currency") && dr["Currency"] != DBNull.Value ? (string)dr["Currency"] : "USD";
                j = 0;
                foreach (DataColumn dc in dt.Columns)
                {
                    if (columnTypes == null)
                    {
                        r.CreateCell(j++).SetCellValue(dr[dc.ColumnName].ToString());
                    }
                    else
                    {
                        switch (columnTypes[dc.ColumnName])
                        {
                            case ColumnType.usdollar:
                                switch (currency)
                                {
                                    case "USD":
                                        var dollarCell = r.CreateCell(j++);
                                        if (dr[dc.ColumnName] != DBNull.Value)
                                        {
                                            dollarCell.SetCellValue(Convert.ToDouble(dr[dc.ColumnName]));
                                            dollarCell.CellStyle = dollarStyle;
                                        }
                                        break;
                                    case "CAD":
                                        var caDollarCell = r.CreateCell(j++);
                                        if (dr[dc.ColumnName] != DBNull.Value)
                                        {
                                            caDollarCell.SetCellValue(Convert.ToDouble(dr[dc.ColumnName]));
                                            caDollarCell.CellStyle = caDollarStyle;
                                        }
                                        break;
                                }

                                break;
                            case ColumnType.num:
                                var numCell = r.CreateCell(j++);
                                if (dr[dc.ColumnName] != DBNull.Value)
                                {
                                    numCell.SetCellValue(Convert.ToDouble(dr[dc.ColumnName]));
                                    numCell.CellStyle = numStyle;
                                }
                                break;
                            case ColumnType.percent:
                                var percentCell = r.CreateCell(j++);
                                if (dr[dc.ColumnName] != DBNull.Value)
                                {
                                    percentCell.SetCellValue(Convert.ToDouble(dr[dc.ColumnName]));
                                    percentCell.CellStyle = percentStyle;
                                }
                                break;
                            default:
                                if (dc.ColumnName != "Currency")
                                    r.CreateCell(j++).SetCellValue(dr[dc.ColumnName].ToString());
                                break;
                        }
                    }
                }
            }
            return workbook;
        }
        public static IWorkbook ExportExcel(IEnumerable<DataTable> dts)
        {
            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet1 = workbook.CreateSheet("sheet1");

            ICellStyle headStyle = workbook.CreateCellStyle();
            headStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.DarkTeal.Index;
            headStyle.FillPattern = FillPattern.SolidForeground;
            IFont font = workbook.CreateFont();
            font.Color = NPOI.HSSF.Util.HSSFColor.White.Index;
            headStyle.SetFont(font);

            ICellStyle boldStyle = workbook.CreateCellStyle();
            IFont boldFont = workbook.CreateFont();
            boldFont.IsBold = true;
            boldStyle.SetFont(boldFont);

            int i = 0, j = 0;
            foreach (var dt in dts)
            {
                IRow row = sheet1.CreateRow(i++);
                foreach (DataColumn dc in dt.Columns)
                {
                    if (dc.ColumnName != "IsBold")
                    {
                        ICell cell = row.CreateCell(j++);
                        cell.CellStyle = headStyle;
                        cell.SetCellValue(dc.ColumnName);
                    }
                }

                foreach (DataRow dr in dt.Rows)
                {
                    IRow r = sheet1.CreateRow(i++);
                    j = 0;
                    foreach (DataColumn dc in dt.Columns)
                    {
                        if (dc.ColumnName != "IsBold")
                        {
                            var cell = r.CreateCell(j++);
                            cell.SetCellValue(dr[dc.ColumnName].ToString());
                            if (dt.Columns.Contains("IsBold") && dr["IsBold"] != DBNull.Value && Convert.ToBoolean(dr["IsBold"]))
                            {
                                cell.CellStyle = boldStyle;
                            }
                        }

                    }
                }
                i++;
                j = 0;
            }
            return workbook;
        }

        /// <summary>
        /// 将多个Table导出到一个Excel文件的多个sheet中,如果指定了TableName,则sheet名称以TableName命名，不设置则取时间yyyyMMddHHmmss命名
        /// </summary>
        /// <param name="dt">需要导出的数据集</param>
        ///   /// <param name="ColumnsNames">指定列名,如果不传则默认使用Table的列名</param>
        /// <param name="workbook">workbook 对象 调用方法前定义 传入</param>
        public static void ExportExcel(DataTable dt, List<string> ColumnsNames, ref IWorkbook workbook)
        {
            ICellStyle headStyle = workbook.CreateCellStyle();
            headStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.DarkTeal.Index;
            headStyle.FillPattern = FillPattern.SolidForeground;
            IFont font = workbook.CreateFont();
            font.Color = NPOI.HSSF.Util.HSSFColor.White.Index;
            headStyle.SetFont(font);

            ICellStyle boldStyle = workbook.CreateCellStyle();
            IFont boldFont = workbook.CreateFont();
            boldFont.IsBold = true;
            boldStyle.SetFont(boldFont);

            ICellStyle percentStyle = workbook.CreateCellStyle();
            percentStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat("0.00%");

            ICellStyle dollarStyle = workbook.CreateCellStyle();
            IDataFormat format = workbook.CreateDataFormat();
            dollarStyle.DataFormat = format.GetFormat("$#,##0.00");

            ICellStyle numStyle = workbook.CreateCellStyle();
            numStyle.DataFormat = format.GetFormat("#,##0");

            int i = 0, j = 0;
            ISheet sheet1 = workbook.CreateSheet(string.IsNullOrEmpty(dt.TableName) ? DateTime.Now.ToString("yyyyMMddHHmmssffff") : dt.TableName);
            IRow row = sheet1.CreateRow(i++);
            if (ColumnsNames != null && ColumnsNames.Count > 0)
            {
                foreach (string name in ColumnsNames)
                {
                    ICell cell = row.CreateCell(j++);
                    cell.CellStyle = headStyle;
                    cell.SetCellValue(name);
                }
            }
            else
            {
                foreach (DataColumn dc in dt.Columns)
                {
                    ICell cell = row.CreateCell(j++);
                    cell.CellStyle = headStyle;
                    cell.SetCellValue(dc.ColumnName);
                }
            }

            foreach (DataRow dr in dt.Rows)
            {
                IRow r = sheet1.CreateRow(i++);
                j = 0;
                foreach (DataColumn dc in dt.Columns)
                {
                    if (dc.ColumnName != "IsBold")
                    {
                        var cell = r.CreateCell(j++);
                        var value = dr[dc.ColumnName].ToString();

                        if (Extension.IsInt(value) || Extension.IsNumeric(value))
                        {
                            cell.SetCellValue(Convert.ToDouble(value));
                            cell.CellStyle = numStyle;
                            continue;
                        }
                        if (value.Contains("$"))
                        {
                            var num = value.Replace("$", "");
                            if (Extension.IsInt(num) || Extension.IsNumeric(num))
                            {
                                cell.SetCellValue(Convert.ToDouble(num));
                                cell.CellStyle = dollarStyle;
                                continue;
                            }
                        }
                        if (value.Contains("%"))
                        {
                            var num = value.Replace("%", "");
                            if (Extension.IsInt(num) || Extension.IsNumeric(num))
                            {
                                cell.SetCellValue(num);
                                cell.SetCellValue(Convert.ToDouble(num) / 100);
                                cell.CellStyle = percentStyle;
                                continue;
                            }
                        }
                        cell.SetCellValue(value);
                    }

                }
            }


        }

        /// <summary>
        /// 将多个List导出到一个Excel文件的多个sheet中,如果指定了TableName,则sheet名称以TableName命名，不设置则取时间yyyyMMddHHmmssffff命名      
        /// </summary>
        /// <typeparam name="T">必须继承ExportExcelBase</typeparam>
        /// <param name="list">需要导出的数据列表</param>
        /// <param name="sheetName">sheet 名称 必须指定</param>
        /// <param name="exportColumn">导出字段列表,代码会读取T对象中标有Excel属性的字段,如果exportColumn指定了列表，则根据exportColumn进行过滤，不然则全部导出</param>
        /// <param name="workbook">workbook 对象 调用方法前定义 传入</param>
        /// <param name="extraColumns">扩展字段 不在 exportColumn过滤范围内</param>
        public static void ExportExcel<T>(IEnumerable<T> list, string sheetName, List<string> exportColumn, ref IWorkbook workbook, IEnumerable<ExcelColumn> extraColumns = null) where T : ExportExcelBase
        {
            ISheet sheet1 = workbook.CreateSheet(string.IsNullOrEmpty(sheetName) ? DateTime.Now.ToString("yyyyMMddHHmmssffff") : sheetName);
            int i = 0, j = 0;
            IRow row = sheet1.CreateRow(i++);
            var properties = typeof(T).GetProperties()
                .Where(x => x.GetCustomAttributes(typeof(ExcelAttribute), true).Any());
            var columns = properties.Select(x =>
            {
                var attribute = x.GetCustomAttributes(typeof(ExcelAttribute), true).FirstOrDefault();
                var columnType = (ExcelAttribute.ColumnType)typeof(ExcelAttribute).GetProperty("ContentType").GetValue(attribute);
                string title = (string)typeof(ExcelAttribute).GetProperty("Title").GetValue(attribute);
                return new ExcelColumn
                {
                    PropertyName = x.Name,
                    Title = string.IsNullOrEmpty(title) ? x.Name : title,
                    ColumnType = columnType
                };
            }).ToList();
            //筛选和排序
            if (exportColumn != null && exportColumn.Count > 0)
            {
                List<ExcelColumn> sortList = new List<ExcelColumn>();
                exportColumn.ForEach(t =>
                {

                    var column = columns.Where(c => c.PropertyName.ToLower() == t.ToLower()).FirstOrDefault();
                    if (column != null)
                    {
                        sortList.Add(column);
                    }
                });
                columns = sortList;
            }

            if (extraColumns != null)
            {
                columns.InsertRange(0, extraColumns);
            }
            ICellStyle headStyle = workbook.CreateCellStyle();
            headStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.DarkTeal.Index;
            headStyle.FillPattern = FillPattern.SolidForeground;
            IFont font = workbook.CreateFont();
            font.Color = NPOI.HSSF.Util.HSSFColor.White.Index;
            headStyle.SetFont(font);

            ICellStyle percentStyle = workbook.CreateCellStyle();
            percentStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat("0.00%");

            ICellStyle dollarStyle = workbook.CreateCellStyle();
            IDataFormat format = workbook.CreateDataFormat();
            dollarStyle.DataFormat = format.GetFormat("$#,##0.00");

            ICellStyle caDollarStyle = workbook.CreateCellStyle();
            caDollarStyle.DataFormat = format.GetFormat("C$#,##0.00");

            ICellStyle numStyle = workbook.CreateCellStyle();
            numStyle.DataFormat = format.GetFormat("#,##0");

            foreach (var column in columns)
            {
                ICell cell = row.CreateCell(j++);
                cell.CellStyle = headStyle;
                cell.SetCellValue(column.Title);
            }
            
            string currency = "USD";
            foreach (T entity in list)
            {
                IRow r = sheet1.CreateRow(i++);
                var currencyProperty = typeof(T).GetProperty("Currency");
                if (currencyProperty != null) currency = (string)currencyProperty.GetValue(entity);
                if (entity.IsInsertNullRow)
                { continue; }
                j = 0;
                foreach (var column in columns)
                {
                    var property = typeof(T).GetProperty(column.PropertyName);
                    switch (column.ColumnType)
                    {
                        case ColumnType.usdollar:
                            switch (currency)
                            {
                                case "USD":
                                    var dollarCell = r.CreateCell(j++);
                                    dollarCell.SetCellValue(Convert.ToDouble(property.GetValue(entity)));
                                    dollarCell.CellStyle = dollarStyle;
                                    break;
                                case "CAD":
                                    var caDollarCell = r.CreateCell(j++);
                                    caDollarCell.SetCellValue(Convert.ToDouble(property.GetValue(entity)));
                                    caDollarCell.CellStyle = caDollarStyle;
                                    break;
                            }
                            break;
                        case ColumnType.num:
                            var numCell = r.CreateCell(j++);
                            numCell.SetCellValue(Convert.ToDouble(property.GetValue(entity)));
                            numCell.CellStyle = numStyle;
                            break;
                        case ColumnType.percent:
                            var percentCell = r.CreateCell(j++);
                            percentCell.SetCellValue(Convert.ToDouble(property.GetValue(entity)) / 100);
                            percentCell.CellStyle = percentStyle;
                            break;
                        default:
                            var entityValue = property.GetValue(entity)?.ToString();
                            if (!string.IsNullOrEmpty(entityValue) && entityValue.Contains("\r\n"))
                            {//多文本在单元格内换行显示(中间用换行符隔开) 
                                ICell cell = r.CreateCell(j++);
                                cell.SetCellValue(entityValue);
                                cell.CellStyle.WrapText = true;
                            }
                            else
                            {
                                r.CreateCell(j++).SetCellValue(entityValue);
                            }

                            break;
                    }
                }
            }
        }
        /// <summary>
        /// 将多个DataTable数据导出至一个Excel文件的多个sheet中,可指定sheet名称
        /// </summary>
        /// <param name="ds">DataTable数据集以及各DataTable对应的列类型和列名称</param>
        /// <param name="sheetNames">sheet名称集</param>
        /// <returns></returns>
        public static IWorkbook ExportExcelWithManySheets(Dictionary<DataTable, Dictionary<string, ColumnType>> ds, List<string> sheetNames = null)
        {

            IWorkbook workbook = new XSSFWorkbook();

            ICellStyle headStyle = workbook.CreateCellStyle();
            headStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.DarkTeal.Index;
            headStyle.FillPattern = FillPattern.SolidForeground;
            IFont font = workbook.CreateFont();
            font.Color = NPOI.HSSF.Util.HSSFColor.White.Index;
            headStyle.SetFont(font);

            ICellStyle percentStyle = workbook.CreateCellStyle();
            percentStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat("0.00%");

            ICellStyle dollarStyle = workbook.CreateCellStyle();
            IDataFormat format = workbook.CreateDataFormat();
            dollarStyle.DataFormat = format.GetFormat("$#,##0.00");

            ICellStyle numStyle = workbook.CreateCellStyle();
            numStyle.DataFormat = format.GetFormat("#,##0");

            int sheetNumber = 0;
            foreach (KeyValuePair<DataTable, Dictionary<string, ColumnType>> kvp in ds)
            {
                var dt = kvp.Key;
                var columnTypes = kvp.Value;
                string sheetName = string.Empty;
                if (sheetNames != null && sheetNames.Any())
                {
                    sheetName = sheetNames[sheetNumber++];
                }
                else
                {
                    sheetName = "sheet" + (++sheetNumber);
                }
                ISheet sheet = workbook.CreateSheet(sheetName);
                int i = 0, j = 0;
                IRow row = sheet.CreateRow(i++);
                foreach (DataColumn dc in dt.Columns)
                {
                    ICell cell = row.CreateCell(j++);
                    cell.CellStyle = headStyle;
                    cell.SetCellValue(dc.ColumnName);
                }

                foreach (DataRow dr in dt.Rows)
                {
                    IRow r = sheet.CreateRow(i++);
                    j = 0;
                    foreach (DataColumn dc in dt.Columns)
                    {
                        if (columnTypes == null)
                        {
                            r.CreateCell(j++).SetCellValue(dr[dc.ColumnName].ToString());
                        }
                        else
                        {
                            switch (columnTypes[dc.ColumnName])
                            {
                                case ColumnType.usdollar:
                                    var dollarCell = r.CreateCell(j++);
                                    if (dr[dc.ColumnName] != DBNull.Value)
                                    {
                                        dollarCell.SetCellValue(Convert.ToDouble(dr[dc.ColumnName]));
                                        dollarCell.CellStyle = dollarStyle;
                                    }
                                    break;
                                case ColumnType.num:
                                    var numCell = r.CreateCell(j++);
                                    if (dr[dc.ColumnName] != DBNull.Value)
                                    {
                                        numCell.SetCellValue(Convert.ToDouble(dr[dc.ColumnName]));
                                        numCell.CellStyle = numStyle;
                                    }
                                    break;
                                case ColumnType.percent:
                                    var percentCell = r.CreateCell(j++);
                                    if (dr[dc.ColumnName] != DBNull.Value)
                                    {
                                        percentCell.SetCellValue(Convert.ToDouble(dr[dc.ColumnName]));
                                        percentCell.CellStyle = percentStyle;
                                    }
                                    break;
                                default:
                                    string columnVal = dr[dc.ColumnName].ToString();
                                    //多行文本以换行符进行拼接,在excel中进行换行处理
                                    if (!string.IsNullOrEmpty(columnVal) && columnVal.Contains("\r\n"))
                                    {
                                        ICell cell = r.CreateCell(j++);
                                        cell.SetCellValue(columnVal);
                                        cell.CellStyle.WrapText = true;
                                    }
                                    else
                                    {
                                        r.CreateCell(j++).SetCellValue(dr[dc.ColumnName].ToString());
                                    }

                                    break;
                            }
                        }
                    }
                }
            }

            return workbook;
        }

        /// <summary>
        /// 从excel文件中读取数据,默认从第一个sheet读取
        /// </summary>
        /// <param name="filePath">文件全路径</param>
        /// <param name="top">读取的行数</param>
        /// <param name="sheetNumber">指定读取的sheet的索引</param>
        /// <returns></returns>
        public static DataTable Read(string filePath, int top = 0,int sheetNumber=0)
        {
            using (FileStream fs = File.OpenRead(filePath))
            {
                IWorkbook workbook = new XSSFWorkbook(fs);
                var sheetNums = workbook.NumberOfSheets;
                if (sheetNums== 0) return null;//excel为空数据,直接返回

                ISheet sheet = workbook.GetSheetAt(sheetNumber);
                DataTable dt = new DataTable();
                foreach (var cell in sheet.GetRow(0).Cells)
                {
                    dt.Columns.Add(new DataColumn(cell.StringCellValue.ToLower()));
                }
                int rowNum = sheet.LastRowNum;
                if (top > 0 && top < rowNum)
                {
                    rowNum = top;
                }
                for (int i = 1; i <= rowNum; i++)
                {
                    DataRow dr = dt.NewRow();
                    int j = 0;
                    foreach (var cell in sheet.GetRow(i).Cells)
                    {
                        dr[dt.Columns[j++]] = cell;
                    }
                    dt.Rows.Add(dr);
                }
                return dt;
            }
        }
    }

    public class ExcelAttribute : Attribute
    {
        public enum ColumnType
        {
            str,
            num,
            percent,
            usdollar,
            date
        }

        public ExcelAttribute() : this(ColumnType.str, "")
        {

        }

        public ExcelAttribute(string title) : this(ColumnType.str, title)
        {

        }

        public ExcelAttribute(ColumnType columnType = ColumnType.str, string title = "")
        {
            ContentType = columnType;
            Title = title;
        }

        public ColumnType ContentType { get; }

        public string Title { get; }
    }

    public class ExcelColumn
    {
        public string PropertyName { get; set; }

        public string Title { get; set; }

        public ExcelAttribute.ColumnType ColumnType { get; set; }
    }
}
