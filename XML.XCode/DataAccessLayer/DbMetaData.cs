using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XML.Core.Data;
using XML.Core;
using System.Text.RegularExpressions;
using System.Xml.Schema;
using XML.Core.Collections;
using XML.Core.Reflection;
using XML.Core.Security;
using XML.XCode.Common;
using XML.XCode.Configuration;
using XML.XCode.Exceptions;

namespace XML.XCode.DataAccessLayer;

/// <summary>数据库元数据</summary>
abstract partial class DbMetaData : DisposeBase, IMetaData
{
    #region 属性
    /// <summary>数据库</summary>
    public virtual IDatabase Database { get; set; }

    private ICollection<String> _MetaDataCollections;
    /// <summary>所有元数据集合</summary>
    public ICollection<String> MetaDataCollections
    {
        get
        {
            if (_MetaDataCollections != null) return _MetaDataCollections;
            lock (this)
            {
                if (_MetaDataCollections != null) return _MetaDataCollections;

                var list = new HashSet<String>(StringComparer.OrdinalIgnoreCase);
                var dt = GetSchema(DbMetaDataCollectionNames.MetaDataCollections, null);
                if (dt?.Rows != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var name = "" + dr[0];
                        if (!name.IsNullOrEmpty() && !list.Contains(name)) list.Add(name);
                    }
                }
                return _MetaDataCollections = list;
            }
        }
    }

    private ICollection<String> _ReservedWords;
    /// <summary>保留关键字</summary>
    public virtual ICollection<String> ReservedWords
    {
        get
        {
            if (_ReservedWords != null) return _ReservedWords;
            lock (this)
            {
                if (_ReservedWords != null) return _ReservedWords;

                var list = new HashSet<String>(StringComparer.OrdinalIgnoreCase);
                if (MetaDataCollections.Contains(DbMetaDataCollectionNames.ReservedWords))
                {
                    var dt = GetSchema(DbMetaDataCollectionNames.ReservedWords, null);
                    if (dt?.Rows != null && dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            var name = "" + dr[0];
                            if (!name.IsNullOrEmpty() && !list.Contains(name)) list.Add(name);
                        }
                    }
                }
                return _ReservedWords = list;
            }
        }
    }
    #endregion

    #region GetSchema方法
    /// <summary>返回数据源的架构信息</summary>
    /// <param name="collectionName">指定要返回的架构的名称。</param>
    /// <param name="restrictionValues">为请求的架构指定一组限制值。</param>
    /// <returns></returns>
    public DataTable GetSchema(String collectionName, String[] restrictionValues)
    {
        // 如果不是MetaDataCollections，并且MetaDataCollections中没有该集合，则返回空
        if (!collectionName.EqualIgnoreCase(DbMetaDataCollectionNames.MetaDataCollections))
        {
            if (!MetaDataCollections.Contains(collectionName)) return null;
        }
        return Database.CreateSession().GetSchema(null, collectionName, restrictionValues);
    }
    #endregion

    #region 辅助函数
    /// <summary>尝试从指定数据行中读取指定名称列的数据</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dr"></param>
    /// <param name="name">名称</param>
    /// <param name="value">数值</param>
    /// <returns></returns>
    protected static Boolean TryGetDataRowValue<T>(DataRow dr, String name, out T value)
    {
        value = default;
        if (dr == null || !dr.Table.Columns.Contains(name) || dr.IsNull(name)) return false;

        var obj = dr[name];

        // 特殊处理布尔类型
        if (Type.GetTypeCode(typeof(T)) == TypeCode.Boolean && obj != null)
        {
            if (obj is Boolean)
            {
                value = (T)obj;
                return true;
            }

            if ("YES".EqualIgnoreCase(obj.ToString()))
            {
                value = (T)(Object)true;
                return true;
            }
            if ("NO".EqualIgnoreCase(obj.ToString()))
            {
                value = (T)(Object)false;
                return true;
            }
        }

        try
        {
            if (obj is T)
                value = (T)obj;
            else
            {
                if (obj != null && obj.GetType().IsInt())
                {
                    var n = Convert.ToUInt64(obj);
                    if (n == UInt32.MaxValue && Type.GetTypeCode(typeof(T)) == TypeCode.Int32)
                    {
                        obj = -1;
                    }
                }
                value = obj.ChangeType<T>();
            }
        }
        catch { return false; }

        return true;
    }

    /// <summary>获取指定数据行指定字段的值，不存在时返回空</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dr"></param>
    /// <param name="names">名称</param>
    /// <returns></returns>
    protected static T GetDataRowValue<T>(DataRow dr, params String[] names)
    {
        foreach (var item in names)
        {
            if (TryGetDataRowValue(dr, item, out T value)) return value;
        }

        return default;
    }

    protected static DbTable Select(DbTable ds, String name, Object value)
    {
        var list = new List<Object[]>();
        var col = ds.GetColumn(name);
        if (col >= 0)
        {
            for (var i = 0; i < ds.Rows.Count; i++)
            {
                var dr = ds.Rows[i];
                if (Equals(dr[col], value)) list.Add(dr);
            }
        }

        var ds2 = new DbTable
        {
            Columns = ds.Columns,
            Types = ds.Types,
            Rows = list
        };

        return ds2;
    }

    ///// <summary>格式化关键字</summary>
    ///// <param name="name">名称</param>
    ///// <returns></returns>
    //protected String FormatName(String name)
    //{
    //    switch (Database.NameFormat)
    //    {
    //        case NameFormats.Upper:
    //            name = name.ToUpper();
    //            break;
    //        case NameFormats.Lower:
    //            name = name.ToLower();
    //            break;
    //        case NameFormats.Default:
    //        default:
    //            break;
    //    }

    //    return Database.FormatName(name);
    //}

    protected String FormatName(IDataTable table, Boolean formatKeyword = true) => Database.FormatName(table, formatKeyword);

    protected String FormatName(IDataColumn column) => Database.FormatName(column);
    #endregion

    #region 日志输出
    /// <summary>输出日志</summary>
    /// <param name="msg"></param>
    public static void WriteLog(String msg) => DAL.WriteLog(msg);

    /// <summary>输出日志</summary>
    /// <param name="format"></param>
    /// <param name="args"></param>
    public static void WriteLog(String format, params Object[] args) => DAL.WriteLog(format, args);
    #endregion
}

internal partial class DbMetaData
{
    #region 属性
    private String ConnName => Database.ConnName;

    #endregion

    #region 反向工程
    /// <summary>设置表模型，检查数据表是否匹配表模型，反向工程</summary>
    /// <param name="mode">设置</param>
    /// <param name="tables"></param>
    public void SetTables(Migration mode, params IDataTable[] tables)
    {
        if (mode == Migration.Off) return;

        OnSetTables(tables, mode);
    }

