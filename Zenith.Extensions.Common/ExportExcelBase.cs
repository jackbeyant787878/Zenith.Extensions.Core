using System;
using System.Collections.Generic;
using System.Text;

namespace Zenith.Extensions.Common
{
    /// <summary>
    /// 导出对象需要继承 
    /// </summary>
    public class ExportExcelBase
    {
        /// <summary>
        /// 标注当前对象是否是空白行
        /// </summary>
        public bool IsInsertNullRow { get; set; }
    }
}
