using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XML.XCode.TDengineDriver;

enum TDengineDataType
{
    TSDB_DATA_TYPE_NULL = 0,     // 1 bytes
    TSDB_DATA_TYPE_BOOL = 1,     // 1 bytes
    TSDB_DATA_TYPE_TINYINT = 2,  // 1 bytes
    TSDB_DATA_TYPE_SMALLINT = 3, // 2 bytes
    TSDB_DATA_TYPE_INT = 4,      // 4 bytes
    TSDB_DATA_TYPE_BIGINT = 5,   // 8 bytes
    TSDB_DATA_TYPE_FLOAT = 6,    // 4 bytes
    TSDB_DATA_TYPE_DOUBLE = 7,   // 8 bytes
    TSDB_DATA_TYPE_BINARY = 8,   // string
    TSDB_DATA_TYPE_TIMESTAMP = 9,// 8 bytes
    TSDB_DATA_TYPE_NCHAR = 10,   // unicode string
    TSDB_DATA_TYPE_UTINYINT = 11,// 1 byte
    TSDB_DATA_TYPE_USMALLINT = 12,// 2 bytes
    TSDB_DATA_TYPE_UINT = 13,    // 4 bytes
    TSDB_DATA_TYPE_UBIGINT = 14   // 8 bytes
}