    protected virtual void OnSetTables(IDataTable[] tables, Migration mode)
    {
        var dbExist = CheckDatabase(mode);

        CheckAllTables(tables, mode, dbExist);
    }

    private Boolean? hasCheckedDatabase;
    private Boolean CheckDatabase(Migration mode)
    {
        if (hasCheckedDatabase != null) return hasCheckedDatabase.Value;

        //数据库检查
        var dbExist = false;
        try
        {
            dbExist = (Boolean)SetSchema(DDLSchema.DatabaseExist, null);
        }
        catch
        {
            // 如果异常，默认认为数据库存在
            dbExist = true;
        }

        if (!dbExist)
        {
            if (mode > Migration.ReadOnly)
            {
                WriteLog("创建数据库：{0}", ConnName);
                SetSchema(DDLSchema.CreateDatabase, null, null);

                dbExist = true;
            }
            else
            {
                var sql = GetSchemaSQL(DDLSchema.CreateDatabase, null, null);
                if (String.IsNullOrEmpty(sql))
                    WriteLog("请为连接{0}创建数据库！", ConnName);
                else
                    WriteLog("请为连接{0}创建数据库，使用以下语句：{1}", ConnName, Environment.NewLine + sql);
            }
        }

        hasCheckedDatabase = dbExist;
        return dbExist;
    }

    private void CheckAllTables(IDataTable[] tables, Migration mode, Boolean dbExit)
    {
        IList<IDataTable> dbtables = null;
        if (dbExit)
        {
            var tableNames = tables.Select(e => FormatName(e, false)).ToArray();
            WriteLog("[{0}]待检查数据表：{1}", Database.ConnName, tableNames.Join());
            dbtables = OnGetTables(tableNames);
        }

        foreach (var item in tables)
        {
            try
            {
                var name = FormatName(item, false);

                // 在MySql中，可能存在同名表（大小写不一致），需要先做确定查找，再做不区分大小写的查找
                var dbtable = dbtables?.FirstOrDefault(e => e.TableName == name);
                if (dbtable == null) dbtable = dbtables?.FirstOrDefault(e => e.TableName.EqualIgnoreCase(name));

                // 判断指定表是否存在于数据库中，以决定是创建表还是修改表
                if (dbtable != null)
                    CheckTable(item, dbtable, mode);
                else
                    CheckTable(item, null, mode);
            }
            catch (Exception ex)
            {
                WriteLog(ex.ToString());
            }
        }
    }

    protected virtual void CheckTable(IDataTable entitytable, IDataTable dbtable, Migration mode)
    {
        var onlySql = mode <= Migration.ReadOnly;
        if (dbtable == null)
        {
            // 没有字段的表不创建
            if (entitytable.Columns.Count <= 0) return;

            WriteLog("创建表：{0}({1})", entitytable.TableName, entitytable.Description);

            var sb = new StringBuilder();
            // 建表，如果不是onlySql，执行时DAL会输出SQL日志
            CreateTable(sb, entitytable, onlySql);

            // 仅获取语句
            if (onlySql) WriteLog("只检查不对数据库进行操作,请手工创建表：" + entitytable.TableName + Environment.NewLine + sb.ToString());
        }
        else
        {
            var noDelete = mode < Migration.Full;
            var sql = CheckColumnsChange(entitytable, dbtable, onlySql, noDelete);
            if (!String.IsNullOrEmpty(sql)) sql += ";";
            sql += CheckTableDescriptionAndIndex(entitytable, dbtable, mode);
            if (!sql.IsNullOrEmpty()) WriteLog("只检查不对数据库进行操作,请手工使用以下语句修改表：" + Environment.NewLine + sql);
        }
    }

    /// <summary>检查字段改变。某些数据库（如SQLite）没有添删改字段的DDL语法，可重载该方法，使用重建表方法ReBuildTable</summary>
    /// <param name="entitytable"></param>
    /// <param name="dbtable"></param>
    /// <param name="onlySql"></param>
    /// <param name="noDelete"></param>
    /// <returns></returns>
    protected virtual String CheckColumnsChange(IDataTable entitytable, IDataTable dbtable, Boolean onlySql, Boolean noDelete)
    {
        //var onlySql = mode <= Migration.ReadOnly;
        //var noDelete = mode < Migration.Full;

        var sb = new StringBuilder();
        var etdic = entitytable.Columns.ToDictionary(e => e.ColumnName.ToLower(), e => e, StringComparer.OrdinalIgnoreCase);
        var dbdic = dbtable.Columns.ToDictionary(e => e.ColumnName.ToLower(), e => e, StringComparer.OrdinalIgnoreCase);

        #region 新增列
        foreach (var item in entitytable.Columns)
        {
            if (!dbdic.ContainsKey(item.ColumnName.ToLower()))
            {
                // 非空字段需要重建表
                if (!item.Nullable)
                {
                    //var sql = ReBuildTable(entitytable, dbtable);
                    //if (noDelete)
                    //{
                    //    WriteLog("数据表新增非空字段[{0}]，需要重建表，请手工执行：\r\n{1}", item.Name, sql);
                    //    return sql;
                    //}

                    //Database.CreateSession().Execute(sql);
                    //return String.Empty;

                    // 非空字段作为可空字段新增，避开重建表
                    item.Nullable = true;
                }

                PerformSchema(sb, onlySql, DDLSchema.AddColumn, item);
                if (!item.Description.IsNullOrEmpty()) PerformSchema(sb, onlySql, DDLSchema.AddColumnDescription, item);
            }
        }
        #endregion

        #region 删除列
        var sbDelete = new StringBuilder();
        for (var i = dbtable.Columns.Count - 1; i >= 0; i--)
        {
            var item = dbtable.Columns[i];
            if (!etdic.ContainsKey(item.ColumnName.ToLower()))
            {
                if (!String.IsNullOrEmpty(item.Description)) PerformSchema(sb, onlySql || noDelete, DDLSchema.DropColumnDescription, item);
                PerformSchema(sbDelete, onlySql || noDelete, DDLSchema.DropColumn, item);
            }
        }
        if (sbDelete.Length > 0)
        {
            if (noDelete)
            {
                // 不许删除列，显示日志
                WriteLog("数据表中发现有多余字段，请手工执行以下语句删除：" + Environment.NewLine + sbDelete);
            }
            else
            {
                if (sb.Length > 0) sb.AppendLine(";");
                sb.Append(sbDelete);
            }
        }
        #endregion

        #region 修改列
        // 开发时的实体数据库
        var entityDb = DbFactory.Create(entitytable.DbType);

        foreach (var item in entitytable.Columns)
        {
            if (!dbdic.TryGetValue(item.ColumnName, out var dbf)) continue;

            if (IsColumnTypeChanged(item, dbf))
            {
                WriteLog("字段{0}.{1}类型需要由数据库的{2}改变为实体的{3}", entitytable.Name, item.Name, dbf.DataType, item.DataType);
                PerformSchema(sb, noDelete, DDLSchema.AlterColumn, item, dbf);
            }
            if (IsColumnChanged(item, dbf, entityDb)) PerformSchema(sb, noDelete, DDLSchema.AlterColumn, item, dbf);

            //if (item.Description + "" != dbf.Description + "")
            if (FormatDescription(item.Description) != FormatDescription(dbf.Description))
            {
                // 先删除旧注释
                //if (dbf.Description != null) PerformSchema(sb, noDelete, DDLSchema.DropColumnDescription, dbf);

                // 加上新注释
                if (!item.Description.IsNullOrEmpty()) PerformSchema(sb, onlySql, DDLSchema.AddColumnDescription, item);
            }
        }
        #endregion

        return sb.ToString();
    }

