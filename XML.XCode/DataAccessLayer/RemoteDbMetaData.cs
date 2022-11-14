using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XML.Core;

namespace XML.XCode.DataAccessLayer;

/// <summary>远程数据库元数据</summary>
abstract class RemoteDbMetaData : DbMetaData
{
    #region 属性
    #endregion

    #region 架构定义
    public override Object SetSchema(DDLSchema schema, params Object[] values)
    {
        var session = Database.CreateSession();
        var databaseName = Database.DatabaseName;

        // ahuang 2014.06.12  类型强制转string的bug
        if (values != null && values.Length > 0 && values[0] is String str && !str.IsNullOrEmpty()) databaseName = str;

        switch (schema)
        {
            //case DDLSchema.TableExist:
            //    return session.QueryCount(GetSchemaSQL(schema, values)) > 0;

            case DDLSchema.DatabaseExist:
                return DatabaseExist(databaseName);

            case DDLSchema.CreateDatabase:
                values = new Object[] { databaseName, values == null || values.Length < 2 ? null : values[1] };

                var sql = base.GetSchemaSQL(schema, values);
                if (sql.IsNullOrEmpty()) return null;

                if (session is RemoteDbSession ss)
                {
                    ss.WriteSQL(sql);
                    return ss.ProcessWithSystem((s, c) =>
                    {
                        using var cmd = Database.Factory.CreateCommand();
                        cmd.Connection = c;
                        cmd.CommandText = sql;

                        return cmd.ExecuteNonQuery();
                    });
                }

                return 0;

            //case DDLSchema.DropDatabase:
            //    return DropDatabase(databaseName);

            default:
                break;
        }
        return base.SetSchema(schema, values);
    }

    protected virtual Boolean DatabaseExist(String databaseName)
    {
        var session = Database.CreateSession();
        return session.QueryCount(GetSchemaSQL(DDLSchema.DatabaseExist, new Object[] { databaseName })) > 0;
    }

    //protected virtual Boolean DropDatabase(String databaseName)
    //{
    //    var session = Database.CreateSession();
    //    var sql = DropDatabaseSQL(databaseName);
    //    if (sql.IsNullOrEmpty()) return session.Execute(sql) > 0;

    //    return true;
    //}

    //Object ProcessWithSystem(Func<IDbSession, Object> callback) => (Database.CreateSession() as RemoteDbSession).ProcessWithSystem((s, c) => callback(s));
    #endregion
}