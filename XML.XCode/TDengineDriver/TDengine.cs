using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace XML.XCode.TDengineDriver;

class TDengine
{
    public const int TSDB_CODE_SUCCESS = 0;

    [DllImport("taos", EntryPoint = "taos_init", CallingConvention = CallingConvention.Cdecl)]
    static extern public void Init();

    [DllImport("taos", EntryPoint = "taos_cleanup", CallingConvention = CallingConvention.Cdecl)]
    static extern public void Cleanup();

    [DllImport("taos", EntryPoint = "taos_options", CallingConvention = CallingConvention.Cdecl)]
    static extern public void Options(int option, string value);

    [DllImport("taos", EntryPoint = "taos_connect", CallingConvention = CallingConvention.Cdecl)]
    static extern public IntPtr Connect(string ip, string user, string password, string db, short port);

    [DllImport("taos", EntryPoint = "taos_errstr", CallingConvention = CallingConvention.Cdecl)]
    static extern private IntPtr taos_errstr(IntPtr res);
    static public string Error(IntPtr res)
    {
        IntPtr errPtr = taos_errstr(res);
        return Marshal.PtrToStringAnsi(errPtr);
    }

    [DllImport("taos", EntryPoint = "taos_errno", CallingConvention = CallingConvention.Cdecl)]
    static extern public int ErrorNo(IntPtr res);

    [DllImport("taos", EntryPoint = "taos_query", CallingConvention = CallingConvention.Cdecl)]
    static extern public IntPtr Query(IntPtr conn, string sqlstr);

    [DllImport("taos", EntryPoint = "taos_affected_rows", CallingConvention = CallingConvention.Cdecl)]
    static extern public int AffectRows(IntPtr res);

    [DllImport("taos", EntryPoint = "taos_field_count", CallingConvention = CallingConvention.Cdecl)]
    static extern public int FieldCount(IntPtr res);

    [DllImport("taos", EntryPoint = "taos_fetch_fields", CallingConvention = CallingConvention.Cdecl)]
    static extern private IntPtr taos_fetch_fields(IntPtr res);
    static public List<TDengineMeta> FetchFields(IntPtr res)
    {
        const int fieldSize = 68;

        List<TDengineMeta> metas = new List<TDengineMeta>();
        if (res == IntPtr.Zero)
        {
            return metas;
        }

        int fieldCount = FieldCount(res);
        IntPtr fieldsPtr = taos_fetch_fields(res);

        for (int i = 0; i < fieldCount; ++i)
        {
            int offset = i * fieldSize;

            TDengineMeta meta = new TDengineMeta();
            meta.name = Marshal.PtrToStringAnsi(fieldsPtr + offset);
            meta.type = Marshal.ReadByte(fieldsPtr + offset + 65);
            meta.size = Marshal.ReadInt16(fieldsPtr + offset + 66);
            metas.Add(meta);
        }

        return metas;
    }

    [DllImport("taos", EntryPoint = "taos_fetch_row", CallingConvention = CallingConvention.Cdecl)]
    static extern public IntPtr FetchRows(IntPtr res);

    [DllImport("taos", EntryPoint = "taos_free_result", CallingConvention = CallingConvention.Cdecl)]
    static extern public IntPtr FreeResult(IntPtr res);

    [DllImport("taos", EntryPoint = "taos_close", CallingConvention = CallingConvention.Cdecl)]
    static extern public int Close(IntPtr taos);

    [DllImport("taos", EntryPoint = "taos_get_client_info", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr GetClientInfo();

    [DllImport("taos", EntryPoint = "taos_get_server_info", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr GetServerInfo(IntPtr taos);

    [DllImport("taos", EntryPoint = "taos_select_db", CallingConvention = CallingConvention.Cdecl)]
    public static extern int SelectDatabase(IntPtr taos, string db);
}