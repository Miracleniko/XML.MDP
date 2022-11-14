using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XML.XCode.TDengineDriver;

class TDengineMeta
{
    public string name;
    public short size;
    public byte type;
    public string TypeName()
    {
        switch ((TDengineDataType)type)
        {
            case TDengineDataType.TSDB_DATA_TYPE_BOOL:
                return "BOOL";
            case TDengineDataType.TSDB_DATA_TYPE_TINYINT:
                return "TINYINT";
            case TDengineDataType.TSDB_DATA_TYPE_SMALLINT:
                return "SMALLINT";
            case TDengineDataType.TSDB_DATA_TYPE_INT:
                return "INT";
            case TDengineDataType.TSDB_DATA_TYPE_BIGINT:
                return "BIGINT";
            case TDengineDataType.TSDB_DATA_TYPE_UTINYINT:
                return "TINYINT UNSIGNED";
            case TDengineDataType.TSDB_DATA_TYPE_USMALLINT:
                return "SMALLINT UNSIGNED";
            case TDengineDataType.TSDB_DATA_TYPE_UINT:
                return "INT UNSIGNED";
            case TDengineDataType.TSDB_DATA_TYPE_UBIGINT:
                return "BIGINT UNSIGNED";
            case TDengineDataType.TSDB_DATA_TYPE_FLOAT:
                return "FLOAT";
            case TDengineDataType.TSDB_DATA_TYPE_DOUBLE:
                return "DOUBLE";
            case TDengineDataType.TSDB_DATA_TYPE_BINARY:
                return "STRING";
            case TDengineDataType.TSDB_DATA_TYPE_TIMESTAMP:
                return "TIMESTAMP";
            case TDengineDataType.TSDB_DATA_TYPE_NCHAR:
                return "NCHAR";
            default:
                return "undefine";
        }
    }
}