    /// <summary>检查表说明和索引</summary>
    /// <param name="entitytable"></param>
    /// <param name="dbtable"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    protected virtual String CheckTableDescriptionAndIndex(IDataTable entitytable, IDataTable dbtable, Migration mode)
    {
        var onlySql = mode <= Migration.ReadOnly;
        var noDelete = mode < Migration.Full;

        var sb = new StringBuilder();

        #region 表说明
        //if (entitytable.Description + "" != dbtable.Description + "")
        if (FormatDescription(entitytable.Description) != FormatDescription(dbtable.Description))
        {
            //// 先删除旧注释
            //if (!String.IsNullOrEmpty(dbtable.Description)) PerformSchema(sb, onlySql, DDLSchema.DropTableDescription, dbtable);

            // 加上新注释
            if (!String.IsNullOrEmpty(entitytable.Description)) PerformSchema(sb, onlySql, DDLSchema.AddTableDescription, entitytable);
        }
        #endregion

        #region 删除索引
        var dbdis = dbtable.Indexes;
        if (dbdis != null)
        {
            foreach (var item in dbdis.ToArray())
            {
                // 计算的索引不需要删除
                //if (item.Computed) continue;

                // 主键的索引不能删
                if (item.PrimaryKey) continue;

                var di = ModelHelper.GetIndex(entitytable, item.Columns);
                if (di != null && di.Unique == item.Unique) continue;

                PerformSchema(sb, noDelete, DDLSchema.DropIndex, item);
                dbdis.Remove(item);
            }
        }
        #endregion

        #region 新增索引
        var edis = entitytable.Indexes;
        if (edis != null)
        {
            var ids = new List<String>();
            foreach (var item in edis.ToArray())
            {
                if (item.PrimaryKey) continue;

                var di = ModelHelper.GetIndex(dbtable, item.Columns);
                // 计算出来的索引，也表示没有，需要创建
                if (di != null && di.Unique == item.Unique) continue;
                //// 如果这个索引的唯一字段是主键，则无需建立索引
                //if (item.Columns.Length == 1 && entitytable.GetColumn(item.Columns[0]).PrimaryKey) continue;
                // 如果索引全部就是主键，无需创建索引
                if (entitytable.GetColumns(item.Columns).All(e => e.PrimaryKey)) continue;

                // 索引不能重复，不缺分大小写，但字段相同而顺序不同，算作不同索引
                var key = item.Columns.Join(",").ToLower();
                if (ids.Contains(key))
                    WriteLog("[{0}]索引重复 {1}({2})", entitytable.TableName, item.Name, item.Columns.Join(","));
                else
                {
                    ids.Add(key);

                    PerformSchema(sb, onlySql, DDLSchema.CreateIndex, item);
                }

                if (di == null)
                    edis.Add(item.Clone(dbtable));
                //else
                //    di.Computed = false;
            }
        }
        #endregion

        if (!onlySql) return null;

        return sb.ToString();
    }

    /// <summary>格式化注释，去除所有非单词字符</summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private String FormatDescription(String str)
    {
        if (str.IsNullOrWhiteSpace()) return null;

        return Regex.Replace(
            str.Replace("\r\n", " ").Replace("\n", " ").Replace("\\", "\\\\").Replace("'", "")
            .Replace("\"", "").Replace("。", ""), @"\W", "");
    }

    /// <summary>检查字段是否有改变，除了默认值和备注以外</summary>
    /// <param name="entityColumn"></param>
    /// <param name="dbColumn"></param>
    /// <param name="entityDb"></param>
    /// <returns></returns>
    protected virtual Boolean IsColumnChanged(IDataColumn entityColumn, IDataColumn dbColumn, IDatabase entityDb)
    {
        // 自增、主键、非空等，不再认为是字段修改，减轻反向工程复杂度
        //if (entityColumn.Identity != dbColumn.Identity) return true;
        //if (entityColumn.PrimaryKey != dbColumn.PrimaryKey) return true;
        //if (entityColumn.Nullable != dbColumn.Nullable && !entityColumn.Identity && !entityColumn.PrimaryKey) return true;

        // 是否已改变
        var isChanged = false;

        // 仅针对字符串类型比较长度
        if (!isChanged && entityColumn.DataType == typeof(String) && entityColumn.Length != dbColumn.Length)
        {
            isChanged = true;

            // 如果是大文本类型，长度可能不等
            if ((entityColumn.Length > Database.LongTextLength || entityColumn.Length <= 0)
                && (entityDb != null && dbColumn.Length > entityDb.LongTextLength || dbColumn.Length <= 0)
                || dbColumn.RawType.EqualIgnoreCase("ntext", "text", "sysname"))
                isChanged = false;
        }

        return isChanged;
    }

    protected virtual Boolean IsColumnTypeChanged(IDataColumn entityColumn, IDataColumn dbColumn)
    {
        var type = entityColumn.DataType;
        if (type.IsEnum) type = typeof(Int32);
        if (type == dbColumn.DataType) return false;
        if (Nullable.GetUnderlyingType(type) == dbColumn.DataType) return false;

        //// 整型不做改变
        //if (type.IsInt() && dbColumn.DataType.IsInt()) return false;

        // 类型不匹配，不一定就是有改变，还要查找类型对照表是否有匹配的，只要存在任意一个匹配，就说明是合法的
        foreach (var item in FieldTypeMaps)
        {
            //if (entityColumn.DataType == item.Key && dbColumn.DataType == item.Value) return false;
            // 把不常用的类型映射到常用类型，比如数据库SByte映射到实体类Byte，UInt32映射到Int32，而不需要重新修改数据库
            if (dbColumn.DataType == item.Key && type == item.Value) return false;
        }

        return true;
    }

