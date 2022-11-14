using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XML.XCode.DataAccessLayer;

/// <summary>远程数据库。一般是分为客户端服务器的中大型数据库，该类数据库支持完整的SQL92</summary>
abstract class RemoteDb : DbBase
{
    #region 属性
    /// <summary>系统数据库名</summary>
    public virtual String SystemDatabaseName => "master";

    private String _User;
    /// <summary>用户名UserID</summary>
    public String User
    {
        get
        {
            if (_User != null) return _User;

            var connStr = ConnectionString;

            if (String.IsNullOrEmpty(connStr)) return null;

            var ocsb = Factory.CreateConnectionStringBuilder();
            ocsb.ConnectionString = connStr;

            if (ocsb.ContainsKey("User ID"))
                _User = (String)ocsb["User ID"];
            else if (ocsb.ContainsKey("User"))
                _User = (String)ocsb["User"];
            else if (ocsb.ContainsKey("uid"))
                _User = (String)ocsb["uid"];
            else
                _User = String.Empty;

            return _User;
        }
    }
    #endregion

    #region 分页
    /// <summary>已重写。获取分页</summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="startRowIndex">开始行，0表示第一行</param>
    /// <param name="maximumRows">最大返回行数，0表示所有行</param>
    /// <param name="keyColumn">主键列。用于not in分页</param>
    /// <returns></returns>
    public override String PageSplit(String sql, Int64 startRowIndex, Int64 maximumRows, String keyColumn) => PageSplitByLimit(sql, startRowIndex, maximumRows);

    /// <summary>构造分页SQL</summary>
    /// <param name="builder">查询生成器</param>
    /// <param name="startRowIndex">开始行，0表示第一行</param>
    /// <param name="maximumRows">最大返回行数，0表示所有行</param>
    /// <returns>分页SQL</returns>
    public override SelectBuilder PageSplit(SelectBuilder builder, Int64 startRowIndex, Int64 maximumRows) => PageSplitByLimit(builder, startRowIndex, maximumRows);

    /// <summary>已重写。获取分页</summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="startRowIndex">开始行，0表示第一行</param>
    /// <param name="maximumRows">最大返回行数，0表示所有行</param>
    /// <returns></returns>
    public static String PageSplitByLimit(String sql, Int64 startRowIndex, Int64 maximumRows)
    {
        // 从第一行开始，不需要分页
        if (startRowIndex <= 0)
        {
            if (maximumRows < 1) return sql;

            return $"{sql} limit {maximumRows}";
        }
        if (maximumRows < 1) throw new NotSupportedException("不支持取第几条数据之后的所有数据！");

        return $"{sql} limit {startRowIndex}, {maximumRows}";
    }

    /// <summary>构造分页SQL</summary>
    /// <param name="builder">查询生成器</param>
    /// <param name="startRowIndex">开始行，0表示第一行</param>
    /// <param name="maximumRows">最大返回行数，0表示所有行</param>
    /// <returns>分页SQL</returns>
    public static SelectBuilder PageSplitByLimit(SelectBuilder builder, Int64 startRowIndex, Int64 maximumRows)
    {
        // 从第一行开始，不需要分页
        if (startRowIndex <= 0)
        {
            if (maximumRows > 0) builder.Limit = $"limit {maximumRows}";
            return builder;
        }
        if (maximumRows < 1) throw new NotSupportedException("不支持取第几条数据之后的所有数据！");

        builder.Limit = $"limit {startRowIndex}, {maximumRows}";
        return builder;
    }
    #endregion
}