    protected virtual String ReBuildTable(IDataTable entitytable, IDataTable dbtable)
    {
        // 通过重建表的方式修改字段
        var tableName = dbtable.TableName;
        var tempTableName = "Temp_" + tableName + "_" + Rand.Next(1000, 10000);
        tableName = FormatName(dbtable);
        //tempTableName = FormatName(tempTableName);

        // 每个分号后面故意加上空格，是为了让DbMetaData执行SQL时，不要按照分号加换行来拆分这个SQL语句
        var sb = new StringBuilder();
        //sb.AppendLine("BEGIN TRANSACTION; ");
        sb.Append(RenameTable(tableName, tempTableName));
        sb.AppendLine("; ");
        sb.Append(CreateTableSQL(entitytable));
        sb.AppendLine("; ");

        // 如果指定了新列和旧列，则构建两个集合
        if (entitytable.Columns != null && entitytable.Columns.Count > 0 && dbtable.Columns != null && dbtable.Columns.Count > 0)
        {
            var db = Database;

            var sbName = new StringBuilder();
            var sbValue = new StringBuilder();
            foreach (var item in entitytable.Columns)
            {
                var fname = FormatName(item);
                var type = item.DataType;
                var field = dbtable.GetColumn(item.ColumnName);
                if (field == null)
                {
                    // 如果新增了不允许空的列，则处理一下默认值
                    if (!item.Nullable)
                    {
                        if (type == typeof(String))
                        {
                            if (sbName.Length > 0) sbName.Append(", ");
                            if (sbValue.Length > 0) sbValue.Append(", ");
                            sbName.Append(fname);
                            sbValue.Append("''");
                        }
                        else if (type == typeof(Int16) || type == typeof(Int32) || type == typeof(Int64) ||
                            type == typeof(Single) || type == typeof(Double) || type == typeof(Decimal))
                        {
                            if (sbName.Length > 0) sbName.Append(", ");
                            if (sbValue.Length > 0) sbValue.Append(", ");
                            sbName.Append(fname);
                            sbValue.Append('0');
                        }
                        else if (type == typeof(DateTime))
                        {
                            if (sbName.Length > 0) sbName.Append(", ");
                            if (sbValue.Length > 0) sbValue.Append(", ");
                            sbName.Append(fname);
                            sbValue.Append(db.FormatDateTime(DateTime.MinValue));
                        }
                        else if (type == typeof(Boolean))
                        {
                            if (sbName.Length > 0) sbName.Append(", ");
                            if (sbValue.Length > 0) sbValue.Append(", ");
                            sbName.Append(fname);
                            sbValue.Append(db.FormatValue(item, false));
                        }
                    }
                }
                else
                {
                    if (sbName.Length > 0) sbName.Append(", ");
                    if (sbValue.Length > 0) sbValue.Append(", ");
                    sbName.Append(fname);

                    var flag = false;

                    // 处理一下非空默认值
                    if (field.Nullable && !item.Nullable || !item.Nullable && db.Type == DatabaseType.SQLite)
                    {
                        flag = true;
                        if (type == typeof(String))
                            sbValue.Append($"ifnull({fname}, \'\')");
                        else if (type == typeof(Int16) || type == typeof(Int32) || type == typeof(Int64) ||
                           type == typeof(Single) || type == typeof(Double) || type == typeof(Decimal) || type.IsEnum)
                            sbValue.Append($"ifnull({fname}, 0)");
                        else if (type == typeof(DateTime))
                            sbValue.Append($"ifnull({fname}, {db.FormatDateTime(DateTime.MinValue)})");
                        else if (type == typeof(Boolean))
                            sbValue.Append($"ifnull({fname}, {db.FormatValue(item, false)})");
                        else
                            flag = false;
                    }

                    if (!flag)
                    {
                        //sbValue.Append(fname);

                        // 处理字符串不允许空，ntext不支持+""
                        if (type == typeof(String) && !item.Nullable && item.Length > 0 && item.Length < db.LongTextLength)
                            sbValue.Append(db.StringConcat(fname, "\'\'"));
                        else
                            sbValue.Append(fname);
                    }
                }
            }
            sb.AppendFormat("Insert Into {0}({2}) Select {3} From {1}", tableName, tempTableName, sbName, sbValue);
        }
        else
        {
            sb.AppendFormat("Insert Into {0} Select * From {1}", tableName, tempTableName);
        }
        sb.AppendLine("; ");
        sb.AppendFormat("Drop Table {0}", tempTableName);
        //sb.AppendLine("; ");
        //sb.Append("COMMIT;");

        return sb.ToString();
    }

    protected virtual String RenameTable(String tableName, String tempTableName) => $"Alter Table {tableName} Rename To {tempTableName}";

    /// <summary>
    /// 获取架构语句，该执行的已经执行。
    /// 如果取不到语句，则输出日志信息；
    /// 如果不是纯语句，则执行；
    /// </summary>
    /// <param name="sb"></param>
    /// <param name="onlySql"></param>
    /// <param name="schema"></param>
    /// <param name="values"></param>
    protected Boolean PerformSchema(StringBuilder sb, Boolean onlySql, DDLSchema schema, params Object[] values)
    {
        var sql = GetSchemaSQL(schema, values);
        if (!String.IsNullOrEmpty(sql))
        {
            if (sb.Length > 0) sb.AppendLine(";");
            sb.Append(sql);
        }
        else if (sql == null)
        {
            // 只有null才表示通过非SQL的方式处理，而String.Empty表示已经通过别的SQL处理，这里不用输出日志

            // 没办法形成SQL，输出日志信息
            var s = new StringBuilder();
            if (values != null && values.Length > 0)
            {
                foreach (var item in values)
                {
                    if (s.Length > 0) s.Append(' ');
                    s.Append(item);
                }
            }

            IDataColumn dc = null;
            IDataTable dt = null;
            if (values != null && values.Length > 0)
            {
                dc = values[0] as IDataColumn;
                dt = values[0] as IDataTable;
            }

            switch (schema)
            {
                case DDLSchema.AddTableDescription:
                    WriteLog("{0}({1},{2})", schema, dt.TableName, dt.Description);
                    break;
                case DDLSchema.DropTableDescription:
                    WriteLog("{0}({1})", schema, dt);
                    break;
                case DDLSchema.AddColumn:
                    WriteLog("{0}({1})", schema, dc);
                    break;
                //case DDLSchema.AlterColumn:
                //    break;
                case DDLSchema.DropColumn:
                    WriteLog("{0}({1})", schema, dc.ColumnName);
                    break;
                case DDLSchema.AddColumnDescription:
                    WriteLog("{0}({1},{2})", schema, dc.ColumnName, dc.Description);
                    break;
                case DDLSchema.DropColumnDescription:
                    WriteLog("{0}({1})", schema, dc.ColumnName);
                    break;
                default:
                    WriteLog("修改表：{0} {1}", schema.ToString(), s.ToString());
                    break;
            }
            //WriteLog("修改表：{0} {1}", schema.ToString(), s.ToString());
        }

        if (!onlySql)
        {
            try
            {
                SetSchema(schema, values);
            }
            catch (Exception ex)
            {
                WriteLog("修改表{0}失败！{1}", schema.ToString(), ex.Message);
                return false;
            }
        }

        return true;
    }

    protected virtual void CreateTable(StringBuilder sb, IDataTable table, Boolean onlySql)
    {
        // 创建表失败后，不再处理注释和索引
        if (!PerformSchema(sb, onlySql, DDLSchema.CreateTable, table)) return;

        // 加上表注释
        if (!String.IsNullOrEmpty(table.Description)) PerformSchema(sb, onlySql, DDLSchema.AddTableDescription, table);

        // 加上字段注释
        foreach (var item in table.Columns)
        {
            if (!String.IsNullOrEmpty(item.Description)) PerformSchema(sb, onlySql, DDLSchema.AddColumnDescription, item);
        }

        // 加上索引
        if (table.Indexes != null)
        {
            var ids = new List<String>();
            foreach (var item in table.Indexes)
            {
                if (item.PrimaryKey) continue;
                // 如果索引全部就是主键，无需创建索引
                if (table.GetColumns(item.Columns).All(e => e.PrimaryKey)) continue;

                // 索引不能重复，不缺分大小写，但字段相同而顺序不同，算作不同索引
                var key = item.Columns.Join(",").ToLower();
                if (ids.Contains(key))
                    WriteLog("[{0}]索引重复 {1}({2})", table.TableName, item.Name, item.Columns.Join(","));
                else
                {
                    ids.Add(key);

                    PerformSchema(sb, onlySql, DDLSchema.CreateIndex, item);
                }
            }
        }
    }
    #endregion

    #region 数据定义
    /// <summary>获取数据定义语句</summary>
    /// <param name="schema">数据定义模式</param>
    /// <param name="values">其它信息</param>
    /// <returns></returns>
    public virtual String GetSchemaSQL(DDLSchema schema, params Object[] values)
    {
        switch (schema)
        {
            case DDLSchema.CreateDatabase:
                return CreateDatabaseSQL((String)values[0], (String)values[1]);
            case DDLSchema.DropDatabase:
                return DropDatabaseSQL((String)values[0]);
            case DDLSchema.DatabaseExist:
                return DatabaseExistSQL(values == null || values.Length <= 0 ? null : (String)values[0]);
            case DDLSchema.CreateTable:
                return CreateTableSQL((IDataTable)values[0]);
            case DDLSchema.DropTable:
                return DropTableSQL((IDataTable)values[0]);
            //case DDLSchema.TableExist:
            //    if (values[0] is IDataTable)
            //        return TableExistSQL((IDataTable)values[0]);
            //    else
            //        return TableExistSQL(values[0].ToString());
            case DDLSchema.AddTableDescription:
                return AddTableDescriptionSQL((IDataTable)values[0]);
            case DDLSchema.DropTableDescription:
                return DropTableDescriptionSQL((IDataTable)values[0]);
            case DDLSchema.AddColumn:
                return AddColumnSQL((IDataColumn)values[0]);
            case DDLSchema.AlterColumn:
                return AlterColumnSQL((IDataColumn)values[0], values.Length > 1 ? (IDataColumn)values[1] : null);
            case DDLSchema.DropColumn:
                return DropColumnSQL((IDataColumn)values[0]);
            case DDLSchema.AddColumnDescription:
                return AddColumnDescriptionSQL((IDataColumn)values[0]);
            case DDLSchema.DropColumnDescription:
                return DropColumnDescriptionSQL((IDataColumn)values[0]);
            case DDLSchema.CreateIndex:
                return CreateIndexSQL((IDataIndex)values[0]);
            case DDLSchema.DropIndex:
                return DropIndexSQL((IDataIndex)values[0]);
            case DDLSchema.CompactDatabase:
                return CompactDatabaseSQL();
            default:
                break;
        }

        throw new NotSupportedException("不支持该操作！");
    }

    /// <summary>设置数据定义模式</summary>
    /// <param name="schema">数据定义模式</param>
    /// <param name="values">其它信息</param>
    /// <returns></returns>
    public virtual Object SetSchema(DDLSchema schema, params Object[] values)
    {
        var sql = GetSchemaSQL(schema, values);
        if (String.IsNullOrEmpty(sql)) return null;
        var session = Database.CreateSession();

        if (/*schema == DDLSchema.TableExist ||*/ schema == DDLSchema.DatabaseExist) return session.QueryCount(sql) > 0;

        // 分隔符是分号加换行，如果不想被拆开执行（比如有事务），可以在分号和换行之间加一个空格
        var sqls = sql.Split(new[] { ";" + Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        if (sqls == null || sqls.Length <= 1) return session.Execute(sql);

        session.BeginTransaction(IsolationLevel.Serializable);
        try
        {
            foreach (var item in sqls)
            {
                session.Execute(item);
            }
            session.Commit();
        }
        catch
        {
            session.Rollback();
            throw;
        }

        return 0;
    }

    /// <summary>字段片段</summary>
    /// <param name="field">字段</param>
    /// <param name="onlyDefine">仅仅定义。定义操作才允许设置自增和使用默认值</param>
    /// <returns></returns>
    public virtual String FieldClause(IDataColumn field, Boolean onlyDefine)
    {
        var sb = new StringBuilder();

        // 字段名
        sb.AppendFormat("{0} ", FormatName(field));

        String typeName = null;
        // 如果还是原来的数据库类型，则直接使用
        //if (Database.DbType == field.Table.DbType) typeName = field.RawType;
        // 每种数据库的自增差异太大，理应由各自处理，而不采用原始值
        if (Database.Type == field.Table.DbType && !field.Identity) typeName = field.RawType;

        if (String.IsNullOrEmpty(typeName)) typeName = GetFieldType(field);

        sb.Append(typeName);

        // 约束
        sb.Append(GetFieldConstraints(field, onlyDefine));

        return sb.ToString();
    }

    /// <summary>字段片段</summary>
    /// <param name="table">表</param>
    /// <param name="index">序号</param>
    /// <param name="onlyDefine">仅仅定义。定义操作才允许设置自增和使用默认值</param>
    /// <returns></returns>
    public virtual String FieldClause(IDataTable table, Int32 index, Boolean onlyDefine)
    {
        var sb = new StringBuilder();
        var field = table.Columns[index];
        // 字段名
        sb.AppendFormat("{0} ", FormatName(field));

        String typeName = null;
        // 如果还是原来的数据库类型，则直接使用
        //if (Database.DbType == field.Table.DbType) typeName = field.RawType;
        // 每种数据库的自增差异太大，理应由各自处理，而不采用原始值
        if (Database.Type == field.Table.DbType && !field.Identity) typeName = field.RawType;

        if (String.IsNullOrEmpty(typeName)) typeName = GetFieldType(field);

        sb.Append(typeName);

        // 约束
        sb.Append(GetFieldConstraints(field, onlyDefine));

        return sb.ToString();
    }

    /// <summary>取得字段约束</summary>
    /// <param name="field">字段</param>
    /// <param name="onlyDefine">仅仅定义</param>
    /// <returns></returns>
    protected virtual String GetFieldConstraints(IDataColumn field, Boolean onlyDefine)
    {
        if (field.PrimaryKey && field.Table.PrimaryKeys.Length <= 1) return " Primary Key";

        // 是否为空
        var str = field.Nullable ? " NULL" : " NOT NULL";

        // 默认值
        if (!field.Nullable && !field.Identity)
        {
            str += GetDefault(field, onlyDefine);
        }

        return str;
    }

    /// <summary>默认值</summary>
    /// <param name="field"></param>
    /// <param name="onlyDefine"></param>
    /// <returns></returns>
    protected virtual String GetDefault(IDataColumn field, Boolean onlyDefine)
    {
        if (field.DataType.IsInt() || field.DataType.IsEnum)
            return " DEFAULT 0";
        else if (field.DataType == typeof(Boolean))
            return " DEFAULT 0";
        else if (field.DataType == typeof(Double) || field.DataType == typeof(Single) || field.DataType == typeof(Decimal))
            return " DEFAULT 0";
        else if (field.DataType == typeof(DateTime))
            return " DEFAULT '0001-01-01'";
        else if (field.DataType == typeof(String))
            return " DEFAULT ''";

        return null;
    }
    #endregion

    #region 数据定义语句
    public virtual String CreateDatabaseSQL(String dbname, String file) => $"Create Database {Database.FormatName(dbname)}";

    public virtual String DropDatabaseSQL(String dbname) => $"Drop Database {Database.FormatName(dbname)}";

    public virtual String DatabaseExistSQL(String dbname) => null;

    public virtual String CreateTableSQL(IDataTable table)
    {
        //var fs = new List<IDataColumn>(table.Columns);
        var sb = new StringBuilder();

        sb.AppendFormat("Create Table {0}(", FormatName(table));
        for (var i = 0; i < table.Columns.Count; i++)
        {
            sb.AppendLine();
            sb.Append('\t');
            sb.Append(FieldClause(table, i, true));
            if (i < table.Columns.Count - 1) sb.Append(',');
        }
        sb.AppendLine();
        sb.Append(')');

        return sb.ToString();
    }

    public virtual String DropTableSQL(IDataTable table) => $"Drop Table {FormatName(table)}";

    public virtual String TableExistSQL(IDataTable table) => throw new NotSupportedException("该功能未实现！");

    public virtual String AddTableDescriptionSQL(IDataTable table) => null;

    public virtual String DropTableDescriptionSQL(IDataTable table) => null;

    public virtual String AddColumnSQL(IDataColumn field) => $"Alter Table {FormatName(field.Table)} Add {FieldClause(field, true)}";

    public virtual String AlterColumnSQL(IDataColumn field, IDataColumn oldfield) => $"Alter Table {FormatName(field.Table)} Alter Column {FieldClause(field, false)}";

    public virtual String DropColumnSQL(IDataColumn field) => $"Alter Table {FormatName(field.Table)} Drop Column {FormatName(field)}";

    public virtual String AddColumnDescriptionSQL(IDataColumn field) => null;

    public virtual String DropColumnDescriptionSQL(IDataColumn field) => null;

    public virtual String CreateIndexSQL(IDataIndex index)
    {
        var sb = Pool.StringBuilder.Get();
        if (index.Unique)
            sb.Append("Create Unique Index ");
        else
            sb.Append("Create Index ");

        sb.Append(index.Name);
        var dcs = index.Table.GetColumns(index.Columns);
        sb.AppendFormat(" On {0} ({1})", FormatName(index.Table), dcs.Join(",", FormatName));

        return sb.Put(true);
    }

    public virtual String DropIndexSQL(IDataIndex index) => $"Drop Index {index.Name} On {FormatName(index.Table)}";

    public virtual String CompactDatabaseSQL() => null;
    #endregion

    #region 操作
    public virtual String Backup(String dbname, String bakfile, Boolean compressed) => null;

    public virtual Int32 CompactDatabase() => -1;
    #endregion
}

partial class DbMetaData
{
    #region 常量
    protected static class _
    {
        public static readonly String Tables = "Tables";
        public static readonly String Views = "Views";
        public static readonly String Indexes = "Indexes";
        public static readonly String IndexColumns = "IndexColumns";
        public static readonly String Databases = "Databases";
        public static readonly String Columns = "Columns";
        public static readonly String ID = "ID";
        public static readonly String OrdinalPosition = "ORDINAL_POSITION";
        public static readonly String ColumnPosition = "COLUMN_POSITION";
        public static readonly String TalbeName = "table_name";
        public static readonly String ColumnName = "COLUMN_NAME";
        public static readonly String IndexName = "INDEX_NAME";
        public static readonly String PrimaryKeys = "PrimaryKeys";
    }
    #endregion

    #region 表构架
    /// <summary>取得所有表构架</summary>
    /// <returns></returns>
    public IList<IDataTable> GetTables()
    {
        try
        {
            return OnGetTables(null);
        }
        catch (DbException ex)
        {
            throw new XDbMetaDataException(this, "取得所有表构架出错！", ex);
        }
    }

    /// <summary>
    /// 快速取得所有表名
    /// </summary>
    /// <returns></returns>
    public virtual IList<String> GetTableNames()
    {
        var list = new List<String>();

        var dt = GetSchema(_.Tables, null);
        if (dt?.Rows == null || dt.Rows.Count <= 0) return list;

        foreach (DataRow dr in dt.Rows)
        {
            list.Add(GetDataRowValue<String>(dr, _.TalbeName));
        }

        return list;
    }

    /// <summary>取得所有表构架</summary>
    /// <returns></returns>
    protected virtual List<IDataTable> OnGetTables(String[] names)
    {
        var list = new List<IDataTable>();

        var dt = GetSchema(_.Tables, null);
        if (dt?.Rows == null || dt.Rows.Count <= 0) return list;

        // 默认列出所有表
        var rows = dt?.Rows.ToArray();

        return GetTables(rows, names);
    }

    /// <summary>根据数据行取得数据表</summary>
    /// <param name="rows">数据行</param>
    /// <param name="names">指定表名</param>
    /// <param name="data">扩展</param>
    /// <returns></returns>
    protected List<IDataTable> GetTables(DataRow[] rows, String[] names, IDictionary<String, DataTable> data = null)
    {
        if (rows == null || rows.Length == 0) return new List<IDataTable>();

        // 表名过滤
        if (names != null && names.Length > 0)
        {
            var hs = new HashSet<String>(names, StringComparer.OrdinalIgnoreCase);
            rows = rows.Where(dr => TryGetDataRowValue(dr, _.TalbeName, out String name) && hs.Contains(name)).ToArray();
        }

        var columns = data?["Columns"];
        var indexes = data?["Indexes"];
        var indexColumns = data?["IndexColumns"];

        if (columns == null) columns = GetSchema(_.Columns, null);
        if (indexes == null) indexes = GetSchema(_.Indexes, null);
        if (indexColumns == null) indexColumns = GetSchema(_.IndexColumns, null);

        var list = new List<IDataTable>();
        foreach (var dr in rows)
        {
            #region 基本属性
            var table = DAL.CreateTable();
            table.TableName = GetDataRowValue<String>(dr, _.TalbeName);

            // 描述
            table.Description = GetDataRowValue<String>(dr, "DESCRIPTION");

            // 拥有者
            table.Owner = GetDataRowValue<String>(dr, "OWNER");

            // 是否视图
            table.IsView = "View".EqualIgnoreCase(GetDataRowValue<String>(dr, "TABLE_TYPE"));

            table.DbType = Database.Type;
            #endregion

            #region 字段及修正
            var cs = GetFields(table, columns, data);
            if (cs != null && cs.Count > 0) table.Columns.AddRange(cs);

            var dis = GetIndexes(table, indexes, indexColumns);
            if (dis != null && dis.Count > 0) table.Indexes.AddRange(dis);

            FixTable(table, dr, data);

            // 修正关系数据
            table.Fix();

            list.Add(table);
            #endregion
        }

        return list;
    }

    /// <summary>修正表</summary>
    /// <param name="table"></param>
    /// <param name="dr"></param>
    /// <param name="data"></param>
    protected virtual void FixTable(IDataTable table, DataRow dr, IDictionary<String, DataTable> data) { }
    #endregion

    #region 字段架构
    /// <summary>取得指定表的所有列构架</summary>
    /// <param name="table"></param>
    /// <param name="columns">列</param>
    /// <param name="data"></param>
    /// <returns></returns>
    protected virtual List<IDataColumn> GetFields(IDataTable table, DataTable columns, IDictionary<String, DataTable> data)
    {
        var dt = columns;
        if (dt == null) return null;

        // 找到该表所有字段，注意排序
        DataRow[] drs = null;
        var where = $"{_.TalbeName}='{table.TableName}'";
        if (dt.Columns.Contains(_.OrdinalPosition))
            drs = dt.Select(where, _.OrdinalPosition);
        else if (dt.Columns.Contains(_.ID))
            drs = dt.Select(where, _.ID);
        else
            drs = dt.Select(where);

        return GetFields(table, drs);
    }

    /// <summary>获取指定表的字段</summary>
    /// <param name="table"></param>
    /// <param name="rows"></param>
    /// <returns></returns>
    protected virtual List<IDataColumn> GetFields(IDataTable table, DataRow[] rows)
    {
        var list = new List<IDataColumn>();
        foreach (var dr in rows)
        {
            var field = table.CreateColumn();

            // 名称
            field.ColumnName = GetDataRowValue<String>(dr, _.ColumnName);

            // 标识、主键
            if (TryGetDataRowValue(dr, "AUTOINCREMENT", out Boolean b))
                field.Identity = b;

            if (TryGetDataRowValue(dr, "PRIMARY_KEY", out b))
                field.PrimaryKey = b;

            // 原始数据类型
            field.RawType = GetDataRowValue<String>(dr, "DATA_TYPE", "DATATYPE", "COLUMN_DATA_TYPE");
            // 长度
            field.Length = GetDataRowValue<Int32>(dr, "CHARACTER_MAXIMUM_LENGTH", "LENGTH", "COLUMN_SIZE");

            if (field is XField fi)
            {
                // 精度 与 位数
                fi.Precision = GetDataRowValue<Int32>(dr, "NUMERIC_PRECISION", "DATETIME_PRECISION", "PRECISION");
                fi.Scale = GetDataRowValue<Int32>(dr, "NUMERIC_SCALE", "SCALE");
                if (field.Length == 0) field.Length = fi.Precision;
            }

            // 允许空
            if (TryGetDataRowValue(dr, "IS_NULLABLE", out b))
                field.Nullable = b;
            else if (TryGetDataRowValue(dr, "IS_NULLABLE", out String str))
            {
                if (!String.IsNullOrEmpty(str)) field.Nullable = "YES".EqualIgnoreCase(str);
            }
            else if (TryGetDataRowValue(dr, "NULLABLE", out str))
            {
                if (!String.IsNullOrEmpty(str)) field.Nullable = "Y".EqualIgnoreCase(str);
            }

            // 描述
            field.Description = GetDataRowValue<String>(dr, "DESCRIPTION");

            FixField(field, dr);

            // 检查是否已正确识别类型
            if (field.DataType == null)
                WriteLog("无法识别{0}.{1}的类型{2}！", table.TableName, field.ColumnName, field.RawType);
            // 非字符串字段，长度没有意义
            //else if (field.DataType != typeof(String))
            //    field.Length = 0;

            field.Fix();
            list.Add(field);
        }

        return list;
    }

    /// <summary>修正指定字段</summary>
    /// <param name="field">字段</param>
    /// <param name="dr"></param>
    protected virtual void FixField(IDataColumn field, DataRow dr)
    {
        // 修正数据类型 +++重点+++
        if (field.DataType == null) field.DataType = GetDataType(field);
    }
    #endregion

    #region 索引架构
    /// <summary>获取索引</summary>
    /// <param name="table"></param>
    /// <param name="indexes">索引</param>
    /// <param name="indexColumns">索引列</param>
    /// <returns></returns>
    protected virtual List<IDataIndex> GetIndexes(IDataTable table, DataTable indexes, DataTable indexColumns)
    {
        if (indexes == null) return null;

        var drs = indexes.Select($"{_.TalbeName}='{table.TableName}'");
        if (drs == null || drs.Length <= 0) return null;

        var list = new List<IDataIndex>();
        foreach (var dr in drs)
        {
            if (!TryGetDataRowValue(dr, _.IndexName, out String name)) continue;

            var di = table.CreateIndex();
            di.Name = name;

            if (TryGetDataRowValue(dr, _.ColumnName, out name) && !String.IsNullOrEmpty(name))
                di.Columns = name.Split(',');
            else if (indexColumns != null)
            {
                String orderby = null;
                // Oracle数据库用ColumnPosition，其它数据库用OrdinalPosition
                if (indexColumns.Columns.Contains(_.OrdinalPosition))
                    orderby = _.OrdinalPosition;
                else if (indexColumns.Columns.Contains(_.ColumnPosition))
                    orderby = _.ColumnPosition;

                var dics = indexColumns.Select($"{_.TalbeName}='{table.TableName}' And {_.IndexName}='{di.Name}'", orderby);
                if (dics != null && dics.Length > 0)
                {
                    var ns = new List<String>();
                    foreach (var item in dics)
                    {
                        if (TryGetDataRowValue(item, _.ColumnName, out String dcname) && !dcname.IsNullOrEmpty() && !ns.Contains(dcname)) ns.Add(dcname);
                    }
                    if (ns.Count <= 0) DAL.WriteLog("表{0}的索引{1}无法取得字段列表！", table, di.Name);
                    di.Columns = ns.ToArray();
                }
            }

            if (TryGetDataRowValue(dr, "UNIQUE", out Boolean b))
                di.Unique = b;

            if (TryGetDataRowValue(dr, "PRIMARY", out b))
                di.PrimaryKey = b;
            else if (TryGetDataRowValue(dr, "PRIMARY_KEY", out b))
                di.PrimaryKey = b;

            FixIndex(di, dr);

            list.Add(di);
        }
        return list != null && list.Count > 0 ? list : null;
    }

    /// <summary>修正索引</summary>
    /// <param name="index"></param>
    /// <param name="dr"></param>
    protected virtual void FixIndex(IDataIndex index, DataRow dr) { }
    #endregion

    #region 数据类型
    /// <summary>类型映射</summary>
    protected IDictionary<Type, String[]> Types { get; set; }

    protected List<KeyValuePair<Type, Type>> _FieldTypeMaps;
    /// <summary>字段类型映射</summary>
    protected virtual List<KeyValuePair<Type, Type>> FieldTypeMaps
    {
        get
        {
            if (_FieldTypeMaps == null)
            {
                // 把不常用的类型映射到常用类型，比如数据库SByte映射到实体类Byte，UInt32映射到Int32，而不需要重新修改数据库
                var list = new List<KeyValuePair<Type, Type>>
                    {
                        new KeyValuePair<Type, Type>(typeof(SByte), typeof(Byte)),
                        //list.Add(new KeyValuePair<Type, Type>(typeof(SByte), typeof(Int16)));
                        // 因为等价，字节需要能够互相映射
                        new KeyValuePair<Type, Type>(typeof(Byte), typeof(SByte)),

                        new KeyValuePair<Type, Type>(typeof(UInt16), typeof(Int16)),
                        new KeyValuePair<Type, Type>(typeof(Int16), typeof(UInt16)),
                        //list.Add(new KeyValuePair<Type, Type>(typeof(UInt16), typeof(Int32)));
                        //list.Add(new KeyValuePair<Type, Type>(typeof(Int16), typeof(Int32)));

                        new KeyValuePair<Type, Type>(typeof(UInt32), typeof(Int32)),
                        new KeyValuePair<Type, Type>(typeof(Int32), typeof(UInt32)),
                        //// 因为自增的原因，某些字段需要被映射到Int32里面来
                        //list.Add(new KeyValuePair<Type, Type>(typeof(SByte), typeof(Int32)));

                        new KeyValuePair<Type, Type>(typeof(UInt64), typeof(Int64)),
                        new KeyValuePair<Type, Type>(typeof(Int64), typeof(UInt64)),
                        //list.Add(new KeyValuePair<Type, Type>(typeof(UInt64), typeof(Int32)));
                        //list.Add(new KeyValuePair<Type, Type>(typeof(Int64), typeof(Int32)));

                        //// 根据常用行，从不常用到常用排序，然后配对进入映射表
                        //var types = new Type[] { typeof(SByte), typeof(Byte), typeof(UInt16), typeof(Int16), typeof(UInt64), typeof(Int64), typeof(UInt32), typeof(Int32) };

                        //for (int i = 0; i < types.Length; i++)
                        //{
                        //    for (int j = i + 1; j < types.Length; j++)
                        //    {
                        //        list.Add(new KeyValuePair<Type, Type>(types[i], types[j]));
                        //    }
                        //}
                        //// 因为自增的原因，某些字段需要被映射到Int64里面来
                        //list.Add(new KeyValuePair<Type, Type>(typeof(UInt32), typeof(Int64)));
                        //list.Add(new KeyValuePair<Type, Type>(typeof(Int32), typeof(Int64)));
                        new KeyValuePair<Type, Type>(typeof(Guid), typeof(String))
                    };
                _FieldTypeMaps = list;
            }
            return _FieldTypeMaps;
        }
    }

    /// <summary>取字段类型</summary>
    /// <param name="field">字段</param>
    /// <returns></returns>
    protected virtual String GetFieldType(IDataColumn field)
    {
        var type = field.DataType;
        if (type == null) return null;

        // 处理枚举
        if (type.IsEnum) type = typeof(Int32);

        if (!Types.TryGetValue(type, out var ns)) return null;

        var typeName = ns.FirstOrDefault();
        // 大文本选第二个类型
        if (ns.Length > 1 && type == typeof(String) && (field.Length <= 0 || field.Length >= Database.LongTextLength)) typeName = ns[1];
        if (typeName.Contains("{0}"))
        {
            if (typeName.Contains("{1}"))
                typeName = String.Format(typeName, field.Precision, field.Scale);
            else
                typeName = String.Format(typeName, field.Length);
        }

        return typeName;
    }

    /// <summary>获取数据类型</summary>
    /// <param name="field"></param>
    /// <returns></returns>
    protected virtual Type GetDataType(IDataColumn field)
    {
        var rawType = field.RawType;
        if (rawType.Contains("(")) rawType = rawType.Substring(null, "(");
        var rawType2 = rawType + "(";

        foreach (var item in Types)
        {
            String dbtype = null;
            if (rawType.EqualIgnoreCase(item.Value))
            {
                dbtype = item.Value[0];

                // 大文本选第二个类型
                if (item.Value.Length > 1 && item.Key == typeof(String) && (field.Length <= 0 || field.Length >= Database.LongTextLength)) dbtype = item.Value[1];
            }
            else
            {
                dbtype = item.Value.FirstOrDefault(e => e.StartsWithIgnoreCase(rawType2));
            }
            if (!dbtype.IsNullOrEmpty())
            {
                // 修正原始类型
                if (dbtype.Contains("{0}"))
                {
                    // 某些字段有精度需要格式化
                    if (dbtype.Contains("{1}"))
                    {
                        if (field is XField xf)
                            field.RawType = String.Format(dbtype, xf.Precision, xf.Scale);
                    }
                    else
                        field.RawType = String.Format(dbtype, field.Length);
                }

                return item.Key;
            }
        }

        return null;
    }

    /// <summary>获取数据类型</summary>
    /// <param name="rawType"></param>
    /// <returns></returns>
    public virtual Type GetDataType(String rawType)
    {
        if (rawType.Contains("(")) rawType = rawType.Substring(null, "(");
        var rawType2 = rawType + "(";

        foreach (var item in Types)
        {
            String dbtype = null;
            if (rawType.EqualIgnoreCase(item.Value))
            {
                dbtype = item.Value[0];
            }
            else
            {
                dbtype = item.Value.FirstOrDefault(e => e.StartsWithIgnoreCase(rawType2));
            }
            if (!dbtype.IsNullOrEmpty()) return item.Key;
        }

        return null;
    }
    #endregion
}